namespace SuggestionAppUI.Pages;

public partial class Details
{
    // parameters
    [Parameter] public string? id { get; set; }

    // services
    [Inject] private ISuggestionData SuggestionData {  get; set; }
    [Inject] private IUserData UserData { get; set; }
    [Inject] private IStatusData StatusData { get; set; }
    [Inject] private NavigationManager Navigate {  get; set; }
    [Inject] private AuthenticationStateProvider Auth { get; set; }

    // data variables
    private SuggestionModel Suggestion { get; set; }
    private UserModel LoggedInUser { get; set; }
    private List<StatusModel> Statuses { get; set; }
    private string SettingStatus { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        Suggestion = await SuggestionData.GetSuggestion(id);
        LoggedInUser = await Auth.GetUserFromAuth(UserData);
        Statuses = await StatusData.GetAllStatusesAsync();
    }

    private async Task CompleteSetStatus()
    {
        switch(SettingStatus)
        {
            case "completed":
                Suggestion.SuggestionStatus = Statuses.Where(x => x.StatusName.ToLower() == SettingStatus.ToLower()).First();
                break;
            case "watching":
                Suggestion.SuggestionStatus = Statuses.Where(x => x.StatusName.ToLower() == SettingStatus.ToLower()).First();
                break;
            case "addressing":
                Suggestion.SuggestionStatus = Statuses.Where(x => x.StatusName.ToLower() == SettingStatus.ToLower()).First();
                break;
            case "dismissed":
                Suggestion.SuggestionStatus = Statuses.Where(x => x.StatusName.ToLower() == SettingStatus.ToLower()).First();
                break;
            default:
                return;
        }

        SettingStatus = string.Empty;

        await SuggestionData.UpdateSuggestion(Suggestion);
    }

    private void ClosePage()
    {
        Navigate.NavigateTo("/");
    }

    private string GetUpvoteTopText()
    {
        if (Suggestion.UserVotes?.Count > 0)
        {
            return Suggestion.UserVotes.Count.ToString("00");
        }
        else
        {
            if (Suggestion.Author.Id == LoggedInUser?.Id)
                return "Awaiting";
            else
                return "Click To";
        }
    }

    private string GetUpBoteBottomText()
    {
        if (Suggestion.UserVotes?.Count > 1)
            return "Upvotes";
        else
            return "Upvote";
    }

    private async Task VoteUp()
    {
        if (LoggedInUser is not null)
        {
            if (Suggestion.Author.Id == LoggedInUser.Id)
                return;
            if (Suggestion.UserVotes.Add(LoggedInUser.Id) == false)
                Suggestion.UserVotes.Remove(LoggedInUser.Id);

            await SuggestionData.UpvoteSuggestion(Suggestion.Id, LoggedInUser.Id);
        }
        else
        {
            Navigate.NavigateTo("/MicrosoftIdentity/Account/SignIn", true);
        }
    }

    private string GetVoteClass()
    {
        if(Suggestion.Author.Id == LoggedInUser?.Id)
            return "suggestion-detail-no-votes";

        if (Suggestion.UserVotes is null || Suggestion.UserVotes.Count == 0)
            return "suggestion-detail-not-voted";
        else if (Suggestion.UserVotes.Contains(LoggedInUser?.Id))
            return "suggestion-detail-voted";
        else
            return "suggestion-detail-none";
    }

    private string GetStatusClass()
    {
        if (Suggestion is null | Suggestion?.SuggestionStatus is null)
            return "suggestion-detail-status-none";

        string output = Suggestion?.SuggestionStatus.StatusName switch
        {
            "Completed" => "suggestion-detail-status-completed",
            "Watching" => "suggestion-detail-status-watching",
            "Addressing" => "suggestion-detail-status-addressing",
            "Dismissed" => "suggestion-detail-status-dismissed",
            _ => "suggestion-detail-detail-none",
        };

        return output;
    }
}