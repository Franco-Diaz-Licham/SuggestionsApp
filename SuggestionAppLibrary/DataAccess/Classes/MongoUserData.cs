namespace SuggestionAppLibrary.DataAccess.Classes;

public class MongoUserData : IUserData
{
    private readonly IMongoCollection<UserModel> Users;

    public MongoUserData(IDbConnection db)
    {
        Users = db.UserCollection;
    }

    public async Task<List<UserModel>> GetAllUsersAsync()
    {
        var result = await Users.FindAsync(_ => true);

        return result.ToList();
    }

    public async Task<UserModel> GetUserAsync(string id)
    {
        var result = await Users.FindAsync(x => x.Id == id);

        return result.FirstOrDefault();
    }


    /// <summary>
    /// Method which gets a user based on the object id.
    /// </summary>
    /// <param name="objectId">Azure B2D object identifier</param>
    /// <returns>UserModel</returns>
    public async Task<UserModel> GetUserFromAuthenticationAsync(string objectId)
    {
        var result = await Users.FindAsync(x => x.ObjectIdentifier == objectId);

        return result.FirstOrDefault();
    }

    public Task CreateUserAsync(UserModel user)
    {
        return Users.InsertOneAsync(user);
    }

    public Task UpdateUserAsync(UserModel user)
    {
        var filter = Builders<UserModel>.Filter.Eq("Id", user.Id);

        // update if found, otherwise create entry
        return Users.ReplaceOneAsync(filter, user, options: new ReplaceOptions { IsUpsert = true });
    }
}
