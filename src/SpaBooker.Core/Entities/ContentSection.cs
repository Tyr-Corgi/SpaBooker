namespace SpaBooker.Core.Entities;

/// <summary>
/// Represents a dynamic content section on the homepage for storytelling.
/// Admins can create, edit, and reorder these sections to craft the spa's narrative.
/// </summary>
public class ContentSection
{
    public int Id { get; set; }
    
    /// <summary>
    /// Main heading for the section (e.g., "Step Inside", "Experience Tranquility")
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional subtitle or tagline
    /// </summary>
    public string? Subtitle { get; set; }
    
    /// <summary>
    /// Main content text for the section
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Section type: TextImage, FullWidthImage, Gallery, Video, TextOnly
    /// </summary>
    public string SectionType { get; set; } = "TextImage";
    
    /// <summary>
    /// Primary image URL
    /// </summary>
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// Second image URL (for galleries)
    /// </summary>
    public string? ImageUrl2 { get; set; }
    
    /// <summary>
    /// Third image URL (for galleries)
    /// </summary>
    public string? ImageUrl3 { get; set; }
    
    /// <summary>
    /// Video URL (for video sections)
    /// </summary>
    public string? VideoUrl { get; set; }
    
    /// <summary>
    /// Whether video should autoplay (muted)
    /// </summary>
    public bool VideoAutoplay { get; set; } = true;
    
    /// <summary>
    /// Whether video should loop continuously
    /// </summary>
    public bool VideoLoop { get; set; } = true;
    
    /// <summary>
    /// Whether to show video player controls
    /// </summary>
    public bool VideoShowControls { get; set; } = false;
    
    /// <summary>
    /// Optional CTA button text
    /// </summary>
    public string? ButtonText { get; set; }
    
    /// <summary>
    /// Button destination URL
    /// </summary>
    public string? ButtonLink { get; set; }
    
    /// <summary>
    /// Layout type: ImageLeft, ImageRight, ImageCenter, TextOnly
    /// </summary>
    public string LayoutType { get; set; } = "ImageLeft";
    
    /// <summary>
    /// Background color: white, cream, light-gray
    /// </summary>
    public string BackgroundColor { get; set; } = "white";
    
    /// <summary>
    /// Display order on the page (lower numbers appear first)
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Whether the section is active and should be displayed
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}


