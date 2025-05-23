using System.Security.Claims;
using ThePantheonSuite.AthenaCore.AuthzAuthn;

namespace ThePantheonSuite.AthenaCore.Interfaces;

public interface ISasAuthorizationService
{
    public string SelectedGroup(ClaimsPrincipal claimsPrincipal);
    public string UserId(ClaimsPrincipal claimsPrincipal);
}
