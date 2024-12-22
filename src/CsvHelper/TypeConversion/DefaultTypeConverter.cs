// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.TypeConversion;

/// <summary>
/// Converts an <see cref="object"/> to and from a <see cref="string"/>.
/// </summary>
public class DefaultTypeConverter : ITypeConverter
{
	/// <inheritdoc/>
	public virtual object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
	{
		FuzzingLogsCollector.Log("DefaultTypeConverter", "ConvertFromString", 18);
		if (memberMapData.UseDefaultOnConversionFailure && memberMapData.IsDefaultSet && memberMapData.Member!.MemberType() == memberMapData.Default?.GetType())
		{
			FuzzingLogsCollector.Log("DefaultTypeConverter", "ConvertFromString", 21);
			return memberMapData.Default;
		}

		if (!row.Configuration.ExceptionMessagesContainRawData)
		{
			FuzzingLogsCollector.Log("DefaultTypeConverter", "ConvertFromString", 27);
			text = $"Hidden because {nameof(IParserConfiguration.ExceptionMessagesContainRawData)} is false.";
		}

		FuzzingLogsCollector.Log("DefaultTypeConverter", "ConvertFromString", 31);
		text ??= string.Empty;

		var message =
			$"The conversion cannot be performed.{Environment.NewLine}" +
			$"    Text: '{text}'{Environment.NewLine}" +
			$"    MemberName: {memberMapData.Member?.Name}{Environment.NewLine}" +
			$"    MemberType: {memberMapData.Member?.MemberType().FullName}{Environment.NewLine}" +
			$"    TypeConverter: '{memberMapData.TypeConverter?.GetType().FullName}'";
		throw new TypeConverterException(this, memberMapData, text, row.Context, message);
	}

	/// <inheritdoc/>
	public virtual string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
	{
		FuzzingLogsCollector.Log("DefaultTypeConverter", "ConvertToString", 46);
		if (value == null)
		{
			FuzzingLogsCollector.Log("DefaultTypeConverter", "ConvertToString", 49);
			if (memberMapData.TypeConverterOptions.NullValues.Count > 0)
			{
				FuzzingLogsCollector.Log("DefaultTypeConverter", "ConvertToString", 52);
				return memberMapData.TypeConverterOptions.NullValues.First();
			}

			FuzzingLogsCollector.Log("DefaultTypeConverter", "ConvertToString", 56);
			return string.Empty;
		}

		if (value is IFormattable formattable)
		{
			FuzzingLogsCollector.Log("DefaultTypeConverter", "ConvertToString", 62);
			var format = memberMapData.TypeConverterOptions.Formats?.FirstOrDefault();
			return formattable.ToString(format, memberMapData.TypeConverterOptions.CultureInfo);
		}

		FuzzingLogsCollector.Log("DefaultTypeConverter", "ConvertToString", 67);
		return value?.ToString() ?? string.Empty;
	}
}
