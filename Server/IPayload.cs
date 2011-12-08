namespace Server {
    public interface IPayload {
        string EventName { get; }
        string Name { get; set; }
        string Value { get; set; }
        string ToJson();
    }
}
