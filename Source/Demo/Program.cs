using System;
using System.Collections.Generic;
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

        public static void Main()
        {
            //Create a command parser instance
            //Each instance can be built upon right away by using .AddCommand(..)
            Parser = CommandParser.CreateNew().UsePrefix(string.Empty).OnError(OnParseError);

            //Method A: Add a command via the fluent interface:
            Parser.AddCommand(Command
                .Create("Ban User") //Friendly Name
                .AddAlias("ban") //Aliases
                .AddAlias("banuser")
                .SetDescription("Bans a user from the server.") //Description
                .SetAction(OnBanExecuted)
                .SetExecutePredicate(delegate
                {
                    if (banned) return "You are already banned!";
                    return string.Empty;
                })
                //Action to be executed when command is ran with correct parameters (Of course, can be method, lamba, delegate, etc)
                .AddArgument(Argument //Add an argument
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

            //Example Commands
            Parser.AddCommand(Command
                .Create("Give Item")
                .AddAlias("item", "giveitem", "give") //Aliases (Note multiple at a time!)
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
                .SetAction(arguments =>
                {
                    var user = arguments.FromName("username");
                    var email = arguments.FromName("email");
                    Console.WriteLine("{0} ({1}) has registered.", user, email);
                })
                .AddArgument(Argument.Create("username").SetValidator(Argument.ValidationRule.Alphanumerical))
                .AddArgument(Argument.Create("password"))
                .AddArgument(Argument.Create("email").SetValidator(Argument.ValidationRule.Email)));

            Parser.AddCommand(Command
                .Create("Mail")
                .AddAlias("mail") //Aliases (Note multiple at a time!)
                .SetDescription("Allows users to send messages.")
                .SetAction(OnMailExecuted)
                .AddArgument(Argument //Add an argument
                    .Create("type")
                    .MakeOptional()
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
                .AddArgument(Argument
                    .Create("player")
                    .SetDefault("User")
                    .AddArgument(Argument
                        .Create("status")
                        .MakeOptional()
                        .SetDefault("on")
                        .AddOption(Argument.Create("on"))
                        .AddOption(Argument.Create("off")))));

            Parser.AddCommand(Command
                .Create("Rain")
                .AddAlias("rain", "storm")
                .SetDescription("Disables or enables rain.")
                .SetAction(
                    delegate(Argument[] arguments)
                    {
                        Console.WriteLine("Rain turned {0}", arguments.FromName("type"));
                    })
                .AddArgument(Argument
                    .Create("type")
                    .MakeOptional()
                    .SetDefault("on")
                    .AddOption(Argument.Create("on").AddArgument(Argument.Create("duration")))
                    .AddOption(Argument.Create("off"))));

            Parser.AddCommand(Command
                .Create("Ban IP")
                .AddAlias("banip")
                .SetDescription("Bans a player by IP")
                .SetAction(
                    delegate(Argument[] arguments) { Console.WriteLine("Player banned: {0}", arguments[0]); })
                .AddArgument(Argument
                    .Create("player name|IP address")));

            Parser.AddCommand(Command
                .Create("Worth")
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
                                int.Parse(arguments.FromName("amount"))*10);
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
                            .SetDefault("10")
                            .MakeOptional()))
                    .SetDefault("item")));


            //Tip: Generate helpful command usage
            Console.WriteLine(Parser.GenerateUsage(Parser.Commands[2]));

            Console.WriteLine("Enter command:\n");
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("$ ");
                Console.ForegroundColor = ConsoleColor.Gray;

                //Read input and parse command
                var input = Console.ReadLine();
                Parser.Parse(input, 10);
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