
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sanay3y.API.Data;
using Sanay3y.API.Models;
using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.Controllers
{
    [ApiController]
    [Route("api/Admin/specialties")]
    [Authorize(Roles = "Admin")]
    public class AdminSpecialtiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminSpecialtiesController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Admin/specialties
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var specialties = await _context.Specialties
                .AsNoTracking()
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.Name)
                .ToListAsync();

            return Ok(specialties);
        }

        // POST: api/Admin/specialties
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateSpecialtyRequest request)
        {
            var name = request.Name.Trim();
            var nameEn = request.NameEn.Trim();

            var exists = await _context.Specialties.AnyAsync(s =>
                s.Name == name || s.NameEn == nameEn);

            if (exists)
            {
                return Conflict(new
                {
                    message = "A specialty with this name already exists."
                });
            }

            var specialty = new Specialty
            {
                Name = name,
                NameEn = nameEn,
                Description = request.Description?.Trim(),
                IsActive = true,
                SortOrder = request.SortOrder,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Specialties.Add(specialty);
            await _context.SaveChangesAsync();

            return Created(
                $"/api/Public/specialties/{specialty.Id}",
                specialty);
        }

        // PUT: api/Admin/specialties/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] CreateSpecialtyRequest request)
        {
            var specialty = await _context.Specialties
                .FirstOrDefaultAsync(s => s.Id == id);

            if (specialty == null)
            {
                return NotFound(new
                {
                    message = "Specialty not found."
                });
            }

            var name = request.Name.Trim();
            var nameEn = request.NameEn.Trim();

            var duplicate = await _context.Specialties.AnyAsync(s =>
                s.Id != id &&
                (s.Name == name || s.NameEn == nameEn));

            if (duplicate)
            {
                return Conflict(new
                {
                    message = "A specialty with this name already exists."
                });
            }

            specialty.Name = name;
            specialty.NameEn = nameEn;
            specialty.Description = request.Description?.Trim();
            specialty.SortOrder = request.SortOrder;

            await _context.SaveChangesAsync();

            return Ok(specialty);
        }

        // PATCH: api/Admin/specialties/1/status
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            [FromBody] UpdateSpecialtyStatusRequest request)
        {
            var specialty = await _context.Specialties
                .FirstOrDefaultAsync(s => s.Id == id);

            if (specialty == null)
            {
                return NotFound(new
                {
                    message = "Specialty not found."
                });
            }

            specialty.IsActive = request.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                specialty.Id,
                specialty.Name,
                specialty.IsActive
            });
        }

        // DELETE: api/Admin/specialties/1
        // Disable instead of permanently deleting the record.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var specialty = await _context.Specialties
                .FirstOrDefaultAsync(s => s.Id == id);

            if (specialty == null)
            {
                return NotFound(new
                {
                    message = "Specialty not found."
                });
            }

            specialty.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class CreateSpecialtyRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string NameEn { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }
    }

    public class UpdateSpecialtyStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
