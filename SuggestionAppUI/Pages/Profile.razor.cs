namespace SuggestionAppUI.Pages;

public partial class Profile
{
    // services
    [Inject] private ISuggestionData SuggestionData { get; set; }
    [Inject] private IUserData UserData { get; set; }
    [Inject] private NavigationManager Navigate { get; set; }
    [Inject] private AuthenticationStateProvider Auth { get; set; }

    // models
    private UserModel LoggedInUser { get; set; }
    private List<SuggestionModel> Submissions { get; set; }
    private List<SuggestionModel> Approved { get; set; }
    private List<SuggestionModel> Archived { get; set; }
    private List<SuggestionModel> Pending { get; set; }
    private List<SuggestionModel> Rejected { get; set; }

    protected override async Task OnInitializedAsync()
    {
        LoggedInUser = await Auth.GetUserFromAuth(UserData);
        var result = await SuggestionData.GetUsersSuggestionsAsync(LoggedInUser.Id);

        if(LoggedInUser is not null && result is not null)
        {
            Submissions = result.OrderByDescending(x => x.DateCreated).ToList();

            Approved = Submissions.Where(x => x.ApprovedForRelease && 
                                                x.Archived == false & 
                                                x.Rejected == false
                                                ).ToList();
            Archived = Submissions.Where(x => x.Archived && 
                                                x.Rejected == false
                                                ).ToList();
            Pending = Submissions.Where(x => x.ApprovedForRelease == false && 
                                                x.Rejected == false
                                                ).ToList();
            Rejected = Submissions.Where(x => x.Rejected).ToList();
        }
    }

    private void ClosePage()
    {
        Navigate.NavigateTo("/");
    }
}