using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddAuthentication(options =>
    {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer("myauth", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("the absolutely secret key for authentication")),
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = false
                };
            });

var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseOcelot().Wait();
app.Run();
