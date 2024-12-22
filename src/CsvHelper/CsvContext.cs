// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration;
using CsvHelper.FuzzingLogger;
using CsvHelper.TypeConversion;

namespace CsvHelper;

/// <summary>
/// Share state for CsvHelper.
/// </summary>
public class CsvContext
{
	/// <summary>
	/// Gets or sets the <see cref="TypeConverterOptionsCache"/>.
	/// </summary>
	public virtual TypeConverterOptionsCache TypeConverterOptionsCache { get; set; } = new TypeConverterOptionsCache();

	/// <summary>
	/// Gets or sets the <see cref="TypeConverterOptionsCache"/>.
	/// </summary>
	public virtual TypeConverterCache TypeConverterCache { get; set; } = new TypeConverterCache();

	/// <summary>
	/// The configured <see cref="ClassMap"/>s.
	/// </summary>
	public virtual ClassMapCollection Maps { get; private set; }

	/// <summary>
	/// Gets the parser.
	/// </summary>
	public IParser? Parser { get; private set; }

	/// <summary>
	/// Gets the reader.
	/// </summary>
	public IReader? Reader { get; internal set; }

	/// <summary>
	/// Gets the writer.
	/// </summary>
	public IWriter? Writer { get; internal set; }

	/// <summary>
	/// Gets the configuration.
	/// </summary>
	public CsvConfiguration Configuration { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="CsvContext"/> class.
	/// </summary>
	/// <param name="reader">The reader.</param>
	public CsvContext(IReader reader)
	{
		FuzzingLogsCollector.Log("CsvContext", "CsvContext", 56);
		Reader = reader;
		Parser = reader.Parser;
		Configuration = reader.Configuration as CsvConfiguration ?? throw new InvalidOperationException($"{nameof(IReader)}.{nameof(IReader.Configuration)} must be of type {nameof(CsvConfiguration)} to be used in the context.");
		Maps = new ClassMapCollection(this);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CsvContext"/> class.
	/// </summary>
	/// <param name="parser">The parser.</param>
	public CsvContext(IParser parser)
	{
		FuzzingLogsCollector.Log("CsvContext", "CsvContext", 70);
		Parser = parser;
		Configuration = parser.Configuration as CsvConfiguration ?? throw new InvalidOperationException($"{nameof(IParser)}.{nameof(IParser.Configuration)} must be of type {nameof(CsvConfiguration)} to be used in the context.");
		Maps = new ClassMapCollection(this);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CsvContext"/> class.
	/// </summary>
	/// <param name="writer">The writer.</param>
	public CsvContext(IWriter writer)
	{
		FuzzingLogsCollector.Log("CsvContext", "CsvContext", 82);
		Writer = writer;
		Configuration = writer.Configuration as CsvConfiguration ?? throw new InvalidOperationException($"{nameof(IWriter)}.{nameof(IWriter.Configuration)} must be of type {nameof(CsvConfiguration)} to be used in the context.");
		Maps = new ClassMapCollection(this);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CsvContext"/> class.
	/// </summary>
	/// <param name="configuration">The configuration.</param>
	public CsvContext(CsvConfiguration configuration)
	{
		FuzzingLogsCollector.Log("CsvContext", "CsvContext", 94);
		Configuration = configuration;
		Maps = new ClassMapCollection(this);
	}

	/// <summary>
	/// Use a <see cref="ClassMap{T}" /> to configure mappings.
	/// When using a class map, no members are mapped by default.
	/// Only member specified in the mapping are used.
	/// </summary>
	/// <typeparam name="TMap">The type of mapping class to use.</typeparam>
	public virtual TMap RegisterClassMap<TMap>() where TMap : ClassMap
	{
		FuzzingLogsCollector.Log("CsvContext", "RegisterClassMap", 107);
		var map = ObjectResolver.Current.Resolve<TMap>();
		RegisterClassMap(map);

		return map;
	}

	/// <summary>
	/// Use a <see cref="ClassMap{T}" /> to configure mappings.
	/// When using a class map, no members are mapped by default.
	/// Only members specified in the mapping are used.
	/// </summary>
	/// <param name="classMapType">The type of mapping class to use.</param>
	public virtual ClassMap RegisterClassMap(Type classMapType)
	{
		FuzzingLogsCollector.Log("CsvContext", "RegisterClassMap", 122);
		if (!typeof(ClassMap).IsAssignableFrom(classMapType))
		{
			FuzzingLogsCollector.Log("CsvContext", "RegisterClassMap", 125);
			throw new ArgumentException("The class map type must inherit from CsvClassMap.");
		}

		var map = (ClassMap)ObjectResolver.Current.Resolve(classMapType);
		RegisterClassMap(map);

		FuzzingLogsCollector.Log("CsvContext", "RegisterClassMap", 132);
		return map;
	}

	/// <summary>
	/// Registers the class map.
	/// </summary>
	/// <param name="map">The class map to register.</param>
	public virtual void RegisterClassMap(ClassMap map)
	{
		FuzzingLogsCollector.Log("CsvContext", "RegisterClassMap", 142);
		if (map.MemberMaps.Count == 0 && map.ReferenceMaps.Count == 0 && map.ParameterMaps.Count == 0)
		{
			FuzzingLogsCollector.Log("CsvContext", "RegisterClassMap", 145);
			throw new ConfigurationException("No mappings were specified in the CsvClassMap.");
		}

		Maps.Add(map);
	}

	/// <summary>
	/// Unregisters the class map.
	/// </summary>
	/// <typeparam name="TMap">The map type to unregister.</typeparam>
	public virtual void UnregisterClassMap<TMap>()
		where TMap : ClassMap
	{
		FuzzingLogsCollector.Log("CsvContext", "UnregisterClassMap<TMap>", 159);
		UnregisterClassMap(typeof(TMap));
	}

	/// <summary>
	/// Unregisters the class map.
	/// </summary>
	/// <param name="classMapType">The map type to unregister.</param>
	public virtual void UnregisterClassMap(Type classMapType)
	{
		FuzzingLogsCollector.Log("CsvContext", "UnregisterClassMap", 169);
		Maps.Remove(classMapType);
	}

	/// <summary>
	/// Unregisters all class maps.
	/// </summary>
	public virtual void UnregisterClassMap()
	{
		FuzzingLogsCollector.Log("CsvContext", "UnregisterClassMap<TMap>", 178);
		Maps.Clear();
	}

	/// <summary>
	/// Generates a <see cref="ClassMap"/> for the type.
	/// </summary>
	/// <typeparam name="T">The type to generate the map for.</typeparam>
	/// <returns>The generate map.</returns>
	public virtual ClassMap<T> AutoMap<T>()
	{
		FuzzingLogsCollector.Log("CsvContext", "AutoMap<T>", 189);
		var map = ObjectResolver.Current.Resolve<DefaultClassMap<T>>();
		map.AutoMap(this);
		Maps.Add(map);

		FuzzingLogsCollector.Log("CsvContext", "AutoMap<T>", 194);
		return map;
	}

	/// <summary>
	/// Generates a <see cref="ClassMap"/> for the type.
	/// </summary>
	/// <param name="type">The type to generate for the map.</param>
	/// <returns>The generate map.</returns>
	public virtual ClassMap AutoMap(Type type)
	{
		FuzzingLogsCollector.Log("CsvContext", "AutoMap<T>", 205);
		var mapType = typeof(DefaultClassMap<>).MakeGenericType(type);
		var map = (ClassMap)ObjectResolver.Current.Resolve(mapType);
		map.AutoMap(this);
		Maps.Add(map);

		FuzzingLogsCollector.Log("CsvContext", "AutoMap<T>", 211);
		return map;
	}
}
