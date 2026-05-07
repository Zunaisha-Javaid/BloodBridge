using BloodBridge.Data;
using BloodBridge.Models;
using BloodBridge.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var donors = await _db.DonorProfiles.CountAsync();
            var totalRequests = await _db.BloodRequests.CountAsync();
            var openRequests = await _db.BloodRequests.CountAsync(r => r.Status == RequestStatus.Open);
            var fulfilledRequests = await _db.BloodRequests.CountAsync(r => r.Status == RequestStatus.Fulfilled);
            var totalUsers = await _userManager.Users.CountAsync();

            var mostNeeded = await _db.BloodRequests
                .Where(r => r.Status == RequestStatus.Open)
                .GroupBy(r => r.BloodType)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync();

            var model = new AdminDashboardViewModel
            {
                TotalDonors = donors,
                TotalRequests = totalRequests,
                OpenRequests = openRequests,
                FulfilledRequests = fulfilledRequests,
                TotalUsers = totalUsers,
                MostNeededBloodType = mostNeeded ?? "N/A"
            };

            return View(model);
        }

        public async Task<IActionResult> Users(string? search)
        {
            var query = _userManager.Users.AsQueryable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => u.FullName.Contains(search) || (u.Email != null && u.Email.Contains(search)));

            var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
            ViewBag.Search = search;
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLockout(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (await _userManager.IsLockedOutAsync(user))
                await _userManager.SetLockoutEndDateAsync(user, null);
            else
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

            TempData["Success"] = "User status updated.";
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            TempData["Success"] = "User deleted.";
            return RedirectToAction("Users");
        }

        public async Task<IActionResult> Requests(string? status)
        {
            var query = _db.BloodRequests.Include(r => r.Requester).AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<RequestStatus>(status, out var s))
                query = query.Where(r => r.Status == s);

            var requests = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
            ViewBag.Status = status;
            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRequest(int requestId)
        {
            var request = await _db.BloodRequests.FindAsync(requestId);
            if (request == null) return NotFound();

            _db.BloodRequests.Remove(request);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Request removed.";
            return RedirectToAction("Requests");
        }

        public async Task<IActionResult> ExportDonors()
        {
            var donors = await _db.DonorProfiles
                .Include(d => d.User)
                .ToListAsync();

            var csv = "FullName,Email,City,Phone,BloodType,TotalDonations,IsAvailable,LastDonationDate\n";
            foreach (var d in donors)
            {
                csv += $"{d.User.FullName},{d.User.Email},{d.User.City},{d.User.Phone}," +
                       $"{d.BloodType},{d.TotalDonations},{d.IsAvailable}," +
                       $"{d.LastDonationDate?.ToString("yyyy-MM-dd") ?? ""}\n";
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "donors.csv");
        }

        public async Task<IActionResult> ExportRequests()
        {
            var requests = await _db.BloodRequests
                .Include(r => r.Requester)
                .ToListAsync();

            var csv = "Id,RequesterName,BloodType,Quantity,Hospital,City,Status,IsUrgent,Deadline,CreatedAt\n";
            foreach (var r in requests)
            {
                csv += $"{r.Id},{r.Requester.FullName},{r.BloodType},{r.Quantity}," +
                       $"{r.HospitalName},{r.City},{r.Status},{r.IsUrgent}," +
                       $"{r.Deadline:yyyy-MM-dd},{r.CreatedAt:yyyy-MM-dd}\n";
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "requests.csv");
        }
    }
}
