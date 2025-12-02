public class Cart
{
    public string UserId { get; set; } = "";         // L'utilisateur propriétaire du panier
    public List<CartItem> Items { get; set; } = new List<CartItem>();  // Initialiser la liste

    public decimal Total => Items.Sum(i => i.SubTotal); // Prix total du panier
}