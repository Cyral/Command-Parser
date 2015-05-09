package com.pyratron.frameworks.commands.parser;

import java.util.ArrayList;
import java.util.Optional;
import java.util.function.Consumer;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import java.util.stream.Collectors;

/**
 * Handles and parses commands and their arguments.
 * create a new parser with:
 * <pre>parser = CommandParser.createNew().usePrefix("").onError(message -&gt; onParseError(message));</pre>
 * Send commands to the parser with:
 * Parser.Parse(input);
 */
public class CommandParser {
    private ArrayList<Command> commands;
    private String prefix;
    private Consumer<String> parseError;

    private CommandParser() {
        prefix = "/";
        commands = new ArrayList<>();
    }

    private CommandParser(String prefix) {
        this.prefix = prefix;
        commands = new ArrayList<>();
    }

    /**
     * Creates a new command parser for handling commands.
     */
    public static CommandParser createNew() {
        return new CommandParser();
    }

    /**
     * Creates a new command parser with the specified prefix.
     *
     * @param prefix The prefix that the parser will use to identity commands. Defaults to "/".
     */
    public static CommandParser createNew(String prefix) {
        return new CommandParser(prefix);
    }

    /**
     * Executes the command with the specified arguments.
     */
    public final CommandParser execute(Command command, ArrayList<Argument> arguments) {
        command.execute(arguments);
        return this;
    }

    /**
     * Adds a predefined command to the parser.
     *
     * @param command The command to execute. Use <pre>Command.create()</pre> to create a command.
     */
    public final CommandParser addCommand(Command command) {
        commands.add(command);
        return this;
    }

    /**
     * Sets the prefix that the parser will use to identity commands. Default is "/".
     */
    public final CommandParser usePrefix(String prefix) {
        this.prefix = prefix;
        return this;
    }

    /**
     * Sets an action to be ran when an error is encountered during parsing.
     * Details on the error are returned by the callback.
     * <p>
     * Ideally used to display an error message if the command entered encounters an error.
     */
    public final CommandParser onError(Consumer<String> callback) {
        parseError = callback;
        return this;
    }

    /**
     * Parses text in search of a command (with prefix), and runs it accordingly.
     * <p>
     * Data does not need to be formatted in any way before parsing. Simply pass your input to the function and
     * it will determine if it is a valid command, check the command's <pre>Command.CanExecute</pre> function, and run the
     * command.
     * Use <pre>Arguments[].FromName(...)</pre> to get the values of the parsed arguments in the command action.
     *
     * @param input A string inputted by a user. If the string does not start with the parser prefix, it will return false, otherwise it will parse the command.
     * @return True if the input is non-empty and starts with the <pre>Prefix</pre>.
     * If the input does not start with a prefix, it returns false so the message can be processed further. (As a chat message, for example)
     */
    public final boolean parse(String input) {
        return parse(input, 0);
    }

    /**
     * Parses text in search of a command (with prefix), and runs it accordingly.
     * <p>
     * Data does not need to be formatted in any way before parsing. Simply pass your input to the function and
     * it will determine if it is a valid command, check the command's <pre>Command.CanExecute</pre> function, and run the
     * command.
     * Use <pre>Arguments[].FromName(...)</pre> to get the values of the parsed arguments in the command action.
     *
     * @param input       A string inputted by a user. If the string does not start with the parser prefix, it will return false, otherwise it will parse the command.
     * @param accessLevel An optional level to limit executing commands if the user doesn't have permission.
     * @return True if the input is non-empty and starts with the <pre>Prefix</pre>.
     * If the input does not start with a prefix, it returns false so the message can be processed further. (As a chat message, for example)
     */
    public final boolean parse(String input, int accessLevel) {
        if (input.equals(""))
            return false;

        //Remove the prefix from the input and trim it just in case.
        input = input.trim();
        if (!prefix.equals("")) {
            int index = input.toLowerCase().indexOf(prefix.toLowerCase());
            if (index == -1)
                return false;
            input = input.substring(index, prefix.length());
        }
        if (input.equals(""))
            return false;

        //Now we are ready to go.
        //Split the string into arguments ignoring spaces between quotes.
        ArrayList<String> inputArgs = new ArrayList<>();
        Matcher m = Pattern.compile("(?<match>[^\\s\"]+)|(?<match2>\"[^\"]*\")")
                .matcher(input);
        while (m.find()) {
            if (m.group("match2") != null && !m.group("match2").equals(""))
                inputArgs.add(m.group("match2"));
            else
                inputArgs.add(m.group("match"));
        }

        ArrayList<Command> matchCommands = commands.stream().filter(cmd -> cmd.getAliases().stream().anyMatch(alias -> alias.equalsIgnoreCase(inputArgs.get(0)))).collect(Collectors.toCollection(ArrayList::new));

        if (matchCommands.isEmpty()) //If no commands found found.
            noCommandsFound(inputArgs);
        else {

            Command command = matchCommands.get(0); //Find command.

            //Verify that the sender/user has permission to run this command.
            if (command.getAccessLevel() > accessLevel) {
                onParseError(String.format("Command '%1$s' requires permission level %2$s. (Currently only %3$s)", command.getName(), command.getAccessLevel(), accessLevel));
                return true;
            }

            //Verify the command can be run.
            String canExecute = command.canExecute(command);
            if (!canExecute.equals("")) {
                onParseError(canExecute);
                return true;
            }

            ArrayList<Argument> returnArgs = new ArrayList<>();

            //Validate each argument.
            String alias = inputArgs.get(0).toLowerCase(); //Preserve the alias typed in.
            inputArgs.remove(0); //Remove the command name.
            if (!parseArguments(false, alias, command, command, inputArgs, returnArgs))
                command.execute(returnArgs); //Execute the command.

            //Return argument values back to default.
            resetArgs(command);
        }
        return true;
    }

    /**
     * Parses the command's arguments or nested argument and recursively parses their children.
     *
     * @return True if an error has occurred during parsing and the calling loop should break.
     */
    private boolean parseArguments(boolean recursive, String commandText, Command command, IArguable comArgs, ArrayList<String> inputArgs, ArrayList<Argument> returnArgs) {
        //For each argument
        for (int i = 0; i < comArgs.getArguments().size(); i++) {
            //If the arguments are longer than they should be, merge them into the last one.
            //This way a user does not need quotes for a chat message for example.
            mergeLastArguments(recursive, command, comArgs, inputArgs, i);

            //If there are not enough arguments supplied, handle accordingly.
            if (i >= inputArgs.size()) {
                if (comArgs.getArguments().get(i).isOptional()) //If optional, we can quit and set a default value.
                {
                    returnArgs.add(comArgs.getArguments().get(i).setValue(""));
                    break;
                }
                //If not optional, show an error with the correct form.
                if (comArgs.getArguments().get(0).isEnum()) //Show list of types if enum (instead of argument name).
                    onParseError(String.format("Invalid arguments, %1$s required. Usage: %2$s", generateEnumArguments(comArgs.getArguments().get(i)), command.generateUsage(commandText)));
                else
                    onParseError(String.format("Invalid arguments, '%1$s' required. Usage: %2$s", comArgs.getArguments().get(i).getName(), command.generateUsage(commandText)));
                return true;
            }

            //If argument is an "enum" (Restricted to certain values), validate it.
            if (comArgs.getArguments().get(i).isEnum()) {
                //Check if passed value is a match for any of the possible values.
                final int finalIndex = i;
                boolean passed = comArgs.getArguments().get(i).getArguments().stream().anyMatch(arg -> arg.getName().equalsIgnoreCase(inputArgs.get(finalIndex)));
                if (!passed) //If it was not found, alert the user, unless it is optional.
                {
                    if (comArgs.getArguments().get(i).isOptional())
                        if (i != comArgs.getArguments().size() - 1)
                            break;
                    onParseError(String.format("Argument '%1$s' not recognized. Must be %2$s", inputArgs.get(i).toLowerCase(), generateEnumArguments(comArgs.getArguments().get(i))));
                    return true;
                }

                //Set the argument to the selected "enum" value.
                returnArgs.add(comArgs.getArguments().get(i).setValue(inputArgs.get(i)));

                if (comArgs.getArguments().get(i).getArguments().size() > 0) //Parse its children.
                {
                    //Find the nested arguments.
                    Optional<Argument> argument =
                            comArgs.getArguments().get(i).getArguments().stream().filter(
                                    arg -> arg.getName().equalsIgnoreCase(inputArgs.get(finalIndex))).findFirst();
                    if (argument.isPresent()) {
                        inputArgs.remove(0); //Remove the type we parsed.
                        //Parse the value, to validate it
                        if (parseArguments(true, commandText, command, argument.get(), inputArgs, returnArgs))
                            return true;
                        if (i == comArgs.getArguments().size() - 1)  //If last argument, break, as no more input is expected
                            break;
                        inputArgs.add(0, ""); //Insert dummy data to fill inputArgs
                        //Now that the enum arg has been parsed, parse the remaining input, if any.
                    }
                }
                continue;
            }

            //Check for validation rule.
            if (checkArgumentValidation(comArgs, inputArgs, i)) {
                return true;
            }

            //Set the value from the input argument if no errors were detected.
            returnArgs.add(comArgs.getArguments().get(i).setValue(inputArgs.get(i)));

            //If the next child argument is an "enum" (Only certain values allowed), then remove the current input argument.
            if ((comArgs.getArguments().get(i).isOptional() && comArgs.getArguments().get(i).getArguments().size() > 0 && !comArgs.getArguments().get(i).getArguments().get(0).isEnum()) || (comArgs.getArguments().get(i).getArguments().size() > 0 && comArgs.getArguments().get(i).getArguments().get(i).isEnum())) {
                inputArgs.remove(0);
            }

            //If the argument has nested arguments, parse them recursively.
            if (comArgs.getArguments().get(i).getArguments().size() > 0) {
                return parseArguments(true, commandText, command, comArgs.getArguments().get(i), inputArgs, returnArgs);
            }
        }
        return false;
    }

    /**
     * Ran when no commands are found. Will create an error detailing what went wrong.
     */
    private void noCommandsFound(ArrayList<String> inputArgs) {
        onParseError(String.format("Command '%1$s' not found.", inputArgs.get(0)));

        //Find related commands (Did you mean?)
        ArrayList<String> related = findRelatedCommands(inputArgs.get(0));

        if (related.size() > 0) {
            String message = formatRelatedCommands(related);
            onParseError(String.format("Did you mean: %1$s?", message));
        }
    }

    /**
     * Takes input from <pre>FindRelatedCommands</pre> and generates a readable string.
     */
    private String formatRelatedCommands(java.util.ArrayList<String> related) {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < related.size(); i++) {
            sb.append('\'').append(related.get(i)).append('\'');
            if (related.size() > 1) {
                if (i == related.size() - 2)
                    sb.append(", or ");
                else if (i < related.size() - 1)
                    sb.append(", ");
            }
        }
        return sb.toString();
    }

    /**
     * Checks the validation of arguments at the specified index.
     */
    private boolean checkArgumentValidation(IArguable comArgs, java.util.ArrayList<String> inputArgs, int index) {
        if (!inputArgs.get(index).equals("") && !comArgs.getArguments().get(index).isValid(inputArgs.get(index))) {
            onParseError(String.format("Argument '%1$s' is invalid. Must be a valid %2$s.", comArgs.getArguments().get(index).getName(), comArgs.getArguments().get(index).getRule().getError()));
            return true;
        }
        return false;
    }

    /**
     * Finds command aliases related to the input command that may have been spelled incorrectly.
     */
    private ArrayList<String> findRelatedCommands(String input) {
        ArrayList<String> related = new ArrayList<>();
        for (Command command : commands) {
            for (String alias : command.getAliases()) {
                if ((alias.startsWith(input)) || //If the user missed the last few letters.
                        (input.length() >= 2 && alias.startsWith(input.substring(0, 2))) || //If user missed last few letters.
                        (input.length() > 2 && alias.endsWith(input.substring(input.length() - 3, input.length() - 1))) || //If user misspelled middle characters.
                        (alias.startsWith(input.substring(0, 1)) && alias.endsWith(input.substring(input.length() - 2, input.length() - 1)))) //If the user did not complete the command.
                {
                    //Add related command to the "Did you mean?" list.
                    related.add(alias);
                    break;
                }
            }
        }
        return related;
    }

    /**
     * Resets the command's arguments back to their default values.
     */
    private void resetArgs(IArguable command) {
        for (Argument arg : command.getArguments()) {
            arg.resetValue();
            if (arg.getArguments().size() > 0)
                resetArgs(arg);
        }
    }

    /**
     * If the arguments are longer than they should be, merge them into the last one.
     * This way a user does not need quotes for a chat message for example.
     */
    private static void mergeLastArguments(boolean recursive, Command command, IArguable comArgs, ArrayList<String> inputArgs, int i) {
        if ((i > 0 || i == comArgs.getArguments().size() - 1) && inputArgs.size() > command.getArguments().size()) {
            if (comArgs.getArguments().size() >= 1 + comArgs.getArguments().get(comArgs.getArguments().size() - 1).getArguments().size() && ((!recursive && !comArgs.getArguments().get(comArgs.getArguments().size() - 1).isEnum()) || recursive)) {
                StringBuilder sb = new StringBuilder();
                for (int j = command.getArguments().size() + (recursive && comArgs.getArguments().size() > 1 ? 1 : 0); j < inputArgs.size(); j++)
                    sb.append(' ').append(inputArgs.get(j));
                inputArgs.set(command.getArguments().size() - (recursive && comArgs.getArguments().size() > 1 ? 0 : 1), inputArgs.get(command.getArguments().size() - (recursive && comArgs.getArguments().size() > 1 ? 0 : 1)) + sb.toString());
            }
        }
    }

    /**
     * Returns a list of possible values for an enum (type) argument in a readable format.
     */
    private static String generateEnumArguments(Argument argument) {
        if (!argument.isEnum())
            throw new IllegalArgumentException("Argument must be an enum style argument.");

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < argument.getArguments().size(); i++) {
            Argument arg = argument.getArguments().get(i);
            //Write name
            sb.append("'");
            sb.append(arg.getName());
            sb.append("'");

            //Indicate default argument
            if (arg.getName() == argument.getDefault())
                sb.append(" (default)");

            //Add comma and "or" if needed
            if (argument.getArguments().size() > 1) {
                if (i == argument.getArguments().size() - 2)
                    sb.append(", or ");
                else if (i < argument.getArguments().size() - 1)
                    sb.append(", ");
            }
        }

        return sb.toString();
    }

    /**
     * Called when an error occurs during parsing. Details on the error are returned such as incorrect arguments, failed validation, etc.
     */
    private void onParseError(String message) {
        if (parseError != null)
            parseError.accept(message);
    }

    /**
     * Returns a list of the commands in the parser *
     */
    public ArrayList<Command> getCommands() {
        return commands;
    }
}
