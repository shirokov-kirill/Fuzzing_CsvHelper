﻿// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.TypeConversion;

/// <summary>
/// Throws an exception when used. This is here so that it's apparent
/// that there is no support for <see cref="Type"/> type conversion. A custom
/// converter will need to be created to have a field convert to and
/// from <see cref="Type"/>.
/// </summary>
public class NotSupportedTypeConverter<T> : TypeConverter<T>
{
	/// <summary>
	/// Throws an exception.
	/// </summary>
	/// <param name="text">The string to convert to an object.</param>
	/// <param name="row">The <see cref="IReaderRow"/> for the current record.</param>
	/// <param name="memberMapData">The <see cref="MemberMapData"/> for the member being created.</param>
	/// <returns>The object created from the string.</returns>
	public override T ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
	{
		FuzzingLogsCollector.Log("NotSupportedTypeConverter<T>", "ConvertFromString", 27);
		var message =
			$"Converting {typeof(T).FullName} is not supported. " +
			"If you want to do this, create your own ITypeConverter and register " +
			"it in the TypeConverterFactory by calling AddConverter.";
		throw new TypeConverterException(this, memberMapData, text ?? string.Empty, row.Context, message);
	}

	/// <summary>
	/// Throws an exception.
	/// </summary>
	/// <param name="value">The object to convert to a string.</param>
	/// <param name="row">The <see cref="IWriterRow"/> for the current record.</param>
	/// <param name="memberMapData">The <see cref="MemberMapData"/> for the member being written.</param>
	/// <returns>The string representation of the object.</returns>
	public override string? ConvertToString(T? value, IWriterRow row, MemberMapData memberMapData)
	{
		FuzzingLogsCollector.Log("NotSupportedTypeConverter<T>", "ConvertToString", 44);
		var message =
			$"Converting {typeof(T).FullName} is not supported. " +
			"If you want to do this, create your own ITypeConverter and register " +
			"it in the TypeConverterFactory by calling AddConverter.";
		throw new TypeConverterException(this, memberMapData, value, row.Context, message);
	}
}
