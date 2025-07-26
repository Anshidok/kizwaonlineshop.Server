using kizwaonlineshop.Server.Data;
using kizwaonlineshop.Server.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kizwaonlineshop.Server.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly kizwacartContext _context;

        public CartController(kizwacartContext context)
        {
            _context = context;
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

        [HttpGet("prodExistCart")]
        public async Task<bool> prodExistCart(string userId , int prodId)
        {
            var products = await _context.prodcart.FirstOrDefaultAsync(p=>p.userId == userId && p.productId == prodId);
            if (products == null)
                return true;
            else
                return false;
        }
    }
}
