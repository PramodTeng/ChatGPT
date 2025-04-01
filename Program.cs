using Azure;
using Azure.AI.OpenAI;
using ChatGPT.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure OpenAI service
builder.Services.AddSingleton(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var token = configuration["GitHubAI:Token"];
    var endpoint = configuration["GitHubAI:Endpoint"] ?? "https://models.inference.ai.azure.com";

    if (string.IsNullOrEmpty(token))
    {
        var envToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (string.IsNullOrEmpty(envToken))
        {
            throw new InvalidOperationException("GitHub token not found in configuration or environment variables.");
        }
        token = envToken;
    }

    var options = new OpenAIClientOptions();
    return new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(token), options);
});

// Register chat service
builder.Services.AddSingleton<ChatService>(sp =>
{
    var client = sp.GetRequiredService<OpenAIClient>();
    var config = sp.GetRequiredService<IConfiguration>();
    return new ChatService(client, config);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();