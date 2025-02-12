/* Scanner.cs
 * Wyvern Compiler
 * Authors:
 *			A01370880 Rub�n Escalante Chan
 *			A01371036 Santiago Nakakawa Bernal
 *			A01371240 Iv�n Rangel Varela
 */

/*
  Wyvern compiler - This class performs the lexical analysis, 
  (a.k.a. scanning).
  Copyright (C) 2013 Ariel Ortiz, ITESM CEM
  
  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.
  
  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.
  
  You should have received a copy of the GNU General Public License
  along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Wyvern
{

	class Scanner
	{

		readonly string input;

		static readonly Regex regex = new Regex(
			@"
			    (?<Decrement>		--																								)			  
			  | (?<Dif>				!=																								)
			  | (?<Equal>			==																								)
              | (?<Increment>       \+\+																							)
			  | (?<GreaterEqual>	>=																								)
              | (?<LessEqual>       <=																								)

              | (?<And>             &&																								)
              | (?<Assign>          =																								)
              | (?<BracketLeft>     \[                                                                                              )
              | (?<BracketRight>    \]                                                                                              )
			  | (?<CharLiteral>		['](([\\]([n]|[r]|[t]|[f]|[\\]|[']|[""]|(u[a-fA-F0-9]{6})))?|[^\\\n\r\t\f'""]?)[']				)
              | (?<Comma>			[,]																								)
              | (?<Comment>         [/]{2}.*																						)
			  | (?<CommentEnd>		.*(\*\/)																						)
			  | (?<CommentInit>		(\/\*).*																						)
			  | (?<CurlyLeft>		[{]																								)
			  | (?<CurlyRight>		[}]																								)
			  | (?<Div>				[/]																								)
			  | (?<Greater>			>																								)
              | (?<Identifier>      [a-zA-Z]+\w*																					)
			  | (?<IntLiteral>		[-]?\d+																							)
              | (?<Less>            <																								)
			  | (?<Mod>				[%]																								)
              | (?<Mul>             [*]																								)
              | (?<Neg>             -																								)
              | (?<Newline>         \n																								)
              | (?<Not>				!																								)
              | (?<Or>				[|]{2}																							)
              | (?<ParLeft>         [(]																								)
              | (?<ParRight>        [)]																								)
              | (?<Plus>            \+																								)
			  | (?<Semicolon>		[;]																								)
			  | (?<StrLiteral>		[""](([\\]([n]|[r]|[t]|[f]|[\\]|[']|[""]|([u][a-fA-F0-9]{6})))?|[^\\\n\r\t\f""']?)*[""]			)
              | (?<WhiteSpace>      \s																								)     # Must go anywhere after Newline.
              | (?<Other>           .																								)     # Must be last: match any other character.
            ",
			RegexOptions.IgnorePatternWhitespace
				| RegexOptions.Compiled
				| RegexOptions.Multiline
			);

		static readonly IDictionary<string, TokenCategory> keywords =
			new Dictionary<string, TokenCategory>() {
				{"break", TokenCategory.BREAK},
				{"else", TokenCategory.ELSE},
				{"elseif", TokenCategory.ELSEIF},
				{"false", TokenCategory.FALSE},
				{"if", TokenCategory.IF},
				{"return", TokenCategory.RETURN},
				{"true", TokenCategory.TRUE},
				{"var", TokenCategory.VAR},
				{"while", TokenCategory.WHILE}
			};

		static readonly IDictionary<string, TokenCategory> nonKeywords =
			new Dictionary<string, TokenCategory>() {
				// Double operators
				{"Decrement", TokenCategory.DECREMENT},
				{"Dif", TokenCategory.DIF},
				{"Equal", TokenCategory.EQUAL},
				{"Increment", TokenCategory.INCREMENT},
				{"GreaterEqual", TokenCategory.GREATER_EQUAL},
				{"LessEqual", TokenCategory.LESS_EQUAL},

				// Single operators
				{"And", TokenCategory.AND},
				{"Assign", TokenCategory.ASSIGN},
				{"BracketLeft", TokenCategory.BRACKET_LEFT},
				{"BracketRight", TokenCategory.BRACKET_RIGHT},
				{"CharLiteral", TokenCategory.CHAR_LITERAL},
				{"Comma", TokenCategory.COMMA},
				{"CurlyLeft", TokenCategory.CURLY_LEFT},
				{"CurlyRight", TokenCategory.CURLY_RIGHT},
				{"Div", TokenCategory.DIV},
				{"Greater", TokenCategory.GREATER},
				{"IntLiteral", TokenCategory.INT_LITERAL},
				{"Less", TokenCategory.LESS},
				{"Mod", TokenCategory.MOD},
				{"Mul", TokenCategory.MUL},
				{"Neg", TokenCategory.NEG},
				{"Not", TokenCategory.NOT},
				{"Or", TokenCategory.OR},
				{"ParLeft", TokenCategory.PARENTHESIS_OPEN},
				{"ParRight", TokenCategory.PARENTHESIS_CLOSE},
				{"Plus", TokenCategory.PLUS},
				{"Semicolon", TokenCategory.SEMICOLON},
				{"StrLiteral", TokenCategory.STR_LITERAL}
			};

		public Scanner(string input)
		{
			this.input = input;
		}

		public IEnumerable<Token> Start()
		{

			var row = 1;
			var columnStart = 0;
			var inComment = false;

			Func<Match, TokenCategory, Token> newTok = (m, tc) =>
				new Token(m.Value, tc, row, m.Index - columnStart + 1);

			foreach (Match m in regex.Matches(input))
			{

				if (inComment
					&& !m.Groups["CommentEnd"].Success)
				{
					if (m.Groups["Newline"].Success) row++;
					// Inside a comment.

				}
				else if (m.Groups["Newline"].Success)
				{

					// Found a new line.
					row++;
					columnStart = m.Index + m.Length;

				}
				else if (m.Groups["WhiteSpace"].Success
				  || m.Groups["Comment"].Success
				  || m.Groups["CommentInit"].Success
				  || m.Groups["CommentEnd"].Success)
				{

					if (m.Groups["CommentInit"].Success) inComment = true;
					else if (m.Groups["CommentEnd"].Success) inComment = false;
					// Skip white space and comments.

				}

				else if (m.Groups["Identifier"].Success)
				{

					if (keywords.ContainsKey(m.Value))
					{

						yield return newTok(m, keywords[m.Value]);

					}
					else
					{

						yield return newTok(m, TokenCategory.IDENTIFIER);
					}

				}
				else if (m.Groups["Other"].Success)
				{

					// Found an illegal character.
					yield return newTok(m, TokenCategory.ILLEGAL_CHAR);

				}
				else
				{

					// Match must be one of the non keywords.
					foreach (var name in nonKeywords.Keys)
					{
						if (m.Groups[name].Success)
						{
							yield return newTok(m, nonKeywords[name]);
							break;
						}
					}
				}
			}

			yield return new Token(null,
								   TokenCategory.EOF,
								   row,
								   input.Length - columnStart + 1);
		}
	}
}