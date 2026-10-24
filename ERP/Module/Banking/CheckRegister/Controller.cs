using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;
using IPay = FarmingApi.Modules.Banking.IncomingPayment.IncomingPayment;
using OPay = FarmingApi.Modules.Banking.OutgoingPayment.OutgoingPayment;

namespace FarmingApi.Modules.Banking.CheckRegister;

// ═══════════════════════════════════════════════════════════════════
// CHECK REGISTER  (route: /CheckRegister)
// Read-only report over IncomingPayment + OutgoingPayment rows made by
// check — there is no separate "check" document, a check is a payment
// method. This union is the register SAP B1 shows under Banking.
// GET /CheckRegister?direction=&status=  → filtered rows
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class CheckRegisterController : ControllerBase
{
    private readonly MyDbContext _db;
    public CheckRegisterController(MyDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Gets([FromQuery] string? direction, [FromQuery] string? status)
    {
        var incoming = _db.Set<IPay>()
            .Where(p => p.PaymentMethod == "Check")
            .Select(p => new CheckRegisterRow
            {
                Direction     = "Incoming",
                PaymentId     = p.Id,
                DocNum        = p.DocNum,
                DocDate       = p.DocDate,
                CardCode      = p.CardCode,
                CardName      = p.CardName,
                CheckNumber   = p.CheckNumber,
                CheckDate     = p.CheckDate,
                CheckStatus   = p.CheckStatus,
                BankAccount   = p.BankAccount,
                BankName      = p.BankName,
                Amount        = p.DocTotal,
                Currency      = p.Currency,
                PaymentStatus = p.Status,
                Deposited     = p.Deposited,
            });

        var outgoing = _db.Set<OPay>()
            .Where(p => p.PaymentMethod == "Check")
            .Select(p => new CheckRegisterRow
            {
                Direction     = "Outgoing",
                PaymentId     = p.Id,
                DocNum        = p.DocNum,
                DocDate       = p.DocDate,
                CardCode      = p.CardCode,
                CardName      = p.CardName,
                CheckNumber   = p.CheckNumber,
                CheckDate     = p.CheckDate,
                CheckStatus   = p.CheckStatus,
                BankAccount   = p.BankAccount,
                BankName      = p.BankName,
                Amount        = p.DocTotal,
                Currency      = p.Currency,
                PaymentStatus = p.Status,
                Deposited     = false,
            });

        var rows = incoming.ToList().Concat(outgoing.ToList());

        if (!string.IsNullOrWhiteSpace(direction))
            rows = rows.Where(r => string.Equals(r.Direction, direction, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(status))
            rows = rows.Where(r => string.Equals(r.CheckStatus, status, StringComparison.OrdinalIgnoreCase));

        return Ok(rows.OrderByDescending(r => r.DocDate).ThenByDescending(r => r.PaymentId).ToList());
    }
}
