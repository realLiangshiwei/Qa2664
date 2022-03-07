using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Volo.Abp.IdentityModel;

namespace Qa2664.Blazor.Pages;

public partial class Index
{
    [Inject] protected IConfiguration Configuration { get; set; }

    [Inject] protected TokenServerAuthenticationStateProvider AuthenticationStateProvider { get; set; }

    private string UserName { get; set; }

    private string Password { get; set; }

    private async Task LoginAsync()
    {
        var httpClient = new HttpClient();

        var passwordTokenRequest = new PasswordTokenRequest()
        {
            Address = Configuration["AuthServer:Authority"].EnsureEndsWith('/') + "connect/token",
            Scope = Configuration["AuthServer:Scope"],
            ClientId = Configuration["AuthServer:ClientId"],
            ClientSecret = Configuration["AuthServer:ClientSecret"],
            UserName = UserName,
            Password = Password
        };


        try
        {
            var result = await httpClient.RequestPasswordTokenAsync(passwordTokenRequest);

            if (result.IsError)
            {
                await Message.Error("Invalid login " + result.Error);
                return;
            }

            await AuthenticationStateProvider.SetTokenAsync(result.AccessToken);

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception e)
        {
            await Message.Error("Invalid login " + e.Message);
            return;
        }

        await Message.Success("Login success");
    }
}
