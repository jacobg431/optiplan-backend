using Microsoft.Extensions.Caching.Memory;
using Optiplan.DatabaseResources;
using Optiplan.WebApi.Repositories;
using Optiplan.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy
                .WithOrigins("http://localhost:3000", "http://localhost:5173") // Use your React dev server's URL
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddOptiplanContext();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IDependencyRepository, DependencyRepository>();
builder.Services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
builder.Services.AddScoped<IWorkOrderToDependencyRepository, WorkOrderToDependencyRepository>();
builder.Services.AddScoped<IOptimizationService, OptimizationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
