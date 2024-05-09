namespace SuggestionAppLibrary.DataAccess.Classes;

public class DbConnection : IDbConnection
{
    private IConfiguration Config;
    private IMongoDatabase Db;
    private string ConnectionId = "MongoDB";
    public string DbName { get; private set; }

    // set collection names to be able to request data
    public string CategoryCollectionName { get; private set; } = "categories";
    public string StatusConnectionName { get; private set; } = "statuses";
    public string UserCollectionName { get; private set; } = "users";
    public string SuggestionCollectionName { get; private set; } = "suggestions";

    // variables to load data from mongo
    public MongoClient Client { get; private set; }
    public IMongoCollection<CategoryModel> CategoryCollection { get; private set; }
    public IMongoCollection<StatusModel> StatusCollection { get; private set; }
    public IMongoCollection<UserModel> UserCollection { get; set; }
    public IMongoCollection<SuggestionModel> SuggestionCollection { get; set; }

    public DbConnection(IConfiguration config)
    {
        Config = config;

        // configure client
        Client = new MongoClient(Config.GetConnectionString(ConnectionId));
        DbName = Config["DatabaseName"];
        Db = Client.GetDatabase(DbName);

        // get collections
        CategoryCollection = Db.GetCollection<CategoryModel>(CategoryCollectionName);
        StatusCollection = Db.GetCollection<StatusModel>(StatusConnectionName);
        UserCollection = Db.GetCollection<UserModel>(UserCollectionName);
        SuggestionCollection = Db.GetCollection<SuggestionModel>(SuggestionCollectionName);
    }
}
