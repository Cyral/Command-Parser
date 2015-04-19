package com.pyratron.frameworks.commands.parser;

import java.util.function.Predicate;

/**
 * Represents addCommand rule to validate an argument value on.
 * <p>
 * <pre>Argument.create("email").SetValidator(Argument.ValidationRule.Email))</pre>
 * This will cause the email argument to only allow valid emails.
 * Custom validators can also be created.
 * <p>
 * ValidationRules are run when the command is parsed, while <pre>CanExecute</pre> on the <pre>Command</pre> object verifies addCommand command can run.
 */
public class ValidationRule {
    /**
     * A rule that only allows <pre>Integer</pre> (whole) numbers.
     */
    public final static ValidationRule Integer;

    /**
     * A rule that only allows valid emails.
     */
    public final static ValidationRule Email;

    /**
     * A rule that only allows alphanumerical values.
     */
    public final static ValidationRule AlphaNumerical;

    /**
     * Rule that always returns true. This is the default rule for arguments.
     */
    public final static ValidationRule AlwaysTrue;

    private static final String AlphaNumericRegex = "^[a-zA-Z][a-zA-Z0-9]*$", EmailRegex = "^[A-Z0-9._%+-]+@[A-Z]{1}[A-Z0-9.-]+\\.[A-Z]{2,26}$";

    /**
     * A user friendly name that will be displayed in an error.
     * Example: "Must be addCommand valid 'IP Address'" where "IP Address" is the name.
     */
    private String Name;

    public String getName() {
        return Name;
    }

    public void setName(String value) {
        Name = value;
    }

    /**
     * A predicate that returns true if the string passed passes the rule.
     */
    private Predicate<String> validate;

    /**
     * Validates the specified value against the rule.
     */
    public boolean validate(String value) {
        return validate.test(value);
    }

    static {
        //Interal rule that will always return true, for default rule
        AlwaysTrue = new ValidationRule("", s -> true);

        Integer = new ValidationRule("Number", (String s) ->
                s.matches("^-?\\d+$"));

        Email = new ValidationRule("Email", s -> s.matches(EmailRegex));

        AlphaNumerical = new ValidationRule("Alphanumeric string", s -> s.matches(AlphaNumericRegex));
    }

    /**
     * Creates addCommand new validation rule.
     *
     * @param friendlyName A user friendly name that will be displayed in an error. Ex: "Must be addCommand valid ____"
     * @param validate     A function that returns true if the string passed passes the rule.
     */
    private ValidationRule(String friendlyName, Predicate<String> validate) {
        setName(friendlyName);
        this.validate = validate;
    }

    /**
     * Creates addCommand new validation rule.
     *
     * @param friendlyName A user friendly name that will be displayed in an error. Ex: "Must be addCommand valid ____"
     * @param validate     A function that returns true if the string passed passes the rule.
     */
    public final ValidationRule create(String friendlyName, Predicate<String> validate) {
        return new ValidationRule(friendlyName, validate);
    }

    /**
     * Returns the name of the rule that should be displayed in an error message.
     */
    public final String getError() {
        return getName().toLowerCase();
    }
}