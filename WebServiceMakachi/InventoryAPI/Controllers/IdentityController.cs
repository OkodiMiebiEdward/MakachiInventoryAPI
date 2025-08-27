using InventoryAPI.Context;
using InventoryAPI.Model;
using InventoryAPI.Model.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
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
        private readonly ApplicationDbContext _dbContext;
        private readonly SignInManager<User> _signInManager;

        public record AuthenticationRole(string? Name, string? RoleDescription);
        private readonly string serverErrorMessage = "Server error, contact administrator";

        public IdentityController(UserManager<User> userManager,
            RoleManager<RoleTb> roleManager, ApplicationDbContext dbContext, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _signInManager = signInManager;
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
                    #region Test Login
                    //ADMIN
                    //Test123456@

                    //John
                    //new password:Test123456& 
                    //old password:Test123456++

                    //Susan
                    //Test123456%
                    #endregion

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

                //if (loginDetail.Email == "")
                //    return BadRequest("Email is required");

                if (loginDetail.Password == "")
                    return BadRequest("Password is required");

                //var validUser = await _userManager.FindByEmailAsync(loginDetail.Email!);
                var validUser = await _userManager.FindByNameAsync(loginDetail.UserName!);

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

                if ((string.IsNullOrWhiteSpace(authRole.Name)) || (string.IsNullOrWhiteSpace(authRole.RoleDescription)))
                    return BadRequest("Role name and Role description must be provided");


                // Try to find the role by name
                var existingRole = await _roleManager.FindByNameAsync(authRole.Name);

                if (existingRole == null)
                {
                    // Role does not exist, create it
                    var role = new RoleTb
                    {
                        Name = authRole.Name,
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
                            Status = "Failed",
                            Description = "Role creation failed"
                        });
                    }
                }
                else
                {
                    // Role exists, update it
                    existingRole.RoleDescription = authRole.RoleDescription;
                    var result = await _roleManager.UpdateAsync(existingRole);
                    if (result.Succeeded)
                    {
                        return StatusCode(200, new ResponseModel
                        {
                            Status = "Success",
                            Description = "Role updated successfully"
                        });
                    }
                    else
                    {
                        return StatusCode(400, new ResponseModel
                        {
                            Status = "Failed",
                            Description = "Role update failed"
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
                return StatusCode(500, serverErrorMessage);
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
                        Name = role.Name,
                        RoleDescription = role.RoleDescription
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
                    //if (string.IsNullOrWhiteSpace(user.Email))
                    //    return BadRequest("Email is Required");
                    if (string.IsNullOrWhiteSpace(user.Password))
                        return BadRequest("Password is Required");
                    if (string.IsNullOrWhiteSpace(user.UserName))
                        return BadRequest("UserName is Required");

                    var userToDelete = await _userManager.FindByNameAsync(user.UserName);
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
        public async Task<ActionResult<ResponseModel>> AssignRolesToUsers([FromBody] AssignRoleVM assignRole)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(assignRole.UserName))
                    return BadRequest("User name should is required");
                if (string.IsNullOrWhiteSpace(assignRole.Role))
                    return BadRequest("Role should be provided");

                var user = await _userManager.FindByNameAsync(assignRole.UserName);
                if (user is null)
                    return NotFound($"The user with name {assignRole.UserName} cannot be found");
                else
                {
                    var roleExist = await _roleManager.RoleExistsAsync(assignRole.Role);
                    if (roleExist)
                    {
                        var isUserInRole = await _userManager.IsInRoleAsync(user, assignRole.Role);
                        if (isUserInRole)
                            return new ResponseModel
                            {
                                Status = "Role Exist",
                                Description = "User already has this role"
                            };
                        else
                        {
                            var result = await _userManager.AddToRoleAsync(user, assignRole.Role);
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
                                Status = "Success",
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

        [HttpGet("GetSingleUserRole")]
        public async Task<ActionResult<AssignRoleVM>> GetSingleUserAndRole([FromQuery] string user, [FromQuery] string role)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(role))
                    return BadRequest("User and role must be provided");

                var userRole = await _dbContext.Users
                    .Where(u => u.UserName == user)
                    .Join(_dbContext.UserRoles, u => u.Id, ur => ur.UserId,
                    (u, ur) => new { u, ur })
                    .Join(_dbContext.Roles.Where(r => r.Name == role),
                    temp => temp.ur.RoleId, r => r.Id,
                    (temp, r) => new AssignRoleVM
                    {
                        UserName = temp.u.UserName,
                        Role = r.Name
                    })
                    .FirstOrDefaultAsync();

                if (userRole == null)
                    return NotFound("User with the specified role not found");

                return userRole;
            }
            catch (Exception)
            {
                return StatusCode(500, serverErrorMessage);
            }
        }

        [HttpGet("GetUsersAndRoles")]
        public async Task<ActionResult<List<AssignRoleVM>>> GetUsersAndRoles()
        {
            try
            {
                var userRoles = await _dbContext.Users
                .Join(_dbContext.UserRoles, u => u.Id, ur => ur.UserId,
                (u, ur) => new { u, ur })
                .Join(_dbContext.Roles, temp => temp.ur.RoleId,
                r => r.Id, (temp, r) => new AssignRoleVM
                {
                    UserName = temp.u.UserName,
                    Role = r.Name
                })
                .ToListAsync();

                if (userRoles.Count > 0)
                    return userRoles;
                else
                    new List<AssignRoleVM>();
            }
            catch (Exception)
            {
                return new List<AssignRoleVM>();
            }
            return new List<AssignRoleVM>();
        }

        [HttpDelete("DeleteRole")]
        public async Task<ActionResult> DeleteRole([FromQuery] string roleName)
        {
            try
            {
                if (roleName is null)
                    return BadRequest("Invalid data");
                else
                {
                    var getRole = await _roleManager.FindByNameAsync(roleName);

                    if (getRole == null)
                        return NotFound("Role is not found");

                    var result = await _roleManager.DeleteAsync(getRole);
                    if (result.Succeeded)
                    {
                        return StatusCode(200, new ResponseModel
                        {
                            Status = "Deleted",
                            Description = "Role deleted"
                        });
                    }
                    else
                    {
                        return StatusCode(400, new ResponseModel
                        {
                            Status = "Failed",
                            Description = "Role deletion failed"
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

        [HttpDelete("RemoveRoleFromUser")]
        public async Task<ActionResult<ResponseModel>> RemoveRoleFromUser([FromQuery] string userName, [FromQuery] string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(roleName))
                    return BadRequest(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Username and role name must be provided"
                    });

                var roleExist = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    return NotFound(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Role does not exist"
                    });
                }

                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    return NotFound(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "User not found"
                    });
                }

                var isInRole = await _userManager.IsInRoleAsync(user, roleName);
                if (!isInRole)
                {
                    return BadRequest(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "User is not in the specified role"
                    });
                }

                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                    return Ok(new ResponseModel
                    {
                        Status = "Success",
                        Description = "Role removed from user successfully"
                    });
                }
                else
                {
                    return StatusCode(500, new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Failed to remove role from user"
                    });
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

        [HttpGet("GetRoleFromSignedIn")]
        public async Task<SignedInDetail> GetWhoseSignedIn()
        {
            try
            {
                // Get the username of the currently signed-in user
                var userName = User?.Identity?.Name;

                if (string.IsNullOrEmpty(userName))
                    return new SignedInDetail
                    {
                        UserName = "",
                        IsAdmin = false,
                    };

                // Find the user by username
                var user = await _userManager.FindByNameAsync(userName);

                if (user == null)
                    return new SignedInDetail
                    {
                        UserName = "",
                        IsAdmin = false,
                    };

                // Get the roles for the user
                var roles = await _userManager.GetRolesAsync(user);

                // Check if the user has the "admin" or "ADMIN" role
                bool isAdmin = roles.Any(r => r.Equals("admin",
                    StringComparison.OrdinalIgnoreCase));

                return new SignedInDetail
                {
                    UserName = user.UserName!,
                    IsAdmin = isAdmin
                };
            }
            catch (Exception)
            {
                return new SignedInDetail
                {
                    UserName = "",
                    IsAdmin = false,
                };
            }
        }

        [HttpPost("ChangePassword")]
        public async Task<ActionResult<ResponseModel>> ChangePassword([FromBody]PasswordDTO user)
        {
            try
            {
                if (user is null)
                    return BadRequest("Invalid data");

                if (user.Username is null)
                    return BadRequest("Username is required");

                if (user.OldPassword == "")
                    return BadRequest("Password is required");

                if (user.NewPassword == "")
                    return BadRequest("New password must be provided");
                
                var validUser = await _userManager.FindByNameAsync(user.Username.Trim()!);

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
                    var isPasswordValid = await _userManager.
                        CheckPasswordAsync(validUser!, user.OldPassword.Trim()!);

                    if (isPasswordValid)
                    {
                        var changeAction = await _userManager
                             .ChangePasswordAsync(validUser, user.OldPassword.Trim(), user.NewPassword.Trim());

                        ActionResult output = changeAction.Succeeded switch
                        {
                            true => Ok(new ResponseModel
                            {
                                Status = "Success",
                                Description = "Password successfully changed"
                            }),
                            false => BadRequest(new ResponseModel
                            {
                                Status = "Error",
                                Description = string.Join("; ", changeAction.Errors
                                .Select(e => e.Description))
                            })
                        };
                        return output;
                    }
                    else
                        return BadRequest(new ResponseModel
                        {
                            Status = "Error",
                            Description = "Password mismatch, check current password"
                        });
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
    }
}
