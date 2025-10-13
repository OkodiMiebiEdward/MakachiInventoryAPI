using BarcodeStandard;
using InventoryAPI.Context;
using InventoryAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BarCodeGeneratorController : ControllerBase
    {
        private readonly InventoryDbContext _dbContext;
        private readonly string serverErrorMessage = "Server error, contact administrator";

        public BarCodeGeneratorController(InventoryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Generates a barcode image for a product based on its name.
        /// </summary>
        /// <remarks>
        /// This endpoint retrieves the stock information for the specified product name,
        /// generates a Code128 barcode image using the product's barcode number, and returns the image as a PNG file.
        /// </remarks>
        /// <param name="productName">
        /// The name of the product for which to generate the barcode. This should match the <c>ProductName</c> property of a product in the database.
        /// </param>
        /// <returns>
        /// Returns a PNG image file containing the generated barcode if the product is found.
        /// If the product is not found, returns a 404 Not Found response with a JSON body:
        /// <code>
        /// {
        ///   "Status": "NotFound",
        ///   "Description": "Product not found"
        /// }
        /// </code>
        /// If a server error occurs, returns a 500 Internal Server Error response with a JSON body:
        /// <code>
        /// {
        ///   "Status": "ServerError",
        ///   "Description": "Server error, contact administrator"
        /// }
        /// </code>
        /// </returns>
        /// <response code="200">Returns the barcode image as a PNG file.</response>
        /// <response code="404">Product not found.</response>
        /// <response code="500">Server error.</response>
        [HttpGet("GenerateBarcode")]
        public async Task<ActionResult> GenerateBarcode([FromQuery] string productName)
        {
            try
            {
                var product = await _dbContext
                    .Stocks.Include(p => p.Product)
                    .FirstOrDefaultAsync(s => s.Product.ProductName == productName);

                if (product == null)
                {
                    return NotFound(new ResponseModel
                    {
                        Status = "NotFound",
                        Description = "Product not found"
                    });
                }

                // 2. Generate the barcode image
                var barcode = new Barcode
                {
                    IncludeLabel = true,
                    Alignment = AlignmentPositions.Center,
                    LabelFont = new SKFont(SKTypeface.FromFamilyName("Sans Serif"), 10)
                };
                var img = barcode
                .Encode(BarcodeStandard.Type.Code128, product.BarCodeNumber, SkiaSharp.SKColors.Black,
                SkiaSharp.SKColors.White, 300, 100);

                // 3. Convert image to byte array
                using var ms = new MemoryStream();
                var encodedImage = img
                    .Encode(SkiaSharp.SKEncodedImageFormat.Png, 100); // Encode the image to PNG format
                encodedImage.SaveTo(ms); // Save the encoded image to the memory stream
                return File(ms.ToArray(), "image/png");
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