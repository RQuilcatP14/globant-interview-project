using Microsoft.EntityFrameworkCore;
using TransactionMicroservice.Application.Services;
using TransactionMicroservice.Infrastructure.Kafka;
using TransactionMicroservice.Infrastructure.Persistence;
using TransactionMicroservice.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

var app = builder.Build();

app.MapControllers();
app.Run();