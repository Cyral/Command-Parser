using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pyratron.Frameworks.Commands.Parser
{
    /// <summary>
    /// Handles and parses commands and their arguments.
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
        /// Creates a new comArgs parser for handling commands.
        /// </summary>
        public static CommandParser CreateNew(string prefix = "/")
        {
            return new CommandParser {Prefix = prefix};
        }

        /// <summary>
        /// Executes the specified comArgs.
        /// An access level can be passed optionally to only execute the comArgs if permission is given.
        /// </summary>
        public CommandParser Execute(Command command, Argument[] arguments)
        {
            command.Execute(arguments);
            return this;
        }

        /// <summary>
        /// Adds a predefined comArgs to the parser.
        /// </summary>
        /// <param name="command">Use Command.CreateNew() to create a comArgs.</param>
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
        /// Parses text in search of a commands (with prefix), and runs it accordingly.
        /// </summary>
        /// <param name="accessLevel">An optional level to limit executing commands if the user doesn't have permission</param>
        public void Parse(string input, int accessLevel = 0)
        {
            if (string.IsNullOrEmpty(input))
                return;

            //Remove the prefix from the input and trim it just in case
            input = input.Trim();
            if (!string.IsNullOrEmpty(Prefix))
            {
                var index = input.IndexOf(Prefix, StringComparison.OrdinalIgnoreCase);
                if (index == -1)
                    return;
                input = input.Remove(index, Prefix.Length);
            }
            if (string.IsNullOrEmpty(input))
                return;

            //Now we are ready to go
            //Split the string into arguments ignoring spaces between quotes
            var inputArgs = Regex.Matches(input, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();

            //Search the commands for a matching command
            var commands = Commands.Where(cmd => cmd.Aliases.Any(alias => alias.Equals(inputArgs[0])));
            if (commands.Count() == 0) //If no commands found found
                NoCommandsFound(inputArgs);
            else
            {
                var command = commands.First(); //Find command

                //Verify that the sender/user has permission to run this command
                if (command.AccessLevel > accessLevel)
                {
                    OnParseError(this,
                        string.Format("Command '{0}' requires permission level {1}. (Currently only {2})", command.Name,
                            command.AccessLevel, accessLevel));
                    return;
                }

                //Verify the command can be run
                var canExecute = command.CanExecute(command);
                if (!string.IsNullOrEmpty(canExecute))
                {
                    OnParseError(this, canExecute);
                    return;
                }

                var returnArgs = new List<Argument>();

                //Validate each argument
                var alias = inputArgs.ElementAt(0).ToLower(); //Preserve the alias typed in
                inputArgs.RemoveAt(0); //Remove the command name
                if (!ParseArguments(false, alias, command, command, inputArgs, returnArgs))
                    command.Execute(returnArgs.ToArray()); //Execute the command

                //Return argument values back to default
                ResetArgs(command);
            }
        }

        /// <summary>
        /// Ran when no commands are found. Will create an error detailing what went wrong.
        /// </summary>
        private void NoCommandsFound(List<string> inputArgs)
        {
            OnParseError(this, string.Format("Command '{0}' not found.", inputArgs[0]));
            //Find related commands (Did you mean?
            var related = FindRelatedCommands(inputArgs[0]);

            if (related.Count > 0)
            {
                var message = FormatRelatedCommands(related);
                OnParseError(this, string.Format("Did you mean: {0}?", message));
            }
        }

        /// <summary>
        /// Takes input from <c>FindRelatedCommands</c> and generates a readable string
        /// </summary>
        private string FormatRelatedCommands(List<string> related)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < related.Count; i++)
            {
                sb.Append('\'').Append(related[i]).Append('\'');
                if (related.Count > 1)
                {
                    if (i == related.Count - 2)
                        sb.Append(", or ");
                    else if (i < related.Count - 1)
                        sb.Append(", ");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Finds command aliases related to the input command that may have been spelled incorrectly
        /// </summary>
        private List<string> FindRelatedCommands(string input)
        {
            var related = new List<string>();
            foreach (var command in Commands)
            {
                foreach (var alias in command.Aliases)
                {
                    if ((alias.StartsWith(input)) //If the user did not complete the command
                        //If the user missed the last few letters
                        || (input.Length >= 2 && alias.StartsWith(input.Substring(0, 2)))
                        //If user missed last few letters
                        || (input.Length > 2 && alias.EndsWith(input.Substring(input.Length - 2, 2)))
                        //If user mispelled middle characters
                        || (alias.StartsWith(input.Substring(0, 1)) && alias.EndsWith(input.Substring(input.Length - 1, 1))))
                    {
                        related.Add(alias);
                        break;
                    }
                }
            }
            return related;
        }

        /// <summary>
        /// Resets the command back to their default values
        /// </summary>
        private void ResetArgs(IArguable command)
        {
            foreach (var arg in command.Arguments)
            {
                arg.ResetValue();
                if (arg.Arguments.Count > 0)
                    ResetArgs(arg);
            }
        }

        /// <summary>
        /// Parses the command's arguments or nested argument and recursively parses their children.
        /// </summary>
        /// <returns>True if an error has occured during parsing and the calling loop should break.</returns>
        private bool ParseArguments(bool recursive, string commandText, Command command, IArguable comArgs,
            List<string> inputArgs,
            List<Argument> returnArgs)
        {
            //For each argument
            for (var i = 0; i < comArgs.Arguments.Count; i++)
            {
                //If the arguments are longer than they should be, merge them into the last one.
                //This way a user does not need quotes for a chat message for example.
                MergeLastArguments(recursive, command, comArgs, inputArgs, i);

                //If there are not enough arguments supplied, handle accordingly
                if (i >= inputArgs.Count)
                {
                    if (comArgs.Arguments[i].Optional) //If optional, we can quit and set a default value
                    {
                        returnArgs.Add(comArgs.Arguments[i].SetValue(string.Empty));
                        break;
                    }
                    //If not optional, show an error with the correct form
                    if (comArgs.Arguments[i].Enum) //Show list of types if enum (instead of argument name)
                        OnParseError(this,
                            string.Format("Invalid arguments, {0} required. Usage: {1}",
                                GenerateEnumArguments(comArgs.Arguments[i]),
                                command.GenerateUsage(commandText)));
                    else
                        OnParseError(this,
                            string.Format("Invalid arguments, '{0}' required. Usage: {1}", comArgs.Arguments[i].Name,
                                command.GenerateUsage(commandText)));
                    return true;
                }

                //If argument is an "enum" (Restricted to certin values), validate it
                if (comArgs.Arguments[i].Enum)
                {
                    //Check if passed value is a match for any of the possible values
                    var passed =
                        comArgs.Arguments[i].Arguments.Any(
                            arg => string.Equals(arg.Name, inputArgs[i], StringComparison.OrdinalIgnoreCase));
                    //If it is not, but there is a default value, use the default value
                    if (!passed && !string.IsNullOrEmpty(comArgs.Arguments[i].Default))
                        inputArgs.Insert(i, comArgs.Arguments[i].Default);
                    else if (!passed) //If it was not found, alert the user, unless it is optional
                    {
                        if (comArgs.Arguments[i].Optional)
                        {
                            if (i != comArgs.Arguments.Count - 1)
                                break;
                        }
                        OnParseError(this,
                            string.Format("Argument '{0}' not recognized. Must be {1}", inputArgs[i].ToLower(),
                                GenerateEnumArguments(comArgs.Arguments[i])));
                        return true;
                    }

                    //Set the argument to the selected "enum" value
                    returnArgs.Add(comArgs.Arguments[i].SetValue(inputArgs[i]));

                    if (comArgs.Arguments[i].Arguments.Count > 0) //Parse its children
                    {
                        //Find the nested arguments
                        var argument =
                            comArgs.Arguments[i].Arguments.FirstOrDefault(
                                arg => string.Equals(arg.Name, inputArgs[i], StringComparison.OrdinalIgnoreCase));
                        if (argument != null)
                        {
                            inputArgs.RemoveAt(0); //Remove the type we parsed
                            return ParseArguments(true, commandText, command, argument, inputArgs, returnArgs);
                        }
                    }
                    break;
                }

                //Check for validation rule
                if (CheckArgumentValidation(comArgs, inputArgs, i)) return true;

                //Set the value from the input argument if no errors were detected
                returnArgs.Add(comArgs.Arguments[i].SetValue(inputArgs[i]));

                //If the next child argument is an "enum" (Only certain values allowed), then remove the current input argument
                if (comArgs.Arguments[i].Arguments.Count > 0 && comArgs.Arguments[i].Arguments[0].Enum)
                    inputArgs.RemoveAt(0);

                //If the argument has nested arguments, parse them recursively
                if (comArgs.Arguments[i].Arguments.Count > 0)
                    return ParseArguments(true, commandText, command, comArgs.Arguments[i], inputArgs, returnArgs);
            }
            return false;
        }

        /// <summary>
        /// Checks the validation of arguments at the specified index.
        /// </summary>
        private bool CheckArgumentValidation(IArguable comArgs, List<string> inputArgs, int index)
        {
            if (!string.IsNullOrEmpty(inputArgs[index]) && !comArgs.Arguments[index].IsValid(inputArgs[index]))
            {
                OnParseError(this,
                    string.Format("Argument '{0}' is invalid. Must be a valid {1}.", comArgs.Arguments[index].Name,
                        comArgs.Arguments[index].Rule.GetError()));
                return true;
            }
            return false;
        }

        /// <summary>
        /// If the arguments are longer than they should be, merge them into the last one.
        /// This way a user does not need quotes for a chat message for example.
        /// </summary>
        private static void MergeLastArguments(bool recursive, Command command, IArguable comArgs, List<string> inputArgs, int i)
        {
            if ((i > 0 || i == comArgs.Arguments.Count - 1) && inputArgs.Count > command.Arguments.Count)
                if (comArgs.Arguments.Count >= 1 &&
                    ((!recursive && !comArgs.Arguments[comArgs.Arguments.Count - 1].Enum) || recursive))
                {
                    var sb = new StringBuilder();
                    for (var j = command.Arguments.Count + (recursive && comArgs.Arguments.Count > 1 ? 1 : 0);
                        j < inputArgs.Count;
                        j++)
                        sb.Append(' ').Append(inputArgs[j]);
                    inputArgs[command.Arguments.Count - (recursive && comArgs.Arguments.Count > 1 ? 0 : 1)] +=
                        sb.ToString();
                }
        }

        /// <summary>
        /// Returns a list of possible values for an enum (type) argument in a readable format.
        /// </summary>
        private static string GenerateEnumArguments(Argument argument)
        {
            if (!argument.Enum) throw new ArgumentException("Argument must be an enum style argument.");

            var sb = new StringBuilder();

            for (var i = 0; i < argument.Arguments.Count; i++)
            {
                var arg = argument.Arguments[i];
                //Write name
                sb.Append("'");
                sb.Append(arg.Name);
                sb.Append("'");
                //Add comma and "or" if needed
                if (argument.Arguments.Count > 1)
                {
                    if (i == argument.Arguments.Count - 2)
                        sb.Append(", or ");
                    else if (i < argument.Arguments.Count - 1)
                        sb.Append(", ");
                }
            }

            return sb.ToString();
        }
    }
}