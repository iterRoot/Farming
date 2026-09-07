using System;
using System.Collections.Generic;

namespace FarmingApi.Modules.Banking.IncomingPayment;

// ═══════════════════════════════════════════════════════════════════
// RESPONSE DTOs
// ═══════════════════════════════════════════════════════════════════
public class IncomingPaymentListResponse
{
    public int Id { get; set; }
    public string DocNum { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public DateTime DueDate { get; set; }
    public string CardCode { get; set; } = null!;
    public string CardName { get; set; } = null!;
    public decimal DocTotal { get; set; }
    public decimal AppliedAmount { get; set; }
    public decimal UnappliedAmount { get; set; }
    public string Currency { get; set; } = null!;
    public decimal ExchangeRate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? CheckNumber { get; set; }
    public DateTime? CheckDate { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public string ?Reference { get; set; }
    public string ?Memo { get; set; }
    public string ?Status { get; set; }
    public int? JournalEntryId { get; set; }
    public int VersionNum { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool InActive { get; set; }
    public List<IncomingPaymentInvoiceResponse> Invoices { get; set; } = new();
}

public class IncomingPaymentInvoiceResponse
{
    public int Id { get; set; }
    public int LineNum { get; set; }
    public int InvoiceId { get; set; }
    public string InvoiceDocNum { get; set; } = null!;
    public DateTime InvoiceDocDate { get; set; }
    public DateTime InvoiceDueDate { get; set; }
    public decimal InvoiceTotal { get; set; }
    public decimal InvoiceBalance { get; set; }
    public decimal AppliedAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public string? InvoiceStatus { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// REQUEST DTOs
// ═══════════════════════════════════════════════════════════════════
public class IncomingPaymentListRequest
{
    public DateTime DocDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? CardCode { get; set; }
    public string? CardName { get; set; }
    public decimal DocTotal { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal ExchangeRate { get; set; } = 1;
    public string PaymentMethod { get; set; } = "Cash";
    public string? CheckNumber { get; set; }
    public DateTime? CheckDate { get; set; }
    public string? BankAccount { get; set; }
    public string? Reference { get; set; }
    public string? Memo { get; set; }

    // The UI also sends header aliases (refNo, remarks, chequeNo…) and a
    // line-based grid. Accept both so the SAP-style Create screen works.
    public string? RefNo      { get; set; }
    public string? Remarks    { get; set; }
    public string? BankName   { get; set; }
    public string? ChequeNo   { get; set; }
    public DateTime? ChequeDate { get; set; }
    public string? TransferRef { get; set; }

    public List<IncomingPaymentInvoiceRequest> Invoices { get; set; } = new();
    public List<IncomingPaymentLineRequest>?   Lines    { get; set; }
}

public class IncomingPaymentInvoiceRequest
{
    public int InvoiceId { get; set; }
    public decimal AppliedAmount { get; set; }
}

// One row of the SAP-style payment grid. When DocType/DocEntry point at an AR
// invoice (KARI), the row is treated as an allocation against that invoice.
public class IncomingPaymentLineRequest
{
    public int     LineNum     { get; set; }
    public string? DocType     { get; set; }
    public int?    DocEntry    { get; set; }
    public string? DocRef      { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public decimal PaymentAmt  { get; set; }
    public decimal DiscountAmt { get; set; }
    public decimal WhtAmount   { get; set; }
    public string? Remarks     { get; set; }
}

public class IncomingPaymentUpdateRequest
{
    public DateTime DocDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal DocTotal { get; set; }
    public string Currency { get; set; } = null!;
    public decimal ExchangeRate { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string? CheckNumber { get; set; }
    public DateTime? CheckDate { get; set; }
    public string? BankAccount { get; set; }
    public string? Reference { get; set; }
    public string? Memo { get; set; }
    public List<IncomingPaymentInvoiceRequest> Invoices { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════
// HELPER: Get Open Invoices for Customer
// ═══════════════════════════════════════════════════════════════════
public class OpenInvoiceResponse
{
    public int InvoiceId { get; set; }
    public string DocNum { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal DocTotal { get; set; }
    public decimal PaidToDate { get; set; }
    public decimal Balance { get; set; }
    public int DaysOverdue { get; set; }
    public string? Status { get; set; }
}