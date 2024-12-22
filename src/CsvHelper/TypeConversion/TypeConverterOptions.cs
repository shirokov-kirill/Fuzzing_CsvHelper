﻿// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System.Globalization;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.TypeConversion;

/// <summary>
/// Options used when doing type conversion.
/// </summary>
public class TypeConverterOptions
{
	private static readonly string[] defaultBooleanTrueValues = { };
	private static readonly string[] defaultBooleanFalseValues = { };
	private static readonly string[] defaultNullValues = { };

	/// <summary>
	/// Gets or sets the culture info.
	/// </summary>
	public CultureInfo? CultureInfo { get; set; }

	/// <summary>
	/// Gets or sets the date time style.
	/// </summary>
	public DateTimeStyles? DateTimeStyle { get; set; }

	/// <summary>
	/// Gets or sets the time span style.
	/// </summary>
	public TimeSpanStyles? TimeSpanStyle { get; set; }

	/// <summary>
	/// Gets or sets the number style.
	/// </summary>
	public NumberStyles? NumberStyles { get; set; }

	/// <summary>
	/// Gets or sets the string format.
	/// </summary>
	public string[]? Formats { get; set; }

	/// <summary>
	/// Gets or sets the <see cref="UriKind"/>.
	/// </summary>
	public UriKind? UriKind { get; set; }

	/// <summary>
	/// Ingore case when parsing enums. Default is false.
	/// </summary>
	public bool? EnumIgnoreCase { get; set; }

	/// <summary>
	/// Gets the list of values that can be
	/// used to represent a boolean of true.
	/// </summary>
	public List<string> BooleanTrueValues { get; } = new List<string>(defaultBooleanTrueValues);

	/// <summary>
	/// Gets the list of values that can be
	/// used to represent a boolean of false.
	/// </summary>
	public List<string> BooleanFalseValues { get; } = new List<string>(defaultBooleanFalseValues);

	/// <summary>
	/// Gets the list of values that can be used to represent a null value.
	/// </summary>
	public List<string> NullValues { get; } = new List<string>(defaultNullValues);

	/// <summary>
	/// Merges TypeConverterOptions by applying the values of sources in order on to each other.
	/// The first object is the source object.
	/// </summary>
	/// <param name="sources">The sources that will be applied.</param>
	/// <returns>The updated source object.</returns>
	public static TypeConverterOptions Merge(params TypeConverterOptions[] sources)
	{
		FuzzingLogsCollector.Log("TypeConverterOptions", "Merge", 79);
		if (sources.Length == 0)
		{
			FuzzingLogsCollector.Log("TypeConverterOptions", "Merge", 82);
			throw new InvalidOperationException("At least one source must be provided.");
		}

		var options = sources[0];

		for (var i = 1; i < sources.Length; i++)
		{
			FuzzingLogsCollector.Log("TypeConverterOptions", "Merge", 90);
			var source = sources[i];

			if (source == null)
			{
				FuzzingLogsCollector.Log("TypeConverterOptions", "Merge", 95);
				continue;
			}

			if (source.CultureInfo != null)
			{
				FuzzingLogsCollector.Log("TypeConverterOptions", "Merge", 101);
				options.CultureInfo = source.CultureInfo;
			}

			if (source.DateTimeStyle != null)
			{
				FuzzingLogsCollector.Log("TypeConverterOptions", "Merge", 107);
				options.DateTimeStyle = source.DateTimeStyle;
			}

			if (source.TimeSpanStyle != null)
			{
				FuzzingLogsCollector.Log("TypeConverterOptions", "Merge", 113);
				options.TimeSpanStyle = source.TimeSpanStyle;
			}

			if (source.NumberStyles != null)
			{
				FuzzingLogsCollector.Log("TypeConverterOptions", "Merge", 119);
				options.NumberStyles = source.NumberStyles;
			}

			if (source.Formats != null)
			{
				FuzzingLogsCollector.Log("TypeConverterOptions", "Merge", 125);
				options.Formats = source.Formats;
			}

			if (source.UriKind != null)
			{
				FuzzingLogsCollector.Log("TypeConverterOptions", "Merge", 131);
				options.UriKind = source.UriKind;
			}

			if (source.EnumIgnoreCase != null)
			{
				FuzzingLogsCollector.Log("TypeConverterOptions", "Merge", 137);
				options.EnumIgnoreCase = source.EnumIgnoreCase;
			}

			// Only change the values if they are different than the defaults.
			// This means there were explicit changes made to the options.

			if (!defaultBooleanTrueValues.SequenceEqual(source.BooleanTrueValues))
			{
				FuzzingLogsCollector.Log("TypeConverterOptions", "Merge", 146);
				options.BooleanTrueValues.Clear();
				options.BooleanTrueValues.AddRange(source.BooleanTrueValues);
			}

			if (!defaultBooleanFalseValues.SequenceEqual(source.BooleanFalseValues))
			{
				FuzzingLogsCollector.Log("TypeConverterOptions", "Merge", 153);
				options.BooleanFalseValues.Clear();
				options.BooleanFalseValues.AddRange(source.BooleanFalseValues);
			}

			if (!defaultNullValues.SequenceEqual(source.NullValues))
			{
				FuzzingLogsCollector.Log("TypeConverterOptions", "Merge", 160);
				options.NullValues.Clear();
				options.NullValues.AddRange(source.NullValues);
			}
			FuzzingLogsCollector.Log("TypeConverterOptions", "Merge", 164);
		}

		FuzzingLogsCollector.Log("TypeConverterOptions", "Merge", 167);
		return options;
	}
}
