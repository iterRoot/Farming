namespace FarmingApi.Modules.Administration.EmailSettings;

// List rows never expose the password.
public class EmailAccountListDto
{
    public int    Id         { get; set; }
    public string Name       { get; set; } = null!;
    public string SmtpHost   { get; set; } = null!;
    public int    SmtpPort   { get; set; }
    public string Encryption { get; set; } = null!;
    public string FromEmail  { get; set; } = null!;
    public string? FromName  { get; set; }
    public bool   IsDefault  { get; set; }
    public bool   InActive   { get; set; }
}

// Detail returns whether a password is stored, not the value itself.
public class EmailAccountDetailDto
{
    public int     Id          { get; set; }
    public string  Name        { get; set; } = null!;
    public string  SmtpHost    { get; set; } = null!;
    public int     SmtpPort    { get; set; }
    public string  Encryption  { get; set; } = null!;
    public bool    UseAuth     { get; set; }
    public string? Username    { get; set; }
    public bool    HasPassword { get; set; }
    public string  FromEmail   { get; set; } = null!;
    public string? FromName    { get; set; }
    public string? ReplyTo     { get; set; }
    public string? Signature   { get; set; }
    public bool    IsDefault   { get; set; }
    public bool    InActive    { get; set; }
}

public class EmailAccountSaveRequest
{
    public string  Name        { get; set; } = null!;
    public string  SmtpHost    { get; set; } = null!;
    public int     SmtpPort    { get; set; } = 587;
    public string  Encryption  { get; set; } = "TLS";
    public bool    UseAuth     { get; set; } = true;
    public string? Username    { get; set; }
    // When null/omitted on update the stored password is kept; empty string clears it.
    public string? Password    { get; set; }
    public string  FromEmail   { get; set; } = null!;
    public string? FromName    { get; set; }
    public string? ReplyTo     { get; set; }
    public string? Signature   { get; set; }
    public bool    IsDefault   { get; set; }
    public bool    InActive    { get; set; }
}
