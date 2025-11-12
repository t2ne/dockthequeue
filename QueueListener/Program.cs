using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text;

// --- MongoDB setup ---
var mongoClient = new MongoClient("mongodb://localhost:27017");
var database = mongoClient.GetDatabase("ApiMessages");
var collection = database.GetCollection<BsonDocument>("ReceivedJson");

// --- RabbitMQ setup ---
var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Ensure queue exists
channel.QueueDeclare(queue: "jsonQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

Console.WriteLine("🟢 Listening for messages on 'jsonQueue'...");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"📩 Received message: {message}");

    try
    {
        var document = BsonDocument.Parse(message);
        collection.InsertOne(document);
        Console.WriteLine("✅ Stored in MongoDB");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error storing in MongoDB: {ex.Message}");
    }
};

channel.BasicConsume(queue: "jsonQueue", autoAck: true, consumer: consumer);

Console.WriteLine("Press [enter] to exit.");
Console.ReadLine();
