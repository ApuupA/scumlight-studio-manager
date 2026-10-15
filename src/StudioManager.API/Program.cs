using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StudioManager.API.Infrastructure;
using StudioManager.DataLayer.Context;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.IService.IFileServices;
using StudioManager.ServiceLayer.IService.IGearServices;
using StudioManager.ServiceLayer.IService.IUserServices;
using StudioManager.ServiceLayer.Mapper;
using StudioManager.ServiceLayer.Service.FileServices;
using StudioManager.ServiceLayer.Service.GearServices;
using StudioManager.ServiceLayer.Service.UserServices;
using System.Reflection;
using System.Text.Json.Serialization;

// Logs startup errors before the full configuration is loaded
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Console());

    // ---------- Database ----------
    var connectionString = builder.Configuration.GetConnectionString("StudioDatabase");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Connection string 'StudioDatabase' must be set in configuration.");
    }

    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

    // ---------- File storage ----------
    var fileStorageRoot = builder.Configuration["FileStorage:RootPath"]
        ?? throw new InvalidOperationException("FileStorage:RootPath must be set in configuration.");

    builder.Services.AddSingleton<IFileStorageService>(new LocalFileStorageService(fileStorageRoot));

    // The keys that encrypt the login cookie are stored on disk, so logins survive a restart of the container
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(fileStorageRoot, ".keys")));

    // ---------- Authentication (cookie based) ----------
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.Name = "studio.auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;

            // An API should answer with 401 / 403 instead of redirecting to a login page
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

    // Every endpoint requires a logged in user, unless it is marked with [AllowAnonymous]
    builder.Services.AddAuthorizationBuilder()
        .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

    // ---------- Controllers, error handling, Swagger ----------
    builder.Services.AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<ApiExceptionHandler>();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new() { Title = "Studio Manager API", Version = "v1" });

        // Makes "string" and "string?" distinguishable in swagger.json, so the generated TypeScript types are exact
        options.SupportNonNullableReferenceTypes();
        options.NonNullableReferenceTypesAsRequired();
    });

    builder.Services.AddAutoMapper(cfg =>
    {
        // Without a key AutoMapper only logs a license warning. An empty key would be validated and log an error.
        var licenseKey = builder.Configuration["AutoMapper:LicenseKey"];
        if (!string.IsNullOrWhiteSpace(licenseKey))
        {
            cfg.LicenseKey = licenseKey;
        }
    }, typeof(MappingProfile));

    // ---------- Repositories & services ----------
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    builder.Services.AddScoped<IGearCategoryService, GearCategoryService>();
    builder.Services.AddScoped<IGearItemService, GearItemService>();
    builder.Services.AddScoped<IUserService, UserService>();

    var app = builder.Build();

    // The Swashbuckle CLI (used by "npm run api:generate") only starts the app to read the endpoints - no database needed
    var isSwaggerGeneration = Assembly.GetEntryAssembly()?.GetName().Name == "dotnet-swagger";
    if (!isSwaggerGeneration)
    {
        await InitializeDatabase(app);
    }

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();

    // API documentation at /swagger - only during development, not on the public server
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Applies open EF Core migrations and creates the owner account on the first start.
/// </summary>
static async Task InitializeDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();

    var owner = app.Configuration.GetSection("OwnerAccount");
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
    await userService.EnsureOwnerExists(
        owner["Email"] ?? throw new InvalidOperationException("OwnerAccount:Email must be set in configuration."),
        owner["Password"] ?? throw new InvalidOperationException("OwnerAccount:Password must be set in configuration."),
        owner["DisplayName"] ?? "Studio Owner");
}
