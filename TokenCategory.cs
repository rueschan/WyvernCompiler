/* TokenCategory.cs
 * Wyvern Compiler
 * Authors:
 *			A01370880 Rub�n Escalante Chan
 *			A01371036 Santiago Nakakawa Bernal
 *			A01371240 Iv�n Rangel Varela
 */

/*
  Wyvern compiler - Token categories for the scanner.
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

namespace Wyvern {

    enum TokenCategory {
        AND,
        ASSIGN,
        BOOL,
        BOOL_LITERAL,
        BRACKET_LEFT,
        BRACKET_RIGHT,
        BREAK,
        CHAR_LITERAL,
		COMMA,
        CURLY_LEFT,
        CURLY_RIGHT,
		DECREMENT,
        DIF,
        DIV,
        ELSE,
        ELSEIF,
        END,
        EOF,
        EQUAL,
        FALSE,
        GREATER,
        GREATER_EQUAL,
        IDENTIFIER,
        IF,
		INCREMENT,
        INT,
        INT_LITERAL,
        LESS,
        LESS_EQUAL,
        MOD,
        MUL,
        NEG,
        NOT,
        OR,
        PARENTHESIS_OPEN,
        PARENTHESIS_CLOSE,
        PLUS,
        RETURN,
        STR_LITERAL,
        TRUE,
        VAR,
        WHILE,
        ILLEGAL_CHAR
    }
}

