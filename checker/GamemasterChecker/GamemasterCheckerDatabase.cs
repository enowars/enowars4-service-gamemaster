namespace GamemasterChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using EnoCore;
    using GamemasterChecker.DbModels;
    using Microsoft.Extensions.Logging;
    using MongoDB.Driver;
    using MongoDB.Driver.Core;

    public class GamemasterCheckerDatabase
    {
        private readonly IMongoCollection<GamemasterUser> users;
        private readonly IMongoCollection<GamemasterToken> tokens;
        private readonly InsertOneOptions insertOneOptions = new InsertOneOptions() { BypassDocumentValidation = false };
        private readonly InsertManyOptions insertManyOptions = new InsertManyOptions() { IsOrdered = false };
        private readonly ILogger logger;

        public GamemasterCheckerDatabase(ILogger<GamemasterCheckerDatabase> logger)
        {
            this.logger.LogDebug("GamemasterCheckerDatabase()");
            while (true)
            {
                try
                {
                    this.logger = logger;
                    var mongo = new MongoClient(MongoConnection);
                    var db = mongo.GetDatabase("GamemasterDatabase");
                    this.users = db.GetCollection<GamemasterUser>("Users");
                    this.users.Indexes.CreateOne(new CreateIndexModel<GamemasterUser>(Builders<GamemasterUser>.IndexKeys
                        .Ascending(gu => gu.Flag)));
                    this.tokens = db.GetCollection<GamemasterToken>("Tokens");
                    var tokenNotificationLogBuilder = Builders<GamemasterToken>.IndexKeys;
                    this.tokens.Indexes.CreateOne(new CreateIndexModel<GamemasterToken>(Builders<GamemasterToken>.IndexKeys
                        .Ascending(gt => gt.Flag)));
                    break;
                }
                catch (Exception e)
                {
                    this.logger.LogError(e.ToFancyString());
                }
            }

            this.logger.LogDebug("GamemasterCheckerDatabase() finished");
        }

        public static string MongoHost => Environment.GetEnvironmentVariable("MONGO_HOST") ?? "localhost";

        public static string MongoPort => Environment.GetEnvironmentVariable("MONGO_PORT") ?? "localhost";

        public static string MongoUser => Environment.GetEnvironmentVariable("MONGO_USER") ?? string.Empty;

        public static string MongoPw => Environment.GetEnvironmentVariable("MONGO_PASSWORD") ?? string.Empty;

        public static string MongoConnection => $"mongodb://{MongoHost}:{MongoPort}";

        public async Task AddTokenUUIDAsync(string flag, string uuid, CancellationToken token)
        {
            var gtoken = new GamemasterToken() { Flag = flag, Token = uuid };
            await this.tokens.InsertOneAsync(gtoken, this.insertOneOptions, token);
        }

        public async Task<string?> GetTokenUUIDAsync(string flag, CancellationToken token)
        {
            var cursor = await this.tokens.FindAsync(t => t.Flag == flag, cancellationToken: token);
            List<GamemasterToken> gtoken = await cursor.ToListAsync(token);
            if (gtoken.Count <= 0)
            {
                return null;
            }

            return gtoken[0].Token;
        }

        public async Task<List<GamemasterUser>> GetUsersAsync(string flag, CancellationToken token)
        {
            var users = await this.users.FindAsync(user => user.Flag == flag, cancellationToken: token);
            return await users.ToListAsync(token);
        }

        public async Task AddUserAsync(GamemasterUser user, CancellationToken token)
        {
            this.logger.LogDebug($"AddUserAsync {user.Username}");
            await this.users.InsertOneAsync(user, this.insertOneOptions, token);
        }

        public async Task InsertUsersAsync(List<GamemasterUser> users, CancellationToken token)
        {
            this.logger.LogDebug($"InsertUsersAsync {users.Count} users");
            await this.users.InsertManyAsync(users, this.insertManyOptions, token);
        }
    }
}
