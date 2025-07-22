using InventoryAPI.Context;
using InventoryAPI.Model;
using InventoryAPI.Model.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly InventoryDbContext _inventoryDbContext;
        private readonly string serverErrorMessage = "Server error, contact administrator";

        public StockController(IConfiguration config,
            InventoryDbContext inventoryDbContext)
        {
            _config = config;
            _inventoryDbContext = inventoryDbContext;
        }

        [HttpPost("CreateStock")]
        public async Task<ActionResult<ResponseModel>> CreateStock([FromBody] StockDTO stock)
        {
            decimal? finalPrice = 0.00m;
            try
            {
                if (stock is null)
                    return BadRequest(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Provide valid data"
                    });


                var category = await _inventoryDbContext.Categories.FindAsync(stock.CategoryId);
                if (category == null)
                {
                    return BadRequest(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Invalid category ID"
                    });
                }

                #region FinalSellingPriceCalculation
                var discountedPrice = (stock.Discount / 100) * stock.SellingUnitPrice;
                finalPrice = (stock.SellingUnitPrice - discountedPrice) * stock.QuantityInStock;
                #endregion

                #region getStock
                var stockToCheck = new Stock
                {
                    Id = stock.Id,
                    ProductId = stock.ProductId,
                    QuantityInStock = stock.QuantityInStock,
                    CategoryId = stock.CategoryId,
                    CostUnitPrice = stock.CostUnitPrice,
                    SellingUnitPrice = stock.SellingUnitPrice,
                    Discount = stock.Discount,
                    CreatedAt = stock.CreatedAt,
                    StockNumber = stock.StockNumber,
                    FinalPrice = finalPrice
                };
                #endregion

                var existingStock = _inventoryDbContext.Stocks
                          .FirstOrDefault(c => c.Id == stockToCheck.Id);

                if (existingStock != null)
                {
                    existingStock.Id = stockToCheck.Id;
                    existingStock.ProductId = stockToCheck.ProductId;
                    existingStock.QuantityInStock = stockToCheck.QuantityInStock;
                    existingStock.CategoryId = stockToCheck.CategoryId;
                    existingStock.CostUnitPrice = stockToCheck.CostUnitPrice;
                    existingStock.SellingUnitPrice = stockToCheck.SellingUnitPrice;
                    existingStock.Discount = stockToCheck.Discount;
                    existingStock.CreatedAt = stockToCheck.CreatedAt;
                    existingStock.StockNumber = stockToCheck.StockNumber;
                    existingStock.FinalPrice = stockToCheck.FinalPrice;
                    _inventoryDbContext.Stocks.Update(existingStock);

                    await _inventoryDbContext.SaveChangesAsync();
                    return Ok(new ResponseModel
                    {
                        Status = "Success",
                        Description = "Stock updated successfully"
                    });
                }
                else
                {
                    await _inventoryDbContext.AddAsync(stockToCheck);
                    await _inventoryDbContext.SaveChangesAsync();
                    return StatusCode(201, new ResponseModel
                    {
                        Status = "Success",
                        Description = "Stock created successfully"
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

        [HttpGet("GetStocks")]
        public async Task<ActionResult<List<StockDTO>>> GetStocks()
        {
            try
            {
                var stocks = await _inventoryDbContext.Stocks
                    .Include(s => s.Product )
                    .Include(s => s.Category)
                    .ToListAsync();


                var stockDTOs = stocks.Select(s => new StockDTO
                {
                    Id = s.Id,
                    ProductId = s.ProductId,   
                    ProductName = s.Product?.ProductName!,
                    QuantityInStock = s.QuantityInStock,
                    CategoryId = s.CategoryId,
                    CostUnitPrice = s.CostUnitPrice,
                    SellingUnitPrice = s.SellingUnitPrice,
                    Discount = s.Discount,
                    CreatedAt = s.CreatedAt,
                    StockNumber = s.StockNumber,
                    FinalPrice = s.FinalPrice

                }).ToList();

                return Ok(stockDTOs);
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

        [HttpGet("GetStock")]
        public async Task<ActionResult<StockDTO>> GetStock([FromQuery] int id)
        {
            try
            {
                StockDTO stockResponse = new();

                if (id == 0)
                    return BadRequest("Enter required parameter");


                var getStock = await _inventoryDbContext.Stocks
                    .Include(s => s.Category)
                    .Include(s => s.Product)
                    .FirstOrDefaultAsync(x => x.Id == id);

                stockResponse = new StockDTO
                {
                    Id = getStock!.Id,
                    QuantityInStock = getStock.QuantityInStock,
                    CostUnitPrice = getStock.CostUnitPrice,
                    SellingUnitPrice = getStock.SellingUnitPrice,
                    Discount = getStock.Discount,
                    CreatedAt = getStock.CreatedAt,
                    CategoryId = getStock.CategoryId,
                    StockNumber = getStock.StockNumber,
                    ProductName = getStock.Product.ProductName,
                    ProductId = getStock.ProductId,
                    FinalPrice = getStock.FinalPrice ?? 0.00m
                };

                if (stockResponse is not null)
                    return Ok(stockResponse);
                else
                    return NotFound($"Stock with Id {id} is not found");
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

        [HttpDelete("DeleteStock")]
        public async Task<ActionResult> DeleteStock([FromQuery] int? id)
        {
            if (id is null)
                return BadRequest(new ResponseModel
                {
                    Status = "Failed",
                    Description = "Stock id is required"
                });
            
            try
            {
                var stockToDelete = _inventoryDbContext.Stocks
                     .Include(p => p.Category)
                     .FirstOrDefault(x => x.Id == id);

                if (stockToDelete is null)
                    return NotFound(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Stock to be deleted is not found"
                    });
                else
                {
                    _inventoryDbContext.Stocks.Remove(stockToDelete);
                    await _inventoryDbContext.SaveChangesAsync();
                    return StatusCode(200, new ResponseModel
                    {
                        Status = "Success",
                        Description = "Stock has been deleted successfully"
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
