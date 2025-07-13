using ClashOpenings.Core.Domain.Enums;

namespace ClashOpenings.Core.Domain.ValueObjects.Dimensions;

public class Thickness(float value, Units unit) : Dimension(value, unit)
{
}