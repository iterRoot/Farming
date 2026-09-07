using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.EmailSettings;

// ═══════════════════════════════════════════════════════════════
// EMAIL SETTINGS  (route: /EmailAccount)  — SMTP profiles
// GET    /EmailAccount            → list (no passwords)
// GET    /EmailAccount/{id}       → detail (HasPassword flag, no value)
// POST   /EmailAccount            → create
// PUT    /EmailAccount/{id}       → update (blank password keeps existing)
// DELETE /EmailAccount/{id}       → delete
// ═══════════════════════════════════════════════════════════════
public class EmailAccountController : MyController
{
    private readonly IEmailAccountRepository _repo;

    public EmailAccountController(IEmailAccountRepository repo) => _repo = repo;

    // Only one profile may be the default — clear the flag on the others.
    private void ClearOtherDefaults(int keepId)
    {
        foreach (var other in _repo.GetAll().Where(x => x.IsDefault && x.Id != keepId))
        {
            other.IsDefault = false;
            _repo.Update(other);
        }
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _repo.GetAll()
            .OrderByDescending(x => x.IsDefault).ThenBy(x => x.Name)
            .Select(x => new EmailAccountListDto
            {
                Id = x.Id, Name = x.Name, SmtpHost = x.SmtpHost, SmtpPort = x.SmtpPort,
                Encryption = x.Encryption, FromEmail = x.FromEmail, FromName = x.FromName,
                IsDefault = x.IsDefault, InActive = x.InActive ?? false,
            })
            .ToList();
        return Ok(list);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult GetOne(int id)
    {
        var x = _repo.GetSingle(e => e.Id == id);
        if (x == null) return NotFound($"Email account {id} not found.");
        return Ok(new EmailAccountDetailDto
        {
            Id = x.Id, Name = x.Name, SmtpHost = x.SmtpHost, SmtpPort = x.SmtpPort,
            Encryption = x.Encryption, UseAuth = x.UseAuth, Username = x.Username,
            HasPassword = !string.IsNullOrEmpty(x.Password),
            FromEmail = x.FromEmail, FromName = x.FromName, ReplyTo = x.ReplyTo,
            Signature = x.Signature, IsDefault = x.IsDefault, InActive = x.InActive ?? false,
        });
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Create([FromBody] EmailAccountSaveRequest req)
    {
        var err = Validate(req);
        if (err != null) return BadRequest(err);

        var e = new EmailAccount
        {
            Name = req.Name.Trim(), SmtpHost = req.SmtpHost.Trim(), SmtpPort = req.SmtpPort,
            Encryption = req.Encryption, UseAuth = req.UseAuth,
            Username = req.Username, Password = req.Password,
            FromEmail = req.FromEmail.Trim(), FromName = req.FromName, ReplyTo = req.ReplyTo,
            Signature = req.Signature, IsDefault = req.IsDefault, InActive = req.InActive,
            CreatedAt = DateTime.UtcNow,
        };
        _repo.Add(e);
        _repo.Commit();

        if (e.IsDefault) { ClearOtherDefaults(e.Id); _repo.Commit(); }

        return Ok(new { id = e.Id, message = "Email account created." });
    }

    [AllowAnonymous]
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] EmailAccountSaveRequest req)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Email account {id} not found.");
        var err = Validate(req);
        if (err != null) return BadRequest(err);

        e.Name = req.Name.Trim();
        e.SmtpHost = req.SmtpHost.Trim();
        e.SmtpPort = req.SmtpPort;
        e.Encryption = req.Encryption;
        e.UseAuth = req.UseAuth;
        e.Username = req.Username;
        // null → keep existing; "" → clear; value → replace
        if (req.Password != null) e.Password = req.Password.Length == 0 ? null : req.Password;
        e.FromEmail = req.FromEmail.Trim();
        e.FromName = req.FromName;
        e.ReplyTo = req.ReplyTo;
        e.Signature = req.Signature;
        e.IsDefault = req.IsDefault;
        e.InActive = req.InActive;
        e.UpdatedAt = DateTime.UtcNow;
        _repo.Update(e);
        _repo.Commit();

        if (e.IsDefault) { ClearOtherDefaults(e.Id); _repo.Commit(); }

        return Ok(new { id = e.Id, message = "Email account updated." });
    }

    [AllowAnonymous]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Email account {id} not found.");
        _repo.Remove(e);
        _repo.Commit();
        return NoContent();
    }

    private static string? Validate(EmailAccountSaveRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Name))      return "Profile name is required.";
        if (string.IsNullOrWhiteSpace(r.SmtpHost))  return "SMTP host is required.";
        if (r.SmtpPort <= 0 || r.SmtpPort > 65535)  return "SMTP port must be between 1 and 65535.";
        if (string.IsNullOrWhiteSpace(r.FromEmail)) return "From e-mail address is required.";
        if (!r.FromEmail.Contains('@'))             return "From e-mail address is not valid.";
        var enc = new[] { "None", "SSL", "TLS" };
        if (!enc.Contains(r.Encryption))            return "Encryption must be None, SSL or TLS.";
        return null;
    }
}
