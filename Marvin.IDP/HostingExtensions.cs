using Duende.IdentityServer;
using Marvin.IDP.DbContexts;
using Marvin.IDP.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Marvin.IDP;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(
        this WebApplicationBuilder builder)
    {
        // configures IIS out-of-proc settings 
        builder.Services.Configure<IISOptions>(iis =>
        {
            iis.AuthenticationDisplayName = "Windows";
            iis.AutomaticAuthentication = false;
        });
        // ..or configures IIS in-proc settings
        builder.Services.Configure<IISServerOptions>(iis =>
        {
            iis.AuthenticationDisplayName = "Windows";
            iis.AutomaticAuthentication = false;
        });


        // uncomment if you want to add a UI
        builder.Services.AddRazorPages();

        builder.Services.AddScoped<IPasswordHasher<Entities.User>,
            PasswordHasher<Entities.User>>();

        builder.Services.AddScoped<ILocalUserService, LocalUserService>();

        builder.Services.AddDbContext<IdentityDbContext>(options =>
        {
            options.UseSqlite(
                builder.Configuration
                .GetConnectionString("MarvinIdentityDBConnectionString"));
        });


        builder.Services.AddIdentityServer(options =>
        {
            // https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
            options.EmitStaticAudienceClaim = true;
        })
            .AddProfileService<LocalUserProfileService>()
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryApiResources(Config.ApiResources)
            .AddInMemoryClients(Config.Clients);

        builder.Services
             .AddAuthentication()
             .AddOpenIdConnect("AAD", "Azure Active Directory", options =>
             {
                 options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                 options.Authority = "https://login.microsoftonline.com/b56c03c6-69b4-4a79-8620-18fd5f42318c/v2.0";
                 options.ClientId = "05699d6d-6afc-4972-8d8d-03d783f6b214";
                 options.ClientSecret = "mVZ8Q~XCT2ExQvLo.-T5g8xaK2KmvPtb1ojgQcKc";
                 options.ResponseType = "code";
                 options.CallbackPath = new PathString("/signin-aad/");
                 options.SignedOutCallbackPath = new PathString("/signout-aad/");
                 options.Scope.Add("email");
                 options.Scope.Add("offline_access");
                 options.SaveTokens = true;
             });

        builder.Services.AddAuthentication()
            .AddFacebook("Facebook",
               options =>
               {
                   options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                   options.AppId = "709489410966964";
                   options.AppSecret = "5326f2265a8b475ba06b6ee63acf7d2d";
               });

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // uncomment if you want to add a UI
        app.UseStaticFiles();
        app.UseRouting();

        app.UseIdentityServer();

        // uncomment if you want to add a UI
        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}
