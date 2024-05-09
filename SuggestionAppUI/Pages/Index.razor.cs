namespace SuggestionAppUI.Pages;

public partial class Index
{
    // services
    [Inject] private ICategoryData CategoryData {  get; set; }
    [Inject] private IStatusData StatusData { get; set; }
    [Inject] private ISuggestionData SuggestionData { get; set; }
    [Inject] private IUserData UserData { get; set; }
    [Inject] private NavigationManager Navigate {  get; set; }
    [Inject] private ProtectedSessionStorage SessionStorage { get; set; }
    [Inject] private AuthenticationStateProvider Auth { get; set; }

    // data variables
    private List<SuggestionModel> Suggestions { get; set; } = new();
    private List<CategoryModel> Categories { get; set; } = new();
    private List<StatusModel> Statuses { get; set; } = new();

    // filter variables
    private string SelectedCategory { get; set; } = "All";
    private string SelectedStatus { get; set; } = "All";
    private string SearchText { get; set; } = "";
    private bool IsSortedByNew { get; set; } = true;
    private bool ShowCategories { get; set; } = false;
    private bool ShowStatuses { get; set; } = false;

    // models
    private UserModel LoggedInUser { get; set; }
    private SuggestionModel? ArchivingSuggestion { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync();
    }

    private async Task GetDataAsync()
    {
        Categories = await CategoryData.GetAllCategoriesAsync();
        Statuses = await StatusData.GetAllStatusesAsync();
        Categories = await CategoryData.GetAllCategoriesAsync();
        await LoadAndVerifyUser();
    }

    private async Task LoadAndVerifyUser()
    {
        var authState = await Auth.GetAuthenticationStateAsync();
        string objectId = authState.User.Claims.FirstOrDefault(x => x.Type.Contains("objectidentifier"))?.Value ?? "";

        if(string.IsNullOrEmpty(objectId) == false)
        {
            LoggedInUser = await UserData.GetUserFromAuthenticationAsync(objectId) ?? new();

            string firstName = authState.User.Claims.FirstOrDefault(x => x.Type.Contains("givenname"))?.Value!;
            string lastName = authState.User.Claims.FirstOrDefault(x => x.Type.Contains("surname"))?.Value!;
            string displayName = authState.User.Claims.FirstOrDefault(x => x.Type.Equals("name"))?.Value!;
            string email = authState.User.Claims.FirstOrDefault(x => x.Type.Contains("email"))?.Value!;

            bool isDirty = false;

            if(objectId.Equals(LoggedInUser.ObjectIdentifier) == false)
            {
                isDirty = true;
                LoggedInUser.ObjectIdentifier = objectId;
            }
            if (firstName.Equals(LoggedInUser.FirstName) == false)
            {
                isDirty = true;
                LoggedInUser.FirstName = firstName;
            }
            if (lastName.Equals(LoggedInUser.LastName) == false)
            {
                isDirty = true;
                LoggedInUser.LastName = lastName;
            }
            if (displayName.Equals(LoggedInUser.DisplayName) == false)
            {
                isDirty = true;
                LoggedInUser.DisplayName = displayName;
            }
            if (email.Equals(LoggedInUser.EmailAddress) == false)
            {
                isDirty = true;
                LoggedInUser.EmailAddress = email;
            }

            // update
            if (isDirty)
            {
                if(string.IsNullOrEmpty(LoggedInUser.Id))
                    await UserData.CreateUserAsync(LoggedInUser);
                else
                    await UserData.UpdateUserAsync(LoggedInUser);
            }
        }
    }

    protected override async Task OnAfterRenderAsync(
            bool firstRender)
    {
        if(firstRender == true)
        {
            await LoadFilterState();
            await FilterSuggestions();
            StateHasChanged();
        }
    }

    private async Task FilterSuggestions()
    {
        var output = await SuggestionData.GetAllApprovedSuggestionsAsync();

        if(SelectedCategory != "All")
            output = output.Where(x => x.Category?.CategoryName == SelectedCategory).ToList();
        if (SelectedStatus != "All")
            output = output.Where(x => x.SuggestionStatus?.StatusName == SelectedStatus).ToList();
        if (string.IsNullOrEmpty(SearchText) == false)
            output = output.Where(x =>
                        x.Suggestion.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase) ||
                        x.Description.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase)
                        ).ToList();
        if (IsSortedByNew == true)
            output = output.OrderByDescending(x => x.DateCreated).ToList();
        else
            output = output.OrderByDescending(x => x.UserVotes.Count)
                            .ThenByDescending(x => x.DateCreated)
                            .ToList();

        Suggestions = output;
        await SaveFilterState();
    }

    private async Task SaveFilterState()
    {
        await SessionStorage.SetAsync(nameof(SelectedCategory), SelectedCategory);
        await SessionStorage.SetAsync(nameof(SelectedStatus), SelectedStatus);
        await SessionStorage.SetAsync(nameof(SearchText), SearchText);
        await SessionStorage.SetAsync(nameof(IsSortedByNew), IsSortedByNew);
    }

    private async Task LoadFilterState()
    {
        var stringResults = await SessionStorage.GetAsync<string>(nameof(SelectedCategory));
        SelectedCategory = stringResults.Success == true ? stringResults.Value! : "All";

        stringResults = await SessionStorage.GetAsync<string>(nameof(SelectedStatus));
        SelectedStatus = stringResults.Success == true ? stringResults.Value! : "All";

        stringResults = await SessionStorage.GetAsync<string>(nameof(SearchText));
        SearchText = stringResults.Success == true ? stringResults.Value! : "";

        var sortedResults = await SessionStorage.GetAsync<bool>(nameof(IsSortedByNew));
        IsSortedByNew = stringResults.Success == true ? sortedResults.Value! : true;
    }

    private async Task OrderByNew(
            bool isNew)
    {
        IsSortedByNew = isNew;
        await FilterSuggestions();
    }

    private async Task OnSearchChanged(
            string text)
    {
        SearchText = text;
        await FilterSuggestions();
    }

    private async Task OnCategoryChanged(
            string category = "All")
    {
        SelectedCategory = category;
        ShowCategories = false;
        await FilterSuggestions();
    }

    private async Task OnStatusChanged(
            string status = "All")
    {
        SelectedStatus = status;
        ShowStatuses = false;
        await FilterSuggestions();
    }

    private async Task VoteUp(
            SuggestionModel suggestion)
    {
        if(LoggedInUser is not null)
        {
            if(suggestion.Author.Id == LoggedInUser.Id)
                return;
            if (suggestion.UserVotes.Add(LoggedInUser.Id) == false)
                suggestion.UserVotes.Remove(LoggedInUser.Id);

            await SuggestionData.UpvoteSuggestion(suggestion.Id, LoggedInUser.Id);

            if(IsSortedByNew == false)
                Suggestions = Suggestions.OrderByDescending(x => x.UserVotes.Count())
                                           .ThenByDescending(x => x.DateCreated)
                                           .ToList();
        }
        else
        {
            Navigate.NavigateTo("/MicrosoftIdentity/Account/SignIn", true);
        }
    }

    private string GetUpvoteTopText(
            SuggestionModel suggestion)
    {
        if(suggestion.UserVotes?.Count > 0)
        {
            return suggestion.UserVotes.Count.ToString("00");
        }
        else
        {
            if(suggestion.Author.Id == LoggedInUser?.Id)
                return "Awaiting";
            else
                return "Click To";
        }
    }

    private string GetUpBoteBottomText(
            SuggestionModel suggestion)
    {
        if(suggestion.UserVotes?.Count > 1)
            return "Upvotes";
        else
            return "Upvote";
    }

    private void OpenDetails(
            SuggestionModel suggestion)
    {
        Navigate.NavigateTo($"/details/{suggestion.Id}");
    }

    private void LoadCreatePage()
    {
        if(LoggedInUser is not null)
            Navigate.NavigateTo("create");
        else
            Navigate.NavigateTo("/MicrosoftIdentity/Account/SignIn", true);
    }

    private string SortedByNewClass(
            bool isNew)
    {
        if(isNew == IsSortedByNew)
            return "sort-selected";
        else
            return "";
    }

    private string GetVoteClass(
            SuggestionModel suggestion)
    {
        if (suggestion.Author.Id == LoggedInUser?.Id)
            return "suggestion-entry-no-votes";

        if (suggestion.UserVotes is null || suggestion.UserVotes.Count == 0)
            return "suggestion-entry-not-voted";
        else if (suggestion.UserVotes.Contains(LoggedInUser?.Id))
            return "suggestion-entry-voted";
        else
            return "suggestion-entry-none";
    }

    private string GetSuggestionStatusClass(
            SuggestionModel suggestion)
    {
        if(suggestion is null | suggestion?.SuggestionStatus is null)
            return "suggestion-entry-status-none";

        string output = suggestion?.SuggestionStatus.StatusName switch
        {
            "Completed" => "suggestion-entry-status-completed",
            "Watching" => "suggestion-entry-status-watching",
            "Addressing" => "suggestion-entry-status-addressing",
            "Dismissed" => "suggestion-entry-status-dismissed",
            _ => "suggestion-entry-status-none",
        };

        return output;
    }

    private string GetSelectedCategory(
            string category = "All")
    {
        if(category == SelectedCategory)
            return "selected-category";
        else
            return "";
    }

    private string GetSelectedStatus(
            string status = "All")
    {
        if (status == SelectedStatus)
            return "selected-status";
        else
            return "";
    }

    private async Task ArchiveSuggestion()
    {
        ArchivingSuggestion.Archived = true;
        await SuggestionData.UpdateSuggestion(ArchivingSuggestion);
        Suggestions.Remove(ArchivingSuggestion);
        ArchivingSuggestion = null;
        //await FilterSuggestions();
    }
}