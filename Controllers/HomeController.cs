using BloodBridge.Data;
using BloodBridge.Models;
using BloodBridge.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public HomeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
            INotificationService notificationService)
        {
            _db = db;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalDonors = await _db.DonorProfiles.CountAsync();
            ViewBag.TotalRequests = await _db.BloodRequests.CountAsync();
            ViewBag.FulfilledRequests = await _db.BloodRequests.CountAsync(r => r.Status == RequestStatus.Fulfilled);

            var urgentRequests = await _db.BloodRequests
                .Include(r => r.Requester)
                .Where(r => r.IsUrgent && r.Status == RequestStatus.Open)
                .OrderByDescending(r => r.CreatedAt)
                .Take(3)
                .ToListAsync();

            ViewBag.UrgentRequests = urgentRequests;
            return View();
        }

        public IActionResult BloodTypeGuide() => View();

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            if (!User.Identity!.IsAuthenticated)
                return Json(new { count = 0, items = new List<object>() });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { count = 0, items = new List<object>() });

            var notifications = await _notificationService.GetUnreadAsync(user.Id);
            var count = notifications.Count;

            var items = notifications.Select(n => new
            {
                n.Message,
                n.Link,
                CreatedAt = n.CreatedAt.ToString("MMM dd, HH:mm")
            });

            return Json(new { count, items });
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationsRead()
        {
            if (!User.Identity!.IsAuthenticated) return Ok();
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Ok();
            await _notificationService.MarkAllReadAsync(user.Id);
            return Ok();
        }
    }
}
