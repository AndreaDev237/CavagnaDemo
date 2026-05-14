using CavagnaDemo.Data;
using CavagnaDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace CavagnaDemo.Services;

public class ProductService
{
    private readonly CavagnaDbContext _db;

    public ProductService(CavagnaDbContext db)
    {
        _db = db;
    }

    public async Task<List<Product>> GetAllProducts()
    {
        return await _db.Products.Include(p => p.Category).ToListAsync();
    }

    public Product? RetrieveById(int id)
    {
        return _db.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
    }

    public async Task<List<Product>> LoadActiveProducts()
    {
        return await _db.Products.Where(p => p.Attivo).ToListAsync();
    }

    public List<Product> GetProductsForQuote(List<int> productIds)
    {
        var result = new List<Product>();
        foreach (var id in productIds)
        {
            var p = _db.Products.Find(id);
            if (p != null) result.Add(p);
        }
        return result;
    }

    public bool CheckStockForOrder(int productId, int requestedQty)
    {
        var product = _db.Products.Find(productId);
        if (product == null) return false;
        return product.Stock >= requestedQty;
    }

    public async Task<Product> CreateProduct(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task<bool> UpdateProduct(Product product)
    {
        try
        {
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> DeleteProduct(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return false;
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<Product>> SearchByName(string query)
    {
        return await _db.Products
            .Where(p => p.Nome.Contains(query))
            .ToListAsync();
    }
}
