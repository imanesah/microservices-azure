namespace Frontend.Models
{
    public class ProductViewModel
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
    }

    public class CartViewModel
    {
        public string UserId { get; set; } = "";
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal Total => Items.Sum(i => i.SubTotal);
    }

    public class CartItemViewModel
    {
        public ProductViewModel Product { get; set; } = new ProductViewModel();
        public int Quantity { get; set; }
        public decimal SubTotal => Product.Price * Quantity;
    }

    public class LoginViewModel
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}