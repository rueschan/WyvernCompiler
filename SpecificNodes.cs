/* SpecificNodes.cs
 * Wyvern Compiler
 * Authors:
 *			A01370880 Rub�n Escalante Chan
 *			A01371036 Santiago Nakakawa Bernal
 *			A01371240 Iv�n Rangel Varela
 */

/*
  Wyvern compiler - Specific node subclasses for the AST (Abstract 
  Syntax Tree).
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

namespace Wyvern
{

	class Program : Node { }

	class DefList : Node { }

	class Def : Node { }

	class VarDef : Node { }

	class VarList : Node { }

	class IdList : Node { }

	class IdListCont : Node { }

	class FunDef : Node { }

	class ParamList : Node { }

	class VarDefList : Node { }

	class StmtList : Node { }

	class Stmt : Node { }

	class StmtAssign : Node { }

	class StmtIncr : Node { }

	class StmtDecr : Node { }

	class StmtFunCall : Node { }

	class FunCall : Node { }

	class ExprList : Node { }

	class ExprListCont : Node { }

	class StmtIf : Node { }

	class ElseIfList : Node { }

	class ElseIf : Node { }

	class Else : Node { }

	class StmtWhile : Node { }

	class StmtBreak : Node { }

	class StmtReturn : Node { }

	class StmtEmpty : Node { }

	class Expr : Node { }

	class ExprOr : Node { }

	class ExprAnd : Node { }

	class ExprComp : Node { }

	class OpComp : Node { }

	class ExprRel : Node { }

	class OpRel : Node { }

	class ExprAdd : Node { }

	class OpAdd : Node { }

	class ExprMul : Node { }

	class OpMul : Node { }

	class ExprUnary : Node { }

	class OpUnary : Node { }

	class ExprPrimary : Node { }

	class ArrayToken : Node { }

	class Lit : Node { }

	// Tokens
	class And : Node { }

	class CharLiteral : Node { }

	class Decrement : Node { }

	class Dif : Node { }

	class Div : Node { }

	class Equal : Node { }

	class False : Node { }

	class Greater : Node { }

	class GreaterEqual : Node { }

	class Identifier : Node { }

	class Increment : Node { }

	class IntLiteral : Node { }

	class Less : Node { }

	class LessEqual : Node { }

	class Mod : Node { }

	class Mul : Node { }

	class Neg : Node { }

	class Not : Node { }

	class Or : Node { }

	class Plus : Node { }

	class StrLiteral : Node { }

	class True : Node { }

}