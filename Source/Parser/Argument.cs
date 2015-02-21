using System;
using System.Collections.Generic;

namespace Pyratron.Frameworks.Commands.Parser
{
    public class Argument : IArguable
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
        /// Indicates if the values should act as an enum, where each possible value must be added through <c>AddOption(..)</c>
        /// </summary>
        public bool Enum { get; set; }

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

        /// <summary>
        /// Nested arguments that are contained within this argument
        /// Example: [foo [bar]]
        /// </summary>
        public List<Argument> Arguments { get; set; }

        private string value, defaultValue;

        /// <summary>
        /// Constructs a new command argument.
        /// </summary>
        /// <param name="name">The name to refer to this argument in documentation/help.</param>
        /// <param name="optional">Indicates if this parameter is optional.</param>
        public Argument(string name, bool optional = false)
        {
            Arguments = new List<Argument>();
            Name = name;
            Optional = optional;
        }

        /// <summary>
        /// Creases a new command argument.
        /// </summary>
        /// <param name="name">The name to refer to this argument in documentation/help.</param>
        /// <param name="optional">Indicates if this parameter is optional.</param>
        public static Argument Create(string name, bool optional = false)
        {
            return new Argument(name, optional);
        }

        /// <summary>
        /// Makes the argument an optional parameter.
        /// Arguments are required by default.
        /// </summary>
        public Argument MakeOptional()
        {
            Optional = true;
            return this;
        }

        /// <summary>
        /// Makes the argument a required parameter.
        /// Arguments are required by default.
        /// </summary>
        public Argument MakeRequired()
        {
            Optional = false;
            return this;
        }

        /// <summary>
        /// Sets the value of the argument when parsing.
        /// Do not use this method when creating arguments.
        /// </summary>
        public Argument SetValue(string value)
        {
            Value = value;
            return this;
        }

        /// <summary>
        /// Sets the value of the argument when parsing.
        /// Do not use this method when creating arguments.
        /// </summary>
        public Argument SetValue(object value)
        {
            Value = value.ToString();
            return this;
        }

        /// <summary>
        /// Sets the default value for an optional parameter when no value is specified.
        /// Do not use this method when creating arguments.
        /// </summary>
        public Argument SetDefault(string value)
        {
            Default = value;
            return this;
        }

        /// <summary>
        /// Sets the default value for an optional parameter when no value is specified.
        /// Do not use this method when creating arguments.
        /// </summary>
        public Argument SetDefault(object value)
        {
            Default = value.ToString();
            return this;
        }

        // <summary>
        /// Creates an argument from a string automatically.
        /// </summary>
        /// <example>
        /// A command such as "Give a player an item X times" could be defined as:
        /// "%lt;player&gt; &lt;item&gt; [amount]"
        /// Where &gt; &lt; represent required items, and [ ] represent optional items.
        /// These tags can also be nested to represent more complex values.
        /// (value) represents a default value for an argument. (Must follow immediately)
        /// </example>
        /// <param name="input">The string with the argument info, see example for more information.</param>
        public static Argument[] InferArguments(string input)
        {
            //Trim input and make sure it isn't null
            if (string.IsNullOrEmpty(input)) throw new ArgumentNullException("input");
            input = input.Trim();

            var inputArgs = input.Split(' '); //Split into arguments

            var result = new Argument[inputArgs.Length];

            for (var i = 0; i < inputArgs.Length; i++)
            {
                var arg = inputArgs[i];

                //Find type
                bool? required = null;
                if (arg.StartsWith("<") && arg.EndsWith(">"))
                    required = true;
                else if (arg.StartsWith("[") && (arg.EndsWith("]") || (arg.EndsWith(")") && arg.Contains("]("))))
                    required = false;
                if (required == null)
                    throw new InvalidOperationException(
                        string.Format(
                            "Argument '{0}' is not defined properly. Arguments must be surrounded with <> if they are required, or [] if they are optional.",
                            arg));

                var end = arg.Length - 2;

                //Find default value if it has one
                var indexDefault = required.Value
                    ? arg.IndexOf(">(", StringComparison.Ordinal)
                    : arg.IndexOf("](", StringComparison.Ordinal);
                var defaultArg = string.Empty;
                if (arg.EndsWith(")") && indexDefault > 1)
                {
                    defaultArg = arg.Substring(indexDefault + 2, end - (indexDefault + 1));
                    end = indexDefault - 1;
                }

                var argName = arg.Substring(1, end); //Take all but first and last character to find name
                if (string.IsNullOrEmpty(argName))
                    throw new InvalidOperationException(
                        string.Format("Argument {0} must contain a name within their brackets.", i + 1));
                argName = char.ToUpper(argName[0]) + argName.Substring(1); //Uppercase first to look nicer

                //Create argument
                var cmdArg = new Argument(argName, !required.Value);
                cmdArg.SetDefault(defaultArg);
                result[i] = cmdArg;
            }

            return result;
        }

        /// <summary>
        /// Adds an option to the argument. Options make the argument behave like an enum, where only certain string values are allowed.
        /// Each option can have children arguments.
        /// </summary>
        public Argument AddOption(Argument value)
        {
            Enum = true;
            AddArgument(value);
            return this;
        }

        /// <summary>
        /// Restricts the possible values to a list of children, acting as if it were an enum.
        /// Each option can have children arguments.
        /// </summary>
        /// <returns></returns>
        public Argument MakeEnum()
        {
            Enum = true;
            return this;
        }


        /// <summary>
        /// Add a nested argument to the argument. All arguments are required by default, and are parsed in the order they are defined.
        /// </summary>
        public Argument AddArgument(Argument argument)
        {
            Arguments.Add(argument);
            return this;
        }

        /// <summary>
        /// Adds an array nested arguments to the current argument. Optional arguments must come last.
        /// </summary>
        public Argument AddArguments(Argument[] arguments)
        {
            bool optional = false;
            foreach (var arg in arguments)
            {
                if (arg.Optional)
                    optional = true;
                else if (optional)
                    throw new InvalidOperationException("Optional arguments must come last.");
            }

            Arguments.AddRange(arguments);
            return this;
        }
    }
}