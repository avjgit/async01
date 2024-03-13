using Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("/auth", (AuthUser user) =>
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("the absolutely secret key for authentication"));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
    var expirationDate = DateTime.UtcNow.AddHours(2);

    var claims = new[]
    {
                new Claim(ClaimTypes.Name, user.Username.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

    var token = new JwtSecurityToken(claims: claims, expires: expirationDate, signingCredentials: credentials);
    return new JwtSecurityTokenHandler().WriteToken(token);
});

app.Run();

