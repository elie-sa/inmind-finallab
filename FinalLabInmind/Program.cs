using FinalLabInmind;
using FinalLabInmind.DbContext;
using FinalLabInmind.Interfaces;
using FinalLabInmind.Services;
using FinalLabInmind.Services.ExceptionServices;
using FinalLabInmind.Services.RabbitMq;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<IMessagePublisher, RabbitMqProducer>();
builder.Services.AddSingleton<IExceptionHandler, ExceptionHandler>();
builder.Services.AddExceptionHandler<ExceptionHandler>();

builder.Services.AddSingleton<RequestLoggingMiddleware>();

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

app.UseExceptionHandler(_ => { });

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<RequestLoggingMiddleware>();

app.Run();