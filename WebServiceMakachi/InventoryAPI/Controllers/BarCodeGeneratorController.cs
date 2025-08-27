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

        [HttpGet("GenerateBarcode")]
        public async Task<ActionResult> GenerateBarcode([FromQuery] string barCodeNumber)
        {
            try
            {
                // 1. Retrieve the product by barcode number
                var product = await _dbContext.Stocks
                    .FirstOrDefaultAsync(s => s.BarCodeNumber == barCodeNumber);

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