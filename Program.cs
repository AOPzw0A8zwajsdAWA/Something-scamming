using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Add CORS and security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    await next();
});

app.MapGet("/", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync($@"<!DOCTYPE html>
<html lang=""en"">
    <head>
        <meta charset=""UTF-8"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
        <title>Contact Form</title>
        <style>
            body {{
                margin: 0;
                font-family: Arial, sans-serif;
                background: linear-gradient(135deg, #1f1f1f, #3a3a3a);
                display: flex;
                justify-content: center;
                align-items: center;
                height: 100vh;
                color: #ffffff;
            }}
            .form-container {{
                background: rgba(255, 255, 255, 0.1);
                backdrop-filter: blur(10px);
                border-radius: 15px;
                padding: 20px;
                box-shadow: 0 8px 32px rgba(0, 0, 0, 0.37);
                text-align: center;
                width: 300px;
            }}
            input[type=email] {{
                padding: 10px;
                border: none;
                border-radius: 10px;
                width: calc(100% - 22px);
                margin-top: 10px;
                margin-bottom: 20px;
                outline: none;
                font-size: 16px;
            }}
            button {{
                padding: 10px 20px;
                border: none;
                border-radius: 10px;
                background: linear-gradient(90deg, #ff8c00, #ff0080);
                color: white;
                font-size: 16px;
                cursor: pointer;
                transition: 0.3s;
            }}
            button:hover {{
                background: linear-gradient(90deg, #ff0080, #ff8c00);
            }}
        </style>
    </head>
    <body>
        <div class=""form-container"">
            <form method=""post"" action=""/submit"">
                <label for=""gmail"">Enter your Email:</label><br>
                <input type=""email"" id=""gmail"" name=""gmail"" required pattern=""[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{{2,}}$""><br>
                <button type=""submit"">Submit</button>
            </form>
        </div>
    </body>
</html>");
});

app.MapPost("/submit", async (HttpContext context) =>
{
    try
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var form = await context.Request.ReadFormAsync();
        var email = form["gmail"].ToString().Trim();

        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid email format.");
            return;
        }

        var webhookUrl = "https://discord.com/api/webhooks/1317241435204096082/-Rpt6RKMAmAElsDTnVTg44c5t7eCEeCn1eTdYa2Qy1kUXXxc-Cm3gtv6_HepoCVgCzsA";

        var payload = new
        {
            content = $"New submission:\n**Email:** {email}\n**IP Address:** {ipAddress}"
        };

        using var httpClient = new HttpClient();
        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(webhookUrl, content);
        response.EnsureSuccessStatusCode();

        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsync("Thank you for your submission.");
    }
    catch (Exception)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync("An error occurred. Please try again later.");
    }
});

app.Run();