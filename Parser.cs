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

		// Nodes

		public Node Program()
		{
			var program = new Program();

			program.Add(DefList());
			Expect(TokenCategory.EOF);

			return program;
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
			var varDef = new VarDef()
			{
				AnchorToken = Expect(TokenCategory.VAR)
			};
			varDef.Add(VarList());
			Expect(TokenCategory.SEMICOLON);

			return varDef;
		}
		public Node VarList()
		{
			return IdList();
		}
		public Node IdList()
		{
			var idList = new IdList();
			idList.Add(new Identifier()
			{
				AnchorToken = Expect(TokenCategory.IDENTIFIER)
			});

			if (CurrentToken == TokenCategory.COMMA)
			{
				var idListCont = IdListCont();
				foreach (var son in idListCont)
				{
					idList.Add(son);
				}
			}

			return idList;
		}
		public Node IdListCont()
		{
			var idListCont = new IdListCont();
			while (CurrentToken == TokenCategory.COMMA)
			{
				Expect(TokenCategory.COMMA);
				idListCont.Add(new Identifier()
				{
					AnchorToken = Expect(TokenCategory.IDENTIFIER)
				});
			}

			return idListCont;
		}
		public Node FunDef()
		{
			var funDef = new FunDef()
			{
				AnchorToken = Expect(TokenCategory.IDENTIFIER)
			};

			Expect(TokenCategory.PARENTHESIS_OPEN);
			var paramList = ParamList();
			funDef.Add(paramList);
			Expect(TokenCategory.PARENTHESIS_CLOSE);
			Expect(TokenCategory.CURLY_LEFT);
			var varDefList = VarDefList();
			funDef.Add(varDefList);
			var stmtList = StmtList();
			funDef.Add(stmtList);
			Expect(TokenCategory.CURLY_RIGHT);

			return funDef;
		}
		public Node ParamList()
		{
			if (CurrentToken == TokenCategory.IDENTIFIER)
			{
				return IdList();
			}

			return new ParamList();
		}
		public Node VarDefList()
		{
			var varDefList = new VarDefList();
			while (CurrentToken == TokenCategory.VAR)
			{
				varDefList.Add(VarDef());
			}

			return varDefList;
		}
		public Node StmtList()
		{
			var stmtList = new StmtList();
			while (firstOfStmt.Contains(CurrentToken))
			{
				stmtList.Add(Stmt());
			}
			return stmtList;
		}
		public Node Stmt()
		{
			switch (CurrentToken)
			{
				case TokenCategory.IDENTIFIER:
					Token anchor = Expect(TokenCategory.IDENTIFIER);

					switch (CurrentToken)
					{
						case TokenCategory.ASSIGN:
							return StmtAssign(anchor);
						case TokenCategory.INCREMENT:
							return StmtIncr(anchor);
						case TokenCategory.DECREMENT:
							return StmtDecr(anchor);
						default:
							return StmtFunCall(anchor);
					}
				case TokenCategory.IF:
					return StmtIf();
				case TokenCategory.WHILE:
					return StmtWhile();
				case TokenCategory.BREAK:
					return StmtBreak();
				case TokenCategory.RETURN:
					return StmtReturn();
				case TokenCategory.SEMICOLON:
					return StmtEmpty();
				default:
					throw new SyntaxError(firstOfStmt, tokenStream.Current);
			}
		}
		public Node StmtAssign(Token anchor)
		{
			var stmtAssign = new StmtAssign()
			{
				AnchorToken = anchor
			};
			Expect(TokenCategory.ASSIGN);
			stmtAssign.Add(Expr());
			Expect(TokenCategory.SEMICOLON);

			return stmtAssign;
		}
		public Node StmtIncr(Token anchor)
		{
			var stmtIncr = new StmtIncr()
			{
				AnchorToken = anchor
			};
			Expect(TokenCategory.INCREMENT);
			Expect(TokenCategory.SEMICOLON);

			return stmtIncr;

		}
		public Node StmtDecr(Token anchor)
		{
			var stmtDecr = new StmtDecr()
			{
				AnchorToken = anchor
			};
			Expect(TokenCategory.DECREMENT);
			Expect(TokenCategory.SEMICOLON);

			return stmtDecr;
		}
		public Node StmtFunCall(Token anchor)
		{
			var stmtFunCall = FunCall(anchor);
			Expect(TokenCategory.SEMICOLON);

			return stmtFunCall;
		}
		public Node FunCall(Token anchor)
		{
			var funCall = new FunCall()
			{
				AnchorToken = anchor
			};
			Expect(TokenCategory.PARENTHESIS_OPEN);
			funCall.Add(ExprList());
			Expect(TokenCategory.PARENTHESIS_CLOSE);
			return funCall;
		}
		public Node ExprList()
		{
			var exprList = new ExprList();

			if (firstOfExpr.Contains(CurrentToken))
			{
				exprList.Add(Expr());

				if (CurrentToken == TokenCategory.COMMA)
				{
					var exprListCont = ExprListCont();
					foreach (var son in exprListCont)
					{
						exprList.Add(son);
					}
				}
			}
			return exprList;
		}

		public Node ExprListCont()
		{
			var exprListCont = new ExprListCont();
			while (CurrentToken == TokenCategory.COMMA)
			{
				Expect(TokenCategory.COMMA);
				exprListCont.Add(Expr());
			}

			return exprListCont;

		}
		public Node StmtIf()
		{
			var stmtIf = new StmtIf()
			{
				AnchorToken = Expect(TokenCategory.IF)
			};

			Expect(TokenCategory.PARENTHESIS_OPEN);
			stmtIf.Add(Expr());
			Expect(TokenCategory.PARENTHESIS_CLOSE);
			Expect(TokenCategory.CURLY_LEFT);
			stmtIf.Add(StmtList());
			Expect(TokenCategory.CURLY_RIGHT);
			stmtIf.Add(ElseIfList());
			stmtIf.Add(Else());

			return stmtIf;

		}
		public Node ElseIfList()
		{
			var elseIfList = new ElseIfList();

			while (CurrentToken == TokenCategory.ELSEIF)
			{
				var elseIf = new ElseIf()
				{
					AnchorToken = Expect(TokenCategory.ELSEIF)
				};
				Expect(TokenCategory.PARENTHESIS_OPEN);
				elseIf.Add(Expr());
				Expect(TokenCategory.PARENTHESIS_CLOSE);
				Expect(TokenCategory.CURLY_LEFT);
				elseIf.Add(StmtList());
				Expect(TokenCategory.CURLY_RIGHT);

				elseIfList.Add(elseIf);
			}

			return elseIfList;
		}
		public Node Else()
		{
			if (CurrentToken == TokenCategory.ELSE)
			{
				var elseNode = new Else()
				{
					AnchorToken = Expect(TokenCategory.ELSE)
				};
				Expect(TokenCategory.CURLY_LEFT);
				elseNode.Add(StmtList());
				Expect(TokenCategory.CURLY_RIGHT);
				return elseNode;
			}
			return new Else();
			
		}
		public Node StmtWhile()
		{
			var stmtWhile = new StmtWhile()
			{
				AnchorToken = Expect(TokenCategory.WHILE)
			};
			Expect(TokenCategory.PARENTHESIS_OPEN);
			stmtWhile.Add(Expr());
			Expect(TokenCategory.PARENTHESIS_CLOSE);
			Expect(TokenCategory.CURLY_LEFT);
			stmtWhile.Add(StmtList());
			Expect(TokenCategory.CURLY_RIGHT);

			return stmtWhile;

		}
		public Node StmtBreak()
		{
			var stmtBreak = new StmtBreak()
			{
				AnchorToken = Expect(TokenCategory.BREAK)
			};
			Expect(TokenCategory.SEMICOLON);

			return stmtBreak;
		}
		public Node StmtReturn()
		{
			var stmtReturn = new StmtReturn()
			{
				AnchorToken = Expect(TokenCategory.RETURN)
			};
			stmtReturn.Add(Expr());
			Expect(TokenCategory.SEMICOLON);

			return stmtReturn;
		}
		public Node StmtEmpty()
		{
			var stmtEmpty = new StmtEmpty()
			{
				AnchorToken = Expect(TokenCategory.SEMICOLON)
			};
			
			return stmtEmpty;

		}
		public Node Expr()
		{
			return ExprOr();
		}
		public Node ExprOr()
		{
			var currentExpr = ExprAnd();

			while (CurrentToken == TokenCategory.OR)
			{
				var exprOr = new Or()
				{
					AnchorToken = Expect(TokenCategory.OR)
				};
				exprOr.Add(currentExpr);
				exprOr.Add(ExprAnd());

				currentExpr = exprOr;
			}
	
			return currentExpr;
		}
		public Node ExprAnd()
		{
			var currentExpr = ExprComp();

			while (CurrentToken == TokenCategory.AND)
			{
				var exprAnd = new And()
				{
					AnchorToken = Expect(TokenCategory.AND)
				};
				exprAnd.Add(currentExpr);
				exprAnd.Add(ExprComp());

				currentExpr = exprAnd;
			}

			return currentExpr;
		}
		public Node ExprComp()
		{
			var currentExpr = ExprRel();

			while (firstOfOpComp.Contains(CurrentToken))
			{
				var opComp = OpComp();
				opComp.Add(currentExpr);
				opComp.Add(ExprRel());

				currentExpr = opComp;
			}
			return currentExpr;
		}
		public Node OpComp()
		{
			switch (CurrentToken)
			{
				case TokenCategory.EQUAL:
					return new Equal()
					{
						AnchorToken = Expect(TokenCategory.EQUAL)
					};
				case TokenCategory.DIF:
					return new Dif()
					{
						AnchorToken = Expect(TokenCategory.DIF)
					};
				default:
					throw new SyntaxError(firstOfOpComp, tokenStream.Current);
			}
		}
		public Node ExprRel()
		{
			var currentExpr = ExprAdd();

			while (firstOfOpRel.Contains(CurrentToken))
			{
				var opRel = OpRel();
				opRel.Add(currentExpr);
				opRel.Add(ExprAdd());

				currentExpr = opRel;
			}

			return currentExpr;
		}
		public Node OpRel()
		{
			switch (CurrentToken)
			{
				case TokenCategory.LESS:
					return new Less()
					{
						AnchorToken = Expect(TokenCategory.LESS)
					};
				case TokenCategory.LESS_EQUAL:
					return new LessEqual()
					{
						AnchorToken = Expect(TokenCategory.LESS_EQUAL)
					};
				case TokenCategory.GREATER:
					return new Greater()
					{
						AnchorToken = Expect(TokenCategory.GREATER)
					};
				case TokenCategory.GREATER_EQUAL:
					return new GreaterEqual()
					{
						AnchorToken = Expect(TokenCategory.GREATER_EQUAL)
					};
				default:
					throw new SyntaxError(firstOfOpRel, tokenStream.Current);
			}
		}
		public Node ExprAdd()
		{
			var currentExpr = ExprMul();

			while (firstOfOpAdd.Contains(CurrentToken))
			{
				var opAdd = OpAdd();
				opAdd.Add(currentExpr);
				opAdd.Add(ExprMul());

				currentExpr = opAdd;
			}

			return currentExpr;
		}
		public Node OpAdd()
		{
			switch (CurrentToken)
			{
				case TokenCategory.PLUS:
					return new Plus()
					{
						AnchorToken = Expect(TokenCategory.PLUS)
					};
				case TokenCategory.NEG:
					return new Neg()
					{
						AnchorToken = Expect(TokenCategory.NEG)
					};
				default:
					throw new SyntaxError(firstOfOpAdd, tokenStream.Current);
			}
		}
		public Node ExprMul()
		{
			var currentExpr = ExprUnary();

			while (firstOfOpMul.Contains(CurrentToken))
			{
				var opMul = OpMul();
				opMul.Add(currentExpr);
				opMul.Add(ExprUnary());

				currentExpr = opMul;
			}

			return currentExpr;
		}
		public Node OpMul()
		{
			switch (CurrentToken)
			{
				case TokenCategory.MUL:
					return new Mul()
					{
						AnchorToken = Expect(TokenCategory.MUL)
					};
				case TokenCategory.DIV:
					return new Div()
					{
						AnchorToken = Expect(TokenCategory.DIV)
					};
				case TokenCategory.MOD:
					return new Mod()
					{
						AnchorToken = Expect(TokenCategory.MOD)
					};
				default:
					throw new SyntaxError(firstOfOpMul, tokenStream.Current);
			}
		}
		public Node ExprUnary()
		{
			if (!firstOfOpUnary.Contains(CurrentToken))
			{
				return ExprPrimary();
			}

			var firstExpr = OpUnary();
			var currentExpr = firstExpr;
			while (firstOfOpUnary.Contains(CurrentToken))
			{
				var opUnary = OpUnary();
				currentExpr.Add(opUnary);

				currentExpr = currentExpr[0];
			}
			currentExpr.Add(ExprPrimary());

			return firstExpr;
		}
		public Node OpUnary()
		{
			switch (CurrentToken)
			{
				case TokenCategory.PLUS:
					return new Plus()
					{
						AnchorToken = Expect(TokenCategory.PLUS)
					};
				case TokenCategory.NEG:
					return new Neg()
					{
						AnchorToken = Expect(TokenCategory.NEG)
					};
				case TokenCategory.NOT:
					return new Not()
					{
						AnchorToken = Expect(TokenCategory.NOT)
					};
				default:
					throw new SyntaxError(firstOfOpUnary, tokenStream.Current);
			}
		}
		public Node ExprPrimary()
		{
			switch (CurrentToken)
			{
				case TokenCategory.IDENTIFIER:
					Token anchor = Expect(TokenCategory.IDENTIFIER);
					if (CurrentToken == TokenCategory.PARENTHESIS_OPEN)
					{
						return FunCall(anchor);
					}
					return new Identifier()
					{
						AnchorToken = anchor
					};
				case TokenCategory.BRACKET_LEFT:
					return Array();
				case TokenCategory.TRUE:
				case TokenCategory.FALSE:
				case TokenCategory.INT_LITERAL:
				case TokenCategory.CHAR_LITERAL:
				case TokenCategory.STR_LITERAL:
					return Lit();
				case TokenCategory.PARENTHESIS_OPEN:
					Expect(TokenCategory.PARENTHESIS_OPEN);
					var expr = Expr();
					Expect(TokenCategory.PARENTHESIS_CLOSE);
					return expr;
				default:
					throw new SyntaxError(firstOfExprPrimary, tokenStream.Current);
			}
		}
		public Node Array()
		{
			var array = new ArrayToken();
			Expect(TokenCategory.BRACKET_LEFT);
			array.Add(ExprList());
			Expect(TokenCategory.BRACKET_RIGHT);
			return array;
		}
		public Node Lit()
		{
			switch (CurrentToken)
			{
				case TokenCategory.TRUE:
					return new True()
					{
						AnchorToken = Expect(TokenCategory.TRUE)
					};
				case TokenCategory.FALSE:
					return new False()
					{
						AnchorToken = Expect(TokenCategory.FALSE)
					};
				case TokenCategory.INT_LITERAL:
					return new IntLiteral()
					{
						AnchorToken = Expect(TokenCategory.INT_LITERAL)
					};
				case TokenCategory.CHAR_LITERAL:
					return new CharLiteral()
					{
						AnchorToken = Expect(TokenCategory.CHAR_LITERAL)
					};
				case TokenCategory.STR_LITERAL:
					return new StrLiteral()
					{
						AnchorToken = Expect(TokenCategory.STR_LITERAL)
					};
				default:
					throw new SyntaxError(firstOfLit, tokenStream.Current);
			}
		}
	}
}
