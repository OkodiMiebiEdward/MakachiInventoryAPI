using InventoryAPI.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static ApiSecurity.Controllers.AuthenticationController;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public record AuthenticationRole(string? RoleName, string? RoleDescription);

        public IdentityController(UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("Items")]
        public ActionResult<string> GetItems()
        {
            return Ok("Hello from IdentityController");
        }

        [HttpPost("CreateUser")]
        public async Task<ActionResult<string>> CreateTestUser([FromBody] AuthenticationData data)
        {
            if (data is null)
                return BadRequest("Invalid data");

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

            if (user is null)
                return null!;
            else
                return Ok("User created successfully");
        }

        [HttpPost("Login")]
        public async Task<ActionResult<ResponseModel>> UserLogin([FromBody] AuthenticationData? loginDetail)
        {
            if (loginDetail is null)
                return BadRequest("Invalid data");

            if (loginDetail.Email == "")
                return BadRequest("Email is required");

            if (loginDetail.Password == "")
                return BadRequest("Password is required");

            var validUser = await _userManager.FindByEmailAsync(loginDetail.Email!);
            if (validUser is null)
            {
                return new ResponseModel
                {
                    Status = "Error",
                    Description = "User not found"
                };
            }
            else
            {
                return new ResponseModel
                {
                    Status = "Success",
                    Description = "User found"
                };
            }
        }

        [HttpPost("CreateRole")]
        public async Task<ActionResult<ResponseModel>> CreateRole([FromBody] AuthenticationRole? authRole)
        {
            /*
               Dummy role
               RoleName = "Admin"
               Description = "Managing administrative activities"
             */

            if (authRole is null)
                return BadRequest("Invalid data");

            if ((string.IsNullOrWhiteSpace(authRole.RoleName)) || (string.IsNullOrWhiteSpace(authRole.RoleDescription)))
                return BadRequest("Role name and Role description must be provided");
            else
            {
                var role = new RoleTb
                {
                    Name = authRole.RoleName,
                    RoleDescription = authRole.RoleDescription
                };
                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    return new ResponseModel
                    {
                        Status = "Success",
                        Description = "Role created successfully"
                    };
                }
                else
                {
                    return new ResponseModel
                    {
                        Status = "Error",
                        Description = "Role creation failed"
                    };
                }
            }
        }

        [HttpGet("GetRoles")]
        public ActionResult<List<RoleTb>> GetAllRoles()
        {
            List<RoleTb> availableRoles = [];
            var roles = _roleManager.Roles.ToList();
            if (roles.Count == 0)
                return NotFound("No roles found");
            else
            {
                roles.ForEach(role =>
                {
                    availableRoles.Add(new RoleTb
                    {
                        Name = role.Name,
                    });
                });
                return availableRoles;
            }
        }

        [HttpGet("GetRole")]
        public ActionResult<RoleTb> GetRoleByName([FromQuery] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest("Rolename is required");

            var role = _roleManager.Roles
                .FirstOrDefault(r => r.Name == roleName);

            if (role is null)
                return BadRequest("Role not found");
            else
            {
                return new RoleTb
                {
                    Name = role.Name
                };
            }
        }

        [HttpGet("GetAllUsers")]
        public ActionResult<List<User>> GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            if (users.Count == 0)
                return NotFound("No available users");
            else
                return users;
        }


        [HttpGet("GetUser")]
        public async Task<ActionResult<User>> GetUserById([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email must be provided");

            var user = await _userManager
                .FindByEmailAsync(email);

            if (user is null)
                return NotFound($"User with the email{email} is not found");
            else
                return user;
        }

        [HttpPost("DeleteUser")]
        public async Task<ActionResult<User>> DeleteUser([FromBody] AuthenticationData user)
        {
            if (user is null) 
                return BadRequest("Provide valid User");
            else
            {
                if (string.IsNullOrWhiteSpace(user.Email))
                    return BadRequest("Email is Required");
                if (string.IsNullOrWhiteSpace(user.Password))
                    return BadRequest("Password is Required");
                if (string.IsNullOrWhiteSpace(user.UserName))
                    return BadRequest("UserName is Required");

                var userToDelete = await _userManager.FindByEmailAsync(user.Email);
                await _userManager.DeleteAsync(userToDelete!);
                return Ok("User deleted successfully");
            }
        }

        [HttpPost("AssignRolesToUsers")]
        public async Task<ActionResult<ResponseModel>> AssignRolesToUsers([FromQuery]string userName, [FromQuery] string role)
        {
            if(string.IsNullOrWhiteSpace(userName))
                return BadRequest("User name should is required");
            if (string.IsNullOrWhiteSpace(role))
                return BadRequest("Role should be provided");

            var user = await _userManager.FindByNameAsync(userName);
            if (user is null)
                return NotFound($"The user with name {userName} cannot be found");
            else
            {
                var roleExist = await _roleManager.RoleExistsAsync(role);
                if(roleExist)
                {
                    var isUserInRole = await  _userManager.IsInRoleAsync(user, role);
                    if (isUserInRole)
                        return new ResponseModel
                        {
                            Status = "Role Exist",
                            Description = "User already has this role"
                        };
                    else
                    {
                        var result = await _userManager.AddToRoleAsync(user, role);
                        if (!result.Succeeded)
                        {
                            return new ResponseModel
                            {
                                Status = "Failed",
                                Description = "Failed to add user to role"
                            };
                        }
                        return new ResponseModel
                        {
                            Status = "success",
                            Description = "Roles successfully added to user"
                        };
                    }
                }
            }
            return new ResponseModel 
            {
                Status = "Error",
                Description = "Action could not be completed"
            };
        }
    }
}
