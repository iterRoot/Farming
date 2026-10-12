namespace FarmingApi.Modules.Sales.CommissionGroup;

// Commission on one sale amount under a group's rule.
//   Percentage — amount × rate
//   Fixed      — the flat amount per transaction
//   Tiered     — the rate of the bracket the amount falls in
// A sale below the group's minimum earns nothing; the group's cap limits each transaction.
public static class CommissionCalc
{
    public static decimal Compute(CommissionGroup group, decimal amount)
    {
        if (amount <= 0) return 0m;
        if (group.MinSaleAmount.HasValue && amount < group.MinSaleAmount) return 0m;

        decimal commission = 0;
        switch (group.CommissionType)
        {
            case "Percentage":
                commission = Math.Round(amount * group.CommissionRate / 100, 2);
                break;
            case "Fixed":
                commission = group.CommissionRate;
                break;
            case "Tiered":
                foreach (var tier in group.Tiers.OrderBy(t => t.TierOrder))
                {
                    if (amount >= tier.FromAmount && (tier.ToAmount == 0 || amount <= tier.ToAmount))
                    {
                        commission = Math.Round(amount * tier.Rate / 100, 2);
                        break;
                    }
                }
                break;
        }
        if (group.MaxCommission.HasValue) commission = Math.Min(commission, group.MaxCommission.Value);
        return commission;
    }
}
