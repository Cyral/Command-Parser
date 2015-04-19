using System.Configuration;
using NUnit.Framework;
using Pyratron.Frameworks.Commands.Parser;

namespace Pyratron.Frameworks.Commands.Tests
{
    [TestFixture]
    public class Tests
    {
        /// <summary>
        /// Tests creation and execution of a simple command.
        /// </summary>
        [Test]
        public void TestCommand()
        {
            var ran = false;
            var parser = CommandParser.CreateNew().UsePrefix(string.Empty);
            parser.AddCommand(Command
                .Create("Test")
                .AddAlias("test")
                .SetAction(delegate { ran = true; }));

            parser.Commands[0].Execute(null);

            Assert.True(ran);
        }

        /// <summary>
        /// Tests misspelling a command.
        /// </summary>
        [Test]
        public void TestInvalidCommand()
        {
            var ran = false;
            var error = false;
            var parser = CommandParser.CreateNew().UsePrefix(string.Empty).OnError((o, s) => error = true);
            parser.AddCommand(Command
                .Create("Test")
                .AddAlias("test")
                .SetAction(delegate { ran = true; }));

            parser.Parse("test2");

            Assert.False(ran);
            Assert.True(error);
        }

        /// <summary>
        /// Tests the parser to see if it successfully parses a simple command.
        /// </summary>
        [Test]
        public void TestParser()
        {
            var ran = false;
            var parser = CommandParser.CreateNew().UsePrefix(string.Empty);
            parser.AddCommand(Command
                .Create("Test")
                .AddAlias("test")
                .SetAction(delegate { ran = true; }));

            parser.Parse("test");

            Assert.True(ran);
        }

        /// <summary>
        /// Tests parsing of a simple argument in a command.
        /// </summary>
        [Test]
        public void TestArguments()
        {
            const string input = "testarg";
            var result = string.Empty;
            var parser = CommandParser.CreateNew().UsePrefix(string.Empty);
            parser.AddCommand(Command
                .Create("Test")
                .AddAlias("test")
                .AddArgument(Argument.Create("arg"))
                .SetAction(delegate(Argument[] args) { result = args.FromName("arg"); }));

            parser.Parse("test " + input);

            Assert.AreEqual(result, input);
        }

        /// <summary>
        /// Tests default argument values.
        /// </summary>
        [Test]
        public void TestDefaultArguments()
        {
            var value = -1;
            var parser = CommandParser.CreateNew().UsePrefix(string.Empty);
            parser.AddCommand(Command
                .Create("Test")
                .AddAlias("test")
                .AddArgument(Argument.Create("arg").MakeOptional().SetDefault(10))
                .SetAction(delegate(Argument[] arguments) { value = int.Parse(arguments.FromName("arg")); }));

            //Test specified value
            parser.Parse("test 20");
            Assert.AreEqual(20, value);

            //Test default value
            value = -1;
            parser.Parse("test");
            Assert.AreEqual(10, value);
        }

        /// <summary>
        /// Tests optional arguments.
        /// </summary>
        [Test]
        public void TestOptionalArguments()
        {
            var ran = false;
            var parser = CommandParser.CreateNew().UsePrefix(string.Empty);
            parser.AddCommand(Command
                .Create("Test")
                .AddAlias("test")
                .AddArgument(Argument.Create("arg"))
                .AddArgument(Argument.Create("arg2").MakeOptional()) //Second argument not needed
                .SetAction(delegate { ran = true; }));

            parser.Parse("test 123");

            Assert.True(ran);
        }

        /// <summary>
        /// Tests required arguments.
        /// </summary>
        [Test]
        public void TestRequiredArguments()
        {
            var ran = false;
            var error = false;
            var parser = CommandParser.CreateNew().UsePrefix(string.Empty).OnError((o, s) => error = true);
            parser.AddCommand(Command
                .Create("Test")
                .AddAlias("test")
                .AddArgument(Argument.Create("arg"))
                .AddArgument(Argument.Create("arg2"))
                .SetAction(delegate { ran = true; }));

            parser.Parse("test 123");

            Assert.True(error);
            Assert.False(ran);
        }

        /// <summary>
        /// Tests nested arguments.
        /// </summary>
        [Test]
        public void TestNestedArguments()
        {
            var ran = false;
            var error = false;
            var parser = CommandParser.CreateNew().UsePrefix(string.Empty).OnError((o, s) => error = true);
            parser.AddCommand(Command
                .Create("Test")
                .AddAlias("test")
                .SetAction(
                    delegate { ran = true; })
                .AddArgument(Argument
                    .Create("arg1").MakeOptional()
                    .AddArgument(Argument
                        .Create("arg2"))));
            //Test with no args, since arg1 is optional
            parser.Parse("test");

            Assert.False(error);
            Assert.True(ran);

            //Test with 1 arg, should result in error
            ran = error = false;
            parser.Parse("test 123");

            Assert.True(error);
            Assert.False(ran);
        }

        /// <summary>
        /// Tests enum argument.
        /// </summary>
        [Test]
        public void TestEnumArguments()
        {
            var ran = false;
            var error = false;
            var parser = CommandParser.CreateNew().UsePrefix(string.Empty).OnError((o, s) => error = true);

            parser.AddCommand(Command.Create("Mail") //Demo command
                .AddAlias("mail")
                .SetDescription("Allows users to send messages.")
                .SetAction(delegate { ran = true; })
                .AddArgument(Argument
                    .Create("type")
                    .MakeOptional()
                    .AddOption(Argument.Create("read"))
                    .AddOption(Argument.Create("clear"))
                    .AddOption(Argument.Create("send")
                        .AddArgument((Argument.Create("user")))
                        .AddArgument((Argument.Create("message"))))));

            //Test default
            parser.Parse("mail");

            Assert.False(error);
            Assert.True(ran);

            //Test with enum
            ran = error = false;
            parser.Parse("mail send user message");

            Assert.False(error);
            Assert.True(ran);

            //Test with invalid option
            ran = error = false;
            parser.Parse("mail invalid");

            Assert.True(error);
            Assert.False(ran);
        }

        /// <summary>
        /// Tests enum argument in nested optional block.
        /// </summary>
        [Test]
        public void TestOptionalEnumArguments()
        {
            var arg1 = string.Empty;
            var arg2= string.Empty;
            var parser = CommandParser.CreateNew().UsePrefix(string.Empty);

            parser.AddCommand(Command
                .Create("Test")
                .AddAlias("test")
                .SetAction(
                    delegate(Argument[] arguments)
                    {
                        arg1 = arguments.FromName("arg1");
                        arg2 = arguments.FromName("arg2");
                    })
                .AddArgument(Argument
                    .Create("arg1")
                    .MakeOptional()
                    .SetDefault("default")
                    .AddArgument(Argument
                        .Create("arg2")
                        .MakeOptional()
                        .SetDefault("on")
                        .AddOption(Argument.Create("on"))
                        .AddOption(Argument.Create("off")))));

            //Test empty
            parser.Parse("test");
            Assert.AreEqual(arg1, "default");
            Assert.AreEqual(arg2, "on");

            //Test with first arg, but not second
            arg1 = arg2 = string.Empty;
            parser.Parse("test 123");

            Assert.AreEqual(arg1, "123");
            Assert.AreEqual(arg2, "on");

            //Test with all args
            arg1 = arg2 = string.Empty;
            parser.Parse("test 123 off");
            Assert.AreEqual(arg1, "123");
            Assert.AreEqual(arg2, "off");
        }

        /// <summary>
        /// Tests the CanExecute precondition on a command.
        /// </summary>
        [Test]
        public void TestCanExecute()
        {
            var canExecute = true;
            var ran = false;
            var parser = CommandParser.CreateNew().UsePrefix(string.Empty);

            parser.AddCommand(Command
               .Create("Test")
               .AddAlias("test")
               .SetAction(obj => ran = true)
               .SetExecutePredicate(command => canExecute ? string.Empty : "Error"));

            //CanExecute true
            parser.Parse("test");
            Assert.IsTrue(ran);

            //CanExecute false
            canExecute = false;
            ran = false;
            parser.Parse("test");
            Assert.IsFalse(ran);
        }
    }
}