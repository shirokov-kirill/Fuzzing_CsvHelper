// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration;
using CsvHelper.Delegates;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using CsvHelper.FuzzingLogger;

namespace CsvHelper;

/// <summary>
/// Parses a CSV file.
/// </summary>
public class CsvParser : IParser, IDisposable
{
	private readonly IParserConfiguration configuration;
	private readonly FieldCache fieldCache = new FieldCache();
	private readonly TextReader reader;
	private readonly char quote;
	private readonly char escape;
	private readonly bool countBytes;
	private readonly Encoding encoding;
	private readonly bool ignoreBlankLines;
	private readonly char comment;
	private readonly bool allowComments;
	private readonly BadDataFound? badDataFound;
	private readonly bool lineBreakInQuotedFieldIsBadData;
	private readonly TrimOptions trimOptions;
	private readonly char[] whiteSpaceChars;
	private readonly bool leaveOpen;
	private readonly CsvMode mode;
	private readonly string newLine;
	private readonly char newLineFirstChar;
	private readonly bool isNewLineSet;
	private readonly bool cacheFields;
	private readonly string[] delimiterValues;
	private readonly bool detectDelimiter;
	private readonly double maxFieldSize;

	private string delimiter;
	private char delimiterFirstChar;
	private char[] buffer;
	private int bufferSize;
	private int charsRead;
	private int bufferPosition;
	private int rowStartPosition;
	private int fieldStartPosition;
	private int row;
	private int rawRow;
	private long charCount;
	private long byteCount;
	private bool inQuotes;
	private bool inEscape;
	private Field[] fields;
	private string[] processedFields;
	private int fieldsPosition;
	private bool disposed;
	private int quoteCount;
	private char[] processFieldBuffer;
	private int processFieldBufferSize;
	private ParserState state;
	private int delimiterPosition = 1;
	private int newLinePosition = 1;
	private bool fieldIsBadData;
	private bool fieldIsQuoted;
	private bool isProcessingField;
	private bool isRecordProcessed;
	private string[] record = [];

	/// <inheritdoc/>
	public long CharCount => charCount;

	/// <inheritdoc/>
	public long ByteCount => byteCount;

	/// <inheritdoc/>
	public int Row => row;

	/// <inheritdoc/>
	public string[]? Record
	{
		get
		{
			FuzzingLogsCollector.Log("CsvParser", "get", 87);
			if (isRecordProcessed == true)
			{
				FuzzingLogsCollector.Log("CsvParser", "get", 90);
				return this.record;
			}

			if (fieldsPosition == 0)
			{
				FuzzingLogsCollector.Log("CsvParser", "get", 96);
				return null;
			}

			var record = new string[fieldsPosition];

			for (var i = 0; i < record.Length; i++)
			{
				FuzzingLogsCollector.Log("CsvParser", "get", 104);
				record[i] = this[i];
			}

			this.record = record;
			isRecordProcessed = true;

			FuzzingLogsCollector.Log("CsvParser", "get", 111);
			return this.record;
		}
	}

	/// <inheritdoc/>
	public string RawRecord => new string(buffer, rowStartPosition, bufferPosition - rowStartPosition);

	/// <inheritdoc/>
	public int Count => fieldsPosition;

	/// <inheritdoc/>
	public int RawRow => rawRow;

	/// <inheritdoc/>
	public string Delimiter => delimiter;

	/// <inheritdoc/>
	public CsvContext Context { get; private set; }

	/// <inheritdoc/>
	public IParserConfiguration Configuration => configuration;

	/// <inheritdoc/>
	public string this[int index]
	{
		get
		{
			FuzzingLogsCollector.Log("CsvParser", "get", 139);
			if (isProcessingField)
			{
				FuzzingLogsCollector.Log("CsvParser", "get", 142);
				var message =
					$"You can't access {nameof(IParser)}[int] or {nameof(IParser)}.{nameof(IParser.Record)} inside of the {nameof(BadDataFound)} callback. " +
					$"Use {nameof(BadDataFoundArgs)}.{nameof(BadDataFoundArgs.Field)} and {nameof(BadDataFoundArgs)}.{nameof(BadDataFoundArgs.RawRecord)} instead."
				;

				throw new ParserException(Context, message);
			}

			isProcessingField = true;

			var field = GetField(index);

			isProcessingField = false;

			FuzzingLogsCollector.Log("CsvParser", "get", 157);
			return field;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CsvParser"/> class.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <param name="culture">The culture.</param>
	/// <param name="leaveOpen">if set to <c>true</c> [leave open].</param>
	public CsvParser(TextReader reader, CultureInfo culture, bool leaveOpen = false) : this(reader, new CsvConfiguration(culture), leaveOpen) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="CsvParser"/> class.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <param name="configuration">The configuration.</param>
	/// <param name="leaveOpen">if set to <c>true</c> [leave open].</param>
	public CsvParser(TextReader reader, IParserConfiguration configuration, bool leaveOpen = false)
	{
		FuzzingLogsCollector.Log("CsvParser", "CsvParser", 178);
		this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
		this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

		configuration.Validate();

		Context = new CsvContext(this);

		allowComments = configuration.AllowComments;
		badDataFound = configuration.BadDataFound;
		bufferSize = configuration.BufferSize;
		cacheFields = configuration.CacheFields;
		comment = configuration.Comment;
		countBytes = configuration.CountBytes;
		delimiter = configuration.Delimiter;
		delimiterFirstChar = configuration.Delimiter[0];
		delimiterValues = configuration.DetectDelimiterValues;
		detectDelimiter = configuration.DetectDelimiter;
		encoding = configuration.Encoding;
		escape = configuration.Escape;
		ignoreBlankLines = configuration.IgnoreBlankLines;
		isNewLineSet = configuration.IsNewLineSet;
		this.leaveOpen = leaveOpen;
		lineBreakInQuotedFieldIsBadData = configuration.LineBreakInQuotedFieldIsBadData;
		maxFieldSize = configuration.MaxFieldSize;
		newLine = configuration.NewLine;
		newLineFirstChar = configuration.NewLine[0];
		mode = configuration.Mode;
		processFieldBufferSize = configuration.ProcessFieldBufferSize;
		quote = configuration.Quote;
		whiteSpaceChars = configuration.WhiteSpaceChars;
		trimOptions = configuration.TrimOptions;

		buffer = new char[bufferSize];
		processFieldBuffer = new char[processFieldBufferSize];
		fields = new Field[128];
		processedFields = new string[128];
	}

	/// <inheritdoc/>
	public bool Read()
	{
		FuzzingLogsCollector.Log("CsvParser", "Read", 220);
		isRecordProcessed = false;
		rowStartPosition = bufferPosition;
		fieldStartPosition = rowStartPosition;
		fieldsPosition = 0;
		quoteCount = 0;
		row++;
		rawRow++;
		var c = '\0';
		var cPrev = c;

		while (true)
		{
			FuzzingLogsCollector.Log("CsvParser", "Read", 233);
			if (bufferPosition >= charsRead)
			{
				FuzzingLogsCollector.Log("CsvParser", "Read", 236);
				if (!FillBuffer())
				{
					FuzzingLogsCollector.Log("CsvParser", "Read", 239);
					return ReadEndOfFile();
				}

				if (row == 1 && detectDelimiter)
				{
					FuzzingLogsCollector.Log("CsvParser", "Read", 245);
					DetectDelimiter();
				}
			}

			if (ReadLine(ref c, ref cPrev) == ReadLineResult.Complete)
			{
				FuzzingLogsCollector.Log("CsvParser", "Read", 252);
				return true;
			}
		}
	}

	/// <inheritdoc/>
	public async Task<bool> ReadAsync()
	{
		FuzzingLogsCollector.Log("CsvParser", "ReadAsync", 261);
		isRecordProcessed = false;
		rowStartPosition = bufferPosition;
		fieldStartPosition = rowStartPosition;
		fieldsPosition = 0;
		quoteCount = 0;
		row++;
		rawRow++;
		var c = '\0';
		var cPrev = c;

		while (true)
		{
			FuzzingLogsCollector.Log("CsvParser", "ReadAsync", 274);
			if (bufferPosition >= charsRead)
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadAsync", 277);
				if (!await FillBufferAsync().ConfigureAwait(false))
				{
					FuzzingLogsCollector.Log("CsvParser", "ReadAsync", 280);
					return ReadEndOfFile();
				}

				if (row == 1 && detectDelimiter)
				{
					FuzzingLogsCollector.Log("CsvParser", "ReadAsync", 286);
					DetectDelimiter();
				}
			}

			if (ReadLine(ref c, ref cPrev) == ReadLineResult.Complete)
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadAsync", 293);
				return true;
			}
		}
	}

	private void DetectDelimiter()
	{
		FuzzingLogsCollector.Log("CsvParser", "DetectDelimiter", 301);
		var text = new string(buffer, 0, charsRead);
		var newDelimiter = configuration.GetDelimiter(new GetDelimiterArgs(text, configuration));
		if (newDelimiter != null)
		{
			FuzzingLogsCollector.Log("CsvParser", "DetectDelimiter", 306);
			delimiter = newDelimiter;
			delimiterFirstChar = newDelimiter[0];
			configuration.Validate();
		}
	}

	private ReadLineResult ReadLine(ref char c, ref char cPrev)
	{
		FuzzingLogsCollector.Log("CsvParser", "ReadLine", 315);
		while (bufferPosition < charsRead)
		{
			FuzzingLogsCollector.Log("CsvParser", "ReadLine", 318);
			if (state != ParserState.None)
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadLine", 321);
				// Continue the state before doing anything else.
				ReadLineResult result;
				switch (state)
				{
					case ParserState.Spaces:
						FuzzingLogsCollector.Log("CsvParser", "ReadLine", 327);
						result = ReadSpaces(ref c);
						break;
					case ParserState.BlankLine:
						FuzzingLogsCollector.Log("CsvParser", "ReadLine", 331);
						result = ReadBlankLine(ref c);
						break;
					case ParserState.Delimiter:
						FuzzingLogsCollector.Log("CsvParser", "ReadLine", 335);
						result = ReadDelimiter(ref c);
						break;
					case ParserState.LineEnding:
						FuzzingLogsCollector.Log("CsvParser", "ReadLine", 339);
						result = ReadLineEnding(ref c);
						break;
					case ParserState.NewLine:
						FuzzingLogsCollector.Log("CsvParser", "ReadLine", 343);
						result = ReadNewLine(ref c);
						break;
					default:
						FuzzingLogsCollector.Log("CsvParser", "ReadLine", 347);
						throw new InvalidOperationException($"Parser state '{state}' is not valid.");
				}

				FuzzingLogsCollector.Log("CsvParser", "ReadLine", 351);
				var shouldReturn =
					// Buffer needs to be filled.
					result == ReadLineResult.Incomplete ||
					// Done reading row.
					result == ReadLineResult.Complete && (state == ParserState.LineEnding || state == ParserState.NewLine)
				;

				if (result == ReadLineResult.Complete)
				{
					FuzzingLogsCollector.Log("CsvParser", "ReadLine", 361);
					state = ParserState.None;
				}

				if (shouldReturn)
				{
					FuzzingLogsCollector.Log("CsvParser", "ReadLine", 367);
					return result;
				}
			}

			cPrev = c;
			c = buffer[bufferPosition];
			bufferPosition++;
			charCount++;

			if (countBytes)
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadLine", 379);
				byteCount += encoding.GetByteCount(new char[] { c });
			}

			if (maxFieldSize > 0 && bufferPosition - fieldStartPosition - 1 > maxFieldSize)
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadLine", 385);
				throw new MaxFieldSizeException(Context);
			}

			var isFirstCharOfRow = rowStartPosition == bufferPosition - 1;
			if (isFirstCharOfRow && (allowComments && c == comment || ignoreBlankLines && ((c == '\r' || c == '\n') && !isNewLineSet || c == newLineFirstChar && isNewLineSet)))
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadLine", 392);
				state = ParserState.BlankLine;
				var result = ReadBlankLine(ref c);
				if (result == ReadLineResult.Complete)
				{
					FuzzingLogsCollector.Log("CsvParser", "ReadLine", 397);
					state = ParserState.None;

					continue;
				}
				else
				{
					FuzzingLogsCollector.Log("CsvParser", "ReadLine", 404);
					return ReadLineResult.Incomplete;
				}
			}

			if (mode == CsvMode.RFC4180)
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadLine", 411);
				var isFirstCharOfField = fieldStartPosition == bufferPosition - 1;
				if (isFirstCharOfField)
				{
					FuzzingLogsCollector.Log("CsvParser", "ReadLine", 415);
					if ((trimOptions & TrimOptions.Trim) == TrimOptions.Trim && ArrayHelper.Contains(whiteSpaceChars, c))
					{
						FuzzingLogsCollector.Log("CsvParser", "ReadLine", 418);
						// Skip through whitespace. This is so we can process the field later.
						var result = ReadSpaces(ref c);
						if (result == ReadLineResult.Incomplete)
						{
							FuzzingLogsCollector.Log("CsvParser", "ReadLine", 423);
							fieldStartPosition = bufferPosition;
							return result;
						}
					}

					// Fields are only quoted if the first character is a quote.
					// If not, read until a delimiter or newline is found.
					fieldIsQuoted = c == quote;
				}

				if (fieldIsQuoted)
				{
					FuzzingLogsCollector.Log("CsvParser", "ReadLine", 436);
					if (c == quote || c == escape)
					{
						FuzzingLogsCollector.Log("CsvParser", "ReadLine", 439);
						quoteCount++;

						if (!inQuotes && !isFirstCharOfField && cPrev != escape)
						{
							FuzzingLogsCollector.Log("CsvParser", "ReadLine", 444);
							fieldIsBadData = true;
						}
						else if (!fieldIsBadData)
						{
							FuzzingLogsCollector.Log("CsvParser", "ReadLine", 449);
							// Don't process field quotes after bad data has been detected.
							inQuotes = !inQuotes;
						}
					}

					if (inQuotes)
					{
						FuzzingLogsCollector.Log("CsvParser", "ReadLine", 457);
						// If we are in quotes we don't want to do any special
						// processing (e.g. of delimiters) until we hit the ending
						// quote. But the newline logic may vary.

						if (!(c == '\r' || (c == '\n' && cPrev != '\r')))
						{
							FuzzingLogsCollector.Log("CsvParser", "ReadLine", 464);
							// We are not at (the beginning of) a newline,
							// so just keep reading.
							continue;
						}

						rawRow++;

						if (lineBreakInQuotedFieldIsBadData)
						{
							FuzzingLogsCollector.Log("CsvParser", "ReadLine", 474);
							// This newline is not valid within the field.
							// We will consume the newline and then end the
							// field (and the row).
							// This avoids growing the field (and the buffer)
							// until another quote is found.
							fieldIsBadData = true;
						}
						else
						{
							FuzzingLogsCollector.Log("CsvParser", "ReadLine", 484);
							// We are at a newline but it is considered valid
							// within a (quoted) field. We keep reading until
							// we find the closing quote.
							continue;
						}
					}
				}
				else
				{
					FuzzingLogsCollector.Log("CsvParser", "ReadLine", 494);
					if (c == quote || c == escape)
					{
						FuzzingLogsCollector.Log("CsvParser", "ReadLine", 497);
						// If the field isn't quoted but contains a
						// quote or escape, it's has bad data.
						fieldIsBadData = true;
					}
				}
			}
			else if (mode == CsvMode.Escape)
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadLine", 506);
				if (inEscape)
				{
					FuzzingLogsCollector.Log("CsvParser", "ReadLine", 509);
					inEscape = false;

					continue;
				}

				if (c == escape)
				{
					FuzzingLogsCollector.Log("CsvParser", "ReadLine", 517);
					inEscape = true;

					continue;
				}
			}

			if (c == delimiterFirstChar)
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadLine", 526);
				state = ParserState.Delimiter;
				var result = ReadDelimiter(ref c);
				if (result == ReadLineResult.Incomplete)
				{
					FuzzingLogsCollector.Log("CsvParser", "ReadLine", 531);
					return result;
				}

				state = ParserState.None;

				continue;
			}

			if (!isNewLineSet && (c == '\r' || c == '\n'))
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadLine", 542);
				state = ParserState.LineEnding;
				var result = ReadLineEnding(ref c);
				if (result == ReadLineResult.Complete)
				{
					FuzzingLogsCollector.Log("CsvParser", "ReadLine", 547);
					state = ParserState.None;
				}

				return result;
			}

			if (isNewLineSet && c == newLineFirstChar)
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadLine", 556);
				state = ParserState.NewLine;
				var result = ReadNewLine(ref c);
				if (result == ReadLineResult.Complete)
				{
					FuzzingLogsCollector.Log("CsvParser", "ReadLine", 561);
					state = ParserState.None;
				}

				return result;
			}
		}

		FuzzingLogsCollector.Log("CsvParser", "ReadLine", 569);
		return ReadLineResult.Incomplete;
	}

	private ReadLineResult ReadSpaces(ref char c)
	{
		FuzzingLogsCollector.Log("CsvParser", "ReadSpaces", 575);
		while (ArrayHelper.Contains(whiteSpaceChars, c))
		{
			FuzzingLogsCollector.Log("CsvParser", "ReadSpaces", 578);
			if (bufferPosition >= charsRead)
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadSpaces", 581);
				return ReadLineResult.Incomplete;
			}

			c = buffer[bufferPosition];
			bufferPosition++;
			charCount++;
			if (countBytes)
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadSpaces", 590);
				byteCount += encoding.GetByteCount(new char[] { c });
			}
		}

		FuzzingLogsCollector.Log("CsvParser", "ReadSpaces", 595);
		return ReadLineResult.Complete;
	}

	private ReadLineResult ReadBlankLine(ref char c)
	{
		FuzzingLogsCollector.Log("CsvParser", "ReadBlankLine", 601);
		while (bufferPosition < charsRead)
		{
			FuzzingLogsCollector.Log("CsvParser", "ReadBlankLine", 604);
			if (c == '\r' || c == '\n')
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadBlankLine", 607);
				var result = ReadLineEnding(ref c);
				if (result == ReadLineResult.Complete)
				{
					FuzzingLogsCollector.Log("CsvParser", "ReadBlankLine", 611);
					rowStartPosition = bufferPosition;
					fieldStartPosition = rowStartPosition;
					row++;
					rawRow++;
				}

				FuzzingLogsCollector.Log("CsvParser", "ReadBlankLine", 618);
				return result;
			}

			c = buffer[bufferPosition];
			bufferPosition++;
			charCount++;
			if (countBytes)
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadBlankLine", 627);
				byteCount += encoding.GetByteCount(new char[] { c });
			}
		}

		FuzzingLogsCollector.Log("CsvParser", "ReadBlankLine", 632);
		return ReadLineResult.Incomplete;
	}

	private ReadLineResult ReadDelimiter(ref char c)
	{
		FuzzingLogsCollector.Log("CsvParser", "ReadDelimiter", 638);
		for (var i = delimiterPosition; i < delimiter.Length; i++)
		{
			FuzzingLogsCollector.Log("CsvParser", "ReadDelimiter", 641);
			if (bufferPosition >= charsRead)
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadDelimiter", 644);
				return ReadLineResult.Incomplete;
			}

			delimiterPosition++;

			c = buffer[bufferPosition];
			if (c != delimiter[i])
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadDelimiter", 653);
				c = buffer[bufferPosition - 1];
				delimiterPosition = 1;

				return ReadLineResult.Complete;
			}

			bufferPosition++;
			charCount++;
			if (countBytes)
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadDelimiter", 664);
				byteCount += encoding.GetByteCount(new[] { c });
			}

			if (bufferPosition >= charsRead)
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadDelimiter", 670);
				return ReadLineResult.Incomplete;
			}
		}

		AddField(fieldStartPosition, bufferPosition - fieldStartPosition - delimiter.Length);

		fieldStartPosition = bufferPosition;
		delimiterPosition = 1;
		fieldIsBadData = false;

		FuzzingLogsCollector.Log("CsvParser", "ReadDelimiter", 681);
		return ReadLineResult.Complete;
	}

	private ReadLineResult ReadLineEnding(ref char c)
	{
		FuzzingLogsCollector.Log("CsvParser", "ReadLineEnding", 687);
		var lessChars = 1;

		if (c == '\r')
		{
			FuzzingLogsCollector.Log("CsvParser", "ReadLineEnding", 692);
			if (bufferPosition >= charsRead)
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadLineEnding", 695);
				return ReadLineResult.Incomplete;
			}

			c = buffer[bufferPosition];

			if (c == '\n')
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadLineEnding", 703);
				lessChars++;
				bufferPosition++;
				charCount++;
				if (countBytes)
				{
					FuzzingLogsCollector.Log("CsvParser", "ReadLineEnding", 709);
					byteCount += encoding.GetByteCount(new char[] { c });
				}
			}
		}

		if (state == ParserState.LineEnding)
		{
			FuzzingLogsCollector.Log("CsvParser", "ReadLineEnding", 717);
			AddField(fieldStartPosition, bufferPosition - fieldStartPosition - lessChars);
		}

		fieldIsBadData = false;

		FuzzingLogsCollector.Log("CsvParser", "ReadLineEnding", 723);
		return ReadLineResult.Complete;
	}

	private ReadLineResult ReadNewLine(ref char c)
	{
		FuzzingLogsCollector.Log("CsvParser", "ReadNewLine", 729);
		for (var i = newLinePosition; i < newLine.Length; i++)
		{
			FuzzingLogsCollector.Log("CsvParser", "ReadNewLine", 732);
			if (bufferPosition >= charsRead)
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadNewLine", 735);
				return ReadLineResult.Incomplete;
			}

			newLinePosition++;

			c = buffer[bufferPosition];
			if (c != newLine[i])
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadNewLine", 744);
				c = buffer[bufferPosition - 1];
				newLinePosition = 1;

				return ReadLineResult.Complete;
			}

			bufferPosition++;
			charCount++;
			if (countBytes)
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadNewLine", 755);
				byteCount += encoding.GetByteCount(new[] { c });
			}

			if (bufferPosition >= charsRead)
			{
				FuzzingLogsCollector.Log("CsvParser", "ReadNewLine", 761);
				return ReadLineResult.Incomplete;
			}
		}

		FuzzingLogsCollector.Log("CsvParser", "ReadNewLine", 766);
		AddField(fieldStartPosition, bufferPosition - fieldStartPosition - newLine.Length);

		fieldStartPosition = bufferPosition;
		newLinePosition = 1;
		fieldIsBadData = false;

		return ReadLineResult.Complete;
	}

	private bool ReadEndOfFile()
	{
		FuzzingLogsCollector.Log("CsvParser", "ReadEndOfFile", 778);
		var state = this.state;
		this.state = ParserState.None;

		if (state == ParserState.BlankLine)
		{
			FuzzingLogsCollector.Log("CsvParser", "ReadEndOfFile", 784);
			return false;
		}

		if (state == ParserState.Delimiter)
		{
			FuzzingLogsCollector.Log("CsvParser", "ReadEndOfFile", 790);
			AddField(fieldStartPosition, bufferPosition - fieldStartPosition - delimiter.Length);

			fieldStartPosition = bufferPosition;

			AddField(fieldStartPosition, bufferPosition - fieldStartPosition);

			return true;
		}

		if (state == ParserState.LineEnding)
		{
			FuzzingLogsCollector.Log("CsvParser", "ReadEndOfFile", 802);
			AddField(fieldStartPosition, bufferPosition - fieldStartPosition - 1);

			return true;
		}

		if (state == ParserState.NewLine)
		{
			FuzzingLogsCollector.Log("CsvParser", "ReadEndOfFile", 810);
			AddField(fieldStartPosition, bufferPosition - fieldStartPosition - newLine.Length);

			return true;
		}

		if (rowStartPosition < bufferPosition)
		{
			FuzzingLogsCollector.Log("CsvParser", "ReadEndOfFile", 818);
			AddField(fieldStartPosition, bufferPosition - fieldStartPosition);
		}

		FuzzingLogsCollector.Log("CsvParser", "ReadEndOfFile", 822);
		return fieldsPosition > 0;
	}

	private void AddField(int start, int length)
	{
		FuzzingLogsCollector.Log("CsvParser", "AddField", 828);
		if (fieldsPosition >= fields.Length)
		{
			FuzzingLogsCollector.Log("CsvParser", "AddField", 831);
			var newSize = fields.Length * 2;
			Array.Resize(ref fields, newSize);
			Array.Resize(ref processedFields, newSize);
		}

		FuzzingLogsCollector.Log("CsvParser", "AddField", 837);
		ref var field = ref fields[fieldsPosition];
		field.Start = start - rowStartPosition;
		field.Length = length;
		field.QuoteCount = quoteCount;
		field.IsBad = fieldIsBadData;
		field.IsProcessed = false;

		fieldsPosition++;
		quoteCount = 0;
	}

	private bool FillBuffer()
	{
		// Don't forget the async method below.
		FuzzingLogsCollector.Log("CsvParser", "FillBuffer", 852);
		if (rowStartPosition == 0 && charCount > 0 && charsRead == bufferSize)
		{
			FuzzingLogsCollector.Log("CsvParser", "FillBuffer", 855);
			// The record is longer than the memory buffer. Increase the buffer.
			bufferSize *= 2;
			var tempBuffer = new char[bufferSize];
			buffer.CopyTo(tempBuffer, 0);
			buffer = tempBuffer;
		}

		var charsLeft = Math.Max(charsRead - rowStartPosition, 0);

		Array.Copy(buffer, rowStartPosition, buffer, 0, charsLeft);

		fieldStartPosition -= rowStartPosition;
		rowStartPosition = 0;
		bufferPosition = charsLeft;

		charsRead = reader.Read(buffer, charsLeft, buffer.Length - charsLeft);
		if (charsRead == 0)
		{
			FuzzingLogsCollector.Log("CsvParser", "FillBuffer", 874);
			return false;
		}

		charsRead += charsLeft;

		FuzzingLogsCollector.Log("CsvParser", "FillBuffer", 880);
		return true;
	}

	private async Task<bool> FillBufferAsync()
	{
		FuzzingLogsCollector.Log("CsvParser", "FillBufferAsync", 886);
		if (rowStartPosition == 0 && charCount > 0 && charsRead == bufferSize)
		{
			FuzzingLogsCollector.Log("CsvParser", "FillBufferAsync", 889);
			// The record is longer than the memory buffer. Increase the buffer.
			bufferSize *= 2;
			var tempBuffer = new char[bufferSize];
			buffer.CopyTo(tempBuffer, 0);
			buffer = tempBuffer;
		}

		var charsLeft = Math.Max(charsRead - rowStartPosition, 0);

		Array.Copy(buffer, rowStartPosition, buffer, 0, charsLeft);

		fieldStartPosition -= rowStartPosition;
		rowStartPosition = 0;
		bufferPosition = charsLeft;

		charsRead = await reader.ReadAsync(buffer, charsLeft, buffer.Length - charsLeft).ConfigureAwait(false);
		if (charsRead == 0)
		{
			FuzzingLogsCollector.Log("CsvParser", "FillBufferAsync", 908);
			return false;
		}

		charsRead += charsLeft;

		FuzzingLogsCollector.Log("CsvParser", "FillBufferAsync", 914);
		return true;
	}

	private string GetField(int index)
	{
		FuzzingLogsCollector.Log("CsvParser", "GetField", 920);
		if (index > fieldsPosition)
		{
			FuzzingLogsCollector.Log("CsvParser", "GetField", 923);
			throw new IndexOutOfRangeException();
		}

		ref var field = ref fields[index];

		if (field.Length == 0)
		{
			FuzzingLogsCollector.Log("CsvParser", "GetField", 931);
			return string.Empty;
		}

		if (field.IsProcessed)
		{
			FuzzingLogsCollector.Log("CsvParser", "GetField", 937);
			return processedFields[index];
		}

		var start = field.Start + rowStartPosition;
		var length = field.Length;
		var quoteCount = field.QuoteCount;

		ProcessedField processedField;
		switch (mode)
		{
			case CsvMode.RFC4180:
				FuzzingLogsCollector.Log("CsvParser", "GetField", 949);
				processedField = field.IsBad
					? ProcessRFC4180BadField(start, length)
					: ProcessRFC4180Field(start, length, quoteCount);
				break;
			case CsvMode.Escape:
				FuzzingLogsCollector.Log("CsvParser", "GetField", 955);
				processedField = ProcessEscapeField(start, length);
				break;
			case CsvMode.NoEscape:
				FuzzingLogsCollector.Log("CsvParser", "GetField", 959);
				processedField = ProcessNoEscapeField(start, length);
				break;
			default:
				FuzzingLogsCollector.Log("CsvParser", "GetField", 963);
				throw new InvalidOperationException($"ParseMode '{mode}' is not handled.");
		}

		var value = cacheFields
			? fieldCache.GetField(processedField.Buffer, processedField.Start, processedField.Length)
			: new string(processedField.Buffer, processedField.Start, processedField.Length);

		processedFields[index] = value;
		field.IsProcessed = true;

		FuzzingLogsCollector.Log("CsvParser", "GetField", 974);
		return value;
	}

	/// <summary>
	/// Processes a field that complies with RFC4180.
	/// </summary>
	/// <param name="start">The start index of the field.</param>
	/// <param name="length">The length of the field.</param>
	/// <param name="quoteCount">The number of counted quotes.</param>
	/// <returns>The processed field.</returns>
	protected ProcessedField ProcessRFC4180Field(int start, int length, int quoteCount)
	{
		FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180Field", 987);
		var newStart = start;
		var newLength = length;

		if ((trimOptions & TrimOptions.Trim) == TrimOptions.Trim)
		{
			FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180Field", 993);
			ArrayHelper.Trim(buffer, ref newStart, ref newLength, whiteSpaceChars);
		}

		if (quoteCount == 0)
		{
			FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180Field", 999);
			// Not quoted.
			// No processing needed.

			return new ProcessedField(newStart, newLength, buffer);
		}

		if (buffer[newStart] != quote || buffer[newStart + newLength - 1] != quote || newLength == 1 && buffer[newStart] == quote)
		{
			FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180Field", 1008);
			// If the field doesn't have quotes on the ends, or the field is a single quote char, it's bad data.
			return ProcessRFC4180BadField(start, length);
		}

		// Remove the quotes from the ends.
		newStart += 1;
		newLength -= 2;

		if ((trimOptions & TrimOptions.InsideQuotes) == TrimOptions.InsideQuotes)
		{
			FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180Field", 1019);
			ArrayHelper.Trim(buffer, ref newStart, ref newLength, whiteSpaceChars);
		}

		if (quoteCount == 2)
		{
			FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180Field", 1025);
			// The only quotes are the ends of the field.
			// No more processing is needed.
			return new ProcessedField(newStart, newLength, buffer);
		}

		if (newLength > processFieldBuffer.Length)
		{
			FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180Field", 1033);
			// Make sure the field processing buffer is large engough.
			while (newLength > processFieldBufferSize)
			{
				FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180Field", 1037);
				processFieldBufferSize *= 2;
			}

			processFieldBuffer = new char[processFieldBufferSize];
		}

		// Remove escapes.
		var inEscape = false;
		var position = 0;
		for (var i = newStart; i < newStart + newLength; i++)
		{
			FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180Field", 1049);
			var c = buffer[i];

			if (inEscape)
			{
				FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180Field", 1054);
				inEscape = false;
			}
			else if (c == escape)
			{
				FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180Field", 1059);
				inEscape = true;

				continue;
			}

			processFieldBuffer[position] = c;
			position++;
		}

		FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180Field", 1069);
		return new ProcessedField(0, position, processFieldBuffer);
	}

	/// <summary>
	/// Processes a field that does not comply with RFC4180.
	/// </summary>
	/// <param name="start">The start index of the field.</param>
	/// <param name="length">The length of the field.</param>
	/// <returns>The processed field.</returns>
	protected ProcessedField ProcessRFC4180BadField(int start, int length)
	{
		FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180BadField", 1081);
		// If field is already known to be bad, different rules can be applied.

		var args = new BadDataFoundArgs(new string(buffer, start, length), RawRecord, Context);
		badDataFound?.Invoke(args);

		var newStart = start;
		var newLength = length;

		if ((trimOptions & TrimOptions.Trim) == TrimOptions.Trim)
		{
			FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180BadField", 1092);
			ArrayHelper.Trim(buffer, ref newStart, ref newLength, whiteSpaceChars);
		}

		if (buffer[newStart] != quote)
		{
			FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180BadField", 1098);
			// If the field doesn't start with a quote, don't process it.
			return new ProcessedField(newStart, newLength, buffer);
		}

		if (newLength > processFieldBuffer.Length)
		{
			FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180BadField", 1105);
			// Make sure the field processing buffer is large engough.
			while (newLength > processFieldBufferSize)
			{
				FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180BadField", 1109);
				processFieldBufferSize *= 2;
			}

			processFieldBuffer = new char[processFieldBufferSize];
		}

		// Remove escapes until the last quote is found.
		var inEscape = false;
		var position = 0;
		var c = '\0';
		var doneProcessing = false;
		for (var i = newStart + 1; i < newStart + newLength; i++)
		{
			FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180BadField", 1123);
			var cPrev = c;
			c = buffer[i];

			// a,"b",c
			// a,"b "" c",d
			// a,"b "c d",e

			if (inEscape)
			{
				FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180BadField", 1133);
				inEscape = false;

				if (c == quote)
				{
					FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180BadField", 1138);
					// Ignore the quote after an escape.
					continue;
				}
				else if (cPrev == quote)
				{
					FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180BadField", 1144);
					// The escape and quote are the same character.
					// This is the end of the field.
					// Don't process escapes for the rest of the field.
					doneProcessing = true;
				}
			}

			if (c == escape && !doneProcessing)
			{
				FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180BadField", 1154);
				inEscape = true;

				continue;
			}

			processFieldBuffer[position] = c;
			position++;
		}

		FuzzingLogsCollector.Log("CsvParser", "ProcessRFC4180BadField", 1164);
		return new ProcessedField(0, position, processFieldBuffer);
	}

	/// <summary>
	/// Processes an escaped field.
	/// </summary>
	/// <param name="start">The start index of the field.</param>
	/// <param name="length">The length of the field.</param>
	/// <returns>The processed field.</returns>
	protected ProcessedField ProcessEscapeField(int start, int length)
	{
		FuzzingLogsCollector.Log("CsvParser", "ProcessEscapeField", 1176);
		var newStart = start;
		var newLength = length;

		if ((trimOptions & TrimOptions.Trim) == TrimOptions.Trim)
		{
			FuzzingLogsCollector.Log("CsvParser", "ProcessEscapeField", 1182);
			ArrayHelper.Trim(buffer, ref newStart, ref newLength, whiteSpaceChars);
		}

		if (newLength > processFieldBuffer.Length)
		{
			FuzzingLogsCollector.Log("CsvParser", "ProcessEscapeField", 1188);
			// Make sure the field processing buffer is large engough.
			while (newLength > processFieldBufferSize)
			{
				FuzzingLogsCollector.Log("CsvParser", "ProcessEscapeField", 1192);
				processFieldBufferSize *= 2;
			}

			processFieldBuffer = new char[processFieldBufferSize];
		}

		FuzzingLogsCollector.Log("CsvParser", "ProcessEscapeField", 1199);
		// Remove escapes.
		var inEscape = false;
		var position = 0;
		for (var i = newStart; i < newStart + newLength; i++)
		{
			FuzzingLogsCollector.Log("CsvParser", "ProcessEscapeField", 1205);
			var c = buffer[i];

			if (inEscape)
			{
				FuzzingLogsCollector.Log("CsvParser", "ProcessEscapeField", 1210);
				inEscape = false;
			}
			else if (c == escape)
			{
				FuzzingLogsCollector.Log("CsvParser", "ProcessEscapeField", 1215);
				inEscape = true;
				continue;
			}

			processFieldBuffer[position] = c;
			position++;
		}

		FuzzingLogsCollector.Log("CsvParser", "ProcessEscapeField", 1224);
		return new ProcessedField(0, position, processFieldBuffer);
	}

	/// <inheritdoc/>
	/// <summary>
	/// Processes an non-escaped field.
	/// </summary>
	/// <param name="start">The start index of the field.</param>
	/// <param name="length">The length of the field.</param>
	/// <returns>The processed field.</returns>
	protected ProcessedField ProcessNoEscapeField(int start, int length)
	{
		FuzzingLogsCollector.Log("CsvParser", "ProcessNoEscapeField", 1237);
		var newStart = start;
		var newLength = length;

		if ((trimOptions & TrimOptions.Trim) == TrimOptions.Trim)
		{
			FuzzingLogsCollector.Log("CsvParser", "ProcessNoEscapeField", 1243);
			ArrayHelper.Trim(buffer, ref newStart, ref newLength, whiteSpaceChars);
		}

		FuzzingLogsCollector.Log("CsvParser", "ProcessNoEscapeField", 1247);
		return new ProcessedField(newStart, newLength, buffer);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		FuzzingLogsCollector.Log("CsvParser", "Dispose", 1254);
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Disposes the object.
	/// </summary>
	/// <param name="disposing">Indicates if the object is being disposed.</param>
	protected virtual void Dispose(bool disposing)
	{
		FuzzingLogsCollector.Log("CsvParser", "Dispose", 1266);
		if (disposed)
		{
			FuzzingLogsCollector.Log("CsvParser", "Dispose", 1269);
			return;
		}

		if (disposing)
		{
			FuzzingLogsCollector.Log("CsvParser", "Dispose", 1275);
			// Dispose managed state (managed objects)

			if (!leaveOpen)
			{
				FuzzingLogsCollector.Log("CsvParser", "Dispose", 1280);
				reader?.Dispose();
			}
		}

		// Free unmanaged resources (unmanaged objects) and override finalizer
		// Set large fields to null

		FuzzingLogsCollector.Log("CsvParser", "Dispose", 1288);
		disposed = true;
	}

	/// <summary>
	/// Processes a raw field based on configuration.
	/// This will remove quotes, remove escapes, and trim if configured to.
	/// </summary>
	[DebuggerDisplay("Start = {Start}, Length = {Length}, Buffer.Length = {Buffer.Length}")]
	protected readonly struct ProcessedField
	{
		/// <summary>
		/// The start of the field in the buffer.
		/// </summary>
		public readonly int Start;

		/// <summary>
		/// The length of the field in the buffer.
		/// </summary>
		public readonly int Length;

		/// <summary>
		/// The buffer that contains the field.
		/// </summary>
		public readonly char[] Buffer;

		/// <summary>
		/// Creates a new instance of ProcessedField.
		/// </summary>
		/// <param name="start">The start of the field in the buffer.</param>
		/// <param name="length">The length of the field in the buffer.</param>
		/// <param name="buffer">The buffer that contains the field.</param>
		public ProcessedField(int start, int length, char[] buffer)
		{
			FuzzingLogsCollector.Log("ProcessedField", "ProcessedField", 1322);
			Start = start;
			Length = length;
			Buffer = buffer;
		}
	}

	private enum ReadLineResult
	{
		None = 0,
		Complete,
		Incomplete,
	}

	private enum ParserState
	{
		None = 0,
		Spaces,
		BlankLine,
		Delimiter,
		LineEnding,
		NewLine,
	}

	[DebuggerDisplay("Start = {Start}, Length = {Length}, QuoteCount = {QuoteCount}, IsBad = {IsBad}")]
	private struct Field
	{
		/// <summary>
		/// Starting position of the field.
		/// This is an offset from <see cref="rowStartPosition"/>.
		/// </summary>
		public int Start;

		public int Length;

		public int QuoteCount;

		public bool IsBad;

		public bool IsProcessed;
	}
}
