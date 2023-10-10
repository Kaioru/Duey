namespace Duey.Abstractions.Types;

public struct DataVector
{
    public DataVector(int x, int y)
    {
        X = x;
        Y = y;
    }
    
    public int X { get; }
    public int Y { get; }
}
