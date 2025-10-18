using HackerNewsBestStories.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Adicione a configuração do Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "HackerNewsBestStories:";
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<HackerNewsBestStories.Api.Middleware.HttpErrorHandlingMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();