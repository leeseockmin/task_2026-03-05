using BackEnd.Application.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace BackEnd.Infrastructure.Logging
{
    public class MongoLogService : IMongoLogService
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<MongoLogService> _logger;

        public MongoLogService(IMongoDatabase database, ILogger<MongoLogService> logger)
        {
            _database = database;
            _logger = logger;
        }

        public async Task LogAsync<T>(string tableName, string action, T payload)
        {
            try
            {
                var collectionName = $"{DateTime.UtcNow:yyyyMMdd}_{tableName}_Log";
                var collection = _database.GetCollection<MongoLogDocument>(collectionName);

                var bsonPayload = SerializePayload<T>(payload);

                var document = new MongoLogDocument
                {
                    Action = action,
                    TableName = tableName,
                    Payload = bsonPayload,
                    OccurredAt = DateTime.UtcNow
                };

                await collection.InsertOneAsync(document);
            }
            catch (Exception ex)
            {
                _logger.LogError($"MongoDB 로그 저장 실패. Table: {tableName}, Action: {action}. Message: {ex.Message}");
            }
        }

        private static BsonDocument SerializePayload<T>(T payload)
        {
            if (payload is null)
            {
                return new BsonDocument();
            }

            if (payload is System.Collections.IEnumerable enumerable && payload is not string)
            {
                var elementType = typeof(T).IsGenericType
                    ? typeof(T).GetGenericArguments()[0]
                    : typeof(object);

                var bsonArray = new BsonArray();
                foreach (var item in enumerable)
                {
                    var itemDoc = new BsonDocument();
                    using var itemWriter = new BsonDocumentWriter(itemDoc);
                    BsonSerializer.Serialize(itemWriter, elementType, item);
                    bsonArray.Add(itemDoc);
                }
                return new BsonDocument("items", bsonArray);
            }

            var doc = new BsonDocument();
            using var writer = new BsonDocumentWriter(doc);
            BsonSerializer.Serialize(writer, typeof(T), payload);
            return doc;
        }
    }
}
