using Newtonsoft.Json;

namespace Netus {
    internal class Protocol {
        private const string NewMember = "newMember";
        private const string Disconnect = "disconnect";

        //This could be one method using enum.
        public static Protocol Message(string message, string userName) => new Message(userName, message);

        //This could be one method using enum.
        public static Protocol MemberJoins(string userName) => new Protocol(NewMember, userName);

        //This could be one method using enum.
        public static Protocol MemberDisconnects(string userName) => new Protocol(Disconnect, userName);

        [JsonProperty("action")]
        private string Action { get; }

        [JsonProperty("result")]
        private string Result { get; }

        protected Protocol(string action, string result) {
            Action = action;
            Result = result;
        }

        public override string ToString() => JsonConvert.SerializeObject(this);
    }

    internal class Message : Protocol {
        [JsonProperty("user")]
        private string UserName { get; }

        public Message(string userName, string result) : base("message", result) {
            UserName = userName;
        }
    }
}