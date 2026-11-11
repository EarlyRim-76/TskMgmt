using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using TskMgmt.Api.Context;
using TskMgmt.Api.Interface;
using TskMgmt.Api.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<PasswordHasher<TskMgmt.Api.Entities.User>>();
builder.Services.AddScoped(typeof(IGenericService<>),typeof(GenericService<>));
builder.Services.AddScoped<RefreshTokenService>();

builder.Services.AddControllers();
//builder.Services.AddOpenApi(); commented because conflix with swagger authorize


builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwt["Issuer"],
                ValidAudience = jwt["Audience"],

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwt["SecretKey"]!)
                ),

                ClockSkew = TimeSpan.FromSeconds(30)
            };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var tokenId = context.Principal?
                    .FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                if (string.IsNullOrEmpty(tokenId))
                {
                    context.Fail("Missing JWT ID.");
                    return;
                }

                var dbContext = context.HttpContext
                    .RequestServices
                    .GetRequiredService<AppDbContext>();

                var isRevoked = await dbContext.RevokedTokens
                    .AnyAsync(x => x.TokenId == tokenId);

                if (isRevoked)
                {
                    context.Fail("This token has been revoked.");
                }
            },

            OnAuthenticationFailed = context =>
            {
                Console.WriteLine(
                    $"JWT ERROR: {context.Exception}"
                );

                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Management Api",
        Version = "v1"
    });

    o.AddSecurityDefinition("Bearer",

        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authentication"
        }
    );

    o.AddSecurityRequirement(document =>
      new OpenApiSecurityRequirement
      {
          [new OpenApiSecuritySchemeReference("Bearer", document)] = []
      });


});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi(); commented because conflix with swagger authorize
    app.UseSwagger();

    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "Task Management Api");

        o.RoutePrefix = "swagger";

        o.UseResponseInterceptor(@"(response) => {

        const url = response.url || '';

        // Detect successful login
        if (
            url.includes('/api/User/login') &&
            response.status >= 200 &&
            response.status < 300
        ) {
            const token = response.body?.accessToken;   
            

            if (token && window.ui) {

                window.ui.authActions.authorize({
                    Bearer: {
                        name: 'Bearer',
                        schema: {
                            type: 'http',
                            scheme: 'bearer',
                            bearerFormat: 'JWT'
                        },
                        value: token
                    }
                });

                console.log('Swagger authenticated!');
                console.log(token);
            }
        }

        // Detect successful logout
        if (
            url.includes('/api/User/logout') &&
            response.status >= 200 &&
            response.status < 300
        ) {
            if (window.ui) {
                window.ui.authActions.logout(['Bearer']);

                console.log('Swagger logged out!');
            }
        }

        return response;
    }");

    });


}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

