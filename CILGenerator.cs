/* CILGenerator.cs
 * Wyvern Compiler
 * Authors:
 *			A01370880 Rub�n Escalante Chan
 *			A01371036 Santiago Nakakawa Bernal
 *			A01371240 Iv�n Rangel Varela
 */

/*
  Wyvern compiler - Common Intermediate Language (CIL) code generator.
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
using System.Text;
using System.Collections.Generic;

namespace Wyvern
{

  class CILGenerator
  {

    SymbolTable globals;
    FunctionSymbolTable functions;

    int labelCounter = 0;
    bool isGlobals = false;
    bool inFunction = false;
    FunctionTable row = new FunctionTable(0);
    string functionName = "";

    List<string> functionReturn = new List<string>();
    Stack<string> endIfTagStack = new Stack<string>();
    Stack<string> whileTagStack = new Stack<string>();

    //-----------------------------------------------------------
    string GenerateLabel()
    {
      return String.Format("${0:000000}", labelCounter++);
    }

    //-----------------------------------------------------------

    public CILGenerator(SymbolTable globals, FunctionSymbolTable functions)
    {
      this.globals = globals;
      this.functions = functions;

      functionReturn.Add("readi");
      functionReturn.Add("reads");
      functionReturn.Add("new");
      functionReturn.Add("size");
      functionReturn.Add("get");
    }

    // **********************************************************

    public string Visit(Program node)
    {
      return "// Code generated by the wyvern compiler.\n\n"
        + ".assembly 'wyvern' {}\n\n"
        + ".assembly extern 'wyvernlib' {}\n\n"
        + ".class public 'Wyvern' extends "
        + "['mscorlib']'System'.'Object'\n"
        + "\t{\n"
        + Visit((dynamic)node[0])
        + "}\n";
    }

    public string Visit(DefList node)
    {
      var sb = new StringBuilder();
      sb.Append(VisitChildren(node));
      return sb.ToString();
    }

    public string Visit(VarDef node)
    {
      var sb = new StringBuilder();
      if (inFunction)
      {
        // The code for the local variable declarations is 
        // generated directly from the symbol table, not from 
        // the AST nodes.
        foreach (var entry in row.funsymtable)
        {
          if (entry.Value) continue;

          sb.Append(String.Format(
                  "\t\t.locals init ({0} '{1}')\n",
                  "int32",
                  entry.Key)
                );
        }

        isGlobals = true;
        return sb.ToString();
      }

      if (isGlobals) return "";

      // The code for the local variable declarations is 
      // generated directly from the symbol table, not from 
      // the AST nodes.
      foreach (var entry in globals)
      {
        sb.Append(String.Format(
          "\t\t.field public static {0} '{1}'\n",
          "int32",
          entry.Key)
        );
      }

      isGlobals = true;
      return sb.ToString();
    }

    public string Visit(IdList node)
    {
      var sb = new StringBuilder();

      for (int i = 0; i < node.CountChildren(); i++)
      {
        if (i != node.CountChildren() - 1)
        {
          sb.Append("int32 '" + node[i].AnchorToken.Lexeme + "', ");
        }
        else
        {
          sb.Append("int32 '" + node[i].AnchorToken.Lexeme + "'");
        }
      }
      return sb.ToString();
    }

    public string Visit(FunDef node)
    {
      inFunction = true;
      row = functions[node.AnchorToken.Lexeme];
      functionName = node.AnchorToken.Lexeme;

      // The code for the local variable declarations is 
      // generated directly from the symbol table, not from 
      // the AST nodes.
      var sb = new StringBuilder();
      sb.Append("\t.method public static hidebysig\n");
      sb.Append(String.Format(
          "\t\tdefault int32 '{0}' (",
          node.AnchorToken.Lexeme)
      );
      sb.Append(Visit((dynamic)node[0])); // ParamList
      sb.Append(") cil managed\n\t{\n");
      if (node.AnchorToken.Lexeme == "main")
        sb.Append("\t\t.entrypoint\n");
      sb.Append("\t\t.maxstack 8\n");
      sb.Append(Visit((dynamic)node[1]));	// VarDefList
      sb.Append(Visit((dynamic)node[2])); // StmtList
      // Return
      sb.Append("\t\tldc.i4.0\n");
      sb.Append("\t\tret\n\t}\n");

      inFunction = false;
      return sb.ToString();
    }

    public string Visit(ParamList node)
    {
      // This method is never called.
      return "";
    }

    public string Visit(VarDefList node)
    {
      var sb = new StringBuilder();
      sb.Append(VisitChildren(node));
      return sb.ToString();
    }

    public string Visit(StmtList node)
    {
      var sb = new StringBuilder();
      sb.Append(VisitChildren(node));
      return sb.ToString();
    }

    public string Visit(StmtAssign node)
    {
      StringBuilder sb = new StringBuilder();

      if (globals.Contains(node.AnchorToken.Lexeme))
      {
        sb.Append(
          Visit((dynamic)node[0]));
        sb.Append(
          String.Format("\t\tstsfld int32 'Wyvern'::'{0}'\n", node.AnchorToken.Lexeme));
      }
      else if (!row.funsymtable[node.AnchorToken.Lexeme]) // Local
      {
        sb.Append(
          Visit((dynamic)node[0]));
        sb.Append(
          String.Format("\t\tstloc '{0}'\n", node.AnchorToken.Lexeme));
      }
      else //Parameter
      {
        sb.Append(
          Visit((dynamic)node[0]));
        sb.Append(
          String.Format("\t\tstarg '{0}'\n", node.AnchorToken.Lexeme));
      }

      return sb.ToString();
    }

    public string Visit(StmtIncr node)
    {
      StringBuilder sb = new StringBuilder();

      if (globals.Contains(node.AnchorToken.Lexeme))
      {
        sb.Append(
          String.Format("\t\tldsfld int32 'Wyvern'::'{0}'\n", node.AnchorToken.Lexeme));
      }
      else if (!row.funsymtable[node.AnchorToken.Lexeme]) // Local
      {
        sb.Append(
          String.Format("\t\tldloc '{0}'\n", node.AnchorToken.Lexeme));
      }
      else //Parameter
      {
        sb.Append(
          String.Format("\t\tldarg '{0}'\n", node.AnchorToken.Lexeme));
      }
      sb.Append("\t\tldc.i4.1\n");
      sb.Append("\t\tadd.ovf\n");
      if (globals.Contains(node.AnchorToken.Lexeme))
      {
        sb.Append(
          String.Format("\t\tstsfld int32 'Wyvern'::'{0}'\n", node.AnchorToken.Lexeme));
      }
      else if (!row.funsymtable[node.AnchorToken.Lexeme]) // Local
      {
        sb.Append(
          String.Format("\t\tstloc '{0}'\n", node.AnchorToken.Lexeme));
      }
      else //Parameter
      {
        sb.Append(
          String.Format("\t\tstarg '{0}'\n", node.AnchorToken.Lexeme));
      }

      return sb.ToString();
    }

    public string Visit(StmtDecr node)
    {
      StringBuilder sb = new StringBuilder();

      if (globals.Contains(node.AnchorToken.Lexeme))
      {
        sb.Append(
          String.Format("\t\tldsfld int32 'Wyvern'::'{0}'\n", node.AnchorToken.Lexeme));
      }
      else if (!row.funsymtable[node.AnchorToken.Lexeme]) // Local
      {
        sb.Append(
          String.Format("\t\tldloc '{0}'\n", node.AnchorToken.Lexeme));
      }
      else //Parameter
      {
        sb.Append(
          String.Format("\t\tldarg '{0}'\n", node.AnchorToken.Lexeme));
      }
      sb.Append("\t\tldc.i4.1\n");
      sb.Append("\t\tsub.ovf\n");
      if (globals.Contains(node.AnchorToken.Lexeme))
      {
        sb.Append(
          String.Format("\t\tstsfld int32 'Wyvern'::'{0}'\n", node.AnchorToken.Lexeme));
      }
      else if (!row.funsymtable[node.AnchorToken.Lexeme]) // Local
      {
        sb.Append(
          String.Format("\t\tstloc '{0}'\n", node.AnchorToken.Lexeme));
      }
      else //Parameter
      {
        sb.Append(
          String.Format("\t\tstarg '{0}'\n", node.AnchorToken.Lexeme));
      }

      return sb.ToString();
    }

    public string Visit(FunCall node)
    {
      StringBuilder sb = new StringBuilder();

      sb.Append(VisitChildren(node));
      if (functions[node.AnchorToken.Lexeme].predefined)
      {
        var funName = node.AnchorToken.Lexeme;
        funName = funName.Remove(0, 1).Insert(0, Char.ToUpper(funName[0]).ToString());
        sb.Append(
          String.Format("\t\tcall int32 class ['wyvernlib']'Wyvern'.'Utils'::'{0}'(", funName));
      }
      else
      {
        sb.Append(
          String.Format("\t\tcall int32 class 'Wyvern'::'{0}'(", node.AnchorToken.Lexeme));
      }

      for (int i = 0; i < functions[node.AnchorToken.Lexeme].arity; i++)
      {
        if (i != node[0].CountChildren() - 1)
        {
          sb.Append("int32, ");
        }
        else
        {
          sb.Append("int32");
        }
      }
      sb.Append(")\n");

      if (!functionReturn.Contains(node.AnchorToken.Lexeme))
      {
        sb.Append("\t\tpop\n");
      }

      return sb.ToString();
    }

    public string Visit(ExprList node)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(VisitChildren(node));
      return sb.ToString();
    }

    public string Visit(StmtIf node)
    {
      var label = GenerateLabel();
      var endTag = GenerateLabel();

      endIfTagStack.Push(endTag);

      StringBuilder sb = new StringBuilder();

      sb.Append(String.Format(
        "{1}\t\tbrfalse '{0}'\n{2}\n\tbr '{3}'\n\t'{0}':\n",
        label,
        Visit((dynamic)node[0]),
        Visit((dynamic)node[1]),
        endTag
      ));
      sb.Append(Visit((dynamic)node[2]));	// ElseIfList
      sb.Append(Visit((dynamic)node[3]));	// Else

      return sb.ToString();
    }

    public string Visit(ElseIfList node)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(VisitChildren(node));
      return sb.ToString();
    }

    public string Visit(ElseIf node)
    {
      var label = GenerateLabel();
      StringBuilder sb = new StringBuilder();

      sb.Append(String.Format(
        "{1}\t\tbrfalse '{0}'\n{2}\n\tbr '{3}'\n\t'{0}':\n",
        label,
        Visit((dynamic)node[0]),
        Visit((dynamic)node[1]),
        endIfTagStack.Peek()
      ));

      return sb.ToString();
    }

    public string Visit(Else node)
    {
      var sb = new StringBuilder();
      sb.Append(VisitChildren(node));
      sb.Append("\t'" + endIfTagStack.Pop() + "':\n");
      return sb.ToString();
    }

    public string Visit(StmtWhile node)
    {
      StringBuilder sb = new StringBuilder();

      var a = GenerateLabel();
      var b = GenerateLabel();
      whileTagStack.Push(b);

      sb.Append("\t" + a + ":\n");
      sb.Append(Visit((dynamic)node[0])); // Comparations
      sb.Append("\t\tbrfalse " + b + "\n");
      sb.Append(Visit((dynamic)node[1])); // Statements
      sb.Append("\t\tbr " + a + "\n");
      sb.Append("\t" + b + ":\n");

      try
      {
        whileTagStack.Pop();
      }
      catch (InvalidOperationException e) { }


      return sb.ToString();
    }

    public string Visit(StmtBreak node)
    {
      return "\t\tbr " + whileTagStack.Pop() + "\n";
    }

    public string Visit(StmtReturn node)
    {
      functionReturn.Add(functionName);

      return Visit((dynamic)node[0])
        + "\t\tret\n";
    }

    // public Type Visit(StmtEmpty node)
    // {
    //   return Type.VOID;
    // }

    public string Visit(Or node)
    {
      return VisitBinaryOperator("or", node);
    }

    public string Visit(And node)
    {
      return VisitBinaryOperator("and", node);
    }

    public string Visit(Equal node)
    {
      return VisitBinaryOperator("ceq", node);
    }

    public string Visit(Dif node)
    {
      return Visit((dynamic)node[0])
        + Visit((dynamic)node[1])
        + "\t\tceq\n"
        + "\t\tldc.i4.0\n"
        + "\t\tceq\n";
    }

    public string Visit(Less node)
    {
      return VisitBinaryOperator("clt", node);
    }

    public string Visit(LessEqual node)
    {
      return Visit((dynamic)node[0])
        + Visit((dynamic)node[1])
        + "\t\tclt\n"
        + Visit((dynamic)node[0])
        + Visit((dynamic)node[1])
        + "\t\tceq\n"
        + "\t\tor\n";
    }

    public string Visit(Greater node)
    {
      return VisitBinaryOperator("cgt", node);
    }

    public string Visit(GreaterEqual node)
    {
      return Visit((dynamic)node[0])
        + Visit((dynamic)node[1])
        + "\t\tcgt\n"
        + Visit((dynamic)node[0])
        + Visit((dynamic)node[1])
        + "\t\tceq\n"
        + "\t\tor\n";
    }

    public string Visit(Mul node)
    {
      return VisitBinaryOperator("mul.ovf", node);
    }

    public string Visit(Div node)
    {
      return VisitBinaryOperator("div", node);
    }

    public string Visit(Mod node)
    {
      return VisitBinaryOperator("rem", node);
    }

    public string Visit(Plus node)
    {
      return VisitBinaryOperator("add.ovf", node);
    }

    public string Visit(Neg node)
    {
      StringBuilder sb = new StringBuilder();

      // sb.Append("\t\tldc.i4 42\n");
      if (node.CountChildren() <= 1)
      {
        sb.Append(Visit((dynamic)node[0]));
        sb.Append("\t\tneg\n");
      }
      else
      {
        sb.Append(Visit((dynamic)node[0]));
        sb.Append(Visit((dynamic)node[1]));
        sb.Append("\t\tsub.ovf\n");
      }

      return sb.ToString();
    }

    public string Visit(Not node)
    {
      return "\t\tldc.i4.0\n"
        + Visit((dynamic)node[0])
        + "\t\tceq\n";
    }

    public string Visit(Identifier node)
    {
      StringBuilder sb = new StringBuilder();

      if (globals.Contains(node.AnchorToken.Lexeme))
      {
        sb.Append(
          String.Format("\t\tldsfld int32 'Wyvern'::'{0}'\n", node.AnchorToken.Lexeme));
      }
      else if (!row.funsymtable[node.AnchorToken.Lexeme]) // Local
      {
        sb.Append(
          String.Format("\t\tldloc '{0}'\n", node.AnchorToken.Lexeme));
      }
      else //Parameter
      {
        sb.Append(
          String.Format("\t\tldarg '{0}'\n", node.AnchorToken.Lexeme));
      }

      return sb.ToString();
    }

    public string Visit(True node)
    {
      return "\t\tldc.i4.1\n";
    }

    public string Visit(False node)
    {
      return "\t\tldc.i4.0\n";
    }

    public string Visit(IntLiteral node)
    {
      StringBuilder sb = new StringBuilder();
      var value = Convert.ToInt32(node.AnchorToken.Lexeme);
      if (0 < value && value <= 8)
      {
        sb.Append(
          String.Format("\t\tldc.i4.{0}\n", Convert.ToInt32(node.AnchorToken.Lexeme)));
      }
      else
      {
        sb.Append(
          String.Format("\t\tldc.i4 {0}\n", Convert.ToInt32(node.AnchorToken.Lexeme)));
      }

      return sb.ToString();
    }

    public string Visit(CharLiteral node)
    {
      StringBuilder sb = new StringBuilder();

      var cleanChar = node.AnchorToken.Lexeme
      .Substring(
        1,
        node.AnchorToken.Lexeme.Length - 2
      );
      var value = 0;

      switch (cleanChar)
      {
        case "\\n":
          value = 10;
          break;
        case "\\r":
          value = 13;
          break;
        case "\\t":
          value = 9;
          break;
        case "\\\\":
          value = 92;
          break;
        case "\\'":
          value = 39;
          break;
        case "\\\"":
          value = 34;
          break;

        default:
          if (cleanChar.Contains("\\u"))
          {
            value = Convert.ToInt32(cleanChar.Substring(2), 16);
          }
          else
          {
            value = Convert.ToInt32(Convert.ToChar(cleanChar));
          }
          break;
      }

      sb.Append(String.Format("\t\tldc.i4 {0}\n", value));

      return sb.ToString();
    }

    public string Visit(StrLiteral node)
    {
      StringBuilder sb = new StringBuilder();

      sb.Append("\t\tldc.i4.0\n");
      sb.Append("\t\tcall int32 class ['wyvernlib']'Wyvern'.'Utils'::'New'(int32)\n");

      var characters = node.AnchorToken.Lexeme.ToCharArray();
      string u = "";
      var value = 0;
      for (int i = 1; i < characters.Length - 1; i++)
      {
        var c = characters[i];
        var cleanChar = node.AnchorToken.Lexeme.Substring(i, 2);

        switch (cleanChar)
        {
          case "\\n":
            value = 10;
            i += 1;
            break;
          case "\\r":
            value = 13;
            i += 1;
            break;
          case "\\t":
            value = 9;
            i += 1;
            break;
          case "\\\\":
            value = 92;
            i += 1;
            break;
          case "\\'":
            value = 39;
            i += 1;
            break;
          case "\\\"":
            value = 34;
            i += 1;
            break;

          default:
            if (cleanChar.Contains("\\u"))
            {
              u = node.AnchorToken.Lexeme.Substring(i, 8);
              i += 7;

              value = Convert.ToInt32(u.Substring(2), 16);
            }
            else
            {
              value = Convert.ToInt32(c);
            }
            break;
        }

        sb.Append("\t\tdup\n");
        sb.Append(String.Format("\t\tldc.i4 {0}\n", value));
        sb.Append("\t\tcall int32 class ['wyvernlib']'Wyvern'.'Utils'::'Add'(int32, int32)\n");
        sb.Append("\t\tpop\n");
      }

      return sb.ToString();
    }

    public string Visit(ArrayToken node)
    {
      StringBuilder sb = new StringBuilder();

      sb.Append("\t\tldc.i4.0\n");
      sb.Append("\t\tcall int32 class ['wyvernlib']'Wyvern'.'Utils'::'New'(int32)\n");
      foreach (var element in node[0])
      {
        sb.Append("\t\tdup\n");

        sb.Append(Visit((dynamic)element));
        sb.Append("\t\tcall int32 class ['wyvernlib']'Wyvern'.'Utils'::'Add'(int32, int32)\n");
        sb.Append("\t\tpop\n");
      }
      return sb.ToString();
    }

    ////-----------------------------------------------------------
    string VisitChildren(Node node)
    {
      var sb = new StringBuilder();
      foreach (var n in node)
      {
        sb.Append(Visit((dynamic)n));
      }
      return sb.ToString();
    }

    ////-----------------------------------------------------------
    string VisitBinaryOperator(string op, Node node)
    {
      return Visit((dynamic)node[0])
        + Visit((dynamic)node[1])
        + "\t\t"
        + op
        + "\n";
    }
  }
}
