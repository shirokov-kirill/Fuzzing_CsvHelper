// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper

using CsvHelper.FuzzingLogger;

namespace CsvHelper.Expressions;

/// <summary>
/// Manages record manipulation.
/// </summary>
public class RecordManager
{
	private readonly RecordCreatorFactory? recordCreatorFactory;
	private readonly RecordHydrator? recordHydrator;
	private readonly RecordWriterFactory? recordWriterFactory;

	/// <summary>
	/// Initializes a new instance using the given reader.
	/// </summary>
	/// <param name="reader"></param>
	public RecordManager(CsvReader reader)
	{
		FuzzingLogsCollector.Log("RecordManager", "RecordManager", 25);
		recordCreatorFactory = ObjectResolver.Current.Resolve<RecordCreatorFactory>(reader);
		recordHydrator = ObjectResolver.Current.Resolve<RecordHydrator>(reader);
	}

	/// <summary>
	/// Initializes a new instance using the given writer.
	/// </summary>
	/// <param name="writer">The writer.</param>
	public RecordManager(CsvWriter writer)
	{
		FuzzingLogsCollector.Log("RecordManager", "RecordManager", 36);
		recordWriterFactory = ObjectResolver.Current.Resolve<RecordWriterFactory>(writer);
	}

	/// <summary>
	/// Gets a cached reader delegate for the given type.
	/// </summary>
	/// <typeparam name="T">The type of the record.</typeparam>
	/// <param name="recordType">The type of the record.</param>
	public Func<T> GetReadDelegate<T>(Type recordType)
	{
		FuzzingLogsCollector.Log("RecordManager", "GetReadDelegate<T>", 47);
		if (recordCreatorFactory is null)
		{
			FuzzingLogsCollector.Log("RecordManager", "GetReadDelegate<T>", 50);
			throw new InvalidOperationException("The record creator factory is null.");
		}

		var recordCreator = recordCreatorFactory.MakeRecordCreator(recordType);
		FuzzingLogsCollector.Log("RecordManager", "GetReadDelegate<T>", 55);
		return recordCreator.GetCreateRecordDelegate<T>(recordType);
	}

	/// <summary>
	/// Hydrates the given record using the current reader row.
	/// </summary>
	/// <typeparam name="T">The type of the record.</typeparam>
	/// <param name="record">The record to hydrate.</param>
	public void Hydrate<T>(T record)
	{
		FuzzingLogsCollector.Log("RecordManager", "Hydrate<T>", 66);
		if (recordHydrator is null)
		{
			FuzzingLogsCollector.Log("RecordManager", "Hydrate<T>", 69);
			throw new InvalidOperationException("The record hydrator is null.");
		}

		recordHydrator.Hydrate(record);
	}

	/// <summary>
	/// Gets a cached writer delegate for the given type.
	/// </summary>
	/// <param name="typeInfo">The record type information.</param>
	/// <typeparam name="T">The type of record being written.</typeparam>
	public Action<T> GetWriteDelegate<T>(RecordTypeInfo typeInfo)
	{
		FuzzingLogsCollector.Log("RecordManager", "GetWriteDelegate<T>", 83);
		if (recordWriterFactory is null)
		{
			FuzzingLogsCollector.Log("RecordManager", "GetWriteDelegate<T>", 86);
			throw new InvalidOperationException("The record creator factory is null.");
		}

		var recordWriter = recordWriterFactory.MakeRecordWriter(typeInfo.RecordType);
		FuzzingLogsCollector.Log("RecordManager", "GetWriteDelegate<T>", 91);
		return recordWriter.GetWriteDelegate<T>(typeInfo);
	}
}
