using System.Diagnostics;
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
            bool ran = false;
            var parser = CommandParser.CreateNew().UsePrefix(string.Empty);
            parser.AddCommand(Command
                .Create("Test")
                .AddAlias("test")
                .SetAction(delegate {
                    ran = true;
                }));

            parser.Commands[0].Execute(null);

            Assert.True(ran);
        }

        /// <summary>
        /// Tests the parser to see if it successfully parses a simple command.
        /// </summary>
        [Test]
        public void TestParser()
        {
            bool ran = false;
            var parser = CommandParser.CreateNew().UsePrefix(string.Empty);
            parser.AddCommand(Command
                .Create("Test")
                .AddAlias("test")
                .SetAction(delegate
            {
                ran = true;
            }));

            parser.Parse("test");

            Assert.True(ran);
        }

        /// <summary>
        /// Tests parsing of a simple argument in a command.
        /// </summary>
        [Test]
        public void TestArgumentsParser()
        {
            const string input = "testarg";
            string result = string.Empty;
            var parser = CommandParser.CreateNew().UsePrefix(string.Empty);
            parser.AddCommand(Command
                .Create("Test")
                .AddAlias("test")
                .AddArgument(Argument.Create("arg"))
                .SetAction(delegate(Argument[] args)
                {
                    result = args.FromName("arg");
                }));

            parser.Parse("test " + input);

            Assert.AreEqual(result, input);
        }
    }
}
