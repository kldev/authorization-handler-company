namespace AuthorizationDemo.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerGen(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
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

        return services;
    }
}