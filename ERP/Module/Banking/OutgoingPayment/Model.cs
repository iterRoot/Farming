using System;
using System.Collections.Generic;

namespace FarmingApi.Modules.Banking.OutgoingPayment;

// ═══════════════════════════════════════════════════════════════════
// RESPONSE DTOs
// ═══════════════════════════════════════════════════════════════════
public class OutgoingPaymentListResponse
{
    public int Id { get; set; }
    public string DocNum { get; set; }
    public DateTime DocDate { get; set; }
    public DateTime DueDate { get; set; }
    public string CardCode { get; set; }
    public string CardName { get; set; }
    public decimal DocTotal { get; set; }
    public decimal AppliedAmount { get; set; }
    public decimal UnappliedAmount { get; set; }
    public string Currency { get; set; }
    public decimal ExchangeRate { get; set; }
    public string PaymentMethod { get; set; }
    public string CheckNumber { get; set; }
    public DateTime? CheckDate { get; set; }
    public string BankAccount { get; set; }
    public string BankName { get; set; }
    public string Reference { get; set; }
    public string Memo { get; set; }
    public string Status { get; set; }
    public int? JournalEntryId { get; set; }
    public int VersionNum { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool InActive { get; set; }
    public List<OutgoingPaymentInvoiceResponse> Invoices { get; set; } = new();
}

public class OutgoingPaymentInvoiceResponse
{
    public int Id { get; set; }
    public int LineNum { get; set; }
    public int InvoiceId { get; set; }
    public string InvoiceDocNum { get; set; }
    public DateTime InvoiceDocDate { get; set; }
    public DateTime InvoiceDueDate { get; set; }
    public decimal InvoiceTotal { get; set; }
    public decimal InvoiceBalance { get; set; }
    public decimal AppliedAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public string InvoiceStatus { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal WithholdingTax { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// REQUEST DTOs
// ═══════════════════════════════════════════════════════════════════
public class OutgoingPaymentListRequest
{
    public DateTime DocDate { get; set; }
    public DateTime DueDate { get; set; }
    public string CardCode { get; set; }
    public decimal DocTotal { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal ExchangeRate { get; set; } = 1;
    public string PaymentMethod { get; set; } = "Cash";
    public string CheckNumber { get; set; }
    public DateTime? CheckDate { get; set; }
    public string BankAccount { get; set; }
    public string Reference { get; set; }
    public string Memo { get; set; }
    public List<OutgoingPaymentInvoiceRequest> Invoices { get; set; } = new();
}

public class OutgoingPaymentInvoiceRequest
{
    public int InvoiceId { get; set; }
    public decimal AppliedAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal WithholdingTax { get; set; }
}

public class OutgoingPaymentUpdateRequest
{
    public DateTime DocDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal DocTotal { get; set; }
    public string Currency { get; set; }
    public decimal ExchangeRate { get; set; }
    public string PaymentMethod { get; set; }
    public string CheckNumber { get; set; }
    public DateTime? CheckDate { get; set; }
    public string BankAccount { get; set; }
    public string Reference { get; set; }
    public string Memo { get; set; }
    public List<OutgoingPaymentInvoiceRequest> Invoices { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════
// HELPER: Get Open AP Invoices for Supplier
// ═══════════════════════════════════════════════════════════════════
public class OpenAPInvoiceResponse
{
    public int InvoiceId { get; set; }
    public string DocNum { get; set; }
    public DateTime DocDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal DocTotal { get; set; }
    public decimal PaidToDate { get; set; }
    public decimal Balance { get; set; }
    public int DaysOverdue { get; set; }
    public string Status { get; set; }
}