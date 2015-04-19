package com.pyratron.frameworks.commands.tests;

import com.pyratron.frameworks.commands.parser.Argument;
import com.pyratron.frameworks.commands.parser.Command;
import com.pyratron.frameworks.commands.parser.CommandParser;
import org.junit.Assert;
import org.junit.Test;

public class Tests {
    private static boolean ran = false, error = false, canExecute = false;
    private static int value;
    private static String result, arg1, arg2;

    /**
     * Tests creation and execution of addCommand simple command.
     */
    @Test
    public final void testCommand() {
        ran = false;
        CommandParser parser = CommandParser.createNew().usePrefix("");
        parser.addCommand(Command.create("Test").addAlias("test").setAction(args -> ran = true));

        parser.getCommands().get(0).execute(null);

        Assert.assertTrue(ran);
    }

    /**
     * Tests misspelling addCommand command.
     */
    @Test
    public final void testInvalidCommand() {
        ran = false;
        error = false;
        CommandParser parser = CommandParser.createNew().usePrefix("").onError(s -> error = true);
        parser.addCommand(Command.create("Test").addAlias("test").setAction((args) -> ran = true));

        parser.parse("test2");

        Assert.assertFalse(ran);
        Assert.assertTrue(error);
    }

    /**
     * Tests parsing addCommand simple command.
     */
    @Test
    public final void testParser() {
        ran = false;
        CommandParser parser = CommandParser.createNew().usePrefix("");
        parser.addCommand(Command.create("Test").addAlias("test").setAction((args) -> ran = true));

        parser.parse("test");

        Assert.assertTrue(ran);
    }

    /**
     * Tests parsing of addCommand simple argument in addCommand command.
     */
    @Test
    public final void testArguments() {
        final String input = "testarg";
        result = "";
        CommandParser parser = CommandParser.createNew().usePrefix("");
        parser.addCommand(Command.create("Test").addAlias("test").addArgument(Argument.create("arg")).setAction(args -> result = Argument.fromName(args, "arg")));

        parser.parse("test " + input);

        Assert.assertEquals(result, input);
    }

    /**
     * Tests default argument values.
     */
    @Test
    public final void testDefaultArguments() {
        value = -1;
        CommandParser parser = CommandParser.createNew().usePrefix("");
        parser.addCommand(Command.create("Test").addAlias("test").addArgument(Argument.create("arg").makeOptional().setDefault(10)).setAction(args ->
                value = Integer.parseInt(Argument.fromName(args, "arg"))));

        //Test specified value
        parser.parse("test 20");
        Assert.assertEquals(20, value);

        //Test default value
        value = -1;
        parser.parse("test");
        Assert.assertEquals(10, value);
    }

    /**
     * Tests optional arguments.
     */
    @Test
    public final void testOptionalArguments() {
        ran = false;
        CommandParser parser = CommandParser.createNew().usePrefix("");
        parser.addCommand(Command.create("Test").addAlias("test")
                .addArgument(Argument.create("arg"))
                .addArgument(Argument.create("arg2").makeOptional()).setAction(args -> ran = true));

        parser.parse("test 123");

        Assert.assertTrue(ran);
    }

    /**
     * Tests required arguments.
     */
    @Test
    public final void testRequiredArguments() {
        ran = false;
        error = false;
        CommandParser parser = CommandParser.createNew().usePrefix("").onError(s -> error = true);
        parser.addCommand(Command.create("Test").addAlias("test")
                .addArgument(Argument.create("arg"))
                .addArgument(Argument.create("arg2")).setAction(args -> ran = true));

        parser.parse("test 123");

        Assert.assertTrue(error);
        Assert.assertFalse(ran);
    }

    /**
     * Tests nested arguments.
     */
    @Test
    public final void testNestedArguments() {
        ran = false;
        error = false;

        CommandParser parser = CommandParser.createNew().usePrefix("").onError(s -> error = true);
        parser.addCommand(Command.create("Test").addAlias("test").setAction(args -> ran = true)
                .addArgument(Argument.create("arg1").makeOptional().addArgument(Argument.create("arg2"))));

        //Test with no arguments, since arg1 is optional
        parser.parse("test");

        Assert.assertFalse(error);
        Assert.assertTrue(ran);

        //Test with 1 argument, should result in error
        ran = error = false;
        parser.parse("test 123");

        Assert.assertTrue(error);
        Assert.assertFalse(ran);
    }

    /**
     * Tests enum argument.
     */
    @Test
    public final void testEnumArguments() {
        ran = false;
        error = false;

        CommandParser parser = CommandParser.createNew().usePrefix("").onError(s -> error = true);

        parser.addCommand(Command.create("Mail").addAlias("mail").setDescription("Allows users to send messages.").setAction(args -> ran = true)
                .addArgument(Argument.create("type").makeOptional()
                        .addOption(Argument.create("read"))
                        .addOption(Argument.create("clear"))
                        .addOption(Argument.create("send")
                                .addArgument((Argument.create("user")))
                                .addArgument((Argument.create("message"))))));

        //Test default
        parser.parse("mail");

        Assert.assertFalse(error);
        Assert.assertTrue(ran);

        //Test with enum
        ran = error = false;
        parser.parse("mail send user message");

        Assert.assertFalse(error);
        Assert.assertTrue(ran);

        //Test with invalid option
        ran = error = false;
        parser.parse("mail invalid");

        Assert.assertTrue(error);
        Assert.assertFalse(ran);
    }

    /**
     * Tests enum argument in nested optional block.
     */
    @Test
    public final void TestOptionalEnumArguments() {
        CommandParser parser = CommandParser.createNew().usePrefix("");

        parser.addCommand(Command.create("Test").addAlias("test").setAction(args ->
        {
            arg1 = Argument.fromName(args, "arg1");
            arg2 = Argument.fromName(args, "arg2");
        }).addArgument(Argument
                .create("arg1").makeOptional().setDefault("default")
                .addArgument(Argument.create("arg2").makeOptional().setDefault("on")
                        .addOption(Argument.create("on"))
                        .addOption(Argument.create("off")))));

        //Test empty
        parser.parse("test");
        Assert.assertEquals(arg1, "default");
        Assert.assertEquals(arg2, "on");

        //Test with first arg, but not second
        arg1 = arg2 = "";
        parser.parse("test 123");

        Assert.assertEquals(arg1, "123");
        Assert.assertEquals(arg2, "on");

        //Test with all args
        arg1 = arg2 = "";
        parser.parse("test 123 off");
        Assert.assertEquals(arg1, "123");
        Assert.assertEquals(arg2, "off");
    }

    /**
     * Tests the CanExecute precondition on a command.
     */
    @Test
    public final void TestCanExecute() {
        canExecute = true;
        ran = false;
        CommandParser parser = CommandParser.createNew().usePrefix("");

        parser.addCommand(Command.create("Test").addAlias("test").setAction(obj -> ran = true).setExecutePredicate(command -> canExecute ? "" : "Error"));

        //CanExecute true
        parser.parse("test");
        Assert.assertTrue(ran);

        //CanExecute false
        canExecute = false;
        ran = false;
        parser.parse("test");
        Assert.assertFalse(ran);
    }
}