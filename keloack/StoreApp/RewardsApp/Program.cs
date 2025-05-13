using Microsoft.EntityFrameworkCore;
using RewardsApp.Data;
using RewardsApp.Services;

var builder = WebApplication.CreateBuilder(args);

// 1) Configure EF Core with SQLite
builder.Services.AddDbContext<RewardsContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("RewardsDb")));

// 2) Register VaultService
builder.Services.AddSingleton<VaultService>();

// 3) Add Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Automatically apply any pending migrations (optional, but handy for dev)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RewardsContext>();
    db.Database.Migrate();
}

// 4) Map Controllers
app.MapControllers();

app.Run();
