
namespace FarmingApi.Modules.Expense;

public class ExpenseListResponse
{
	// public Guid GuidId { get; set; }
	public int Id { get; set; }
        public DateTime ServiceDate { get; set; }
        public DateTime? ExpectedCalvingDate { get; set; }
        public DateTime? ActualCalvingDate { get; set; }
        public string? Outcome { get; set; }
        public string? Notes { get; set; }

}

public class ExpenseListRequest
{
	// public Guid GuidId { get; set; }
	// public int Id { get; set; }
        public DateTime ServiceDate { get; set; }
        public DateTime? ExpectedCalvingDate { get; set; }
        public DateTime? ActualCalvingDate { get; set; }
        public string? Outcome { get; set; }
        public string? Notes { get; set; }

}