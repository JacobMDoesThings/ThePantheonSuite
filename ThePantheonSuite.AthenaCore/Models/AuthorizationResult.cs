namespace ThePantheonSuite.AthenaCore.Models;

public class AuthorizationResult
{
    public bool IsAuthorized { get; set; }
    public string UserId { get; set; }
    public List<string> Roles { get; set; } = [];
}

public class UnauthorizedAccessException : Exception { }
