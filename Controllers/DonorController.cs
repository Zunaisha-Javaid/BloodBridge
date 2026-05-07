using BloodBridge.Data;
using BloodBridge.Models;
using BloodBridge.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.Controllers
{
    [Authorize]
    public class DonorController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public DonorController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Setup donor profile after registration
        [HttpGet]
        public IActionResult Setup() => View(new DonorProfileViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup(DonorProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var existing = await _db.DonorProfiles.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (existing != null)
            {
                TempData["Info"] = "Profile already set up.";
                return RedirectToAction("Dashboard");
            }

            _db.DonorProfiles.Add(new DonorProfile
            {
                UserId = user.Id,
                BloodType = model.BloodType,
                LastDonationDate = model.LastDonationDate,
                IsAvailable = model.IsAvailable
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = "Donor profile created!";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var profile = await _db.DonorProfiles
                .Include(d => d.DonationHistories)
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (profile == null)
                return RedirectToAction("Setup");

            ViewBag.User = user;
            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAvailability()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var profile = await _db.DonorProfiles.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (profile == null) return NotFound();

            profile.IsAvailable = !profile.IsAvailable;
            await _db.SaveChangesAsync();

            TempData["Success"] = profile.IsAvailable
                ? "You are now marked as available to donate."
                : "You are now marked as unavailable.";

            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var profile = await _db.DonorProfiles
                .Include(d => d.DonationHistories)
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (profile == null) return RedirectToAction("Setup");
            return View(profile.DonationHistories.OrderByDescending(d => d.DonationDate).ToList());
        }

        [HttpGet]
        public IActionResult LogDonation() => View(new DonationHistoryViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogDonation(DonationHistoryViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var profile = await _db.DonorProfiles.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (profile == null) return RedirectToAction("Setup");

            _db.DonationHistories.Add(new DonationHistory
            {
                DonorProfileId = profile.Id,
                DonationDate = model.DonationDate,
                HospitalName = model.HospitalName,
                Units = model.Units,
                Notes = model.Notes
            });

            profile.LastDonationDate = model.DonationDate;
            profile.TotalDonations++;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Donation logged successfully!";
            return RedirectToAction("History");
        }

        // Search donors (public-ish)
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Search(DonorSearchViewModel model)
        {
            if (!string.IsNullOrEmpty(model.BloodType) || !string.IsNullOrEmpty(model.City))
            {
                var query = _db.DonorProfiles
                    .Include(d => d.User)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(model.BloodType))
                    query = query.Where(d => d.BloodType == model.BloodType);

                if (!string.IsNullOrEmpty(model.City))
                    query = query.Where(d => d.User.City.ToLower().Contains(model.City.ToLower()));

                if (model.OnlyAvailable)
                    query = query.Where(d => d.IsAvailable);

                model.Results = await query.Select(d => new DonorResultViewModel
                {
                    FullName = d.User.FullName,
                    City = d.User.City,
                    BloodType = d.BloodType,
                    Phone = d.User.Phone,
                    Email = d.User.Email ?? "",
                    IsAvailable = d.IsAvailable,
                    TotalDonations = d.TotalDonations,
                    IsEligible = d.LastDonationDate == null ||
                                 (DateTime.UtcNow - d.LastDonationDate.Value).TotalDays >= 56
                }).ToListAsync();
            }

            return View(model);
        }
    }
}
