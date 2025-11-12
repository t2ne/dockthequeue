using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text;

var mongoConn = Environment.GetEnvironmentVariable("MONGO_CONN_STR") ?? "mongodb://localhost:27017";
var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ__HOSTNAME") ?? "localhost";

var mongoClient = new MongoClient(mongoConn);
var database = mongoClient.GetDatabase("ApiMessages");
var collection = database.GetCollection<BsonDocument>("ReceivedJson");

var factory = new ConnectionFactory() { HostName = rabbitHost };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "jsonQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

Console.WriteLine($"Listening for messages on 'jsonQueue' via RabbitMQ ({rabbitHost})...");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"Received message: {message}");

    try
    {
        var document = BsonDocument.Parse(message);
        collection.InsertOne(document);
        Console.WriteLine($"Stored in MongoDB ({mongoConn})");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error storing in MongoDB: {ex.Message}");
    }
};

channel.BasicConsume(queue: "jsonQueue", autoAck: true, consumer: consumer);

Console.WriteLine("Press [enter] to exit.");
Console.ReadLine();
