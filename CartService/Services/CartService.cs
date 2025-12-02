using StackExchange.Redis;
using CartService.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CartService.Services
{
    public class CartManager
    {
        private readonly IDatabase _redisDb;
        //connexion Redis
        public CartManager(IConnectionMultiplexer redis)
        {
            _redisDb = redis.GetDatabase();
        }
        //generer cle redis par email
        private string GetCartKey(string userId) => $"cart:{userId}";

        private JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<Cart> GetCart(string userId)
        {
            var data = await _redisDb.StringGetAsync(GetCartKey(userId));
            if (data.HasValue)
            {
                try
                {
                    var cart = JsonSerializer.Deserialize<Cart>(data, GetJsonOptions());
                    Console.WriteLine($"=== GET CART {userId} ===");
                    Console.WriteLine($"Redis data: {data}");
                    Console.WriteLine($"Deserialized - Items count: {cart?.Items?.Count}");
                    return cart ?? new Cart { UserId = userId, Items = new List<CartItem>() };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deserializing cart: {ex.Message}");
                    return new Cart { UserId = userId, Items = new List<CartItem>() };
                }
            }
            return new Cart { UserId = userId, Items = new List<CartItem>() };
        }

        public async Task AddOrUpdateItem(string userId, CartItem item)
        {
            Console.WriteLine($"=== ADD/UPDATE ITEM ===");
            Console.WriteLine($"UserId: {userId}, Product: {item.Product?.Id}, Quantity: {item.Quantity}");

            var cart = await GetCart(userId);

            if (cart.Items == null)
                cart.Items = new List<CartItem>();

            var existing = cart.Items.FirstOrDefault(i => i.Product?.Id == item.Product?.Id);
            if (existing != null)
            {
                Console.WriteLine($"Updating existing item: {existing.Product.Id} from {existing.Quantity} to {existing.Quantity + item.Quantity}");
                existing.Quantity += item.Quantity;
            }
            else
            {
                Console.WriteLine($"Adding new item: {item.Product.Id}");
                cart.Items.Add(item);
            }

            var json = JsonSerializer.Serialize(cart, GetJsonOptions());
            Console.WriteLine($"Saving to Redis: {json}");
            await _redisDb.StringSetAsync(GetCartKey(userId), json);
        }

        public async Task UpdateItemQuantity(string userId, string productId, int newQuantity)
        {
            Console.WriteLine($"=== UPDATE QUANTITY ===");
            Console.WriteLine($"UserId: {userId}, ProductId: {productId}, NewQuantity: {newQuantity}");

            var cart = await GetCart(userId);
            Console.WriteLine($"Cart items before: {cart.Items?.Count}");

            if (cart.Items == null || cart.Items.Count == 0)
            {
                Console.WriteLine("No items in cart!");
                return;
            }

            // Debug: afficher tous les items
            foreach (var item in cart.Items)
            {
                Console.WriteLine($"Item: {item.Product?.Id}, Current Qty: {item.Quantity}");
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.Product?.Id == productId);
            if (existingItem != null)
            {
                Console.WriteLine($"Found item! Current quantity: {existingItem.Quantity}, New quantity: {newQuantity}");

                if (newQuantity <= 0)
                {
                    Console.WriteLine("Removing item (quantity <= 0)");
                    cart.Items.RemoveAll(i => i.Product?.Id == productId);
                }
                else
                {
                    existingItem.Quantity = newQuantity;
                    Console.WriteLine($"Quantity updated to: {existingItem.Quantity}");
                }

                var json = JsonSerializer.Serialize(cart, GetJsonOptions());
                Console.WriteLine($"Saving updated cart: {json}");
                await _redisDb.StringSetAsync(GetCartKey(userId), json);
            }
            else
            {
                Console.WriteLine($"Item {productId} not found in cart!");
            }
        }

        public async Task RemoveItem(string userId, string productId)
        {
            var cart = await GetCart(userId);

            if (cart.Items != null)
            {
                cart.Items.RemoveAll(i => i.Product?.Id == productId);
                await _redisDb.StringSetAsync(GetCartKey(userId), JsonSerializer.Serialize(cart, GetJsonOptions()));
            }
        }

        public async Task ClearCart(string userId)
        {
            await _redisDb.KeyDeleteAsync(GetCartKey(userId));
        }
    }
}