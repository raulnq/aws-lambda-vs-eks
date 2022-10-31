using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.SQS.Model;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda;

public class Function
{
    private readonly AmazonDynamoDBClient dynamoClient;
    private readonly AmazonSQSClient sqsClient;
    private readonly DynamoDBContext dbContext;
    private readonly string? queueUrl;
    public Function()
    {
        dynamoClient = new AmazonDynamoDBClient();
        dbContext = new DynamoDBContext(dynamoClient);
        sqsClient = new AmazonSQSClient();
        queueUrl = Environment.GetEnvironmentVariable("QUEUEURL");

    }
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        var req = JsonSerializer.Deserialize<RegisterPostRequest>(input.Body, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
        var task = new Post() { Id = Guid.NewGuid(), Description = req.Description, Title = req.Title };
        await dbContext.SaveAsync(task);
        var resp = JsonSerializer.Serialize(new RegisterPostResponse(task.Id));

        await sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = resp
        });

        return new APIGatewayProxyResponse
        {
            Body = resp,
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}

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
