﻿using QuantifulStocksAPI.Context;
using QuantifulStocksAPI.Helpers;
using QuantifulStocksAPI.Interfaces;
using QuantifulStocksAPI.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.Configure<GlobalSettings>(builder.Configuration.GetSection(GlobalSettings.Settings));

builder.Services.AddControllers();
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

