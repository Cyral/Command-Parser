using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pyratron.Frameworks.Commands.Parser
{
    public static class Extensions
    {
        /// <summary>
        /// Retrieves an argument by it's name from a <c>CommandArgument</c> collection or array.
        /// </summary>
        public static CommandArgument ArgumentFromName(this IEnumerable<CommandArgument> arguments, string name)
        {
            return arguments.FirstOrDefault(arg => arg.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Generates an argument string for the given arguments. (Ex: "%lt;player&gt; &lt;item&gt; [amount]")
        /// Strings like this are what <c>Command.InferArguments(..)</c> take.
        /// </summary>
        public static string GenerateArgumentString(this IEnumerable<CommandArgument> arguments)
        {
            var sb = new StringBuilder();
            foreach (var arg in arguments)
            {
                //Write bracket, name, and closing bracket for each argument
                sb.Append(arg.Optional ? "[" : "<");
                sb.Append(arg.Name.ToLower());
                sb.Append(arg.Optional ? "] " : "> ");
            }

            return sb.ToString().Trim();
        }
    }
}