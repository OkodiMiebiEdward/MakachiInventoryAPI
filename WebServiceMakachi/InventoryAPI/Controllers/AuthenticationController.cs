using InventoryAPI.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ApiSecurity.Controllers;

[Route("api/[controller]")]
[ApiController]

public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly UserManager<User> _userManager;

    public AuthenticationController(IConfiguration config, UserManager<User> userManager)
    {
        _config = config;
        _userManager = userManager;
    }

    public record AuthenticationData(string? UserName, string? Password, string? Email = "", string Phonenumber = "");
    public record UserData(string UserId, string UserName);

    // api/Authentication/token
    [HttpPost("token")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> Authenticate([FromBody] AuthenticationData data)
    {
        var user =await ValidateCredentials(data)!;

        if (user is null)
        {
            return Unauthorized();
        }

        var token = GenerateToken(user);

        return Ok(token);
    }

    private string GenerateToken(UserData user)
    {
        var secretKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(
                _config.GetValue<string>("Authentication:SecretKey")!));

        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims = new();
        claims.Add(new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()));
        claims.Add(new(JwtRegisteredClaimNames.UniqueName, user.UserName));

        var token = new JwtSecurityToken(
            _config.GetValue<string>("Authentication:Issuer"),
            _config.GetValue<string>("Authentication:Audience"),
            claims,
            DateTime.UtcNow, // When this token becomes valid
            DateTime.UtcNow.AddMinutes(600), // When the token will expire
            signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<UserData>? ValidateCredentials(AuthenticationData data)
    {
        var validUser = await _userManager
            .FindByNameAsync(data.UserName!);

        if (validUser is null)
            return null!;
        
        if (CompareValues(data.UserName, validUser.UserName!))  //CompareValues(data.Email, validUser.Email!))
        {
            return new UserData(validUser.Id,validUser.UserName!);
        }
        return null!;
    }

    private bool CompareValues(string? actual, string expected)
    {
        if (actual is not null)
        {
            if (actual.Equals(expected))
            {
                return true;
            }
        }

        return false;
    }
}
