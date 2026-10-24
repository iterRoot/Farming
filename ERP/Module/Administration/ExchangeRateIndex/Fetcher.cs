using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using CurrencyMaster = FarmingApi.Modules.Administration.SetUp.Currencies.Currencies;

namespace FarmingApi.Modules.Financials.ExchangeRate;

public class FetchLine
{
    public string   Currency { get; set; } = null!;
    public string   Outcome  { get; set; } = null!;   // Saved | Kept manual | Skipped
    public decimal? Rate     { get; set; }
    public string?  Note     { get; set; }
}

public class FetchResult
{
    public string        Status  { get; set; } = "Failed";   // Success | Partial | Failed
    public string        Message { get; set; } = "";
    public DateTime      Date    { get; set; }
    public List<FetchLine> Lines { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════
// Pulls today's rates from a free public provider (default
// open.er-api.com) and stores them as ordinary ExchangeRate rows
// with Source = "open.er-api.com", so the rest of the app reads
// them exactly like typed-in rates.
//
// The provider quotes "1 BASE = x CURRENCY"; this app stores
// "1 CURRENCY = Rate BASE", so Rate = 1 / x.
// Guards: a rate typed by hand for the same day is never overwritten;
// a rate that jumps more than 25 % from the previous one is refused
// (bad provider data) and reported; a failed call changes nothing.
// ═══════════════════════════════════════════════════════════════
public class ExchangeRateFetcher
{
    public const string SourceName = "open.er-api.com";
    public const decimal MaxJump   = 0.25m;

    private static readonly Regex Iso = new("^[A-Z]{3}$", RegexOptions.Compiled);

    private readonly IHttpClientFactory _http;
    private readonly IConfiguration     _config;

    public ExchangeRateFetcher(IHttpClientFactory http, IConfiguration config)
    {
        _http   = http;
        _config = config;
    }

    public static List<string> ParseCurrencies(string? csv, string baseCurrency) =>
        (csv ?? "").Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim().ToUpperInvariant())
            .Where(c => c != baseCurrency.ToUpperInvariant())
            .Distinct().ToList();

    public static string? ValidateSettings(ExchangeRateSettings s)
    {
        if (!Iso.IsMatch(s.BaseCurrency)) return "Base currency must be a 3-letter code such as KHR or USD.";
        var raw = (s.Currencies ?? "").Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim().ToUpperInvariant()).ToList();
        var bad = raw.FirstOrDefault(c => !Iso.IsMatch(c));
        if (bad != null) return $"'{bad}' is not a 3-letter currency code.";
        var list = ParseCurrencies(s.Currencies, s.BaseCurrency);
        if (s.AutoFetchEnabled && list.Count == 0) return "Choose at least one currency (other than the base) to keep updated.";
        if (list.Count > 20) return "Keep no more than 20 currencies updated.";
        if (s.RunAtHourUtc < 0 || s.RunAtHourUtc > 23) return "Run hour must be between 0 and 23 (UTC).";
        return null;
    }

    public async Task<FetchResult> FetchAsync(MyDbContext db, CancellationToken ct = default)
    {
        var settings = await db.Set<ExchangeRateSettings>().OrderBy(s => s.Id).FirstOrDefaultAsync(ct);
        if (settings == null)
        {
            settings = new ExchangeRateSettings { CreatedAt = DateTime.UtcNow, InActive = false };
            db.Set<ExchangeRateSettings>().Add(settings);
        }

        var today  = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        var result = new FetchResult { Date = today };
        var wanted = ParseCurrencies(settings.Currencies, settings.BaseCurrency);
        settings.LastAttemptAt = DateTime.UtcNow;

        try
        {
            if (wanted.Count == 0) throw new InvalidOperationException("No currencies are set up to update.");

            var rates = await CallProviderAsync(settings.BaseCurrency.ToUpperInvariant(), ct);

            foreach (var cur in wanted)
            {
                var line = new FetchLine { Currency = cur };
                result.Lines.Add(line);

                if (!rates.TryGetValue(cur, out var perBase) || perBase <= 0)
                {
                    line.Outcome = "Skipped"; line.Note = "the provider has no rate for it";
                    continue;
                }
                var rate = Math.Round(1m / perBase, 6);
                if (rate <= 0) { line.Outcome = "Skipped"; line.Note = "the rate is too small to store"; continue; }
                line.Rate = rate;

                var existing = await db.Set<ExchangeRate>().FirstOrDefaultAsync(x => x.Currency == cur && x.EffectiveDate == today, ct);
                if (existing != null && existing.Source != SourceName)
                {
                    line.Outcome = "Kept manual"; line.Note = $"a rate typed in for today ({existing.Rate:0.######}) was left alone";
                    line.Rate = existing.Rate;
                    continue;
                }

                var previous = await db.Set<ExchangeRate>()
                    .Where(x => x.Currency == cur && x.EffectiveDate < today && x.BaseCurrency == settings.BaseCurrency)
                    .OrderByDescending(x => x.EffectiveDate).FirstOrDefaultAsync(ct);
                if (previous != null && previous.Rate > 0 && Math.Abs(rate / previous.Rate - 1m) > MaxJump)
                {
                    line.Outcome = "Skipped";
                    line.Note = $"{rate:0.######} is more than {MaxJump:P0} away from the previous {previous.Rate:0.######} — check it by hand";
                    continue;
                }

                var name = await db.Set<CurrencyMaster>().Where(c => c.Code == cur).Select(c => c.CurrencyName).FirstOrDefaultAsync(ct)
                           ?? existing?.CurrencyName ?? previous?.CurrencyName ?? cur;
                if (existing == null)
                {
                    db.Set<ExchangeRate>().Add(new ExchangeRate
                    {
                        Currency = cur, CurrencyName = name, BaseCurrency = settings.BaseCurrency.ToUpperInvariant(),
                        Rate = rate, EffectiveDate = today, Source = SourceName, IsActive = true,
                        CreatedAt = DateTime.UtcNow, InActive = false,
                    });
                }
                else
                {
                    existing.Rate = rate; existing.BaseCurrency = settings.BaseCurrency.ToUpperInvariant();
                    existing.IsActive = true; existing.UpdatedAt = DateTime.UtcNow;
                }
                line.Outcome = "Saved";
            }

            var skipped = result.Lines.Count(l => l.Outcome == "Skipped");
            var saved   = result.Lines.Count(l => l.Outcome == "Saved");
            var kept    = result.Lines.Count(l => l.Outcome == "Kept manual");
            result.Status = skipped == 0 ? "Success" : saved + kept > 0 ? "Partial" : "Failed";
            result.Message = $"{saved} saved, {kept} kept as typed" + (skipped > 0 ? $", {skipped} skipped: " + string.Join("; ", result.Lines.Where(l => l.Outcome == "Skipped").Select(l => $"{l.Currency} ({l.Note})")) : "") + ".";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or InvalidOperationException or KeyNotFoundException)
        {
            result.Status  = "Failed";
            result.Message = ex is TaskCanceledException ? "The rate provider did not answer in time."
                : ex is JsonException ? "The rate provider sent a reply that could not be read." : ex.Message;
        }

        settings.LastStatus  = result.Status;
        settings.LastMessage = result.Message.Length > 1000 ? result.Message[..1000] : result.Message;
        if (result.Status == "Success") settings.LastSuccessAt = DateTime.UtcNow;
        settings.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return result;
    }

    // Returns "1 BASE = x CURRENCY" for every currency the provider knows.
    private async Task<Dictionary<string, decimal>> CallProviderAsync(string baseCurrency, CancellationToken ct)
    {
        var root = _config["ExchangeRates:ProviderUrl"] ?? "https://open.er-api.com/v6/latest/";
        var client = _http.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        using var resp = await client.GetAsync(root + baseCurrency, ct);
        if (!resp.IsSuccessStatusCode) throw new HttpRequestException($"The rate provider answered {(int)resp.StatusCode}.");
        var body = await resp.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(body);
        var top = doc.RootElement;
        if (top.ValueKind != JsonValueKind.Object) throw new InvalidOperationException("The rate provider sent an unexpected reply.");
        if (!top.TryGetProperty("result", out var ok) || ok.GetString() != "success")
            throw new InvalidOperationException("The rate provider reported an error" + (top.TryGetProperty("error-type", out var et) ? $" ({et.GetString()})." : "."));
        if (!top.TryGetProperty("rates", out var rates) || rates.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("The rate provider's reply had no rates.");

        var map = new Dictionary<string, decimal>();
        foreach (var p in rates.EnumerateObject())
            if (p.Value.ValueKind == JsonValueKind.Number && p.Value.TryGetDecimal(out var v)) map[p.Name.ToUpperInvariant()] = v;
        return map;
    }
}

// Runs the fetch once a day (after the configured UTC hour) while auto-update is on.
// Checks every 10 minutes; a failed or partial run is retried at most hourly.
public class ExchangeRateDailyJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<ExchangeRateDailyJob> _log;

    public ExchangeRateDailyJob(IServiceScopeFactory scopes, ILogger<ExchangeRateDailyJob> log)
    {
        _scopes = scopes;
        _log    = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stop)
    {
        try { await Task.Delay(TimeSpan.FromSeconds(30), stop); } catch (OperationCanceledException) { return; }
        while (!stop.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                var s  = await db.Set<ExchangeRateSettings>().AsNoTracking().OrderBy(x => x.Id).FirstOrDefaultAsync(stop);
                var now = DateTime.UtcNow;
                // Timestamps come back from the database in local kind; compare in UTC.
                var lastOk  = s?.LastSuccessAt?.ToUniversalTime();
                var lastTry = s?.LastAttemptAt?.ToUniversalTime();
                var due = s is { AutoFetchEnabled: true }
                          && now.Hour >= s.RunAtHourUtc
                          && (lastOk == null || lastOk.Value.Date < now.Date)
                          && (lastTry == null || now - lastTry.Value >= TimeSpan.FromMinutes(60));
                _log.LogDebug("Exchange rate auto-update check: enabled={On}, due={Due}", s?.AutoFetchEnabled, due);
                if (due)
                {
                    var r = await scope.ServiceProvider.GetRequiredService<ExchangeRateFetcher>().FetchAsync(db, stop);
                    _log.LogInformation("Exchange rate auto-update: {Status} — {Message}", r.Status, r.Message);
                }
            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex) { _log.LogWarning(ex, "Exchange rate auto-update could not run."); }

            try { await Task.Delay(TimeSpan.FromMinutes(10), stop); } catch (OperationCanceledException) { return; }
        }
    }
}
