// From http://antlrcsharp.codeplex.com/
// License: Eclipse Public License (EPL)  
// Author: http://www.codeplex.com/site/users/view/anbrad

// Parse.cs
//

using System;
using System.IO;
using System.Reflection;

using Antlr.Runtime;
using Antlr.Runtime.Debug;
using Antlr.Runtime.Tree;

using Parser = Antlr.Runtime.Parser;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Castle.Blade.Tests.TestFx
{
	/// <summary> public partial class Parse
	/// The class to use to parse a directory of files.
	/// </summary>
	public partial class Parse
	{
		public csParser.compilation_unit_return ParseContent(string content)
		{
			var tokens = CreateLexer<PreProcessor>(content);
			var p = new csParser(tokens);
			p.TraceDestination = Console.Error;

            return p.compilation_unit();
            
            /**
			using (ConsolePause con = new ConsolePause(wait))
			{
				// Try and call a rule like CSParser.namespace_body() 
				// Use reflection to find the rule to use.
				MethodInfo mi = p.GetType().GetMethod(rule_name);

				// did we find a method the same as file name?
				if (mi != null)
				{
					con.warn(string.Format("parser using rule -- {0}:", rule_name));
					mi.Invoke(p, new object[0]);
				}
				else
				{
					// by default use the start rule for csharp, I called this compilation_unit in the grammar.
					con.warn("parser using rule -- compilation_unit:");
					p.compilation_unit();
				}


				// If we parsed the file (no error messages), clear the Archive flag
				if (!con.HasMessages)
				{
					// Clear archive attribute
					File.SetAttributes(file_name, File.GetAttributes(file_name) & ~FileAttributes.Archive);
					++count;
				}
			}
            */
		}

		/// <summary> CreateLexer </summary>
		public CommonTokenStream CreateLexer<L> (string content) where L : Lexer, new()
		{
			CommonTokenStream tokens = null;

			ICharStream input = new ANTLRStringStream(content, "inlinecontent");
			L lex = new L();
			lex.CharStream = input;

			tokens = new CommonTokenStream(lex);
			return tokens;
		}

        /// <summary> DumpNodes
        /// The CommonTreeNodeStream has a tree in "flat form".  The UP and DOWN tokens represent the branches of the
        /// tree.  Dump these out in tree form to the console.
        /// </summary>
        private static void DumpNodes(CommonTreeNodeStream nodes)
		{
			// Dump out nodes if -n on command line
			// if (Util.Args.IsFlagSet("-n"))
			{
				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.WriteLine("Nodes");
				int spaces = 0;
				string str_spaces = "                                                                                       ";
				object o_prev = string.Empty;
				object o = nodes.NextElement();
				while (!nodes.IsEndOfFile(o))
				{
					//object o = nodes.Get(n);
					//object o = nodes[n];
					if (o.ToString() == "DOWN")
					{
						spaces += 2;
						if (o_prev.ToString() != "UP" && o_prev.ToString() != "DOWN")
							Console.Write("\r\n{0} {1}", str_spaces.Substring(0, spaces), o_prev);
					}
					else if (o.ToString() == "UP")
					{
						spaces -= 2;
						if (o_prev.ToString() != "UP" && o_prev.ToString() != "DOWN")
							Console.Write(" {0}\r\n{1}", o_prev, str_spaces.Substring(0, spaces));
					}
					else if (o_prev.ToString() != "UP" && o_prev.ToString() != "DOWN")
						Console.Write(" {0}", o_prev.ToString());

					o_prev = o;
					o = nodes.NextElement();
				}
				if (o_prev.ToString() != "UP" && o_prev.ToString() != "DOWN")
					Console.WriteLine(" {0}", o_prev.ToString());
				Console.ResetColor();
			}
		}
	}
}
