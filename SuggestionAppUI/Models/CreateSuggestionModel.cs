namespace SuggestionAppUI.Models;

public class CreateSuggestionModel
{
    [Required]
    [MaxLength(100)]
    public string Suggestion { get; set; }

    [Required]
    [MinLength(10)]
    [Display(Name = "Category")]
    public string CategoryId { get; set; }

    [MaxLength(5000)]
    public string Description { get; set; }
}
