namespace AmazonSqsListener.Messages;

public interface IMessage
{
    public string MessageTypeName { get; }
}