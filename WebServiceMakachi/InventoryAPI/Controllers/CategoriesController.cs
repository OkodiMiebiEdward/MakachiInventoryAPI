using InventoryAPI.Context;
using InventoryAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly InventoryDbContext _inventoryDbContext;
        private readonly string serverErrorMessage = "Server error, contact administrator";

        public CategoriesController(IConfiguration config, InventoryDbContext inventoryDbContext)
        {
            _config = config;
            _inventoryDbContext = inventoryDbContext;
        }

        [HttpPost("CreateCategory")]
        public async Task<ActionResult<ResponseModel>> CreateCategory([FromBody] Category category)
        {
            try
            {
                if (category is null)
                    return BadRequest(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Provide valid data"
                    });

                if (string.IsNullOrEmpty(category.Name))
                    return BadRequest(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Category name is required"
                    });

                if (string.IsNullOrEmpty(category.Description))
                    return BadRequest(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Category description is required"
                    });

                // Check if category exists by Name
                var existingCategory = _inventoryDbContext.Categories
                    .FirstOrDefault(c => c.Name == category.Name);

                if (existingCategory != null)
                {
                    // Update existing category
                    existingCategory.Description = category.Description;
                    existingCategory.IsActive = category.IsActive;
                    _inventoryDbContext.Categories.Update(existingCategory);

                    await _inventoryDbContext.SaveChangesAsync();
                    return Ok(new ResponseModel
                    {
                        Status = "Success",
                        Description = "Category updated successfully"
                    });
                }
                else
                {
                    // Create new category
                    await _inventoryDbContext.AddAsync(category);
                    await _inventoryDbContext.SaveChangesAsync();
                    return StatusCode(201, new ResponseModel
                    {
                        Status = "Success",
                        Description = "Category created successfully"
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

        [HttpGet("GetCategories")]
        public async Task<ActionResult<List<Category>>> GetCategories()
        {
            List<Category> categories = new();
            try
            {
                categories = _inventoryDbContext.Categories.ToList();
                return Ok(categories);
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


        [HttpGet("GetCategory")]
        public ActionResult<Category> GetCategory([FromQuery]int id)
        {
            Category category = new();
            try
            {
                if (id == 0)
                    return BadRequest("Enter required parameter");

                var getCategory = _inventoryDbContext.Categories
                    .FirstOrDefault(x => x.Id == id);

                if (getCategory is not null)
                    return Ok(getCategory);
                else
                    return NotFound($"Category with name {id} is not found");
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

        [HttpDelete("DeleteCategory")]
        public async Task<ActionResult> DeleteCategory([FromQuery] int? id)
        {
            if (id is null)
                return BadRequest(new ResponseModel
                {
                    Status = "Failed",
                    Description = "Category id is required"
                });

            Category category = new();
            try
            {
                var categoryToDelete = _inventoryDbContext.Categories
                    .FirstOrDefault(x => x.Id == id);

                if (categoryToDelete is null)
                    return NotFound(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Category to be deleted is not found"
                    });
                else
                {
                    _inventoryDbContext.Categories.Remove(categoryToDelete);
                    await _inventoryDbContext.SaveChangesAsync();
                    return StatusCode(200, new ResponseModel
                    {
                        Status = "Success",
                        Description = "Category has been deleted successfully"
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
