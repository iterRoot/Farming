namespace FarmingApi.Modules.Administration.Security;

public class PasswordPolicyDto
{
    public int  MinLength         { get; set; } = 8;
    public bool RequireUppercase  { get; set; } = true;
    public bool RequireLowercase  { get; set; } = true;
    public bool RequireDigit      { get; set; } = true;
    public bool RequireSpecial    { get; set; } = true;
    public int  MaxFailedAttempts { get; set; } = 5;
    public int? ExpiryDays        { get; set; }
}

// One row of the "who needs a new password" table under Password
// Administration. Built from the User table — nothing is stored here.
public class PasswordAgeRow
{
    public int       Id                { get; set; }
    public string    UserId            { get; set; } = null!; // the login id, e.g. "DARA.SOK"
    public string    UserName          { get; set; } = null!;
    public DateTime? PasswordChangedAt { get; set; }
    public int?      AgeDays           { get; set; }
    public bool      Expired           { get; set; }
}
