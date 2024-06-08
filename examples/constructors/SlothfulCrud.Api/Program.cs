using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Api.EF;
using SlothfulCrud.Api.Slothful.Behaviors;
using SlothfulCrud.Api.Validators;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SlothfulDbContext>(options =>
    options.UseInMemoryDatabase(databaseName: "InMemoryDatabase"));

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

builder.Services.AddValidatorsFromAssemblyContaining<SlothValidator>();

builder.Services.AddSlothfulCrud<SlothfulDbContext>();
builder.Services.AddScoped<ICreateConstructorBehavior, CustomCreateConstructorBehavior>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSlothfulCrud<SlothfulDbContext>();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapGet("/", () => "SlothfulCrud.Api")
    .WithName("Get")
    .WithOpenApi();

app.Run();