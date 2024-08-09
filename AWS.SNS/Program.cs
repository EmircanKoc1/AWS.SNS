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

app.MapGet("list-topics", async (
    [FromServices] IAmazonSimpleNotificationService _simpleNotificationService) =>
{
    var listTopicsRequest = new ListTopicsRequest();


    var listTopicResponse = await _simpleNotificationService.ListTopicsAsync(listTopicsRequest);

    return Results.Ok(listTopicResponse.Topics);


});


app.MapDelete("delete-topic-by-name", async (
    [FromServices] IAmazonSimpleNotificationService _simpleNotificationService,
    [FromQuery] string topicName) =>
{
    DeleteTopicResponse? deleteTopicResponse = default;

    if ((await GetTopic(_simpleNotificationService, topicName)) is Topic topic)
        deleteTopicResponse = await _simpleNotificationService.DeleteTopicAsync(topic.TopicArn);


    if (deleteTopicResponse is null)
        return Results.BadRequest("topic not found");

    return Results.Ok("topic deleted");

});


app.MapGet("list-topic-subscriptions", async (
    [FromServices] IAmazonSimpleNotificationService _simpleNotificationService,
    [FromQuery] string topicName) =>
{
    ListSubscriptionsByTopicResponse listSubscriptionsByTopicResponse = default;

    if ((await GetTopic(_simpleNotificationService, topicName)) is Topic topic)
    {
        listSubscriptionsByTopicResponse = await _simpleNotificationService.ListSubscriptionsByTopicAsync(topic.TopicArn);
        return Results.Ok(listSubscriptionsByTopicResponse.Subscriptions);
    }
    
    return Results.BadRequest("topic not found");

});

app.MapPost("subscribe-protocol-to-topics/{topicName}/{protocol}", async (
      [FromServices] IAmazonSimpleNotificationService _simpleNotificationService,
      [FromRoute] string topicName,
      [FromRoute] string protocol,
      [FromQuery] string arn) =>
{
    if (!ProtocolIsValid(protocol))
        return Results.BadRequest("protocol not valid");

    if ((await GetTopic(_simpleNotificationService, topicName)) is Topic topic)
    {
        var subscribeRequest = new SubscribeRequest
        {
            TopicArn = topic.TopicArn,
            Protocol = protocol,
            Endpoint = arn
        };

        var subscribeResponse = await _simpleNotificationService.SubscribeAsync(subscribeRequest);

        return subscribeResponse.HttpStatusCode is System.Net.HttpStatusCode.OK ?
        Results.Ok(subscribeRequest) :
        Results.BadRequest($"{protocol} not subscribed topic");

    }

    return Results.BadRequest("topic not found ");



});





app.Run();


static async Task<Topic?> GetTopic(IAmazonSimpleNotificationService snsService, string topicName)
    => await snsService.FindTopicAsync(topicName);


static IEnumerable<string> GetAWSProtocols()
{
    // aws protocols  : aws, lambda ,http , https , email , email.json , sms , application , iot 

    yield return "sqs";
    yield return "lambda";
    yield return "http";
    yield return "https";
    yield return "email";
    yield return "email.json";
    yield return "application";
    yield return "iot";
}

static bool ProtocolIsValid(string protocol)
{

    foreach (var protocolItem in GetAWSProtocols())
        if (protocolItem.Equals(protocol))
            return true;


    return false;
}


