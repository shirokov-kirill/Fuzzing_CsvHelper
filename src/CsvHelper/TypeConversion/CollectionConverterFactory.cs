// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.TypeConversion;

/// <inheritdoc />
public class CollectionConverterFactory : ITypeConverterFactory
{
	private int dictionaryTypeHashCode = typeof(IDictionary).GetHashCode();
	private List<int> enumerableTypeHashCodes = new List<int>
	{
		typeof(IList).GetHashCode(),
		typeof(ICollection).GetHashCode(),
		typeof(IEnumerable).GetHashCode(),
	};

	/// <inheritdoc />
	public bool CanCreate(Type type)
	{
		FuzzingLogsCollector.Log("CollectionConverterFactory", "CanCreate", 26);
		switch (type)
		{
			case IList:
			case IDictionary:
			case ICollection:
			case IEnumerable:
				FuzzingLogsCollector.Log("CollectionConverterFactory", "CanCreate", 33);
				return true;
		}

		FuzzingLogsCollector.Log("CollectionConverterFactory", "CanCreate", 37);
		if (type.IsArray)
		{
			FuzzingLogsCollector.Log("CollectionConverterFactory", "CanCreate", 40);
			// ArrayConverter
			return true;
		}

		if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
		{
			FuzzingLogsCollector.Log("CollectionConverterFactory", "CanCreate", 47);
			// IDictionaryGenericConverter
			return true;
		}

		if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
		{
			// IDictionaryGenericConverter
			FuzzingLogsCollector.Log("CollectionConverterFactory", "CanCreate", 55);
			return true;
		}

		if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
		{
			FuzzingLogsCollector.Log("CollectionConverterFactory", "CanCreate", 61);
			// CollectionGenericConverter
			return true;
		}

		if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Collection<>))
		{
			FuzzingLogsCollector.Log("CollectionConverterFactory", "CanCreate", 68);
			// CollectionGenericConverter
			return true;
		}

		if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
		{
			FuzzingLogsCollector.Log("CollectionConverterFactory", "CanCreate", 75);
			// IEnumerableGenericConverter
			return true;
		}

		if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>))
		{
			FuzzingLogsCollector.Log("CollectionConverterFactory", "CanCreate", 82);
			// IEnumerableGenericConverter
			return true;
		}

		if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
		{
			FuzzingLogsCollector.Log("CollectionConverterFactory", "CanCreate", 89);
			// IEnumerableGenericConverter
			return true;
		}

		// A specific IEnumerable converter doesn't exist.
		if (typeof(IEnumerable).IsAssignableFrom(type))
		{
			FuzzingLogsCollector.Log("CollectionConverterFactory", "CanCreate", 97);
			// EnumerableConverter
			return true;
		}

		FuzzingLogsCollector.Log("CollectionConverterFactory", "CanCreate", 102);
		return false;
	}

	/// <inheritdoc />
	public bool Create(Type type, TypeConverterCache cache, out ITypeConverter typeConverter)
	{
		FuzzingLogsCollector.Log("CollectionConverterFactory", "Create", 109);
		var typeHashCode = type.GetHashCode();

		if (typeHashCode == dictionaryTypeHashCode)
		{
			FuzzingLogsCollector.Log("CollectionConverterFactory", "Create", 114);
			typeConverter = new IDictionaryConverter();
			return true;
		}

		if (enumerableTypeHashCodes.Contains(typeHashCode))
		{
			FuzzingLogsCollector.Log("CollectionConverterFactory", "Create", 121);
			typeConverter = new IEnumerableConverter();
			return true;
		}

		if (type.IsArray)
		{
			FuzzingLogsCollector.Log("CollectionConverterFactory", "Create", 128);
			typeConverter = new ArrayConverter();
			return true;
		}

		var isGenericType = type.GetTypeInfo().IsGenericType;
		if (isGenericType)
		{
			FuzzingLogsCollector.Log("CollectionConverterFactory", "Create", 136);
			var genericTypeDefinition = type.GetGenericTypeDefinition();

			if (genericTypeDefinition == typeof(Dictionary<,>))
			{
				FuzzingLogsCollector.Log("CollectionConverterFactory", "Create", 141);
				typeConverter = new IDictionaryGenericConverter();
				return true;
			}

			if (genericTypeDefinition == typeof(IDictionary<,>))
			{
				FuzzingLogsCollector.Log("CollectionConverterFactory", "Create", 148);
				typeConverter = new IDictionaryGenericConverter();
				return true;
			}

			if (genericTypeDefinition == typeof(List<>))
			{
				FuzzingLogsCollector.Log("CollectionConverterFactory", "Create", 155);
				typeConverter = new CollectionGenericConverter();
				return true;
			}

			if (genericTypeDefinition == typeof(Collection<>))
			{
				FuzzingLogsCollector.Log("CollectionConverterFactory", "Create", 162);
				typeConverter = new CollectionGenericConverter();
				return true;
			}

			if (genericTypeDefinition == typeof(IList<>))
			{
				FuzzingLogsCollector.Log("CollectionConverterFactory", "Create", 169);
				typeConverter = new IEnumerableGenericConverter();
				return true;
			}

			if (genericTypeDefinition == typeof(ICollection<>))
			{
				FuzzingLogsCollector.Log("CollectionConverterFactory", "Create", 176);
				typeConverter = new IEnumerableGenericConverter();
				return true;
			}

			if (genericTypeDefinition == typeof(IEnumerable<>))
			{
				FuzzingLogsCollector.Log("CollectionConverterFactory", "Create", 183);
				typeConverter = new IEnumerableGenericConverter();
				return true;
			}
		}

		// A specific IEnumerable converter doesn't exist.
		if (typeof(IEnumerable).IsAssignableFrom(type))
		{
			FuzzingLogsCollector.Log("CollectionConverterFactory", "Create", 192);
			typeConverter = new EnumerableConverter();
			return true;
		}

		FuzzingLogsCollector.Log("CollectionConverterFactory", "Create", 197);
		throw new InvalidOperationException($"Cannot create collection converter for type '{type.FullName}'.");
	}
}
