using CavagnaDemo.Data;
using CavagnaDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace CavagnaDemo.Services;

public class OrderService
{
    private readonly CavagnaDbContext _db;

    public OrderService(CavagnaDbContext db)
    {
        _db = db;
    }

    public async Task<List<Order>> GetAllOrders()
    {
        return await _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .ToListAsync();
    }

    public async Task<Order?> GetById(int id)
    {
        return await _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order> CreateOrder(int customerId, List<OrderItem> items, string? shippingAddress = null)
    {

        var order = new Order
        {
            CustomerId = customerId,
            DataOrdine = DateTime.Now,
            NumeroOrdine = $"ORD-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            Stato = "Confermato",
            Items = items,
            IndirizzoSpedizione = shippingAddress
        };

        decimal total = 0;
        foreach (var item in items)
        {
            item.LineTotal = (item.UnitPrice * item.Quantity) - item.Discount;
            total += item.LineTotal;
        }
        order.Totale = total * 1.22m;

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        return order;
    }

    public async Task<bool> CancelOrder(int orderId)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return false;

        foreach (var item in order.Items)
        {
            var product = await _db.Products.FindAsync(item.ProductId);
            if (product != null)
            {
                product.Stock -= item.Quantity;
            }
        }

        order.Stato = "Annullato";
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkAsShipped(int orderId)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order == null) return false;
        order.Stato = "Spedito";
        order.DataSpedizione = DateTime.Now;
        return await _db.SaveChangesAsync() > 0;
    }

    public List<Order> GetOrdersByCustomer(int customerId)
    {
        return _db.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.Items)
            .ToListAsync()
            .Result;
    }
}
