using System.Collections.Generic;

namespace Pyratron.Frameworks.Commands.Parser
{
    /// <summary>
    /// Represents an object that has arguments/parameters
    /// </summary>
    internal interface IArguable
    {
        /// <summary>
        /// The input (Including alias and help) that are passed with the command or argument.
        /// </summary>
        List<Argument> Arguments { get; set; }
    }
}