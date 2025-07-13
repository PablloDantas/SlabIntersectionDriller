namespace ClashOpenings.Core.Domain.Entities.Identifiers;

public class Id(long value)
{
    public long Value { get; private set; } = value;

    public static Id Create(long value)
    {
        return new Id(value);
    }
}