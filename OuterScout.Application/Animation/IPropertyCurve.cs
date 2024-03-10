using OuterScout.Domain;

namespace OuterScout.Application.Animation;

public interface IPropertyCurve<out T>
{
    public IEnumerable<T> GetValues(IntRange frameRange);
}
