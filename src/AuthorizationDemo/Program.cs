using System.Text;
using AuthorizationDemo.Authorization;
using AuthorizationDemo.Endpoints;
using AuthorizationDemo.Extensions;
using AuthorizationDemo.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// --- Repositories ---
builder.Services.AddSingleton<ICompanyRepository, FakeCompanyRepository>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapCompanyEndpoints();
app.MapInvoiceEndpoints();

app.Run();
