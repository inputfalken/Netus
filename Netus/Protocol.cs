using Newtonsoft.Json;

namespace Netus {
    internal class Protocol {
        private const string MesssageAction = "message";
        private const string NewMemberAction = "new-member";
        private const string DisconnectAction = "disconnect";

        //This could be one method using enum.
        public static Protocol Message(string message) => new Protocol(MesssageAction, message);

        //This could be one method using enum.
        public static Protocol MemberJoins(string userName) => new Protocol(NewMemberAction, userName);

        //This could be one method using enum.
        public static Protocol MemberDisconnects(string userName) => new Protocol(DisconnectAction, userName);

        [JsonProperty("action")]
        private string Action { get; }

        [JsonProperty("result")]
        private string Result { get; }

        private Protocol(string action, string result) {
            Action = action;
            Result = result;
        }

        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}