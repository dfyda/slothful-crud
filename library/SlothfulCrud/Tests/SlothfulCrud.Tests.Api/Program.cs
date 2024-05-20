using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Extensions;
using SlothfulCrud.Tests.Api.EF;
using SlothfulCrud.Tests.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie();

builder.Services.AddDbContext<SlothfulDbContext>(options =>
    options.UseInMemoryDatabase(databaseName: "InMemoryDatabase"));

builder.Services.AddValidatorsFromAssemblyContaining<SlothValidator>();
builder.Services.AddSlothfulCrud<SlothfulDbContext>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "SlothfulCrud.Tests.Api")
    .WithName("GetApiName")
    .WithOpenApi();

app.MapGet("/db/test", (SlothfulDbContext context) => context.Koalas.ToList())
    .WithName("GetKoalas")
    .WithOpenApi();

app.UseSlothfulCrud<SlothfulDbContext>(options => options.UseSlothfulProblemHandling = true);
app.UseAuthentication();
app.UseAuthorization();

app.Run();

