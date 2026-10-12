namespace FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster;

// ═══════════════════════════════════════════════════════════════
// BUSINESS PARTNER TRADING STATUS
//
// The BP master already carries everything needed to decide whether a
// partner may be traded with — ActiveStatus with its date window,
// CreditLimit, CommitmentLimit and the running Balance — but nothing
// consulted them. Documents posted against inactive partners and past
// credit limits without comment.
//
// This service answers that question in one place. It is deliberately
// split in two:
//
//   Evaluate(...)  reports, and never throws — screens use it to warn
//                  before the user commits to a document.
//   Assert(...)    throws on a hard stop, matching how posting
//                  services already signal a refusal.
//
// Nothing calls Assert yet: switching it on would start refusing
// documents that post today, which is a policy decision rather than a
// bug fix. See the note on CreditCheckResult.Blocking.
// ═══════════════════════════════════════════════════════════════

/// <summary>Why a partner cannot, or should not, be traded with right now.</summary>
public class CreditCheckIssue
{
    public string Code    { get; set; } = null!;   // Inactive / NotYetActive / Expired / CreditLimit / CommitmentLimit
    public string Message { get; set; } = null!;
    /// <summary>True for a status problem, false for a limit that is merely exceeded.</summary>
    public bool   IsHardStop { get; set; }
}

public class CreditCheckResult
{
    public string  Code        { get; set; } = null!;
    public string  CardName    { get; set; } = null!;
    public bool    Ok          { get; set; }
    public decimal Balance     { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CommitmentLimit { get; set; }
    /// <summary>Balance plus the document being considered.</summary>
    public decimal ProjectedBalance { get; set; }
    /// <summary>Headroom left against the credit limit; negative means over.</summary>
    public decimal AvailableCredit  { get; set; }

    public List<CreditCheckIssue> Issues { get; set; } = new();

    /// <summary>Issues that would stop a document if checking were enforced.</summary>
    public IEnumerable<CreditCheckIssue> Blocking => Issues.Where(i => i.IsHardStop);
}

public interface IBusinessPartnerCreditService
{
    /// <summary>Reports on a partner's trading status. Never throws.</summary>
    CreditCheckResult Evaluate(BusinessPartnersMaster partner, decimal documentTotal = 0m, DateTime? on = null);

    /// <summary>Same checks, but throws InvalidOperationException on a hard stop.</summary>
    void Assert(BusinessPartnersMaster partner, decimal documentTotal = 0m, DateTime? on = null);
}

public class BusinessPartnerCreditService : IBusinessPartnerCreditService
{
    public CreditCheckResult Evaluate(
        BusinessPartnersMaster partner, decimal documentTotal = 0m, DateTime? on = null)
    {
        var asOf = (on ?? DateTime.UtcNow).Date;

        var result = new CreditCheckResult
        {
            Code             = partner.Code,
            CardName         = partner.CardName,
            Balance          = partner.Balance,
            CreditLimit      = partner.CreditLimit,
            CommitmentLimit  = partner.CommitmentLimit,
            ProjectedBalance = partner.Balance + documentTotal,
        };

        result.AvailableCredit = partner.CreditLimit <= 0
            ? decimal.MaxValue                       // no limit set means no ceiling
            : partner.CreditLimit - result.ProjectedBalance;

        // ── Active status ────────────────────────────────────────
        switch ((BPActiveStatus)partner.ActiveStatus)
        {
            case BPActiveStatus.Inactive:
                result.Issues.Add(new CreditCheckIssue
                {
                    Code = "Inactive", IsHardStop = true,
                    Message = $"{partner.CardName} is marked inactive." +
                              (string.IsNullOrWhiteSpace(partner.ActiveRemarks)
                                  ? "" : $" Reason: {partner.ActiveRemarks}"),
                });
                break;

            case BPActiveStatus.Advanced:
                // Advanced means the partner is active only inside a date window.
                if (partner.ActiveFrom.HasValue && asOf < partner.ActiveFrom.Value.Date)
                    result.Issues.Add(new CreditCheckIssue
                    {
                        Code = "NotYetActive", IsHardStop = true,
                        Message = $"{partner.CardName} is not active until " +
                                  $"{partner.ActiveFrom.Value:yyyy-MM-dd}.",
                    });
                if (partner.ActiveTo.HasValue && asOf > partner.ActiveTo.Value.Date)
                    result.Issues.Add(new CreditCheckIssue
                    {
                        Code = "Expired", IsHardStop = true,
                        Message = $"{partner.CardName} stopped being active on " +
                                  $"{partner.ActiveTo.Value:yyyy-MM-dd}.",
                    });
                break;
        }

        // ── Limits ───────────────────────────────────────────────
        // A limit of zero or less is "not set", matching the column default.
        if (partner.CreditLimit > 0 && result.ProjectedBalance > partner.CreditLimit)
            result.Issues.Add(new CreditCheckIssue
            {
                Code = "CreditLimit", IsHardStop = false,
                Message = $"{partner.CardName} would be {result.ProjectedBalance - partner.CreditLimit:N2} " +
                          $"over a credit limit of {partner.CreditLimit:N2}.",
            });

        if (partner.CommitmentLimit > 0 && result.ProjectedBalance > partner.CommitmentLimit)
            result.Issues.Add(new CreditCheckIssue
            {
                Code = "CommitmentLimit", IsHardStop = false,
                Message = $"{partner.CardName} would be {result.ProjectedBalance - partner.CommitmentLimit:N2} " +
                          $"over a commitment limit of {partner.CommitmentLimit:N2}.",
            });

        result.Ok = result.Issues.Count == 0;
        return result;
    }

    public void Assert(BusinessPartnersMaster partner, decimal documentTotal = 0m, DateTime? on = null)
    {
        var result = Evaluate(partner, documentTotal, on);
        var blocking = result.Blocking.ToList();
        if (blocking.Count == 0) return;

        throw new InvalidOperationException(string.Join(" ", blocking.Select(b => b.Message)));
    }
}
