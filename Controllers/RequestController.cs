using BloodBridge.Data;
using BloodBridge.Models;
using BloodBridge.Services;
using BloodBridge.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.Controllers
{
    public class RequestController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public RequestController(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
            IEmailService emailService, INotificationService notificationService)
        {
            _db = db;
            _userManager = userManager;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        // Public feed
        [HttpGet]
        public async Task<IActionResult> Feed(string? bloodType, string? city, bool urgentOnly = false)
        {
            var query = _db.BloodRequests
                .Include(r => r.Requester)
                .Include(r => r.Responses)
                .Where(r => r.Status == RequestStatus.Open || r.Status == RequestStatus.InProgress)
                .AsQueryable();

            if (!string.IsNullOrEmpty(bloodType))
                query = query.Where(r => r.BloodType == bloodType);

            if (!string.IsNullOrEmpty(city))
                query = query.Where(r => r.City.ToLower().Contains(city.ToLower()));

            if (urgentOnly)
                query = query.Where(r => r.IsUrgent);

            var requests = await query
                .OrderByDescending(r => r.IsUrgent)
                .ThenByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.BloodType = bloodType;
            ViewBag.City = city;
            ViewBag.UrgentOnly = urgentOnly;
            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var request = await _db.BloodRequests
                .Include(r => r.Requester)
                .Include(r => r.Responses)
                    .ThenInclude(rr => rr.DonorProfile)
                        .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();
            return View(request);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create() => View(new BloodRequestViewModel());

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BloodRequestViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var request = new BloodRequest
            {
                RequesterId = user.Id,
                BloodType = model.BloodType,
                Quantity = model.Quantity,
                HospitalName = model.HospitalName,
                City = model.City,
                Deadline = model.Deadline,
                IsUrgent = model.IsUrgent,
                AdditionalInfo = model.AdditionalInfo,
                CreatedAt = DateTime.UtcNow
            };

            _db.BloodRequests.Add(request);
            await _db.SaveChangesAsync();

            // Notify matching donors in the same city
            var compatibleTypes = GetCompatibleDonorTypes(model.BloodType);
            var donors = await _db.DonorProfiles
                .Include(d => d.User)
                .Where(d => d.IsAvailable &&
                            compatibleTypes.Contains(d.BloodType) &&
                            d.User.City.ToLower() == model.City.ToLower())
                .ToListAsync();

            foreach (var donor in donors)
            {
                if (donor.User.Email == null) continue;

                await _notificationService.CreateAsync(
                    donor.UserId,
                    $"New blood request: {model.BloodType} needed at {model.HospitalName}, {model.City}",
                    $"/Request/Details/{request.Id}"
                );

                try
                {
                    await _emailService.SendBloodRequestAlertAsync(
                        donor.User.Email, donor.User.FullName,
                        model.BloodType, model.City, model.HospitalName,
                        model.IsUrgent, request.Id
                    );
                }
                catch { /* Email failure should not block the request */ }
            }

            TempData["Success"] = "Blood request posted successfully. Matching donors have been notified.";
            return RedirectToAction("Details", new { id = request.Id });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var requests = await _db.BloodRequests
                .Include(r => r.Responses)
                    .ThenInclude(rr => rr.DonorProfile)
                        .ThenInclude(d => d.User)
                .Where(r => r.RequesterId == user.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Respond(int requestId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var donorProfile = await _db.DonorProfiles.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (donorProfile == null)
            {
                TempData["Error"] = "You must be a registered donor to respond to requests.";
                return RedirectToAction("Details", new { id = requestId });
            }

            var alreadyResponded = await _db.RequestResponses
                .AnyAsync(rr => rr.BloodRequestId == requestId && rr.DonorProfileId == donorProfile.Id);

            if (alreadyResponded)
            {
                TempData["Info"] = "You have already responded to this request.";
                return RedirectToAction("Details", new { id = requestId });
            }

            var request = await _db.BloodRequests
                .Include(r => r.Requester)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null) return NotFound();

            _db.RequestResponses.Add(new RequestResponse
            {
                BloodRequestId = requestId,
                DonorProfileId = donorProfile.Id,
                ResponseDate = DateTime.UtcNow
            });

            if (request.Status == RequestStatus.Open)
                request.Status = RequestStatus.InProgress;

            await _db.SaveChangesAsync();

            // Notify requester
            await _notificationService.CreateAsync(
                request.RequesterId,
                $"{user.FullName} has responded to your blood request!",
                $"/Request/MyRequests"
            );

            try
            {
                if (request.Requester.Email != null)
                    await _emailService.SendRequestResponseNotificationAsync(
                        request.Requester.Email, request.Requester.FullName, user.FullName, requestId);
            }
            catch { }

            TempData["Success"] = "You have responded to this request. The requester has been notified.";
            return RedirectToAction("Details", new { id = requestId });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Fulfill(int requestId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var request = await _db.BloodRequests.FirstOrDefaultAsync(r => r.Id == requestId && r.RequesterId == user.Id);
            if (request == null) return NotFound();

            request.Status = RequestStatus.Fulfilled;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Request marked as fulfilled. Thank you!";
            return RedirectToAction("MyRequests");
        }

        // Blood type compatibility helper
        private static List<string> GetCompatibleDonorTypes(string recipientType)
        {
            return recipientType switch
            {
                "A+" => new List<string> { "A+", "A-", "O+", "O-" },
                "A-" => new List<string> { "A-", "O-" },
                "B+" => new List<string> { "B+", "B-", "O+", "O-" },
                "B-" => new List<string> { "B-", "O-" },
                "AB+" => new List<string> { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" },
                "AB-" => new List<string> { "A-", "B-", "AB-", "O-" },
                "O+" => new List<string> { "O+", "O-" },
                "O-" => new List<string> { "O-" },
                _ => new List<string> { recipientType }
            };
        }
    }
}
