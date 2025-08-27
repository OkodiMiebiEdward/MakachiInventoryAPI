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

                #region sale Calculation(Previous)
                //var productStocks = _inventoryDbContext.Stocks
                //    .Include(s => s.Product)
                //    .Where(s => s.Product.BarCodeNumber == sales.Barcodenumber)
                //    .ToList();

                //if (productStocks.Count > 0)
                //{
                //    int totalQuantity = productStocks.Sum(s => s.QuantityInStock);
                //    if (totalQuantity >= sales.Quantity)
                //    {
                //        var stockToUpdate = productStocks
                //            .OrderBy(s => s.CreatedAt)
                //            .FirstOrDefault(s => s.QuantityInStock >= sales.Quantity);

                //        if (stockToUpdate != null)
                //        {
                //            //stockToUpdate.QuantityInStock -= sales.Quantity;
                //            _inventoryDbContext.SaveChanges();

                //            sale.StockId = productStocks.First().ProductId;
                //            sale.Quantity = sales.Quantity;
                //            sale.PriceSold = sales.PriceSold;
                //            sale.Discount = sales.Discount;
                //            sale.Barcodenumber = sales.Barcodenumber;

                //            await _inventoryDbContext.AddAsync(sale);
                //            await _inventoryDbContext.SaveChangesAsync();
                //            return StatusCode(201, sale);
                //        }
                //        else
                //        {
                //            return StatusCode(400, new ResponseModel
                //            {
                //                Status = "Failed",
                //                Description = "No individual stock row has enough quantity"
                //            });
                //        }
                //    }
                //    else
                //    {
                //        return StatusCode(400, new ResponseModel
                //        {
                //            Status = "Failed",
                //            Description = "No individual stock row has enough quantity"
                //        });
                //    }
                //}
                //else
                //    return StatusCode(400, new ResponseModel
                //    {
                //        Status = "Failed",
                //        Description = $"No product attached to this barcode {sales.Barcodenumber}"
                //    });
                #endregion

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

