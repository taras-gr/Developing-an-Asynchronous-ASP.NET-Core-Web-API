using Books.API.DbContexts;
using Books.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

ThreadPool.SetMaxThreads(2, 2);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHttpClient();

builder.Services.AddDbContext<BooksContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("BooksDbConnectionString")));

builder.Services.AddScoped<IBooksRepository, BooksRepository>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
