namespace Server {
    public interface ITransportPublisher {
        void Publish(IPayload payload);
    }
}
