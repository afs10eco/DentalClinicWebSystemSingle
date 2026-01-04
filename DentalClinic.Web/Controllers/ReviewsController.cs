using DentalClinic.Web.Data;
using DentalClinic.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.Web.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class ReviewsController : Controller
{
    private readonly AppDbContext _db;
    public ReviewsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var list = await _db.Reviews
            .Include(r => r.Appointment)!.ThenInclude(a => a.Patient)
            .Include(r => r.Appointment)!.ThenInclude(a => a.Doctor)
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(list);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var review = await _db.Reviews
            .Include(r => r.Appointment)!.ThenInclude(a => a.Patient)
            .Include(r => r.Appointment)!.ThenInclude(a => a.Doctor)
            .Include(r => r.Appointment)!.ThenInclude(a => a.Treatment)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review is null) return NotFound();
        return View(review);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateAppointmentsAsync();
        return View(new Review());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Review review)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAppointmentsAsync(review.AppointmentId);
            return View(review);
        }

        // Prevent duplicate review per appointment
        var exists = await _db.Reviews.AnyAsync(r => r.AppointmentId == review.AppointmentId);
        if (exists)
        {
            ModelState.AddModelError(nameof(review.AppointmentId), "Această programare are deja un review.");
            await PopulateAppointmentsAsync(review.AppointmentId);
            return View(review);
        }

        review.CreatedAt = DateTime.UtcNow;
        _db.Add(review);
        await _db.SaveChangesAsync();

        // Mark appointment completed automatically
        var appt = await _db.Appointments.FindAsync(review.AppointmentId);
        if (appt is not null && !appt.Completed)
        {
            appt.Completed = true;
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var review = await _db.Reviews.FindAsync(id);
        if (review is null) return NotFound();

        await PopulateAppointmentsAsync(review.AppointmentId);
        return View(review);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Review review)
    {
        if (id != review.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateAppointmentsAsync(review.AppointmentId);
            return View(review);
        }

        try
        {
            _db.Update(review);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Reviews.AnyAsync(r => r.Id == id))
                return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var review = await _db.Reviews
            .Include(r => r.Appointment)!.ThenInclude(a => a.Patient)
            .Include(r => r.Appointment)!.ThenInclude(a => a.Doctor)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review is null) return NotFound();
        return View(review);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var review = await _db.Reviews.FindAsync(id);
        if (review is not null)
        {
            _db.Reviews.Remove(review);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateAppointmentsAsync(int? selectedId = null)
    {
        var appointments = await _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Treatment)
            .AsNoTracking()
            .ToListAsync();

        appointments = appointments
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.Time)
            .ToList();

        var items = appointments.Select(a => new
        {
            a.Id,
            Text = $"{a.Date:yyyy-MM-dd} {a.Time:hh\\:mm} — {a.Patient!.FullName} / {a.Doctor!.FullName} / {a.Treatment!.Name}"
        });

        ViewData["AppointmentId"] = new SelectList(items, "Id", "Text", selectedId);
    }

}
