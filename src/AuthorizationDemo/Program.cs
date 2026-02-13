using AuthorizationDemo.Endpoints;
using AuthorizationDemo.Extensions;
using AuthorizationDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Repositories ---
var postgresConnectionString = builder.Configuration.GetConnectionString("PostgreSQL");
if (!string.IsNullOrEmpty(postgresConnectionString))
    builder.Services.AddPostgresCompanyRepository(postgresConnectionString);
else
    builder.Services.AddFakeCompanyRepository();

builder.Services.AddScoped<IAuthTokenService, AuthTokenService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// --- Authentication (JWT) ---
builder.Services.AddJwtBearer(builder.Configuration);

// --- Authorization (policy-based) ---
builder.Services.BuildPolicies();

// --- Swagger ---
builder.Services.AddSwaggerGen();

// --- Observability ---
builder.Services.AddObservabilityAloy(builder.Configuration);

builder.SetupObservabilityLogging();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseReDoc(options =>
{
    options.SpecUrl = "/swagger/v1/swagger.json";
    options.RoutePrefix = "docs";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapCompanyEndpoints();
app.MapInvoiceEndpoints();

if (!string.IsNullOrEmpty(postgresConnectionString))
    await app.SeedPostgresAsync();

app.Run();
