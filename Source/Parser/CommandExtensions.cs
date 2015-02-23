using System;
using System.Collections.Generic;
using System.Text;

namespace Pyratron.Frameworks.Commands.Parser
{
    /// <summary>
    /// Provides extension methods for arguments that can be used with the library.
    /// </summary>
    public static class CommandExtensions
    {
        /// <summary>
        /// Retrieves an argument's value by it's name from an <c>Argument</c> collection or array.
        /// </summary>
        public static string FromName(this IEnumerable<Argument> arguments, string name)
        {
            if (arguments == null) throw new ArgumentNullException("arguments");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            foreach (var arg in arguments)
            {
                if (arg.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return arg.Value;
                if (arg.Arguments.Count > 0) //If argument has nested args, recursively search
                    return FromName(arg.Arguments, name);
            }

            throw new InvalidOperationException(string.Format("No argument of name {0} found.", name));
        }

        /// <summary>
        /// Generates an readable argument string for the given arguments. (Ex: "%lt;player&gt; &lt;item&gt; [amount]")
        /// </summary>
        public static string GenerateArgumentString(this List<Argument> arguments)
        {
            if (arguments == null) throw new ArgumentNullException("arguments");

            var sb = new StringBuilder();
            arguments.WriteArguments(sb);
            return sb.ToString().Trim();
        }

        /// <summary>
        /// Generates an readable argument string for the given arguments. (Ex: "%lt;player&gt; &lt;item&gt; [amount]")
        /// </summary>
        private static void WriteArguments(this List<Argument> arguments, StringBuilder sb)
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