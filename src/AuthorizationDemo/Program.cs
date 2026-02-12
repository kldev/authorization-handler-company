using System.Text;
using AuthorizationDemo.Authorization;
using AuthorizationDemo.Endpoints;
using AuthorizationDemo.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- Repositories ---
builder.Services.AddSingleton<ICompanyRepository, FakeCompanyRepository>();

// --- Authentication (JWT) ---
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// --- Authorization (policy-based) ---
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.CanAccessCompany, policy =>
        policy.AddRequirements(new CompanyAccessRequirement()))
    .AddPolicy(Policies.CanCreateInvoice, policy =>
        policy.AddRequirements(new InvoiceCreateRequirement()));

builder.Services.AddSingleton<IAuthorizationHandler, CompanyAccessHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, InvoiceCreateHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, InvoiceAmountLimitHandler>();

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Paste JWT token from /auth/login"
    });

    options.AddSecurityRequirement(doc => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", doc),
            new List<string>()
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapCompanyEndpoints();
app.MapInvoiceEndpoints();

app.Run();
