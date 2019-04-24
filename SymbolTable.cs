/*
  Buttercup compiler - Symbol table class.
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

  public class SymbolTable : IEnumerable<KeyValuePair<string, Type>>
  {

    IDictionary<string, Type> data = new SortedDictionary<string, Type>();

    //-----------------------------------------------------------
    public override string ToString()
    {
      var sb = new StringBuilder();
      sb.Append("Symbol Table\n");
      sb.Append("====================\n");
      foreach (var entry in data)
      {
        sb.Append(String.Format("{0}: {1}\n",
                                entry.Key,
                                entry.Value));
      }
      sb.Append("====================\n");
      return sb.ToString();
    }

    //-----------------------------------------------------------
    public Type this[string key]
    {
      get
      {
        return data[key];
      }
      set
      {
        data[key] = value;
      }
    }

    //-----------------------------------------------------------
    public bool Contains(string key)
    {
      return data.ContainsKey(key);
    }

    //-----------------------------------------------------------
    public IEnumerator<KeyValuePair<string, Type>> GetEnumerator()
    {
      return data.GetEnumerator();
    }

    //-----------------------------------------------------------
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      throw new NotImplementedException();
    }
  }

  public class FunctionTable
  {
    public int arity
    {
      get;
      set;
    }

    public bool predefined
    {
      get;
      set;
    }

    public IDictionary<string, bool> funsymtable
    {
      get;
      set;
    }

    public FunctionTable(int arity)
    {
      this.arity = arity;
      this.predefined = true;
      this.funsymtable = null;
    }
    public FunctionTable(int arity, bool predefined)
    {
      this.arity = arity;
      this.predefined = predefined;
      this.funsymtable = new Dictionary<string, bool>();
    }

    public bool Contains(string key)
    {
      return funsymtable.ContainsKey(key);
    }

    public override string ToString()
    {
      var sb = new StringBuilder();
      sb.Append(arity + ", ");
      sb.Append(predefined + ", ");
      if (funsymtable == null)
      {
        sb.Append("null");
      }
      else
      {
        sb.Append("\n\t------------\n");
        foreach (var entry in funsymtable)
        {
          sb.Append("\t  " + entry.Key + ", " + entry.Value + "\n");
        }
        sb.Append("\t------------");
      }

      return sb.ToString();
    }
  }

  public class FunctionSymbolTable : IEnumerable<KeyValuePair<string, FunctionTable>>
  {

    IDictionary<string, FunctionTable> data = new SortedDictionary<string, FunctionTable>();

    //-----------------------------------------------------------
    public override string ToString()
    {
      var sb = new StringBuilder();
      sb.Append("Symbol Table\n");
      sb.Append("====================\n");
      foreach (var entry in data)
      {
        sb.Append(String.Format("{0}: {1}\t {2}\t {3}\n",
                                entry.Key,
                                entry.Value.arity,
                                entry.Value.predefined,
                                entry.Value.funsymtable));
      }
      sb.Append("====================\n");
      return sb.ToString();
    }

    //-----------------------------------------------------------
    public FunctionTable this[string key]
    {
      get
      {
        return data[key];
      }
      set
      {
        data[key] = value;
      }
    }

    //-----------------------------------------------------------
    public bool Contains(string key)
    {
      return data.ContainsKey(key);
    }

    //-----------------------------------------------------------
    public IEnumerator<KeyValuePair<string, FunctionTable>> GetEnumerator()
    {
      return data.GetEnumerator();
    }

    //-----------------------------------------------------------
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      throw new NotImplementedException();
    }
  }
}
