namespace FarmingApi.Modules.HumanResources.Payroll;

// ═══════════════════════════════════════════════════════════════
// PAYROLL AUTO-RUN WORKER
//
// Runs the monthly payroll on its own, so nobody has to remember.
// Deliberately conservative:
//
//   • It only ever creates DRAFT payslips. Nothing is approved and
//     nothing is paid without a person doing it.
//   • It never touches a payslip already marked Paid.
//   • It writes a row to KPRL for every run and checks that table
//     before starting, so a restart, a redeploy or two instances
//     cannot double-run a period.
//   • If the server was down on the scheduled day it catches up on
//     the next tick instead of silently skipping the month.
//   • A failure is logged (KPRL row with Status=Failed) and retried
//     on the next tick; it never takes the API down with it.
//
// Default schedule: 02:00 UTC on the 1st of each month, covering the
// month that just ended — attendance for a month is only complete
// once the month is over. Override in appsettings.json:
//
//   "Payroll": {
//     "AutoRun": {
//       "Enabled": true,
//       "DayOfMonth": 1,
//       "HourUtc": 2,
//       "Target": "PreviousMonth"
//     }
//   }
// ═══════════════════════════════════════════════════════════════
public class PayrollAutoRunWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IConfiguration       _config;
    private readonly ILogger<PayrollAutoRunWorker> _logger;

    private static readonly TimeSpan Tick        = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan StartupWait = TimeSpan.FromSeconds(30);

    public PayrollAutoRunWorker(IServiceScopeFactory scopes,
                                IConfiguration config,
                                ILogger<PayrollAutoRunWorker> logger)
    {
        _scopes = scopes;
        _config = config;
        _logger = logger;
    }

    // ── Settings ──────────────────────────────────────────────
    public sealed record Settings(bool Enabled, int DayOfMonth, int HourUtc, string Target);

    public static Settings ReadSettings(IConfiguration config)
    {
        var s = config.GetSection("Payroll:AutoRun");
        return new Settings(
            Enabled:    s.GetValue("Enabled", true),
            DayOfMonth: Math.Clamp(s.GetValue("DayOfMonth", 1), 1, 28),
            HourUtc:    Math.Clamp(s.GetValue("HourUtc", 2), 0, 23),
            Target:     s.GetValue("Target", "PreviousMonth") ?? "PreviousMonth");
    }

    /// <summary>The period a run started at <paramref name="now"/> would cover.</summary>
    public static (int Year, int Month) TargetPeriod(DateTime now, Settings s) =>
        string.Equals(s.Target, "CurrentMonth", StringComparison.OrdinalIgnoreCase)
            ? (now.Year, now.Month)
            : (now.AddMonths(-1).Year, now.AddMonths(-1).Month);

    /// <summary>When this month's run is due. Past this instant, the run is owed.</summary>
    public static DateTime ScheduledFor(DateTime now, Settings s) =>
        new DateTime(now.Year, now.Month, s.DayOfMonth, s.HourUtc, 0, 0, DateTimeKind.Utc);

    public static DateTime NextRunUtc(DateTime now, Settings s)
    {
        var thisMonth = ScheduledFor(now, s);
        return now < thisMonth ? thisMonth : thisMonth.AddMonths(1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(StartupWait, stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                TryRunDuePeriod();
            }
            catch (Exception ex)
            {
                // Never let the scheduler kill the host.
                _logger.LogError(ex, "Payroll auto-run tick failed.");
            }

            try { await Task.Delay(Tick, stoppingToken); }
            catch (OperationCanceledException) { return; }
        }
    }

    private void TryRunDuePeriod()
    {
        var settings = ReadSettings(_config);
        if (!settings.Enabled) return;

        var now = DateTime.UtcNow;

        // Not yet the scheduled moment this month → nothing owed.
        if (now < ScheduledFor(now, settings)) return;

        var (year, month) = TargetPeriod(now, settings);

        using var scope = _scopes.CreateScope();
        var logs    = scope.ServiceProvider.GetRequiredService<IPayrollRunLogRepository>();
        var payroll = scope.ServiceProvider.GetRequiredService<IPayrollService>();

        // Already done — this is what makes restarts and redeploys safe.
        var already = logs.GetAll().Any(l =>
            l.PeriodYear == year && l.PeriodMonth == month
            && l.Trigger == "Auto" && l.Status == "Success");
        if (already) return;

        _logger.LogInformation("Payroll auto-run starting for {Year}-{Month:D2}.", year, month);

        var startedAt = DateTime.UtcNow;
        try
        {
            var result = payroll.Generate(new PayrollGenerateRequest
            {
                PeriodYear      = year,
                PeriodMonth     = month,
                RefreshExisting = true,
            }, trigger: "Auto");

            _logger.LogInformation(
                "Payroll auto-run for {Period} finished: {Created} created, {Refreshed} refreshed, " +
                "{Paid} already paid. Net {Net:F2}.",
                result.Period, result.Created, result.Refreshed, result.SkippedPaid, result.TotalNet);
        }
        catch (Exception ex)
        {
            // Log the failure so it is visible in the run history, and let
            // the next tick retry — no Success row means it is still owed.
            _logger.LogError(ex, "Payroll auto-run failed for {Year}-{Month:D2}.", year, month);
            WriteFailure(year, month, startedAt, Root(ex).Message);
        }
    }

    // Written through a FRESH scope: the DbContext that just threw may be
    // holding failed changes, and reusing it would lose the failure row too.
    private void WriteFailure(int year, int month, DateTime startedAt, string message)
    {
        try
        {
            using var scope = _scopes.CreateScope();
            var logs = scope.ServiceProvider.GetRequiredService<IPayrollRunLogRepository>();
            logs.Add(new PayrollRunLog
            {
                PeriodYear  = year,
                PeriodMonth = month,
                Trigger     = "Auto",
                Status      = "Failed",
                RunAt       = startedAt,
                DurationMs  = (int)(DateTime.UtcNow - startedAt).TotalMilliseconds,
                Error       = message.Length > 2000 ? message[..2000] : message,
                CreatedAt   = DateTime.UtcNow,
                InActive    = false,
            });
            logs.Commit();
        }
        catch (Exception logEx)
        {
            _logger.LogError(logEx, "Could not write the payroll auto-run failure to the run log.");
        }
    }

    private static Exception Root(Exception ex)
    {
        while (ex.InnerException != null) ex = ex.InnerException;
        return ex;
    }
}
