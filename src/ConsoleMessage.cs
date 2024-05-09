namespace ReProcess
{
    public struct ConsoleMessage
    {
        public string Data { get; }
        public MessageType MessageType { get; }
        public Command Command { get; }

        public ConsoleMessage(Command command, string data, MessageType type)
        {
            Command = command;
            Data = data;
            MessageType = type;
        }
    }

    public enum MessageType
    {
        Output = 0,
        Error = 1
    }
}
