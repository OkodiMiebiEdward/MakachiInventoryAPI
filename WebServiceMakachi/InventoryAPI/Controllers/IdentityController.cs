using InventoryAPI.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using static ApiSecurity.Controllers.AuthenticationController;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<RoleTb> _roleManager;

        public record AuthenticationRole(string? RoleName, string? RoleDescription);
        private readonly string serverErrorMessage = "Server error, contact administrator";

        public IdentityController(UserManager<User> userManager,
            RoleManager<RoleTb> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("CreateUser")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseModel>> CreateUser([FromBody] AuthenticationData data)
        {
            try
            {
                if (data is null)
                    return BadRequest("Invalid data");

                var user = await _userManager.CreateAsync(new User
                {
                    //ADMIN
                    //Test123456@
                    UserName = data.UserName,
                    Email = data.Email,
                    EmailConfirmed = false,
                    PhoneNumber = data.Phonenumber,
                    PhoneNumberConfirmed = false,
                },
                data.Password!);

                if (!user.Succeeded)
                {
                    return StatusCode(400, new ResponseModel
                    {
                        Status = "Failed",
                        Description = "User creation failed"
                    });
                }
                else
                    return StatusCode(201, new ResponseModel 
                    {
                       Status = "Success",
                       Description = "User successfully created"
                    });
            }
            catch (Exception)
            {
                return StatusCode(500, new ResponseModel
                {
                    Status = "ServerError",
                    Description = serverErrorMessage
                });
            }
        }

        [HttpPost("Login")]
        public async Task<ActionResult<ResponseModel>> UserLogin([FromBody] AuthenticationData? loginDetail)
        {
            try
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
                    return NotFound(new ResponseModel
                    {
                        Status = "Error",
                        Description = "User not found"
                    });
                }
                else
                {
                    var isPasswordValid = await _userManager.CheckPasswordAsync(validUser!, loginDetail.Password!);
                    var output = isPasswordValid switch
                    {
                        true => StatusCode(200, new ResponseModel
                        {
                            Status = "Success",
                            Description = "Login successful"
                        }),

                        false => StatusCode(400, new ResponseModel
                        {
                            Status = "Error",
                            Description = "Login unsuccessful"
                        })
                    };
                    return output;
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new ResponseModel
                {
                    Status = "ServerError",
                    Description = serverErrorMessage
                });
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
            try
            {
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
                        return StatusCode(200, new ResponseModel
                        {
                            Status = "Success",
                            Description = "Role created successfully"
                        });
                    }
                    else
                    {
                        return StatusCode(400, new ResponseModel
                        {
                            Status = "Error",
                            Description = "Role creation failed"
                        });
                    }
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new ResponseModel
                {
                    Status = "ServerError",
                    Description = serverErrorMessage
                });
            }
        }

        [HttpGet("GetRoles")]
        public ActionResult<List<RoleTb>> GetAllRoles()
        {
            try
            {
                var availableRoles = _roleManager.Roles
                    .Select(role => new RoleTb
                    {
                        Name = role.Name,
                        RoleDescription = role.RoleDescription
                    })
                    .ToList();

                if (availableRoles.Count == 0)
                    return NotFound("No roles found");
                else
                    return availableRoles;
            }
            catch (Exception)
            {
                return StatusCode(500, serverErrorMessage   );
            }
        }

        [HttpGet("GetRole")]
        public ActionResult<RoleTb> GetRoleByName([FromQuery] string roleName)
        {
            try
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
            catch (Exception)
            {
                return StatusCode(500, serverErrorMessage);
            }
        }

        [HttpGet("GetAllUsers")]
        public ActionResult<List<User>> GetAllUsers()
        {
            try
            {
                var users = _userManager.Users.ToList();
                if (users.Count == 0)
                    return NotFound("No available users");
                else
                    return users;
            }
            catch (Exception)
            {
                return StatusCode(500, serverErrorMessage);
            }
        }


        [HttpGet("GetUser")]
        public async Task<ActionResult<User>> GetUserById([FromQuery] string email)
        {
            try
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
            catch (Exception)
            {
                return StatusCode(500, serverErrorMessage);
            }
        }

        [HttpPost("DeleteUser")]
        public async Task<ActionResult<User>> DeleteUser([FromBody] AuthenticationData user)
        {
            try
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
            catch (Exception)
            {
                return StatusCode(500, serverErrorMessage);
            }
        }

        [HttpPost("AssignRolesToUsers")]
        public async Task<ActionResult<ResponseModel>> AssignRolesToUsers([FromQuery]string userName, [FromQuery] string role)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName))
                    return BadRequest("User name should is required");
                if (string.IsNullOrWhiteSpace(role))
                    return BadRequest("Role should be provided");

                var user = await _userManager.FindByNameAsync(userName);
                if (user is null)
                    return NotFound($"The user with name {userName} cannot be found");
                else
                {
                    var roleExist = await _roleManager.RoleExistsAsync(role);
                    if (roleExist)
                    {
                        var isUserInRole = await _userManager.IsInRoleAsync(user, role);
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
            catch (Exception)
            {
                return StatusCode(500, serverErrorMessage);
            }
        }
    }
}
