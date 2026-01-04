using DentalClinic.Web.Data;
using DentalClinic.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.Web.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class AppointmentsController : Controller
{
    private readonly AppDbContext _db;

    public AppointmentsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var list = await _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Treatment)
            .AsNoTracking()
            .ToListAsync();

        // sortare în memorie (SQLite nu suportă ORDER BY pe TimeSpan)
        list = list
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.Time)
            .ToList();

        return View(list);
    }


    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var appointment = await _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Treatment)
            .Include(a => a.Review)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment is null) return NotFound();
        return View(appointment);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateSelectListsAsync();
        return View(new Appointment());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Appointment appointment)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync();
            return View(appointment);
        }

        _db.Add(appointment);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment is null) return NotFound();

        await PopulateSelectListsAsync();
        return View(appointment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Appointment appointment)
    {
        if (id != appointment.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync();
            return View(appointment);
        }

        try
        {
            _db.Update(appointment);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Appointments.AnyAsync(a => a.Id == id))
                return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var appointment = await _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Treatment)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment is null) return NotFound();
        return View(appointment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment is not null)
        {
            _db.Appointments.Remove(appointment);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateSelectListsAsync()
    {
        ViewData["PatientId"] = new SelectList(await _db.Patients.AsNoTracking().OrderBy(p => p.FullName).ToListAsync(), "Id", "FullName");
        ViewData["DoctorId"] = new SelectList(await _db.Doctors.AsNoTracking().OrderBy(d => d.FullName).ToListAsync(), "Id", "FullName");
        ViewData["TreatmentId"] = new SelectList(await _db.Treatments.AsNoTracking().OrderBy(t => t.Name).ToListAsync(), "Id", "Name");
    }
}
