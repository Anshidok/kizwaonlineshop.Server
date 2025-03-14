using kizwaonlineshop.Server.Data;
using kizwaonlineshop.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddDbContext<kizwacartContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("kizwacartdb")));
//var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

//var configuration = new ConfigurationBuilder()
//    .SetBasePath(Directory.GetCurrentDirectory())
//    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//    .AddJsonFile($"appsettings.{environment}.json", optional: true)
//    .AddEnvironmentVariables() 
//    .Build();
//var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
//                        ?? builder.Configuration.GetConnectionString("DefaultConnection");


var isDevelopment = builder.Environment.IsDevelopment();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var localConnection = builder.Configuration.GetConnectionString("DefaultConnection");
var railwayConnection = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")?.Trim();
var finalConnection = !string.IsNullOrEmpty(railwayConnection) ? railwayConnection : localConnection;

builder.Services.AddDbContext<kizwacartContext>(options =>
    options.UseNpgsql(finalConnection));


builder.Services.AddSingleton<AuthService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("https://localhost:4200", "https://kizwaonlineshop.up.railway.app")
              .AllowAnyMethod()
              .AllowAnyHeader();

        //.WithOrigins("https://localhost:4200", "https://kizwaonlineshop.up.railway.app")
        //policy.AllowAnyOrigin()
    });
});
var app = builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
