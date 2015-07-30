package com.pyratron.frameworks.commands.demo;

import com.pyratron.frameworks.commands.parser.Argument;
import com.pyratron.frameworks.commands.parser.Command;
import com.pyratron.frameworks.commands.parser.CommandParser;
import com.pyratron.frameworks.commands.parser.ValidationRule;

import java.util.ArrayList;
import java.util.List;
import java.util.Scanner;

/**
 * Runs a demo application to showcase the Pyratron Command Parser Framework.
 */
public class Program {
    private static boolean banned;
    private static CommandParser parser;

    public static void main(String[] progArgs) {
        System.out.println("Welcome to the Pyratron Command Parser Framework Demo\nProgram.java in the Demo module contains a short tutorial and many examples.\nType 'list' for commands.\n");
        System.out.println("Additional Information: https://www.pyratron.com/projects/command-parser");

        /* --------
         * Tutorial
		 * -------- */

        //Create a command parser instance.
        //Each instance can be built upon right away by using .addCommand(..)
        parser = CommandParser.createNew().usePrefix("").onError(message -> onParseError(message));

        //Add a command via the fluent interface:
        parser.addCommand(Command
                .create("Ban User") //Friendly Name.
                .addAlias("ban") //Aliases
                .addAlias("banuser")
                .setDescription("Bans a user from the server.") //Description.
                .setAction((args, data) -> onBanExecuted(args)) //Action to be executed when command is ran with correct parameters. (Of course, can be method, lambda, delegate, etc)
                .setExecutePredicate(canExecute -> //Precondition to be checked before executing the command.
                {
                    //In reality this logic would be more complex but this is just an example.
                    if (banned) {
                        return "You are already banned!";
                    }
                    return "";
                }).addArgument(Argument
                        .create("User"))); //Add an argument.

        /* -----------------
         * Example Commands
         * ---------------- */
        parser.addCommand(Command
                .create("Give Item")
                .addAlias("item", "giveitem", "give")  //Note multiple aliases.
                .setDescription("Gives a user an item.")
                .setAction((args, data) -> onGiveExecuted(args))
                .addArgument(Argument.create("user"))
                .addArgument(Argument.create("item"))
                .addArgument(Argument.create("amount")
                        .makeOptional()
                        .setDefault(10))); //Default value if argument not present.

        parser.addCommand(Command
                .create("Register")
                .addAlias("register")
                .setDescription("Create an account")
                .setAction((args, data) -> //Note inline action.
                {
                    String user = Argument.fromName(args, "username");
                    String email = Argument.fromName(args, "email");
                    System.out.printf("%1$s (%2$s) has registered." + "\r\n", user, email);
                })
                .addArgument(Argument
                        .create("username")
                        .setValidator(ValidationRule.AlphaNumerical)) //Note argument validator.
                .addArgument(Argument.create("password"))
                .addArgument(Argument.create("email")
                        .setValidator(ValidationRule.Email)));

        parser.addCommand(Command
                .create("Mail")
                .addAlias("mail")
                .setDescription("Allows users to send messages.")
                .setAction((args, data) -> onMailExecuted(args))
                .addArgument(Argument.create("type") //Note a deep nesting of arguments.
                        .makeOptional() //Note command options. These turn the "type" argument into an enum style list of values.
                        .addOption(Argument //The user has to type read, clear, or send (Which has it's own nested arguments).
                                .create("read"))
                        .addOption(Argument
                                .create("clear"))
                        .addOption(Argument
                                .create("send")
                                .addArgument((Argument
                                        .create("user")))
                                .addArgument((Argument
                                        .create("message"))))));

        parser.addCommand(Command
                .create("Godmode")
                .addAlias("god", "godmode")
                .setDescription("Disables or enables godmode.")
                .setAction((args, data) ->
                        System.out.printf("Godmode turned %1$s for %2$s" + "\r\n", Argument
                                .fromName(args, "status"), Argument
                                .fromName(args, "player")))
                .addArgument(Argument.create("player") //Again, a complex hierarchy that results in: godmode [player <on|off>]
                        .makeOptional()
                        .setDefault("User")
                        .addArgument(Argument
                                .create("status")
                                .makeOptional()
                                .setDefault("on")
                                .addOption(Argument
                                        .create("on"))
                                .addOption(Argument
                                        .create("off")))));

        parser.addCommand(Command
                .create("Command List")
                .addAlias("list", "commands")
                .setDescription("Lists commands")
                .setAction((args, data) ->
                        parser.getCommands().stream().forEachOrdered(command -> System.out.println(command.showHelp())))); //Listing of commands

        //Another advanced command
        parser.addCommand(Command.create("Worth").addAlias("worth").setDescription("Item worth").setAction((args, data) ->
        {
            String type = Argument.fromName(args, "type");
            if (type.equals("hand"))
                System.out.println("Items in hand worth: $10");
            else if (type.equals("all"))
                System.out.println("All your items worth: $100");
            else if (type.equals("item"))
                System.out.printf("%2$s of %1$s is worth $%3$s" + "\r\n", Argument.fromName(args, "itemname"), Argument.fromName(args, "amount"), Integer.parseInt(Argument.fromName(args, "amount")) * 10);
        }).addArgument(Argument
                .create("type")
                .addOption(Argument
                        .create("hand"))
                .addOption(Argument
                        .create("all"))
                .addOption(Argument
                        .create("item")
                        .makeOptional()
                        .addArgument(Argument
                                .create("itemname"))
                        .addArgument(Argument
                                .create("amount")
                                .makeOptional()
                                .setDefault("10")))
                .setDefault("item"))); //Note use of default value.

        /* --------
         *   Tips
         * -------- */

        //Tip: Show command help
        System.out.println(parser.getCommands().get(3).showHelp());

        //Tip: Generate helpful command usage
        System.out.println(parser.getCommands().get(4).generateUsage());


        /* --------
         *   Demo
         * -------- */
        System.out.println("\nEnter command:\n");
        Scanner scanner = new Scanner(System.in);
        while (true) {
            System.out.print("$ ");

            //Read input and parse command
            String input = scanner.nextLine();
            parser.parse(input);
        }
    }

    private static void askStartGame(ArrayList<Argument> args) {
        for (int i = 0; i < args.size(); i++) {
            System.out.println("- " + args.get(i).getName() + " - " + Argument.fromName(args, args.get(i).getName()));
        }
    }

    private static void onMailExecuted(List<Argument> args) {
        String type = Argument.fromName(args, "type");
        switch (type) {
            case "read":
                System.out.println("No new mail!");
                break;
            case "clear":
                System.out.println("Mail cleared!");
                break;
            case "send":
                String user = Argument.fromName(args, "user");
                String message = Argument.fromName(args, "message");
                System.out.printf("%1$s has been sent the message: %2$s" + "\r\n", user, message);
                break;
            default:
                System.out.println("Welcome to the mail system!");
                break;
        }
    }

    private static void onGiveExecuted(List<Argument> args) {
        String user = Argument.fromName(args, "user");
        String item = Argument.fromName(args, "item");
        String amount = Argument.fromName(args, "amount");
        System.out.printf("User %1$s was given %2$s of %3$s" + "\r\n", user, amount, item);
    }

    private static void onBanExecuted(List<Argument> args) {
        String user = Argument.fromName(args, "user");
        System.out.printf("User %1$s was banned!" + "\r\n", user);
        banned = true;
    }

    private static void onParseError(String message) {
        //Print error
        System.out.println(message);
    }
}
