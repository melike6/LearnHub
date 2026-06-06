using LearnHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LearnHub.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            return View(user);
        }

        // ── PROFİL GÜNCELLE ──────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string fullName,
            IFormFile? profilePicture)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                TempData["Error"] = "Ad Soyad boş olamaz.";
                return RedirectToAction(nameof(Index));
            }

            user.FullName = fullName;

            if (profilePicture != null && profilePicture.Length > 0)
            {
                // Sadece resim dosyası kabul et
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(profilePicture.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    TempData["Error"] = "Sadece JPG, PNG veya GIF dosyası yükleyebilirsiniz.";
                    return RedirectToAction(nameof(Index));
                }

                var fileName = Guid.NewGuid() + extension;
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(),
                    "wwwroot", "uploads", "profiles");
                Directory.CreateDirectory(uploadPath);
                var filePath = Path.Combine(uploadPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await profilePicture.CopyToAsync(stream);

                user.ProfilePicture = "/uploads/profiles/" + fileName;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Profil güncellendi.";
            }
            else
            {
                TempData["Error"] = "Güncelleme sırasında hata oluştu.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ── ŞİFRE DEĞİŞTİR ───────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword,
            string newPassword, string confirmPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Yeni şifreler eşleşmiyor.";
                return RedirectToAction(nameof(Index));
            }

            if (newPassword.Length < 8)
            {
                TempData["Error"] = "Şifre en az 8 karakter olmalıdır.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.ChangePasswordAsync(
                user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Şifre başarıyla değiştirildi.";
            }
            else
            {
                TempData["Error"] = result.Errors.FirstOrDefault()?.Description
                    ?? "Şifre değiştirilemedi. Mevcut şifrenizi kontrol edin.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}