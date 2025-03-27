using kizwaonlineshop.Server.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using kizwaonlineshop.Server.Data;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.AspNetCore.Cors;
//using kizwaonlineshop.Server.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Supabase;
using Supabase.Storage;

namespace kizwaonlineshop.Server.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductController : ControllerBase
    {
        private readonly kizwacartContext _context;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly Supabase.Client _supabase;
        public ProductController(kizwacartContext context, IConfiguration config, IWebHostEnvironment env)
        {
            _context = context;
            _config = config;
            _env = env;

            // Initialize Supabase client
            _supabase = new Supabase.Client(
                _config["Supabase:Url"],
                _config["Supabase:ApiKey"]
            );
        }

        [HttpGet("getall")]
        public async Task<IActionResult> getallproduct()
        {
            var products = await _context.product.ToListAsync();
            return Ok(products);
        }

        [HttpGet("getTrendProd")]
        public async Task<IActionResult> getTrendProd()
        {
            var products = await _context.product.ToListAsync();
            return Ok(products);
        }

        [HttpPost("addproduct")]
        public async Task<IActionResult> addproduct([FromForm] Products_master product)
        {
            if (product == null)
            {
                return BadRequest("Product data is null.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Return validation errors
            }
            if (product.ImageFile != null)
            {
                //var fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                //var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", fileName);
                //using (var stream = new FileStream(filePath, FileMode.Create))
                //{
                //    await product.ImageFile.CopyToAsync(stream);
                //}
                //product.Image = fileName;

                if (product.ImageFile != null)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                    string imageUrl = "";
                    if (_env.IsDevelopment())
                    {
                        var wwwRootPath = _env.WebRootPath;
                        var filePath = Path.Combine(wwwRootPath, "Images", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await product.ImageFile.CopyToAsync(stream);
                        }
                        imageUrl = $"/Images/{fileName}";
                    }
                    else 
                    {
                        var bucketName = _config["Supabase:Bucket"];
                        var storage = _supabase.Storage.From(bucketName);
                        using (var stream = product.ImageFile.OpenReadStream())
                        using (var memoryStream = new MemoryStream())
                        {
                            await stream.CopyToAsync(memoryStream);
                            byte[] fileBytes = memoryStream.ToArray();
                            var response = await storage.Upload(fileBytes, fileName, new Supabase.Storage.FileOptions
                            {
                                ContentType = product.ImageFile.ContentType
                            });
                            if (response == null)
                            {
                                return BadRequest(new { message = "Image upload failed" });
                            }
                        }
                        imageUrl = _supabase.Storage.From(bucketName).GetPublicUrl(fileName);
                    }
                    product.Image = imageUrl;
                }
            }

            _context.product.Add(product);
            var isSaved = await _context.SaveChangesAsync();
            if (isSaved > 0)
            {
                return Ok(new { message = "Product saved successfully" });
            }
            else
            {
                return BadRequest(new { message = "Product not saved successfully" });
            }
        }

        [HttpPost("addToCart")]
        public async Task<IActionResult> addToCart([FromBody] Product_Cart cart)
        {            
            //var cartprod = new Product_Cart
            //{
            //    userId = cart.userId,
            //    productId = cart.productId,
            //    productName = cart.productName,
            //    category = cart.category,
            //    size = cart.size,
            //    image = cart.image,
            //    addedDate = cart.addedDate
            //};
            _context.prodcart.Add(cart);
            var isSaved = await _context.SaveChangesAsync();
            if (isSaved > 0)
            {
                return Ok(new { message = "Product saved successfully" });
            }
            else
            {
                return BadRequest(new { message = "Product not saved successfully" });
            }
        }

        [HttpGet("getCart/{userId}")]
        public async Task<IActionResult> getcart(string userId)
        {
            var count = await _context.prodcart
                .Where(p => p.userId == userId)
               .CountAsync();
            return Ok(count);
        }

        [HttpGet("productDet/{Id}")]
        public async Task<IActionResult> productDet(int Id)
        {
            var product = await _context.product.FirstOrDefaultAsync(p => p.Id == Id);
            if (product == null)
                return NotFound("Product not found");
            return Ok(product);
        }


        [HttpGet("getCartProcucts/{userId}")]
        public async Task<IActionResult> getCartProcucts(string userId)
        {
            var cartItems = await _context.prodcart
                .Where(p => p.userId == userId)
                .Select(p => p.productId)
                .ToListAsync();

            var products = await _context.product
                .Where(p => cartItems.Contains(p.Id))
                .ToListAsync();

            return Ok(products);
        }

        [HttpDelete("deleteproduct/{Id}")]
        public async Task<IActionResult> deleteproduct(int Id)
        {
            var product = await _context.product.FirstOrDefaultAsync(u => u.Id == Id);
            if (product == null)
            {
                return NotFound(new { Message = "Product not found" });
            }
            _context.product.Remove(product);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Product deleted successfully" });
        }

    }
}
