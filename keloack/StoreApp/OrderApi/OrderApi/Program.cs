using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using OrderApi.Data;
using OrderApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1) Read connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2) Configure DbContext to use SQLite
builder.Services.AddDbContext<OrderContext>(options =>
{
    options.UseSqlite(connectionString);
});

// 3) Set up Authentication/Authorization (Keycloak, etc.)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:8080/realms/StoreRealm";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true
        };
        options.RequireHttpsMetadata = false;
        // OPTIONAL: Hook the OnTokenValidated event to debug claims
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var principal = context.Principal;
                Console.WriteLine("[DEBUG] OnTokenValidated => The token's claims:");
                foreach (var claim in principal.Claims)
                {
                    Console.WriteLine($"    {claim.Type}: {claim.Value}");
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// --- Database Migration and Seeding for Order API ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<OrderContext>();
        // Apply any pending migrations
        context.Database.Migrate();
        // Seed the database if needed
        SeedData.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the order database.");
    }
}
// --- End of Migration and Seeding ---



app.UseAuthentication();
app.UseAuthorization();

// 3) Minimal API endpoint
app.MapPost("/orders", [Authorize] async (
    OrderContext db,
    HttpContext httpContext,
    OrderRequest request
) =>
{
    // Correct usage
    var token = await httpContext.GetTokenAsync("access_token");

    Console.WriteLine($" [DEBUG] access token: {token}");
    
    var email = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

    var username = httpContext.User.FindFirst("name")?.Value ?? "unknown_user";
    

    var order = new OrderInfo
    {
        Username = username,
        Email = email,
        ProductName = request.ProductName,
        ProductPrice = request.ProductPrice,
        CreatedAt = DateTime.UtcNow
    };

    db.Orders.Add(order);
    await db.SaveChangesAsync();

    return Results.Created($"/orders/{order.Id}", order);
});

app.MapGet("/orders", [Authorize] async (OrderContext db, HttpContext httpContext) =>
{

    // 1) Try to get the user's display name from the "name" claim
        var fullName = httpContext.User.FindFirst("name")?.Value ?? "unknown_user";
        // 2) Also try to get the user's email from the claim
        var email = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "no@email";
        
        var realmAccessJson = httpContext.User.FindFirst("resource_access")?.Value;
        if (!string.IsNullOrEmpty(realmAccessJson))
        {
            Console.WriteLine($"[DEBUG] realm_access: {realmAccessJson}");
        }
        else
        {
            Console.WriteLine("No realm_access claim found.");
        }

        // 3) Query the orders, matching either name or email
        var userOrders = await db.Orders
            .Where(o => o.Username == fullName 
                     || o.Email == email)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return userOrders;

});

app.MapGet("/orders_all", [Authorize] async (OrderContext db, HttpContext httpContext) =>
{

    return await db.Orders
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

});


app.Run();
