namespace SmartX.Shared.Models;

// Represents a power value and supports direct sensor-value calculations.
public readonly record struct PowerReading(int Watts)
{
    public static PowerReading operator +(PowerReading left, PowerReading right)
    {
        return new PowerReading(left.Watts + right.Watts);
    }

    public static PowerReading operator -(PowerReading left, PowerReading right)
    {
        return new PowerReading(left.Watts - right.Watts);
    }

    public static bool operator >(PowerReading left, PowerReading right)
    {
        return left.Watts > right.Watts;
    }

    public static bool operator <(PowerReading left, PowerReading right)
    {
        return left.Watts < right.Watts;
    }
}
