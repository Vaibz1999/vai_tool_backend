using BillingTool.Api.Data;
using BillingTool.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingTool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController(BillingDbContext db) : ControllerBase
{
    private readonly BillingDbContext _db = db;

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] int limit = 30)
    {
        limit = Math.Clamp(limit, 1, 100);
        var query = _db.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(c =>
                EF.Functions.Like(c.Name, $"%{term}%") ||
                EF.Functions.Like(c.Gstin, $"%{term}%"));
        }

        var results = await query
            .OrderBy(c => c.Name)
            .Take(limit)
            .Select(c => new Customer(c.Id, c.Name, c.Address, c.Gstin))
            .ToListAsync();

        return Ok(results);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
    {
        var name = request.Name.Trim();
        var address = request.Address.Trim();
        var gstin = request.Gstin.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Customer name is required.");

        var exists = await _db.Customers.AnyAsync(c => c.Name.ToLower() == name.ToLower());
        if (exists)
            return Conflict("Customer with this name already exists.");

        var entity = new CustomerEntity
        {
            Name = name,
            Address = address,
            Gstin = gstin
        };

        _db.Customers.Add(entity);
        await _db.SaveChangesAsync();

        var created = new Customer(entity.Id, entity.Name, entity.Address, entity.Gstin);
        return CreatedAtAction(nameof(Search), new { q = entity.Name, limit = 1 }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerRequest request)
    {
        var name = request.Name.Trim();
        var address = request.Address.Trim();
        var gstin = request.Gstin.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Customer name is required.");

        var entity = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null)
            return NotFound("Customer not found.");

        var duplicateExists = await _db.Customers.AnyAsync(c =>
            c.Id != id && c.Name.ToLower() == name.ToLower());
        if (duplicateExists)
            return Conflict("Another customer with this name already exists.");

        entity.Name = name;
        entity.Address = address;
        entity.Gstin = gstin;
        await _db.SaveChangesAsync();

        var updated = new Customer(entity.Id, entity.Name, entity.Address, entity.Gstin);
        return Ok(updated);
    }
}
