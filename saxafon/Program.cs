using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Nu sa gasit ConnectionString");

builder.Services.AddDbContext<saxafon.Data.AppDbContext>(options => options.UseMySQL(connectionString));
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.IPasswordHasher<saxafon.Models.User>, Microsoft.AspNetCore.Identity.PasswordHasher<saxafon.Models.User>>();
builder.Services.AddScoped<saxafon.Services.IUserService, saxafon.Services.UserService>();
builder.Services.AddScoped<saxafon.Services.IResourcesService, saxafon.Services.ResourcesService>();
builder.Services.AddScoped<saxafon.Services.IReservationService, saxafon.Services.ReservationService>();

var clientAdress = builder.Configuration["ClientAdressURL:ClientAdress"] ?? throw new InvalidOperationException("ClientBaseUrl is missing in appsettings.json");

var AllowCors = "AllowBlazorClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowCors, policy =>
    {
        policy.WithOrigins(clientAdress) 
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseCors(AllowCors);
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
