using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pyratron.Frameworks.Commands.Parser
{
    /// <summary>
    /// Represents a parameter that is passed with command. Arguments may be required or optional, may contain a restricted set
    /// of values, and have their own nested arguments.
    /// </summary>
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

        private string value, defaultValue;

        /// <summary>
        /// Constructs a new command argument.
        /// </summary>
        /// <param name="name">The name to refer to this argument in documentation/help.</param>
        /// <param name="optional">Indicates if this parameter is optional.</param>
        public Argument(string name, bool optional = false)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            Arguments = new List<Argument>();
            Name = name.ToLower();
            Optional = optional;
        }

        #region IArguable Members

        /// <summary>
        /// Nested arguments that are contained within this argument
        /// Example: [foo [bar]]
        /// </summary>
        public List<Argument> Arguments { get; set; }

        #endregion

        /// <summary>
        /// Creases a new command argument.
        /// </summary>
        /// <param name="name">The name to refer to this argument in documentation/help.</param>
        /// <param name="optional">Indicates if this parameter is optional.</param>
        public static Argument Create(string name, bool optional = false)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
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
 
        /// <summary>
        /// Adds an option to the argument. Options make the argument behave like an enum, where only certain string values are
        /// allowed.
        /// Each option can have children arguments.
        /// </summary>
        public Argument AddOption(Argument value)
        {
            if (value == null) throw new ArgumentNullException("value");

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
        /// Add a nested argument to the argument. All arguments are required by default, and are parsed in the order they are
        /// defined.
        /// </summary>
        public Argument AddArgument(Argument argument)
        {
            if (argument == null) throw new ArgumentNullException("argument");

            var optional = Arguments.Any(arg => arg.Optional);
            if (optional && !argument.Optional)
                throw new InvalidOperationException("Optional arguments must come last.");

            Arguments.Add(argument);

            return this;
        }

        /// <summary>
        /// Adds an array nested arguments to the current argument. Optional arguments must come last.
        /// </summary>
        public Argument AddArguments(Argument[] arguments)
        {
            if (arguments == null) throw new ArgumentNullException("arguments");

            foreach (var arg in arguments)
                AddArgument(arg);
            return this;
        }
    }
}