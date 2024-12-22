// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration;
using System.Collections;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.TypeConversion;

/// <summary>
/// Converts an <see cref="IDictionary"/> to and from a <see cref="string"/>.
/// </summary>
public class IDictionaryConverter : DefaultTypeConverter
{
	/// <summary>
	/// Converts the object to a string.
	/// </summary>
	/// <param name="value">The object to convert to a string.</param>
	/// <param name="row">The <see cref="IWriterRow"/> for the current record.</param>
	/// <param name="memberMapData">The <see cref="MemberMapData"/> for the member being written.</param>
	/// <returns>The string representation of the object.</returns>
	public override string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
	{
		FuzzingLogsCollector.Log("IDictionaryConverter", "ConvertToString", 25);
		var dictionary = value as IDictionary;
		if (dictionary == null)
		{
			FuzzingLogsCollector.Log("IDictionaryConverter", "ConvertToString", 29);
			return base.ConvertToString(value, row, memberMapData);
		}

		foreach (DictionaryEntry entry in dictionary)
		{
			FuzzingLogsCollector.Log("IDictionaryConverter", "ConvertToString", 35);
			row.WriteField(entry.Value);
		}

		FuzzingLogsCollector.Log("IDictionaryConverter", "ConvertToString", 39);
		return null;
	}

	/// <summary>
	/// Converts the string to an object.
	/// </summary>
	/// <param name="text">The string to convert to an object.</param>
	/// <param name="row">The <see cref="IReaderRow"/> for the current record.</param>
	/// <param name="memberMapData">The <see cref="MemberMapData"/> for the member being created.</param>
	/// <returns>The object created from the string.</returns>
	public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
	{
		FuzzingLogsCollector.Log("IDictionaryConverter", "ConvertFromString", 52);
		var dictionary = new Dictionary<string, string?>();

		var indexEnd = memberMapData.IndexEnd < memberMapData.Index
			? row.Parser.Count - 1
			: memberMapData.IndexEnd;

		for (var i = memberMapData.Index; i <= indexEnd; i++)
		{
			FuzzingLogsCollector.Log("IDictionaryConverter", "ConvertFromString", 61);
			if (row.TryGetField(i, out string? field))
			{
				FuzzingLogsCollector.Log("IDictionaryConverter", "ConvertFromString", 64);
				dictionary.Add(row.HeaderRecord![i], field);
			}
		}

		FuzzingLogsCollector.Log("IDictionaryConverter", "ConvertFromString", 69);
		return dictionary;
	}
}
