// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System.Reflection;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.TypeConversion;

/// <summary>
/// Converts an <see cref="Enum"/> to and from a <see cref="string"/>.
/// </summary>
public class EnumConverter : DefaultTypeConverter
{
	private readonly Type type;
	private readonly Dictionary<string, string> enumNamesByAttributeNames = new Dictionary<string, string>();
	private readonly Dictionary<string, string> enumNamesByAttributeNamesIgnoreCase = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<object, string> attributeNamesByEnumValues = new Dictionary<object, string>();

	// enumNamesByAttributeNames
	// enumNamesByAttributeNamesIgnoreCase
	// [Name("Foo")]:One

	// attributeNamesByEnumValues
	// 1:[Name("Foo")]

	/// <summary>
	/// Creates a new <see cref="EnumConverter"/> for the given <see cref="Enum"/> <see cref="System.Type"/>.
	/// </summary>
	/// <param name="type">The type of the Enum.</param>
	public EnumConverter(Type type)
	{
		FuzzingLogsCollector.Log("EnumConverter", "EnumConverter", 35);
		if (!typeof(Enum).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
		{
			FuzzingLogsCollector.Log("EnumConverter", "EnumConverter", 38);
			throw new ArgumentException($"'{type.FullName}' is not an Enum.");
		}

		this.type = type;

		foreach (var value in Enum.GetValues(type))
		{
			FuzzingLogsCollector.Log("EnumConverter", "EnumConverter", 46);
			var enumName = Enum.GetName(type, value) ?? string.Empty;

			var nameAttribute = type.GetField(enumName)?.GetCustomAttribute<NameAttribute>();
			if (nameAttribute != null && nameAttribute.Names.Length > 0)
			{
				FuzzingLogsCollector.Log("EnumConverter", "EnumConverter", 52);
				foreach (var attributeName in nameAttribute.Names)
				{
					FuzzingLogsCollector.Log("EnumConverter", "EnumConverter", 55);
					if (!enumNamesByAttributeNames.ContainsKey(attributeName))
					{
						FuzzingLogsCollector.Log("EnumConverter", "EnumConverter", 58);
						enumNamesByAttributeNames.Add(attributeName, enumName);
					}

					if (!enumNamesByAttributeNamesIgnoreCase.ContainsKey(attributeName))
					{
						FuzzingLogsCollector.Log("EnumConverter", "EnumConverter", 64);
						enumNamesByAttributeNamesIgnoreCase.Add(attributeName, enumName);
					}

					if (!attributeNamesByEnumValues.ContainsKey(value))
					{
						FuzzingLogsCollector.Log("EnumConverter", "EnumConverter", 70);
						attributeNamesByEnumValues.Add(value, attributeName);
					}
				}
			}
		}
		FuzzingLogsCollector.Log("EnumConverter", "EnumConverter", 76);
	}

	/// <inheritdoc/>
	public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
	{
		FuzzingLogsCollector.Log("EnumConverter", "ConvertFromString", 82);
		var ignoreCase = memberMapData.TypeConverterOptions.EnumIgnoreCase ?? false;

		if (text != null)
		{
			FuzzingLogsCollector.Log("EnumConverter", "ConvertFromString", 87);
			var dict = ignoreCase
				? enumNamesByAttributeNamesIgnoreCase
				: enumNamesByAttributeNames;
			if (dict.TryGetValue(text, out var name))
			{
				FuzzingLogsCollector.Log("EnumConverter", "ConvertFromString", 93);
				return Enum.Parse(type, name);
			}
		}

#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
		if (Enum.TryParse(type, text, ignoreCase, out var value))
		{
			return value;
		}
		else
		{
			return base.ConvertFromString(text, row, memberMapData);
		}
#else
		try
		{
			FuzzingLogsCollector.Log("EnumConverter", "ConvertFromString", 110);
			return Enum.Parse(type, text, ignoreCase);
		}
		catch
		{
			FuzzingLogsCollector.Log("EnumConverter", "ConvertFromString", 115);
			return base.ConvertFromString(text, row, memberMapData);
		}
#endif
	}

	/// <inheritdoc/>
	public override string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
	{
		FuzzingLogsCollector.Log("EnumConverter", "ConvertToString", 124);
		if (value != null && attributeNamesByEnumValues.TryGetValue(value, out var name))
		{
			FuzzingLogsCollector.Log("EnumConverter", "ConvertToString", 127);
			return name;
		}

		FuzzingLogsCollector.Log("EnumConverter", "ConvertToString", 131);
		return base.ConvertToString(value, row, memberMapData);
	}
}
