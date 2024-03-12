namespace OuterScout.Domain;

public sealed class Unit
{
    public static Unit Instance { get; } = new();

    private Unit() { }
}
