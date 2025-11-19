using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpaBooker.Core.Entities;

namespace SpaBooker.Web.Pages;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _logger.LogInformation("Login attempt for email: {Email}, RememberMe: {RememberMe}", Input.Email, Input.RememberMe);
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState invalid. Errors: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            ErrorMessage = "Please fill in all required fields.";
            return Page();
        }

        // Find user by email
        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            _logger.LogWarning("User not found for email: {Email}", Input.Email);
            ErrorMessage = "Invalid email or password.";
            return Page();
        }

        _logger.LogInformation("Attempting sign-in for user: {UserName} with RememberMe: {RememberMe}", user.UserName, Input.RememberMe);
        
        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("Login successful for user: {UserName}", user.UserName);
            
            // Redirect based on role
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return Redirect("/admin/dashboard");
            }
            else if (await _userManager.IsInRoleAsync(user, "Therapist"))
            {
                return Redirect("/schedule/enhanced");
            }
            else
            {
                return Redirect("/");
            }
        }
        else if (result.IsLockedOut)
        {
            _logger.LogWarning("Account locked out for user: {UserName}", user.UserName);
            ErrorMessage = "Account is locked out. Please try again later.";
        }
        else
        {
            _logger.LogWarning("Login failed for user: {UserName}. Result: RequiresTwoFactor={RequiresTwoFactor}, IsNotAllowed={IsNotAllowed}", 
                user.UserName, result.RequiresTwoFactor, result.IsNotAllowed);
            ErrorMessage = "Invalid email or password.";
        }

        return Page();
    }
}





