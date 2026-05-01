using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using CsvHelper.Configuration;

namespace Wolfgang.Etl.Csv;

/// <summary>
/// Builds and caches CsvHelper <see cref="ClassMap{T}"/> instances from
/// <see cref="CsvColumnAttribute"/> and <see cref="CsvIgnoreAttribute"/>
/// decorations on the target type.
/// </summary>
/// <remarks>
/// Caching is per <see cref="Type"/>: the first call for a given <c>T</c>
/// pays the reflection cost, every subsequent call returns the cached map.
/// </remarks>
internal static class CsvClassMapFactory
{
    private static readonly ConcurrentDictionary<Type, ClassMap> Cache = new();



    /// <summary>
    /// Returns a <see cref="ClassMap{T}"/> derived from the
    /// <see cref="CsvColumnAttribute"/> and <see cref="CsvIgnoreAttribute"/>
    /// decorations on <typeparamref name="T"/>, or <c>null</c> when the type
    /// has no Wolfgang.Etl.Csv attributes (in which case the underlying parser
    /// uses its default conventions).
    /// </summary>
    /// <typeparam name="T">The record type being mapped.</typeparam>
    [RequiresUnreferencedCode("Reflects over the public properties of T to build a CsvHelper ClassMap. Not safe under aggressive trimming or NativeAOT without preserving T's properties.")]
    public static ClassMap<T>? GetMap<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>()
    {
        var type = typeof(T);

        if (Cache.TryGetValue(type, out var cached))
        {
            return (ClassMap<T>?)cached;
        }

        var built = BuildMap<T>();

        // Cache even nulls so we don't re-reflect the type on every call.
        Cache[type] = built!;
        return built;
    }



    [RequiresUnreferencedCode("Reflects over the public properties of T to build a CsvHelper ClassMap.")]
    private static ClassMap<T>? BuildMap<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>()
    {
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Quick-out: if no Wolfgang.Etl.Csv attributes are present, defer to
        // the parser's default behaviour rather than registering a map.
        var hasAnyAttributes = properties.Any
        (
            p => p.IsDefined(typeof(CsvColumnAttribute), inherit: true)
              || p.IsDefined(typeof(CsvIgnoreAttribute), inherit: true)
        );

        if (!hasAnyAttributes)
        {
            return null;
        }

        var map = new DefaultClassMap<T>();
        map.AutoMap(System.Globalization.CultureInfo.CurrentCulture);

        foreach (var prop in properties)
        {
            ApplyAttributes(map, prop);
        }

        return map;
    }



    private static void ApplyAttributes(ClassMap map, PropertyInfo prop)
    {
        if (prop.IsDefined(typeof(CsvIgnoreAttribute), inherit: true))
        {
            var existing = map.MemberMaps.FirstOrDefault(mm => mm.Data.Member == prop);
            existing?.Ignore();
            return;
        }

        var col = prop.GetCustomAttribute<CsvColumnAttribute>(inherit: true);
        if (col is null)
        {
            return;
        }

        var memberMap = map.MemberMaps.FirstOrDefault(mm => mm.Data.Member == prop)
                        ?? map.Map(prop.DeclaringType ?? prop.ReflectedType!, prop);

        if (!string.IsNullOrEmpty(col.Name))
        {
            memberMap.Name(col.Name!);
        }

        if (col.Index >= 0)
        {
            memberMap.Index(col.Index);
        }

        if (col.Optional)
        {
            memberMap.Optional();
        }

        if (!string.IsNullOrEmpty(col.Format))
        {
            memberMap.TypeConverterOption.Format(col.Format!);
        }

        if (col.Default is not null)
        {
            memberMap.Default(col.Default);
        }
    }
}
