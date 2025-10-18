using Microsoft.EntityFrameworkCore;
using ZiyoMarket.Api.Extensions;
using ZiyoMarket.Data.Context;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ? PostgreSQL bazaga ulash
builder.Services.AddDbContext<ZiyoMarketDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ? Servislarni DI konteynerga qo‘shamiz
builder.Services.AddCustomServices();



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
