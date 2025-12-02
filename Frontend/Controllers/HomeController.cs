using Microsoft.AspNetCore.Mvc;
using Frontend.Models;
using System.Text;
using System.Text.Json;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiGatewayUrl = "http://apigateway:5157";

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            // Utiliser le client nommé "ApiGateway"
            _httpClient = httpClientFactory.CreateClient("ApiGateway");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        // Page d'accueil
        public IActionResult Index()
        {
            return RedirectToAction("Products");
        }

        // Page de connexion
        public IActionResult Login()
        {
            return View();
        }

        // Traitement de la connexion
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    TempData["Error"] = "Veuillez remplir tous les champs";
                    return View();
                }

                var loginData = new { email, password };
                var json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    TempData["Success"] = "Connexion réussie!";

                    // Stocker l'email dans la session pour l'utiliser comme userId
                    HttpContext.Session.SetString("UserEmail", email);

                    return RedirectToAction("Products");
                }
                else
                {
                    TempData["Error"] = "Email ou mot de passe incorrect";
                    return View();
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur de connexion: {ex.Message}";
                return View();
            }
        }

        // Page d'inscription
        public IActionResult Register()
        {
            return View();
        }

        // Traitement de l'inscription
        [HttpPost]
        public async Task<IActionResult> Register(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    TempData["Error"] = "Veuillez remplir tous les champs";
                    return View();
                }

                var registerData = new { email, password };
                var json = JsonSerializer.Serialize(registerData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/register", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Inscription réussie! Vous pouvez maintenant vous connecter.";
                    return RedirectToAction("Login");
                }
                else
                {
                    TempData["Error"] = "Erreur lors de l'inscription";
                    return View();
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur: {ex.Message}";
                return View();
            }
        }

        // Page des produits
        public async Task<IActionResult> Products()
        {
            try
            {
                // Liste de produits fictifs - vous pouvez les récupérer d'une API plus tard
                var products = new List<ProductViewModel>
                {
                    new ProductViewModel { Id = "1", Name = "iPhone 15", Price = 999.99m },
                    new ProductViewModel { Id = "2", Name = "Samsung Galaxy S24", Price = 849.99m },
                    new ProductViewModel { Id = "3", Name = "MacBook Pro", Price = 1999.99m },
                    new ProductViewModel { Id = "4", Name = "AirPods Pro", Price = 249.99m },
                    new ProductViewModel { Id = "5", Name = "iPad Air", Price = 599.99m },
                    new ProductViewModel { Id = "6", Name = "Apple Watch", Price = 399.99m }
                };

                return View(products);
            }
            catch (Exception)
            {
                return View(new List<ProductViewModel>());
            }
        }

        // Ajouter au panier
        [HttpPost]
        public async Task<IActionResult> AddToCart(string productId, string productName, decimal productPrice)
        {
            try
            {
                // Utiliser l'email comme userId, ou un ID par défaut
                var userId = HttpContext.Session.GetString("UserEmail") ?? "default-user";

                var cartItem = new
                {
                    Product = new
                    {
                        Id = productId,
                        Name = productName,
                        Price = productPrice
                    },
                    Quantity = 1
                };

                var json = JsonSerializer.Serialize(cartItem);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/api/cart/{userId}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = $"{productName} ajouté au panier!";
                }
                else
                {
                    TempData["Error"] = "Erreur lors de l'ajout au panier";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Service indisponible: {ex.Message}";
            }

            return RedirectToAction("Products");
        }

        // Voir le panier
        public async Task<IActionResult> Cart()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserEmail") ?? "default-user";
                var response = await _httpClient.GetAsync($"/api/cart/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var cart = JsonSerializer.Deserialize<CartViewModel>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return View(cart);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur: {ex.Message}";
            }

            // Retourner un panier vide en cas d'erreur
            return View(new CartViewModel { UserId = "default-user", Items = new List<CartItemViewModel>() });
        }

        // Mettre à jour la quantité
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(string productId, int quantity)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserEmail") ?? "default-user";

                var response = await _httpClient.PutAsync($"/api/cart/{userId}/items/{productId}?quantity={quantity}", null);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Quantité mise à jour!";
                }
                else
                {
                    TempData["Error"] = "Erreur lors de la mise à jour";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur: {ex.Message}";
            }

            return RedirectToAction("Cart");
        }

        // Supprimer un article du panier
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(string productId)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserEmail") ?? "default-user";

                var response = await _httpClient.DeleteAsync($"/api/cart/{userId}/{productId}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Article supprimé du panier!";
                }
                else
                {
                    TempData["Error"] = "Erreur lors de la suppression";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur: {ex.Message}";
            }

            return RedirectToAction("Cart");
        }

        // Vider le panier
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserEmail") ?? "default-user";

                var response = await _httpClient.DeleteAsync($"/api/cart/clear/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Panier vidé!";
                }
                else
                {
                    TempData["Error"] = "Erreur lors du vidage du panier";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur: {ex.Message}";
            }

            return RedirectToAction("Cart");
        }

        // Déconnexion
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Déconnexion réussie!";
            return RedirectToAction("Products");
        }
    }
}