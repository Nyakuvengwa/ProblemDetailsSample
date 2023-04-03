using Microsoft.AspNetCore.Diagnostics;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-7.0#problem-details-service
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.ContentType = "application/problem+json";
        if (context.RequestServices.GetService<IProblemDetailsService>() is { } problemDetailsService)
        {
            var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
            var exceptionType = exceptionHandlerFeature?.Error;
            if (exceptionType is not null)
            {
                (string Detail, string Title, int StatusCode) = exceptionType switch
                {
                    ApplicationCustomException customException =>
                    (
                        exceptionType.Message,
                        exceptionType.GetType().Name,
                        context.Response.StatusCode = (int)customException.StatusCode
                    ),
                    _ =>
                    (
                        exceptionType.Message,
                        exceptionType.GetType().Name,
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError
                    )
                };

                await problemDetailsService.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails =
                    {
                        Title = Title,
                        Detail = Detail,
                        Type = $"https://httpstatuses.io/{StatusCode}",
                        Status = StatusCode
                    }
                });
            }
        }
    });
});



app.UseStatusCodePages();

app.MapGet("/throw", () =>
{
    throw new ApplicationCustomException("Sample Error", HttpStatusCode.InternalServerError);
})
.WithName("ThrowSampleError")
.WithOpenApi();

app.MapGet("/throw/notfound/{product}", (string product) =>
{
    throw new ProductNotFoundException($"Product {product} does not exist.");
})
.WithName("ThrowProductNotFoundError")
.WithOpenApi();

app.Run();


public class ApplicationCustomException
    : Exception
{
    public ApplicationCustomException(
        string message,
        HttpStatusCode statusCode
    ) : base(message)
    {
        StatusCode = statusCode;
    }
    public HttpStatusCode StatusCode { get; }
}
public class ProductNotFoundException : ApplicationCustomException
{
    public ProductNotFoundException(string message) : base(message, HttpStatusCode.NotFound)
    {
    }
}