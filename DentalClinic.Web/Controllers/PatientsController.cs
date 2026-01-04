using DentalClinic.Web.Data;
using DentalClinic.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.Web.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class PatientsController : Controller
{
    private readonly AppDbContext _db;

    public PatientsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
        => View(await _db.Patients.AsNoTracking().ToListAsync());

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var entity = await _db.Patients.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (entity is null) return NotFound();
        return View(entity);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Patient model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var model = await _db.Patients.FindAsync(id);
        if (model is null) return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Patient model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        try
        {
            _db.Update(model);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Patients.AnyAsync(e => e.Id == id))
                return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var entity = await _db.Patients.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (entity is null) return NotFound();
        return View(entity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _db.Patients.FindAsync(id);
        if (entity is not null)
        {
            _db.Patients.Remove(entity);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
