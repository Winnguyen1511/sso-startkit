using Microsoft.IdentityModel.Tokens;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
/// <summary>
/// Provides a forwarding func for JWT vs reference tokens (based on existence of dot in token)
/// </summary>
/// <param name="introspectionScheme">Scheme name of the introspection handler</param>
/// <returns></returns>
static Func<HttpContext, string> ForwardReferenceToken(string introspectionScheme = "introspection")
{
    string Select(HttpContext context)
    {
        var (scheme, credential) = GetSchemeAndCredential(context);

        if (scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase) &&
            !credential.Contains("."))
        {
            return introspectionScheme;
        }

        return null;
    }

    return Select;
}

/// <summary>
/// Extracts scheme and credential from Authorization header (if present)
/// </summary>
/// <param name="context"></param>
/// <returns></returns>
static (string, string) GetSchemeAndCredential(HttpContext context)
{
    var header = context.Request.Headers["Authorization"].FirstOrDefault();

    if (string.IsNullOrEmpty(header))
    {
        return ("", "");
    }

    var parts = header.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length != 2)
    {
        return ("", "");
    }

    return (parts[0], parts[1]);
}
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:5001";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
        options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
        options.ForwardDefaultSelector = ForwardReferenceToken("introspection");
    })
    .AddOAuth2Introspection("introspection", options =>
    {
        options.Authority = "https://localhost:5001";

        options.ClientId = "api1";
        options.ClientSecret = "secret";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "api1");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireAuthorization("ApiScope");

app.Run();
