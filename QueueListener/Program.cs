using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MongoDB.Driver;
using System.Text;

var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "jsonQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

Console.WriteLine("👂 A escutar a fila 'jsonQueue'...");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"📩 Mensagem recebida: {message}");

    // Conectar ao MongoDB e guardar
    var mongoClient = new MongoClient("mongodb://localhost:27017");
    var database = mongoClient.GetDatabase("MensagensDB");
    var collection = database.GetCollection<dynamic>("Mensagens");

    var doc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<MongoDB.Bson.BsonDocument>(message);
    collection.InsertOne(doc);

    Console.WriteLine("💾 Mensagem gravada no MongoDB!");
};

channel.BasicConsume(queue: "jsonQueue", autoAck: true, consumer: consumer);

Console.WriteLine("Pressiona [Enter] para sair.");
Console.ReadLine();
