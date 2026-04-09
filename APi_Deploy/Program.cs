using APi_Presentation.Extensions;
using Application.Common;
using Application.Common.Mappings;
using Application.Features.Auth.Commands.Register;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data;
using Infrastructure.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Text;
using Infrastructure.Repositories;

// ============================================================
// 1️⃣ Serilog 
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
        retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("Starting Shipping & Import API");

    var builder = WebApplication.CreateBuilder(args);
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
    // 4️⃣ JWT Authentication
    // ============================================================
    var jwtSection = builder.Configuration.GetSection("JwtSettings");
    builder.Services.Configure<JwtSettings>(jwtSection);

    var jwtSettings = jwtSection.Get<JwtSettings>()!;
    var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; 
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    // ============================================================
    // 5️⃣ MediatR + HttpContext
    // ============================================================
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(RegisterCommandHandler).Assembly));

    builder.Services.AddAutoMapper(typeof(MappingProfile));

    // ============================================================
    // 6️⃣ Seeder (Scoped)
    // ============================================================
    builder.Services.AddScoped(typeof(IGeneralRepository<>), typeof(GeneralRepository<>));

    builder.Services.AddScoped<RoleSeeder>();
    // ============================================================
    // 7️⃣ CORS
    // ============================================================
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowReact", policy =>
            policy
                .WithOrigins(
                    "http://localhost:5173",
                    "https://gleeful-churros-3fcff0.netlify.app"
                )
                .AllowAnyHeader()
                .AllowAnyMethod());
    });
    // ============================================================
    // 8️⃣ Controllers + JSON
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
    // 9️⃣ Swagger (بدون SecurityRequirement)
    // ============================================================
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Shipping & Import API",
            Version = "v1"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token here. Example: Bearer eyJhbGci..."
        });
    });

    // ============================================================
    // 🔟 Global Error Handler
    // ============================================================
    builder.Services.AddGlobalErrorHandler();

    // ============================================================
    // BUILD
    // ============================================================
    var app = builder.Build();

    var isEfMigration = args.Contains("/efmigration");

    if (!isEfMigration)
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();

            var seeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
            await seeder.SeedAsync();
        }

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

        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diag, http) =>
            {
                diag.Set("RequestHost", http.Request.Host.Value);
                diag.Set("RequestScheme", http.Request.Scheme);
                diag.Set("RemoteIpAddress", http.Connection.RemoteIpAddress);
            };
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        Log.Information("Shipping & Import API started successfully");
        await app.RunAsync();
    }
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