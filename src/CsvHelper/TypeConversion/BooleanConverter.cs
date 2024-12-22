// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration;
using System.Globalization;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.TypeConversion;

/// <summary>
/// Converts a <see cref="bool"/> to and from a <see cref="string"/>.
/// </summary>
public class BooleanConverter : DefaultTypeConverter
{
	/// <inheritdoc/>
	public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
	{
		FuzzingLogsCollector.Log("BooleanConverter", "ConvertFromString", 18);
		if (bool.TryParse(text, out var b))
		{
			FuzzingLogsCollector.Log("BooleanConverter", "ConvertFromString", 22);
			return b;
		}

		if (short.TryParse(text, out var sh))
		{
			FuzzingLogsCollector.Log("BooleanConverter", "ConvertFromString", 28);
			if (sh == 0)
			{
				FuzzingLogsCollector.Log("BooleanConverter", "ConvertFromString", 31);
				return false;
			}
			if (sh == 1)
			{
				FuzzingLogsCollector.Log("BooleanConverter", "ConvertFromString", 36);
				return true;
			}
		}

		FuzzingLogsCollector.Log("BooleanConverter", "ConvertFromString", 41);
		var t = (text ?? string.Empty).Trim();
		foreach (var trueValue in memberMapData.TypeConverterOptions.BooleanTrueValues)
		{
			FuzzingLogsCollector.Log("BooleanConverter", "ConvertFromString", 45);
			if (memberMapData.TypeConverterOptions.CultureInfo!.CompareInfo.Compare(trueValue, t, CompareOptions.IgnoreCase) == 0)
			{
				FuzzingLogsCollector.Log("BooleanConverter", "ConvertFromString", 48);
				return true;
			}
		}

		foreach (var falseValue in memberMapData.TypeConverterOptions.BooleanFalseValues)
		{
			FuzzingLogsCollector.Log("BooleanConverter", "ConvertFromString", 55);
			if (memberMapData.TypeConverterOptions.CultureInfo!.CompareInfo.Compare(falseValue, t, CompareOptions.IgnoreCase) == 0)
			{
				FuzzingLogsCollector.Log("BooleanConverter", "ConvertFromString", 58);
				return false;
			}
		}

		FuzzingLogsCollector.Log("BooleanConverter", "ConvertFromString", 63);
		return base.ConvertFromString(text, row, memberMapData);
	}

	/// <inheritdoc/>
	public override string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
	{
		FuzzingLogsCollector.Log("BooleanConverter", "ConvertToString", 70);
		var b = value as bool?;
		if (b == true && memberMapData.TypeConverterOptions.BooleanTrueValues.Count > 0)
		{
			FuzzingLogsCollector.Log("BooleanConverter", "ConvertToString", 74);
			return memberMapData.TypeConverterOptions.BooleanTrueValues.First();
		}
		else if (b == false && memberMapData.TypeConverterOptions.BooleanFalseValues.Count > 0)
		{
			FuzzingLogsCollector.Log("BooleanConverter", "ConvertToString", 79);
			return memberMapData.TypeConverterOptions.BooleanFalseValues.First();
		}

		FuzzingLogsCollector.Log("BooleanConverter", "ConvertToString", 83);
		return base.ConvertToString(value, row, memberMapData);
	}
}
