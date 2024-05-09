namespace SuggestionAppUI.Shared;

public partial class NotAuthorized
{
    [Inject] private NavigationManager Navigate {  get; set; }
    
    private void ClosePage()
    {
        Navigate.NavigateTo("/");
    }
}