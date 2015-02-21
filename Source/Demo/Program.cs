using System;
using System.Collections.Generic;
using Pyratron.Frameworks.Commands.Parser;

namespace Demo
{
    /// <summary>
    /// Runs a demo application to showcase the Pyratron Command Parser Framework.
    /// </summary>
    public static class Program
    {
        public static CommandParser Parser { get; private set; }

        public static void Main(string[] args)
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
                //Action to be executed when command is ran with correct parameters (Of course, can be method, lamba, delegate, etc)
                .AddArgument(Argument //Add an argument
                    .Create("User")));

            //Method B: Add a command via the standard interface:
            Parser.Commands.Add(new Command("Unban User")
            {
                Aliases = new List<string> {"unban", "unbanuser"},
                Description = "Unbans a user from the server.",
                Action = OnUnbanExecuted,
                Arguments = new List<Argument>
                {
                    new Argument("User")
                }
            });

            //Tip 1: Automatically create arguments
            Parser.AddCommand(Command
                .Create("Give Item")
                .AddAlias("item", "giveitem", "give") //Aliases (Note multiple at a time!)
                .SetDescription("Gives a user an item.")
                .SetAction(OnGiveExecuted)
                .AddArguments(Argument
                    .InferArguments("<user> <item> [amount](10)"))
                //Watch how it will automatically create these parameters!
                .RestrictAccess(10)); //User must have 10 permission power to run command

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

            //Tip 2: Convert from premade arguments to argument string
            Console.WriteLine(Parser.Commands[2].Arguments.GenerateArgumentString());

            //Tip 2: Generate helpful command usage
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
            var type = args.ArgumentFromName("type").Value;
            if (type == "read")
                Console.WriteLine("No new mail!");
            else if (type == "clear")
                Console.WriteLine("Mail cleared!");
            else if (type == "send")
            {
                var user = args.ArgumentFromName("user").Value;
                var message = args.ArgumentFromName("message").Value;

                Console.WriteLine("{0} has been sent the message: {1}", user, message);
            }
            else
                Console.WriteLine("Welcome to the mail system!");
        }

        private static void OnGiveExecuted(Argument[] args)
        {
            var user = args.ArgumentFromName("user").Value;
            var item = args.ArgumentFromName("item").Value;
            var amount = args.ArgumentFromName("amount").Value;
            Console.WriteLine("User {0} was given {1} of {2}", user, amount, item);
        }

        private static void OnUnbanExecuted(Argument[] args)
        {
            var user = args.ArgumentFromName("user").Value;
            Console.WriteLine("User {0} was unbanned!", user);
        }

        private static void OnBanExecuted(Argument[] args)
        {
            var user = args.ArgumentFromName("user").Value;
            Console.WriteLine("User {0} was banned!", user);
        }

        private static void OnParseError(object sender, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}