using System;

namespace Server {
    public class ChangeValuePayload : IPayload {
        public string EventName {
            get {
                return "changeValue";
            }
        }

        public string Name { get; set; }

        public string Value { get; set; }

        public string ToJson() {
            return string.Format(@"{{""event"": ""{0}"", ""args"": {{ ""name"": ""{1}"", ""value"": ""{2}""}} }}", EventName, Name, Value);
        }
    }
}
