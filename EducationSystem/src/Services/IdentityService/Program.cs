using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<IdentityDbContext>(options =>
{
	var connectionString = builder.Configuration.GetConnectionString("SqlServer")
		?? throw new InvalidOperationException("Connection string 'SqlServer' was not found.");

	options.UseSqlServer(connectionString, sql =>
		sql.MigrationsHistoryTable("__EFMigrationsHistory", IdentityDbContext.Schema));
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
