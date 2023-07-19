using Forth;

var engine = new Engine();

while (true)
{
    Console.Write("> ");
    var statement = Console.ReadLine();

    try
    {
        engine.Execute(statement);
    }
    catch (ForthException e)
    {
        Console.WriteLine(e.Message);
        Console.Write(statement![..e.Symbol.Location]);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write(e.Symbol.Value);
        Console.ResetColor();
        Console.WriteLine(statement![(e.Symbol.Location + e.Symbol.Value.Length)..]);
    }
}
