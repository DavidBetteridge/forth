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
        Console.Write(statement![..e.Symbol.LocationStart]);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write(statement[e.Symbol.LocationStart..e.Symbol.LocationEnd]);
        Console.ResetColor();
        Console.WriteLine(statement[e.Symbol.LocationEnd..]);
    }
}
