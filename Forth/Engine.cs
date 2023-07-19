namespace Forth;


public record Symbol
{
    public int LocationStart { get; set; }
    public int LocationEnd { get; set; }
}

public record NumberSymbol : Symbol
{
    public int Numeric { get; set; }
}

public record WordSymbol : Symbol
{
    public string Name { get; set; }
}

public record CommentSymbol : Symbol
{
    public string Content { get; set; }
}

public record FunctionSymbol : Symbol
{
    public string Name { get; set; }
    public string Body { get; set; }
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
            throw new ForthException($"Stack is empty executing command at position {symbol.LocationStart}", symbol);
        return _stack.Pop();
    }
    
    private void Product(Symbol symbol)
    {
        var a = Pop(symbol);
        var b = Pop(symbol);
        _stack.Push(a * b);
    }
    
    private void Divide(Symbol symbol)
    {
        var a = Pop(symbol);
        var b = Pop(symbol);
        _stack.Push(a / b);
    }
    
    private void Add(Symbol symbol)
    {
        var a = Pop(symbol);
        var b = Pop(symbol);
        _stack.Push(a + b);
    }
    
    private void Subtract(Symbol symbol)
    {
        var a = Pop(symbol);
        var b = Pop(symbol);
        _stack.Push(b - 1);
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
        _words["-"] = Subtract;
        _words["*"] = Product;
        _words["/"] = Divide;
        _words["CR"] = NewLine;
        _words["."] = Print;
        _words["DUP"] = Duplicate;
    }
    
    public void Execute(string statement, Symbol parent = null)
    {
        foreach (var symbol in GetSymbols(statement))
        {
            switch (symbol)
            {
                case CommentSymbol _:
                    break;
                
                case FunctionSymbol function:
                    // Console.WriteLine($"Defining function {function.Name}");
                    _words[function.Name] = symbol => Execute(function.Body, symbol);
                    break;
                    
                case NumberSymbol number:
                    _stack.Push(number.Numeric);
                    break;
                
                case WordSymbol word:
                    if (_words.TryGetValue(word.Name, out var action))
                    {
                        action(symbol);
                    }
                    else
                    {
                        throw new ForthException($"Unknown word {word.Name} at position {symbol.LocationStart}", symbol);
                    }
                    break;
            }
        }
        
        if (parent is null)
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
            
            if (token.StartsWith("(") )
            {
                // Keep advancing until we find the closing parenthesis
                var numberOpen = 1;
                while (nextSpace < statement.Length)
                {
                    if (statement[nextSpace] == '(')
                    {
                        numberOpen++;
                    }
                    else if (statement[nextSpace] == ')')
                    {
                        numberOpen--;
                        if (numberOpen == 0)
                        {
                            break;
                        }
                    }
                    nextSpace++;
                }
                
                if (numberOpen != 0)
                    throw new ForthException("Unbalanced parenthesis", new Symbol
                    {
                        LocationStart = p,
                        LocationEnd = nextSpace
                    });

                var text = statement[(p + 2)..(nextSpace - 1)];
                yield return new CommentSymbol
                {
                    Content = text, 
                    LocationStart = p,
                    LocationEnd = nextSpace
                };
            }
            else if (int.TryParse(token, out var numeric))
            {
                yield return new NumberSymbol
                {
                    Numeric = numeric, 
                    LocationStart = p, 
                    LocationEnd = p + token.Length
                };
            }
            else if (token.StartsWith(":"))
            {
                // Find the end of the function skipping over any comments
                var numberOpen = 0;
                while (nextSpace < statement.Length)
                {
                    if (statement[nextSpace] == '(')
                    {
                        numberOpen++;
                    }
                    else if (statement[nextSpace] == ')')
                    {
                        numberOpen--;
                    }
                    else if (statement[nextSpace] == ';')
                    {
                        if (numberOpen == 0)
                        {
                            break;
                        }
                    }
                    nextSpace++;
                }
                
                if (numberOpen != 0)
                    throw new ForthException("Unbalanced parenthesis", new Symbol
                    {
                        LocationStart = p,
                        LocationEnd = nextSpace
                    });

                if (nextSpace == statement.Length)
                {
                    throw new ForthException("Missing ;", new Symbol
                    {
                        LocationStart = p,
                        LocationEnd = nextSpace
                    });
                }
                
                var text = statement[(p + 2)..(nextSpace - 1)];
                var sep = text.IndexOf(' ');
                
                if (sep == -1)
                    throw new ForthException("Missing function name", new Symbol
                    {
                        LocationStart = p,
                        LocationEnd = nextSpace
                    });
                
                yield return new FunctionSymbol
                {
                    Name = text[..sep],
                    Body = text[(sep + 1)..],
                    LocationStart = p, 
                    LocationEnd = p + token.Length};
            }
            else
            {
                yield return new WordSymbol
                {
                    Name = token, 
                    LocationStart = p, 
                    LocationEnd = p + token.Length
                };
            }
            p = nextSpace + 1;
        }
    }
}