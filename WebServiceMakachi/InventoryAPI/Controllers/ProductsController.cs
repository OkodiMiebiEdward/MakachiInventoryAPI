using InventoryAPI.Context;
using InventoryAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly InventoryDbContext _inventoryDb;
        private readonly IConfiguration _config;
        private readonly string serverErrorMessage = "Server error, contact administrator";

        public ProductsController(InventoryDbContext inventoryDb,
            IConfiguration config)
        {
            _inventoryDb = inventoryDb;
            _config = config;
        }

        [HttpPost("CreateProduct")]
        public async Task<ActionResult<ResponseModel>> CreateProduct([FromBody] Product product)
        {
            try
            {
                if (product is null)
                    return BadRequest(new ResponseModel 
                    {
                       Status = "Error",
                       Description = "Please provide valid data"
                    });

                await _inventoryDb.AddAsync(product);
                await _inventoryDb.SaveChangesAsync();
                return StatusCode(201, new ResponseModel 
                {
                    Status = "Success",
                    Description = "Record saved successfully"
                });
            }
            catch (Exception)
            {
                throw;
            }
            return Ok();
        }
    }
}
