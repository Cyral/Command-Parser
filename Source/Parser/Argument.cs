using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
                if (!IsValid(value))
                    throw new ArgumentException("Value does not fulfill the validation rule.");

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
        /// A validation rule that checks if the argument is valid.
        /// </summary>
        public ValidationRule Rule { get; set; }

        /// <summary>
        /// The value of this argument parsed from the command.
        /// </summary>
        public string Value
        {
            get { return value; }
            set
            {
                if (!IsValid(value))
                    throw new ArgumentException("Value does not fulfill the validation rule.");

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
            Rule = ValidationRule.AlwaysTrue;
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
            if (!IsValid(value))
                throw new ArgumentException("Value does not fulfill the validation rule.");

            Value = value;
            return this;
        }

        /// <summary>
        /// Sets the value of the argument when parsing.
        /// Do not use this method when creating arguments.
        /// </summary>
        public Argument SetValue(object value)
        {
            SetValue(value.ToString());
            return this;
        }

        /// <summary>
        /// Sets the default value for an optional parameter when no value is specified.
        /// Do not use this method when creating arguments.
        /// </summary>
        public Argument SetDefault(string value)
        {
            if (!IsValid(value))
                throw new ArgumentException("Value does not fulfill the validation rule.");

            Default = value;
            return this;
        }

        /// <summary>
        /// Sets the default value for an optional parameter when no value is specified.
        /// Do not use this method when creating arguments.
        /// </summary>
        public Argument SetDefault(object value)
        {
            SetDefault(value.ToString());
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
        /// </summary>
        /// <param name="rule">Represents a rule to validate an argument value on.</param>
        public Argument SetValidator(ValidationRule rule)
        {
            if (rule == null) throw new ArgumentNullException("rule");

            Rule = rule;
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

        /// <summary>
        /// Checks if the specified string is valid for the validation rule.
        /// </summary>
        public bool IsValid(string value)
        {
            return Rule.Validate(value);
        }

        /// <summary>
        /// Resets a value to empty, bypassing any validation.
        /// </summary>
        internal Argument ResetValue()
        {
            value = string.Empty;
            return this;
        }

        #region Nested type: Class

        /// <summary>
        /// Represents a rule to validate an argument value on.
        /// </summary>
        public class ValidationRule
        {
            public static readonly ValidationRule Integer, Double, Alphanumerical, Email, IP;
            internal static readonly ValidationRule AlwaysTrue;

            private static readonly Regex
                AlphaNumericReg = new Regex("^[a-zA-Z][a-zA-Z0-9]*$");

            /// <summary>
            /// A user friendly name that will be displayed in an error.
            /// Example: "Must be a valid ____"
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// A function that returns true if the string passed passes the rule.
            /// </summary>
            public Predicate<string> Validate { get; set; }

            static ValidationRule()
            {
                //Interal rule that will always return true, for default rule
                AlwaysTrue = new ValidationRule(string.Empty, s => true);

                Integer = new ValidationRule("Number", delegate(string s)
                {
                    int result;
                    return int.TryParse(s, out result);
                });

                Double = new ValidationRule("Number", delegate(string s)
                {
                    double result;
                    return double.TryParse(s, out result);
                });

                Email = new ValidationRule("Email", s => s.Contains('@') && s.Contains('.'));

                Alphanumerical = new ValidationRule("Alphanumeric string", s => AlphaNumericReg.IsMatch(s));

                IP = new ValidationRule("IP Address", delegate(string s)
                {
                    IPAddress ip;
                    var valid = !string.IsNullOrEmpty(s) && IPAddress.TryParse(s, out ip);
                    return valid;
                });
            }

            /// <summary>
            /// Creates a new validation rule
            /// </summary>
            /// <param name="friendlyName"> A user friendly name that will be displayed in an error. "Must be a valid ____"</param>
            /// <param name="validate">A function that returns true if the string passed passes the rule.</param>
            public ValidationRule(string friendlyName, Predicate<string> validate)
            {
                Name = friendlyName;
                Validate = validate;
            }

            /// <summary>
            /// Creates a new validation rule
            /// </summary>
            /// <param name="friendlyName"> A user friendly name that will be displayed in an error. "Must be a valid ____"</param>
            /// <param name="validate">A function that returns true if the string passed passes the rule.</param>
            public ValidationRule Create(string friendlyName, Predicate<string> validate)
            {
                return new ValidationRule(friendlyName, validate);
            }

            /// <summary>
            /// Returns the name of the rule that should be displayed in an error message.
            /// </summary>
            public string GetError()
            {
                return Name.ToLower();
            }
        }

        #endregion
    }
}