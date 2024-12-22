// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration;
using CsvHelper.Expressions;
using CsvHelper.TypeConversion;
using System.Collections;
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using CsvHelper.FuzzingLogger;

#pragma warning disable 649
#pragma warning disable 169

namespace CsvHelper;

/// <summary>
/// Used to write CSV files.
/// </summary>
public class CsvWriter : IWriter
{
	private readonly TextWriter writer;
	private readonly CsvContext context;
	private readonly Lazy<RecordManager> recordManager;
	private readonly TypeConverterCache typeConverterCache;
	private readonly TrimOptions trimOptions;
	private readonly ShouldQuote shouldQuote;
	private readonly MemberMapData reusableMemberMapData = new MemberMapData(null);
	private readonly Dictionary<Type, TypeConverterOptions> typeConverterOptionsCache = new Dictionary<Type, TypeConverterOptions>();
	private readonly string quoteString;
	private readonly char quote;
	private readonly CultureInfo cultureInfo;
	private readonly char comment;
	private readonly bool hasHeaderRecord;
	private readonly bool includePrivateMembers;
	private readonly IComparer<string>? dynamicPropertySort;
	private readonly string delimiter;
	private readonly bool leaveOpen;
	private readonly string newLine;
	private readonly char[] injectionCharacters;
	private readonly char injectionEscapeCharacter;
	private readonly InjectionOptions injectionOptions;
	private readonly CsvMode mode;
	private readonly string escapeString;
	private readonly string escapeQuoteString;
	private readonly string escapeDelimiterString;
	private readonly string escapeNewlineString;
	private readonly string escapeEscapeString;

	private bool disposed;
	private bool hasHeaderBeenWritten;
	private int row = 1;
	private int index;
	private char[] buffer;
	private int bufferSize;
	private int bufferPosition;
	private Type? fieldType;

	/// <inheritdoc/>
	public virtual string?[]? HeaderRecord { get; private set; }

	/// <inheritdoc/>
	public virtual int Row => row;

	/// <inheritdoc/>
	public virtual int Index => index;

	/// <inheritdoc/>
	public virtual CsvContext Context => context;

	/// <inheritdoc/>
	public virtual IWriterConfiguration Configuration { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="CsvWriter"/> class.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="culture">The culture.</param>
	/// <param name="leaveOpen"><c>true</c> to leave the <see cref="TextWriter"/> open after the <see cref="CsvWriter"/> object is disposed, otherwise <c>false</c>.</param>
	public CsvWriter(TextWriter writer, CultureInfo culture, bool leaveOpen = false) : this(writer, new CsvConfiguration(culture), leaveOpen) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="CsvWriter"/> class.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="configuration">The configuration.</param>
	/// <param name="leaveOpen"><c>true</c> to leave the <see cref="TextWriter"/> open after the <see cref="CsvWriter"/> object is disposed, otherwise <c>false</c>.</param>
	public CsvWriter(TextWriter writer, IWriterConfiguration configuration, bool leaveOpen = false)
	{
		FuzzingLogsCollector.Log("CsvWriter", "CsvWriter", 94);
		configuration.Validate();

		this.writer = writer;
		Configuration = configuration;
		context = new CsvContext(this);
		typeConverterCache = context.TypeConverterCache;
		recordManager = new Lazy<RecordManager>(() => ObjectResolver.Current.Resolve<RecordManager>(this));

		comment = configuration.Comment;
		bufferSize = configuration.BufferSize;
		delimiter = configuration.Delimiter;
		cultureInfo = configuration.CultureInfo;
		dynamicPropertySort = configuration.DynamicPropertySort;
		escapeDelimiterString = new string(configuration.Delimiter.SelectMany(c => new[] { configuration.Escape, c }).ToArray());
		escapeNewlineString = new string(configuration.NewLine.SelectMany(c => new[] { configuration.Escape, c }).ToArray());
		escapeQuoteString = new string(new[] { configuration.Escape, configuration.Quote });
		escapeEscapeString = new string(new[] { configuration.Escape, configuration.Escape });
		hasHeaderRecord = configuration.HasHeaderRecord;
		includePrivateMembers = configuration.IncludePrivateMembers;
		injectionCharacters = configuration.InjectionCharacters;
		injectionEscapeCharacter = configuration.InjectionEscapeCharacter;
		this.leaveOpen = leaveOpen;
		mode = configuration.Mode;
		newLine = configuration.NewLine;
		quote = configuration.Quote;
		quoteString = configuration.Quote.ToString();
		escapeString = configuration.Escape.ToString();
		injectionOptions = configuration.InjectionOptions;
		shouldQuote = configuration.ShouldQuote;
		trimOptions = configuration.TrimOptions;

		buffer = new char[bufferSize];
	}

	/// <inheritdoc/>
	public virtual void WriteConvertedField(string? field, Type fieldType)
	{
		FuzzingLogsCollector.Log("CsvWriter", "WriteConvertedField", 132);
		this.fieldType = fieldType;

		if (field == null)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteConvertedField", 137);
			return;
		}

		FuzzingLogsCollector.Log("CsvWriter", "WriteConvertedField", 141);
		WriteField(field);
	}

	/// <inheritdoc/>
	public virtual void WriteField(string? field)
	{
		FuzzingLogsCollector.Log("CsvWriter", "WriteField", 148);
		if (field != null && (trimOptions & TrimOptions.Trim) == TrimOptions.Trim)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteField", 151);
			field = field.Trim();
		}

		fieldType ??= typeof(string);

		var args = new ShouldQuoteArgs(field, fieldType, this);
		var shouldQuoteResult = shouldQuote(args);

		FuzzingLogsCollector.Log("CsvWriter", "WriteField", 160);
		WriteField(field, shouldQuoteResult);
	}

	/// <inheritdoc/>
	public virtual void WriteField(string? field, bool shouldQuote)
	{
		FuzzingLogsCollector.Log("CsvWriter", "WriteField", 167);
		if (mode == CsvMode.RFC4180)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteField", 170);
			// All quotes must be escaped.
			if (shouldQuote)
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteField", 174);
				if (escapeString != quoteString)
				{
					FuzzingLogsCollector.Log("CsvWriter", "WriteField", 177);
					field = field?.Replace(escapeString, escapeEscapeString);
				}

				field = field?.Replace(quoteString, escapeQuoteString);
				field = quote + field + quote;
			}
		}
		else if (mode == CsvMode.Escape)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteField", 187);
			field = field?
				.Replace(escapeString, escapeEscapeString)
				.Replace(quoteString, escapeQuoteString)
				.Replace(delimiter, escapeDelimiterString)
				.Replace(newLine, escapeNewlineString);
		}

		if (injectionOptions != InjectionOptions.None)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteField", 197);
			field = SanitizeForInjection(field);
		}

		if (index > 0)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteField", 203);
			WriteToBuffer(delimiter);
		}

		FuzzingLogsCollector.Log("CsvWriter", "WriteField", 207);
		WriteToBuffer(field);
		index++;
		fieldType = null;
	}

	/// <inheritdoc/>
	public virtual void WriteField<T>(T? field)
	{
		FuzzingLogsCollector.Log("CsvWriter", "WriteField<T>", 216);
		var type = field == null ? typeof(string) : field.GetType();
		var converter = typeConverterCache.GetConverter(type);
		WriteField(field, converter);
	}

	/// <inheritdoc/>
	public virtual void WriteField<T>(T? field, ITypeConverter converter)
	{
		FuzzingLogsCollector.Log("CsvWriter", "WriteField<T>", 225);
		var type = field == null ? typeof(string) : field.GetType();
		reusableMemberMapData.TypeConverter = converter;
		if (!typeConverterOptionsCache.TryGetValue(type, out TypeConverterOptions? typeConverterOptions))
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteField<T>", 230);
			typeConverterOptions = TypeConverterOptions.Merge(new TypeConverterOptions { CultureInfo = cultureInfo }, context.TypeConverterOptionsCache.GetOptions(type));
			typeConverterOptionsCache.Add(type, typeConverterOptions);
		}

		reusableMemberMapData.TypeConverterOptions = typeConverterOptions;

		var fieldString = converter.ConvertToString(field, this, reusableMemberMapData);

		FuzzingLogsCollector.Log("CsvWriter", "WriteField<T>", 239);
		WriteConvertedField(fieldString, type);
	}

	/// <inheritdoc/>
	public virtual void WriteField<T, TConverter>(T? field)
	{
		FuzzingLogsCollector.Log("CsvWriter", "WriteField<T, TConverter>", 246);
		var converter = typeConverterCache.GetConverter<TConverter>();

		WriteField(field, converter);
	}

	/// <inheritdoc/>
	public virtual void WriteComment(string? text)
	{
		FuzzingLogsCollector.Log("CsvWriter", "WriteComment", 255);
		WriteField(comment + text, false);
	}

	/// <inheritdoc/>
	public virtual void WriteHeader<T>()
	{
		FuzzingLogsCollector.Log("CsvWriter", "WriteHeader<T>", 262);
		WriteHeader(typeof(T));
	}

	/// <inheritdoc/>
	public virtual void WriteHeader(Type type)
	{
		FuzzingLogsCollector.Log("CsvWriter", "WriteHeader", 269);
		if (type == null)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteHeader", 272);
			throw new ArgumentNullException(nameof(type));
		}

		if (type == typeof(object))
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteHeader", 278);
			return;
		}

		if (context.Maps[type] == null)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteHeader", 284);
			context.Maps.Add(context.AutoMap(type));
		}

		var members = new MemberMapCollection();
		members.AddMembers(context.Maps[type]!); // The map is added above.

		var headerRecord = new List<string?>();

		foreach (var member in members)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteHeader", 295);
			if (CanWrite(member))
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteHeader", 298);
				if (member.Data.IndexEnd >= member.Data.Index)
				{
					FuzzingLogsCollector.Log("CsvWriter", "WriteHeader", 301);
					var count = member.Data.IndexEnd - member.Data.Index + 1;
					for (var i = 1; i <= count; i++)
					{
						FuzzingLogsCollector.Log("CsvWriter", "WriteHeader", 305);
						var header = member.Data.Names.FirstOrDefault() + i;
						WriteField(header);
						headerRecord.Add(header);
					}
				}
				else
				{
					FuzzingLogsCollector.Log("CsvWriter", "WriteHeader", 313);
					var header = member.Data.Names.FirstOrDefault();
					WriteField(header);
					headerRecord.Add(header);
				}
			}
		}

		FuzzingLogsCollector.Log("CsvWriter", "WriteHeader", 321);
		HeaderRecord = headerRecord.ToArray();

		hasHeaderBeenWritten = true;
	}

	/// <summary>
	/// Writes a dynamic header record.
	/// </summary>
	/// <param name="record">The header record to write.</param>
	/// <exception cref="ArgumentNullException">Thrown when no record is passed.</exception>
	public virtual void WriteDynamicHeader(IDynamicMetaObjectProvider? record)
	{
		FuzzingLogsCollector.Log("CsvWriter", "WriteDynamicHeader", 334);
		if (record == null)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteDynamicHeader", 337);
			throw new ArgumentNullException(nameof(record));
		}

		var metaObject = record.GetMetaObject(Expression.Constant(record));
		var names = metaObject.GetDynamicMemberNames().ToList();
		if (dynamicPropertySort != null)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteDynamicHeader", 345);
			names = names.OrderBy(name => name, dynamicPropertySort).ToList();
		}

		HeaderRecord = names.ToArray();

		foreach (var name in names)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteDynamicHeader", 353);
			WriteField(name);
		}

		FuzzingLogsCollector.Log("CsvWriter", "WriteDynamicHeader", 357);
		hasHeaderBeenWritten = true;
	}

	/// <inheritdoc/>
	public virtual void WriteRecord<T>(T? record)
	{
		FuzzingLogsCollector.Log("CsvWriter", "WriteRecord<T>", 364);
		try
		{
			var recordTypeInfo = GetTypeInfoForRecord(record);
			var write = recordManager.Value.GetWriteDelegate<T?>(recordTypeInfo);
			write(record);
		}
		catch (TargetInvocationException ex)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteRecord<T>", 373);
			if (ex.InnerException != null)
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecord<T>", 376);
				throw ex.InnerException;
			}
			else
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecord<T>", 381);
				throw;
			}
		}
		catch (Exception ex) when (ex is not CsvHelperException)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteRecord<T>", 387);
			throw new WriterException(context, "An unexpected error occurred. See inner exception for details.", ex);
		}
	}

	/// <inheritdoc/>
	public virtual void WriteRecords(IEnumerable records)
	{
		FuzzingLogsCollector.Log("CsvWriter", "WriteRecords", 395);
		// Changes in this method require changes in method WriteRecords<T>(IEnumerable<T> records) also.

		var enumerator = records.GetEnumerator();

		try
		{
			if (!enumerator.MoveNext())
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecords", 404);
				return;
			}

			if (WriteHeaderFromRecord(enumerator.Current))
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecords", 410);
				NextRecord();
			}

			Action<object>? write = null;
			RecordTypeInfo writeType = default;

			do
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecords", 419);
				var record = enumerator.Current;

				if (record == null)
				{
					FuzzingLogsCollector.Log("CsvWriter", "WriteRecords", 424);
					// Since every record could be a different type, just write a blank line.
					NextRecord();
					continue;
				}

				if (write == null || writeType.RecordType != record.GetType())
				{
					FuzzingLogsCollector.Log("CsvWriter", "WriteRecords", 432);
					writeType = GetTypeInfoForRecord(record);
					write = recordManager.Value.GetWriteDelegate<object>(writeType);
				}

				FuzzingLogsCollector.Log("CsvWriter", "WriteRecords", 437);
				write(record);
				NextRecord();
			}
			while (enumerator.MoveNext());
		}
		catch (Exception ex) when (ex is not CsvHelperException)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteRecords", 445);
			throw new WriterException(context, "An unexpected error occurred. See inner exception for details.", ex);
		}
		finally
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteRecords", 450);
			if (enumerator is IDisposable en)
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecords", 453);
				en.Dispose();
			}
		}
	}

	/// <inheritdoc/>
	public virtual void WriteRecords<T>(IEnumerable<T> records)
	{
		FuzzingLogsCollector.Log("CsvWriter", "WriteRecords<T>", 462);
		// Changes in this method require changes in method WriteRecords(IEnumerable records) also.

		var enumerator = records.GetEnumerator() ?? throw new InvalidOperationException("Enumerator is null.");

		try
		{
			if (WriteHeaderFromType<T>())
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecords<T>", 471);
				NextRecord();
			}

			if (!enumerator.MoveNext())
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecords<T>", 477);
				return;
			}

			if (WriteHeaderFromRecord(enumerator.Current))
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecords<T>", 483);
				NextRecord();
			}

			Action<T>? write = null;
			RecordTypeInfo writeType = default;

			do
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecords<T>", 492);
				var record = enumerator.Current;

				if (write == null || (record != null && writeType.RecordType != typeof(T)))
				{
					FuzzingLogsCollector.Log("CsvWriter", "WriteRecords<T>", 497);
					writeType = GetTypeInfoForRecord(record);
					write = recordManager.Value.GetWriteDelegate<T>(writeType);
				}

				write(record);
				NextRecord();
			}
			while (enumerator.MoveNext());
		}
		catch (Exception ex) when (ex is not CsvHelperException)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteRecords<T>", 509);
			throw new WriterException(context, "An unexpected error occurred. See inner exception for details.", ex);
		}
		finally
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteRecords<T>", 514);
			if (enumerator is IDisposable en)
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecords<T>", 517);
				en.Dispose();
			}
		}
	}

	/// <inheritdoc/>
	public virtual async Task WriteRecordsAsync(IEnumerable records, CancellationToken cancellationToken = default)
	{
		// These methods should all be the same;
		// - WriteRecordsAsync(IEnumerable records)
		// - WriteRecordsAsync<T>(IEnumerable<T> records)
		// - WriteRecordsAsync<T>(IAsyncEnumerable<T> records)

		FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync", 531);
		var enumerator = records.GetEnumerator();

		try
		{
			if (!enumerator.MoveNext())
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync", 538);
				return;
			}

			if (WriteHeaderFromRecord(enumerator.Current))
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync", 544);
				await NextRecordAsync().ConfigureAwait(false);
			}

			Action<object?>? write = null;
			RecordTypeInfo writeType = default;

			do
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync", 553);
				cancellationToken.ThrowIfCancellationRequested();

				var record = enumerator.Current;

				if (write == null || (record != null && writeType.RecordType != record.GetType()))
				{
					FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync", 560);
					writeType = GetTypeInfoForRecord(record);
					write = recordManager.Value.GetWriteDelegate<object?>(writeType);
				}

				write(record);
				await NextRecordAsync().ConfigureAwait(false);
			}
			while (enumerator.MoveNext());
		}
		catch (Exception ex) when (ex is not CsvHelperException)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync", 572);
			throw new WriterException(context, "An unexpected error occurred. See inner exception for details.", ex);
		}
		finally
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync", 577);
			if (enumerator is IDisposable en)
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync", 580);
				en.Dispose();
			}
		}
	}

	/// <inheritdoc/>
	public virtual async Task WriteRecordsAsync<T>(IEnumerable<T> records, CancellationToken cancellationToken = default)
	{
		// These methods should all be the same;
		// - WriteRecordsAsync(IEnumerable records)
		// - WriteRecordsAsync<T>(IEnumerable<T> records)
		// - WriteRecordsAsync<T>(IAsyncEnumerable<T> records)

		FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync<T>", 594);
		var enumerator = records.GetEnumerator() ?? throw new InvalidOperationException("Enumerator is null.");

		try
		{
			if (WriteHeaderFromType<T>())
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync<T>", 601);
				await NextRecordAsync().ConfigureAwait(false);
			}

			if (!enumerator.MoveNext())
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync<T>", 607);
				return;
			}

			if (WriteHeaderFromRecord(enumerator.Current))
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync<T>", 613);
				await NextRecordAsync().ConfigureAwait(false);
			}

			Action<T?>? write = null;
			RecordTypeInfo writeType = default;

			do
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync<T>", 622);
				cancellationToken.ThrowIfCancellationRequested();

				var record = enumerator.Current;

				if (write == null || (record != null && writeType.RecordType != typeof(T)))
				{
					FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync<T>", 629);
					writeType = GetTypeInfoForRecord(record);
					write = recordManager.Value.GetWriteDelegate<T?>(writeType);
				}

				write(record);
				await NextRecordAsync().ConfigureAwait(false);
			}
			while (enumerator.MoveNext());
		}
		catch (Exception ex) when (ex is not CsvHelperException)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync<T>", 641);
			throw new WriterException(context, "An unexpected error occurred. See inner exception for details.", ex);
		}
		finally
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync<T>", 646);
			if (enumerator is IDisposable en)
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync<T>", 649);
				en.Dispose();
			}
		}
	}

	/// <inheritdoc/>
	public virtual async Task WriteRecordsAsync<T>(IAsyncEnumerable<T> records, CancellationToken cancellationToken = default)
	{
		// These methods should all be the same;
		// - WriteRecordsAsync(IEnumerable records)
		// - WriteRecordsAsync<T>(IEnumerable<T> records)
		// - WriteRecordsAsync<T>(IAsyncEnumerable<T> records)

		FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync<T>", 663);
		var enumerator = records.GetAsyncEnumerator() ?? throw new InvalidOperationException("Enumerator is null.");

		try
		{
			if (WriteHeaderFromType<T>())
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync<T>", 670);
				await NextRecordAsync().ConfigureAwait(false);
			}

			if (!await enumerator.MoveNextAsync())
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync<T>", 676);
				return;
			}

			if (WriteHeaderFromRecord(enumerator.Current))
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync<T>", 682);
				await NextRecordAsync().ConfigureAwait(false);
			}

			Action<T?>? write = null;
			RecordTypeInfo writeType = default;

			do
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync<T>", 691);
				cancellationToken.ThrowIfCancellationRequested();

				var record = enumerator.Current;

				if (write == null || (record != null && writeType.RecordType != typeof(T)))
				{
					FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync<T>", 698);
					writeType = GetTypeInfoForRecord(record);
					write = recordManager.Value.GetWriteDelegate<T?>(writeType);
				}

				write(record);
				await NextRecordAsync().ConfigureAwait(false);
			}
			while (await enumerator.MoveNextAsync().ConfigureAwait(false));
		}
		catch (Exception ex) when (ex is not CsvHelperException)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync<T>", 710);
			throw new WriterException(context, "An unexpected error occurred. See inner exception for details.", ex);
		}
		finally
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync<T>", 715);
			if (enumerator is IDisposable en)
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteRecordsAsync<T>", 718);
				en.Dispose();
			}
		}
	}

	/// <inheritdoc/>
	public virtual void NextRecord()
	{
		FuzzingLogsCollector.Log("CsvWriter", "NextRecord", 727);
		WriteToBuffer(newLine);
		FlushBuffer();

		index = 0;
		row++;
	}

	/// <inheritdoc/>
	public virtual async Task NextRecordAsync()
	{
		FuzzingLogsCollector.Log("CsvWriter", "NextRecordAsync", 738);
		WriteToBuffer(newLine);
		await FlushBufferAsync().ConfigureAwait(false);

		index = 0;
		row++;
	}

	/// <inheritdoc/>
	public virtual void Flush()
	{
		FuzzingLogsCollector.Log("CsvWriter", "Flush", 749);
		FlushBuffer();
		writer.Flush();
	}

	/// <inheritdoc/>
	public virtual async Task FlushAsync()
	{
		FuzzingLogsCollector.Log("CsvWriter", "FlushAsync", 757);
		await FlushBufferAsync().ConfigureAwait(false);
		await writer.FlushAsync().ConfigureAwait(false);
	}

	/// <summary>
	/// Flushes the buffer.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual void FlushBuffer()
	{
		FuzzingLogsCollector.Log("CsvWriter", "FlushBuffer", 768);
		writer.Write(buffer, 0, bufferPosition);
		bufferPosition = 0;
	}

	/// <summary>
	/// Asynchronously flushes the buffer.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual async Task FlushBufferAsync()
	{
		FuzzingLogsCollector.Log("CsvWriter", "FlushBufferAsync", 779);
		await writer.WriteAsync(buffer, 0, bufferPosition).ConfigureAwait(false);
		bufferPosition = 0;
	}

	/// <summary>
	/// Indicates if values can be written.
	/// </summary>
	/// <param name="memberMap">The member map.</param>
	/// <returns>True if values can be written.</returns>
	public virtual bool CanWrite(MemberMap memberMap)
	{
		FuzzingLogsCollector.Log("CsvWriter", "CanWrite", 791);
		var cantWrite =
			// Ignored members.
			memberMap.Data.Ignore;

		if (memberMap.Data.Member is PropertyInfo property)
		{
			FuzzingLogsCollector.Log("CsvWriter", "CanWrite", 798);
			cantWrite = cantWrite ||
			// Properties that don't have a public getter
			// and we are honoring the accessor modifier.
			property.GetGetMethod() == null && !includePrivateMembers ||
			// Properties that don't have a getter at all.
			property.GetGetMethod(true) == null;
		}

		FuzzingLogsCollector.Log("CsvWriter", "CanWrite", 807);
		return !cantWrite;
	}

	/// <summary>
	/// Determines the type for the given record.
	/// </summary>
	/// <typeparam name="T">The type of the record.</typeparam>
	/// <param name="record">The record to determine the type of.</param>
	/// <returns>The System.Type for the record.</returns>
	public virtual RecordTypeInfo GetTypeInfoForRecord<T>(T? record)
	{
		FuzzingLogsCollector.Log("CsvWriter", "GetTypeInfoForRecord<T>", 819);
		var type = typeof(T);
		if (type == typeof(object) && record != null)
		{
			FuzzingLogsCollector.Log("CsvWriter", "GetTypeInfoForRecord<T>", 823);
			return new RecordTypeInfo(record.GetType(), true);
		}

		FuzzingLogsCollector.Log("CsvWriter", "GetTypeInfoForRecord<T>", 827);
		return new RecordTypeInfo(type, false);
	}

	/// <summary>
	/// Sanitizes the given field, before it is injected.
	/// </summary>
	/// <param name="field">The field to sanitize.</param>
	/// <returns>The sanitized field.</returns>
	/// <exception cref="WriterException">Thrown when an injection character is found in the field.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual string? SanitizeForInjection(string? field)
	{
		FuzzingLogsCollector.Log("CsvWriter", "SanitizeForInjection", 840);
		if (field == null || field.Length == 0)
		{
			FuzzingLogsCollector.Log("CsvWriter", "SanitizeForInjection", 843);
			return field;
		}

		int injectionCharIndex;
		if (ArrayHelper.Contains(injectionCharacters, field[0]))
		{
			FuzzingLogsCollector.Log("CsvWriter", "SanitizeForInjection", 850);
			injectionCharIndex = 0;
		}
		else if (field[0] == quote && field[field.Length - 1] == quote && ArrayHelper.Contains(injectionCharacters, field[1]))
		{
			FuzzingLogsCollector.Log("CsvWriter", "SanitizeForInjection", 855);
			injectionCharIndex = 1;
		}
		else
		{
			FuzzingLogsCollector.Log("CsvWriter", "SanitizeForInjection", 860);
			return field;
		}

		if (injectionOptions == InjectionOptions.Exception)
		{
			FuzzingLogsCollector.Log("CsvWriter", "SanitizeForInjection", 866);
			throw new WriterException(context, $"Injection character '{field[injectionCharIndex]}' detected");
		}

		if (injectionOptions == InjectionOptions.Escape)
		{
			FuzzingLogsCollector.Log("CsvWriter", "SanitizeForInjection", 872);
			if (injectionCharIndex == 0)
			{
				FuzzingLogsCollector.Log("CsvWriter", "SanitizeForInjection", 875);
				// =1+"2 -> "'=1+""2"
				field = quoteString + injectionEscapeCharacter + field.Replace(quoteString, escapeQuoteString) + quoteString;
			}
			else
			{
				FuzzingLogsCollector.Log("CsvWriter", "SanitizeForInjection", 881);
				// "=1+2" -> "'=1+2"
				field = quoteString + injectionEscapeCharacter + field.Substring(injectionCharIndex);
			}
		}
		else if (injectionOptions == InjectionOptions.Strip)
		{
			FuzzingLogsCollector.Log("CsvWriter", "SanitizeForInjection", 888);
			while (true)
			{
				FuzzingLogsCollector.Log("CsvWriter", "SanitizeForInjection", 891);
				field = field.Substring(1);

				if (field.Length == 0 || !ArrayHelper.Contains(injectionCharacters, field[0]))
				{
					FuzzingLogsCollector.Log("CsvWriter", "SanitizeForInjection", 896);
					break;
				}
			}

			if (injectionCharIndex == 1)
			{
				FuzzingLogsCollector.Log("CsvWriter", "SanitizeForInjection", 903);
				field = quoteString + field;
			}
		}

		FuzzingLogsCollector.Log("CsvWriter", "SanitizeForInjection", 908);
		return field;
	}

	/// <summary>
	/// Writes the given value to the buffer.
	/// </summary>
	/// <param name="value">The value to write.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void WriteToBuffer(string? value)
	{
		FuzzingLogsCollector.Log("CsvWriter", "WriteToBuffer", 919);
		var length = value?.Length ?? 0;

		if (value == null || length == 0)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteToBuffer", 924);
			return;
		}

		var lengthNeeded = bufferPosition + length;
		if (lengthNeeded >= bufferSize)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteToBuffer", 931);
			while (lengthNeeded >= bufferSize)
			{
				FuzzingLogsCollector.Log("CsvWriter", "WriteToBuffer", 934);
				bufferSize *= 2;
			}

			Array.Resize(ref buffer, bufferSize);
		}

		FuzzingLogsCollector.Log("CsvWriter", "WriteToBuffer", 941);
		value.CopyTo(0, buffer, bufferPosition, length);

		bufferPosition += length;
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		FuzzingLogsCollector.Log("CsvWriter", "Dispose", 950);
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Disposes the object.
	/// </summary>
	/// <param name="disposing">Indicates if the object is being disposed.</param>
	protected virtual void Dispose(bool disposing)
	{
		FuzzingLogsCollector.Log("CsvWriter", "Dispose", 961);
		if (disposed)
		{
			FuzzingLogsCollector.Log("CsvWriter", "Dispose", 964);
			return;
		}

		Flush();

		if (disposing)
		{
			FuzzingLogsCollector.Log("CsvWriter", "Dispose", 972);
			// Dispose managed state (managed objects)

			if (!leaveOpen)
			{
				FuzzingLogsCollector.Log("CsvWriter", "Dispose", 977);
				writer.Dispose();
			}
		}

		// Free unmanaged resources (unmanaged objects) and override finalizer
		// Set large fields to null

		FuzzingLogsCollector.Log("CsvWriter", "Dispose", 985);
		disposed = true;
	}

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		FuzzingLogsCollector.Log("CsvWriter", "DisposeAsync", 992);
		await DisposeAsync(true).ConfigureAwait(false);
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc/>
	protected virtual async ValueTask DisposeAsync(bool disposing)
	{
		FuzzingLogsCollector.Log("CsvWriter", "DisposeAsync", 1000);
		if (disposed)
		{
			FuzzingLogsCollector.Log("CsvWriter", "DisposeAsync", 1003);
			return;
		}

		await FlushAsync().ConfigureAwait(false);

		if (disposing)
		{
			FuzzingLogsCollector.Log("CsvWriter", "DisposeAsync", 1011);
			// Dispose managed state (managed objects)

			if (!leaveOpen)
			{
				FuzzingLogsCollector.Log("CsvWriter", "DisposeAsync", 1016);
				await writer.DisposeAsync().ConfigureAwait(false);
			}
		}

		// Free unmanaged resources (unmanaged objects) and override finalizer
		// Set large fields to null

		FuzzingLogsCollector.Log("CsvWriter", "DisposeAsync", 1024);
		disposed = true;
	}

	private bool WriteHeaderFromType<T>()
	{
		FuzzingLogsCollector.Log("CsvWriter", "WriteHeaderFromType<T>", 1030);
		if (!hasHeaderRecord || hasHeaderBeenWritten)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteHeaderFromType<T>", 1033);
			return false;
		}

		var recordType = typeof(T);
		var isPrimitive = recordType.GetTypeInfo().IsPrimitive;
		if (!isPrimitive && recordType != typeof(object))
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteHeaderFromType<T>", 1041);
			WriteHeader(recordType);

			return hasHeaderBeenWritten;
		}

		FuzzingLogsCollector.Log("CsvWriter", "WriteHeaderFromType<T>", 1047);
		return false;
	}

	private bool WriteHeaderFromRecord(object? record)
	{
		FuzzingLogsCollector.Log("CsvWriter", "WriteHeaderFromType<T>", 1053);
		if (!hasHeaderRecord || hasHeaderBeenWritten)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteHeaderFromType<T>", 1056);
			return false;
		}

		if (record == null)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteHeaderFromType<T>", 1062);
			return false;
		}

		if (record is IDynamicMetaObjectProvider dynamicObject)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteHeaderFromType<T>", 1068);
			WriteDynamicHeader(dynamicObject);

			return true;
		}

		var recordType = record.GetType();
		var isPrimitive = recordType.GetTypeInfo().IsPrimitive;
		if (!isPrimitive)
		{
			FuzzingLogsCollector.Log("CsvWriter", "WriteHeaderFromType<T>", 1078);
			WriteHeader(recordType);

			return true;
		}

		FuzzingLogsCollector.Log("CsvWriter", "WriteHeaderFromType<T>", 1084);
		return false;
	}
}
