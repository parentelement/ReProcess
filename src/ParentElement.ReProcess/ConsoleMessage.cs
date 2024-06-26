﻿namespace ParentElement.ReProcess
{
    /// <summary>
    /// Represents a message generated by a <see cref="Command"/>.
    /// </summary>
    public struct ConsoleMessage
    {
        /// <summary>
        /// The generated message.
        /// </summary>
        public string Data { get; }

        /// <summary>
        /// MessageType of the generated message.
        /// </summary>
        public MessageType MessageType { get; }

        /// <summary>
        /// The <see cref="Command"/> that generated this message.
        /// </summary>
        public Command Command { get; }

        internal ConsoleMessage(Command command, string data, MessageType type)
        {
            Command = command;
            Data = data;
            MessageType = type;
        }
    }

    /// <summary>
    /// Represents the type of message that was generated.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Message was generated from the standard output stream.
        /// </summary>
        Output = 0,

        /// <summary>
        /// Message was generated from the standard error stream.
        /// </summary>
        Error = 1
    }
}
