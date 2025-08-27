using InventoryAPI.Context;
using InventoryAPI.Model;
using InventoryAPI.Model.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<ResponseModel>> CreateProduct([FromBody] ProductDTO product)
        {
            try
            {
                if (product is null)
                    return BadRequest(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Provide valid data"
                    });

                if (string.IsNullOrEmpty(product.ProductName))
                    return BadRequest(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Product name is required"
                    });

                if (string.IsNullOrEmpty(product.ProductDescription))
                    return BadRequest(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Product description is required"
                    });

                var category = await _inventoryDb.Categories.FindAsync(product.CategoryId);
                if (category == null)
                {
                    return BadRequest(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Invalid category ID"
                    });
                }

                #region getProduct
                var productToCheck = new Product
                {
                    Id = product.Id,
                    ProductName = product.ProductName,
                    ProductDescription = product.ProductDescription,
                    CategoryId = product.CategoryId,
                    //Category = category,
                    SKU = product.SKU,
                    //BarCodeNumber = product.BarCodeNumber,
                    Variants = product.Variants
                              .Select(x => new Variant()
                              {
                                  Color = x.Color,
                                  Size = x.Size
                              }).ToList()
                };
                #endregion

                var existingProduct = _inventoryDb.Products
                    .FirstOrDefault(c => c.Id == productToCheck.Id);

                if (existingProduct != null)
                {
                    existingProduct.Id = productToCheck.Id;
                    existingProduct.ProductDescription = productToCheck.ProductDescription;
                    existingProduct.SKU = productToCheck.SKU;
                    existingProduct.ProductName = productToCheck.ProductName;
                    existingProduct.CategoryId = productToCheck.CategoryId;
                    existingProduct.Variants = productToCheck.Variants;
                    _inventoryDb.Products.Update(existingProduct);

                    await _inventoryDb.SaveChangesAsync();
                    return Ok(new ResponseModel
                    {
                        Status = "Success",
                        Description = "Product updated successfully"
                    });
                }
                else
                {
                    await _inventoryDb.AddAsync(productToCheck);
                    await _inventoryDb.SaveChangesAsync();
                    return StatusCode(201, new ResponseModel
                    {
                        Status = "Success",
                        Description = "Product created successfully"
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


        [HttpGet("GetProducts")]
        public async Task<ActionResult<List<ProductDTO>>> GetProducts()
        {
            try
            {
                var products = await _inventoryDb.Products
                                .Include(p => p.Variants)
                                .ToListAsync();

                var productDTOs = products.Select(p => new ProductDTO
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    ProductDescription = p.ProductDescription,
                    CategoryId = p.CategoryId,
                    SKU = p.SKU,
                    //BarCodeNumber = p.BarCodeNumber,
                    Variants = p.Variants.Select(v => new VariantDTO
                    {
                        Size = v.Size,
                        Color = v.Color
                    }).ToList()
                }).ToList();

                return Ok(productDTOs);
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

        [HttpGet("GetProduct")]
        public async Task<ActionResult<ProductDTO>> GetProduct([FromQuery] int id)
        {
            try
            {
                ProductDTO productResponse = new();

                var getProduct = await _inventoryDb.Products
                    .Include(p => p.Variants)
                    .FirstOrDefaultAsync(x => x.Id == id);

                productResponse = new ProductDTO
                {
                    Id = getProduct!.Id,
                    ProductName = getProduct!.ProductName,
                    ProductDescription = getProduct.ProductDescription,
                    CategoryId = getProduct.CategoryId,
                    SKU = getProduct.SKU,
                    //BarCodeNumber = getProduct.BarCodeNumber,
                    Variants = getProduct.Variants.Select(v => new VariantDTO
                    {
                        Size = v.Size,
                        Color = v.Color
                    }).ToList()
                };

                if (productResponse is not null)
                    return Ok(productResponse);
                else
                    return NotFound($"Product with id {id} is not found");
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

        [HttpDelete("DeleteProduct")]
        public async Task<ActionResult> DeleteProduct([FromQuery] int? id)
        {
            if (id is null)
                return BadRequest(new ResponseModel
                {
                    Status = "Failed",
                    Description = "Product id is required"
                });

            Product product = new();
            try
            {
                var productToDelete = _inventoryDb.Products
                     .Include(p => p.Variants)
                     .FirstOrDefault(x => x.Id == id);

                if (productToDelete is null)
                    return NotFound(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Product to be deleted is not found"
                    });
                else
                {
                    _inventoryDb.Products.Remove(productToDelete);
                    await _inventoryDb.SaveChangesAsync();
                    return StatusCode(200, new ResponseModel
                    {
                        Status = "Success",
                        Description = "Product has been deleted successfully"
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


        [HttpGet("GetProductByBarCodeNumber")]
        public async Task<ActionResult<StockDTO>> GetProductByBarCodeNumber([FromQuery] string barcodenumber)
        {
            StockDTO stockItem = new();
            try
            {
                var stockdetails = _inventoryDb.Stocks
                    .Include(p => p.Product)
                    .ToList();
                var result = stockdetails.Where(p => p.BarCodeNumber == barcodenumber).FirstOrDefault();

                if (String.IsNullOrWhiteSpace(barcodenumber))
                    return BadRequest(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Barcodenumber is required"
                    });

                var productByBarcodenumber = await _inventoryDb.Stocks
                    .FirstOrDefaultAsync(x => x.BarCodeNumber == barcodenumber);

                if (productByBarcodenumber is null)
                    return BadRequest(new ResponseModel
                    {
                        Status = "Failed",
                        Description = "Product is not found"
                    });
                else
                {
                    stockItem.BarCodeNumber = productByBarcodenumber.BarCodeNumber;
                    stockItem.ProductName = productByBarcodenumber.Product.ProductName;
                    stockItem.Discount = productByBarcodenumber.Discount;
                    stockItem.SellingUnitPrice = productByBarcodenumber.SellingUnitPrice;
                    stockItem.FinalPrice = productByBarcodenumber.FinalPrice;
                    stockItem.StockNumber = productByBarcodenumber.StockNumber;
                }
                return Ok(stockItem);
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
