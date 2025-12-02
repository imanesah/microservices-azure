using Microsoft.AspNetCore.Mvc;
using CartService.Models;
using CartService.Services;

namespace CartService.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly CartManager _cartService;

        public CartController(CartManager cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(string userId)
        {
            var cart = await _cartService.GetCart(userId);
            return Ok(cart);
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> AddItem(string userId, [FromBody] CartItem item)
        {
            await _cartService.AddOrUpdateItem(userId, item);
            return Ok(await _cartService.GetCart(userId));
        }

        // API pour modifier la quantité - SANS DTO
        [HttpPut("{userId}/items/{productId}")]
        public async Task<IActionResult> UpdateItemQuantity(string userId, string productId, [FromQuery] int quantity)
        {
            await _cartService.UpdateItemQuantity(userId, productId, quantity);
            return Ok(await _cartService.GetCart(userId));
        }

        [HttpDelete("{userId}/{productId}")]
        public async Task<IActionResult> RemoveItem(string userId, string productId)
        {
            await _cartService.RemoveItem(userId, productId);
            return Ok(await _cartService.GetCart(userId));
        }

        [HttpDelete("clear/{userId}")]
        public async Task<IActionResult> ClearCart(string userId)
        {
            await _cartService.ClearCart(userId);
            return Ok();
        }
    }
}