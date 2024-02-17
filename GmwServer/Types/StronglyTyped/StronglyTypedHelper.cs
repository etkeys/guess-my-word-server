
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace GmwServer;

// StronglyTypedId in ASP.net
// https://thomaslevesque.com/2020/11/23/csharp-9-records-as-strongly-typed-ids-part-2-aspnet-core-route-and-query-parameters/

public static class StronglyTypedHelper
{
    private static readonly ConcurrentDictionary<Type, Delegate> StronglyTypedFactories = new();

    public static Func<TValue, object> GetFactory<TValue>(Type stronglyTypedIdType)
        where TValue : notnull
    {
        return (Func<TValue, object>)StronglyTypedFactories.GetOrAdd(
            stronglyTypedIdType,
            CreateFactory<TValue>);
    }

    private static Func<TValue, object> CreateFactory<TValue>(Type stronglyTypedType)
        where TValue : notnull
    {
        if (!IsStronglyTyped(stronglyTypedType))
            throw new ArgumentException($"Type '{stronglyTypedType}' is not a strongly-typed id type", nameof(stronglyTypedType));

        var ctor = stronglyTypedType.GetConstructor(new[] { typeof(TValue) });
        if (ctor is null)
            throw new ArgumentException($"Type '{stronglyTypedType}' doesn't have a constructor with one parameter of type '{typeof(TValue)}'", nameof(stronglyTypedType));

        var param = Expression.Parameter(typeof(TValue), "value");
        var body = Expression.New(ctor, param);
        var lambda = Expression.Lambda<Func<TValue, object>>(body, param);
        return lambda.Compile();
    }

    public static bool IsStronglyTyped(Type type) => IsStronglyTyped(type, out _);

    public static bool IsStronglyTyped(Type type, [NotNullWhen(true)] out Type idType)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (type.BaseType is Type baseType &&
            baseType.IsGenericType &&
            baseType.GetGenericTypeDefinition() == typeof(StronglyTyped<>))
        {
            idType = baseType.GetGenericArguments()[0];
            return true;
        }

        idType = null!;
        return false;
    }
}
