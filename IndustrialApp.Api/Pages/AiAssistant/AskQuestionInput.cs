using System.ComponentModel.DataAnnotations;

namespace IndustrialApp.Api.Pages.AiAssistant;

public sealed class AskQuestionInput
{
    [Required(ErrorMessage = "Please enter a question.")]
    [StringLength(4000, ErrorMessage = "Question must be 4000 characters or fewer.")]
    public string Question { get; set; } = string.Empty;
}
