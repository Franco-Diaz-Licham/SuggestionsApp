namespace SuggestionAppLibrary.DataAccess.Classes;

public class MongoSuggestionData : ISuggestionData
{
    private readonly IDbConnection Db;
    private readonly IUserData UserData;
    private readonly IMemoryCache Cache;
    private readonly IMongoCollection<SuggestionModel> Suggestions;
    private const string CacheName = "SuggestionData";

    public MongoSuggestionData(
        IDbConnection db,
        IUserData userData,
        IMemoryCache cache)
    {
        Cache = cache;
        Db = db;
        UserData = userData;
        Suggestions = db.SuggestionCollection;
    }

    public async Task<List<SuggestionModel>> GetAllSuggestionsAsync()
    {
        var output = Cache.Get<List<SuggestionModel>>(CacheName);

        if (output == null)
        {
            var result = await Suggestions.FindAsync(x => x.Archived == false);
            output = result.ToList();
            Cache.Set(CacheName, output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    public async Task<List<SuggestionModel>> GetUsersSuggestionsAsync(string userId)
    {
        var output = Cache.Get<List<SuggestionModel>>(userId);

        if(output is null)
        {
            var results = await Suggestions.FindAsync(x => x.Author.Id == userId);
            output = results.ToList();
            Cache.Set(userId, output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    public async Task<List<SuggestionModel>> GetAllApprovedSuggestionsAsync()
    {
        var output = await GetAllSuggestionsAsync();

        return output.Where(x => x.ApprovedForRelease).ToList();
    }

    public async Task<SuggestionModel> GetSuggestion(string id)
    {
        var result = await Suggestions.FindAsync(x => x.Id == id);

        return result.FirstOrDefault();
    }

    public async Task<List<SuggestionModel>> GetAllSuggestionsWaitingForApproval()
    {
        var output = await GetAllSuggestionsAsync();
        var result = output.Where(x => x.ApprovedForRelease == false && x.Rejected == false).ToList();

        return result;
    }

    public async Task UpdateSuggestion(SuggestionModel suggestion)
    {
        await Suggestions.ReplaceOneAsync(x => x.Id == suggestion.Id, suggestion);
        Cache.Remove(CacheName);
    }

    /// <summary>
    /// Method which updates the suggestion upvote information and related user information.
    /// Handles multiple steps at a time before committing transactions.
    /// </summary>
    /// <param name="suggestionId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task UpvoteSuggestion(string suggestionId, string userId)
    {
        // create session so multiple steps can be completed
        var client = Db.Client;
        using var session = await client.StartSessionAsync();
        session.StartTransaction();

        try
        {
            // find suggestion of interest
            var db = client.GetDatabase(Db.DbName);
            var suggestionsInTraction = db.GetCollection<SuggestionModel>(Db.SuggestionCollectionName);
            var suggestion = (await suggestionsInTraction.FindAsync(x => x.Id == suggestionId)).First();

            // attempt to add user who voted to the suggestion list of users
            bool isUpvote = suggestion.UserVotes.Add(userId);

            if (isUpvote == false)
                suggestion.UserVotes.Remove(userId);

            // complete update suggestion with the new one
            await suggestionsInTraction.ReplaceOneAsync(session, x => x.Id == suggestionId, suggestion);

            // find user details who upvoted
            var usersInTransaction = db.GetCollection<UserModel>(Db.UserCollectionName);
            var user = await UserData.GetUserAsync(userId);

            // add upvoted suggestion to list of voted suggestion of user.
            if (isUpvote == true)
            {
                user.VotedOnSuggestions.Add(new BasicSuggestionModel(suggestion));
            }
            else
            {
                var suggestionToRemove = user.VotedOnSuggestions.Where(x => x.Id == suggestionId).First();
                user.VotedOnSuggestions.Remove(suggestionToRemove);
            }

            await usersInTransaction.ReplaceOneAsync(session, x => x.Id == userId, user);
            await session.CommitTransactionAsync();

            Cache.Remove(CacheName);
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    /// <summary>
    /// Method which creates a suggestion and updates related information for user.
    /// Handles multiple steps at a time before committing transactions.
    /// </summary>
    /// <param name="suggestion"></param>
    /// <returns></returns>
    public async Task CreateSuggestionAsync(SuggestionModel suggestion)
    {
        // create session
        var client = Db.Client;
        using var session = await client.StartSessionAsync();
        session.StartTransaction();

        try
        {
            // insert new suggestion into collection
            var db = client.GetDatabase(Db.DbName);
            var suggestionsInTransaction = db.GetCollection<SuggestionModel>(Db.SuggestionCollectionName);
            await suggestionsInTransaction.InsertOneAsync(suggestion);

            // find user who authored the suggestion
            var usersInTransaction = db.GetCollection<UserModel>(Db.UserCollectionName);
            var user = await UserData.GetUserAsync(suggestion.Author.Id);

            // update authored list of suggestions for user.
            user.AuthoredSuggestions.Add(new BasicSuggestionModel(suggestion));
            await usersInTransaction.ReplaceOneAsync(session, x => x.Id == user.Id, user);

            await session.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }
}
