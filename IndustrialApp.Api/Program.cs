using IndustrialApp.Application.Ai;
using IndustrialApp.Application.Audit;
using IndustrialApp.Application.Security;
using IndustrialApp.Infrastructure.Audit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var modelId = builder.Configuration["Ai:ModelId"]
              ?? throw new InvalidOperationException("Ai:ModelId is missing.");

var endpoint = builder.Configuration["Ai:Endpoint"]
               ?? throw new InvalidOperationException("Ai:Endpoint is missing.");

builder.Services.AddKernel()
    .AddOllamaChatCompletion(
        modelId: modelId,
        endpoint: new Uri(endpoint));

builder.Services.AddHttpClient("OllamaHealth", client =>
{
    client.BaseAddress = new Uri(endpoint);
    client.Timeout = TimeSpan.FromSeconds(2);
});

builder.Services.AddScoped<IAiAssistantService, AiAssistantService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();

app.MapControllers();
app.MapRazorPages();

app.MapGet("/", context =>
{
    context.Response.Redirect("/AiAssistant");
    return Task.CompletedTask;
});

app.Run();