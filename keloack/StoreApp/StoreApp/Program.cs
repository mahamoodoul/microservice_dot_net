using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StoreApp.Data;
using StoreApp.Helper;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
// Register HttpClient for Rewards API

// builder.Services.AddSingleton<VaultService>();
builder.Services.AddHttpClient();

// 1) Configure EF Core (already present)
builder.Services.AddDbContext<StoreContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// 2) Configure Keycloak / OIDC
var keycloakConfig = builder.Configuration.GetSection("Keycloak");
var authority = keycloakConfig["Authority"];           // e.g. "http://localhost:8080/realms/StoreRealm"
var clientId = keycloakConfig["ClientId"];             // e.g. "store-app-client"
var clientSecret = keycloakConfig["ClientSecret"];     // e.g. "xxx-xxx-xxx"
var callbackPath = keycloakConfig["CallbackPath"];     // "/signin-oidc"
var signOutCallbackPath = keycloakConfig["SignOutCallbackPath"]; // "/signout-callback-oidc"

builder.Services.AddAuthentication(options =>
{
    // Use cookies to maintain an authenticated user
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // Challenge scheme = how to force user to log in
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    options.Authority = authority;
    options.ClientId = clientId;
    options.ClientSecret = clientSecret;
    options.ResponseType = "code"; // standard OIDC flow
    options.SaveTokens = true;
    options.CallbackPath = callbackPath;
    options.SignedOutCallbackPath = signOutCallbackPath;

    options.RequireHttpsMetadata = false;

    // Usually Keycloak sets `aud` to the clientId, so let's not strictly validate audience
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false,
        ValidateIssuer = true
    };
    options.Scope.Clear();
    options.ClaimActions.MapJsonKey("email", "email");
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");

    options.Events = new OpenIdConnectEvents
    {
        OnTokenValidated = context =>
        {
            var principal = context.Principal;
            if (principal != null)
            {
                Console.WriteLine("[DEBUG] OnTokenValidated => The token's claims:");
                foreach (var claim in principal.Claims)
                {
                    Console.WriteLine($"    {claim.Type}: {claim.Value}");
                }
            }
            return Task.CompletedTask;
        }
    };



});

var app = builder.Build();


// --- Database Migration and Seeding ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Get your EF Core context
        var context = services.GetRequiredService<StoreContext>();
        // Apply any pending migrations
        context.Database.Migrate();
        // Seed the database
        SeedData.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}
// --- End of Database Migration and Seeding ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --- Prometheus Middleware ---
// This middleware tracks HTTP request metrics.
app.UseHttpMetrics();

// 3) Enable authentication & authorization
app.UseAuthentication();
app.UseAuthorization();

// 4) Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Exposes metrics on http://localhost:<port>/metrics (accessible publicly)
app.MapMetrics();

app.Run();