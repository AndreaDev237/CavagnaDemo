using CavagnaDemo.Data;
using CavagnaDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace CavagnaDemo.Services;

public class QuoteService
{
    private readonly CavagnaDbContext _db;

    public QuoteService(CavagnaDbContext db)
    {
        _db = db;
    }

    public async Task<List<Quote>> GetAll()
    {
        return await _db.Quotes
            .Include(q => q.Customer)
            .Include(q => q.Righe)
            .ToListAsync();
    }

    public async Task<Quote?> OttieniPreventivo(int id)
    {
        return await _db.Quotes
            .Include(q => q.Customer)
            .Include(q => q.Righe)
                .ThenInclude(r => r.Product)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<Quote> CreaPreventivo(int customerId, List<QuoteItem> righe)
    {
        var quote = new Quote
        {
            CustomerId = customerId,
            DataCreazione = DateTime.Now,
            DataScadenza = DateTime.Now.AddDays(30),
            NumeroPreventivo = $"PRV-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            Stato = "Bozza",
            Righe = righe
        };

        // Calcola subtotale per riga
        foreach (var r in righe)
        {
            var subtotale = r.PrezzoUnitario * r.Quantita;
            r.Subtotale = subtotale - (subtotale * r.ScontoPercentuale / 100m);
        }

        // Applica eventuale sconto globale e IVA
        var totaleRighe = righe.Sum(r => r.Subtotale);
        var scontoTotalePercentuale = quote.ScontoTotale;

        var totaleConSconto = totaleRighe - (totaleRighe * scontoTotalePercentuale / 100m);
        quote.Totale = totaleConSconto * 1.20m;

        _db.Quotes.Add(quote);
        await _db.SaveChangesAsync();
        return quote;
    }

    public async Task<bool> AggiornaStato(int quoteId, string nuovoStato)
    {
        var quote = await _db.Quotes.FindAsync(quoteId);
        if (quote == null) return false;
        quote.Stato = nuovoStato;
        await _db.SaveChangesAsync();
        return true;
    }

    // Calcolo totale a parte (potrebbe servire)
    public decimal CalculateTotal(Quote quote)
    {
        decimal total = 0;
        foreach (var r in quote.Righe)
        {
            var sub = r.PrezzoUnitario * r.Quantita;
            total += sub - (sub * r.ScontoPercentuale / 100m);
        }
        return total * 1.22m;
    }
}
