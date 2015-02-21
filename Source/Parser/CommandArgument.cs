namespace Pyratron.Frameworks.Commands.Parser
{
    public class CommandArgument
    {
        /// <summary>
        /// The default value when no value is specified. Only used for optional commands.
        /// </summary>
        public string Default
        {
            get { return defaultValue; }
            private set
            {
                defaultValue = value;
                if (string.IsNullOrEmpty(this.value)) //If value is empty, set to default value
                    this.value = defaultValue;
            }
        }

        /// <summary>
        /// The name to refer to this argument in documentation/help.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates if this parameter is optional. All arguments are required by default.
        /// </summary>
        public bool Optional { get; set; }

        /// <summary>
        /// The value of this argument parsed from the command.
        /// </summary>
        public string Value
        {
            get { return value; }
            private set
            {
                this.value = value;
                if (string.IsNullOrEmpty(this.value)) //If value is empty, set to default value
                    this.value = Default;
            }
        }

        private string value, defaultValue;

        /// <summary>
        /// Constructs a new command argument.
        /// </summary>
        /// <param name="name">The name to refer to this argument in documentation/help.</param>
        /// <param name="optional">Indicates if this parameter is optional.</param>
        public CommandArgument(string name, bool optional = false)
        {
            Name = name;
            Optional = optional;
        }

        /// <summary>
        /// Creases a new command argument.
        /// </summary>
        /// <param name="name">The name to refer to this argument in documentation/help.</param>
        /// <param name="optional">Indicates if this parameter is optional.</param>
        public static CommandArgument Create(string name, bool optional = false)
        {
            return new CommandArgument(name, optional);
        }

        /// <summary>
        /// Makes the argument an optional parameter.
        /// Arguments are required by default.
        /// </summary>
        public CommandArgument MakeOptional()
        {
            Optional = true;
            return this;
        }

        /// <summary>
        /// Makes the argument a required parameter.
        /// Arguments are required by default.
        /// </summary>
        public CommandArgument MakeRequired()
        {
            Optional = false;
            return this;
        }

        /// <summary>
        /// Sets the value of the argument when parsing.
        /// Do not use this method when creating arguments.
        /// </summary>
        public CommandArgument SetValue(string value)
        {
            Value = value;
            return this;
        }

        /// <summary>
        /// Sets the value of the argument when parsing.
        /// Do not use this method when creating arguments.
        /// </summary>
        public CommandArgument SetValue(object value)
        {
            Value = value.ToString();
            return this;
        }

        /// <summary>
        /// Sets the default value for an optional parameter when no value is specified.
        /// Do not use this method when creating arguments.
        /// </summary>
        public CommandArgument SetDefault(string value)
        {
            Default = value;
            return this;
        }

        /// <summary>
        /// Sets the default value for an optional parameter when no value is specified.
        /// Do not use this method when creating arguments.
        /// </summary>
        public CommandArgument SetDefault(object value)
        {
            Default = value.ToString();
            return this;
        }
    }
}