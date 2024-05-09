namespace SuggestionAppLibrary.DataAccess.Classes;

public class MongoCategoryData : ICategoryData
{
    private IMemoryCache Cache;
    private IMongoCollection<CategoryModel> Categories;
    private const string CacheName = "CategoryData";

    public MongoCategoryData(
        IDbConnection db,
        IMemoryCache cache)
    {
        Cache = cache;
        Categories = db.CategoryCollection;
    }

    /// <summary>
    /// Method which gets all categories and sets the cache for categories.
    /// </summary>
    /// <returns></returns>
    public async Task<List<CategoryModel>> GetAllCategoriesAsync()
    {
        var output = Cache.Get<List<CategoryModel>>(CacheName);

        if (output == null)
        {
            var results = await Categories.FindAsync(_ => true);
            output = results.ToList();

            Cache.Set(CacheName, output, TimeSpan.FromDays(1));
        }

        return output;
    }

    /// <summary>
    /// Method which inserts a category into the database.
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    public Task CreateCategoryAsync(
            CategoryModel category)
    {
        return Categories.InsertOneAsync(category);
    }
}
