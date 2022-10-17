using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AplicationDbContext>();

var app = builder.Build();
var configuration = app.Configuration;
ProductReository.Init(configuration);

// Testando Endpoint
app.MapGet("/", () => "Testando a API 22");
app.MapPost("/", () => new {Name = "Augusto Maia", Age = 36});
app.MapGet("/AddHeader",(HttpResponse response) => {
    response.Headers.Add("teste", "Augusto Maia");
    return new {Name = "Augusto Maia", Age = 36};
    });

// Passando parâmetros pelo Header, 
app.MapGet("/getproductbyheader", (HttpRequest request) => {
    return request.Headers["product-code"].ToString();
});

// consulta através de Query, usando parâmetros, de forma dinâmica.
app.MapGet("/getproduct", ([FromQuery] string dateStart, [FromQuery] string dateEnd) => {
    return dateStart + " - " + dateEnd;
});

///////////////////////////////////////////////////

//Passando parâmetro pelo body
app.MapPost("/products", (Product product) => {
    ProductReository.Add(product);
    return Results.Created($"/products/{product.Code}", product.Code);
});

// por exemplo: api.app.com/user/{code} através da rota.
app.MapGet("/products/{code}", ([FromRoute]string code) => {
    var product = ProductReository.GetBy(code);
    if(product != null)
        return Results.Ok(product);
    return Results.NotFound();
});

// Editando com endpoint do tipo Put
app.MapPut("/products", (Product product) => {
    var productSaved = ProductReository.GetBy(product.Code);
    productSaved.Name = product.Name;
    return Results.Ok;
});

// Deletando com endpoint do tipo Delete
app.MapDelete("/products/{code}", ([FromRoute]string code) => {
    var productSaved = ProductReository.GetBy(code);
    ProductReository.Remove(productSaved);
    return Results.Ok;
});

app.MapGet("/configuration/database", (IConfiguration configuration) => {
    return Results.Ok($"{configuration["database:Connection"]}/{configuration["database:Port"]}");
});

app.Run();

public static class ProductReository {
    public static List<Product> Products { get; set; } = Products = new List<Product>();

    public static void Init(IConfiguration configuration) {
        var products = configuration.GetSection("Products").Get<List<Product>>();
        Products = products;
    }

    public static void Add(Product product){         
        Products.Add(product);
    }

    public static Product GetBy(string code) {
        return Products.FirstOrDefault(p => p.Code == code);
    }

    public static void Remove(Product product) {
        Products.Remove(product);
    }

}

public class Product {
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class AplicationDbContext : DbContext {
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options) 
        => options.UseSqlServer("Server=localhost;Database=Products;User Id=sa;Password=Ugug1979!;MultipleActiveResultSets=True;Encrypt=Yes;TrustServerCertificate=Yes");
}