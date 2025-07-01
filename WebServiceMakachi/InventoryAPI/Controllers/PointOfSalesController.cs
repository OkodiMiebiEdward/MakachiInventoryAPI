using InventoryAPI.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PointOfSalesController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly InventoryDbContext _inventoryDbContext;

        public PointOfSalesController(IConfiguration config,
            InventoryDbContext inventoryDbContext)
        {
            _config = config;
            _inventoryDbContext = inventoryDbContext;
        }


    }
}
