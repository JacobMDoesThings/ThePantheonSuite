using System.Security.Claims;

namespace ThePantheonSuite.AthenaCore.SasService;

public class SasGenerationRequestModel
{
    private ClaimsPrincipal? _principal;
    private readonly string? _selectedGroupId;

    public required string SelectedGroupId
    {
        get
        {
            if (_principal is null) throw new Exception("Principal has not been set or is null");
            var groups = _principal.FindFirst("groups")?.Value.Split(",", StringSplitOptions.RemoveEmptyEntries);
            if (groups is null || !groups.Contains(_selectedGroupId)) throw new Exception();
            return SelectedGroupId;
        }
        init => _selectedGroupId = value;
    }
    
    public string UserId
    {
        get
        {
            if(_principal is null) throw new Exception("Principal has not been set or is null");
            var claim = _principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            if (claim is null) throw new Exception("Claim for oid has not been set");
            return claim;
        }
    }

    public bool IsPublic { get; init; }

    public void SetPrincipal(ClaimsPrincipal principal)
    {
        _principal = principal;
    }
}