namespace Server {
    public interface ISubscriptionHandler {
        void Start(ITransportPublisher publisher);
        void OnSubscribe(string eventName);
        void OnUnsubscribe(string eventName);
    }
}
