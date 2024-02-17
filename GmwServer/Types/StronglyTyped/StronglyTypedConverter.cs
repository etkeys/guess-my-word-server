
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;

namespace GmwServer;

// StronglyTypedId in ASP.net
// https://thomaslevesque.com/2020/11/23/csharp-9-records-as-strongly-typed-ids-part-2-aspnet-core-route-and-query-parameters/

public class StronglyTypedConverter: TypeConverter
{
    private static readonly ConcurrentDictionary<Type, TypeConverter> ActualConverters = new();

    private readonly TypeConverter _innerConverter;

    public StronglyTypedConverter(Type stronglyTypedIdType)
    {
        _innerConverter = ActualConverters.GetOrAdd(stronglyTypedIdType, CreateActualConverter);
    }

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        _innerConverter.CanConvertFrom(context, sourceType);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
        _innerConverter.CanConvertTo(context, destinationType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        _innerConverter.ConvertFrom(context, culture, value);

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
        _innerConverter.ConvertTo(context, culture, value, destinationType);


    private static TypeConverter CreateActualConverter(Type stronglyTypedType)
    {
        if (!StronglyTypedHelper.IsStronglyTyped(stronglyTypedType, out var type))
            throw new InvalidOperationException($"The type '{stronglyTypedType}' is not a strongly typed id");

        var actualConverterType = typeof(StronglyTypedConverter<>).MakeGenericType(type);
        return (TypeConverter)Activator.CreateInstance(actualConverterType, stronglyTypedType)!;
    }
}


public class StronglyTypedConverter<TValue> : TypeConverter
    where TValue : notnull
{
    private static readonly TypeConverter _valueConverter = GetValueConverter();

    private static TypeConverter GetValueConverter()
    {
        var converter = TypeDescriptor.GetConverter(typeof(TValue));
        if (!converter.CanConvertFrom(typeof(string)))
            throw new InvalidOperationException(
                $"Type '{typeof(TValue)}' doesn't have a converter that can convert from string");
        return converter;
    }

    private readonly Type _type;
    public StronglyTypedConverter(Type type)
    {
        _type = type;
    }

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string)
            || sourceType == typeof(TValue)
            || base.CanConvertFrom(context, sourceType);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string)
            || destinationType == typeof(TValue)
            || base.CanConvertTo(context, destinationType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string s)
        {
            value = _valueConverter.ConvertFrom(s)!;
        }

        if (value is TValue idValue)
        {
            var factory = StronglyTypedHelper.GetFactory<TValue>(_type);
            return factory(idValue);
        }

        return base.ConvertFrom(context, culture, value);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var stronglyTyped = (StronglyTyped<TValue>)value;
        TValue idValue = stronglyTyped.Value;
        if (destinationType == typeof(string))
            return idValue.ToString()!;
        if (destinationType == typeof(TValue))
            return idValue;
        return base.ConvertTo(context, culture, value, destinationType);
    }
}