using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonSimpleNotificationService>();



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("create-topic", async (
    [FromServices] IAmazonSimpleNotificationService _simpleNotificationService,
    [FromQuery] string topicName) =>
{

    var createTopicRequest = new CreateTopicRequest()
    {
        Name = topicName
    };

    var createTopicResponse = await _simpleNotificationService.CreateTopicAsync(createTopicRequest);


    return Results.Ok(createTopicResponse);

});





app.Run();
