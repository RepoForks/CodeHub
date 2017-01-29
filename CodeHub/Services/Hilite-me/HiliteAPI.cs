﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;
using CodeHub.Helpers;
using JetBrains.Annotations;

namespace CodeHub.Services.Hilite_me
{
    /// <summary>
    /// A static class that makes API calls to the Hilite.me web service
    /// </summary>
    public static class HiliteAPI
    {
        /// <summary>
        /// Gets the web API URL
        /// </summary>
        public const String APIUrl = "http://hilite.me/api";

        /// <summary>
        /// Gets a collection of lexers for less common source code files extensions
        /// </summary>
        public static readonly IReadOnlyDictionary<String, String> UncommonExtensions = new ReadOnlyDictionary<String, String>(new Dictionary<String, String>
        {
            { ".h", "c" },
            { ".xaml", "xml" },
            { ".cs", "csharp" },
            { ".gitignore", "c" },
            { ".gitattributes", "c" },
            { ".jshintrc", "xml" },
            { ".yml", "xml" },
            { ".snyk", "xml" },
            { ".lock", "xml" },
            { ".sln", "xml" },
            { ".eslintrc", "xml" },
            { ".babelrc", "json" },
            { ".bowerrc", "json" },
            { ".editorconfig", "xml" },
            { ".eslintignore", "xml" },

        });

        /// <summary>
        /// Tries to get the highlighted HTML code for a given source code
        /// </summary>
        /// <param name="code">The code to highlight</param>
        /// <param name="path">The path of the file that contains the input code</param>
        /// <param name="style">The requested highlight style</param>
        /// <param name="lineNumbers">Indicates whether or not to show line numbers in the result HTML</param>
        /// <param name="token">The cancellation token for the operation</param>
        [ItemCanBeNull]
        public static async Task<String> TryGetHighlightedCodeAsync([NotNull] String code, [NotNull] String path, 
            SyntaxHighlightStyle style, bool lineNumbers, CancellationToken token)
        {
            // Try to extract the code language
            Match match = Regex.Match(path, @".*([.]\w+)");
            if (!match.Success || match.Groups.Count != 2) return null;
            String
                extension = match.Groups[1].Value.ToLowerInvariant(),
                lexer = UncommonExtensions.ContainsKey(extension)
                    ? UncommonExtensions[extension]
                    : extension.Substring(1); // Remove the leading '.'

            // Prepare the POST request content
            IDictionary<String, String> values = new Dictionary<String, String>
            {
                { "code", code }, // The code to highlight
                { "lexer", lexer }, // The code language
                { "style", style.ToString().ToLowerInvariant() }, // The requested syntax highlight style
                { "divstyles", "border:solid gray;border-width:.0em .0em .0em .0em;padding:.2em .6em;" }, // Default CSS properties
                { "linenos", lineNumbers ? "pls" : String.Empty } // Includes the line numbers if not empty
            };

            // Make the POST
            return await HTTPHelper.POSTWithCacheSupportAsync(APIUrl, values, token);
        }
    }
}
