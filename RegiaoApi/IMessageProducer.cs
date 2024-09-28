using RegiaoApi.Models;

public interface IMessageProducer
{
    void SendMessageToQueue<T>(Message<T> bodyMessage) where T : class;
}

