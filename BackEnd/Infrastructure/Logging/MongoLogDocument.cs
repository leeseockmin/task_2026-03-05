using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackEnd.Infrastructure.Logging
{
    public record MongoLogDocument
    {
        [BsonId]
        public ObjectId Id { get; init; } = ObjectId.GenerateNewId();
        public string Action { get; init; } = string.Empty;
        public string TableName { get; init; } = string.Empty;
        public BsonDocument Payload { get; init; } = new();
        public DateTime OccurredAt { get; init; }
    }
}
