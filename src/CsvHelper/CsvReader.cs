// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration;
using CsvHelper.Expressions;
using CsvHelper.TypeConversion;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using CsvHelper.FuzzingLogger;

namespace CsvHelper;

/// <summary>
/// Reads data that was parsed from <see cref="IParser" />.
/// </summary>
public class CsvReader : IReader
{
	private readonly Lazy<RecordManager> recordManager;
	private readonly bool detectColumnCountChanges;
	private readonly Dictionary<string, List<int>> namedIndexes = new Dictionary<string, List<int>>();
	private readonly Dictionary<string, (string, int)> namedIndexCache = new Dictionary<string, (string, int)>();
	private readonly Dictionary<Type, TypeConverterOptions> typeConverterOptionsCache = new Dictionary<Type, TypeConverterOptions>();
	private readonly MemberMapData reusableMemberMapData = new MemberMapData(null);
	private readonly bool hasHeaderRecord;
	private readonly HeaderValidated? headerValidated;
	private readonly ShouldSkipRecord? shouldSkipRecord;
	private readonly ReadingExceptionOccurred? readingExceptionOccurred;
	private readonly CultureInfo cultureInfo;
	private readonly bool ignoreBlankLines;
	private readonly MissingFieldFound? missingFieldFound;
	private readonly bool includePrivateMembers;
	private readonly PrepareHeaderForMatch prepareHeaderForMatch;

	private CsvContext context;
	private bool disposed;
	private IParser parser;
	private int prevColumnCount;
	private int currentIndex = -1;
	private bool hasBeenRead;
	private string[]? headerRecord;

	/// <inheritdoc/>
	public virtual int ColumnCount => parser.Count;

	/// <inheritdoc/>
	public virtual int CurrentIndex => currentIndex;

	/// <inheritdoc/>
	public virtual string[]? HeaderRecord => headerRecord;

	/// <inheritdoc/>
	public virtual CsvContext Context => context;

	/// <inheritdoc/>
	public virtual IReaderConfiguration Configuration { get; private set; }

	/// <inheritdoc/>
	public virtual IParser Parser => parser;

	/// <summary>
	/// Creates a new CSV reader using the given <see cref="TextReader" />.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <param name="culture">The culture.</param>
	/// <param name="leaveOpen"><c>true</c> to leave the <see cref="TextReader"/> open after the <see cref="CsvReader"/> object is disposed, otherwise <c>false</c>.</param>
	public CsvReader(TextReader reader, CultureInfo culture, bool leaveOpen = false) : this(new CsvParser(reader, culture, leaveOpen)) { }

	/// <summary>
	/// Creates a new CSV reader using the given <see cref="TextReader" /> and
	/// <see cref="CsvHelper.Configuration.CsvConfiguration" /> and <see cref="CsvParser" /> as the default parser.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <param name="configuration">The configuration.</param>
	/// <param name="leaveOpen"><c>true</c> to leave the <see cref="TextReader"/> open after the <see cref="CsvReader"/> object is disposed, otherwise <c>false</c>.</param>
	public CsvReader(TextReader reader, IReaderConfiguration configuration, bool leaveOpen = false) : this(new CsvParser(reader, configuration, leaveOpen)) { }

	/// <summary>
	/// Creates a new CSV reader using the given <see cref="IParser" />.
	/// </summary>
	/// <param name="parser">The <see cref="IParser" /> used to parse the CSV file.</param>
	public CsvReader(IParser parser)
	{
		FuzzingLogsCollector.Log("CsvReader", "CsvReader", 85);
		Configuration = parser.Configuration as IReaderConfiguration ?? throw new ConfigurationException($"The {nameof(IParser)} configuration must implement {nameof(IReaderConfiguration)} to be used in {nameof(CsvReader)}.");

		this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
		context = parser.Context ?? throw new InvalidOperationException($"For {nameof(IParser)} to be used in {nameof(CsvReader)}, {nameof(IParser.Context)} must also implement {nameof(CsvContext)}.");
		context.Reader = this;
		recordManager = new Lazy<RecordManager>(() => ObjectResolver.Current.Resolve<RecordManager>(this));

		cultureInfo = Configuration.CultureInfo;
		detectColumnCountChanges = Configuration.DetectColumnCountChanges;
		hasHeaderRecord = Configuration.HasHeaderRecord;
		headerValidated = Configuration.HeaderValidated;
		ignoreBlankLines = Configuration.IgnoreBlankLines;
		includePrivateMembers = Configuration.IncludePrivateMembers;
		missingFieldFound = Configuration.MissingFieldFound;
		prepareHeaderForMatch = Configuration.PrepareHeaderForMatch;
		readingExceptionOccurred = Configuration.ReadingExceptionOccurred;
		shouldSkipRecord = Configuration.ShouldSkipRecord;
	}

	/// <inheritdoc/>
	public virtual bool ReadHeader()
	{
		FuzzingLogsCollector.Log("CsvReader", "ReadHeader", 108);
		if (!hasHeaderRecord)
		{
			FuzzingLogsCollector.Log("CsvReader", "ReadHeader", 111);
			throw new ReaderException(context, "Configuration.HasHeaderRecord is false.");
		}

		headerRecord = parser.Record;
		ParseNamedIndexes();

		FuzzingLogsCollector.Log("CsvReader", "ReadHeader", 118);
		return headerRecord != null;
	}

	/// <summary>
	/// Validates the header to be of the given type.
	/// </summary>
	/// <typeparam name="T">The expected type of the header</typeparam>
	public virtual void ValidateHeader<T>()
	{
		ValidateHeader(typeof(T));
	}

	/// <summary>
	/// Validates the header to be of the given type.
	/// </summary>
	/// <param name="type">The expected type of the header.</param>
	public virtual void ValidateHeader(Type type)
	{
		FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 137);
		if (hasHeaderRecord == false)
		{
			FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 140);
			throw new InvalidOperationException($"Validation can't be performed on a the header if no header exists. {nameof(Configuration.HasHeaderRecord)} can't be false.");
		}

		CheckHasBeenRead();

		if (headerRecord == null)
		{
			FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 148);
			throw new InvalidOperationException($"The header must be read before it can be validated.");
		}

		if (context.Maps[type] == null)
		{
			FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 154);
			context.Maps.Add(context.AutoMap(type));
		}

		var map = context.Maps[type]!; // The map was added above if null.
		var invalidHeaders = new List<InvalidHeader>();
		ValidateHeader(map, invalidHeaders);

		var args = new HeaderValidatedArgs(invalidHeaders.ToArray(), context);
		headerValidated?.Invoke(args);
		FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 164);
	}

	/// <summary>
	/// Validates the header to be of the given type.
	/// </summary>
	/// <param name="map">The mapped classes.</param>
	/// <param name="invalidHeaders">The invalid headers.</param>
	protected virtual void ValidateHeader(ClassMap map, List<InvalidHeader> invalidHeaders)
	{
		FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 174);
		foreach (var parameter in map.ParameterMaps)
		{
			FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 177);
			if (parameter.Data.Ignore)
			{
				FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 180);
				continue;
			}

			if (parameter.Data.IsConstantSet)
			{
				FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 186);
				// If ConvertUsing and Constant don't require a header.
				continue;
			}

			if (parameter.Data.IsIndexSet && !parameter.Data.IsNameSet)
			{
				FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 193);
				// If there is only an index set, we don't want to validate the header name.
				continue;
			}

			if (parameter.ConstructorTypeMap != null)
			{
				FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 200);
				ValidateHeader(parameter.ConstructorTypeMap, invalidHeaders);
			}
			else if (parameter.ReferenceMap != null)
			{
				FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 205);
				ValidateHeader(parameter.ReferenceMap.Data.Mapping, invalidHeaders);
			}
			else
			{
				FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 210);
				var index = GetFieldIndex(parameter.Data.Names, parameter.Data.NameIndex, true);
				var isValid = index != -1 || parameter.Data.IsOptional;
				if (!isValid)
				{
					FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 215);
					invalidHeaders.Add(new InvalidHeader { Index = parameter.Data.NameIndex, Names = parameter.Data.Names.ToList() });
				}
			}
		}

		foreach (var memberMap in map.MemberMaps)
		{
			FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 223);
			if (memberMap.Data.Ignore || !CanRead(memberMap))
			{
				FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 226);
				continue;
			}

			if (memberMap.Data.ReadingConvertExpression != null || memberMap.Data.IsConstantSet)
			{
				FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 232);
				// If ConvertUsing and Constant don't require a header.
				continue;
			}

			if (memberMap.Data.IsIndexSet && !memberMap.Data.IsNameSet)
			{
				FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 239);
				// If there is only an index set, we don't want to validate the header name.
				continue;
			}

			var index = GetFieldIndex(memberMap.Data.Names, memberMap.Data.NameIndex, true);
			var isValid = index != -1 || memberMap.Data.IsOptional;
			if (!isValid)
			{
				FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 248);
				invalidHeaders.Add(new InvalidHeader { Index = memberMap.Data.NameIndex, Names = memberMap.Data.Names.ToList() });
			}
		}

		foreach (var referenceMap in map.ReferenceMaps)
		{
			FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 255);
			if (!CanRead(referenceMap))
			{
				FuzzingLogsCollector.Log("CsvReader", "ValidateHeader", 258);
				continue;
			}

			ValidateHeader(referenceMap.Data.Mapping, invalidHeaders);
		}
	}

	/// <inheritdoc/>
	public virtual bool Read()
	{
		FuzzingLogsCollector.Log("CsvReader", "Read", 269);
		// Don't forget about the async method below!

		bool hasMoreRecords;
		do
		{
			FuzzingLogsCollector.Log("CsvReader", "Read", 275);
			hasMoreRecords = parser.Read();
			hasBeenRead = true;
		}
		while (hasMoreRecords && (shouldSkipRecord?.Invoke(new ShouldSkipRecordArgs(this)) ?? false));

		currentIndex = -1;

		if (detectColumnCountChanges && hasMoreRecords)
		{
			FuzzingLogsCollector.Log("CsvReader", "Read", 285);
			if (prevColumnCount > 0 && prevColumnCount != parser.Count)
			{
				FuzzingLogsCollector.Log("CsvReader", "Read", 288);
				var csvException = new BadDataException(string.Empty, parser.RawRecord, context, "An inconsistent number of columns has been detected.");

				var args = new ReadingExceptionOccurredArgs(csvException);
				if (readingExceptionOccurred?.Invoke(args) ?? true)
				{
					FuzzingLogsCollector.Log("CsvReader", "Read", 294);
					throw csvException;
				}
			}

			prevColumnCount = parser.Count;
		}

		FuzzingLogsCollector.Log("CsvReader", "Read", 302);
		return hasMoreRecords;
	}

	/// <inheritdoc/>
	public virtual async Task<bool> ReadAsync()
	{
		FuzzingLogsCollector.Log("CsvReader", "ReadAsync", 309);
		bool hasMoreRecords;
		do
		{
			FuzzingLogsCollector.Log("CsvReader", "ReadAsync", 313);
			hasMoreRecords = await parser.ReadAsync().ConfigureAwait(false);
			hasBeenRead = true;
		}
		while (hasMoreRecords && (shouldSkipRecord?.Invoke(new ShouldSkipRecordArgs(this)) ?? false));

		currentIndex = -1;

		if (detectColumnCountChanges && hasMoreRecords)
		{
			FuzzingLogsCollector.Log("CsvReader", "ReadAsync", 323);
			if (prevColumnCount > 0 && prevColumnCount != parser.Count)
			{
				FuzzingLogsCollector.Log("CsvReader", "ReadAsync", 326);
				var csvException = new BadDataException(string.Empty, parser.RawRecord, context, "An inconsistent number of columns has been detected.");

				var args = new ReadingExceptionOccurredArgs(csvException);
				if (readingExceptionOccurred?.Invoke(args) ?? true)
				{
					FuzzingLogsCollector.Log("CsvReader", "ReadAsync", 332);
					throw csvException;
				}
			}

			prevColumnCount = parser.Count;
		}

		FuzzingLogsCollector.Log("CsvReader", "ReadAsync", 340);
		return hasMoreRecords;
	}

	/// <inheritdoc/>
	public virtual string? this[int index]
	{
		get
		{
			FuzzingLogsCollector.Log("CsvReader", "get", 349);
			CheckHasBeenRead();

			return GetField(index);
		}
	}

	/// <inheritdoc/>
	public virtual string? this[string name]
	{
		get
		{
			FuzzingLogsCollector.Log("CsvReader", "get", 361);
			CheckHasBeenRead();

			return GetField(name);
		}
	}

	/// <inheritdoc/>
	public virtual string? this[string name, int index]
	{
		get
		{
			FuzzingLogsCollector.Log("CsvReader", "get", 373);
			CheckHasBeenRead();

			return GetField(name, index);
		}
	}

	/// <inheritdoc/>
	public virtual string? GetField(int index)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetField", 383);
		CheckHasBeenRead();

		// Set the current index being used so we
		// have more information if an error occurs
		// when reading records.
		currentIndex = index;

		if (index >= parser.Count || index < 0)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetField", 393);
			var args = new MissingFieldFoundArgs(null, index, context);
			missingFieldFound?.Invoke(args);
			return default;
		}

		var field = parser[index];

		FuzzingLogsCollector.Log("CsvReader", "GetField", 401);
		return field;
	}

	/// <inheritdoc/>
	public virtual string? GetField(string name)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetField", 408);
		CheckHasBeenRead();

		var index = GetFieldIndex(name);
		if (index < 0)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetField", 414);
			return null;
		}

		FuzzingLogsCollector.Log("CsvReader", "GetField", 418);
		return GetField(index);
	}

	/// <inheritdoc/>
	public virtual string? GetField(string name, int index)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetField", 425);
		CheckHasBeenRead();

		var fieldIndex = GetFieldIndex(name, index);
		if (fieldIndex < 0)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetField", 431);
			return null;
		}

		FuzzingLogsCollector.Log("CsvReader", "GetField", 435);
		return GetField(fieldIndex);
	}

	/// <inheritdoc/>
	public virtual object? GetField(Type type, int index)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetField", 442);
		CheckHasBeenRead();

		var converter = context.TypeConverterCache.GetConverter(type);
		return GetField(type, index, converter);
	}

	/// <inheritdoc/>
	public virtual object? GetField(Type type, string name)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetField", 452);
		CheckHasBeenRead();

		var converter = context.TypeConverterCache.GetConverter(type);
		return GetField(type, name, converter);
	}

	/// <inheritdoc/>
	public virtual object? GetField(Type type, string name, int index)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetField", 462);
		CheckHasBeenRead();

		var converter = context.TypeConverterCache.GetConverter(type);
		return GetField(type, name, index, converter);
	}

	/// <inheritdoc/>
	public virtual object? GetField(Type type, int index, ITypeConverter converter)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetField", 472);
		CheckHasBeenRead();

		reusableMemberMapData.Index = index;
		reusableMemberMapData.TypeConverter = converter;
		if (!typeConverterOptionsCache.TryGetValue(type, out TypeConverterOptions? typeConverterOptions))
		{
			FuzzingLogsCollector.Log("CsvReader", "GetField", 479);
			typeConverterOptions = TypeConverterOptions.Merge(new TypeConverterOptions { CultureInfo = cultureInfo }, context.TypeConverterOptionsCache.GetOptions(type));
			typeConverterOptionsCache.Add(type, typeConverterOptions);
		}

		reusableMemberMapData.TypeConverterOptions = typeConverterOptions;

		var field = GetField(index);
		FuzzingLogsCollector.Log("CsvReader", "GetField", 487);
		return converter.ConvertFromString(field, this, reusableMemberMapData);
	}

	/// <inheritdoc/>
	public virtual object? GetField(Type type, string name, ITypeConverter converter)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetField", 494);
		CheckHasBeenRead();

		var index = GetFieldIndex(name);
		return GetField(type, index, converter);
	}

	/// <inheritdoc/>
	public virtual object? GetField(Type type, string name, int index, ITypeConverter converter)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetField", 504);
		CheckHasBeenRead();

		var fieldIndex = GetFieldIndex(name, index);
		return GetField(type, fieldIndex, converter);
	}

	/// <inheritdoc/>
	public virtual T? GetField<T>(int index)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetField<T>", 514);
		CheckHasBeenRead();

		var converter = context.TypeConverterCache.GetConverter<T>();
		return GetField<T>(index, converter);
	}

	/// <inheritdoc/>
	public virtual T? GetField<T>(string name)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetField<T>", 524);
		CheckHasBeenRead();

		var converter = context.TypeConverterCache.GetConverter<T>();
		return GetField<T>(name, converter);
	}

	/// <inheritdoc/>
	public virtual T? GetField<T>(string name, int index)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetField<T>", 534);
		CheckHasBeenRead();

		var converter = context.TypeConverterCache.GetConverter<T>();
		return GetField<T>(name, index, converter);
	}

	/// <inheritdoc/>
	public virtual T? GetField<T>(int index, ITypeConverter converter)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetField<T>", 544);
		CheckHasBeenRead();

		if (index >= parser.Count || index < 0)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetField<T>", 549);
			currentIndex = index;
			var args = new MissingFieldFoundArgs(null, index, context);
			missingFieldFound?.Invoke(args);

			return default;
		}

		FuzzingLogsCollector.Log("CsvReader", "GetField<T>", 557);
		return (T?)GetField(typeof(T), index, converter);
	}

	/// <inheritdoc/>
	public virtual T? GetField<T>(string name, ITypeConverter converter)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetField<T>", 564);
		CheckHasBeenRead();

		var index = GetFieldIndex(name);
		return GetField<T>(index, converter);
	}

	/// <inheritdoc/>
	public virtual T? GetField<T>(string name, int index, ITypeConverter converter)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetField<T>", 574);
		CheckHasBeenRead();

		var fieldIndex = GetFieldIndex(name, index);
		return GetField<T>(fieldIndex, converter);
	}

	/// <inheritdoc/>
	public virtual T? GetField<T, TConverter>(int index) where TConverter : ITypeConverter
	{
		FuzzingLogsCollector.Log("CsvReader", "GetField<T, TConverter>", 584);
		CheckHasBeenRead();

		var converter = ObjectResolver.Current.Resolve<TConverter>();
		return GetField<T>(index, converter);
	}

	/// <inheritdoc/>
	public virtual T? GetField<T, TConverter>(string name) where TConverter : ITypeConverter
	{
		FuzzingLogsCollector.Log("CsvReader", "GetField<T, TConverter>", 594);
		CheckHasBeenRead();

		var converter = ObjectResolver.Current.Resolve<TConverter>();
		return GetField<T>(name, converter);
	}

	/// <inheritdoc/>
	public virtual T? GetField<T, TConverter>(string name, int index) where TConverter : ITypeConverter
	{
		FuzzingLogsCollector.Log("CsvReader", "GetField<T, TConverter>", 604);
		CheckHasBeenRead();

		var converter = ObjectResolver.Current.Resolve<TConverter>();
		return GetField<T>(name, index, converter);
	}

	/// <inheritdoc/>
	public virtual bool TryGetField(Type type, int index, out object? field)
	{
		FuzzingLogsCollector.Log("CsvReader", "TryGetField", 614);
		CheckHasBeenRead();

		var converter = context.TypeConverterCache.GetConverter(type);
		return TryGetField(type, index, converter, out field);
	}

	/// <inheritdoc/>
	public virtual bool TryGetField(Type type, string name, out object? field)
	{
		FuzzingLogsCollector.Log("CsvReader", "TryGetField", 624);
		CheckHasBeenRead();

		var converter = context.TypeConverterCache.GetConverter(type);
		return TryGetField(type, name, converter, out field);
	}

	/// <inheritdoc/>
	public virtual bool TryGetField(Type type, string name, int index, out object? field)
	{
		FuzzingLogsCollector.Log("CsvReader", "TryGetField", 634);
		CheckHasBeenRead();

		var converter = context.TypeConverterCache.GetConverter(type);
		return TryGetField(type, name, index, converter, out field);
	}

	/// <inheritdoc/>
	public virtual bool TryGetField(Type type, int index, ITypeConverter converter, out object? field)
	{
		FuzzingLogsCollector.Log("CsvReader", "TryGetField", 644);
		CheckHasBeenRead();

		// TypeConverter.IsValid() just wraps a
		// ConvertFrom() call in a try/catch, so lets not
		// do it twice and just do it ourselves.
		try
		{
			FuzzingLogsCollector.Log("CsvReader", "TryGetField", 652);
			field = GetField(type, index, converter);
			return true;
		}
		catch
		{
			FuzzingLogsCollector.Log("CsvReader", "TryGetField", 658);
			field = type.GetTypeInfo().IsValueType ? ObjectResolver.Current.Resolve(type) : null;
			return false;
		}
	}

	/// <inheritdoc/>
	public virtual bool TryGetField(Type type, string name, ITypeConverter converter, out object? field)
	{
		FuzzingLogsCollector.Log("CsvReader", "TryGetField", 667);
		CheckHasBeenRead();

		var index = GetFieldIndex(name, isTryGet: true);
		if (index == -1)
		{
			FuzzingLogsCollector.Log("CsvReader", "TryGetField", 673);
			field = type.GetTypeInfo().IsValueType ? ObjectResolver.Current.Resolve(type) : null;
			return false;
		}

		FuzzingLogsCollector.Log("CsvReader", "TryGetField", 678);
		return TryGetField(type, index, converter, out field);
	}

	/// <inheritdoc/>
	public virtual bool TryGetField(Type type, string name, int index, ITypeConverter converter, out object? field)
	{
		FuzzingLogsCollector.Log("CsvReader", "TryGetField", 685);
		CheckHasBeenRead();

		var fieldIndex = GetFieldIndex(name, index, true);
		if (fieldIndex == -1)
		{
			FuzzingLogsCollector.Log("CsvReader", "TryGetField", 691);
			field = type.GetTypeInfo().IsValueType ? ObjectResolver.Current.Resolve(type) : null;
			return false;
		}

		FuzzingLogsCollector.Log("CsvReader", "TryGetField", 696);
		return TryGetField(type, fieldIndex, converter, out field);
	}

	/// <inheritdoc/>
	public virtual bool TryGetField<T>(int index, out T? field)
	{
		FuzzingLogsCollector.Log("CsvReader", "TryGetField<T>", 703);
		CheckHasBeenRead();

		var converter = context.TypeConverterCache.GetConverter<T>();
		return TryGetField(index, converter, out field);
	}

	/// <inheritdoc/>
	public virtual bool TryGetField<T>(string name, out T? field)
	{
		FuzzingLogsCollector.Log("CsvReader", "TryGetField<T>", 713);
		CheckHasBeenRead();

		var converter = context.TypeConverterCache.GetConverter<T>();
		return TryGetField(name, converter, out field);
	}

	/// <inheritdoc/>
	public virtual bool TryGetField<T>(string name, int index, out T? field)
	{
		FuzzingLogsCollector.Log("CsvReader", "TryGetField<T>", 723);
		CheckHasBeenRead();

		var converter = context.TypeConverterCache.GetConverter<T>();
		return TryGetField(name, index, converter, out field);
	}

	/// <inheritdoc/>
	public virtual bool TryGetField<T>(int index, ITypeConverter converter, out T? field)
	{
		FuzzingLogsCollector.Log("CsvReader", "TryGetField<T>", 733);
		CheckHasBeenRead();

		// TypeConverter.IsValid() just wraps a
		// ConvertFrom() call in a try/catch, so lets not
		// do it twice and just do it ourselves.
		try
		{
			FuzzingLogsCollector.Log("CsvReader", "TryGetField<T>", 741);
			field = GetField<T>(index, converter);
			return true;
		}
		catch
		{
			FuzzingLogsCollector.Log("CsvReader", "TryGetField<T>", 747);
			field = default;
			return false;
		}
	}

	/// <inheritdoc/>
	public virtual bool TryGetField<T>(string name, ITypeConverter converter, out T? field)
	{
		FuzzingLogsCollector.Log("CsvReader", "TryGetField<T>", 756);
		CheckHasBeenRead();

		var index = GetFieldIndex(name, isTryGet: true);
		if (index == -1)
		{
			FuzzingLogsCollector.Log("CsvReader", "TryGetField<T>", 762);
			field = default;
			return false;
		}

		FuzzingLogsCollector.Log("CsvReader", "TryGetField<T>", 767);
		return TryGetField(index, converter, out field);
	}

	/// <inheritdoc/>
	public virtual bool TryGetField<T>(string name, int index, ITypeConverter converter, out T? field)
	{
		FuzzingLogsCollector.Log("CsvReader", "TryGetField<T>", 774);
		CheckHasBeenRead();

		var fieldIndex = GetFieldIndex(name, index, true);
		if (fieldIndex == -1)
		{
			FuzzingLogsCollector.Log("CsvReader", "TryGetField<T>", 780);
			field = default;
			return false;
		}

		FuzzingLogsCollector.Log("CsvReader", "TryGetField<T>", 785);
		return TryGetField(fieldIndex, converter, out field);
	}

	/// <inheritdoc/>
	public virtual bool TryGetField<T, TConverter>(int index, out T? field) where TConverter : ITypeConverter
	{
		FuzzingLogsCollector.Log("CsvReader", "TryGetField<T>", 792);
		CheckHasBeenRead();

		var converter = ObjectResolver.Current.Resolve<TConverter>();
		FuzzingLogsCollector.Log("CsvReader", "TryGetField<T>", 796);
		return TryGetField(index, converter, out field);
	}

	/// <inheritdoc/>
	public virtual bool TryGetField<T, TConverter>(string name, out T? field) where TConverter : ITypeConverter
	{
		FuzzingLogsCollector.Log("CsvReader", "TryGetField<T, TConverter>", 803);
		CheckHasBeenRead();

		var converter = ObjectResolver.Current.Resolve<TConverter>();
		return TryGetField(name, converter, out field);
	}

	/// <inheritdoc/>
	public virtual bool TryGetField<T, TConverter>(string name, int index, out T? field) where TConverter : ITypeConverter
	{
		FuzzingLogsCollector.Log("CsvReader", "TryGetField<T, TConverter>", 813);
		CheckHasBeenRead();

		var converter = ObjectResolver.Current.Resolve<TConverter>();
		return TryGetField(name, index, converter, out field);
	}

	/// <inheritdoc/>
	public virtual T GetRecord<T>()
	{
		FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 823);
		CheckHasBeenRead();

		if (headerRecord == null && hasHeaderRecord)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 828);
			ReadHeader();
			ValidateHeader<T>();

			if (!Read())
			{
				FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 834);
				throw new ReaderException(context, "There are no records.");
			}
		}

		T record;
		try
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 842);
			var read = recordManager.Value.GetReadDelegate<T>(typeof(T));
			record = read();
		}
		catch (Exception ex)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 848);
			var csvHelperException = ex as CsvHelperException ?? new ReaderException(context, "An unexpected error occurred.", ex);

			var args = new ReadingExceptionOccurredArgs(csvHelperException);
			if (readingExceptionOccurred?.Invoke(args) ?? true)
			{
				FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 854);
				if (ex is CsvHelperException)
				{
					FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 857);
					throw;
				}
				else
				{
					FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 862);
					throw csvHelperException;
				}
			}

			record = (T?)args.Record!; // If the user is ignoring exceptions, we'll let a possible null be returned to them.
		}

		FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 870);
		return record;
	}

	/// <inheritdoc/>
	public virtual T GetRecord<T>(T anonymousTypeDefinition)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 877);
		if (anonymousTypeDefinition == null)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 880);
			throw new ArgumentNullException(nameof(anonymousTypeDefinition));
		}

		if (!anonymousTypeDefinition.GetType().IsAnonymous())
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 886);
			throw new ArgumentException($"Argument is not an anonymous type.", nameof(anonymousTypeDefinition));
		}

		FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 890);
		return GetRecord<T>();
	}

	/// <inheritdoc/>
	public virtual object GetRecord(Type type)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 897);
		CheckHasBeenRead();

		if (headerRecord == null && hasHeaderRecord)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 902);
			ReadHeader();
			ValidateHeader(type);

			if (!Read())
			{
				FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 908);
				throw new ReaderException(context, "There are no records.");
			}
		}

		object record;
		try
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 916);
			var read = recordManager.Value.GetReadDelegate<object>(type);
			record = read();
		}
		catch (Exception ex)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 922);
			var csvHelperException = ex as CsvHelperException ?? new ReaderException(context, "An unexpected error occurred.", ex);

			var args = new ReadingExceptionOccurredArgs(csvHelperException);
			if (readingExceptionOccurred?.Invoke(args) ?? true)
			{
				FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 928);
				if (ex is CsvHelperException)
				{
					FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 931);
					throw;
				}
				else
				{
					FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 936);
					throw csvHelperException;
				}
			}

			record = args.Record!; // If the user is ignoring exceptions, we'll let a possible null be returned to them.
		}

		FuzzingLogsCollector.Log("CsvReader", "GetRecord<T>", 944);
		return record;
	}

	/// <inheritdoc/>
	public virtual IEnumerable<T> GetRecords<T>()
	{
		FuzzingLogsCollector.Log("CsvReader", "GetRecords<T>", 951);
		if (disposed)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecords<T>", 954);
			throw new ObjectDisposedException(nameof(CsvReader),
				"GetRecords<T>() returns an IEnumerable<T> that yields records. This means that the method isn't actually called until " +
				"you try and access the values. e.g. .ToList() Did you create CsvReader inside a using block and are now trying to access " +
				"the records outside of that using block?"
			);
		}

		// Don't need to check if it's been read
		// since we're doing the reading ourselves.

		if (hasHeaderRecord && headerRecord == null)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecords<T>", 967);
			if (!Read())
			{
				FuzzingLogsCollector.Log("CsvReader", "GetRecords<T>", 970);
				yield break;
			}

			ReadHeader();
			ValidateHeader<T>();
		}

		FuzzingLogsCollector.Log("CsvReader", "GetRecords<T>", 978);
		Func<T>? read = null;

		while (Read())
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecords<T>", 983);
			T record;
			try
			{
				if (read == null)
				{
					FuzzingLogsCollector.Log("CsvReader", "GetRecords<T>", 989);
					read = recordManager.Value.GetReadDelegate<T>(typeof(T));
				}

				record = read();
			}
			catch (Exception ex)
			{
				FuzzingLogsCollector.Log("CsvReader", "GetRecords<T>", 997);
				var csvHelperException = ex as CsvHelperException ?? new ReaderException(context, "An unexpected error occurred.", ex);

				var args = new ReadingExceptionOccurredArgs(csvHelperException);
				if (readingExceptionOccurred?.Invoke(args) ?? true)
				{
					FuzzingLogsCollector.Log("CsvReader", "GetRecords<T>", 1003);
					if (ex is CsvHelperException)
					{
						FuzzingLogsCollector.Log("CsvReader", "GetRecords<T>", 1006);
						throw;
					}
					else
					{
						FuzzingLogsCollector.Log("CsvReader", "GetRecords<T>", 1011);
						throw csvHelperException;
					}
				}

				// If the callback doesn't throw, keep going.
				continue;
			}
			FuzzingLogsCollector.Log("CsvReader", "GetRecords<T>", 1019);
			yield return record;
		}
	}

	/// <inheritdoc/>
	public virtual IEnumerable<T> GetRecords<T>(T anonymousTypeDefinition)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetRecords<T>", 1027);
		if (anonymousTypeDefinition == null)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecords<T>", 1030);
			throw new ArgumentNullException(nameof(anonymousTypeDefinition));
		}

		if (!anonymousTypeDefinition.GetType().IsAnonymous())
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecords<T>", 1036);
			throw new ArgumentException($"Argument is not an anonymous type.", nameof(anonymousTypeDefinition));
		}

		FuzzingLogsCollector.Log("CsvReader", "GetRecords<T>", 1040);
		return GetRecords<T>();
	}

	/// <inheritdoc/>
	public virtual IEnumerable<object> GetRecords(Type type)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetRecords", 1047);
		if (disposed)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecords", 1050);
			throw new ObjectDisposedException(nameof(CsvReader),
				"GetRecords<object>() returns an IEnumerable<T> that yields records. This means that the method isn't actually called until " +
				"you try and access the values. e.g. .ToList() Did you create CsvReader inside a using block and are now trying to access " +
				"the records outside of that using block?"
			);
		}

		// Don't need to check if it's been read
		// since we're doing the reading ourselves.

		if (hasHeaderRecord && headerRecord == null)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecords", 1063);
			if (!Read())
			{
				FuzzingLogsCollector.Log("CsvReader", "GetRecords", 1066);
				yield break;
			}

			FuzzingLogsCollector.Log("CsvReader", "GetRecords", 1070);
			ReadHeader();
			ValidateHeader(type);
		}

		FuzzingLogsCollector.Log("CsvReader", "GetRecords", 1074);
		Func<object>? read = null;

		while (Read())
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecords", 1079);
			object record;
			try
			{
				if (read == null)
				{
					FuzzingLogsCollector.Log("CsvReader", "GetRecords", 1085);
					read = recordManager.Value.GetReadDelegate<object>(type);
				}

				record = read();
			}
			catch (Exception ex)
			{
				FuzzingLogsCollector.Log("CsvReader", "GetRecords", 1093);
				var csvHelperException = ex as CsvHelperException ?? new ReaderException(context, "An unexpected error occurred.", ex);

				var args = new ReadingExceptionOccurredArgs(csvHelperException);
				if (readingExceptionOccurred?.Invoke(args) ?? true)
				{
					FuzzingLogsCollector.Log("CsvReader", "GetRecords", 1099);
					if (ex is CsvHelperException)
					{
						FuzzingLogsCollector.Log("CsvReader", "GetRecords", 1102);
						throw;
					}
					else
					{
						FuzzingLogsCollector.Log("CsvReader", "GetRecords", 1107);
						throw csvHelperException;
					}
				}

				// If the callback doesn't throw, keep going.
				continue;
			}
			FuzzingLogsCollector.Log("CsvReader", "GetRecords", 1115);
			yield return record;
		}
	}

	/// <inheritdoc/>
	public virtual IEnumerable<T> EnumerateRecords<T>(T record)
	{
		FuzzingLogsCollector.Log("CsvReader", "EnumerateRecords<T>", 1124);
		if (disposed)
		{
			FuzzingLogsCollector.Log("CsvReader", "EnumerateRecords<T>", 1127);
			throw new ObjectDisposedException(nameof(CsvReader),
				"GetRecords<T>() returns an IEnumerable<T> that yields records. This means that the method isn't actually called until " +
				"you try and access the values. e.g. .ToList() Did you create CsvReader inside a using block and are now trying to access " +
				"the records outside of that using block?"
			);
		}

		// Don't need to check if it's been read
		// since we're doing the reading ourselves.

		if (hasHeaderRecord && headerRecord == null)
		{
			FuzzingLogsCollector.Log("CsvReader", "EnumerateRecords<T>", 1140);
			if (!Read())
			{
				FuzzingLogsCollector.Log("CsvReader", "EnumerateRecords<T>", 1143);
				yield break;
			}

			FuzzingLogsCollector.Log("CsvReader", "EnumerateRecords<T>", 1147);
			ReadHeader();
			ValidateHeader<T>();
		}

		while (Read())
		{
			FuzzingLogsCollector.Log("CsvReader", "EnumerateRecords<T>", 1154);
			try
			{
				recordManager.Value.Hydrate(record);
			}
			catch (Exception ex)
			{
				FuzzingLogsCollector.Log("CsvReader", "EnumerateRecords<T>", 1161);
				var csvHelperException = ex as CsvHelperException ?? new ReaderException(context, "An unexpected error occurred.", ex);

				var args = new ReadingExceptionOccurredArgs(csvHelperException);
				if (readingExceptionOccurred?.Invoke(args) ?? true)
				{
					FuzzingLogsCollector.Log("CsvReader", "EnumerateRecords<T>", 1167);
					if (ex is CsvHelperException)
					{
						FuzzingLogsCollector.Log("CsvReader", "EnumerateRecords<T>", 1170);
						throw;
					}
					else
					{
						FuzzingLogsCollector.Log("CsvReader", "EnumerateRecords<T>", 1175);
						throw csvHelperException;
					}
				}

				// If the callback doesn't throw, keep going.
				continue;
			}

			FuzzingLogsCollector.Log("CsvReader", "EnumerateRecords<T>", 1184);
			yield return record;
		}
	}

	/// <inheritdoc/>
	public virtual async IAsyncEnumerable<T> GetRecordsAsync<T>([EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
	{
		FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync<T>", 1192);
		if (disposed)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync<T>", 1195);
			throw new ObjectDisposedException(nameof(CsvReader),
				"GetRecords<T>() returns an IEnumerable<T> that yields records. This means that the method isn't actually called until " +
				"you try and access the values. Did you create CsvReader inside a using block and are now trying to access " +
				"the records outside of that using block?"
			);
		}

		// Don't need to check if it's been read
		// since we're doing the reading ourselves.

		if (hasHeaderRecord && headerRecord == null)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync<T>", 1208);
			if (!await ReadAsync().ConfigureAwait(false))
			{
				FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync<T>", 1211);
				yield break;
			}

			ReadHeader();
			ValidateHeader<T>();
		}

		FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync<T>", 1219);
		Func<T>? read = null;

		while (await ReadAsync().ConfigureAwait(false))
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync<T>", 1224);
			cancellationToken.ThrowIfCancellationRequested();
			T record;
			try
			{
				FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync<T>", 1229);
				if (read == null)
				{
					FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync<T>", 1232);
					read = recordManager.Value.GetReadDelegate<T>(typeof(T));
				}

				record = read();
			}
			catch (Exception ex)
			{
				FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync<T>", 1240);
				var csvHelperException = ex as CsvHelperException ?? new ReaderException(context, "An unexpected error occurred.", ex);

				var args = new ReadingExceptionOccurredArgs(csvHelperException);
				if (readingExceptionOccurred?.Invoke(args) ?? true)
				{
					FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync<T>", 1246);
					if (ex is CsvHelperException)
					{
						FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync<T>", 1249);
						throw;
					}
					else
					{
						FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync<T>", 1254);
						throw csvHelperException;
					}
				}

				// If the callback doesn't throw, keep going.
				continue;
			}

			FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync<T>", 1263);
			yield return record;
		}
	}

	/// <inheritdoc/>
	public virtual IAsyncEnumerable<T> GetRecordsAsync<T>(T anonymousTypeDefinition, CancellationToken cancellationToken = default)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync<T>", 1271);
		if (anonymousTypeDefinition == null)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync<T>", 1274);
			throw new ArgumentNullException(nameof(anonymousTypeDefinition));
		}

		if (!anonymousTypeDefinition.GetType().IsAnonymous())
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync<T>", 1280);
			throw new ArgumentException($"Argument is not an anonymous type.", nameof(anonymousTypeDefinition));
		}

		FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync<T>", 1284);
		return GetRecordsAsync<T>(cancellationToken);
	}

	/// <inheritdoc/>
	public virtual async IAsyncEnumerable<object> GetRecordsAsync(Type type, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync", 1291);
		if (disposed)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync", 1294);
			throw new ObjectDisposedException(nameof(CsvReader),
				"GetRecords<object>() returns an IEnumerable<T> that yields records. This means that the method isn't actually called until " +
				"you try and access the values. Did you create CsvReader inside a using block and are now trying to access " +
				"the records outside of that using block?"
			);
		}

		// Don't need to check if it's been read
		// since we're doing the reading ourselves.

		if (hasHeaderRecord && headerRecord == null)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync", 1307);
			if (!await ReadAsync().ConfigureAwait(false))
			{
				FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync", 1310);
				yield break;
			}

			ReadHeader();
			ValidateHeader(type);
		}

		Func<object>? read = null;

		while (await ReadAsync().ConfigureAwait(false))
		{
			FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync", 1322);
			cancellationToken.ThrowIfCancellationRequested();
			object record;
			try
			{
				FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync", 1327);
				if (read == null)
				{
					FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync", 1330);
					read = recordManager.Value.GetReadDelegate<object>(type);
				}

				record = read();
			}
			catch (Exception ex)
			{
				FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync", 1338);
				var csvHelperException = ex as CsvHelperException ?? new ReaderException(context, "An unexpected error occurred.", ex);

				var args = new ReadingExceptionOccurredArgs(csvHelperException);
				if (readingExceptionOccurred?.Invoke(args) ?? true)
				{
					FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync", 1344);
					if (ex is CsvHelperException)
					{
						FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync", 1347);
						throw;
					}
					else
					{
						FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync", 1352);
						throw csvHelperException;
					}
				}

				// If the callback doesn't throw, keep going.
				continue;
			}

			FuzzingLogsCollector.Log("CsvReader", "GetRecordsAsync", 1361);
			yield return record;
		}
	}

	/// <inheritdoc/>
	public virtual async IAsyncEnumerable<T> EnumerateRecordsAsync<T>(T record, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		FuzzingLogsCollector.Log("CsvReader", "EnumerateRecordsAsync<T>", 1369);
		if (disposed)
		{
			FuzzingLogsCollector.Log("CsvReader", "EnumerateRecordsAsync<T>", 1372);
			throw new ObjectDisposedException(nameof(CsvReader),
				"GetRecords<T>() returns an IEnumerable<T> that yields records. This means that the method isn't actually called until " +
				"you try and access the values. Did you create CsvReader inside a using block and are now trying to access " +
				"the records outside of that using block?"
			);
		}

		// Don't need to check if it's been read
		// since we're doing the reading ourselves.

		if (hasHeaderRecord && headerRecord == null)
		{
			FuzzingLogsCollector.Log("CsvReader", "EnumerateRecordsAsync<T>", 1385);
			if (!await ReadAsync().ConfigureAwait(false))
			{
				FuzzingLogsCollector.Log("CsvReader", "EnumerateRecordsAsync<T>", 1388);
				yield break;
			}

			ReadHeader();
			ValidateHeader<T>();
		}

		while (await ReadAsync().ConfigureAwait(false))
		{
			FuzzingLogsCollector.Log("CsvReader", "EnumerateRecordsAsync<T>", 1398);
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				FuzzingLogsCollector.Log("CsvReader", "EnumerateRecordsAsync<T>", 1402);
				recordManager.Value.Hydrate(record);
			}
			catch (Exception ex)
			{
				FuzzingLogsCollector.Log("CsvReader", "EnumerateRecordsAsync<T>", 1407);
				var csvHelperException = ex as CsvHelperException ?? new ReaderException(context, "An unexpected error occurred.", ex);

				var args = new ReadingExceptionOccurredArgs(csvHelperException);
				if (readingExceptionOccurred?.Invoke(args) ?? true)
				{
					FuzzingLogsCollector.Log("CsvReader", "EnumerateRecordsAsync<T>", 1413);
					if (ex is CsvHelperException)
					{
						FuzzingLogsCollector.Log("CsvReader", "EnumerateRecordsAsync<T>", 1416);
						throw;
					}
					else
					{
						FuzzingLogsCollector.Log("CsvReader", "EnumerateRecordsAsync<T>", 1421);
						throw csvHelperException;
					}
				}

				// If the callback doesn't throw, keep going.
				continue;
			}
			FuzzingLogsCollector.Log("CsvReader", "EnumerateRecordsAsync<T>", 1429);
			yield return record;
		}
	}

	/// <summary>
	/// Gets the index of the field with the given name.
	/// </summary>
	/// <param name="name">The name of the field.</param>
	/// <param name="index">The index of the field.</param>
	/// <param name="isTryGet">Indicates if a TryGet is executed.</param>
	/// <returns>The index of the field.</returns>
	public virtual int GetFieldIndex(string name, int index = 0, bool isTryGet = false)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetFieldIndex", 1443);
		return GetFieldIndex(new[] { name }, index, isTryGet);
	}

	/// <summary>
	/// Gets the index of the field with the given name.
	/// </summary>
	/// <param name="names">The names of the field.</param>
	/// <param name="index">The index of the field.</param>
	/// <param name="isTryGet">Indicates if a TryGet is executed.</param>
	/// <param name="isOptional">Indicates if the field is optional.</param>
	/// <returns>The index of the field.</returns>
	public virtual int GetFieldIndex(IEnumerable<string> names, int index = 0, bool isTryGet = false, bool isOptional = false)
	{
		FuzzingLogsCollector.Log("CsvReader", "GetFieldIndex", 1457);
		if (names == null)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetFieldIndex", 1460);
			throw new ArgumentNullException(nameof(names));
		}

		if (!hasHeaderRecord)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetFieldIndex", 1466);
			throw new ReaderException(context, "There is no header record to determine the index by name.");
		}

		if (headerRecord == null)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetFieldIndex", 1472);
			throw new ReaderException(context, "The header has not been read. You must call ReadHeader() before any fields can be retrieved by name.");
		}

		// Caching the named index speeds up mappings that use ConvertUsing tremendously.
		var nameKey = string.Join("_", names) + index;
		if (namedIndexCache.TryGetValue(nameKey, out var cache))
		{
			FuzzingLogsCollector.Log("CsvReader", "GetFieldIndex", 1480);
			(var cachedName, var cachedIndex) = cache;
			return namedIndexes[cachedName][cachedIndex];
		}

		FuzzingLogsCollector.Log("CsvReader", "GetFieldIndex", 1485);
		// Check all possible names for this field.
		string? name = null;
		var i = 0;
		foreach (var n in names)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetFieldIndex", 1491);
			// Get the list of indexes for this name.
			var args = new PrepareHeaderForMatchArgs(n, i);
			var fieldName = prepareHeaderForMatch(args);
			if (namedIndexes.ContainsKey(fieldName ?? string.Empty))
			{
				FuzzingLogsCollector.Log("CsvReader", "GetFieldIndex", 1497);
				name = fieldName;
				break;
			}

			i++;
		}

		// Check if the index position exists.
		if (name == null || index >= namedIndexes[name].Count)
		{
			FuzzingLogsCollector.Log("CsvReader", "GetFieldIndex", 1508);
			// It doesn't exist. The field is missing.
			if (!isTryGet && !isOptional)
			{
				FuzzingLogsCollector.Log("CsvReader", "GetFieldIndex", 1512);
				var args = new MissingFieldFoundArgs(names.ToArray(), index, context);
				missingFieldFound?.Invoke(args);
			}

			FuzzingLogsCollector.Log("CsvReader", "GetFieldIndex", 1517);
			return -1;
		}

		namedIndexCache.Add(nameKey, (name, index));

		FuzzingLogsCollector.Log("CsvReader", "GetFieldIndex", 1523);
		return namedIndexes[name][index];
	}

	/// <summary>
	/// Indicates if values can be read.
	/// </summary>
	/// <param name="memberMap">The member map.</param>
	/// <returns>True if values can be read.</returns>
	public virtual bool CanRead(MemberMap memberMap)
	{
		FuzzingLogsCollector.Log("CsvReader", "CanRead", 1534);
		var cantRead =
			// Ignored member;
			memberMap.Data.Ignore;

		var property = memberMap.Data.Member as PropertyInfo;
		if (property != null)
		{
			FuzzingLogsCollector.Log("CsvReader", "CanRead", 1542);
			cantRead = cantRead ||
				// Properties that don't have a public setter
				// and we are honoring the accessor modifier.
				property.GetSetMethod() == null && !includePrivateMembers ||
				// Properties that don't have a setter at all.
				property.GetSetMethod(true) == null;
		}

		FuzzingLogsCollector.Log("CsvReader", "CanRead", 1551);
		return !cantRead;
	}

	/// <summary>
	/// Indicates if values can be read.
	/// </summary>
	/// <param name="memberReferenceMap">The member reference map.</param>
	/// <returns>True if values can be read.</returns>
	public virtual bool CanRead(MemberReferenceMap memberReferenceMap)
	{
		FuzzingLogsCollector.Log("CsvReader", "CanRead", 1562);
		var cantRead = false;

		var property = memberReferenceMap.Data.Member as PropertyInfo;
		if (property != null)
		{
			FuzzingLogsCollector.Log("CsvReader", "CanRead", 1568);
			cantRead =
				// Properties that don't have a public setter
				// and we are honoring the accessor modifier.
				property.GetSetMethod() == null && !includePrivateMembers ||
				// Properties that don't have a setter at all.
				property.GetSetMethod(true) == null;
		}

		FuzzingLogsCollector.Log("CsvReader", "CanRead", 1577);
		return !cantRead;
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		FuzzingLogsCollector.Log("CsvReader", "Dispose", 1584);
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Disposes the object.
	/// </summary>
	/// <param name="disposing">Indicates if the object is being disposed.</param>
	protected virtual void Dispose(bool disposing)
	{
		FuzzingLogsCollector.Log("CsvReader", "Dispose", 1595);
		if (disposed)
		{
			FuzzingLogsCollector.Log("CsvReader", "Dispose", 1598);
			return;
		}

		// Dispose managed state (managed objects)
		if (disposing)
		{
			FuzzingLogsCollector.Log("CsvReader", "Dispose", 1605);
			parser.Dispose();
		}

		// Free unmanaged resources (unmanaged objects) and override finalizer
		// Set large fields to null

		FuzzingLogsCollector.Log("CsvReader", "Dispose", 1612);
		disposed = true;
	}

	/// <summary>
	/// Checks if the file has been read.
	/// </summary>
	/// <exception cref="ReaderException">Thrown when the file has not yet been read.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual void CheckHasBeenRead()
	{
		FuzzingLogsCollector.Log("CsvReader", "CheckHasBeenRead", 1623);
		if (!hasBeenRead)
		{
			FuzzingLogsCollector.Log("CsvReader", "CheckHasBeenRead", 1626);
			throw new ReaderException(context, "You must call read on the reader before accessing its data.");
		}
	}

	/// <summary>
	/// Parses the named indexes.
	/// </summary>
	/// <exception cref="ReaderException">Thrown when no header record was found.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual void ParseNamedIndexes()
	{
		FuzzingLogsCollector.Log("CsvReader", "ParseNamedIndexes", 1638);
		if (headerRecord == null)
		{
			FuzzingLogsCollector.Log("CsvReader", "ParseNamedIndexes", 1641);
			throw new ReaderException(context, "No header record was found.");
		}

		namedIndexes.Clear();
		namedIndexCache.Clear();

		for (var i = 0; i < headerRecord.Length; i++)
		{
			FuzzingLogsCollector.Log("CsvReader", "ParseNamedIndexes", 1650);
			var args = new PrepareHeaderForMatchArgs(headerRecord[i], i);
			var name = prepareHeaderForMatch(args);
			if (namedIndexes.TryGetValue(name, out var index))
			{
				FuzzingLogsCollector.Log("CsvReader", "ParseNamedIndexes", 1655);
				index.Add(i);
			}
			else
			{
				FuzzingLogsCollector.Log("CsvReader", "ParseNamedIndexes", 1660);
				namedIndexes[name] = new List<int> { i };
			}
		}
	}
}
