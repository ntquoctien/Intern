using AcademicService.Application;
using AcademicService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

AddSharedConnectionStringFile(builder);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddApplicationServices();
var connectionString = builder.Configuration.GetConnectionString("AcademicDb")
    ?? throw new InvalidOperationException("Connection string 'AcademicDb' was not found.");

builder.Services.AddDbContext<AcademicDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("FrontendDev");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

static void AddSharedConnectionStringFile(WebApplicationBuilder builder)
{
    var directory = new DirectoryInfo(builder.Environment.ContentRootPath);
    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "EducationSystem.sln")))
    {
        directory = directory.Parent;
    }

    if (directory is null)
    {
        return;
    }

    var connectionStringFile = Path.Combine(directory.FullName, $"connectionstrings.{builder.Environment.EnvironmentName}.json");
    builder.Configuration.AddJsonFile(connectionStringFile, optional: true, reloadOnChange: true);
}
