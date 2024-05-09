namespace SuggestionAppUI.Pages;

public partial class AdminApproval
{
    // services
    [Inject] private NavigationManager Navigate { get; set; }
    [Inject] private ISuggestionData SuggestionData { get; set; }
    [Inject] private IUserData UserData { get; set; }

    // data
    private string EditedTitle { get; set; } = string.Empty;
    private string EditedDescription { get; set; } = string.Empty;
    private string CurrentEditingTitle { get; set; } = string.Empty;
    private string CurrentEditingDescription {  get; set; } = string.Empty;
    private SuggestionModel EditingModel { get; set; } = new();
    private List<SuggestionModel> Submissions { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Submissions = await SuggestionData.GetAllSuggestionsWaitingForApproval();
    }

    private async Task ApprovedSubmission(
            SuggestionModel submission)
    {
        submission.ApprovedForRelease = true;
        Submissions.Remove(submission);
        await SuggestionData.UpdateSuggestion(submission);
    }

    private async Task RejectSubmission(
            SuggestionModel submission)
    {
        submission.Rejected = true;
        Submissions.Remove(submission);
        await SuggestionData.UpdateSuggestion(submission);
    }

    private void EditTitle(
            SuggestionModel model)
    {
        EditingModel = model;
        EditedTitle = model.Suggestion;
        CurrentEditingTitle = model.Id;
        CurrentEditingDescription = "";
    }

    private async Task SaveTitle(
            SuggestionModel model)
    {
        CurrentEditingTitle = string.Empty;
        model.Suggestion = EditedTitle;
        await SuggestionData.UpdateSuggestion(model);
    }

    private void ClosePage()
    {
        Navigate.NavigateTo("/");
    }

    private void EditDescription(
            SuggestionModel model)
    {
        EditingModel = model;
        EditedDescription = model.Description;
        CurrentEditingTitle = string.Empty;
        CurrentEditingDescription = model.Id;
    }

    private async Task SaveDescription(
            SuggestionModel model)
    {
        CurrentEditingDescription = string.Empty;
        model.Description = EditedDescription;
        await SuggestionData.UpdateSuggestion(model);
    }
}