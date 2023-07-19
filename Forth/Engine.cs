using System.Diagnostics;

namespace Forth;

public enum SymbolType
{
    Word,
    Number,
    Comment,
    Function
}

public record Symbol
{
    public string Value { get; set; }

    public int Location { get; set; }
    
    public SymbolType SymbolType { get; set; }
}

public class ForthException : Exception
{
    public Symbol Symbol { get; }
    
    public ForthException(string message, Symbol symbol) : base(message)
    {
        Symbol = symbol;
    }
}

public class Engine
{
    private Stack<int> _stack = new();
    private Dictionary<string, Action<Symbol>> _words = new();

    private int Pop(Symbol symbol)
    {
        if (_stack.Count == 0)
            throw new ForthException($"Stack is empty executing {symbol.Value} at position {symbol.Location}", symbol);
        return _stack.Pop();
    }
    
    private void Product(Symbol symbol)
    {
        var a = Pop(symbol);
        var b = Pop(symbol);
        _stack.Push(a * b);
    }
    
    private void Add(Symbol symbol)
    {
        var a = Pop(symbol);
        var b = Pop(symbol);
        _stack.Push(a + b);
    }
    
    private void Print(Symbol symbol)
    {
        var a = Pop(symbol);
        Console.Write(a);
    }
    
    private void NewLine(Symbol symbol)
    {
        Console.WriteLine();
    }
    
    private void Duplicate(Symbol symbol)
    {
        var a = Pop(symbol);
        _stack.Push(a);
        _stack.Push(a);
    }


    public Engine()
    {
        _words["+"] = Add;
        _words["*"] = Product;
        _words["CR"] = NewLine;
        _words["."] = Print;
        _words["DUP"] = Duplicate;
    }
    
    public void Execute(string statement)
    {
        foreach (var symbol in GetSymbols(statement))
        {
            switch (symbol.SymbolType)
            {
                case SymbolType.Comment:
                    break;
                
                case SymbolType.Function:
                    var functionName = symbol.Value[1..];
                    var functionBody = statement[(symbol.Location + symbol.Value.Length)..];
                    _words[functionName] = s => Execute(functionBody);
                    return;
                
                case SymbolType.Number:
                    _stack.Push(symbol.Value);
                    break;
                
                case SymbolType.Word:
                    break;
            }
            
            if (_words.TryGetValue(symbol.Value, out var action))
            {
                action(symbol);
            }
            else if (int.TryParse(symbol.Value, out var number))
            {
            }
            else
            {
                throw new ForthException($"Unknown word {symbol.Value} at position {symbol.Location}", symbol);
            }
            
        }
        
        Console.WriteLine(" OK");
    }

    private static IEnumerable<Symbol> GetSymbols(string statement)
    {
        var p = 0;
        while (p < statement.Length)
        {
            var nextSpace = statement.IndexOf(' ', p + 1);
            if (nextSpace == -1)
            {
                nextSpace = statement.Length;
            }

            var token = statement[p..nextSpace];
            
            var symbolType = SymbolType.Word;
            if (token.StartsWith("(") )
            {
                symbolType = SymbolType.Comment;
            }
            else if (int.TryParse(token, out _))
            {
                symbolType = SymbolType.Number;
            }
            else if (token.StartsWith(":"))
            {
                symbolType = SymbolType.Function;
            }   
            
            yield return new Symbol{ Value = token, Location = p, SymbolType = symbolType};
            p = nextSpace + 1;
        }
    }
}