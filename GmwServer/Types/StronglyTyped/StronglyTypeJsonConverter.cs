#nullable disable

using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GmwServer;

public class StronglyTypedJsonConverter<TStronglyTyped, TValue> : JsonConverter<TStronglyTyped>
    where TStronglyTyped : StronglyTyped<TValue>
    where TValue : notnull
{
    public override TStronglyTyped Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
            return null!;

        var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
        var factory = StronglyTypedHelper.GetFactory<TValue>(typeToConvert);
        return (TStronglyTyped)factory(value);
    }

    public override void Write(Utf8JsonWriter writer, TStronglyTyped value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            JsonSerializer.Serialize(writer, value.Value, options);
    }
}


public class StronglyTypedJsonConverterFactory : JsonConverterFactory
{
    private static readonly ConcurrentDictionary<Type, JsonConverter> Cache = new();

    public override bool CanConvert(Type typeToConvert)
    {
        return StronglyTypedHelper.IsStronglyTyped(typeToConvert);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return Cache.GetOrAdd(typeToConvert, CreateConverter);
    }

    private static JsonConverter CreateConverter(Type typeToConvert)
    {
        if (!StronglyTypedHelper.IsStronglyTyped(typeToConvert, out var valueType))
            throw new InvalidOperationException($"Cannot create converter for '{typeToConvert}'");

        var type = typeof(StronglyTypedJsonConverter<,>).MakeGenericType(typeToConvert, valueType);
        return (JsonConverter)Activator.CreateInstance(type);
    }
}