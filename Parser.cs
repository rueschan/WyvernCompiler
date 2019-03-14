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

		public Token Expect(TokenCategory category) {
            if (CurrentToken == category) {
                Token current = tokenStream.Current;
                tokenStream.MoveNext();
                return current;
            } else {
                throw new SyntaxError(category, tokenStream.Current);                
            }
        }

		public void Program() {
			DefList();
			Expect(TokenCategory.EOF);
		}
		public void DefList() {
			while (firstOfDef.Contains(CurrentToken)) {
				Def();
			}
		}
		public void Def() {
			switch (CurrentToken) {
				case TokenCategory.VAR:
					VarDef();
					break;
				case TokenCategory.IDENTIFIER:
					FunDef();
					break;
				default:
					throw new SyntaxError(firstOfDef, tokenStream.Current);
			}
		}
		public void VarDef() {
			Expect(TokenCategory.VAR);
			VarList();
			Expect(TokenCategory.SEMICOLON);
		}
		public void VarList() {
			IdList();
		}
		public void IdList() {
			Expect(TokenCategory.IDENTIFIER);
			IdListCont();
		}
		public void IdListCont() {
			while (CurrentToken == TokenCategory.COMMA) {
				Expect(TokenCategory.COMMA);
				Expect(TokenCategory.IDENTIFIER);
			}
		}
		public void FunDef() {
			Expect(TokenCategory.IDENTIFIER);
			Expect(TokenCategory.PARENTHESIS_OPEN);
			ParamList();
			Expect(TokenCategory.PARENTHESIS_CLOSE);
			Expect(TokenCategory.CURLY_LEFT);
			VarDefList();
			StmtList();
			Expect(TokenCategory.CURLY_RIGHT);
		}
		public void ParamList() {
			if (CurrentToken == TokenCategory.IDENTIFIER) {
				IdList();
			}
		}
		public void VarDefList() {
			while (CurrentToken == TokenCategory.VAR) {
				VarDef();
			}
		}
		public void StmtList() {
			while (firstOfStmt.Contains(CurrentToken)) {
				Stmt();
			}
		}
		public void Stmt() {
			switch (CurrentToken) {
				case TokenCategory.IDENTIFIER:
					VarDef();
					break;
				case TokenCategory.IF:
					FunDef();
					break;
				case TokenCategory.WHILE:
					FunDef();
					break;
				case TokenCategory.BREAK:
					FunDef();
					break;
				case TokenCategory.RETURN:
					FunDef();
					break;
				case TokenCategory.SEMICOLON:
					FunDef();
					break;
				default:
					throw new SyntaxError(firstOfStmt, tokenStream.Current);
			}
		}
		public void StmtAssign() {}
		public void StmtIncr() {}
		public void StmtDecr() {}
		public void StmtFunCall() {}
		public void FunCall() {}
		public void ExprList() {}
		public void ExprListCont() {}
		public void StmtIf() {}
		public void ElseIfList() {}
		public void Else() {}
		public void StmtWhile() {}
		public void StmtBreak() {}
		public void StmtReturn() {}
		public void StmtEmpty() {}
		public void Expr() {}
		public void ExprOr() {}
		public void ExprAnd() {}
		public void ExprComp() {}
		public void OpComp() {}
		public void ExprRel() {}
		public void OpRel() {}
		public void ExprAdd() {}
		public void OpAdd() {}
		public void ExprMul() {}
		public void OpMul() {}
		public void ExprUnary() {}
		public void OpUnary() {}
		public void ExprPrimary() {}
		public void OpPrimary() {}
		public void Array() {}
		public void Lit() {}

		// public void Program()
		// {

		// 	while (firstOfDeclaration.Contains(CurrentToken))
		// 	{
		// 		Declaration();
		// 	}

		// 	while (firstOfStatement.Contains(CurrentToken))
		// 	{
		// 		Statement();
		// 	}

		// 	Expect(TokenCategory.EOF);
		// }

		// public void Declaration()
		// {
		// 	Type();
		// 	Expect(TokenCategory.IDENTIFIER);
		// }

		// public void Statement()
		// {

		// 	switch (CurrentToken)
		// 	{

		// 		case TokenCategory.IDENTIFIER:
		// 			Assignment();
		// 			break;

		// 		case TokenCategory.PRINT:
		// 			Print();
		// 			break;

		// 		case TokenCategory.IF:
		// 			If();
		// 			break;

		// 		default:
		// 			throw new SyntaxError(firstOfStatement,
		// 								  tokenStream.Current);
		// 	}
		// }

		// public void Type()
		// {
		// 	switch (CurrentToken)
		// 	{

		// 		case TokenCategory.INT:
		// 			Expect(TokenCategory.INT);
		// 			break;

		// 		case TokenCategory.BOOL:
		// 			Expect(TokenCategory.BOOL);
		// 			break;

		// 		default:
		// 			throw new SyntaxError(firstOfDeclaration,
		// 								  tokenStream.Current);
		// 	}
		// }

		// public void Assignment()
		// {
		// 	Expect(TokenCategory.IDENTIFIER);
		// 	Expect(TokenCategory.ASSIGN);
		// 	Expression();
		// }

		// public void Print()
		// {
		// 	Expect(TokenCategory.PRINT);
		// 	Expression();
		// }

		// public void If()
		// {
		// 	Expect(TokenCategory.IF);
		// 	Expression();
		// 	Expect(TokenCategory.THEN);
		// 	while (firstOfStatement.Contains(CurrentToken))
		// 	{
		// 		Statement();
		// 	}
		// 	Expect(TokenCategory.END);
		// }

		// public void Expression()
		// {
		// 	SimpleExpression();
		// 	while (firstOfOperator.Contains(CurrentToken))
		// 	{
		// 		Operator();
		// 		SimpleExpression();
		// 	}
		// }

		// public void SimpleExpression()
		// {

		// 	switch (CurrentToken)
		// 	{

		// 		case TokenCategory.IDENTIFIER:
		// 			Expect(TokenCategory.IDENTIFIER);
		// 			break;

		// 		case TokenCategory.INT_LITERAL:
		// 			Expect(TokenCategory.INT_LITERAL);
		// 			break;

		// 		case TokenCategory.TRUE:
		// 			Expect(TokenCategory.TRUE);
		// 			break;

		// 		case TokenCategory.FALSE:
		// 			Expect(TokenCategory.FALSE);
		// 			break;

		// 		case TokenCategory.PARENTHESIS_OPEN:
		// 			Expect(TokenCategory.PARENTHESIS_OPEN);
		// 			Expression();
		// 			Expect(TokenCategory.PARENTHESIS_CLOSE);
		// 			break;

		// 		case TokenCategory.NEG:
		// 			Expect(TokenCategory.NEG);
		// 			SimpleExpression();
		// 			break;

		// 		default:
		// 			throw new SyntaxError(firstOfSimpleExpression,
		// 								  tokenStream.Current);
		// 	}
		// }

		// public void Operator()
		// {

		// 	switch (CurrentToken)
		// 	{

		// 		case TokenCategory.AND:
		// 			Expect(TokenCategory.AND);
		// 			break;

		// 		case TokenCategory.LESS:
		// 			Expect(TokenCategory.LESS);
		// 			break;

		// 		case TokenCategory.PLUS:
		// 			Expect(TokenCategory.PLUS);
		// 			break;

		// 		case TokenCategory.MUL:
		// 			Expect(TokenCategory.MUL);
		// 			break;

		// 		default:
		// 			throw new SyntaxError(firstOfOperator,
		// 								  tokenStream.Current);
		// 	}
		// }
	}
}
