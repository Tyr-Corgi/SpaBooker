using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SpaBooker.Core.Entities;
using System.Text;

namespace SpaBooker.Web.Pages;

public class ConfirmEmailModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ConfirmEmailModel> _logger;

    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    public ConfirmEmailModel(
        UserManager<ApplicationUser> userManager,
        ILogger<ConfirmEmailModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(string? userId, string? code)
    {
        if (userId == null || code == null)
        {
            IsSuccess = false;
            ErrorMessage = "Invalid confirmation link.";
            _logger.LogWarning("Email confirmation attempted with missing userId or code");
            return Page();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            IsSuccess = false;
            ErrorMessage = "User not found.";
            _logger.LogWarning("Email confirmation attempted for non-existent user {UserId}", userId);
            return Page();
        }

        // Decode the code
        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

        var result = await _userManager.ConfirmEmailAsync(user, code);
        
        if (result.Succeeded)
        {
            IsSuccess = true;
            _logger.LogInformation("Email confirmed successfully for user {UserId}", userId);
        }
        else
        {
            IsSuccess = false;
            ErrorMessage = "Error confirming your email. The confirmation link may have expired.";
            _logger.LogWarning("Email confirmation failed for user {UserId}. Errors: {Errors}", 
                userId, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return Page();
    }
}

