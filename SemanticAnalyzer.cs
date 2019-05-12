/*
  Wyvern compiler - Semantic analyzer.
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

  class SemanticAnalyzer
  {

    String current = "global";
    bool isParam = false;
    bool isDef = false;
    int whileCount = 0;

    //-----------------------------------------------------------
    static readonly IDictionary<TokenCategory, Type> typeMapper =
        new Dictionary<TokenCategory, Type>() {
                { TokenCategory.CHAR_LITERAL, Type.CHAR_LITERAL },
                { TokenCategory.INT_LITERAL, Type.INT_LITERAL },
                { TokenCategory.STR_LITERAL, Type.STR_LITERAL },
                { TokenCategory.IDENTIFIER, Type.IDENTIFIER },
                { TokenCategory.TRUE, Type.TRUE },
                { TokenCategory.FALSE, Type.FALSE },
        };

    //-----------------------------------------------------------
    public SymbolTable Globals
    {
      get;
      private set;
    }

    public FunctionSymbolTable Functions
    {
      get;
      private set;
    }

    //-----------------------------------------------------------
    public SemanticAnalyzer()
    {

      Globals = new SymbolTable();
      Functions = new FunctionSymbolTable();

      Functions["printi"] = new FunctionTable(1);
      Functions["printc"] = new FunctionTable(1);
      Functions["prints"] = new FunctionTable(1);
      Functions["println"] = new FunctionTable(0);
      Functions["readi"] = new FunctionTable(0);
      Functions["reads"] = new FunctionTable(0);
      Functions["new"] = new FunctionTable(1);
      Functions["size"] = new FunctionTable(1);
      Functions["add"] = new FunctionTable(2);
      Functions["get"] = new FunctionTable(2);
      Functions["set"] = new FunctionTable(3);
    }

    //-----------------------------------------------------------
    public Type Visit(Program node)
    {
      Visit((dynamic) node[0]);

      if (!Functions.Contains("main"))
      {
        throw new SemanticError("Function main was not found");
      }
      return Type.VOID;
    }

    public Type Visit(DefList node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(VarDef node)
    {
      isDef = true;
      VisitChildren(node);
      isDef = false;

      return Type.VOID;
    }

    public Type Visit(IdList node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(FunDef node)
    {
      var functionName = node.AnchorToken.Lexeme;
      current = functionName;

      if (functionName == "main" && node[0].CountChildren() > 0)
      {
        throw new SemanticError(
                        "Function main does not expect to recieve parameters",
                        node.AnchorToken);
      }

      if (Functions.Contains(functionName))
      {
        throw new SemanticError(
            "Duplicated function: " + functionName,
            node[0].AnchorToken);
      }
      else
      {
        Functions[functionName] =
            new FunctionTable(node[0].CountChildren(), false);
      }

      // ParamList
      isParam = true;
      isDef = true;
      Visit((dynamic)node[0]);
      isParam = false;
      isDef = false;

      // VarDefList
      Visit((dynamic)node[1]);

      // StmtList
      Visit((dynamic)node[2]);

      current = "global";
      return Type.VOID;
    }

    public Type Visit(ParamList node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(VarDefList node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(StmtList node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(StmtAssign node)
    {
      var variableName = node.AnchorToken.Lexeme;

      if (!Globals.Contains(variableName) && !Functions[current].funsymtable.ContainsKey(variableName))
      {
        throw new SemanticError(
            "Undeclared variable: " + variableName,
            node.AnchorToken);
      }
      
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(StmtIncr node)
    {
      return Type.VOID;
    }

    public Type Visit(StmtDecr node)
    {
      return Type.VOID;
    }

    public Type Visit(FunCall node)
    {
      var functionName = node.AnchorToken.Lexeme;
      var numOfParams = node[0].CountChildren();

      if (!Functions.Contains(functionName))
      {
        throw new SemanticError(
              "Function call to undeclared function: " + functionName,
              node.AnchorToken);
      }

      if (Functions[functionName].arity != numOfParams)
      {
        throw new SemanticError(
              "Function call expects different number of params: " + functionName,
              node.AnchorToken);
      }

      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(ExprList node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(StmtIf node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(ElseIfList node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(ElseIf node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(Else node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(StmtWhile node)
    {
      whileCount++;
      VisitChildren(node);
      whileCount--;
      return Type.VOID;
    }

    public Type Visit(StmtBreak node)
    {
      if (whileCount <= 0)
      {
        throw new SemanticError(
          "Break not in while loop",
          node.AnchorToken);
      }
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(StmtReturn node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(StmtEmpty node)
    {
      return Type.VOID;
    }

    public Type Visit(Or node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(And node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(Equal node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(Dif node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(Less node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(LessEqual node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(Greater node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(GreaterEqual node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(Mul node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(Div node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(Mod node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(Plus node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(Neg node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(Not node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    public Type Visit(Identifier node)
    {
      if (isDef)
      {
        var variableName = node.AnchorToken.Lexeme;

        if (current == "global")
        {
          if (Globals.Contains(variableName))
          {
            throw new SemanticError(
                "Duplicated variable: " + variableName,
                node.AnchorToken);
          }
          else
          {
            Globals[variableName] =
                typeMapper[node.AnchorToken.Category];
          }
        }
        else
        {
          if (Functions[current].Contains(variableName))
          {
            throw new SemanticError(
                "Duplicated variable: " + variableName,
                node.AnchorToken);
          }
          else
          {
            Functions[current].funsymtable.Add(variableName, isParam);
          }
        }

        return Type.VOID;
      }

      return Type.IDENTIFIER;

    }

    public Type Visit(True node)
    {
      return Type.TRUE;
    }

    public Type Visit(False node)
    {
      return Type.FALSE;
    }

    public Type Visit(IntLiteral node)
    {
      try
      {
        Convert.ToInt32(node.AnchorToken.Lexeme);
      }
      catch (OverflowException)
      {
        throw new SemanticError(
              "Integer exceedes 32 bits",
              node.AnchorToken);
      }
      return Type.INT_LITERAL;
    }

    public Type Visit(CharLiteral node)
    {
      return Type.CHAR_LITERAL;
    }

    public Type Visit(StrLiteral node)
    {
      return Type.STR_LITERAL;
    }

    public Type Visit(ArrayToken node)
    {
      VisitChildren(node);
      return Type.VOID;
    }

    //-----------------------------------------------------------
    // public Type Visit(DeclarationList node) {
    //     VisitChildren(node);
    //     return Type.VOID;
    // }

    // //-----------------------------------------------------------
    // public Type Visit(Declaration node) {

    //     var variableName = node[0].AnchorToken.Lexeme;

    //     if (Table.Contains(variableName)) {
    //         throw new SemanticError(
    //             "Duplicated variable: " + variableName,
    //             node[0].AnchorToken);

    //     } else {
    //         Table[variableName] = 
    //             typeMapper[node.AnchorToken.Category];              
    //     }

    //     return Type.VOID;
    // }

    // //-----------------------------------------------------------
    // public Type Visit(StatementList node) {
    //     VisitChildren(node);
    //     return Type.VOID;
    // }

    // //-----------------------------------------------------------
    // public Type Visit(Assignment node) {

    //     var variableName = node.AnchorToken.Lexeme;

    //     if (Table.Contains(variableName)) {

    //         var expectedType = Table[variableName];

    //         if (expectedType != Visit((dynamic) node[0])) {
    //             throw new SemanticError(
    //                 "Expecting type " + expectedType 
    //                 + " in assignment statement",
    //                 node.AnchorToken);
    //         }

    //     } else {
    //         throw new SemanticError(
    //             "Undeclared variable: " + variableName,
    //             node.AnchorToken);
    //     }

    //     return Type.VOID;
    // }

    //-----------------------------------------------------------
    // public Type Visit(Print node) {
    //     node.ExpressionType = Visit((dynamic) node[0]);
    //     return Type.VOID;
    // }

    // //-----------------------------------------------------------
    // public Type Visit(If node) {
    //     if (Visit((dynamic) node[0]) != Type.BOOL) {
    //         throw new SemanticError(
    //             "Expecting type " + Type.BOOL 
    //             + " in conditional statement",                   
    //             node.AnchorToken);
    //     }
    //     VisitChildren(node[1]);
    //     return Type.VOID;
    // }

    // //-----------------------------------------------------------
    // public Type Visit(Identifier node) {

    //     var variableName = node.AnchorToken.Lexeme;

    //     if (Table.Contains(variableName)) {
    //         return Table[variableName];
    //     }

    //     throw new SemanticError(
    //         "Undeclared variable: " + variableName,
    //         node.AnchorToken);
    // }

    // //-----------------------------------------------------------
    // public Type Visit(IntLiteral node) {

    //     var intStr = node.AnchorToken.Lexeme;

    //     try {
    //         Convert.ToInt32(intStr);

    //     } catch (OverflowException) {
    //         throw new SemanticError(
    //             "Integer literal too large: " + intStr, 
    //             node.AnchorToken);
    //     }

    //     return Type.INT;
    // }

    // //-----------------------------------------------------------
    // public Type Visit(True node) {
    //     return Type.BOOL;
    // }

    // //-----------------------------------------------------------
    // public Type Visit(False node) {
    //     return Type.BOOL;
    // }

    // //-----------------------------------------------------------
    // public Type Visit(Neg node) {          
    //     if (Visit((dynamic) node[0]) != Type.INT) {
    //         throw new SemanticError(
    //             "Operator - requires an operand of type " + Type.INT,
    //             node.AnchorToken);
    //     }
    //     return Type.INT;
    // }

    // //-----------------------------------------------------------
    // public Type Visit(And node) {
    //     VisitBinaryOperator('&', node, Type.BOOL);
    //     return Type.BOOL;
    // }

    // //-----------------------------------------------------------
    // public Type Visit(Less node) {
    //     VisitBinaryOperator('<', node, Type.INT);
    //     return Type.BOOL;
    // }

    // //-----------------------------------------------------------
    // public Type Visit(Plus node) {
    //     VisitBinaryOperator('+', node, Type.INT);
    //     return Type.INT;
    // }

    // //-----------------------------------------------------------
    // public Type Visit(Mul node) {
    //     VisitBinaryOperator('*', node, Type.INT);
    //     return Type.INT;
    // }

    //-----------------------------------------------------------
    // void VisitChildren(Node node)
    // {
    //   foreach (var n in node)
    //   {
    //     Visit((dynamic)n);
    //   }
    // }

    void VisitChildren(Node node)
    {
      foreach (var n in node)
      {
        Visit((dynamic)n);
      }
    }

    //-----------------------------------------------------------
    void VisitBinaryOperator(char op, Node node, Type type)
    {
      if (Visit((dynamic)node[0]) != type ||
          Visit((dynamic)node[1]) != type)
      {
        throw new SemanticError(
            String.Format(
                "Operator {0} requires two operands of type {1}",
                op,
                type),
            node.AnchorToken);
      }
    }
  }
}
