namespace SuggestionAppLibrary.DataAccess.Classes;

public class MongoStatusData : IStatusData
{
    private readonly IMongoCollection<StatusModel> Statuses;
    private readonly IMemoryCache Cache;
    private readonly string CacheName = "StatusData";

    public MongoStatusData(
        IDbConnection db,
        IMemoryCache cache)
    {
        Cache = cache;
        Statuses = db.StatusCollection;
    }

    /// <summary>
    /// Method which gets all statuses from database and caches the response.
    /// </summary>
    /// <returns></returns>
    public async Task<List<StatusModel>> GetAllStatusesAsync()
    {
        var output = Cache.Get<List<StatusModel>>(CacheName);

        if (output == null)
        {
            var result = await Statuses.FindAsync(_ => true);
            output = result.ToList();

            Cache.Set(CacheName, output, TimeSpan.FromDays(1));
        }

        return output;
    }

    /// <summary>
    /// Method which insert a status into the collection.
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public Task CreateStatusAsync(StatusModel status)
    {
        return Statuses.InsertOneAsync(status);
    }
}
