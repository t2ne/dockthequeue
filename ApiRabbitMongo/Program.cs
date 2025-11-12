using RabbitMQ.Client;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ__HOSTNAME") ?? "localhost";

app.MapPost("/send", async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    var factory = new ConnectionFactory() { HostName = rabbitHost };
    using var connection = factory.CreateConnection();
    using var channel = connection.CreateModel();

    channel.QueueDeclare(
        queue: "jsonQueue",
        durable: false,
        exclusive: false,
        autoDelete: false,
        arguments: null
    );

    var messageBytes = Encoding.UTF8.GetBytes(body);
    channel.BasicPublish(
        exchange: "",
        routingKey: "jsonQueue",
        basicProperties: null,
        body: messageBytes
    );

    Console.WriteLine($"Sent message to RabbitMQ ({rabbitHost}): {body}");

    return Results.Ok(new { message = "JSON enviado para a fila com sucesso!", body });
});

app.Run();
