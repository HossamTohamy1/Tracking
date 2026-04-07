using APi_Presentation.Extensions;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

// ============================================================
// 1️⃣ Serilog — Initialize Early (قبل أي حاجة)
// ============================================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("Starting Shipping & Import API");

    var builder = WebApplication.CreateBuilder(args);

    // ✅ استخدام Serilog كـ Logger رئيسي
    builder.Host.UseSerilog();

    // ============================================================
    // 2️⃣ Database
    // ============================================================
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // ============================================================
    // 3️⃣ Identity
    // ============================================================
    builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // ============================================================
    // 4️⃣ Core Services
    // ============================================================
    builder.Services.AddHttpContextAccessor();

    // ============================================================
    // 5️⃣ CORS
    // ============================================================
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowReact", policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173",
                    "https://my-portfolie7.netlify.app")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    // ============================================================
    // 6️⃣ Controllers + JSON
    // ============================================================
    builder.Services.AddControllers()
        .AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.PropertyNamingPolicy =
                System.Text.Json.JsonNamingPolicy.CamelCase;
            opts.JsonSerializerOptions.ReferenceHandler =
                System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

    // ============================================================
    // 7️⃣ Swagger
    // ============================================================
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new()
        {
            Title = "Shipping & Import API",
            Version = "v1"
        });
    });

    // ============================================================
    // 8️⃣ Global Error Handler ✅ (قبل Build)
    // ============================================================
    builder.Services.AddGlobalErrorHandler();

    // ============================================================
    // BUILD
    // ============================================================
    var app = builder.Build();

    // ============================================================
    // 9️⃣ Auto Migrate
    // ============================================================
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
    }

    // ============================================================
    // 🔟 Middleware Pipeline
    // ============================================================

    // ✅ أول حاجة — Global Error Handler
    app.UseGlobalErrorHandler();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shipping API v1");
        c.RoutePrefix = "swagger";
    });

    app.UseCors("AllowReact");
    app.UseHttpsRedirection();
    app.UseStaticFiles();

    // ✅ Serilog Request Logging
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
        };
    });

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("Shipping & Import API started successfully");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}