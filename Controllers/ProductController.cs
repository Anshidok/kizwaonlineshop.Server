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
using kizwaonlineshop.Server.Services;

namespace kizwaonlineshop.Server.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductController : ControllerBase
    {
        private readonly kizwacartContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly CloudinaryService _cloudinaryService;
        public ProductController(kizwacartContext context, IWebHostEnvironment env, CloudinaryService cloudinaryService)
        {
            _context = context;
            _env = env;
            _cloudinaryService = cloudinaryService;
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
            string imageUrl = "";
            try
            {
                if (product.ImageFile != null)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                    if (_env.IsDevelopment())
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await product.ImageFile.CopyToAsync(stream);
                        }
                        imageUrl = fileName;
                        //imageUrl = await _cloudinaryService.UploadImageAsync(product.ImageFile);
                    }
                    else
                    {
                        imageUrl = await _cloudinaryService.UploadImageAsync(product.ImageFile);
                        if (string.IsNullOrEmpty(imageUrl))
                        {
                            return BadRequest(new { message = "Image upload failed" });
                        }
                    }
                }
                product.Image = imageUrl;
                _context.product.Add(product);
                var isSaved = await _context.SaveChangesAsync();

                if (isSaved > 0)
                {
                    return Ok(new { message = "Product saved successfully", product });
                }
                else
                {
                    return BadRequest(new { message = "Product not saved successfully" });
                }            
            }
            catch (Exception ex)
            {
                Console.WriteLine(" Error saving product: " + ex.Message);
                Console.WriteLine(" StackTrace: " + ex.StackTrace);
                return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
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
            if (!string.IsNullOrEmpty(product.Image))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", product.Image);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            _context.product.Remove(product);
            var isDeleted = await _context.SaveChangesAsync();
            if (isDeleted > 0)
            {
                return Ok(new { message = "Product deleted successfully" });
            }
            else
            {
                return BadRequest(new { message = "Failed to delete product" });
            }
        }

    }
}
