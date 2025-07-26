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

var isDevelopment = builder.Environment.IsDevelopment();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//var localConnection = builder.Configuration.GetConnectionString("DefaultConnection");
//var railwayConnection = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")?.Trim();
//var finalConnection = !string.IsNullOrEmpty(railwayConnection) ? railwayConnection : localConnection;

builder.Services.AddDbContext<kizwacartContext>(options =>
    options.UseNpgsql(connectionString));


builder.Services.AddSingleton<AuthService>();
builder.Services.AddScoped<CloudinaryService>(); // Register Cloudinary service

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
