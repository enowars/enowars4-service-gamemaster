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
#pragma warning restore CS8618
    }

    public class GamemasterDatabase
    {
        public static string MongoDomain=> Environment.GetEnvironmentVariable("DATABASE_DOMAIN") ?? "localhost";
        public static string MongoConnection => "mongodb://" + MongoDomain + ":27017";
        private readonly IMongoCollection<GamemasterUser> Users;
        private readonly InsertOneOptions InsertOneOptions = new InsertOneOptions() { BypassDocumentValidation = false };
        private readonly InsertManyOptions InsertManyOptions = new InsertManyOptions() { IsOrdered = false };

        public GamemasterDatabase()
        {
            var mongo = new MongoClient(MongoConnection);
            var db = mongo.GetDatabase("GamemasterDatabase");
            Users = db.GetCollection<GamemasterUser>("Users");

            var notificationLogBuilder = Builders<GamemasterUser>.IndexKeys;
            var indexModel = new CreateIndexModel<GamemasterUser>(notificationLogBuilder
                .Ascending(gu => gu.RoundId)
                .Ascending(gu => gu.TeamId));
            Users.Indexes.CreateOne(indexModel);
        }

        public async Task<List<GamemasterUser>> GetUsersAsync(long roundId, long teamId, CancellationToken token)
        {
            var users = await Users.FindAsync(user => user.RoundId == roundId && user.TeamId == teamId);
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
