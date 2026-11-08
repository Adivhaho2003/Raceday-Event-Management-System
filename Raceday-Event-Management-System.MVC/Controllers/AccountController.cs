using Microsoft.AspNetCore.Mvc;
using Raceday_Event_Management_System.MVC.Models.ViewModels;
using Raceday_Event_Management_System.MVC.Services;

namespace Raceday_Event_Management_System.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;

        public AccountController(AuthService authService)
        {
            _authService = authService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _authService.LoginAsync(model);

            if (response != null && response.IsSuccess)
            {
                var userResponse = await _authService.GetCurrentUserAsync();

                if (userResponse != null && userResponse.IsSuccess && userResponse.Data != null)
                {
                    HttpContext.Session.SetInt32("UserId", userResponse.Data.UserId);
                    HttpContext.Session.SetString("Role", userResponse.Data.Role);
                    HttpContext.Session.SetString("FirstName", userResponse.Data.FirstName);
                    HttpContext.Session.SetString("LastName", userResponse.Data.LastName);

                    if (userResponse.Data.Role == "Organiser")
                        return RedirectToAction("Dashboard", "Organiser");

                    return RedirectToAction("Index", "Home");
                }
            }

            // Clean error message
            var errorMessage = response?.ErrorMessage ?? "Invalid login attempt";
            if (errorMessage.StartsWith("{"))
                errorMessage = "Invalid login attempt. Please check your credentials.";

            ModelState.AddModelError(string.Empty, errorMessage);
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _authService.RegisterAsync(model);

            if (response != null && response.IsSuccess)
            {
                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                return RedirectToAction(nameof(Login));
            }

            // Clean error message
            var errorMessage = response?.ErrorMessage ?? "Registration failed";
            if (errorMessage.StartsWith("{"))
                errorMessage = "Registration failed. Please check your details and try again.";

            ModelState.AddModelError(string.Empty, errorMessage);
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}