package com.pyratron.frameworks.commands.parser;

import java.util.ArrayList;
import java.util.function.Consumer;
import java.util.function.Function;

public class Command implements IArguable {

    private int accessLevel;
    private Consumer<ArrayList<Argument>> action;
    private ArrayList<String> aliases;
    private Function<Command, String> canExecute;
    private String description;
    private String name;
    private ArrayList<Argument> arguments;

    /**
     * Creates a command with a human friendly name and a command alias.
     *
     * @param name        Human friendly name.
     * @param alias       Alias to use when sending the command. (Ex: "help", "exit")
     * @param description A description that provides basic information about the command.
     */
    private Command(String name, String alias, String description) {
        this(name, alias);
        setDescription(description);
    }

    /**
     * Creates a command with a human friendly name and a command alias.
     *
     * @param name  Human friendly name.
     * @param alias Alias to use when sending the command. (Ex: "help", "exit")
     */
    private Command(String name, String alias) {
        this(name);
        addAlias(alias);
    }

    /**
     * Creates a command with the specified name.
     *
     * @param name Human friendly name.
     */
    private Command(String name) {
        arguments = new ArrayList<>();
        aliases = new ArrayList<>();
        setName(name);
        canExecute = command -> ""; //Can execute always by default
    }


    /**
     * Creates a command with a human friendly name and a command alias.
     *
     * @param name  Human friendly name.
     * @param alias Alias to use when sending the command. (Ex: "help", "exit")
     */
    public static Command create(String name, String alias, String description) {
        return new Command(name, alias, description);
    }

    /**
     * Creates a command with a human friendly name and a command alias.
     *
     * @param name  Human friendly name.
     * @param alias Alias to use when sending the command. (Ex: "help", "exit")
     */
    public static Command create(String name, String alias) {
        return new Command(name, alias);
    }

    /**
     * Creates a command with the specified name.
     *
     * @param name Human friendly name.
     */
    public static Command create(String name) {
        return new Command(name);
    }

    /**
     * Creates a help string giving information on the command.
     * Example Result:
     * <pre>"Ban - Bans a user (Usage: ban &lt;user&gt;)"</pre>
     */
    public String showHelp() {
        return showHelp("");
    }

    /**
     * Creates a help string giving information on the command.
     * Example Result:
     * <pre>"Ban - Bans a user (Usage: ban &lt;user&gt;)"</pre>
     *
     * @param alias Custom alias to use in the message. (Example, if user inputs "banuser" as an alias, but the real input is "ban", make sure we use the alias in the message.)
     */
    public String showHelp(String alias) {
        StringBuilder sb = new StringBuilder();
        //Start off with the name
        sb.append(name);

        //Then description, if defined.
        if (description.equals(""))
            sb.append(": ").append(description);

        //Add a sample on how to use the command
        sb.append(" (Usage: ")
                .append(generateUsage(alias))
                .append(')');

        return sb.toString();
    }

    /**
     * Generates help text defining the usage of the command and its arguments.
     */
    public String generateUsage() {
        return generateUsage("");
    }

    /**
     * Generates help text defining the usage of the command and its arguments.
     *
     * @param alias Custom alias to use in the message. (Example, if user inputs "banuser" as an alias, but the real input is "ban", make sure we use the alias in the message.)
     */
    public String generateUsage(String alias) {
        StringBuilder sb = new StringBuilder();
        if (aliases.size() == 0) return ""; //If no aliases, a usage cannot be determined
        if (arguments.size() == 0) return aliases.get(0); //If no arguments, simply return the command name

        //create usage string with main alias and arguments
        return sb.append(alias.equals("") ? aliases.get(0) : alias)
                .append(' ')
                .append(Argument.generateArgumentString(arguments))
                .toString();
    }

    /**
     * Sets a friendly name for the command.
     * Note that the actual "/command" is defined as an alias.
     */
    public Command setName(String name) {
        if (name.equals("")) throw new IllegalArgumentException("name");

        this.name = name;
        return this;
    }


    /**
     * Adds a command alias that will call the command's action.
     *
     * @param alias Alias that will call the action (Ex: "help", "exit")
     */
    public Command addAlias(String alias) {
        if (alias.equals("")) throw new IllegalArgumentException("alias");

        aliases.add(alias.toLowerCase());
        return this;
    }

    /**
     * Adds command aliases that will call the action.
     *
     * @param aliases Aliases that will call the action (Ex: "help", "exit")
     */
    public Command addAlias(String... aliases) {
        if (aliases == null) throw new IllegalArgumentException("aliases");

        for (String arg : aliases)
            addAlias(arg);
        return this;
    }

    /**
     * Sets a description for the command.
     *
     * @param description Describes the command and provides basic information about it.
     */
    public Command setDescription(String description) {
        this.description = description;
        return this;
    }

    /**
     * Restricts the command from being run if the access level is below what is specified.
     * Useful for creating "ranks" where permission is needed to run a command.
     */
    public Command restrictAccess(int accessLevel) {
        this.accessLevel = accessLevel;
        return this;
    }


    /**
     * Sets an action to be ran when the command is executed.
     *
     * @param action Action to be ran, which takes a <pre>Argument</pre> array parameter representing the passed input.
     */
    public Command setAction(Consumer<ArrayList<Argument>> action) {
        if (action == null) throw new IllegalArgumentException("action");

        this.action = action;
        return this;
    }

    public Command execute(ArrayList<Argument> arguments) {
        if (action == null)
            throw new IllegalArgumentException("The command's action must be defined before calling it.");

        //Run the pre-condition, if it passes (returns no error), run the action
        if (canExecute.apply(this).equals(""))
            action.accept(arguments);
        return this;
    }

    /**
     * Sets the rule that determines if the command can be executed.
     * The function should return an error message if the command cannot be executed. Function is called before the command
     * arguments are parsed.
     */
    public Command setExecutePredicate(Function<Command, String> canExecute) {
        this.canExecute = canExecute;
        return this;
    }

    /**
     * Add an argument to the command. All input are required by default, and are parsed in the order they are defined.
     * Optional arguments must come last.
     */
    public Command addArgument(Argument argument) {
        if (argument == null) throw new IllegalArgumentException("argument");

        boolean optional = getArguments().stream().anyMatch(arg -> arg.isOptional());
        if (optional && !argument.isOptional())
            throw new IllegalStateException("Optional arguments must come last.");

        getArguments().add(argument);
        return this;
    }

    /**
     * Adds an array of arguments to the command. Optional arguments must come last.
     */
    public Command addArguments(Argument[] arguments) {
        if (arguments == null) throw new IllegalArgumentException("arguments");

        for (Argument arg : arguments)
            addArgument(arg);

        return this;
    }

    /**
     * Returns the access level required to run the command *
     */
    public int getAccessLevel() {
        return accessLevel;
    }

    /**
     * Returns if the command can be run. The rule that determines if the command can be executed, which is true by default.
     *
     * @return Returns an error message if the command can not be executed, otherwise returns ""
     */
    public String canExecute(Command command) {
        return canExecute.apply(command);
    }

    /**
     * Returns the human friendly name of this command.
     */
    public String getName() {
        return name;
    }

    /**
     * Returns the aliases of this command,
     */
    public ArrayList<String> getAliases() {
        return aliases;
    }

    @Override
    public ArrayList<Argument> getArguments() {
        return arguments;
    }
}
