using VeraAltin.Server.Hubs;
using VeraAltin.Server.Workers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<RabbitMQConsumer>();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true)
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseCors("CorsPolicy");

app.MapHub<VeraAltinHub>("/goldHub");
app.MapGet("/", () => "VeraAltin Server Çalışıyor!");
app.Run();