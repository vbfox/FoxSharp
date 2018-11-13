using System;
using System.Collections.Generic;

using gboolean = System.Boolean;
using gchar = System.Char;
using GString = System.String;

namespace BlackFox.CommandLine.TestParsers
{
    /// <summary>
    /// Mono command line parsing ported to C#
    /// </summary>
    public static class MonoUnix
    {
        // From https://github.com/mono/mono/blob/d4952011ea889848ea8b9285b42582d238ccecf8/mono/eglib/gshell.c#L33

        public static unsafe string[] Parse(string cmdLine)
        {
            var list = new List<string>();
            fixed (char* c = cmdLine)
            {
                split_cmdline(c, list);
            }
            return list.ToArray();
        }

        private static GString g_string_new(string s) => s;
        private static void g_string_append_c(ref string s, char c) => s = s + c;
        private static bool g_ascii_isspace(char c) => c == ' ' || c == '\t' || c == '\n' || c == '\v' || c == '\f' || c == '\r';
        private static string g_string_free(string s, bool ignored) => s;
        private static void g_ptr_array_add(List<string> lst, string s) => lst.Add(s);
        private static bool FALSE = false;
        private static bool TRUE = true;

        private static unsafe void split_cmdline(gchar* cmdline, List<string> array)
        {
            gchar *ptr;
            gchar c;
            gboolean escaped = FALSE, fresh = TRUE;
            gchar quote_char = '\0';
            GString str;

            str = g_string_new("");
            ptr = (gchar*)cmdline;
            while ((c = *ptr++) != '\0')
            {
                if (escaped)
                {
                    /*
                    * \CHAR is only special inside a double quote if CHAR is
                    * one of: $`"\ and newline
                    */
                    if (quote_char == '\"')
                    {
                        if (!(c == '$' || c == '`' || c == '"' || c == '\\'))
                            g_string_append_c(ref str, '\\');
                        g_string_append_c(ref str, c);
                    }
                    else
                    {
                        if (!g_ascii_isspace(c))
                            g_string_append_c(ref str, c);
                    }
                    escaped = FALSE;
                }
                else if (quote_char != '\0')
                {
                    if (c == quote_char)
                    {
                        quote_char = '\0';
                        if (fresh && (g_ascii_isspace(*ptr) || *ptr == '\0'))
                        {
                            g_ptr_array_add(array, g_string_free(str, FALSE));
                            str = g_string_new("");
                        }
                    }
                    else if (c == '\\')
                    {
                        escaped = TRUE;
                    }
                    else
                        g_string_append_c(ref str, c);
                }
                else if (g_ascii_isspace(c))
                {
                    if (str.Length > 0)
                    {
                        g_ptr_array_add(array, g_string_free(str, FALSE));
                        str = g_string_new("");
                    }
                }
                else if (c == '\\')
                {
                    escaped = TRUE;
                }
                else if (c == '\'' || c == '"')
                {
                    fresh = str.Length == 0;
                    quote_char = c;
                }
                else
                {
                    g_string_append_c(ref str, c);
                }
            }

            if (escaped)
            {
                throw new InvalidOperationException("Unfinished escape.");
            }

            if (quote_char != '\0')
            {
                throw new InvalidOperationException("Unfinished quote.");
            }

            if (str.Length > 0)
            {
                g_ptr_array_add(array, g_string_free(str, FALSE));
            }
            else
            {
                g_string_free(str, TRUE);
            }
        }
    }
}
