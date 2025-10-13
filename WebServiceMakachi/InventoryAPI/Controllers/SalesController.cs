using InventoryAPI.Context;
using InventoryAPI.Model;
using InventoryAPI.Model.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly InventoryDbContext _inventoryDbContext;
        private readonly string serverErrorMessage = "Server error, contact administrator";

        public SalesController(IConfiguration config,
            InventoryDbContext inventoryDbContext)
        {
            _config = config;
            _inventoryDbContext = inventoryDbContext;
        }

        /// <summary>
        /// Creates a new sale record for a product based on the provided sales data.
        /// </summary>
        /// <param name="sales">The sales data for the product.</param>
        /// <returns>
        /// 201 Created with the sale details if successful, 400 Bad Request if input is invalid or 
        /// product does not exist, or 500 Internal Server Error on failure.
        /// </returns>
        [HttpPost("CreateSale")]
        public async Task<ActionResult<SalesDTO>> CreateSales([FromBody] SalesDTO sales)
        {
            Sales sale = new();
            try
            {
                if (sales is null)
                    return BadRequest(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Provide valid data"
                    });
                

                var productStocks = _inventoryDbContext.Stocks
                    .Include(s => s.Product)
                    .Where(s => s.BarCodeNumber == sales.Barcodenumber)
                    .ToList();

                if (productStocks.Count > 0)
                {
                    sale.StockId = productStocks.First().ProductId;
                    sale.Quantity = sales.Quantity;
                    sale.PriceSold = sales.PriceSold;
                    sale.Discount = sales.Discount;
                    sale.Barcodenumber = sales.Barcodenumber;
                    return StatusCode(201, sale);
                }
                else
                {
                    return StatusCode(400, new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Product does not exist"
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

        /// <summary>
        /// Removes a sale record based on the provided sales data.
        /// </summary>
        /// <param name="sales">The sales data to identify the sale to remove.</param>
        /// <returns>
        /// 200 OK if the operation is successful, 400 Bad Request if input is invalid or item not found, or 500 Internal Server Error on failure.
        /// </returns>
        [HttpPost("RemoveSale")]
        public ActionResult RemoveSale([FromBody] SalesDTO sales)
        {
            Sales sale = new();
            try
            {
                if (sales is null)
                    return BadRequest(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Provide valid data"
                    });

                #region sale Calculation
                var productStocks = _inventoryDbContext.Stocks
                    .Include(s => s.Product)
                    .Where(s => s.BarCodeNumber == sales.Barcodenumber).ToList();

                if (productStocks.Count > 0)
                {
                    return StatusCode(200, new ResponseModel
                    {
                        Status = "Success",
                        Description = "Save operation successful"
                    });
                }
                else
                {
                    return StatusCode(400, new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Item could not be found"
                    });
                }
                #endregion
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

        /// <summary>
        /// Processes a checkout operation and generates a receipt for the transaction.
        /// </summary>
        /// <param name="checkout">The checkout data including items and user information.</param>
        /// <returns>
        /// 200 OK with the receipt details if successful, 400 Bad Request if input is invalid, or 500 Internal Server Error on failure.
        /// </returns>
        [HttpPost("Checkout")]
        public async Task<ActionResult> CheckoutReceipt([FromBody] Checkout checkout)
        {
            Checkout receiptDetail = new();
            try
            {
                if (checkout is null)
                {
                    return BadRequest(receiptDetail);
                }
                else
                {
                    if (checkout.SubData.Count > 0)
                    {
                        foreach (var item in checkout.SubData)
                        {
                            var stockItem = _inventoryDbContext.Stocks
                                 .Include(p => p.Product)
                                 .Where(x => x.BarCodeNumber == item.Barcodenumber && x.QuantityInStock > 0)
                                 .FirstOrDefault();

                            var itemFound = stockItem != null ? true : false;
                            if (itemFound)
                            {
                                receiptDetail.Id = checkout.Id;
                                receiptDetail.LoggedInUser = User?.Identity?.Name!;
                                receiptDetail.SubData.Add(new SubData
                                {
                                    Barcodenumber = item.Barcodenumber,
                                    PriceSold = item.PriceSold,
                                    Discount = item.Discount,
                                    Quantity = item.Quantity,
                                    FinalPrice = item.FinalPrice,
                                    ProductName = stockItem?.Product.ProductName!
                                });
                                stockItem!.QuantityInStock -= item.Quantity;
                                await _inventoryDbContext.SaveChangesAsync();
                            }
                        }
                        return StatusCode(200, receiptDetail);
                    }
                    else
                    {
                        return BadRequest(receiptDetail);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

