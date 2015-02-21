using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pyratron.Frameworks.Commands.Parser
{
    /// <summary>
    /// Handles commands and lists the command instances.
    /// </summary>
    public class CommandParser
    {
        #region Delegates

        public delegate void ParseErrorHandler(object sender, string error);

        #endregion

        /// <summary>
        /// All of the commands in the parser.
        /// </summary>
        public List<Command> Commands { get; set; }

        /// <summary>
        /// Fired when an error occurs during parsing. Details on the error are returned.
        /// </summary>
        public ParseErrorHandler ParseError { get; private set; }

        /// <summary>
        /// The prefix, "/" by default, that all commands must be prefixed with.
        /// Prefix is case-insensitive.
        /// </summary>
        public string Prefix { get; set; }

        private CommandParser()
        {
            Commands = new List<Command>();
        }

        private void OnParseError(object sender, string error)
        {
            var handler = ParseError;
            if (handler != null) handler(sender, error);
        }

        /// <summary>
        /// Creates a new command parser for handling commands.
        /// </summary>
        public static CommandParser CreateNew(string prefix = "/")
        {
            return new CommandParser {Prefix = prefix};
        }

        /// <summary>
        /// Executes the specified command.
        /// An access level can be passed optionally to only execute the command if permission is given.
        /// </summary>
        public CommandParser Execute(Command command, CommandArgument[] arguments, int accessLevel = 0)
        {
            command.Execute(arguments, accessLevel);
            return this;
        }

        /// <summary>
        /// Adds a predefined command to the parser.
        /// </summary>
        /// <param name="command">Use Command.CreateNew() to create a command.</param>
        public CommandParser AddCommand(Command command)
        {
            Commands.Add(command);
            return this;
        }

        /// <summary>
        /// Sets the prefix that the parser will use to identity commands. Defaults to "/".
        /// </summary>
        public CommandParser UsePrefix(string prefix = "/")
        {
            Prefix = prefix;
            return this;
        }

        /// <summary>
        /// Sets an action to be ran when an error is encountered during parsing.
        /// </summary>
        public CommandParser OnError(Action<object, string> callback)
        {
            ParseError += new ParseErrorHandler(callback);
            return this;
        }

        /// <summary>
        /// Generates help text defining the usage of a command
        /// </summary>
        /// <param name="alias">
        /// Custom alias to use in the message. (Example, if user inputs "banuser" as an alias, but the real
        /// command is "ban", make sure we use the alias)
        /// </param>
        public string GenerateUsage(Command command, string alias = "")
        {
            var sb = new StringBuilder();
            if (command.Aliases.Count <= 0) return string.Empty;
            sb.Append(Prefix);
            sb.Append(string.IsNullOrEmpty(alias) ? command.Aliases[0] : alias.ToLower());
            sb.Append(' ');
            sb.Append(command.Arguments.GenerateArgumentString());

            return sb.ToString();
        }

        /// <summary>
        /// Parses text in search of a command (with prefix), and runs it accordingly.
        /// </summary>
        public void Parse(string input)
        {
            //Remove the prefix from the input and trim it just in case
            input = input.Trim();
            if (!string.IsNullOrEmpty(Prefix))
            {
                var index = input.IndexOf(Prefix, StringComparison.OrdinalIgnoreCase);
                if (index == -1)
                    return;
                input = input.Substring(index, Prefix.Length);
            }
            if (string.IsNullOrEmpty(input))
                return;

            //Now we are ready to go
            //Split the string into arguments
            var inputArgs = input.Split(' ');

            //Search the commands for a matching command
            var commands = Commands.Where(cmd => cmd.Aliases.Any(alias => alias.Equals(inputArgs[0])));
            if (commands.Count() == 0) //If no command found
                OnParseError(this, string.Format("Command '{0}' not found.", inputArgs[0]));
            else
            {
                var command = commands.First();
                var returnArgs = new CommandArgument[command.Arguments.Count];

                //Validate each command argument
                for (var i = 0; i < command.Arguments.Count; i++)
                {
                    if (inputArgs.Count() - 1 <= i) //If there are not enough arguments supplied
                    {
                        if (command.Arguments[i].Optional) //If optional, we can quit and set a default value
                        {
                            returnArgs[i] = command.Arguments[i];
                            returnArgs[i].SetValue(string.Empty);
                            break;
                        }
                        OnParseError(this,
                            string.Format("Invalid arguments, '{0}' required. Usage: {1}", command.Arguments[i].Name,
                                GenerateUsage(command, inputArgs[0])));
                        return;
                    }
                    //Set the value from the input
                    returnArgs[i] = command.Arguments[i];
                    returnArgs[i].SetValue(inputArgs[i + 1]);
                }

                command.Execute(returnArgs, 0);
            }
        }
    }
}