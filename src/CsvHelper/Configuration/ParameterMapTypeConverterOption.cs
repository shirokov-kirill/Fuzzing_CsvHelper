// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System.Globalization;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.Configuration;

/// <summary>
/// Sets type converter options on a parameter map.
/// </summary>
public class ParameterMapTypeConverterOption
{
	private readonly ParameterMap parameterMap;

	/// <summary>
	/// Creates a new instance using the given <see cref="ParameterMap"/>.
	/// </summary>
	/// <param name="parameterMap">The member map the options are being applied to.</param>
	public ParameterMapTypeConverterOption(ParameterMap parameterMap)
	{
		FuzzingLogsCollector.Log("ParameterMapTypeConverterOption", "ParameterMapTypeConverterOption", 23);
		this.parameterMap = parameterMap;
	}

	/// <summary>
	/// The <see cref="CultureInfo"/> used when type converting.
	/// This will override the global <see cref="CsvConfiguration.CultureInfo"/>
	/// setting.
	/// </summary>
	/// <param name="cultureInfo">The culture info.</param>
	public virtual ParameterMap CultureInfo(CultureInfo cultureInfo)
	{
		FuzzingLogsCollector.Log("ParameterMapTypeConverterOption", "CultureInfo", 35);
		parameterMap.Data.TypeConverterOptions.CultureInfo = cultureInfo;

		return parameterMap;
	}

	/// <summary>
	/// The <see cref="DateTimeStyles"/> to use when type converting.
	/// This is used when doing any <see cref="DateTime"/> conversions.
	/// </summary>
	/// <param name="dateTimeStyle">The date time style.</param>
	public virtual ParameterMap DateTimeStyles(DateTimeStyles dateTimeStyle)
	{
		FuzzingLogsCollector.Log("ParameterMapTypeConverterOption", "DateTimeStyles", 48);
		parameterMap.Data.TypeConverterOptions.DateTimeStyle = dateTimeStyle;

		return parameterMap;
	}

	/// <summary>
	/// The <see cref="TimeSpanStyles"/> to use when type converting.
	/// This is used when doing <see cref="TimeSpan"/> converting.
	/// </summary>
	/// <param name="timeSpanStyles">The time span styles.</param>
	public virtual ParameterMap TimespanStyles(TimeSpanStyles timeSpanStyles)
	{
		FuzzingLogsCollector.Log("ParameterMapTypeConverterOption", "TimespanStyles", 61);
		parameterMap.Data.TypeConverterOptions.TimeSpanStyle = timeSpanStyles;

		return parameterMap;
	}

	/// <summary>
	/// The <see cref="NumberStyles"/> to use when type converting.
	/// This is used when doing any number conversions.
	/// </summary>
	/// <param name="numberStyle"></param>
	public virtual ParameterMap NumberStyles(NumberStyles numberStyle)
	{
		FuzzingLogsCollector.Log("ParameterMapTypeConverterOption", "NumberStyles", 74);
		parameterMap.Data.TypeConverterOptions.NumberStyles = numberStyle;

		return parameterMap;
	}

	/// <summary>
	/// The string format to be used when type converting.
	/// </summary>
	/// <param name="formats">The format.</param>
	public virtual ParameterMap Format(params string[] formats)
	{
		FuzzingLogsCollector.Log("ParameterMapTypeConverterOption", "Format", 86);
		parameterMap.Data.TypeConverterOptions.Formats = formats;

		return parameterMap;
	}

	/// <summary>
	/// The <see cref="UriKind"/> to use when converting.
	/// This is used when doing <see cref="Uri"/> conversions.
	/// </summary>
	/// <param name="uriKind">Kind of the URI.</param>
	public virtual ParameterMap UriKind(UriKind uriKind)
	{
		FuzzingLogsCollector.Log("ParameterMapTypeConverterOption", "UriKind", 99);
		parameterMap.Data.TypeConverterOptions.UriKind = uriKind;

		return parameterMap;
	}

	/// <summary>
	/// The string values used to represent a boolean when converting.
	/// </summary>
	/// <param name="isTrue">A value indicating whether true values or false values are being set.</param>
	/// <param name="clearValues">A value indication if the current values should be cleared before adding the new ones.</param>
	/// <param name="booleanValues">The string boolean values.</param>
	public virtual ParameterMap BooleanValues(bool isTrue, bool clearValues = true, params string[] booleanValues)
	{
		FuzzingLogsCollector.Log("ParameterMapTypeConverterOption", "BooleanValues", 113);
		if (isTrue)
		{
			FuzzingLogsCollector.Log("ParameterMapTypeConverterOption", "BooleanValues", 116);
			if (clearValues)
			{
				FuzzingLogsCollector.Log("ParameterMapTypeConverterOption", "BooleanValues", 119);
				parameterMap.Data.TypeConverterOptions.BooleanTrueValues.Clear();
			}

			parameterMap.Data.TypeConverterOptions.BooleanTrueValues.AddRange(booleanValues);
		}
		else
		{
			FuzzingLogsCollector.Log("ParameterMapTypeConverterOption", "BooleanValues", 127);
			if (clearValues)
			{
				FuzzingLogsCollector.Log("ParameterMapTypeConverterOption", "BooleanValues", 130);
				parameterMap.Data.TypeConverterOptions.BooleanFalseValues.Clear();
			}

			parameterMap.Data.TypeConverterOptions.BooleanFalseValues.AddRange(booleanValues);
		}

		FuzzingLogsCollector.Log("ParameterMapTypeConverterOption", "BooleanValues", 137);
		return parameterMap;
	}

	/// <summary>
	/// The string values used to represent null when converting.
	/// </summary>
	/// <param name="nullValues">The values that represent null.</param>
	/// <returns></returns>
	public virtual ParameterMap NullValues(params string[] nullValues)
	{
		FuzzingLogsCollector.Log("ParameterMapTypeConverterOption", "NullValues", 148);
		return NullValues(true, nullValues);
	}

	/// <summary>
	/// The string values used to represent null when converting.
	/// </summary>
	/// <param name="clearValues">A value indication if the current values should be cleared before adding the new ones.</param>
	/// <param name="nullValues">The values that represent null.</param>
	/// <returns></returns>
	public virtual ParameterMap NullValues(bool clearValues, params string[] nullValues)
	{
		FuzzingLogsCollector.Log("ParameterMapTypeConverterOption", "NullValues", 160);
		if (clearValues)
		{
			FuzzingLogsCollector.Log("ParameterMapTypeConverterOption", "NullValues", 163);
			parameterMap.Data.TypeConverterOptions.NullValues.Clear();
		}

		parameterMap.Data.TypeConverterOptions.NullValues.AddRange(nullValues);

		FuzzingLogsCollector.Log("ParameterMapTypeConverterOption", "NullValues", 169);
		return parameterMap;
	}
}
