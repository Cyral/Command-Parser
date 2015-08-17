package com.pyratron.frameworks.commands.parser;

import java.util.ArrayList;

public class Argument implements IArguable {

    private ArrayList<Argument> arguments;
    private String defaultValue;
    private boolean _enum;
    private String name;
    private boolean _optional;
    private ValidationRule rule;
    private String value;

    private Argument(String name, boolean optional) {
        if (name.equals("")) throw new IllegalArgumentException("name");
        rule = ValidationRule.AlwaysTrue;
        arguments = new ArrayList<>();
        this.name = name.toLowerCase();
        this._optional = optional;
        this.defaultValue = "";
        this.value = "";
    }

    /**
     * Creases a new command argument.
     *
     * @param name     The name to refer to this argument in documentation/help.
     * @param optional Indicates if this parameter is optional.
     */
    public static Argument create(String name, boolean optional) {
        if (name.equals("")) throw new IllegalArgumentException("name");
        return new Argument(name, optional);
    }

    /**
     * Creases a new command argument.
     *
     * @param name The name to refer to this argument in documentation/help.
     */
    public static Argument create(String name) {
        if (name.equals("")) throw new IllegalArgumentException("name");
        return new Argument(name, false);
    }

    /**
     * Makes the argument a required parameter.
     * Arguments are required by default.
     */
    public Argument makeRequired() {
        _optional = false;
        return this;
    }

    /**
     * Makes the argument an optional parameter.
     * Arguments are required by default.
     */
    public Argument makeOptional() {
        _optional = true;
        return this;
    }

    /**
     * Sets the value of the argument when parsing.
     */
    Argument setValue(String value) {
        if (!isValid(value) && !value.equals(""))
            throw new IllegalArgumentException("Value does not fulfill the validation rule.");

        if (value.equals(""))
            this.value = defaultValue;
        else
            this.value = value;
        return this;
    }

    /**
     * Sets the default value for an optional parameter when no value is specified.
     */
    public Argument setDefault(String value) {
        //Run a few tests
        if (!isValid(value))
            throw new IllegalArgumentException("Value does not fulfill the validation rule.");
        if (!_optional && !_enum)
            throw new IllegalStateException("Argument must be optional or enum to set a default value. Set default after marking argument as optional or adding types.");

        defaultValue = value;
        if (this.value.equals("")) //If value is empty, set to default value
            this.value = defaultValue;
        return this;
    }


    /**
     * Sets the default value for an optional parameter when no value is specified.
     * Only works on optional commands. (As required commands would not need a default value.
     */
    public Argument setDefault(Object value) {
        setDefault(value.toString());
        return this;
    }

    /**
     * Adds an option to the argument. Options make the argument behave like an enum, where only certain string values are allowed.
     *
     * @param value Each option can have children arguments.
     */
    public Argument addOption(Argument value) {
        if (value == null) throw new IllegalArgumentException("value");

        _enum = true; //An "enum" argument signifies that it had a limited set of values that can be passed.
        addArgument(value);
        return this;
    }

    /**
     * Sets the validation rule that is used for this argument.
     * Validation rules verify the input is valid.
     *
     * Example:
     * Argument.create("email").setValidator(Argument.ValidationRule.Email))
     * This will cause the email argument to only allow valid emails.
     * Custom validators can also be created.
     *
     * ValidationRules are run when the command is parsed, while <pre>CanExecute</pre> on the <pre>Command</pre> object verifies a command can run.
     *
     * @param rule Represents a rule to validate an argument value on.
     */
    public Argument setValidator(ValidationRule rule) {
        if (rule == null) throw new IllegalArgumentException("rule");

        this.rule = rule;
        return this;
    }


    /**
     * Restricts the possible values to a list of specific values, acting as if it were an enum.
     * Each option can have children arguments that define specific behavior.
     * Use AddOption to add "options" to this argument.
     *
     * For example, an argument that is set as an "enum" could have a few possible values added through <pre>AddOption</pre>.
     * You could add options such as "yes" and "no", which will only allow those two options to be used.
     * They will also be shown as choices in command help.
     */
    public Argument makeEnum() {
        _enum = true;
        return this;
    }

    /**
     * Add a nested argument to the argument. All arguments are required by default, and are parsed in the order they are defined.
     */
    public Argument addArgument(Argument argument) {
        if (argument == null) throw new IllegalArgumentException("argument");

        boolean optional = getArguments().stream().anyMatch(arg -> arg._optional);
        if (optional && !argument._optional)
            throw new IllegalStateException("Optional arguments must come last.");

        getArguments().add(argument);
        return this;
    }

    /**
     * Adds an array nested arguments to the current argument. Optional arguments must come last.
     */
    public Argument addArguments(Argument[] arguments) {
        if (arguments == null) throw new IllegalArgumentException("arguments");

        for (Argument arg : arguments)
            addArgument(arg);
        return this;
    }

    /**
     * Checks if the specified string is valid for the validation rule.
     */
    public boolean isValid(String value) {
        return rule.validate(value);
    }

    /**
     * Resets a value to empty, bypassing any validation.
     */
    public Argument resetValue() {
        setValue("");
        return this;
    }


    @Override
    /**
     * Nested arguments that are contained within this argument.
     */
    public ArrayList<Argument> getArguments() {
        return arguments;
    }

    /**
     * Returns true if the argument is not required.
     */
    public boolean isOptional() {
        return _optional;
    }

    /**
     * Returns true if the argument is an "enum", that is, restricted a set of values.
     */
    public boolean isEnum() {
        return _enum;
    }

    /**
     * Generates an readable argument string for the given arguments. (Ex: "&lt;player&gt; &lt;item&gt; [amount]")
     */
    public static String generateArgumentString(ArrayList<Argument> arguments) {
        if (arguments == null)
            throw new IllegalArgumentException("arguments");

        StringBuilder sb = new StringBuilder();
        writeArguments(arguments, sb);
        return sb.toString().trim();
    }

    /**
     * Returns the name of the argument.
     */
    public String getName() {
        return name;
    }

    /**
     * Generates an readable argument string for the given arguments. (Ex: "&lt;player&gt; &lt;item&gt; [amount]")
     * (Different than generateArgumentString which is for public use and creates a StringBuilder)
     */
    private static void writeArguments(ArrayList<Argument> arguments, StringBuilder sb) {
        if (arguments == null)
            throw new IllegalArgumentException("arguments");

        for (int i = 0; i < arguments.size(); i++) {
            Argument arg = arguments.get(i);

            //Write bracket, name, and closing bracket for each argument.
            sb.append(arg.isOptional() ? '[' : '<');
            if (arg.isEnum()) //Print possible values if "enum".
            {
                for (int j = 0; j < arg.getArguments().size(); j++) {
                    Argument possibility = arg.getArguments().get(j);
                    sb.append(possibility.getName().toLowerCase().replace("_", " "));
                    if (arg.getArguments().get(j).getArguments().size() >= 1) //Child arguments (Print each possible value).
                    {
                        sb.append(' ');
                        writeArguments(arg.getArguments().get(j).getArguments(), sb);
                    }
                    if (j < arg.getArguments().size() - 1 && arg.getArguments().size() > 1) //Print "or".
                        sb.append('|');
                }
            } else {
                sb.append(arg.getName().toLowerCase().replace("_", " "));
                if (arg.getArguments().size() >= 1) //Child arguments.
                {
                    sb.append(' ');
                    writeArguments(arg.getArguments(), sb);
                }
            }

            //Closing tag
            sb.append(arg.isOptional() ? "]" : ">");
            if (i != arguments.size() - 1)
                sb.append(' ');
        }
    }

    /**
     * Retrieves an argument's value by it's name from an <pre>Argument</pre> collection or array.
     * <code>
     * private static void OnCommandExecuted(Argument[] args) {
     * var user = args.FromName("user");
     * </code>
     */
    public static String fromName(Iterable<Argument> arguments, String name) {
        if (arguments == null)
            throw new IllegalArgumentException("arguments");
        if (name.equals(""))
            throw new IllegalArgumentException("name can not be empty");

        String value = fromNameRecurse(arguments, name);
        if (!value.equals(""))
            return value;

        throw new IllegalStateException(String.format("No argument of name %1$s found.", name));
    }

    private static String fromNameRecurse(Iterable<Argument> arguments, String name)
    {
        //Search top level arguments first
        for (Argument arg : arguments)
        {
            if (arg.getName().equals(name))
                return arg.getValue();
        }

        //Recursively search children
        for (Argument arg : arguments)
        {
            if (arg.getArguments().size() > 0) //If argument has nested args, recursively search
            {
                String value = fromNameRecurse(arg.getArguments(), name);
                if (!value.equals(""))
                    return value;
            }

        }
        return "";
    }

    /**
     * Returns the current value of the argument *
     */
    String getValue() {
        return value;
    }

    /**
     * Returns the default value of the argument *
     */
    public String getDefault() {
        return defaultValue;
    }

    /**
     * Returns the validation rule used to validate the argument value
     */
    public ValidationRule getRule() {
        return rule;
    }
}
