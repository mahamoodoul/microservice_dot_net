using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreApp.Data;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using System.Text.Json;
using StoreApp.Models;
using System.IdentityModel.Tokens.Jwt;
using StoreApp.Helper;


public class OrdersController : Controller
{
    private readonly StoreContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _orderApiUrl = "http://localhost:5199"; // Adjust as needed

    public OrdersController(StoreContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(int productId)
    {
        // 1) Check product in the StoreApp DB
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            return NotFound("Product not found.");
        }

        // 2) Get the logged-in user's access token from OIDC session
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        if (string.IsNullOrEmpty(accessToken))
        {
            return Unauthorized("No access token found. Are you logged in?");
        }

        // 3) Prepare data to send to OrderApi
        // We'll just send product name & price to the OrderApi
        // The OrderApi will figure out username/email from the token claims
        var orderData = new
        {
            productName = product.Name,
            productPrice = product.Price
        };


        // 4) Call the OrderApi
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        //OrderApi is running at: http://localhost:5199
        var orderApiUrl = "http://localhost:5199/orders";

        var jsonBody = JsonSerializer.Serialize(orderData);
        var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync(orderApiUrl, content);
        if (!response.IsSuccessStatusCode)
        {
            var statusCode = response.StatusCode; 
            var errorMsg = await response.Content.ReadAsStringAsync();

            // Log it or return it
            return BadRequest($"Error placing order. Status Code: {statusCode}; Body: {errorMsg}");
        }

        // Optionally, parse the returned order from the OrderApi if you want to display info
        // var createdOrderJson = await response.Content.ReadAsStringAsync();
        // var createdOrder = JsonSerializer.Deserialize<OrderApiResponseType>(createdOrderJson);

        // 5) Return a success page
        return RedirectToAction("OrderConfirmation");
    }

    public IActionResult OrderConfirmation()
    {
        return View("success");
    }

    [Authorize] // user must be logged in to view their orders
    [HttpGet]
    public async Task<IActionResult> MyOrders()
    {

        // Get the access token
        bool isAdmin = await KeycloakRoleHelper.HasRealmAdminRole(HttpContext);
        var accessToken = await HttpContext.GetTokenAsync("access_token");

       

        string url;
        Console.WriteLine($"[DEBUG] realm.....: {User.IsInRole("realm-admin")}");
        if (isAdmin)
        {
            url = $"{_orderApiUrl}/orders_all";
        }
        else
        {
            url = $"{_orderApiUrl}/orders";
        }
        // 1) Get user's access token from OIDC session
        // var accessToken = await HttpContext.GetTokenAsync("access_token");
        Console.WriteLine($"[DEBUG] access token: {accessToken}");
        if (string.IsNullOrEmpty(accessToken))
        {
            return Unauthorized("No access token - user not logged in?");
        }

        // 2) Create HttpClient and set Bearer token
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);


        // 3) Call OrderApi GET /orders
        // var response = await client.GetAsync($"{_orderApiUrl}/orders");
        var response = await client.GetAsync($"{url}");
        if (!response.IsSuccessStatusCode)
        {
            var errBody = await response.Content.ReadAsStringAsync();
            var status = response.StatusCode;
            return BadRequest($"Error fetching orders. Status: {status}, Body: {errBody}");
        }

        // 4) Deserialize JSON into a list of your order model
        var json = await response.Content.ReadAsStringAsync();
        var orders = JsonSerializer.Deserialize<List<OrderInfoViewModel>>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        });

        Console.WriteLine($"[DEBUG] order list: {orders}");
        // 5) Pass the orders to the view
        return View(orders);
    }
}

