using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using my_books.Data;
using my_books.Data.Services;
using my_books.Exceptions;
using Serilog;

var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();

try
{
    Log.Logger = new LoggerConfiguration()
        .CreateLogger();
}
finally
{
    Log.CloseAndFlush();
}

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(configuration));

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString"));
});
builder.Services.AddTransient<BooksService>();
builder.Services.AddTransient<AuthorsService>();
builder.Services.AddTransient<PublishersService>();
builder.Services.AddTransient<LogsService>();

builder.Services.AddApiVersioning(config =>
{
    config.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    config.AssumeDefaultVersionWhenUnspecified = true;

    config.ApiVersionReader = new HeaderApiVersionReader("custom-version-header");
});

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

//Exception Handling
ILoggerFactory loggerFactory = new LoggerFactory();
app.ConfigureBuildInExceptionHandler(loggerFactory);
//app.ConfigureCustomExceptionHandler();

app.MapControllers();

app.Run();

AppDbInitializer.Seed(app);