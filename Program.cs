using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Endpoint que recebe um JSON e envia para a queue
app.MapPost("/send", async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    // Enviar para RabbitMQ
    var factory = new ConnectionFactory() { HostName = "localhost" };
    using var connection = factory.CreateConnection();
    using var channel = connection.CreateModel();

    channel.QueueDeclare(queue: "jsonQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

    var messageBytes = Encoding.UTF8.GetBytes(body);
    channel.BasicPublish(exchange: "", routingKey: "jsonQueue", basicProperties: null, body: messageBytes);

    return Results.Ok(new { message = "JSON enviado para a fila com sucesso!", body });
});

app.Run();
