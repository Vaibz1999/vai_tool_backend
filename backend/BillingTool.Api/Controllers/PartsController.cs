using BillingTool.Api.Data;
using BillingTool.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingTool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartsController(BillingDbContext db) : ControllerBase
{
    private readonly BillingDbContext _db = db;

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] int limit = 30)
    {
        limit = Math.Clamp(limit, 1, 100);
        var query = _db.AutoParts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(p =>
                EF.Functions.Like(p.Name, $"%{term}%") ||
                EF.Functions.Like(p.HsnSac, $"%{term}%"));
        }

        var results = await query
            .OrderBy(p => p.Name)
            .Take(limit)
            .Select(p => new AutoPart(
                p.Id,
                p.Name,
                p.HsnSac,
                p.Rate,
                p.TaxPercent))
            .ToListAsync();

        return Ok(results);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAutoPartRequest request)
    {
        var name = request.Name.Trim();
        var hsnSac = request.HsnSac.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Part name is required.");
        if (string.IsNullOrWhiteSpace(hsnSac))
            return BadRequest("HSN/SAC is required.");
        if (request.Rate < 0)
            return BadRequest("Rate cannot be negative.");
        if (request.TaxPercent < 0 || request.TaxPercent > 100)
            return BadRequest("Tax percent must be between 0 and 100.");

        var exists = await _db.AutoParts.AnyAsync(p =>
            p.Name.ToLower() == name.ToLower() && p.HsnSac == hsnSac);
        if (exists)
            return Conflict("Part already exists.");

        var entity = new AutoPartEntity
        {
            Name = name,
            HsnSac = hsnSac,
            Rate = request.Rate,
            TaxPercent = request.TaxPercent
        };

        _db.AutoParts.Add(entity);
        await _db.SaveChangesAsync();

        var created = new AutoPart(
            entity.Id,
            entity.Name,
            entity.HsnSac,
            entity.Rate,
            entity.TaxPercent);

        return CreatedAtAction(nameof(Search), new { q = entity.Name, limit = 1 }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAutoPartRequest request)
    {
        var name = request.Name.Trim();
        var hsnSac = request.HsnSac.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Part name is required.");
        if (string.IsNullOrWhiteSpace(hsnSac))
            return BadRequest("HSN/SAC is required.");
        if (request.Rate < 0)
            return BadRequest("Rate cannot be negative.");
        if (request.TaxPercent < 0 || request.TaxPercent > 100)
            return BadRequest("Tax percent must be between 0 and 100.");

        var entity = await _db.AutoParts.FirstOrDefaultAsync(p => p.Id == id);
        if (entity is null)
            return NotFound("Part not found.");

        var duplicateExists = await _db.AutoParts.AnyAsync(p =>
            p.Id != id &&
            p.Name.ToLower() == name.ToLower() &&
            p.HsnSac == hsnSac);
        if (duplicateExists)
            return Conflict("Another part with same name and HSN/SAC already exists.");

        entity.Name = name;
        entity.HsnSac = hsnSac;
        entity.Rate = request.Rate;
        entity.TaxPercent = request.TaxPercent;
        await _db.SaveChangesAsync();

        var updated = new AutoPart(
            entity.Id,
            entity.Name,
            entity.HsnSac,
            entity.Rate,
            entity.TaxPercent);

        return Ok(updated);
    }
}
