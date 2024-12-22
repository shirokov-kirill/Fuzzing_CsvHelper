// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.TypeConversion;
using System.Diagnostics;
using System.Reflection;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.Configuration;

/// <summary>
/// Mapping for a constructor parameter.
/// This may contain value type data, a constructor type map,
/// or a reference map, depending on the type of the parameter.
/// </summary>
[DebuggerDisplay("Data = {Data}")]
public class ParameterMap
{
	/// <summary>
	/// Gets the parameter map data.
	/// </summary>
	public virtual ParameterMapData Data { get; protected set; }

	/// <summary>
	/// Type converter options.
	/// </summary>
	public virtual ParameterMapTypeConverterOption TypeConverterOption { get; protected set; }

	/// <summary>
	/// Gets or sets the map for a constructor type.
	/// </summary>
	public virtual ClassMap? ConstructorTypeMap { get; set; }

	/// <summary>
	/// Gets or sets the map for a reference type.
	/// </summary>
	public virtual ParameterReferenceMap? ReferenceMap { get; set; }

	/// <summary>
	/// Creates an instance of <see cref="ParameterMap"/> using
	/// the given information.
	/// </summary>
	/// <param name="parameter">The parameter being mapped.</param>
	public ParameterMap(ParameterInfo parameter)
	{
		TypeConverterOption = new ParameterMapTypeConverterOption(this);

		Data = new ParameterMapData(parameter);
	}

	/// <summary>
	/// When reading, is used to get the field
	/// at the index of the name if there was a
	/// header specified. It will look for the
	/// first name match in the order listed.
	/// When writing, sets the name of the
	/// field in the header record.
	/// The first name will be used.
	/// </summary>
	/// <param name="names">The possible names of the CSV field.</param>
	public virtual ParameterMap Name(params string[] names)
	{
		FuzzingLogsCollector.Log("ParameterMap", "Name", 64);
		if (names == null || names.Length == 0)
		{
			FuzzingLogsCollector.Log("ParameterMap", "Name", 67);
			throw new ArgumentNullException(nameof(names));
		}

		Data.Names.Clear();
		Data.Names.AddRange(names);
		Data.IsNameSet = true;

		FuzzingLogsCollector.Log("ParameterMap", "Name", 75);
		return this;
	}

	/// <summary>
	/// When reading, is used to get the
	/// index of the name used when there
	/// are multiple names that are the same.
	/// </summary>
	/// <param name="index">The index of the name.</param>
	public virtual ParameterMap NameIndex(int index)
	{
		FuzzingLogsCollector.Log("ParameterMap", "NameIndex", 87);
		Data.NameIndex = index;

		return this;
	}

	/// <summary>
	/// When reading, is used to get the field at
	/// the given index. When writing, the fields
	/// will be written in the order of the field
	/// indexes.
	/// </summary>
	/// <param name="index">The index of the CSV field.</param>
	public virtual ParameterMap Index(int index)
	{
		FuzzingLogsCollector.Log("ParameterMap", "Index", 102);
		Data.Index = index;
		Data.IsIndexSet = true;

		return this;
	}

	/// <summary>
	/// Ignore the parameter when reading and writing.
	/// </summary>
	public virtual ParameterMap Ignore()
	{
		FuzzingLogsCollector.Log("ParameterMap", "Ignore", 114);
		Data.Ignore = true;

		return this;
	}

	/// <summary>
	/// Ignore the parameter when reading and writing.
	/// </summary>
	/// <param name="ignore">True to ignore, otherwise false.</param>
	public virtual ParameterMap Ignore(bool ignore)
	{
		FuzzingLogsCollector.Log("ParameterMap", "Ignore", 126);
		Data.Ignore = ignore;

		return this;
	}

	/// <summary>
	/// The default value that will be used when reading when
	/// the CSV field is empty.
	/// </summary>
	/// <param name="defaultValue">The default value.</param>
	public virtual ParameterMap Default(object defaultValue)
	{
		FuzzingLogsCollector.Log("ParameterMap", "Default", 139);
		if (defaultValue == null && Data.Parameter.ParameterType.IsValueType)
		{
			FuzzingLogsCollector.Log("ParameterMap", "Default", 142);
			throw new ArgumentException($"Parameter of type '{Data.Parameter.ParameterType.FullName}' can't have a default value of null.");
		}

		if (defaultValue != null && defaultValue.GetType() != Data.Parameter.ParameterType)
		{
			FuzzingLogsCollector.Log("ParameterMap", "Default", 148);
			throw new ArgumentException($"Default of type '{defaultValue.GetType().FullName}' does not match parameter of type '{Data.Parameter.ParameterType.FullName}'.");
		}

		Data.Default = defaultValue;
		Data.IsDefaultSet = true;

		FuzzingLogsCollector.Log("ParameterMap", "Default", 155);
		return this;
	}

	/// <summary>
	/// The constant value that will be used for every record when
	/// reading and writing. This value will always be used no matter
	/// what other mapping configurations are specified.
	/// </summary>
	/// <param name="constantValue">The constant value.</param>
	public virtual ParameterMap Constant(object constantValue)
	{
		FuzzingLogsCollector.Log("ParameterMap", "Constant", 167);
		if (constantValue == null && Data.Parameter.ParameterType.IsValueType)
		{
			FuzzingLogsCollector.Log("ParameterMap", "Constant", 170);
			throw new ArgumentException($"Parameter of type '{Data.Parameter.ParameterType.FullName}' can't have a constant value of null.");
		}

		if (constantValue != null && constantValue.GetType() != Data.Parameter.ParameterType)
		{
			FuzzingLogsCollector.Log("ParameterMap", "Constant", 176);
			throw new ArgumentException($"Constant of type '{constantValue.GetType().FullName}' does not match parameter of type '{Data.Parameter.ParameterType.FullName}'.");
		}

		Data.Constant = constantValue;
		Data.IsConstantSet = true;

		FuzzingLogsCollector.Log("ParameterMap", "Constant", 183);
		return this;
	}

	/// <summary>
	/// The field is optional.
	/// </summary>
	public virtual ParameterMap Optional()
	{
		FuzzingLogsCollector.Log("ParameterMap", "Optional", 192);
		Data.IsOptional = true;

		return this;
	}

	/// <summary>
	/// Specifies the <see cref="TypeConverter"/> to use
	/// when converting the parameter to and from a CSV field.
	/// </summary>
	/// <param name="typeConverter">The TypeConverter to use.</param>
	public virtual ParameterMap TypeConverter(ITypeConverter typeConverter)
	{
		FuzzingLogsCollector.Log("ParameterMap", "TypeConverter", 205);
		Data.TypeConverter = typeConverter;

		return this;
	}

	/// <summary>
	/// Specifies the <see cref="TypeConverter"/> to use
	/// when converting the parameter to and from a CSV field.
	/// </summary>
	/// <typeparam name="TConverter">The <see cref="System.Type"/> of the
	/// <see cref="TypeConverter"/> to use.</typeparam>
	public virtual ParameterMap TypeConverter<TConverter>() where TConverter : ITypeConverter
	{
		FuzzingLogsCollector.Log("ParameterMap", "TypeConverter<TConverter>", 219);
		TypeConverter(ObjectResolver.Current.Resolve<TConverter>());

		return this;
	}

	internal int GetMaxIndex()
	{
		FuzzingLogsCollector.Log("ParameterMap", "GetMaxIndex", 205);
		return ReferenceMap?.GetMaxIndex() ?? Data.Index;
	}
}
