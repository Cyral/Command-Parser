using System;
using System.Collections.Generic;

namespace Pyratron.Frameworks.Commands.Parser
{
    public class Command
    {
        /// <summary>
        /// The permission level needed to invoke the command.
        /// Useful for disabling commands for different "ranks".
        /// </summary>
        public int AccessLevel { get; set; }

        /// <summary>
        /// An action to be executed when the command is ran with successful input.
        /// </summary>
        public Action<CommandArgument[]> Action { get; set; }

        /// <summary>
        /// The strings that will call the command.
        /// </summary>
        public List<string> Aliases { get; set; }

        /// <summary>
        /// The input (Including alias and help) that are passed with the command.
        /// </summary>
        public List<CommandArgument> Arguments { get; set; }

        /// <summary>
        /// Describes the command and provides basic information about it.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Represents the command's name.
        /// Used for help and a "friendly" name.
        /// Use <c>Aliases</c> for the "short" version that calls the command.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Creates a command with a human friendly name and a command alias.
        /// </summary>
        /// <param name="name">Human friendly name.</param>
        /// <param name="alias">Alias to use when sending the command. (Ex: "help", "exit")</param>
        /// <param name="description">A description that provides basic information about the command.</param>
        public Command(string name, string alias, string description) : this(name, alias)
        {
            SetDescription(description);
        }

        /// <summary>
        /// Creates a command with a human friendly name and a command alias.
        /// </summary>
        /// <param name="name">Human friendly name.</param>
        /// <param name="alias">Alias to use when sending the command. (Ex: "help", "exit")</param>
        public Command(string name, string alias) : this(name)
        {
            AddAlias(alias);
        }

        /// <summary>
        /// Creates a command with the specified name.
        /// </summary>
        /// <param name="name">Human friendly name</param>
        public Command(string name)
        {
            Arguments = new List<CommandArgument>();
            Aliases = new List<string>();
            SetName(name);
        }

        /// <summary>
        /// Creates a command with a human friendly name and a command alias.
        /// </summary>
        /// <param name="name">Human friendly name.</param>
        /// <param name="alias">Alias to use when sending the command. (Ex: "help", "exit")</param>
        /// <param name="description">A description that provides basic information about the command.</param>
        public static Command Create(string name, string alias, string description)
        {
            return new Command(name, alias, description);
        }

        /// <summary>
        /// Creates a command with a human friendly name and a command alias.
        /// </summary>
        /// <param name="name">Human friendly name.</param>
        /// <param name="alias">Alias to use when sending the command. (Ex: "help", "exit")</param>
        public static Command Create(string name, string alias)
        {
            return new Command(name, alias);
        }

        /// <summary>
        /// Creates a command with the specified name.
        /// </summary>
        /// <param name="name">Human friendly name</param>
        public static Command Create(string name)
        {
            return new Command(name);
        }

        /// <summary>
        /// Sets a friendly name for the command.
        /// Note that the actual "/command" is defined as an alias.
        /// </summary>
        public Command SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Adds a command alias that will call the action.
        /// </summary>
        /// <param name="alias">Alias that will call the action (Ex: "help", "exit")</param>
        public Command AddAlias(string alias)
        {
            Aliases.Add(alias);
            return this;
        }

        /// <summary>
        /// Adds command aliases that will call the action.
        /// </summary>
        /// <param name="aliases">Aliases that will call the action (Ex: "help", "exit")</param>
        public Command AddAlias(params string[] aliases)
        {
            Aliases.AddRange(aliases);
            return this;
        }

        /// <summary>
        /// Sets a description for the command.
        /// </summary>
        /// <param name="description">Describes the command and provides basic information about it.</param>
        public Command SetDescription(string description)
        {
            Description = description;
            return this;
        }

        /// <summary>
        /// Restricts the command from being run if the access level is below what is specified.
        /// Useful for creating "ranks" where permission is needed to run a command.
        /// </summary>
        public Command RestrictAccess(int accessLevel)
        {
            AccessLevel = accessLevel;
            return this;
        }

        /// <summary>
        /// Sets an action to be ran when the command is executed.
        /// </summary>
        /// <param name="action">
        /// Action to be ran, which takes a <c>CommandArgument</c> array parameter representing the passes
        /// input.
        /// </param>
        public Command SetAction(Action<CommandArgument[]> action)
        {
            Action = action;
            return this;
        }

        /// <summary>
        /// Executes a command with the specified input and an optional access level.
        /// </summary>
        /// <param name="arguments">The parsed input</param>
        /// <param name="accessLevel">
        /// Access level to prevent the command from running unless it is greater than the commands
        /// permission level.
        /// </param>
        public Command Execute(CommandArgument[] arguments, int accessLevel = 0)
        {
            Action.Invoke(arguments);
            return this;
        }

        /// <summary>
        /// Creates the input from a string automatically.
        /// </summary>
        /// <example>
        /// A command such as "Give a player an item X times" could be defined as:
        /// "%lt;player&gt; &lt;item&gt; [amount]"
        /// Where &gt; &lt; represent required items, and [ ] represent optional items.
        /// These tags can also be nested to represent more complex values.
        /// (value) represents a default value for an argument. (Must follow immediately)
        /// </example>
        /// <param name="input">The string with the argument info, see example for more information.</param>
        public Command InferArguments(string input)
        {
            //Trim input and make sure it isn't null
            if (string.IsNullOrEmpty(input)) throw new ArgumentNullException("input");
            input = input.Trim();

            var inputArgs = input.Split(' '); //Split into arguments

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
                int indexDefault = required.Value ? arg.IndexOf(">(", StringComparison.Ordinal) : arg.IndexOf("](", StringComparison.Ordinal);
                var defaultValue = string.Empty;
                if (arg.EndsWith(")") && indexDefault > 1)
                {
                    defaultValue = arg.Substring(indexDefault + 2, end - (indexDefault + 1));
                    end = indexDefault-1;
                }

                var argName = arg.Substring(1, end); //Take all but first and last character to find name
                if (string.IsNullOrEmpty(argName))
                    throw new InvalidOperationException(
                        string.Format("Argument {0} must contain a name within their brackets.", i + 1));
                argName = char.ToUpper(argName[0]) + argName.Substring(1); //Uppercase first to look nicer

                //Create argument
                var cmdArg = new CommandArgument(argName, !required.Value);
                cmdArg.SetDefault(defaultValue);
                AddArgument(cmdArg);
            }

            return this;
        }

        /// <summary>
        /// Add an argument to the command. All input are required by default, and are parsed in the order they are defined.
        /// </summary>
        public Command AddArgument(CommandArgument argument)
        {
            Arguments.Add(argument);
            return this;
        }
    }
}