using Microsoft.EntityFrameworkCore;
using Sisora.API.Data;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Controllers ───────────────────────────────────────────
builder.Services.AddControllers();

// ── API Docs (Scalar) ─────────────────────────────────────
builder.Services.AddOpenApi();

// ── CORS ──────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("SisoraPolicy", policy =>
    {
        policy
            .WithOrigins(builder.Configuration["AllowedOrigins"] ?? "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // required for SignalR
    });
});

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("SisoraPolicy");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();