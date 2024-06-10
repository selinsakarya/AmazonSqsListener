using System.Net;
using Amazon.SQS;
using Amazon.SQS.Model;
using AmazonSqsListener.Messages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AmazonSqsListener.Services;

public class UserService : BackgroundService
{
    private readonly ILogger<UserService> _logger;
    private readonly IAmazonSQS _sqs;
    private const string QueueName = "users";
    private readonly List<string> _messageSystemAttributeNames = ["All"];

    public UserService(
        ILogger<UserService> logger,
        IAmazonSQS sqs)
    {
        _logger = logger;
        _sqs = sqs;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        GetQueueUrlResponse? queueUrl = await _sqs.GetQueueUrlAsync(QueueName, stoppingToken);
       
        ReceiveMessageRequest receiveRequest = new ReceiveMessageRequest
        {
            QueueUrl = queueUrl.QueueUrl,
            MessageSystemAttributeNames = _messageSystemAttributeNames
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                ReceiveMessageResponse? messageResponse = await _sqs.ReceiveMessageAsync(receiveRequest, stoppingToken);

                if (messageResponse.HttpStatusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("An error occured");

                    continue;
                }

                foreach (Message? message in messageResponse.Messages)
                {
                    string? messageTypeName = message.MessageAttributes.GetValueOrDefault(nameof(IMessage.MessageTypeName))?.StringValue;

                    await _sqs.DeleteMessageAsync(queueUrl.QueueUrl, message.ReceiptHandle, stoppingToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
    }
}