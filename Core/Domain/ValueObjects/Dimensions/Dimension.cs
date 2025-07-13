using ClashOpenings.Core.Domain.Enums;

namespace ClashOpenings.Core.Domain.ValueObjects.Dimensions;

public abstract class Dimension(float value, Units unit)
{
    public float Value { get; set; } = value;
    public Units Unit { get; set; } = unit;
}