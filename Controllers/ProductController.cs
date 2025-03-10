using kizwaonlineshop.Server.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using kizwaonlineshop.Server.Data;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
//using kizwaonlineshop.Server.Services;

namespace kizwaonlineshop.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly kizwacartContext _context;
        public ProductController(kizwacartContext context)
        {
            _context = context;
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
            //string sizeString = product.Size != null ? string.Join(",", product.Size) : string.Empty;
            //var prod = new Products_master
            //{
            //    ProductName = product.ProductName,
            //    Category = product.Category,
            //    Size = sizeString,
            //    Price = product.Price, 
            //    Stocks = product.Stocks,
            //    Image = product.Image,
            //};
            if (product.ImageFile != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(stream);
                }
                product.Image = fileName;
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
            var cartprod = new Product_Cart
            {
                userId = cart.userId,
                productId = cart.productId,
            };
            _context.prodcart.Add(cartprod);
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
    }
}
