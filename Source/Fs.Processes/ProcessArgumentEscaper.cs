using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes
{
    /// <summary>
    /// Provides argument escaping compatible with Window's GetCommandLineArgsW() function.
    /// </summary>
    public static class ProcessArgumentEscaper
    {
        private enum RequiredQuotes
        {
            None,
            Simple,
            Escaped
        }

        /// <summary>
        /// Escapes the specified argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>The escaped argument.</returns>
        public static string Escape ( string argument )
        {
            return Escape(argument, false);
        }

        /// <summary>
        /// Escapes the specified argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="forceQuotes"><c>true</c> to always escape the argument</param>
        /// <returns>The escaped argument.</returns>
        public static string Escape ( string argument, bool forceQuotes )
        {
            if (argument == null)
                return null;

            RequiredQuotes requiredQuotes = RequiredQuotes.Escaped;
            if ((!forceQuotes) && ((requiredQuotes = RequiresQuotes(argument)) == RequiredQuotes.None))
                return argument;

            return QuoteArgument(new StringBuilder(), argument, requiredQuotes).ToString();
        }

        /// <summary>
        /// Escapes the argument, sending the output to the specified <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="argumentBuilder">The <see cref="StringBuilder"/> to write to.</param>
        /// <param name="argument">The argument.</param>
        /// <returns>The value of <paramref name="argumentBuilder"/>.</returns>
        public static StringBuilder Escape ( StringBuilder argumentBuilder, string argument )
        {
            return Escape(argumentBuilder, argument, false);
        }

        /// <summary>
        /// Escapes the argument, sending the output to the specified <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="argumentBuilder">The <see cref="StringBuilder"/> to write to.</param>
        /// <param name="argument">The argument.</param>
        /// <param name="forceQuotes"><c>true</c> to always escape the argument</param>
        /// <returns>The value of <paramref name="argumentBuilder"/>.</returns>
        public static StringBuilder Escape ( StringBuilder argumentBuilder, string argument, bool forceQuotes )
        {
            if (argumentBuilder == null)
                throw new ArgumentNullException(nameof(argumentBuilder));

            if (argument != null)
            {
                RequiredQuotes requiredQuotes = RequiredQuotes.Escaped;
                if ((!forceQuotes) && ((requiredQuotes = RequiresQuotes(argument)) == RequiredQuotes.None))
                    argumentBuilder.Append(argument);
                else
                    QuoteArgument(argumentBuilder, argument, requiredQuotes);
            }

            return argumentBuilder;
        }

        private static StringBuilder QuoteArgument ( StringBuilder argumentBuilder, string argument, RequiredQuotes quoteType )
        {
            // less work if it's a simple quote situation..
            if (quoteType == RequiredQuotes.None)
                argumentBuilder.Append(argument);
            else if (quoteType == RequiredQuotes.Simple)
                argumentBuilder.Append('"').Append(argument).Append('"');
            else
            {
                // things to escape, go through the longer process..
                argumentBuilder.Append('"');
                int argumentLength = argument.Length;

                for (int iIndex = 0; ; iIndex++)
                {
                    int backSlashCount = 0;

                    while ((iIndex < argumentLength) && (argument[iIndex] == '\\'))
                    {
                        iIndex++;
                        backSlashCount++;
                    }

                    if (iIndex == argumentLength)
                    {
                        // escape all backslashes, but our additional double-quote is not escaped..
                        argumentBuilder.Append('\\', backSlashCount * 2);
                        break;
                    }
                    else if (argument[iIndex] == '"')
                    {
                        // escape all backslashes and the double quote mark..
                        argumentBuilder.Append('\\', backSlashCount * 2 + 1);
                        argumentBuilder.Append('"');
                    }
                    else
                    {
                        // slashes don't matter here, include the number we counted..
                        argumentBuilder.Append('\\', backSlashCount);
                        argumentBuilder.Append(argument[iIndex]);
                    }
                }

                argumentBuilder.Append('"');
            }

            return argumentBuilder;
        }

        private static RequiredQuotes RequiresQuotes ( string argument )
        {
            if (argument.Length == 0)
                return RequiredQuotes.Simple;

            int simpleQuotes = 0;

            for (int iIndex = 0; iIndex < argument.Length; iIndex++)
            {
                switch (argument[iIndex])
                {
                    case ' ':
                    case '\t':
                    case '\n':
                    case '\v':
                        simpleQuotes++;
                        continue;

                    case '"':
                    case '\\':
                        return RequiredQuotes.Escaped;
                }
            }

            return (simpleQuotes > 0) ? RequiredQuotes.Simple : RequiredQuotes.None;
        }
    }
}

