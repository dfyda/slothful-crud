using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Extensions;
using SlothfulCrud.Services;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Tests.Api.EF;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SlothfulDbContext>(options =>
    options.UseInMemoryDatabase(databaseName: "InMemoryDatabase"));

builder.Services.AddSlothfulServices(typeof(SlothfulDbContext), Assembly.GetExecutingAssembly());
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

app.MapGet("/db/test", (SlothfulDbContext context) => context.Sloths.ToList())
    .WithName("GetSloths")
    .WithOpenApi();

app.MapGet("/di/test", (IOperationService<Sloth, SlothfulDbContext> service) => service.Get())
    .WithName("GetDiSloths")
    .WithOpenApi();

app.RegisterSlothfulEndpoints(typeof(SlothfulDbContext), Assembly.GetExecutingAssembly());

app.Run();