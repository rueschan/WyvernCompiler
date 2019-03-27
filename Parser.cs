/* Parser.cs
 * Wyvern Compiler
 * Authors:
 *			A01370880 Rub�n Escalante Chan
 *			A01371036 Santiago Nakakawa Bernal
 *			A01371240 Iv�n Rangel Varela
 */

/*
  Wyvern compiler - This class performs the syntactic analysis,
  (a.k.a. parsing).
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

namespace Wyvern
{

	class Parser
	{

		static readonly ISet<TokenCategory> firstOfDef =
			new HashSet<TokenCategory>() {
				TokenCategory.VAR,
				TokenCategory.IDENTIFIER
			};

		static readonly ISet<TokenCategory> firstOfStmt =
			new HashSet<TokenCategory>() {
				TokenCategory.IDENTIFIER,
				TokenCategory.IF,
				TokenCategory.WHILE,
				TokenCategory.BREAK,
				TokenCategory.RETURN,
				TokenCategory.SEMICOLON
			};

		static readonly ISet<TokenCategory> firstOfExpr =
			new HashSet<TokenCategory>() {
				TokenCategory.PLUS,
				TokenCategory.NEG,
				TokenCategory.NOT,
				TokenCategory.IDENTIFIER,
				TokenCategory.BRACKET_LEFT,
				TokenCategory.TRUE,
				TokenCategory.FALSE,
				TokenCategory.INT_LITERAL,
				TokenCategory.CHAR_LITERAL,
				TokenCategory.STR_LITERAL,
				TokenCategory.PARENTHESIS_OPEN
			};

		static readonly ISet<TokenCategory> firstOfOpComp =
		new HashSet<TokenCategory>() {
				TokenCategory.EQUAL,
				TokenCategory.DIF
		};

		static readonly ISet<TokenCategory> firstOfOpRel =
		new HashSet<TokenCategory>() {
				TokenCategory.LESS,
				TokenCategory.LESS_EQUAL,
				TokenCategory.GREATER,
				TokenCategory.GREATER_EQUAL
		};

		static readonly ISet<TokenCategory> firstOfOpAdd =
		new HashSet<TokenCategory>() {
				TokenCategory.PLUS,
				TokenCategory.NEG
		};

		static readonly ISet<TokenCategory> firstOfOpMul =
		new HashSet<TokenCategory>() {
				TokenCategory.MUL,
				TokenCategory.DIV,
				TokenCategory.MOD
		};

		static readonly ISet<TokenCategory> firstOfOpUnary =
		new HashSet<TokenCategory>() {
				TokenCategory.PLUS,
				TokenCategory.NEG,
				TokenCategory.NOT
		};

		static readonly ISet<TokenCategory> firstOfExprPrimary =
		new HashSet<TokenCategory>() {
				TokenCategory.IDENTIFIER,
				TokenCategory.BRACKET_LEFT,
				TokenCategory.TRUE,
				TokenCategory.FALSE,
				TokenCategory.INT_LITERAL,
				TokenCategory.CHAR_LITERAL,
				TokenCategory.STR_LITERAL,
				TokenCategory.PARENTHESIS_OPEN
		};

		static readonly ISet<TokenCategory> firstOfLit =
		new HashSet<TokenCategory>() {
				TokenCategory.TRUE,
				TokenCategory.FALSE,
				TokenCategory.INT_LITERAL,
				TokenCategory.CHAR_LITERAL,
				TokenCategory.STR_LITERAL
		};

		IEnumerator<Token> tokenStream;

		public Parser(IEnumerator<Token> tokenStream)
		{
			this.tokenStream = tokenStream;
			this.tokenStream.MoveNext();
		}

		public TokenCategory CurrentToken
		{
			get { return tokenStream.Current.Category; }
		}

		public Token Expect(TokenCategory category)
		{
			if (CurrentToken == category)
			{
				Token current = tokenStream.Current;
				tokenStream.MoveNext();
				return current;
			}
			else
			{
				throw new SyntaxError(category, tokenStream.Current);
			}
		}

		public Node Program()
		{
			var defList = DefList();
			Expect(TokenCategory.EOF);

			return new Program() { defList };
		}
		public Node DefList()
		{
			var defList = new DefList();
			while (firstOfDef.Contains(CurrentToken))
			{
				defList.Add(Def());
			}

			return defList;
		}
		public Node Def()
		{
			switch (CurrentToken)
			{
				case TokenCategory.VAR:
					return VarDef();
				case TokenCategory.IDENTIFIER:
					return FunDef();
				default:
					throw new SyntaxError(firstOfDef, tokenStream.Current);
			}
		}
		public Node VarDef()
		{
			var varDef = new VarDef() {
				AnchorToken = Expect(TokenCategory.VAR)
			};
			varDef.Add(VarList());
			Expect(TokenCategory.SEMICOLON);

			return varDef;
		}
		public Node VarList()
		{
			var varList = new VarList();
			varList.Add(IdList());

			return varList;
		}
		public Node IdList()
		{
			var idList = new IdList();
			idList.Add(new Identifier() {
				AnchorToken = Expect(TokenCategory.IDENTIFIER)
			});
			
			idList.Add(IdListCont());
			
			return idList;
		}
		public Node IdListCont()
		{
			var idListCont = new IdListCont();
			while (CurrentToken == TokenCategory.COMMA)
			{
				Expect(TokenCategory.COMMA);
				idListCont.Add(new Identifier() {
					AnchorToken = Expect(TokenCategory.IDENTIFIER)
				});
			}
		}
		public Node FunDef()
		{
			var funDef = new FunDef();
			funDef.Add(new Identifier() {
				AnchorToken = Expect(TokenCategory.IDENTIFIER)
			});
			
			Expect(TokenCategory.PARENTHESIS_OPEN);
			var paramList = ParamList();
			Expect(TokenCategory.PARENTHESIS_CLOSE);
			Expect(TokenCategory.CURLY_LEFT);
			var varDefList = VarDefList();
			var stmtList = StmtList();
			Expect(TokenCategory.CURLY_RIGHT);
		}
		public void ParamList()
		{
			if (CurrentToken == TokenCategory.IDENTIFIER)
			{
				IdList();
			}
		}
		public void VarDefList()
		{
			while (CurrentToken == TokenCategory.VAR)
			{
				VarDef();
			}
		}
		public void StmtList()
		{
			while (firstOfStmt.Contains(CurrentToken))
			{
				Stmt();
			}
		}
		public void Stmt()
		{
			switch (CurrentToken)
			{
				case TokenCategory.IDENTIFIER:
					Expect(TokenCategory.IDENTIFIER);
					switch (CurrentToken)
					{
						case TokenCategory.ASSIGN:
							StmtAssign();
							break;
						case TokenCategory.INCREMENT:
							StmtIncr();
							break;
						case TokenCategory.DECREMENT:
							StmtDecr();
							break;
						case TokenCategory.PARENTHESIS_OPEN:
							StmtFunCall();
							break;
					}
					break;
				case TokenCategory.IF:
					StmtIf();
					break;
				case TokenCategory.WHILE:
					StmtWhile();
					break;
				case TokenCategory.BREAK:
					StmtBreak();
					break;
				case TokenCategory.RETURN:
					StmtReturn();
					break;
				case TokenCategory.SEMICOLON:
					StmtEmpty();
					break;
				default:
					throw new SyntaxError(firstOfStmt, tokenStream.Current);
			}
		}
		public void StmtAssign()
		{
			Expect(TokenCategory.ASSIGN);
			Expr();
			Expect(TokenCategory.SEMICOLON);
		}
		public void StmtIncr()
		{
			Expect(TokenCategory.INCREMENT);
			Expect(TokenCategory.SEMICOLON);
		}
		public void StmtDecr()
		{
			Expect(TokenCategory.DECREMENT);
			Expect(TokenCategory.SEMICOLON);
		}
		public void StmtFunCall()
		{
			FunCall();
			Expect(TokenCategory.SEMICOLON);
		}
		public void FunCall()
		{
			Expect(TokenCategory.PARENTHESIS_OPEN);
			ExprList();
			Expect(TokenCategory.PARENTHESIS_CLOSE);
		}
		public void ExprList()
		{
			if (firstOfExpr.Contains(CurrentToken))
			{
				Expr();
				ExprListCont();
			}
		}
		public void ExprListCont()
		{
			while (CurrentToken == TokenCategory.COMMA)
			{
				Expect(TokenCategory.COMMA);
				Expr();
			}
		}
		public void StmtIf()
		{
			Expect(TokenCategory.IF);
			Expect(TokenCategory.PARENTHESIS_OPEN);
			Expr();
			Expect(TokenCategory.PARENTHESIS_CLOSE);
			Expect(TokenCategory.CURLY_LEFT);
			StmtList();
			Expect(TokenCategory.CURLY_RIGHT);
			ElseIfList();
			Else();
		}
		public void ElseIfList()
		{
			while (CurrentToken == TokenCategory.ELSEIF)
			{
				Expect(TokenCategory.ELSEIF);
				Expect(TokenCategory.PARENTHESIS_OPEN);
				Expr();
				Expect(TokenCategory.PARENTHESIS_CLOSE);
				Expect(TokenCategory.CURLY_LEFT);
				StmtList();
				Expect(TokenCategory.CURLY_RIGHT);
			}
		}
		public void Else()
		{
			if (CurrentToken == TokenCategory.ELSE)
			{
				Expect(TokenCategory.ELSE);
				Expect(TokenCategory.CURLY_LEFT);
				StmtList();
				Expect(TokenCategory.CURLY_RIGHT);
			}
		}
		public void StmtWhile()
		{
			Expect(TokenCategory.WHILE);
			Expect(TokenCategory.PARENTHESIS_OPEN);
			Expr();
			Expect(TokenCategory.PARENTHESIS_CLOSE);
			Expect(TokenCategory.CURLY_LEFT);
			StmtList();
			Expect(TokenCategory.CURLY_RIGHT);
		}
		public void StmtBreak()
		{
			Expect(TokenCategory.BREAK);
			Expect(TokenCategory.SEMICOLON);
		}
		public void StmtReturn()
		{
			Expect(TokenCategory.RETURN);
			Expr();
			Expect(TokenCategory.SEMICOLON);
		}
		public void StmtEmpty()
		{
			Expect(TokenCategory.SEMICOLON);
		}
		public void Expr()
		{
			ExprOr();
		}
		public void ExprOr()
		{
			ExprAnd();
			while (CurrentToken == TokenCategory.OR)
			{
				Expect(TokenCategory.OR);
				ExprAnd();
			}
		}
		public void ExprAnd()
		{
			ExprComp();
			while (CurrentToken == TokenCategory.AND)
			{
				Expect(TokenCategory.AND);
				ExprComp();
			}
		}
		public void ExprComp()
		{
			ExprRel();
			while (firstOfOpComp.Contains(CurrentToken))
			{
				OpComp();
				ExprRel();
			}
		}
		public void OpComp()
		{
			switch (CurrentToken)
			{
				case TokenCategory.EQUAL:
					Expect(TokenCategory.EQUAL);
					break;
				case TokenCategory.DIF:
					Expect(TokenCategory.DIF);
					break;
				default:
					throw new SyntaxError(firstOfOpComp, tokenStream.Current);
			}
		}
		public void ExprRel()
		{
			ExprAdd();
			while (firstOfOpRel.Contains(CurrentToken))
			{
				OpRel();
				ExprAdd();
			}
		}
		public void OpRel()
		{
			switch (CurrentToken)
			{
				case TokenCategory.LESS:
					Expect(TokenCategory.LESS);
					break;
				case TokenCategory.LESS_EQUAL:
					Expect(TokenCategory.LESS_EQUAL);
					break;
				case TokenCategory.GREATER:
					Expect(TokenCategory.GREATER);
					break;
				case TokenCategory.GREATER_EQUAL:
					Expect(TokenCategory.GREATER_EQUAL);
					break;
				default:
					throw new SyntaxError(firstOfOpRel, tokenStream.Current);
			}
		}
		public void ExprAdd()
		{
			ExprMul();
			while (firstOfOpAdd.Contains(CurrentToken))
			{
				OpAdd();
				ExprMul();
			}
		}
		public void OpAdd()
		{
			switch (CurrentToken)
			{
				case TokenCategory.PLUS:
					Expect(TokenCategory.PLUS);
					break;
				case TokenCategory.NEG:
					Expect(TokenCategory.NEG);
					break;
				default:
					throw new SyntaxError(firstOfOpAdd, tokenStream.Current);
			}
		}
		public void ExprMul()
		{
			ExprUnary();
			while (firstOfOpMul.Contains(CurrentToken))
			{
				OpMul();
				ExprUnary();
			}
		}
		public void OpMul()
		{
			switch (CurrentToken)
			{
				case TokenCategory.MUL:
					Expect(TokenCategory.MUL);
					break;
				case TokenCategory.DIV:
					Expect(TokenCategory.DIV);
					break;
				case TokenCategory.MOD:
					Expect(TokenCategory.MOD);
					break;
				default:
					throw new SyntaxError(firstOfOpMul, tokenStream.Current);
			}
		}
		public void ExprUnary()
		{
			while (firstOfOpUnary.Contains(CurrentToken))
			{
				OpUnary();
			}
			ExprPrimary();
		}
		public void OpUnary()
		{
			switch (CurrentToken)
			{
				case TokenCategory.PLUS:
					Expect(TokenCategory.PLUS);
					break;
				case TokenCategory.NEG:
					Expect(TokenCategory.NEG);
					break;
				case TokenCategory.NOT:
					Expect(TokenCategory.NOT);
					break;
				default:
					throw new SyntaxError(firstOfOpUnary, tokenStream.Current);
			}
		}
		public void ExprPrimary()
		{
			switch (CurrentToken)
			{
				case TokenCategory.IDENTIFIER:
					Expect(TokenCategory.IDENTIFIER);
					if (CurrentToken == TokenCategory.PARENTHESIS_OPEN)
					{
						FunCall();
					}
					break;
				case TokenCategory.BRACKET_LEFT:
					Array();
					break;
				case TokenCategory.TRUE:
				case TokenCategory.FALSE:
					Lit();
					break;
				case TokenCategory.INT_LITERAL:
					Lit();
					break;
				case TokenCategory.CHAR_LITERAL:
					Lit();
					break;
				case TokenCategory.STR_LITERAL:
					Lit();
					break;
				case TokenCategory.PARENTHESIS_OPEN:
					Expect(TokenCategory.PARENTHESIS_OPEN);
					Expr();
					Expect(TokenCategory.PARENTHESIS_CLOSE);
					break;
				default:
					throw new SyntaxError(firstOfExprPrimary, tokenStream.Current);
			}
		}
		public void Array()
		{
			Expect(TokenCategory.BRACKET_LEFT);
			ExprList();
			Expect(TokenCategory.BRACKET_RIGHT);
		}
		public void Lit()
		{
			switch (CurrentToken)
			{
				case TokenCategory.TRUE:
					Expect(TokenCategory.TRUE);
					break;
				case TokenCategory.FALSE:
					Expect(TokenCategory.FALSE);
					break;
				case TokenCategory.INT_LITERAL:
					Expect(TokenCategory.INT_LITERAL);
					break;
				case TokenCategory.CHAR_LITERAL:
					Expect(TokenCategory.CHAR_LITERAL);
					break;
				case TokenCategory.STR_LITERAL:
					Expect(TokenCategory.STR_LITERAL);
					break;
				default:
					throw new SyntaxError(firstOfLit, tokenStream.Current);
			}
		}
	}
}
