using Microsoft.AspNetCore.Components;
using Microsoft.Identity.Client;
using ThePantheonSuite.ArtemisUI.Configuration.MSALClient;

namespace ThePantheonSuite.ArtemisUI.Components.Pages.Auth;

public partial class LoginPage : ComponentBase
{
    [Inject]
    NavigationManager NavigationManager { get; set; } = null!;
    protected override Task OnInitializedAsync()
    {
        IAccount cachedUserAccount; 
        InvokeAsync(async
            ()=> cachedUserAccount = await PublicClientSingleton.Instance.MSALClientHelper.FetchSignedInUserFromCache());

        InvokeAsync(async() => 
        {
            // if (cachedUserAccount == null)
            // {
            //     SignInButton.IsEnabled = true;
            // }
            // else
            // {
                //await Shell.Current.GoToAsync("claimsview");
            //}
           
            // NavigationManager.NavigateTo("/claims");
        });
        return base.OnInitializedAsync();
    }

    private async Task OnSignInClickedAsync()
    {
        await PublicClientSingleton.Instance.AcquireTokenSilentAsync();
    }
}