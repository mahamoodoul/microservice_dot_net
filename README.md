# Microservices

A sample microservices-based e-commerce backend and front-end implemented in .NET 8 / ASP.NET Core. This solution demonstrates:

- **7 independent APIs** (Auth, Product, Order, ShoppingCart, Coupon, Reward, Email)  
- **API Gateway** with [Ocelot](https://github.com/ThreeMammals/Ocelot)  
- **Async messaging** via Azure Service Bus (Topics & Queues)  
- **Authentication & Authorization** using ASP.NET Core Identity (JWT + role-based)  
- **N-Layer architecture** with Repository & Unit of Work patterns  
- **Swagger / OpenAPI** documentation for each service  
- **ASP.NET Core MVC** front-end (`Mango.Web`) with Bootstrap 5  
- **Entity Framework Core** + SQL Server for persistence
