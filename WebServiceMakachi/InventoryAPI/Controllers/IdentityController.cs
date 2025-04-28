using InventoryAPI.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static ApiSecurity.Controllers.AuthenticationController;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;

        public IdentityController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }


        [HttpGet("items")]

        public ActionResult<string> GetItems()
        {
            return Ok("Hello from IdentityController");
        }

        [HttpPost("createUser")]
        public async Task<ActionResult<string>> CreateTestUser([FromBody] AuthenticationData data)
        {
            if (data is null)
            {
                return BadRequest("Invalid data");
            }

            var user = await _userManager.CreateAsync(new User
            {
                //ADMIN
                //Test123456@
                UserName = data.UserName,
                Email = "ADMIN@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "1234567890",
                PhoneNumberConfirmed = true,
            },
            data.Password!);

            if (user is not null)
                return null;
            else
                return Ok("User created successfully");
        }
    }
}
