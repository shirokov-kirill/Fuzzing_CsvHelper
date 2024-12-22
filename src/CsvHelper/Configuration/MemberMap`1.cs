// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.TypeConversion;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using CsvHelper.FuzzingLogger;

namespace CsvHelper.Configuration;

/// <summary>
/// Mapping info for a member to a CSV field.
/// </summary>
public class MemberMap<TClass, TMember> : MemberMap
{
	private MemberMapTypeConverterOption typeConverterOption;

	/// <inheritdoc />
	public override MemberMapTypeConverterOption TypeConverterOption => typeConverterOption;

	/// <summary>
	/// Creates a new <see cref="MemberMap"/> instance using the specified member.
	/// </summary>
	public MemberMap(MemberInfo? member)
	{
		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "MemberMap", 29);
		typeConverterOption = new MemberMapTypeConverterOption(this);

		Data = new MemberMapData(member);
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
	public new virtual MemberMap<TClass, TMember> Name(params string[] names)
	{
		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "Name", 47);
		if (names == null || names.Length == 0)
		{
			FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "Name", 50);
			throw new ArgumentNullException(nameof(names));
		}

		Data.Names.Clear();
		Data.Names.AddRange(names);
		Data.IsNameSet = true;

		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "Name", 58);
		return this;
	}

	/// <summary>
	/// When reading, is used to get the
	/// index of the name used when there
	/// are multiple names that are the same.
	/// </summary>
	/// <param name="index">The index of the name.</param>
	public new virtual MemberMap<TClass, TMember> NameIndex(int index)
	{
		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "NameIndex", 70);
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
	/// <param name="indexEnd">The end index used when mapping to an <see cref="IEnumerable"/> member.</param>
	public new virtual MemberMap<TClass, TMember> Index(int index, int indexEnd = -1)
	{
		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "Index", 86);
		Data.Index = index;
		Data.IsIndexSet = true;
		Data.IndexEnd = indexEnd;

		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "Index", 91);
		return this;
	}

	/// <summary>
	/// Ignore the member when reading and writing.
	/// If this member has already been mapped as a reference
	/// member, either by a class map, or by automapping, calling
	/// this method will not ignore all the child members down the
	/// tree that have already been mapped.
	/// </summary>
	public new virtual MemberMap<TClass, TMember> Ignore()
	{
		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "Ignore", 104);
		Data.Ignore = true;

		return this;
	}

	/// <summary>
	/// Ignore the member when reading and writing.
	/// If this member has already been mapped as a reference
	/// member, either by a class map, or by automapping, calling
	/// this method will not ignore all the child members down the
	/// tree that have already been mapped.
	/// </summary>
	/// <param name="ignore">True to ignore, otherwise false.</param>
	public new virtual MemberMap<TClass, TMember> Ignore(bool ignore)
	{
		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "Ignore", 120);
		Data.Ignore = ignore;

		return this;
	}

	/// <summary>
	/// The default value that will be used when reading when
	/// the CSV field is empty.
	/// </summary>
	/// <param name="defaultValue">The default value.</param>
	/// <param name="useOnConversionFailure">Use default on conversion failure.</param>
	public virtual MemberMap<TClass, TMember> Default(TMember defaultValue, bool useOnConversionFailure = false)
	{
		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "Default", 134);
		Data.Default = defaultValue;
		Data.IsDefaultSet = true;
		Data.UseDefaultOnConversionFailure = useOnConversionFailure;

		return this;
	}

	/// <summary>
	/// The default value that will be used when reading when
	/// the CSV field is empty. This value is not type checked
	/// and will use a <see cref="ITypeConverter"/> to convert
	/// the field. This could potentially have runtime errors.
	/// </summary>
	/// <param name="defaultValue">The default value.</param>
	/// <param name="useOnConversionFailure">Use default on conversion failure.</param>
	public virtual MemberMap<TClass, TMember> Default(string? defaultValue, bool useOnConversionFailure = false)
	{
		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "Default", 152);
		Data.Default = defaultValue;
		Data.IsDefaultSet = true;
		Data.UseDefaultOnConversionFailure = useOnConversionFailure;

		return this;
	}

	/// <summary>
	/// The constant value that will be used for every record when
	/// reading and writing. This value will always be used no matter
	/// what other mapping configurations are specified.
	/// </summary>
	/// <param name="constantValue">The constant value.</param>
	public virtual MemberMap<TClass, TMember> Constant(TMember? constantValue)
	{
		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "Constant", 168);
		Data.Constant = constantValue;
		Data.IsConstantSet = true;

		return this;
	}

	/// <summary>
	/// Specifies the <see cref="TypeConverter"/> to use
	/// when converting the member to and from a CSV field.
	/// </summary>
	/// <param name="typeConverter">The TypeConverter to use.</param>
	public new virtual MemberMap<TClass, TMember> TypeConverter(ITypeConverter typeConverter)
	{
		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "TypeConverter", 182);
		Data.TypeConverter = typeConverter;

		return this;
	}

	/// <summary>
	/// Specifies the <see cref="TypeConverter"/> to use
	/// when converting the member to and from a CSV field.
	/// </summary>
	/// <typeparam name="TConverter">The <see cref="System.Type"/> of the
	/// <see cref="TypeConverter"/> to use.</typeparam>
	public new virtual MemberMap<TClass, TMember> TypeConverter<TConverter>() where TConverter : ITypeConverter
	{
		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "TypeConverter<TConverter>", 182);
		TypeConverter(ObjectResolver.Current.Resolve<TConverter>());

		return this;
	}

	/// <summary>
	/// Specifies an expression to be used to convert data in the
	/// row to the member.
	/// </summary>
	/// <param name="convertFromStringFunction">The convert expression.</param>
	public virtual MemberMap<TClass, TMember> Convert(ConvertFromString<TMember> convertFromStringFunction)
	{
		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "Convert", 209);
		var instance = convertFromStringFunction.Target != null ? Expression.Constant(convertFromStringFunction.Target) : null;
		var fieldParameter = Expression.Parameter(typeof(ConvertFromStringArgs), "args");
		var methodExpression = Expression.Call
		(
			instance,
			convertFromStringFunction.Method,
			fieldParameter
		);
		var lambdaExpression = Expression.Lambda<ConvertFromString<TMember>>(methodExpression, fieldParameter);

		Data.ReadingConvertExpression = lambdaExpression;

		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "Convert", 222);
		return this;
	}

	/// <summary>
	/// Specifies an expression to be used to convert the object
	/// to a field.
	/// </summary>
	/// <param name="convertToStringFunction">The convert expression.</param>
	public virtual MemberMap<TClass, TMember> Convert(ConvertToString<TClass> convertToStringFunction)
	{
		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "Convert", 233);
		var instance = convertToStringFunction.Target != null ? Expression.Constant(convertToStringFunction.Target) : null;
		var fieldParameter = Expression.Parameter(typeof(ConvertToStringArgs<TClass>), "args");
		var methodExpression = Expression.Call
		(
			instance,
			convertToStringFunction.Method,
			fieldParameter
		);
		var lambdaExpression = Expression.Lambda<ConvertToString<TClass>>(methodExpression, fieldParameter);

		Data.WritingConvertExpression = lambdaExpression;

		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "Convert", 246);
		return this;
	}

	/// <summary>
	/// Ignore the member when reading if no matching field name can be found.
	/// </summary>
	public new virtual MemberMap<TClass, TMember> Optional()
	{
		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "Optional", 255);
		Data.IsOptional = true;

		return this;
	}

	/// <summary>
	/// Specifies an expression to be used to validate a field when reading.
	/// </summary>
	/// <param name="validateExpression"></param>
	public new virtual MemberMap<TClass, TMember> Validate(Validate validateExpression)
	{
		return Validate(validateExpression, args => $"Field '{args.Field}' is not valid.");
	}

	/// <summary>
	/// Specifies an expression to be used to validate a field when reading along with specified exception message.
	/// </summary>
	/// <param name="validateExpression"></param>
	/// <param name="validateMessageExpression"></param>
	public new virtual MemberMap<TClass, TMember> Validate(Validate validateExpression, ValidateMessage validateMessageExpression)
	{
		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "Validate", 277);
		var fieldParameter = Expression.Parameter(typeof(ValidateArgs), "args");
		var validateCallExpression = Expression.Call(
			Expression.Constant(validateExpression.Target),
			validateExpression.Method,
			fieldParameter
		);
		var messageCallExpression = Expression.Call(
			Expression.Constant(validateMessageExpression.Target),
			validateMessageExpression.Method,
			fieldParameter
		);

		Data.ValidateExpression = Expression.Lambda<Validate>(validateCallExpression, fieldParameter);
		Data.ValidateMessageExpression = Expression.Lambda<ValidateMessage>(messageCallExpression, fieldParameter);

		FuzzingLogsCollector.Log("MemberMap<TClass, TMember>", "Validate", 293);
		return this;
	}
}
