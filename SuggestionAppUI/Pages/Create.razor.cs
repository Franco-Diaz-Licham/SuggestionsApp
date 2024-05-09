namespace SuggestionAppUI.Pages;

public partial class Create
{
    // services
    [Inject] private ICategoryData CategoryData { get; set; }
    [Inject] private ISuggestionData SuggestionData { get; set; }
    [Inject] private IUserData UserData { get; set; }
    [Inject] private NavigationManager Navigate { get; set; }
    [Inject] private AuthenticationStateProvider Auth {  get; set; }

    private CreateSuggestionModel Suggestion { get; set; } = new();
    private List<CategoryModel> Categories {  get; set; } = new();
    private UserModel LoggedInUser { get; set; }

    protected override async Task OnInitializedAsync()
    {
        LoggedInUser = await Auth.GetUserFromAuth(UserData);
        Categories = await CategoryData.GetAllCategoriesAsync();
    }

    private void ClosePage()
    {
        Navigate.NavigateTo("/");
    }

    private async Task CreateSuggestion()
    {
        SuggestionModel s = new()
        {
            Suggestion = Suggestion.Suggestion,
            Description = Suggestion.Description,
            Author = new BasicUserModel(LoggedInUser),
            Category = Categories.Where(x => x.Id == Suggestion.CategoryId).FirstOrDefault(),
        };

        if(s.Category == null)
            Suggestion.CategoryId = "";

        await SuggestionData.CreateSuggestionAsync(s);
        Suggestion = new();
    }
}