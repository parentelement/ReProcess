namespace ParentElement.ReProcess
{
    /// <summary>
    /// A builder class for constructing <see cref="Command"/> instances.
    /// </summary>
    public sealed class CommandBuilder
    {
        private CommandDefinition _definition;

        private CommandBuilder(string commandName)
        {
            _definition = new CommandDefinition(commandName);
        }

        /// <summary>
        /// Creates a new <see cref="CommandBuilder"/> instance with the specified command name.
        /// </summary>
        /// <param name="commandName">The name of the executable</param>
        /// <returns>The current <see cref="CommandBuilder" /> instance </returns>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="commandName"/> is null or empty</exception>
        public static CommandBuilder Create(string commandName)
        {
            if(string.IsNullOrWhiteSpace(commandName))
                throw new ArgumentException("Command name cannot be null or empty", nameof(commandName));

            return new CommandBuilder(commandName);
        }

        /// <summary>
        /// Supplies an argument to the generated <see cref="Command"/>.
        /// </summary>
        /// <param name="argument">A <see langword="string" /> representing the argument or arguments to include</param>
        /// <returns>The current <see cref="CommandBuilder" /> instance </returns>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="argument"/> is null or zero length</exception>
        public CommandBuilder WithArgument(string argument)
        {
            if(string.IsNullOrEmpty(argument))
                throw new ArgumentException("Argument cannot be null or zero length", nameof(argument));

            _definition.Arguments.Add(argument);
            return this;
        }

        /// <summary>
        /// Supplies arguments to the generated <see cref="Command"/>.
        /// </summary>
        /// <param name="arguments">The collection of arguments to include</param>
        /// <returns>The current <see cref="CommandBuilder" /> instance </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="arguments"/> is null</exception>
        public CommandBuilder WithArguments(IEnumerable<string> arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            foreach(var arg in arguments)
            {
                _definition.Arguments.Add(arg);
            }

            return this;
        }

        /// <summary>
        /// Defines the working directory for the constructed <see cref="Command" />.
        /// </summary>
        /// <param name="path">A <see langword="string"/> representing the Working Directory for the command</param>
        /// <returns>The current <see cref="CommandBuilder" /> instance </returns>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="path"/> is specified and not a valid directory</exception>
        public CommandBuilder WithWorkingDirectory(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(path))
                throw new DirectoryNotFoundException("The specified working directory does not exist.");

            _definition.WorkingDirectory = path;
            return this;
        }

        /// <summary>
        /// Adds or replaces an environment variable to the constructed <see cref="Command"/>.
        /// </summary>
        /// <param name="key">The name of the environment variable to add</param>
        /// <param name="value">The value of the specified environment variable</param>
        /// <returns>The current <see cref="CommandBuilder" /> instance </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="key"/> is null</exception>
        public CommandBuilder WithEnvironmentVariable(string key, string value)
        {
            if(key == null)
                throw new ArgumentNullException(nameof(key));

            if (_definition.EnvironmentVariables.ContainsKey(key))
            {
                _definition.EnvironmentVariables[key] = value;
            }
            else
            {
                _definition.EnvironmentVariables.Add(key, value);
            }

            return this;
        }

        /// <summary>
        /// Enables output of StdOut and StdErr messages to be read by "M:ReProcess.Command.ReadOutputAsync".
        /// </summary>
        /// <param name="maxMessageCount">The number of StdOut and StdErr messages to keep in the buffer at one time.  Default is no limit</param>
        /// <returns>The current <see cref="CommandBuilder" /> instance </returns>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="maxMessageCount"/> is non-null and non-positive</exception>
        public CommandBuilder WithOutput(int? maxMessageCount = null)
        {
            if(maxMessageCount.HasValue && maxMessageCount.Value <= 0)
                throw new ArgumentException("Max message count must be greater than zero if supplied", nameof(maxMessageCount));

            _definition.RelayOutput = true;
            _definition.MaxBufferSize = maxMessageCount;
            return this;
        }

        /// <summary>
        /// Removes the delay between messsage output.  By default, the delay is 50ms per iteration.
        /// </summary>
        /// <returns>The current <see cref="CommandBuilder" /> instance </returns>
        public CommandBuilder WithAggressiveOutputProcessing()
        {
            _definition.UseAggressiveOutputProcessing = true;
            return this;
        }

        /// <summary>
        /// Builds a <see cref="Command"/> instance based on the current configuration."/>
        /// </summary>
        /// <returns>The constructed <see cref="Command"/></returns>
        public Command Build()
        {
            return new Command(_definition);
        }
    }
}
