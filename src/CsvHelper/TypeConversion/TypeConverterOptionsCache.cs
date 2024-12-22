// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper

using CsvHelper.FuzzingLogger;

namespace CsvHelper.TypeConversion;

/// <summary>
/// Caches <see cref="TypeConverterOptions"/> for a given type.
/// </summary>
public class TypeConverterOptionsCache
{
	private Dictionary<Type, TypeConverterOptions> typeConverterOptions = new Dictionary<Type, TypeConverterOptions>();

	/// <summary>
	/// Adds the <see cref="TypeConverterOptions"/> for the given <see cref="Type"/>.
	/// </summary>
	/// <param name="type">The type the options are for.</param>
	/// <param name="options">The options.</param>
	public void AddOptions(Type type, TypeConverterOptions options)
	{
		FuzzingLogsCollector.Log("TypeConverterOptionsCache", "AddOptions", 21);
		if (type == null)
		{
			FuzzingLogsCollector.Log("TypeConverterOptionsCache", "AddOptions", 27);
			throw new ArgumentNullException(nameof(type));
		}

		FuzzingLogsCollector.Log("TypeConverterOptionsCache", "AddOptions", 31);
		typeConverterOptions[type] = options ?? throw new ArgumentNullException(nameof(options));
	}

	/// <summary>
	/// Adds the <see cref="TypeConverterOptions"/> for the given <see cref="Type"/>.
	/// </summary>
	/// <typeparam name="T">The type the options are for.</typeparam>
	/// <param name="options">The options.</param>
	public void AddOptions<T>(TypeConverterOptions options)
	{
		FuzzingLogsCollector.Log("TypeConverterOptionsCache", "AddOptions<T>", 42);
		AddOptions(typeof(T), options);
	}

	/// <summary>
	/// Adds the given <see cref="TypeConverterOptions"/> to all registered types.
	/// </summary>
	/// <param name="options"></param>
	public void AddOptions(TypeConverterOptions options)
	{
		FuzzingLogsCollector.Log("TypeConverterOptionsCache", "AddOptions", 52);
		foreach (var type in typeConverterOptions.Keys)
		{
			FuzzingLogsCollector.Log("TypeConverterOptionsCache", "AddOptions", 55);
			typeConverterOptions[type] = options;
		}
	}

	/// <summary>
	/// Removes the <see cref="TypeConverterOptions"/> for the given type.
	/// </summary>
	/// <param name="type">The type to remove the options for.</param>
	public void RemoveOptions(Type type)
	{
		FuzzingLogsCollector.Log("TypeConverterOptionsCache", "RemoveOptions", 66);
		if (type == null)
		{
			FuzzingLogsCollector.Log("TypeConverterOptionsCache", "RemoveOptions", 69);
			throw new ArgumentNullException(nameof(type));
		}

		FuzzingLogsCollector.Log("TypeConverterOptionsCache", "RemoveOptions", 73);
		typeConverterOptions.Remove(type);
	}

	/// <summary>
	/// Removes the <see cref="TypeConverterOptions"/> for the given type.
	/// </summary>
	/// <typeparam name="T">The type to remove the options for.</typeparam>
	public void RemoveOptions<T>()
	{
		FuzzingLogsCollector.Log("TypeConverterOptionsCache", "RemoveOptions<T>", 83);
		RemoveOptions(typeof(T));
	}

	/// <summary>
	/// Get the <see cref="TypeConverterOptions"/> for the given <see cref="Type"/>.
	/// </summary>
	/// <param name="type">The type the options are for.</param>
	/// <returns>The options for the given type.</returns>
	public TypeConverterOptions GetOptions(Type type)
	{
		FuzzingLogsCollector.Log("TypeConverterOptionsCache", "GetOptions", 94);
		if (type == null)
		{
			FuzzingLogsCollector.Log("TypeConverterOptionsCache", "GetOptions", 97);
			throw new ArgumentNullException();
		}

		if (!typeConverterOptions.TryGetValue(type, out var options))
		{
			FuzzingLogsCollector.Log("TypeConverterOptionsCache", "GetOptions", 103);
			options = new TypeConverterOptions();
			typeConverterOptions.Add(type, options);
		}

		FuzzingLogsCollector.Log("TypeConverterOptionsCache", "GetOptions", 108);
		return options;
	}

	/// <summary>
	/// Get the <see cref="TypeConverterOptions"/> for the given <see cref="Type"/>.
	/// </summary>
	/// <typeparam name="T">The type the options are for.</typeparam>
	/// <returns>The options for the given type.</returns>
	public TypeConverterOptions GetOptions<T>()
	{
		FuzzingLogsCollector.Log("TypeConverterOptionsCache", "GetOptions<T>", 119);
		return GetOptions(typeof(T));
	}
}
