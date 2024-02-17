using System.ComponentModel;

namespace GmwServer;

// StronglyTypedId in ASP.net
// https://thomaslevesque.com/2020/11/23/csharp-9-records-as-strongly-typed-ids-part-2-aspnet-core-route-and-query-parameters/

[TypeConverter(typeof(StronglyTypedConverter))]
public abstract record StronglyTyped<TValue>(TValue Value) where TValue: notnull
{
    public override string ToString() => Value.ToString()!;
}