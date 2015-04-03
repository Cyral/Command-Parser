using System;
using System.Collections.Generic;
using System.Diagnostics;
using Pyratron.Frameworks.Commands.Parser;

namespace Pyratron.Frameworks.Commands.Demo
{
    /// <summary>
    /// Runs a demo application to showcase the Pyratron Command Parser Framework.
    /// </summary>
    public static class Program
    {
        private static bool banned;
        private static CommandParser Parser { get; set; }

        /// <summary>
        /// A sample demo and tutorial.
        /// </summary>
        public static void Main()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(
                "Welcome to the Pyratron Command Parser Framework Demo\nProgram.cs in the Demo project. contains a short tutorial and many examples.\nType 'list' for commands.\n");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Additional Information: https://www.pyratron.com/projects/command-parser");
            Console.ForegroundColor = ConsoleColor.Gray;

            /* --------
             * Tutorial
             * -------- */

            //Create a command parser instance.
            //Each instance can be built upon right away by using .AddCommand(..)
            Parser = CommandParser.CreateNew().UsePrefix(string.Empty).OnError(OnParseError);

            //Method A: Add a command via the fluent interface:
            Parser.AddCommand(Command
                .Create("Ban User") //Friendly Name.
                .AddAlias("ban") //Aliases.
                .AddAlias("banuser")
                .SetDescription("Bans a user from the server.") //Description.
                //Action to be executed when command is ran with correct parameters. (Of course, can be method, lamba, delegate, etc)
                .SetAction(OnBanExecuted)
                //Precondition to be checked before executing the command.
                .SetExecutePredicate(delegate
                {
                    if (banned)
                        return "You are already banned!";
                    //In reality this logic would be more complex but this is just an example.
                    return string.Empty;
                })
                .AddArgument(Argument //Add an argument.
                    .Create("User")));

            //Method B: Add a command via the standard interface:
            Parser.Commands.Add(new Command("Unban User")
            {
                Aliases = new List<string> {"unban", "unbanuser"},
                Description = "Unbans a user from the server.",
                Action = OnUnbanExecuted,
                CanExecute = (delegate
                {
                    if (!banned) return "You are not banned!";
                    return string.Empty;
                }),
                Arguments = new List<Argument>
                {
                    new Argument("User")
                }
            });

            /* -----------------
             * Example Commands
             * ---------------- */
            Parser.AddCommand(Command
                .Create("Give Item")
                .AddAlias("item", "giveitem", "give") //Note multiple aliases.
                .SetDescription("Gives a user an item.")
                .SetAction(OnGiveExecuted)
                .AddArgument(Argument
                    .Create("user"))
                .AddArgument(Argument
                    .Create("item"))
                .AddArgument(Argument
                    .Create("amount")
                    .MakeOptional()
                    .SetDefault(10)));

            Parser.AddCommand(Command.Create("Register").AddAlias("register").SetDescription("Create an account")
                //Note inline action.
                .SetAction(arguments =>
                {
                    var user = arguments.FromName("username");
                    var email = arguments.FromName("email");
                    Console.WriteLine("{0} ({1}) has registered.", user, email);
                })
                //Note argument validator.
                .AddArgument(Argument.Create("username").SetValidator(Argument.ValidationRule.AlphaNumerical))
                .AddArgument(Argument.Create("password"))
                .AddArgument(Argument.Create("email").SetValidator(Argument.ValidationRule.Email)));

            Parser.AddCommand(Command
                .Create("Mail")
                .AddAlias("mail")
                .SetDescription("Allows users to send messages.")
                .SetAction(OnMailExecuted)
                //Note a deep nesting of arguments.
                .AddArgument(Argument
                    .Create("type")
                    .MakeOptional()
                    //Note command options. These turn the "type" argument into an enum style list of values.
                    //The user has to type read, clear, or send (Which has it's own nested arguments).
                    .AddOption(Argument.Create("read"))
                    .AddOption(Argument.Create("clear"))
                    .AddOption(Argument.Create("send")
                        .AddArgument((Argument.Create("user")))
                        .AddArgument((Argument.Create("message"))))));

            Parser.AddCommand(Command
                .Create("Godmode")
                .AddAlias("god", "godmode")
                .SetDescription("Disables or enables godmode.")
                .SetAction(
                    delegate(Argument[] arguments)
                    {
                        Console.WriteLine("Godmode turned {0} for {1}", arguments.FromName("status"),
                            arguments.FromName("player"));
                    })
                //Again, a complex hierarchy that results in: godmode [player <on|off]
                .AddArgument(Argument
                    .Create("player")
                    .MakeOptional()
                    .SetDefault("User")
                    .AddArgument(Argument
                        .Create("status")
                        .MakeOptional()
                        .SetDefault("on")
                        .AddOption(Argument.Create("on"))
                        .AddOption(Argument.Create("off")))));

            Parser.AddCommand(Command //Listing of commands
                .Create("Command List")
                .AddAlias("list", "commands")
                .SetDescription("Lists commands")
                .SetAction(delegate
                {
                    foreach (var command in Parser.Commands)
                        Console.WriteLine(command.ShowHelp());
                }));

            Parser.AddCommand(Command
                .Create("Worth") //Another advanced command
                .AddAlias("worth")
                .SetDescription("Item worth")
                .SetAction(
                    delegate(Argument[] arguments)
                    {
                        var type = arguments.FromName("type");
                        if (type == "hand")
                            Console.WriteLine("Items in hand worth: $10");
                        else if (type == "all")
                            Console.WriteLine("All your items worth: $100");
                        else if (type == "item")
                            Console.WriteLine("{1} of {0} is worth ${2}", arguments.FromName("itemname"),
                                arguments.FromName("amount"),
                                int.Parse(arguments.FromName("amount")) * 10);
                    })
                .AddArgument(Argument
                    .Create("type")
                    .AddOption(Argument.Create("hand"))
                    .AddOption(Argument.Create("all"))
                    .AddOption(Argument
                        .Create("item")
                        .MakeOptional()
                        .AddArgument(Argument
                            .Create("itemname"))
                        .AddArgument(Argument
                            .Create("amount")
                            .MakeOptional()
                            .SetDefault("10"))) //Note use of default value
                    .SetDefault("item")));

            /* --------
             *   Tips
             * -------- */

            //Tip: Show command help
            Debug.WriteLine(Parser.Commands[3].ShowHelp());

            //Tip: Generate helpful command usage
            Debug.WriteLine(Parser.Commands[4].GenerateUsage());

            /* --------
             *   Demo
             * -------- */
            Console.WriteLine("\nEnter command:\n");
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("$ ");
                Console.ForegroundColor = ConsoleColor.Gray;

                //Read input and parse command
                var input = Console.ReadLine();
                Parser.Parse(input);
            }
        }

        private static void OnMailExecuted(Argument[] args)
        {
            var type = args.FromName("type");
            switch (type)
            {
                case "read":
                    Console.WriteLine("No new mail!");
                    break;
                case "clear":
                    Console.WriteLine("Mail cleared!");
                    break;
                case "send":
                    var user = args.FromName("user");
                    var message = args.FromName("message");

                    Console.WriteLine("{0} has been sent the message: {1}", user, message);
                    break;
                default:
                    Console.WriteLine("Welcome to the mail system!");
                    break;
            }
        }

        private static void OnGiveExecuted(Argument[] args)
        {
            var user = args.FromName("user");
            var item = args.FromName("item");
            var amount = args.FromName("amount");
            Console.WriteLine("User {0} was given {1} of {2}", user, amount, item);
        }

        private static void OnUnbanExecuted(Argument[] args)
        {
            var user = args.FromName("user");
            Console.WriteLine("User {0} was unbanned!", user);
            banned = false;
        }

        private static void OnBanExecuted(Argument[] args)
        {
            var user = args.FromName("user");
            Console.WriteLine("User {0} was banned!", user);
            banned = true;
        }

        private static void OnParseError(object sender, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}