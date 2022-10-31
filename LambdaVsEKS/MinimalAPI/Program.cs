using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var awsOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();
builder.Services.AddAWSService<IAmazonSQS>();
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);
var queueUrl = Environment.GetEnvironmentVariable("QUEUEURL");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/posts", async (RegisterPostRequest request, IDynamoDBContext _dbContext, IAmazonSQS _sqsClient) =>
{
    var post = new Post() { Id = Guid.NewGuid(), Description = request.Description, Title = request.Title };
    await _dbContext.SaveAsync(post);
    await _sqsClient.SendMessageAsync(new SendMessageRequest
    {
        QueueUrl = queueUrl,
        MessageBody = JsonSerializer.Serialize(post)
    });
    return Results.Ok(new RegisterPostResponse(post.Id));
});

app.Run();

public record RegisterPostRequest(string Description, string Title);

public record RegisterPostResponse(Guid Id);

[DynamoDBTable("PostsTable")]
public class Post
{
    [DynamoDBHashKey("id")]
    public Guid Id { get; set; }
    [DynamoDBProperty("description")]
    public string? Description { get; set; }
    [DynamoDBProperty("title")]
    public string? Title { get; set; }
}