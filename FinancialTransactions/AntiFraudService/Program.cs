using AntiFraudMicroservice.Application.Services;
using AntiFraudMicroservice.Infrastructure.Kafka;
using AntiFraudMicroservice.Infrastructure.Persistence;
using AntiFraudMicroservice.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IAntiFraudService, AntiFraudMicroservice.Application.Services.AntiFraudService>();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddSingleton<IAntiFraudConsumer, AntiFraudConsumer>();

var app = builder.Build();

// Iniciar el consumidor de Kafka en un hilo separado
var consumer = app.Services.GetRequiredService<IAntiFraudConsumer>();
Task.Run(() => consumer.ConsumeAsync());

app.MapControllers();
app.Run();
