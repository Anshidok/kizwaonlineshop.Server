using kizwaonlineshop.Server.Data;
using kizwaonlineshop.Server.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Supabase;
var builder = WebApplication.CreateBuilder(args);


if (!builder.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://*:{port}");
}


builder.Services.AddHealthChecks();


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
        policy.WithOrigins("https://localhost:4200", "https://kizwaonline.vercel.app")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
        //policy.AllowAnyOrigin()
    });
});
var app = builder.Build();
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<kizwacartContext>();
//    dbContext.Database.Migrate();
//}

var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:AnonKey"];
if (!string.IsNullOrEmpty(supabaseUrl) && !string.IsNullOrEmpty(supabaseKey))
{
    var options = new SupabaseOptions { AutoConnectRealtime = true };
    var supabaseClient = new Supabase.Client(supabaseUrl, supabaseKey, options);
    builder.Services.AddSingleton(supabaseClient);
}
if (app.Environment.IsDevelopment())
{
    app.UseStaticFiles();
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images")),
        RequestPath = "/Images"
    });
}
else
{
    app.UseStaticFiles();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
