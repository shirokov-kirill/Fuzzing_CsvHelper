﻿// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration;
using System.Globalization;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.TypeConversion;

/// <summary>
/// Converts a <see cref="DateTimeOffset"/> to and from a <see cref="string"/>.
/// </summary>
public class DateTimeOffsetConverter : DefaultTypeConverter
{
	/// <summary>
	/// Converts the string to an object.
	/// </summary>
	/// <param name="text">The string to convert to an object.</param>
	/// <param name="row">The <see cref="IReaderRow"/> for the current record.</param>
	/// <param name="memberMapData">The <see cref="MemberMapData"/> for the member being created.</param>
	/// <returns>The object created from the string.</returns>
	public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
	{
		FuzzingLogsCollector.Log("DateTimeOffsetConverter", "ConvertFromString", 25);
		if (text == null)
		{
			FuzzingLogsCollector.Log("DateTimeOffsetConverter", "ConvertFromString", 28);
			return base.ConvertFromString(null, row, memberMapData);
		}

		FuzzingLogsCollector.Log("DateTimeOffsetConverter", "ConvertFromString", 32);
		var formatProvider = (IFormatProvider?)memberMapData.TypeConverterOptions.CultureInfo?.GetFormat(typeof(DateTimeFormatInfo)) ?? memberMapData.TypeConverterOptions.CultureInfo;
		var dateTimeStyle = memberMapData.TypeConverterOptions.DateTimeStyle ?? DateTimeStyles.None;

		DateTimeOffset dateTimeOffset;
		var success = memberMapData.TypeConverterOptions.Formats == null || memberMapData.TypeConverterOptions.Formats.Length == 0
			? DateTimeOffset.TryParse(text, formatProvider, dateTimeStyle, out dateTimeOffset)
			: DateTimeOffset.TryParseExact(text, memberMapData.TypeConverterOptions.Formats, formatProvider, dateTimeStyle, out dateTimeOffset);

		FuzzingLogsCollector.Log("DateTimeOffsetConverter", "ConvertFromString", 41);
		return success
			? dateTimeOffset
			: base.ConvertFromString(null, row, memberMapData);
	}
}
