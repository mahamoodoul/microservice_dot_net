using OrderApi.Data;
using OrderApi.Models;

public static class SeedData
{
    public static void Initialize(OrderContext context)
    {
        // Look for any orders.
        if (context.Orders.Any())
        {
            return;   // Database has been seeded
        }

        context.Orders.AddRange(
            new OrderInfo
            {
                Username = "john.doe",
                Email = "john.doe@example.com",
                ProductName = "Sample Product A",
                ProductPrice = 200,
                CreatedAt = DateTime.UtcNow
            },
            new OrderInfo
            {
                Username = "jane.smith",
                Email = "jane.smith@example.com",
                ProductName = "Sample Product B",
                ProductPrice = 190,
                CreatedAt = DateTime.UtcNow
            }
        );

        context.SaveChanges();
    }
}
