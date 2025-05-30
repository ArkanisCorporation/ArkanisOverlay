namespace Arkanis.Overlay.Infrastructure.Data.Converters;

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Abstractions.Game;
using Domain.Models;
using Domain.Models.Game;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public sealed class GuidDomainIdConverter<T>() : ValueConverter<T, Guid>(
    source => source.Identity,
    guid => (Activator.CreateInstance(typeof(T), guid) as T)!
) where T : TypedDomainId<Guid>;

public sealed class UexApiDomainIdConverter() : ValueConverter<UexApiGameEntityId, string>(
    source => JsonSerializer.Serialize(StrongId.CreateGeneric(source, typeof(UexId<>)), JsonSerializerOptions.Default),
    json => JsonSerializer.Deserialize<StrongGenericId<int>>(json, JsonSerializerOptions.Default)!.ConstructReferencedType<UexApiGameEntityId>()
);

public sealed class UexApiDomainIdConverter<T>() : ValueConverter<UexId<T>, string>(
    source => JsonSerializer.Serialize(StrongId.CreateGeneric(source, typeof(UexId<>)), JsonSerializerOptions.Default),
    json => JsonSerializer.Deserialize<StrongGenericId<int>>(json, JsonSerializerOptions.Default)!.ConstructReferencedType<UexId<T>>()
) where T : IGameEntity;

internal static class StrongId
{
    public static StrongId<T> Create<T>(TypedDomainId<T> typedId) where T : notnull
    {
        var type = typedId.GetType();
        return new StrongId<T>(typedId.Identity, type.FullName ?? type.Name);
    }

    public static StrongGenericId<T> CreateGeneric<T>(TypedDomainId<T> typedId, Type openGenericType) where T : notnull
    {
        Debug.Assert(openGenericType.IsGenericType);

        var typeArguments = typedId.GetType()
            .GenericTypeArguments
            .Select(type => type.AssemblyQualifiedName!)
            .ToArray();

        var openGenericTypeName = openGenericType.AssemblyQualifiedName!;
        return new StrongGenericId<T>(typedId.Identity, openGenericTypeName, typeArguments);
    }
}

internal record StrongId<T>(
    [property: JsonPropertyName("i")] T Identity,
    [property: JsonPropertyName("t")] string TypeName
)
{
    [JsonIgnore]
    public Type Type
        => GetTypeByName(TypeName);

    protected Type GetTypeByName(string typeName)
        => Type.GetType(typeName, true)!;

    public virtual TResult ConstructReferencedType<TResult>() where TResult : class
        => Activator.CreateInstance(Type, Identity) as TResult
           ?? throw new InvalidOperationException($"Failed constructing referenced type using: {Type}({Identity})");
}

internal record StrongGenericId<T>(
    T Identity,
    string TypeName,
    [property: JsonPropertyName("g")] string[] GenericTypeArguments
) : StrongId<T>(Identity, TypeName)
{
    [JsonIgnore]
    public Type[] GenericTypes
        => GenericTypeArguments.Select(GetTypeByName).ToArray();

    public override TResult ConstructReferencedType<TResult>() where TResult : class
    {
        var strongIdType = Type.MakeGenericType(GenericTypes);
        return Activator.CreateInstance(strongIdType, Identity) as TResult
               ?? throw new InvalidOperationException($"Failed constructing referenced type using: {strongIdType}({Identity})");
    }
}
