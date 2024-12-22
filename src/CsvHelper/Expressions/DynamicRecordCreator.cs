// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper

using CsvHelper.FuzzingLogger;

namespace CsvHelper.Expressions;

/// <summary>
/// Creates dynamic records.
/// </summary>
public class DynamicRecordCreator : RecordCreator
{
	/// <summary>
	/// Initializes a new instance.
	/// </summary>
	/// <param name="reader">The reader.</param>
	public DynamicRecordCreator(CsvReader reader) : base(reader) { }

	/// <summary>
	/// Creates a <see cref="Delegate"/> of type <see cref="Func{T}"/>
	/// that will create a record of the given type using the current
	/// reader row.
	/// </summary>
	/// <param name="recordType">The record type.</param>
	protected override Delegate CreateCreateRecordDelegate(Type recordType) => (Func<dynamic>)CreateDynamicRecord;

	/// <summary>
	/// Creates a dynamic record of the current reader row.
	/// </summary>
	protected virtual dynamic CreateDynamicRecord()
	{
		FuzzingLogsCollector.Log("CreateDynamicRecord", "CreateDynamicRecord", 34);
		var obj = new FastDynamicObject();
		var dict = obj as IDictionary<string, object?>;
		if (Reader.HeaderRecord != null)
		{
			FuzzingLogsCollector.Log("CreateDynamicRecord", "CreateDynamicRecord", 39);
			for (var i = 0; i < Reader.HeaderRecord.Length; i++)
			{
				FuzzingLogsCollector.Log("CreateDynamicRecord", "CreateDynamicRecord", 42);
				var args = new GetDynamicPropertyNameArgs(i, Reader.Context);
				var propertyName = Reader.Configuration.GetDynamicPropertyName(args);
				Reader.TryGetField(i, out string? field);
				dict[propertyName] = field;
			}
		}
		else
		{
			FuzzingLogsCollector.Log("CreateDynamicRecord", "CreateDynamicRecord", 51);
			for (var i = 0; i < Reader.Parser.Count; i++)
			{
				FuzzingLogsCollector.Log("CreateDynamicRecord", "CreateDynamicRecord", 54);
				var args = new GetDynamicPropertyNameArgs(i, Reader.Context);
				var propertyName = Reader.Configuration.GetDynamicPropertyName(args);
				var field = Reader.GetField(i);
				dict[propertyName] = field;
			}
		}

		FuzzingLogsCollector.Log("CreateDynamicRecord", "CreateDynamicRecord", 62);
		return obj;
	}
}
