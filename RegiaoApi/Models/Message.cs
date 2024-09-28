namespace RegiaoApi.Models
{
    public class Message<T> where T:class
    {
        public Message(EventTypes eventType, T payload) 
        {
            EventType = eventType;
            Payload = payload;
            MessageDate = DateTime.Now;
            MessageId = Guid.NewGuid();
        }
        public Guid MessageId { get; set;}
        public DateTime MessageDate { get; set; }
        public EventTypes EventType { get; set; }
        public T Payload { get; set; }
    }

    public enum EventTypes
    {
        CREATE = 1,
        UPDATE = 2,
        DELETE = 3
    }
}