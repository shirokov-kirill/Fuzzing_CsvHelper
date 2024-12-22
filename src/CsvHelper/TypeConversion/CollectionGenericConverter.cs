// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration;
using System.Collections;
using System.Collections.ObjectModel;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.TypeConversion;

/// <summary>
/// Converts a <see cref="Collection{T}"/> to and from a <see cref="string"/>.
/// </summary>
public class CollectionGenericConverter : IEnumerableConverter
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
		FuzzingLogsCollector.Log("CollectionGenericConverter", "ConvertFromString", 26);
		// Since we're using the MemberType here, this converter can be used for multiple types
		// as long as they implement IList.
		var list = (IList)ObjectResolver.Current.Resolve(memberMapData.Member!.MemberType());
		var type = memberMapData.Member!.MemberType().GetGenericArguments()[0];
		var converter = row.Context.TypeConverterCache.GetConverter(type);

		if (memberMapData.IsNameSet || row.Configuration.HasHeaderRecord && !memberMapData.IsIndexSet)
		{
			FuzzingLogsCollector.Log("CollectionGenericConverter", "ConvertFromString", 35);
			// Use the name.
			var nameIndex = 0;
			while (true)
			{
				FuzzingLogsCollector.Log("CollectionGenericConverter", "ConvertFromString", 40);
				if (!row.TryGetField(type, memberMapData.Names.FirstOrDefault() ?? string.Empty, nameIndex, out var field))
				{
					FuzzingLogsCollector.Log("CollectionGenericConverter", "ConvertFromString", 43);
					break;
				}

				FuzzingLogsCollector.Log("CollectionGenericConverter", "ConvertFromString", 47);
				list.Add(field);
				nameIndex++;
			}
		}
		else
		{
			FuzzingLogsCollector.Log("CollectionGenericConverter", "ConvertFromString", 54);
			// Use the index.
			var indexEnd = memberMapData.IndexEnd < memberMapData.Index
				? row.Parser.Count - 1
				: memberMapData.IndexEnd;

			for (var i = memberMapData.Index; i <= indexEnd; i++)
			{
				FuzzingLogsCollector.Log("CollectionGenericConverter", "ConvertFromString", 62);
				var field = converter.ConvertFromString(row.GetField(i), row, memberMapData);
				list.Add(field);
			}
		}

		FuzzingLogsCollector.Log("CollectionGenericConverter", "ConvertFromString", 68);
		return list;
	}
}
