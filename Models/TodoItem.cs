using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class TodoItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("isComplete")]
    public bool IsComplete { get; set; } = false;
}
