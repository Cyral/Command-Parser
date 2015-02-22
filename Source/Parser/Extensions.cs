using System;
using System.Collections.Generic;
using System.Text;

namespace Pyratron.Frameworks.Commands.Parser
{
    public static class Extensions
    {
        /// <summary>
        /// Retrieves an argument by it's name from a <c>Argument</c> collection or array.
        /// </summary>
        public static Argument ArgumentFromName(this IEnumerable<Argument> arguments, string name)
        {
            if (arguments == null) throw new ArgumentNullException("arguments");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            foreach (var arg in arguments)
            {
                if (arg.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return arg;
                if (arg.Arguments.Count > 0) //If argument has nested args, recursively search
                    return ArgumentFromName(arg.Arguments, name);
            }

            throw new InvalidOperationException(string.Format("No argument of name {0} found.", name));
        }

        /// <summary>
        /// Generates an readable argument string for the given arguments. (Ex: "%lt;player&gt; &lt;item&gt; [amount]")
        /// Strings like this are similar to what <c>Command.InferArguments(..)</c> take.
        /// </summary>
        public static string GenerateArgumentString(this List<Argument> arguments)
        {
            if (arguments == null) throw new ArgumentNullException("arguments");

            var sb = new StringBuilder();
            WriteArguments(arguments, sb);
            return sb.ToString().Trim();
        }

        /// <summary>
        /// Generates an readable argument string for the given arguments. (Ex: "%lt;player&gt; &lt;item&gt; [amount]")
        /// Strings like this are similar to what <c>Command.InferArguments(..)</c> take.
        /// </summary>
        private static void WriteArguments(List<Argument> arguments, StringBuilder sb)
        {
            if (arguments == null) throw new ArgumentNullException("arguments");

            for (var i = 0; i < arguments.Count; i++)
            {
                var arg = arguments[i];
                //Write bracket, name, and closing bracket for each argument
                sb.Append(arg.Optional ? '[' : '<');
                if (arg.Enum) //Print possible values if "enum"
                {
                    for (var j = 0; j < arg.Arguments.Count; j++)
                    {
                        var possibility = arg.Arguments[j];
                        sb.Append(possibility.Name);
                        if (arg.Arguments[j].Arguments.Count >= 1) //Child arguments (Print each possible value)
                        {
                            sb.Append(' ');
                            WriteArguments(arg.Arguments[j].Arguments, sb);
                        }
                        if (j < arg.Arguments.Count - 1 && arg.Arguments.Count > 1) //Print "or"
                            sb.Append('|');
                    }
                }
                else
                {
                    sb.Append(arg.Name.ToLower());
                    if (arg.Arguments.Count >= 1) //Child arguments
                    {
                        sb.Append(' ');
                        WriteArguments(arg.Arguments, sb);
                    }
                }

                sb.Append(arg.Optional ? "]" : ">");
                if (i != arguments.Count - 1)
                    sb.Append(' ');
            }
        }
    }
}