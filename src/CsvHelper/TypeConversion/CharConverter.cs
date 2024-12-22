﻿// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.TypeConversion;

/// <summary>
/// Converts a <see cref="char"/> to and from a <see cref="string"/>.
/// </summary>
public class CharConverter : DefaultTypeConverter
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
		FuzzingLogsCollector.Log("CharConverter", "ConvertFromString", 24);
		if (text != null && text.Length > 1)
		{
			FuzzingLogsCollector.Log("CharConverter", "ConvertFromString", 27);
			text = text.Trim();
		}

		if (char.TryParse(text, out var c))
		{
			FuzzingLogsCollector.Log("CharConverter", "ConvertFromString", 33);
			return c;
		}

		FuzzingLogsCollector.Log("CharConverter", "ConvertFromString", 37);
		return base.ConvertFromString(text, row, memberMapData);
	}
}
