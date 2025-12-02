using CartService.Models;

public class CartItem
{
    public Product Product { get; set; } = new Product();   // Le produit complet
    public int Quantity { get; set; }      // Quantité commandée
    public decimal SubTotal => Product.Price * Quantity; // Prix total pour cet item
}