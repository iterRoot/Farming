using System;
using System.Collections.Generic;

namespace FarmingApi.Modules.Financials.JournalVoucher;

public class JournalVoucherListResponse
{
    public int Id { get; set; }
    public string VoucherNo { get; set; }
    public DateTime VoucherDate { get; set; }
    public DateTime DueDate { get; set; }
    public string RefNo { get; set; }
    public string Description { get; set; }
    public string Memo { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public string Currency { get; set; }
    public decimal ExchangeRate { get; set; }
    public string Status { get; set; }
    public int? PostedToJournalEntryId { get; set; }
    public DateTime? PostedDate { get; set; }
    public string ApprovalStatus { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string ApprovalNote { get; set; }
    public string ProjectCode { get; set; }
    public string CostCenter { get; set; }
    public int VersionNum { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool InActive { get; set; }
    public List<JournalVoucherLineResponse> Lines { get; set; } = new();
}

public class JournalVoucherLineResponse
{
    public int Id { get; set; }
    public int LineNum { get; set; }
    public string AccountCode { get; set; }
    public string AccountName { get; set; }
    public string CardCode { get; set; }
    public string CardName { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal DebitFC { get; set; }
    public decimal CreditFC { get; set; }
    public string LineMemo { get; set; }
    public string CostCenter { get; set; }
    public string Project { get; set; }
    public string TaxCode { get; set; }
    public decimal TaxAmount { get; set; }
    public string Reference { get; set; }
    public DateTime? DueDate { get; set; }
}

public class JournalVoucherListRequest
{
    public DateTime VoucherDate { get; set; }
    public DateTime DueDate { get; set; }
    public string RefNo { get; set; }
    public string Description { get; set; }
    public string Memo { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal ExchangeRate { get; set; } = 1;
    public string ProjectCode { get; set; }
    public string CostCenter { get; set; }
    public List<JournalVoucherLineRequest> Lines { get; set; } = new();
}

public class JournalVoucherLineRequest
{
    public int LineNum { get; set; }
    public string AccountCode { get; set; }
    public string CardCode { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal DebitFC { get; set; }
    public decimal CreditFC { get; set; }
    public string LineMemo { get; set; }
    public string CostCenter { get; set; }
    public string Project { get; set; }
    public string TaxCode { get; set; }
    public decimal TaxAmount { get; set; }
    public string Reference { get; set; }
    public DateTime? DueDate { get; set; }
}

public class JournalVoucherUpdateRequest
{
    public DateTime VoucherDate { get; set; }
    public DateTime DueDate { get; set; }
    public string RefNo { get; set; }
    public string Description { get; set; }
    public string Memo { get; set; }
    public string Currency { get; set; }
    public decimal ExchangeRate { get; set; }
    public string ProjectCode { get; set; }
    public string CostCenter { get; set; }
    public List<JournalVoucherLineRequest> Lines { get; set; } = new();
}

public class ApprovalRequest
{
    public string Action { get; set; }     // "Approve" or "Reject"
    public string Note { get; set; }
}