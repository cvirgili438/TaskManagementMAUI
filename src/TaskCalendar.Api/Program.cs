using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TaskCalendar.Application.Services;
using TaskCalendar.Infrastructure.Configuration;
using TaskCalendar.Infrastructure.Data;
using TaskCalendar.Infrastructure.Data.Seed;
using TaskCalendar.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddTaskCalendarInfrastructure(builder.Configuration);
builder.Services.AddScoped<TaskValidationService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwt = new JwtOptions
{
    Key = jwtSection["Key"] ?? new JwtOptions().Key,
    Issuer = jwtSection["Issuer"] ?? new JwtOptions().Issuer,
    Audience = jwtSection["Audience"] ?? new JwtOptions().Audience,
    ExpirationMinutes = int.TryParse(jwtSection["ExpirationMinutes"], out var expirationMinutes)
        ? expirationMinutes
        : new JwtOptions().ExpirationMinutes
};
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskCalendarDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<TaskCalendar.Infrastructure.Identity.AppUser>>();
    await Seeder.SeedAsync(dbContext, userManager);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
