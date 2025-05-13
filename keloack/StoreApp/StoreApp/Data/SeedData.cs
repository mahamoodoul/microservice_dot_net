
using StoreApp.Data;
using StoreApp.Models; // Assuming your Product model is in this namespace

public static class SeedData
{
    public static void Initialize(StoreContext context)
    {
        // Check if any products exist. If yes, the DB has been seeded.
        if (context.Products.Any())
        {
            return;
        }

        // Seed with some default products
        context.Products.AddRange(
            new Product { Name = "Product 1", Price = 100 },
            new Product { Name = "Product 2", Price = 200 }
            // Add more seed data as needed
        );

        context.SaveChanges();
    }
}