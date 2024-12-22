// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System.Reflection;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.Expressions;

/// <summary>
/// Factory to create record creators.
/// </summary>
public class RecordCreatorFactory
{
	private readonly DynamicRecordCreator dynamicRecordCreator;
	private readonly PrimitiveRecordCreator primitiveRecordCreator;
	private readonly ObjectRecordCreator objectRecordCreator;

	/// <summary>
	/// Initializes a new instance using the given reader.
	/// </summary>
	/// <param name="reader">The reader.</param>
	public RecordCreatorFactory(CsvReader reader)
	{
		FuzzingLogsCollector.Log("RecordCreatorFactory", "RecordCreatorFactory", 25);
		dynamicRecordCreator = new DynamicRecordCreator(reader);
		primitiveRecordCreator = new PrimitiveRecordCreator(reader);
		objectRecordCreator = new ObjectRecordCreator(reader);
	}

	/// <summary>
	/// Creates a record creator for the given record type.
	/// </summary>
	/// <param name="recordType">The record type.</param>
	public virtual RecordCreator MakeRecordCreator(Type recordType)
	{
		FuzzingLogsCollector.Log("RecordCreatorFactory", "MakeRecordCreator", 37);
		if (recordType.GetTypeInfo().IsPrimitive)
		{
			FuzzingLogsCollector.Log("RecordCreatorFactory", "MakeRecordCreator", 40);
			return primitiveRecordCreator;
		}

		if (recordType == typeof(object))
		{
			FuzzingLogsCollector.Log("RecordCreatorFactory", "MakeRecordCreator", 46);
			return dynamicRecordCreator;
		}

		FuzzingLogsCollector.Log("RecordCreatorFactory", "MakeRecordCreator", 50);
		return objectRecordCreator;
	}
}
