using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GamemasterChecker
{
    public class GamemasterUser
    {
#pragma warning disable CS8618
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public long TeamId { get; set; }
        public long RoundId { get; set; }
        public long SessionId { get; set; }
        public string? Flag { get; set; }
#pragma warning restore CS8618
    }
    public class GamemasterToken
    {
#pragma warning disable CS8618
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Token { get; set; }
        public string Flag { get; set; }
#pragma warning restore CS8618
    }

    public class GamemasterDatabase
    {
        public static string MongoHost=> Environment.GetEnvironmentVariable("MONGO_HOST") ?? "localhost";
        public static string MongoPort=> Environment.GetEnvironmentVariable("MONGO_PORT") ?? "localhost";
        public static string MongoUser=> Environment.GetEnvironmentVariable("MONGO_USER") ?? "";
        public static string MongoPw=> Environment.GetEnvironmentVariable("MONGO_PASSWORD") ?? "";
        public static string MongoConnection => (MongoUser!="")? $"mongodb://{MongoUser}:{MongoPw}@{MongoHost}:{MongoPort} : $"mongodb://{MongoHost}:{MongoPort}";
        private readonly IMongoCollection<GamemasterUser> Users;
        private readonly IMongoCollection<GamemasterToken> Tokens;
        private readonly InsertOneOptions InsertOneOptions = new InsertOneOptions() { BypassDocumentValidation = false };
        private readonly InsertManyOptions InsertManyOptions = new InsertManyOptions() { IsOrdered = false };

        public GamemasterDatabase()
        {
            var mongo = new MongoClient(MongoConnection);
            var db = mongo.GetDatabase("GamemasterDatabase");
            Users = db.GetCollection<GamemasterUser>("Users");
            Users.Indexes.CreateOne(new CreateIndexModel<GamemasterUser>(Builders<GamemasterUser>.IndexKeys
                .Ascending(gu => gu.RoundId)
                .Ascending(gu => gu.TeamId)));
            Users.Indexes.CreateOne(new CreateIndexModel<GamemasterUser>(Builders<GamemasterUser>.IndexKeys
                .Ascending(gu => gu.Flag)));
            Tokens = db.GetCollection<GamemasterToken>("Tokens");
            var tokenNotificationLogBuilder = Builders<GamemasterToken>.IndexKeys;
            Tokens.Indexes.CreateOne(new CreateIndexModel<GamemasterToken>(Builders<GamemasterToken>.IndexKeys
                .Ascending(gt => gt.Flag)));
        }
        public async Task AddTokenUUIDAsync(string flag, string UUID, CancellationToken token)
        {
            var gtoken = new GamemasterToken() { Flag = flag, Token = UUID };
            await Tokens.InsertOneAsync(gtoken, InsertOneOptions, token);
        }
        public async Task<string> GetTokenUUIDAsync(string flag, CancellationToken token)
        {
            var cursor = await Tokens.FindAsync(t => t.Flag == flag);
            List<GamemasterToken> gtoken = await cursor.ToListAsync(token);
            if (gtoken.Count <= 0) return "";
            return gtoken[0].Token;
        }

        public async Task<List<GamemasterUser>> GetUsersAsync(string flag, CancellationToken token)
        {
            var users = await Users.FindAsync(user => user.Flag == flag, cancellationToken: token);
            return await users.ToListAsync(token);
        }
        public async Task<List<GamemasterUser>> GetUsersAsync(long roundId, long teamId, CancellationToken token)
        {
            var users = await Users.FindAsync(user => user.RoundId == roundId && user.TeamId == teamId, cancellationToken: token);
            return await users.ToListAsync(token);
        }
        public async Task AddUserAsync(GamemasterUser user, CancellationToken token)
        {
            await Users.InsertOneAsync(user, InsertOneOptions, token);
        }

        public async Task InsertUsersAsync(List<GamemasterUser>users, CancellationToken token)
        {
            await Users.InsertManyAsync(users,InsertManyOptions, token);
        }
    }
}
