/* Driver.cs
 * Wyvern Compiler
 * Authors:
 *			A01370880 Rub�n Escalante Chan
 *			A01371036 Santiago Nakakawa Bernal
 *			A01371240 Iv�n Rangel Varela
 */

/*
  Wyvern compiler - Program driver.
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
using System.IO;
using System.Text;

namespace Wyvern
{

  public class Driver
  {

    const string VERSION = "0.4";

    //-----------------------------------------------------------
    static readonly string[] ReleaseIncludes = {
      "Lexical analysis",
      "Syntactic analysis",
      "AST construction",
      "Semantic analysis"
    };

    //-----------------------------------------------------------
    void PrintAppHeader()
    {
      Console.WriteLine("Wyvern compiler, version " + VERSION);
      Console.WriteLine("Copyright \u00A9 2013 by A. Ortiz, ITESM CEM."
      );
      Console.WriteLine("This program is free software; you may "
        + "redistribute it under the terms of");
      Console.WriteLine("the GNU General Public License version 3 or "
        + "later.");
      Console.WriteLine("This program has absolutely no warranty.");
    }

    //-----------------------------------------------------------
    void PrintReleaseIncludes()
    {
      Console.WriteLine("Included in this release:");
      foreach (var phase in ReleaseIncludes)
      {
        Console.WriteLine("   * " + phase);
      }
    }

    //-----------------------------------------------------------
    void Run(string[] args)
    {

      PrintAppHeader();
      Console.WriteLine();
      PrintReleaseIncludes();
      Console.WriteLine();

      if (args.Length != 2)
      {
        Console.Error.WriteLine(
          "Please specify the name of the input file.");
        Environment.Exit(1);
      }

      // ********* Lexical *********
      // try
      // {
      //   var inputPath = args[0];
      //   var input = File.ReadAllText(inputPath);

      //   Console.WriteLine(String.Format(
      //     "===== Tokens from: \"{0}\" =====", inputPath)
      //   );
      //   var count = 1;
      //   foreach (var tok in new Scanner(input).Start())
      //   {
      //     Console.WriteLine(String.Format("[{0}] {1}",
      //                     count++, tok)
      //     );
      //   }

      // }
      // catch (FileNotFoundException e)
      // {
      //   Console.Error.WriteLine(e.Message);
      //   Environment.Exit(1);
      // }

      // Console.WriteLine("\n--- --- --- --- --- --- --- --- --- ---\n");

      // ********* Syntactic *********
      // try
      // {
      //   var inputPath = args[0];
      //   var input = File.ReadAllText(inputPath);
      //   var parser = new Parser(new Scanner(input).Start().GetEnumerator());
      //   parser.Program();
      //   Console.WriteLine("Syntax OK.");

      // }
      // catch (Exception e)
      // {

      //   if (e is FileNotFoundException || e is SyntaxError)
      //   {
      //     Console.Error.WriteLine(e.Message);
      //     Environment.Exit(1);
      //   }

      //   throw;
      // }

      // Console.WriteLine("\n--- --- --- --- --- --- --- --- --- ---\n");

      // ********* AST *********
      // try
      // {
      //   var inputPath = args[0];
      //   var input = File.ReadAllText(inputPath);
      //   var parser = new Parser(new Scanner(input).Start().GetEnumerator());
      //   var program = parser.Program();
      //   Console.Write(program.ToStringTree());

      // }
      // catch (Exception e)
      // {

      //   if (e is FileNotFoundException || e is SyntaxError)
      //   {
      //     Console.Error.WriteLine(e.Message);
      //     Environment.Exit(1);
      //   }

      //   throw;
      // }

      // Console.WriteLine("\n--- --- --- --- --- --- --- --- --- ---\n");

      // ********* Semantic *********
      // try
      // {
      //   var inputPath = args[0];
      //   var input = File.ReadAllText(inputPath);
      //   var parser = new Parser(new Scanner(input).Start().GetEnumerator());
      //   var program = parser.Program();
      //   Console.WriteLine("Syntax OK.");

      //   var semantic = new SemanticAnalyzer();
      //   semantic.Visit((dynamic)program);

      //   Console.WriteLine("Semantics OK.");

      //   Console.WriteLine();
      //   Console.WriteLine("Global Symbol Table");
      //   Console.WriteLine("============");
      //   foreach (var entry in semantic.Globals)
      //   {
      //     Console.WriteLine(entry);
      //   }

      //   Console.WriteLine();
      //   Console.WriteLine("Functions Symbol Table");
      //   Console.WriteLine("============");
      //   foreach (var entry in semantic.Functions)
      //   {
      //     Console.WriteLine(entry);
      //   }

      // }
      // catch (Exception e)
      // {

      //   if (e is FileNotFoundException
      //       || e is SyntaxError
      //       || e is SemanticError)
      //   {
      //     Console.Error.WriteLine(e.Message);
      //     Environment.Exit(1);
      //   }

      //   Console.WriteLine(e);
      //   throw;
      // }

      // Console.WriteLine("\n--- --- --- --- --- --- --- --- --- ---\n");

      // ********* CIL *********
      try
      {
        var inputPath = args[0];
        var outputPath = args[1];
        var input = File.ReadAllText(inputPath);
        var parser = new Parser(new Scanner(input).Start().GetEnumerator());
        var ast = parser.Program();
        Console.WriteLine("Syntax OK.");
        Console.Write(ast.ToStringTree());

        var semantic = new SemanticAnalyzer();
        semantic.Visit((dynamic)ast);
        Console.WriteLine("Semantics OK.");

        Console.WriteLine();
        Console.WriteLine("Global Symbol Table");
        Console.WriteLine("============");
        foreach (var entry in semantic.Globals)
        {
          Console.WriteLine(entry);
        }

        Console.WriteLine();
        Console.WriteLine("Functions Symbol Table");
        Console.WriteLine("============");
        foreach (var entry in semantic.Functions)
        {
          Console.WriteLine(entry);
        }
        Console.WriteLine();

        var codeGenerator = new CILGenerator(semantic.Globals, semantic.Functions);
        File.WriteAllText(
          outputPath,
          codeGenerator.Visit((dynamic)ast));
        Console.WriteLine(
          "Generated CIL code to '" + outputPath + "'.");
        Console.WriteLine();

      }
      catch (Exception e)
      {

        if (e is FileNotFoundException
          || e is SyntaxError
          || e is SemanticError)
        {
          Console.Error.WriteLine(e.Message);
          Environment.Exit(1);
        }

        throw;
      }
    }

    //-----------------------------------------------------------
    public static void Main(string[] args)
    {
      new Driver().Run(args);
    }
  }
}
