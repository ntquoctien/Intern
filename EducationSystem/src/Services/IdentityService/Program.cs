using IdentityService.Application;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Application.StudentAccess;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

var builder = WebApplication.CreateBuilder(args);

AddSharedConnectionStringFile(builder);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
        policy.SetIsOriginAllowed(origin =>
            Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            && (uri.Host == "localhost" || uri.Host == "127.0.0.1"))
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddApplicationServices();
builder.Services.AddOptions<StudentLoginOptions>()
    .Bind(builder.Configuration.GetSection(StudentLoginOptions.SectionName))
    .Validate(options => Enum.IsDefined(options.CodeSource), "StudentLogin:CodeSource is invalid.")
    .Validate(options => Uri.TryCreate(options.AcademicServiceBaseUrl, UriKind.Absolute, out _), "StudentLogin:AcademicServiceBaseUrl must be absolute.")
    .ValidateOnStart();
builder.Services.AddStudentJwtAuthentication(builder.Configuration);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IStudentTokenIssuer, StudentTokenIssuer>();
builder.Services.AddSingleton<ReadOnlyCommandInterceptor>();
var connectionString = builder.Configuration.GetConnectionString("IdentityDb")
    ?? throw new InvalidOperationException("Connection string 'IdentityDb' was not found.");

builder.Services.AddDbContext<IdentityDbContext>((services, options) =>
    options.UseSqlServer(connectionString)
        .AddInterceptors(services.GetRequiredService<ReadOnlyCommandInterceptor>()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("FrontendDev");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

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
