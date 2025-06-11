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

                await _inventoryDbContext.AddAsync(category);
                await _inventoryDbContext.SaveChangesAsync();
                return StatusCode(201, new ResponseModel
                {
                    Status = "Success",
                    Description = "Category created successfully"
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
        public ActionResult<Category> GetCategory(string categoryName)
        {
            Category category = new();
            try
            {
                if (string.IsNullOrEmpty(categoryName))
                    return BadRequest("Enter required parameter");

                var getCategory = _inventoryDbContext.Categories
                    .FirstOrDefault(x => x.Name == categoryName);

                if (getCategory is not null)
                    return Ok(getCategory);
                else
                    return NotFound($"Category with name {categoryName} is not found");
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
