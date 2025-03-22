using FinalLabInmind;
using FinalLabInmind.DbContext;
using FinalLabInmind.Interfaces;
using FinalLabInmind.Services;
using FinalLabInmind.Services.AccountLocalizationService;
using FinalLabInmind.Services.AccountService;
using FinalLabInmind.Services.ExceptionServices;
using FinalLabInmind.Services.RabbitMq;
using FinalLabInmind.Services.TransactionLocalizationService;
using FinalLabInmind.Services.TransactionLogService;
using FinalLabInmind.Services.TransactionService;
using FinalLabInmind.Services.UnitOfWork;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<IMessagePublisher, RabbitMqProducer>();

// my custom services
builder.Services.AddScoped<ITransactionLogService, TransactionLogService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddSingleton<IExceptionHandler, ExceptionHandler>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITransactionLocalizationService, TransactionLocalizationService>();
builder.Services.AddScoped<IAccountLocalizationService, AccountLocalizationService>();

builder.Services.AddSingleton<RequestLoggingMiddleware>();

builder.Services.AddControllers()
    .AddOData(options => options
        .Select().Filter().OrderBy().Expand().Count().SetMaxTop(100))
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.AddLocalization(options => 
    options.ResourcesPath = "");

builder.Services.AddScoped<IAppDbContext, AppDbContext>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

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

app.UseExceptionHandler(_ => { });

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<RequestLoggingMiddleware>();

app.Run();