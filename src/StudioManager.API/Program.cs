using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StudioManager.API.Infrastructure;
using StudioManager.DataLayer.Context;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.IService.ICalendarServices;
using StudioManager.ServiceLayer.IService.IClientServices;
using StudioManager.ServiceLayer.IService.IFileServices;
using StudioManager.ServiceLayer.IService.IFinanceServices;
using StudioManager.ServiceLayer.IService.IGearServices;
using StudioManager.ServiceLayer.IService.IProjectServices;
using StudioManager.ServiceLayer.IService.ISessionServices;
using StudioManager.ServiceLayer.IService.ISettingsServices;
using StudioManager.ServiceLayer.IService.IUserServices;
using StudioManager.ServiceLayer.Mapper;
using StudioManager.ServiceLayer.Service.CalendarServices;
using StudioManager.ServiceLayer.Service.ClientServices;
using StudioManager.ServiceLayer.Service.FileServices;
using StudioManager.ServiceLayer.Service.FinanceServices;
using StudioManager.ServiceLayer.Service.GearServices;
using StudioManager.ServiceLayer.Service.ProjectServices;
using StudioManager.ServiceLayer.Service.SessionServices;
using StudioManager.ServiceLayer.Service.SettingsServices;
using StudioManager.ServiceLayer.Service.UserServices;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

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
    builder.Services.AddScoped<IPhotoService, PhotoService>();

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
            // Opened via https (on the server): the cookie is only ever sent over https.
            // Opened via http://localhost: it still works. "https or not" comes from the proxies, see ForwardedHeaders below.
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;

            // A deleted login is logged out right away (the cookie itself would stay valid for 14 days)
            options.Events.OnValidatePrincipal = async context =>
            {
                var userId = int.Parse(context.Principal!.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();

                if (await userService.Get(userId) == null)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            };

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

    // ---------- Running behind proxies ----------
    // The API is only reached through nginx (web container) - on the server there is also an HTTPS proxy in front.
    // The proxies pass on the real IP address of the browser (X-Forwarded-For) and whether it used https (X-Forwarded-Proto).
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

        // The proxies run in Docker networks with changing addresses. That is safe, because the API container
        // itself is not reachable from outside - every request comes through nginx.
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();

        // Number of proxies in front of the API: 1 locally (nginx), 2 on a server with an HTTPS proxy in front.
        // Only that many entries are read from the right, so a browser can not fake its IP address.
        options.ForwardLimit = builder.Configuration.GetValue("ReverseProxy:ProxyCount", 1);
    });

    // ---------- Rate limiting ----------
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.AddPolicy(RateLimits.Login, context => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(15)
            }));

        options.AddPolicy(RateLimits.Public, context => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1)
            }));
    });

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
    builder.Services.AddScoped<ICalendarService, CalendarService>();
    builder.Services.AddScoped<IFinanceService, FinanceService>();
    builder.Services.AddScoped<ITimeEntryService, TimeEntryService>();
    builder.Services.AddScoped<IProjectCostItemService, ProjectCostItemService>();
    builder.Services.AddScoped<IStandardTaskService, StandardTaskService>();
    builder.Services.AddScoped<IClientService, ClientService>();
    builder.Services.AddScoped<IBandService, BandService>();
    builder.Services.AddScoped<IProjectService, ProjectService>();
    builder.Services.AddScoped<ISongService, SongService>();
    builder.Services.AddScoped<ISongSectionService, SongSectionService>();
    builder.Services.AddScoped<ISongTrackService, SongTrackService>();
    builder.Services.AddScoped<IProjectLinkService, ProjectLinkService>();
    builder.Services.AddScoped<IDrumTuningService, DrumTuningService>();
    builder.Services.AddScoped<IExternalGearService, ExternalGearService>();
    builder.Services.AddScoped<IBandGearLinkService, BandGearLinkService>();
    builder.Services.AddScoped<ISessionService, SessionService>();
    builder.Services.AddScoped<ISessionTaskService, SessionTaskService>();
    builder.Services.AddScoped<IProjectInstrumentService, ProjectInstrumentService>();
    builder.Services.AddScoped<IInstrumentSetupItemService, InstrumentSetupItemService>();
    builder.Services.AddScoped<ISetupTemplateService, SetupTemplateService>();
    builder.Services.AddScoped<IInputListService, InputListService>();
    builder.Services.AddScoped<IInputGroupService, InputGroupService>();
    builder.Services.AddScoped<IStudioInputService, StudioInputService>();

    var app = builder.Build();

    // The Swashbuckle CLI (used by "npm run api:generate") only starts the app to read the endpoints - no database needed
    var isSwaggerGeneration = Assembly.GetEntryAssembly()?.GetName().Name == "dotnet-swagger";
    if (!isSwaggerGeneration)
    {
        await InitializeDatabase(app);
    }

    // First: take the real IP address and https / http from the proxies (used by the rate limiter and the cookie)
    app.UseForwardedHeaders();

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();

    // API documentation at /swagger - only during development, not on the public server
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRateLimiter();
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
