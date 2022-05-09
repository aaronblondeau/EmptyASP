// This is wasy to run with docker too!
// Right click Dockerfile to build
// Then run with
// docker run --rm -p 8080:80 -d emptyasp

// References
// https://docs.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-6.0&tabs=visual-studio
// https://benfoster.io/blog/mvc-to-minimal-apis-aspnet-6/
// https://github.com/Handlebars-Net/Handlebars.Net

// For swagger gen config
using Microsoft.OpenApi.Models;

// For Handlebars.Net
using HandlebarsDotNet;

var builder = WebApplication.CreateBuilder(args);

// Enable swagger
builder.Services.AddEndpointsApiExplorer();

// Trigger swagger doc generation
// If open api support was not checked on project init, you can add it with:
// PM> Install-Package Swashbuckle.AspNetCore
builder.Services.AddSwaggerGen(options =>
{
    // https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-6.0&tabs=visual-studio
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "The Minimal API"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // Will be at /swagger/index.html
    app.UseSwaggerUI();
}

// Custom middleware example (adds data to request)
app.Use(async (context, next) =>
{
    Console.WriteLine("Halo Fren - I iz Middleware! " + context.Request.Path);
    context.Items.Add("user", "Doge");
    await next.Invoke();
});

// Smallest thing that works
app.MapGet("/", () => "Hello World!");

// Get input from url parameters
app.MapGet("/hello/{name}", (string name) => $"Hello {name}!");

// Handle a basic post request, get input from body (includes input/output types for swagger)
// Swagger friendly version
app.MapPost("/thing", Thing (HttpContext context, Thing thang) => {
    Console.WriteLine(context.Request.Method);
    return thang;
});

// Use value from an env var
// Note - env var can be set by:
// Right click on EmptyASP project > Properties > Debug > Open debug launch profiles UI
var message = app.Configuration["HelloKey"] ?? "Hello";
app.MapGet("/message", String () => message);

// Make http call and return the results. See : https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-6.0
app.MapGet("/proxy", async Task<Todo> (HttpContext context) => {
    Console.WriteLine("Middleware says I am " + (string)context.Items["user"]);
    var client = new HttpClient();
    var todo = await client.GetFromJsonAsync<Todo>("https://jsonplaceholder.typicode.com/todos/1");
    return todo;
}).WithTags("Proxy"); // Sets swagger tag

// Serve static files (from wwwroot)
app.UseStaticFiles();

// Serve html rendered from a template//
// Install-Package Handlebars.Net
app.MapGet("/html", async context =>
{
    var source = System.IO.File.ReadAllText(@"./views/demo.html");
    var template = Handlebars.Compile(source);
    var data = new
    {
        title = "Demo Html",
        body = "This is super simple html!"
    };
    var result = template(data);
    await context.Response.WriteAsync(result);
});

app.Run();

// Input/Output classes:

public class Todo
{
    public string title { get; set; } = "";
    public bool completed { get; set; } = false;
    public int id { get; set; } = 0;
    public int userId { get; set; } = 0;
}

public class Thing
{
    public string name { get; set; } = "";
}