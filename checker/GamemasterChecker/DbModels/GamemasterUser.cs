namespace GamemasterChecker.DbModels
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class GamemasterUser
    {
#pragma warning disable CS8618
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public long SessionId { get; set; }

        public string? Flag { get; set; }

        public bool IsMaster { get; set; }
#pragma warning restore CS8618
    }
}
