using System.Security.Claims;
using ThePantheonSuite.AthenaCore.AuthzAuthn;
using ThePantheonSuite.AthenaCore.Interfaces;

namespace ThePantheonSuite.MinervaServices.AuthorizationService;

public class SasAuthorizationService : ISasAuthorizationService
{
    public string SelectedGroup(ClaimsPrincipal claimsPrincipal)
    {
        throw new NotImplementedException();
    }

    public string UserId(ClaimsPrincipal claimsPrincipal)
    {
        throw new NotImplementedException();
    }
}

