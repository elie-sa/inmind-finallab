using FinalLabInmind.DbContext;
using FinalLabInmind.Interfaces;
using FinalLabInmind.Services;
using FinalLabInmind.Services.AccountService;
using FinalLabInmind.Services.TransactionLogService;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<IMessagePublisher, RabbitMqProducer>();

// my custom services
builder.Services.AddScoped<ITransactionLogService, TransactionLogService>();
builder.Services.AddScoped<IAccountService, AccountService>();

builder.Services.AddControllers()
    .AddOData(options => options
        .Select().Filter().OrderBy().Expand().Count().SetMaxTop(100));

builder.Services.AddScoped<IAppDbContext, AppDbContext>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();