// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper

using CsvHelper.FuzzingLogger;

namespace CsvHelper.TypeConversion;

/// <inheritdoc />
public class EnumConverterFactory : ITypeConverterFactory
{
	/// <inheritdoc />
	public bool CanCreate(Type type)
	{
		FuzzingLogsCollector.Log("EnumConverterFactory", "CanCreate", 16);
		return typeof(Enum).IsAssignableFrom(type);
	}

	/// <inheritdoc />
	public bool Create(Type type, TypeConverterCache cache, out ITypeConverter typeConverter)
	{
		FuzzingLogsCollector.Log("EnumConverterFactory", "Create", 23);
		if (cache.Contains(typeof(Enum)))
		{
			FuzzingLogsCollector.Log("EnumConverterFactory", "Create", 26);
			// If the user has registered a converter for the generic Enum type,
			// that converter will be used as a default for all enums.
			typeConverter = cache.GetConverter<Enum>();

			return false;
		}

		typeConverter = new EnumConverter(type);

		FuzzingLogsCollector.Log("EnumConverterFactory", "Create", 36);
		return true;
	}
}
