using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using IdentityModel;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Qa2664.Blazor.Menus;
using Volo.Abp.AspNetCore.Components.Web.BasicTheme.Themes.Basic;
using Volo.Abp.AspNetCore.Components.Web.Theming.Routing;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Volo.Abp.UI.Navigation;
using Volo.Abp.AspNetCore.Components.WebAssembly.BasicTheme;
using Volo.Abp.Identity.Blazor.WebAssembly;
using Volo.Abp.SettingManagement.Blazor.WebAssembly;
using Volo.Abp.TenantManagement.Blazor.WebAssembly;

namespace Qa2664.Blazor;

public class TokenServerAuthenticationStateProvider :
    AuthenticationStateProvider, IAccessTokenProvider
{
    private readonly IJSRuntime _jsRuntime;
        private readonly IConfiguration Configuration;

        public TokenServerAuthenticationStateProvider(IJSRuntime jsRuntime, IConfiguration configuration)
        {
            _jsRuntime = jsRuntime;
            Configuration = configuration;
        }

        public async Task<string> GetTokenAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (String.IsNullOrEmpty(token))
                return token;
            
            //check if token is expired from exp value
            if (!CheckTokenIsValid(token))
            {
                token = await GetAccessTokenFromRefreshToken(await GetRefreshTokenAsync());
            }
            
            return token;
        }

        public async Task SetTokenAsync(string token, string refreshToken)
        {
            if (token == null)
            {
                await _jsRuntime.InvokeAsync<object>("localStorage.removeItem", "authToken");
                await _jsRuntime.InvokeAsync<object>("localStorage.removeItem", "refreshToken");
            }
            else
            {
                await _jsRuntime.InvokeAsync<object>("localStorage.setItem", "authToken", token);
                await _jsRuntime.InvokeAsync<object>("localStorage.setItem", "refreshToken", refreshToken);
            }

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task<string> GetRefreshTokenAsync()
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "refreshToken");
        }



        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await GetTokenAsync();
            if (String.IsNullOrEmpty(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public async Task<string> GetAccessTokenFromRefreshToken(string refreshToken)
        {
            if (String.IsNullOrEmpty(refreshToken))
                return null;
            
            var httpClient = new HttpClient();
            var tokenResult = await httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest()
            {
                Address = Configuration["AuthServer:Authority"].EnsureEndsWith('/') + "connect/token",
                Scope = Configuration["AuthServer:Scope"],
                ClientId = Configuration["AuthServer:ClientId"],
                //ClientSecret = Configuration["AuthServer:ClientSecret"],
                RefreshToken = refreshToken
            });
            await SetTokenAsync(tokenResult.AccessToken, tokenResult.RefreshToken);
            return tokenResult.AccessToken;
        }

         

        public async ValueTask<AccessTokenResult> RequestAccessToken()
        {
            var token = new AccessToken() { Value = await GetTokenAsync() };
            return new AccessTokenResult(AccessTokenResultStatus.Success, token, null);
        }

        public async ValueTask<AccessTokenResult> RequestAccessToken(AccessTokenRequestOptions options)
        {
            var token = new AccessToken() { Value = await GetTokenAsync() };
            return new AccessTokenResult(AccessTokenResultStatus.Success, token, null);
            //throw new System.NotImplementedException();
        }

        public long GetTokenExpirationTime(string token)
        {
            var claims = ParseClaimsFromJwt(token);
            var tokenExp = claims.First(claim => claim.Type.Equals("exp")).Value;
            var ticks = long.Parse(tokenExp);
            return ticks;
        }

        public bool CheckTokenIsValid(string token)
        {
            var tokenTicks = GetTokenExpirationTime(token);
            var tokenDate = DateTimeOffset.FromUnixTimeSeconds(tokenTicks).UtcDateTime;

            var now = DateTime.UtcNow.ToUniversalTime();

            var valid = tokenDate >= now;

            return valid;
        }


        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty));
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2:
                    base64 += "==";
                    break;
                case 3:
                    base64 += "=";
                    break;
            }

            return Convert.FromBase64String(base64);
        }
}                

[DependsOn(
    typeof(AbpAutofacWebAssemblyModule),
    typeof(Qa2664HttpApiClientModule),
    typeof(AbpAspNetCoreComponentsWebAssemblyBasicThemeModule),
    typeof(AbpIdentityBlazorWebAssemblyModule),
    typeof(AbpTenantManagementBlazorWebAssemblyModule),
    typeof(AbpSettingManagementBlazorWebAssemblyModule)
)]
public class Qa2664BlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var environment = context.Services.GetSingletonInstance<IWebAssemblyHostEnvironment>();
        var builder = context.Services.GetSingletonInstance<WebAssemblyHostBuilder>();

        ConfigureAuthentication(builder);
        ConfigureHttpClient(context, environment);
        ConfigureBlazorise(context);
        ConfigureRouter(context);
        ConfigureUI(builder);
        ConfigureMenu(context);
        ConfigureAutoMapper(context);
        
        context.Services.AddScoped<TokenServerAuthenticationStateProvider>();
        context.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<TokenServerAuthenticationStateProvider>());
    }

    private void ConfigureRouter(ServiceConfigurationContext context)
    {
        Configure<AbpRouterOptions>(options =>
        {
            options.AppAssembly = typeof(Qa2664BlazorModule).Assembly;
        });
    }

    private void ConfigureMenu(ServiceConfigurationContext context)
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new Qa2664MenuContributor(context.Services.GetConfiguration()));
        });
    }

    private void ConfigureBlazorise(ServiceConfigurationContext context)
    {
        context.Services
            .AddBootstrap5Providers()
            .AddFontAwesomeIcons();
    }

    private static void ConfigureAuthentication(WebAssemblyHostBuilder builder)
    {
        builder.Services.AddOidcAuthentication(options =>
        {
            builder.Configuration.Bind("AuthServer", options.ProviderOptions);
            options.UserOptions.RoleClaim = JwtClaimTypes.Role;
            options.ProviderOptions.DefaultScopes.Add("Qa2664");
            options.ProviderOptions.DefaultScopes.Add("role");
            options.ProviderOptions.DefaultScopes.Add("email");
            options.ProviderOptions.DefaultScopes.Add("phone");
        });
    }

    private static void ConfigureUI(WebAssemblyHostBuilder builder)
    {
        builder.RootComponents.Add<App>("#ApplicationContainer");
    }

    private static void ConfigureHttpClient(ServiceConfigurationContext context, IWebAssemblyHostEnvironment environment)
    {
        context.Services.AddTransient(sp => new HttpClient
        {
            BaseAddress = new Uri(environment.BaseAddress)
        });
    }

    private void ConfigureAutoMapper(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<Qa2664BlazorModule>();
        });
    }
}
