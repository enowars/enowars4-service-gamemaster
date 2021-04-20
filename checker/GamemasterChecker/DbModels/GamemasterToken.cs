namespace GamemasterChecker.DbModels
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

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
}
