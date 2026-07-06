using BackEnd.Api.Filters;
using BackEnd.Api.Middleware;
using BackEnd.Application.ALLApplicationDependencies;
using BackEnd.Infrastructure.AllInfrastructureDependencies;
using BackEnd.Infrastructure.InfrastructureDependencies;
using BackEnd.Infrastructure.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;
using Hangfire;
using Hangfire.Dashboard;
using BackEnd.Infrastructure.Services;

// ✅ Bootstrap Logger — يشتغل قبل أي حاجة
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("🚀 Resala Charity API is starting...");

    var builder = WebApplication.CreateBuilder(args);

    // ✅ Serilog بدل الـ Default Logger
    builder.Host.UseSerilog((context, services, config) =>
        config
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
    );

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ResultActionFilter>();
    });

    builder.Services.AddSignalR();
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.SetIsOriginAllowed(origin => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    builder.Services
        .AddApplicationDependencies()
        .AuthenticationServices(builder.Configuration)
        .AddInfrastructureDependencies(builder.Configuration)
        .AddFirebase(builder.Configuration);

    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddHangfireServer();

    builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "رسالة الخيرية API",
        Version = "v1",
        Description = "## نظام إدارة رسالة الخيرية\n\n" +
                      "### الأدوار المتاحة:\n" +
                      "| الدور | طريقة الدخول | الصلاحيات |\n" +
                      "|-------|-------------|----------|\n" +
                      "| **Donor** | Phone Number | تسجيل — عرض بياناته — الاشتراك |\n" +
                      "| **Reception** | Username | إدارة المتبرعين — تسجيل تبرعات |\n" +
                      "| **Admin** | Username | كل الصلاحيات + إدارة الموظفين |\n\n" +
                      "### كيفية الاستخدام:\n" +
                      "1. سجّل دخول من `/api/v1/auth/login`\n" +
                      "2. انسخ الـ `token` من الـ Response\n" +
                      "3. اضغط **Authorize** في الأعلى والصق: `Bearer {token}`",
        Contact = new OpenApiContact
        {
            Name = "Resala Dev Team",
            Email = "dev@resala.org"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "أدخل الـ Token هكذا: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.EnableAnnotations();

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

var app = builder.Build();



// ✅ لازم يكون أول حاجة
app.UseMiddleware<ExceptionHandlingMiddleware>();

// ✅ Request Logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} => {StatusCode} في {Elapsed:0.0}ms";
});

// اعمل كده
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BackEnd v1");
    c.RoutePrefix = "swagger"; 
});
app.MapGet("/", () => Results.Redirect("/swagger"));
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<BackEnd.Api.Hubs.SupportChatHub>("/hubs/support");

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAnonymousAuthorizationFilter() }
});

using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate<SubscriptionReminderJob>(
        "subscription-reminder-job",
        job => job.ProcessRemindersAsync(),
        Cron.Daily(2));
}

app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public class HangfireAnonymousAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}