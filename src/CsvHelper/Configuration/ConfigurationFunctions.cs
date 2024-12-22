// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Delegates;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.Configuration;

/// <summary>Holds the default callback methods for delegate members of <c>CsvHelper.Configuration.Configuration</c>.</summary>
public static class ConfigurationFunctions
{
	private static readonly char[] lineEndingChars = new char[] { '\r', '\n' };

	/// <summary>
	/// Throws a <see cref="ValidationException"/> if <see name="HeaderValidatedArgs.InvalidHeaders"/> is not empty.
	/// </summary>
	public static void HeaderValidated(HeaderValidatedArgs args)
	{
		FuzzingLogsCollector.Log("ConfigurationFunctions", "HeaderValidated", 23);
		if (args.InvalidHeaders.Count() == 0)
		{
			FuzzingLogsCollector.Log("ConfigurationFunctions", "HeaderValidated", 26);
			return;
		}

		var errorMessage = new StringBuilder();
		foreach (var invalidHeader in args.InvalidHeaders)
		{
			FuzzingLogsCollector.Log("ConfigurationFunctions", "HeaderValidated", 33);
			errorMessage.AppendLine($"Header with name '{string.Join("' or '", invalidHeader.Names)}'[{invalidHeader.Index}] was not found.");
		}

		if (args.Context.Reader?.HeaderRecord != null)
		{
			FuzzingLogsCollector.Log("ConfigurationFunctions", "HeaderValidated", 39);
			errorMessage.AppendLine($"Headers: '{string.Join("', '", args.Context.Reader.HeaderRecord)}'");
		}

		var messagePostfix =
			$"If you are expecting some headers to be missing and want to ignore this validation, " +
			$"set the configuration {nameof(HeaderValidated)} to null. You can also change the " +
			$"functionality to do something else, like logging the issue.";
		errorMessage.AppendLine(messagePostfix);

		throw new HeaderValidationException(args.Context, args.InvalidHeaders, errorMessage.ToString());
	}

	/// <summary>
	/// Throws a <c>MissingFieldException</c>.
	/// </summary>
	public static void MissingFieldFound(MissingFieldFoundArgs args)
	{
		FuzzingLogsCollector.Log("ConfigurationFunctions", "MissingFieldFound", 57);
		var messagePostfix = $"You can ignore missing fields by setting {nameof(MissingFieldFound)} to null.";

		// Get by index.

		if (args.HeaderNames == null || args.HeaderNames.Length == 0)
		{
			FuzzingLogsCollector.Log("ConfigurationFunctions", "MissingFieldFound", 64);
			throw new MissingFieldException(args.Context, $"Field at index '{args.Index}' does not exist. {messagePostfix}");
		}

		// Get by name.

		var indexText = args.Index > 0 ? $" at field index '{args.Index}'" : string.Empty;

		if (args.HeaderNames.Length == 1)
		{
			FuzzingLogsCollector.Log("ConfigurationFunctions", "MissingFieldFound", 74);
			throw new MissingFieldException(args.Context, $"Field with name '{args.HeaderNames[0]}'{indexText} does not exist. {messagePostfix}");
		}

		throw new MissingFieldException(args.Context, $"Field containing names '{string.Join("' or '", args.HeaderNames)}'{indexText} does not exist. {messagePostfix}");
	}

	/// <summary>
	/// Throws a <see cref="BadDataException"/>.
	/// </summary>
	public static void BadDataFound(BadDataFoundArgs args)
	{
		FuzzingLogsCollector.Log("ConfigurationFunctions", "BadDataFound", 86);
		throw new BadDataException(args.Field, args.RawRecord, args.Context, $"You can ignore bad data by setting {nameof(BadDataFound)} to null.");
	}

	/// <summary>
	/// Throws the given <see name="ReadingExceptionOccurredArgs.Exception"/>.
	/// </summary>
	public static bool ReadingExceptionOccurred(ReadingExceptionOccurredArgs args)
	{
		FuzzingLogsCollector.Log("ConfigurationFunctions", "ReadingExceptionOccurred", 95);
		return true;
	}

	/// <summary>
	/// Returns true if the field contains a <see cref="IWriterConfiguration.Quote"/>,
	/// starts with a space, ends with a space, contains \r or \n, or contains
	/// the <see cref="IWriterConfiguration.Delimiter"/>.
	/// </summary>
	/// <param name="args">The args.</param>
	/// <returns><c>true</c> if the field should be quoted, otherwise <c>false</c>.</returns>
	public static bool ShouldQuote(ShouldQuoteArgs args)
	{
		FuzzingLogsCollector.Log("ConfigurationFunctions", "ShouldQuote", 108);
		var config = args.Row.Configuration;
		var field = args.Field;

		if (field == null || field.Length == 0)
		{
			FuzzingLogsCollector.Log("ConfigurationFunctions", "ShouldQuote", 114);
			return false;
		}

		var shouldQuote =
		(
			field[0] == ' ' // Starts with a space
			|| field[field.Length - 1] == ' ' // Ends with a space
			|| field.Contains(config.Quote) // Contains quote
			|| !config.IsNewLineSet && field.IndexOfAny(lineEndingChars) > -1 // Contains line ending characters
			|| config.IsNewLineSet && field.Contains(config.NewLine) // Contains newline
			|| (config.Delimiter.Length > 0 && field.Contains(config.Delimiter)) // Contains delimiter
		);

		return shouldQuote;
	}

	/// <summary>
	/// Returns the <see name="PrepareHeaderForMatchArgs.Header"/> as given.
	/// </summary>
	public static string PrepareHeaderForMatch(PrepareHeaderForMatchArgs args)
	{
		FuzzingLogsCollector.Log("ConfigurationFunctions", "PrepareHeaderForMatch", 108);
		return args.Header ?? string.Empty;
	}

	/// <summary>
	/// Returns <c>true</c> if <paramref name="args.ParameterType"/>:
	/// 1. does not have a parameterless constructor
	/// 2. has a constructor
	/// 3. is not a value type
	/// 4. is not a primitive
	/// 5. is not an enum
	/// 6. is not an interface
	/// 7. TypeCode is an Object.
	/// </summary>
	public static bool ShouldUseConstructorParameters(ShouldUseConstructorParametersArgs args)
	{
		FuzzingLogsCollector.Log("ConfigurationFunctions", "ShouldUseConstructorParameters", 152);
		return !args.ParameterType.HasParameterlessConstructor()
			&& args.ParameterType.HasConstructor()
			&& !args.ParameterType.IsValueType
			&& !args.ParameterType.IsPrimitive
			&& !args.ParameterType.IsEnum
			&& !args.ParameterType.IsInterface
			&& Type.GetTypeCode(args.ParameterType) == TypeCode.Object;
	}

	/// <summary>
	/// Returns the type's constructor with the most parameters.
	/// If two constructors have the same number of parameters, then
	/// there is no guarantee which one will be returned. If you have
	/// that situation, you should probably implement this function yourself.
	/// </summary>
	public static ConstructorInfo GetConstructor(GetConstructorArgs args)
	{
		FuzzingLogsCollector.Log("ConfigurationFunctions", "GetConstructor", 170);
		return args.ClassType.GetConstructorWithMostParameters();
	}

	/// <summary>
	/// Returns the header name ran through <see cref="PrepareHeaderForMatch(PrepareHeaderForMatchArgs)"/>.
	/// If no header exists, property names will be Field1, Field2, Field3, etc.
	/// </summary>
	/// <param name="args">The args.</param>
	public static string GetDynamicPropertyName(GetDynamicPropertyNameArgs args)
	{
		FuzzingLogsCollector.Log("ConfigurationFunctions", "GetDynamicPropertyName", 181);
		if (args.Context.Reader?.HeaderRecord == null)
		{
			FuzzingLogsCollector.Log("ConfigurationFunctions", "GetDynamicPropertyName", 184);
			return $"Field{args.FieldIndex + 1}";
		}

		var header = args.Context.Reader.HeaderRecord[args.FieldIndex];
		var prepareHeaderForMatchArgs = new PrepareHeaderForMatchArgs(header, args.FieldIndex);
		header = args.Context.Reader.Configuration.PrepareHeaderForMatch(prepareHeaderForMatchArgs);

		return header;
	}

	/// <summary>
	/// Detects the delimiter based on the given text.
	/// Return the detected delimiter or null if one wasn't found.
	/// </summary>
	/// <param name="args">The args.</param>
	public static string GetDelimiter(GetDelimiterArgs args)
	{
		FuzzingLogsCollector.Log("ConfigurationFunctions", "GetDelimiter", 202);
		var text = args.Text;
		var config = args.Configuration;

		if (config.Mode == CsvMode.RFC4180)
		{
			FuzzingLogsCollector.Log("ConfigurationFunctions", "GetDelimiter", 208);
			// Remove text in between pairs of quotes.
			text = Regex.Replace(text, $"{config.Quote}.*?{config.Quote}", string.Empty, RegexOptions.Singleline);
		}
		else if (config.Mode == CsvMode.Escape)
		{
			FuzzingLogsCollector.Log("ConfigurationFunctions", "GetDelimiter", 214);
			// Remove escaped characters.
			text = Regex.Replace(text, $"({config.Escape}.)", string.Empty, RegexOptions.Singleline);
		}

		var newLine = config.NewLine;
		if ((new[] { "\r\n", "\r", "\n" }).Contains(newLine))
		{
			FuzzingLogsCollector.Log("ConfigurationFunctions", "GetDelimiter", 222);
			newLine = "\r\n|\r|\n";
		}

		var lineDelimiterCounts = new List<Dictionary<string, int>>();
		while (text.Length > 0)
		{
			FuzzingLogsCollector.Log("ConfigurationFunctions", "GetDelimiter", 229);
			// Since all escaped text has been removed, we can reliably read line by line.
			var match = Regex.Match(text, newLine);
			var line = match.Success ? text.Substring(0, match.Index) : text;

			if (line.Length > 0)
			{
				FuzzingLogsCollector.Log("ConfigurationFunctions", "GetDelimiter", 236);
				var delimiterCounts = new Dictionary<string, int>();
				foreach (var delimiter in config.DetectDelimiterValues)
				{
					FuzzingLogsCollector.Log("ConfigurationFunctions", "GetDelimiter", 240);
					// Escape regex special chars to use as regex pattern.
					var pattern = Regex.Replace(delimiter, @"([.$^{\[(|)*+?\\])", "\\$1");
					delimiterCounts[delimiter] = Regex.Matches(line, pattern).Count;
				}
				FuzzingLogsCollector.Log("ConfigurationFunctions", "GetDelimiter", 245);
				lineDelimiterCounts.Add(delimiterCounts);
			}

			FuzzingLogsCollector.Log("ConfigurationFunctions", "GetDelimiter", 249);
			text = match.Success ? text.Substring(match.Index + match.Length) : string.Empty;
		}

		if (lineDelimiterCounts.Count > 1)
		{
			FuzzingLogsCollector.Log("ConfigurationFunctions", "GetDelimiter", 255);
			// The last line isn't complete and can't be used to reliably detect a delimiter.
			lineDelimiterCounts.Remove(lineDelimiterCounts.Last());
		}

		FuzzingLogsCollector.Log("ConfigurationFunctions", "GetDelimiter", 260);
		// Rank only the delimiters that appear on every line.
		var delimiters =
		(
			from counts in lineDelimiterCounts
			from count in counts
			group count by count.Key into g
			where g.All(x => x.Value > 0)
			let sum = g.Sum(x => x.Value)
			orderby sum descending
			select new
			{
				Delimiter = g.Key,
				Count = sum
			}
		).ToList();

		FuzzingLogsCollector.Log("ConfigurationFunctions", "GetDelimiter", 277);
		string? newDelimiter = null;
		if (delimiters.Any(x => x.Delimiter == config.CultureInfo.TextInfo.ListSeparator) && lineDelimiterCounts.Count > 1)
		{
			FuzzingLogsCollector.Log("ConfigurationFunctions", "GetDelimiter", 281);
			// The culture's separator is on every line. Assume this is the delimiter.
			newDelimiter = config.CultureInfo.TextInfo.ListSeparator;
		}
		else
		{
			FuzzingLogsCollector.Log("ConfigurationFunctions", "GetDelimiter", 287);
			// Choose the highest ranked delimiter.
			newDelimiter = delimiters.Select(x => x.Delimiter).FirstOrDefault();
		}

		if (newDelimiter != null)
		{
			FuzzingLogsCollector.Log("ConfigurationFunctions", "GetDelimiter", 294);
			config.Validate();
		}

		FuzzingLogsCollector.Log("ConfigurationFunctions", "GetDelimiter", 298);
		return newDelimiter ?? config.Delimiter;
	}
}
