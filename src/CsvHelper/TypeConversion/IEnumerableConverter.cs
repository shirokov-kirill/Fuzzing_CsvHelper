// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration;
using System.Collections;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.TypeConversion;

/// <summary>
/// Converts an <see cref="IEnumerable"/> to and from a <see cref="string"/>.
/// </summary>
public class IEnumerableConverter : DefaultTypeConverter
{
	/// <summary>
	/// Converts the object to a string.
	/// </summary>
	/// <param name="value">The object to convert to a string.</param>
	/// <param name="row"></param>
	/// <param name="memberMapData"></param>
	/// <returns>The string representation of the object.</returns>
	public override string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
	{
		FuzzingLogsCollector.Log("IEnumerableConverter", "ConvertToString", 25);
		var list = value as IEnumerable;
		if (list == null)
		{
			FuzzingLogsCollector.Log("IEnumerableConverter", "ConvertToString", 29);
			return base.ConvertToString(value, row, memberMapData);
		}

		foreach (var item in list)
		{
			FuzzingLogsCollector.Log("IEnumerableConverter", "ConvertToString", 35);
			row.WriteField(item.ToString());
		}

		FuzzingLogsCollector.Log("IEnumerableConverter", "ConvertToString", 39);
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
		FuzzingLogsCollector.Log("IEnumerableConverter", "ConvertFromString", 52);
		var list = new List<string?>();

		if (memberMapData.IsNameSet || row.Configuration.HasHeaderRecord && !memberMapData.IsIndexSet)
		{
			FuzzingLogsCollector.Log("IEnumerableConverter", "ConvertFromString", 57);
			// Use the name.
			var nameIndex = 0;
			while (true)
			{
				FuzzingLogsCollector.Log("IEnumerableConverter", "ConvertFromString", 62);
				if (!row.TryGetField(memberMapData.Names.FirstOrDefault()!, nameIndex, out string? field))
				{
					FuzzingLogsCollector.Log("IEnumerableConverter", "ConvertFromString", 65);
					break;
				}

				FuzzingLogsCollector.Log("IEnumerableConverter", "ConvertFromString", 69);
				list.Add(field);
				nameIndex++;
			}
		}
		else
		{
			FuzzingLogsCollector.Log("IEnumerableConverter", "ConvertFromString", 76);
			// Use the index.
			var indexEnd = memberMapData.IndexEnd < memberMapData.Index
				? row.Parser.Count - 1
				: memberMapData.IndexEnd;

			for (var i = memberMapData.Index; i <= indexEnd; i++)
			{
				FuzzingLogsCollector.Log("IEnumerableConverter", "ConvertFromString", 84);
				if (row.TryGetField(i, out string? field))
				{
					FuzzingLogsCollector.Log("IEnumerableConverter", "ConvertFromString", 87);
					list.Add(field);
				}
			}
		}

		FuzzingLogsCollector.Log("IEnumerableConverter", "ConvertFromString", 93);
		return list;
	}
}
