// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration.Attributes;
using System.Numerics;
using System.Reflection;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.TypeConversion;

/// <summary>
/// Caches <see cref="ITypeConverter"/>s for a given type.
/// </summary>
public class TypeConverterCache
{
	private readonly Dictionary<Type, ITypeConverter> typeConverters = new Dictionary<Type, ITypeConverter>();
	private readonly List<ITypeConverterFactory> defaultTypeConverterFactories = new List<ITypeConverterFactory>();
	private readonly List<ITypeConverterFactory> typeConverterFactories = new List<ITypeConverterFactory>();
	private readonly Dictionary<Type, ITypeConverterFactory> typeConverterFactoryCache = new Dictionary<Type, ITypeConverterFactory>();

	/// <summary>
	/// Initializes the <see cref="TypeConverterCache" /> class.
	/// </summary>
	public TypeConverterCache()
	{
		FuzzingLogsCollector.Log("TypeConverterCache", "TypeConverterCache", 27);
		CreateDefaultConverters();
	}

	/// <summary>
	/// Determines if there is a converter registered for the given type.
	/// </summary>
	/// <param name="type">The type to check.</param>
	/// <returns><c>true</c> if the converter is registered, otherwise false.</returns>
	public bool Contains(Type type)
	{
		FuzzingLogsCollector.Log("TypeConverterCache", "Contains", 38);
		return typeConverters.ContainsKey(type);
	}

	/// <summary>
	/// Adds the <see cref="ITypeConverterFactory"/>.
	/// Factories are queried in order of being added and first factory that handles the type is used for creating the <see cref="ITypeConverter"/>.
	/// </summary>
	/// <param name="typeConverterFactory">Type converter factory</param>
	public void AddConverterFactory(ITypeConverterFactory typeConverterFactory)
	{
		FuzzingLogsCollector.Log("TypeConverterCache", "AddConverterFactory", 49);
		if (typeConverterFactory == null)
		{
			FuzzingLogsCollector.Log("TypeConverterCache", "AddConverterFactory", 52);
			throw new ArgumentNullException(nameof(typeConverterFactory));
		}

		FuzzingLogsCollector.Log("TypeConverterCache", "AddConverterFactory", 56);
		typeConverterFactories.Add(typeConverterFactory);
	}

	/// <summary>
	/// Adds the <see cref="ITypeConverter"/> for the given <see cref="System.Type"/>.
	/// </summary>
	/// <param name="type">The type the converter converts.</param>
	/// <param name="typeConverter">The type converter that converts the type.</param>
	public void AddConverter(Type type, ITypeConverter typeConverter)
	{
		FuzzingLogsCollector.Log("TypeConverterCache", "AddConverter", 67);
		if (type == null)
		{
			FuzzingLogsCollector.Log("TypeConverterCache", "AddConverter", 70);
			throw new ArgumentNullException(nameof(type));
		}

		if (typeConverter == null)
		{
			FuzzingLogsCollector.Log("TypeConverterCache", "AddConverter", 76);
			throw new ArgumentNullException(nameof(typeConverter));
		}

		FuzzingLogsCollector.Log("TypeConverterCache", "AddConverter", 80);
		typeConverters[type] = typeConverter;
	}

	/// <summary>
	/// Adds the <see cref="TypeConverter{T}"/> for the given <see cref="System.Type"/>.
	/// </summary>
	/// <typeparam name="T">The type the converter converts.</typeparam>
	/// <param name="typeConverter">The type converter that converts the type.</param>
	public void AddConverter<T>(TypeConverter<T> typeConverter) =>
		AddConverter<T>(typeConverter as ITypeConverter);

	/// <summary>
	/// Adds the <see cref="ITypeConverter"/> for the given <see cref="System.Type"/>.
	/// </summary>
	/// <typeparam name="T">The type the converter converts.</typeparam>
	/// <param name="typeConverter">The type converter that converts the type.</param>
	public void AddConverter<T>(ITypeConverter typeConverter)
	{
		FuzzingLogsCollector.Log("TypeConverterCache", "AddConverter<T>", 99);
		if (typeConverter == null)
		{
			FuzzingLogsCollector.Log("TypeConverterCache", "AddConverter<T>", 102);
			throw new ArgumentNullException(nameof(typeConverter));
		}

		FuzzingLogsCollector.Log("TypeConverterCache", "AddConverter<T>", 106);
		typeConverters[typeof(T)] = typeConverter;
	}

	/// <summary>
	/// Adds the given <see cref="ITypeConverter"/> to all registered types.
	/// </summary>
	/// <param name="typeConverter">The type converter.</param>
	public void AddConverter(ITypeConverter typeConverter)
	{
		FuzzingLogsCollector.Log("TypeConverterCache", "AddConverter", 116);
		foreach (var type in typeConverters.Keys)
		{
			FuzzingLogsCollector.Log("TypeConverterCache", "AddConverter", 119);
			typeConverters[type] = typeConverter;
		}
	}

	/// <summary>
	/// Removes the <see cref="ITypeConverter"/> for the given <see cref="System.Type"/>.
	/// </summary>
	/// <param name="type">The type to remove the converter for.</param>
	public void RemoveConverter(Type type)
	{
		FuzzingLogsCollector.Log("TypeConverterCache", "RemoveConverter", 130);
		if (type == null)
		{
			FuzzingLogsCollector.Log("TypeConverterCache", "RemoveConverter", 133);
			throw new ArgumentNullException(nameof(type));
		}

		FuzzingLogsCollector.Log("TypeConverterCache", "RemoveConverter", 137);
		typeConverters.Remove(type);
	}

	/// <summary>
	/// Removes the <see cref="ITypeConverter"/> for the given <see cref="System.Type"/>.
	/// </summary>
	/// <typeparam name="T">The type to remove the converter for.</typeparam>
	public void RemoveConverter<T>()
	{
		FuzzingLogsCollector.Log("TypeConverterCache", "RemoveConverter<T>", 147);
		RemoveConverter(typeof(T));
	}

	/// <summary>
	/// Removes the ITypeConverterFactory.
	/// </summary>
	/// <param name="typeConverterFactory">The ITypeConverterFactory to remove.</param>
	public void RemoveConverterFactory(ITypeConverterFactory typeConverterFactory)
	{
		FuzzingLogsCollector.Log("TypeConverterCache", "RemoveConverterFactory", 157);
		typeConverterFactories.Remove(typeConverterFactory);
		var toRemove = typeConverterFactoryCache.Where(pair => pair.Value == typeConverterFactory);
		foreach (var pair in toRemove)
		{
			FuzzingLogsCollector.Log("TypeConverterCache", "RemoveConverterFactory", 162);
			typeConverterFactoryCache.Remove(pair.Key);
		}
	}

	/// <summary>
	/// Gets the converter for the given <see cref="System.Type"/>.
	/// </summary>
	/// <param name="type">The type to get the converter for.</param>
	/// <returns>The <see cref="ITypeConverter"/> for the given <see cref="System.Type"/>.</returns>
	public ITypeConverter GetConverter(Type type)
	{
		FuzzingLogsCollector.Log("TypeConverterCache", "GetConverter", 174);
		if (type == null)
		{
			FuzzingLogsCollector.Log("TypeConverterCache", "GetConverter", 177);
			throw new ArgumentNullException(nameof(type));
		}

		if (typeConverters.TryGetValue(type, out ITypeConverter? typeConverter))
		{
			FuzzingLogsCollector.Log("TypeConverterCache", "GetConverter", 183);
			return typeConverter;
		}

		if (!typeConverterFactoryCache.TryGetValue(type, out var factory))
		{
			FuzzingLogsCollector.Log("TypeConverterCache", "GetConverter", 189);
			factory = typeConverterFactories.Concat(defaultTypeConverterFactories).FirstOrDefault(f => f.CanCreate(type));
			if (factory != null)
			{
				FuzzingLogsCollector.Log("TypeConverterCache", "GetConverter", 193);
				typeConverterFactoryCache[type] = factory;
			}
		}

		if (factory != null)
		{
			FuzzingLogsCollector.Log("TypeConverterCache", "GetConverter", 200);
			if (factory.Create(type, this, out typeConverter))
			{
				FuzzingLogsCollector.Log("TypeConverterCache", "GetConverter", 203);
				AddConverter(type, typeConverter);
			}

			FuzzingLogsCollector.Log("TypeConverterCache", "GetConverter", 207);
			return typeConverter;
		}

		FuzzingLogsCollector.Log("TypeConverterCache", "GetConverter", 211);
		return new DefaultTypeConverter();
	}

	/// <summary>
	/// Gets the converter for the given member. If an attribute is
	/// found on the member, that will be used, otherwise the cache
	/// will be used.
	/// </summary>
	/// <param name="member">The member to get the converter for.</param>
	public ITypeConverter GetConverter(MemberInfo member)
	{
		FuzzingLogsCollector.Log("TypeConverterCache", "GetConverter", 223);
		var typeConverterAttribute = member.GetCustomAttribute<TypeConverterAttribute>();
		if (typeConverterAttribute != null)
		{
			FuzzingLogsCollector.Log("TypeConverterCache", "GetConverter", 227);
			return typeConverterAttribute.TypeConverter;
		}

		FuzzingLogsCollector.Log("TypeConverterCache", "GetConverter", 231);
		return GetConverter(member.MemberType());
	}

	/// <summary>
	/// Gets the converter for the given <see cref="System.Type"/>.
	/// </summary>
	/// <typeparam name="T">The type to get the converter for.</typeparam>
	/// <returns>The <see cref="ITypeConverter"/> for the given <see cref="System.Type"/>.</returns>
	public ITypeConverter GetConverter<T>()
	{
		FuzzingLogsCollector.Log("TypeConverterCache", "GetConverter<T>", 242);
		return GetConverter(typeof(T));
	}

	private void CreateDefaultConverters()
	{
		FuzzingLogsCollector.Log("TypeConverterCache", "CreateDefaultConverters", 248);
		AddConverter(typeof(BigInteger), new BigIntegerConverter());
		AddConverter(typeof(bool), new BooleanConverter());
		AddConverter(typeof(byte), new ByteConverter());
		AddConverter(typeof(byte[]), new ByteArrayConverter());
		AddConverter(typeof(char), new CharConverter());
		AddConverter(typeof(DateTime), new DateTimeConverter());
		AddConverter(typeof(DateTimeOffset), new DateTimeOffsetConverter());
		AddConverter(typeof(decimal), new DecimalConverter());
		AddConverter(typeof(double), new DoubleConverter());
		AddConverter(typeof(float), new SingleConverter());
		AddConverter(typeof(Guid), new GuidConverter());
		AddConverter(typeof(short), new Int16Converter());
		AddConverter(typeof(int), new Int32Converter());
		AddConverter(typeof(long), new Int64Converter());
		AddConverter(typeof(sbyte), new SByteConverter());
		AddConverter(typeof(string), new StringConverter());
		AddConverter(typeof(TimeSpan), new TimeSpanConverter());
		AddConverter(new NotSupportedTypeConverter<Type>());
		AddConverter(typeof(ushort), new UInt16Converter());
		AddConverter(typeof(uint), new UInt32Converter());
		AddConverter(typeof(ulong), new UInt64Converter());
		AddConverter(typeof(Uri), new UriConverter());
#if NET6_0_OR_GREATER
		AddConverter(typeof(DateOnly), new DateOnlyConverter());
		AddConverter(typeof(TimeOnly), new TimeOnlyConverter());
#endif

		defaultTypeConverterFactories.Add(new EnumConverterFactory());
		defaultTypeConverterFactories.Add(new NullableConverterFactory());
		defaultTypeConverterFactories.Add(new CollectionConverterFactory());
	}
}
