using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Pyratron.Frameworks.Commands.Parser
{
    public class Command : IArguable
    {
        /// <summary>
        /// The permission level needed to invoke the command.
        /// Useful for disabling commands for different "ranks".
        /// </summary>
        public int AccessLevel { get; set; }

        /// <summary>
        /// An action to be executed when the command is ran with successful input.
        /// </summary>
        public Action<Argument[]> Action { get; set; }

        /// <summary>
        /// The strings that will call the command.
        /// </summary>
        public List<string> Aliases { get; set; }

        /// <summary>
        /// The input (Including alias and help) that are passed with the command.
        /// </summary>
        public List<Argument> Arguments { get; set; }

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
            Arguments = new List<Argument>();
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
        /// Action to be ran, which takes a <c>Argument</c> array parameter representing the passes
        /// input.
        /// </param>
        public Command SetAction(Action<Argument[]> action)
        {
            Action = action;
            return this;
        }

        /// <summary>
        /// Executes a command with the specified input and an optional access level.
        /// </summary>
        /// <param name="arguments">The parsed input</param>
        public Command Execute(Argument[] arguments)
        {
            Action.Invoke(arguments);
            return this;
        }

        /// <summary>
        /// Add an argument to the command. All input are required by default, and are parsed in the order they are defined.
        /// </summary>
        public Command AddArgument(Argument argument)
        {
            Arguments.Add(argument);
            return this;
        }

        /// <summary>
        /// Adds an array of arguments to the command. Optional arguments must come last.
        /// </summary>
        public  Command AddArguments(Argument[] arguments)
        {
            bool optional = false;
            foreach (var arg in arguments)
            {
                if (arg.Optional)
                    optional = true;
                else if (optional)
                    throw  new InvalidOperationException("Optional arguments must come last.");
            }

            Arguments.AddRange(arguments);
            return this;
        }
    }
}