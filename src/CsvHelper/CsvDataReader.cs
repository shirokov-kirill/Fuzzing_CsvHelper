// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System.Data;
using System.Globalization;
using CsvHelper.FuzzingLogger;

namespace CsvHelper;

/// <summary>
/// Provides a means of reading a CSV file forward-only by using CsvReader.
/// </summary>
/// <seealso cref="System.Data.IDataReader" />
public class CsvDataReader : IDataReader
{
	private readonly CsvReader csv;
	private readonly DataTable schemaTable;
	private bool skipNextRead;

	/// <inheritdoc />
	public object this[int i]
	{
		get
		{
			FuzzingLogsCollector.Log("CsvDataReader", "get", 26);
			return csv[i] ?? string.Empty;
		}
	}

	/// <inheritdoc />
	public object this[string name]
	{
		get
		{
			FuzzingLogsCollector.Log("CsvDataReader", "get", 36);
			return csv[name] ?? string.Empty;
		}
	}

	/// <inheritdoc />
	public int Depth
	{
		get
		{
			FuzzingLogsCollector.Log("CsvDataReader", "get", 46);
			return 0;
		}
	}

	/// <inheritdoc />
	public bool IsClosed { get; private set; }

	/// <inheritdoc />
	public int RecordsAffected
	{
		get
		{
			FuzzingLogsCollector.Log("CsvDataReader", "get", 59);
			return 0;
		}
	}

	/// <inheritdoc />
	public int FieldCount
	{
		get
		{
			FuzzingLogsCollector.Log("CsvDataReader", "get", 69);
			return csv?.Parser.Count ?? 0;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CsvDataReader"/> class.
	/// </summary>
	/// <param name="csv">The CSV.</param>
	/// <param name="schemaTable">The DataTable representing the file schema.</param>
	public CsvDataReader(CsvReader csv, DataTable? schemaTable = null)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "CsvDataReader", 81);
		this.csv = csv;

		csv.Read();

		if (csv.Configuration.HasHeaderRecord && csv.HeaderRecord == null)
		{
			FuzzingLogsCollector.Log("CsvDataReader", "CsvDataReader", 88);
			csv.ReadHeader();
		}
		else
		{
			FuzzingLogsCollector.Log("CsvDataReader", "CsvDataReader", 93);
			skipNextRead = true;
		}

		FuzzingLogsCollector.Log("CsvDataReader", "CsvDataReader", 97);
		this.schemaTable = schemaTable ?? GetSchemaTable();
	}

	/// <inheritdoc />
	public void Close()
	{
		FuzzingLogsCollector.Log("CsvDataReader", "Close", 104);
		Dispose();
	}

	/// <inheritdoc />
	public void Dispose()
	{
		FuzzingLogsCollector.Log("CsvDataReader", "Dispose", 111);
		csv.Dispose();
		IsClosed = true;
	}

	/// <inheritdoc />
	public bool GetBoolean(int i)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetBoolean", 119);
		return csv.GetField<bool>(i);
	}

	/// <inheritdoc />
	public byte GetByte(int i)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetByte", 126);
		return csv.GetField<byte>(i);
	}

	/// <inheritdoc />
	public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetBytes", 133);
		var bytes = csv.GetField<byte[]>(i);
		if (bytes == null)
		{
			FuzzingLogsCollector.Log("CsvDataReader", "GetBytes", 137);
			return 0;
		}

		FuzzingLogsCollector.Log("CsvDataReader", "GetBytes", 141);
		buffer ??= new byte[bytes.Length];

		Array.Copy(bytes, fieldOffset, buffer, bufferoffset, length);

		return bytes.Length;
	}

	/// <inheritdoc />
	public char GetChar(int i)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetChar", 152);
		return csv.GetField<char>(i);
	}

	/// <inheritdoc />
	public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetChars", 159);
		var chars = csv.GetField(i)?.ToCharArray();

		if (chars == null)
		{
			FuzzingLogsCollector.Log("CsvDataReader", "GetChars", 164);
			return 0;
		}

		buffer ??= new char[chars.Length];

		Array.Copy(chars, fieldoffset, buffer, bufferoffset, length);

		FuzzingLogsCollector.Log("CsvDataReader", "GetChars", 172);
		return chars.Length;
	}

	/// <inheritdoc />
	public IDataReader GetData(int i)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetData", 179);
		throw new NotSupportedException();
	}

	/// <inheritdoc />
	public string GetDataTypeName(int i)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetDataTypeName", 186);
		if (i >= schemaTable.Rows.Count)
		{
			FuzzingLogsCollector.Log("CsvDataReader", "GetDataTypeName", 189);
			throw new IndexOutOfRangeException($"SchemaTable does not contain a definition for field '{i}'.");
		}

		var row = schemaTable.Rows[i];
		var field = row["DataType"] as Type;

		if (field == null)
		{
			FuzzingLogsCollector.Log("CsvDataReader", "GetDataTypeName", 198);
			throw new InvalidOperationException($"SchemaTable does not contain a 'DataType' of type 'Type' for field '{i}'.");
		}

		FuzzingLogsCollector.Log("CsvDataReader", "GetDataTypeName", 202);
		return field.Name;
	}

	/// <inheritdoc />
	public DateTime GetDateTime(int i)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetDateTime", 209);
		return csv.GetField<DateTime>(i);
	}

	/// <inheritdoc />
	public decimal GetDecimal(int i)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetDecimal", 216);
		return csv.GetField<decimal>(i);
	}

	/// <inheritdoc />
	public double GetDouble(int i)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetDouble", 223);
		return csv.GetField<double>(i);
	}

	/// <inheritdoc />
	public Type GetFieldType(int i)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetFieldType", 223);
		return typeof(string);
	}

	/// <inheritdoc />
	public float GetFloat(int i)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetFloat", 237);
		return csv.GetField<float>(i);
	}

	/// <inheritdoc />
	public Guid GetGuid(int i)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetGuid", 244);
		return csv.GetField<Guid>(i);
	}

	/// <inheritdoc />
	public short GetInt16(int i)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetInt16", 251);
		return csv.GetField<short>(i);
	}

	/// <inheritdoc />
	public int GetInt32(int i)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetInt32", 258);
		return csv.GetField<int>(i);
	}

	/// <inheritdoc />
	public long GetInt64(int i)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetInt64", 265);
		return csv.GetField<long>(i);
	}

	/// <inheritdoc />
	public string GetName(int i)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetName", 272);
		return csv.Configuration.HasHeaderRecord
			? (csv.HeaderRecord?[i] ?? string.Empty)
			: string.Empty;
	}

	/// <inheritdoc />
	public int GetOrdinal(string name)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetOrdinal", 281);
		var index = csv.GetFieldIndex(name, isTryGet: true);
		if (index >= 0)
		{
			FuzzingLogsCollector.Log("CsvDataReader", "GetOrdinal", 285);
			return index;
		}

		FuzzingLogsCollector.Log("CsvDataReader", "GetOrdinal", 289);
		var args = new PrepareHeaderForMatchArgs(name, 0);
		var namePrepared = csv.Configuration.PrepareHeaderForMatch(args);

		var headerRecord = csv.HeaderRecord;
		for (var i = 0; i < (headerRecord?.Length ?? 0); i++)
		{
			FuzzingLogsCollector.Log("CsvDataReader", "GetOrdinal", 296);
			args = new PrepareHeaderForMatchArgs(headerRecord?[i] ?? string.Empty, i);
			var headerPrepared = csv.Configuration.PrepareHeaderForMatch(args);
			if (csv.Configuration.CultureInfo.CompareInfo.Compare(namePrepared, headerPrepared, CompareOptions.IgnoreCase) == 0)
			{
				FuzzingLogsCollector.Log("CsvDataReader", "GetOrdinal", 301);
				return i;
			}
		}

		FuzzingLogsCollector.Log("CsvDataReader", "GetOrdinal", 306);
		throw new IndexOutOfRangeException($"Field with name '{name}' and prepared name '{namePrepared}' was not found.");
	}

	/// <inheritdoc />
	public DataTable GetSchemaTable()
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetSchemaTable", 313);
		if (schemaTable != null)
		{
			FuzzingLogsCollector.Log("CsvDataReader", "GetSchemaTable", 316);
			return schemaTable;
		}

		FuzzingLogsCollector.Log("CsvDataReader", "GetSchemaTable", 320);
		// https://docs.microsoft.com/en-us/dotnet/api/system.data.datatablereader.getschematable?view=netframework-4.7.2
		var dt = new DataTable("SchemaTable");
		dt.Columns.Add("AllowDBNull", typeof(bool));
		dt.Columns.Add("AutoIncrementSeed", typeof(long));
		dt.Columns.Add("AutoIncrementStep", typeof(long));
		dt.Columns.Add("BaseCatalogName");
		dt.Columns.Add("BaseColumnName");
		dt.Columns.Add("BaseColumnNamespace");
		dt.Columns.Add("BaseSchemaName");
		dt.Columns.Add("BaseTableName");
		dt.Columns.Add("BaseTableNamespace");
		dt.Columns.Add("ColumnName");
		dt.Columns.Add("ColumnMapping", typeof(MappingType));
		dt.Columns.Add("ColumnOrdinal", typeof(int));
		dt.Columns.Add("ColumnSize", typeof(int));
		dt.Columns.Add("DataType", typeof(Type));
		dt.Columns.Add("DefaultValue", typeof(object));
		dt.Columns.Add("Expression");
		dt.Columns.Add("IsAutoIncrement", typeof(bool));
		dt.Columns.Add("IsKey", typeof(bool));
		dt.Columns.Add("IsLong", typeof(bool));
		dt.Columns.Add("IsReadOnly", typeof(bool));
		dt.Columns.Add("IsRowVersion", typeof(bool));
		dt.Columns.Add("IsUnique", typeof(bool));
		dt.Columns.Add("NumericPrecision", typeof(short));
		dt.Columns.Add("NumericScale", typeof(short));
		dt.Columns.Add("ProviderType", typeof(int));

		for (var i = 0; i < csv.ColumnCount; i++)
		{
			FuzzingLogsCollector.Log("CsvDataReader", "GetSchemaTable", 351);
			object? columnName = csv.Configuration.HasHeaderRecord ? csv.HeaderRecord?[i] : i;

			var row = dt.NewRow();
			row["AllowDBNull"] = true;
			row["AutoIncrementSeed"] = DBNull.Value;
			row["AutoIncrementStep"] = DBNull.Value;
			row["BaseCatalogName"] = null;
			row["BaseColumnName"] = columnName;
			row["BaseColumnNamespace"] = null;
			row["BaseSchemaName"] = null;
			row["BaseTableName"] = null;
			row["BaseTableNamespace"] = null;
			row["ColumnName"] = columnName;
			row["ColumnMapping"] = MappingType.Element;
			row["ColumnOrdinal"] = i;
			row["ColumnSize"] = int.MaxValue;
			row["DataType"] = typeof(string);
			row["DefaultValue"] = null;
			row["Expression"] = null;
			row["IsAutoIncrement"] = false;
			row["IsKey"] = false;
			row["IsLong"] = false;
			row["IsReadOnly"] = true;
			row["IsRowVersion"] = false;
			row["IsUnique"] = false;
			row["NumericPrecision"] = DBNull.Value;
			row["NumericScale"] = DBNull.Value;
			row["ProviderType"] = DbType.String;

			dt.Rows.Add(row);
		}

		FuzzingLogsCollector.Log("CsvDataReader", "GetSchemaTable", 384);
		return dt;
	}

	/// <inheritdoc />
	public string GetString(int i)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetString", 391);
		return csv.GetField(i) ?? string.Empty;
	}

	/// <inheritdoc />
	public object GetValue(int i)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetValue", 398);
		return IsDBNull(i) ? DBNull.Value : (csv.GetField(i) ?? string.Empty);
	}

	/// <inheritdoc />
	public int GetValues(object[] values)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "GetValues", 405);
		for (var i = 0; i < values.Length; i++)
		{
			FuzzingLogsCollector.Log("CsvDataReader", "GetValues", 408);
			values[i] = IsDBNull(i) ? DBNull.Value : (csv.GetField(i) ?? string.Empty);
		}

		FuzzingLogsCollector.Log("CsvDataReader", "GetValues", 412);
		return csv.Parser.Count;
	}

	/// <inheritdoc />
	public bool IsDBNull(int i)
	{
		FuzzingLogsCollector.Log("CsvDataReader", "IsDBNull", 419);
		var field = csv.GetField(i);
		var nullValues = csv.Context.TypeConverterOptionsCache.GetOptions<string>().NullValues;

		FuzzingLogsCollector.Log("CsvDataReader", "IsDBNull", 423);
		return field == null || nullValues.Contains(field);
	}

	/// <inheritdoc />
	public bool NextResult()
	{
		FuzzingLogsCollector.Log("CsvDataReader", "NextResult", 430);
		return false;
	}

	/// <inheritdoc />
	public bool Read()
	{
		FuzzingLogsCollector.Log("CsvDataReader", "Read", 437);
		if (skipNextRead)
		{
			FuzzingLogsCollector.Log("CsvDataReader", "Read", 440);
			skipNextRead = false;
			return true;
		}

		FuzzingLogsCollector.Log("CsvDataReader", "Read", 445);
		return csv.Read();
	}
}
