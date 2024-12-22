// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using CsvHelper.FuzzingLogger;

namespace CsvHelper;

/// <summary>
/// Extensions to help with reflection.
/// </summary>
public static class ReflectionExtensions
{
	/// <summary>
	/// Gets the type from the member.
	/// </summary>
	/// <param name="member">The member to get the type from.</param>
	/// <returns>The type.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Type MemberType(this MemberInfo member)
	{
		FuzzingLogsCollector.Log("ReflectionExtensions", "MemberType", 25);
		var property = member as PropertyInfo;
		if (property != null)
		{
			FuzzingLogsCollector.Log("ReflectionExtensions", "MemberType", 29);
			return property.PropertyType;
		}

		var field = member as FieldInfo;
		if (field != null)
		{
			FuzzingLogsCollector.Log("ReflectionExtensions", "MemberType", 36);
			return field.FieldType;
		}

		FuzzingLogsCollector.Log("ReflectionExtensions", "MemberType", 40);
		throw new InvalidOperationException("Member is not a property or a field.");
	}

	/// <summary>
	/// Gets a member expression for the member.
	/// </summary>
	/// <param name="member">The member to get the expression for.</param>
	/// <param name="expression">The member expression.</param>
	/// <returns>The member expression.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static MemberExpression GetMemberExpression(this MemberInfo member, Expression expression)
	{
		FuzzingLogsCollector.Log("ReflectionExtensions", "GetMemberExpression", 53);
		var property = member as PropertyInfo;
		if (property != null)
		{
			FuzzingLogsCollector.Log("ReflectionExtensions", "GetMemberExpression", 57);
			return Expression.Property(expression, property);
		}

		var field = member as FieldInfo;
		if (field != null)
		{
			FuzzingLogsCollector.Log("ReflectionExtensions", "GetMemberExpression", 64);
			return Expression.Field(expression, field);
		}

		FuzzingLogsCollector.Log("ReflectionExtensions", "GetMemberExpression", 68);
		throw new InvalidOperationException("Member is not a property or a field.");
	}

	/// <summary>
	/// Gets a value indicating if the given type is anonymous.
	/// True for anonymous, otherwise false.
	/// </summary>
	/// <param name="type">The type.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsAnonymous(this Type type)
	{
		FuzzingLogsCollector.Log("ReflectionExtensions", "IsAnonymous", 80);
		if (type == null)
		{
			FuzzingLogsCollector.Log("ReflectionExtensions", "IsAnonymous", 83);
			throw new ArgumentNullException(nameof(type));
		}

		FuzzingLogsCollector.Log("ReflectionExtensions", "IsAnonymous", 87);
		// https://stackoverflow.com/a/2483054/68499
		var isAnonymous = Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
			&& type.IsGenericType
			&& type.Name.Contains("AnonymousType")
			&& (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
			&& (type.Attributes & TypeAttributes.Public) != TypeAttributes.Public;

		return isAnonymous;
	}

	/// <summary>
	/// Gets a value indicating if the given type has a parameterless constructor.
	/// True if it has a parameterless constructor, otherwise false.
	/// </summary>
	/// <param name="type">The type.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool HasParameterlessConstructor(this Type type)
	{
		FuzzingLogsCollector.Log("ReflectionExtensions", "HasParameterlessConstructor", 106);
		return type.GetConstructor(new Type[0]) != null;
	}

	/// <summary>
	/// Gets a value indicating if the given type has any constructors.
	/// </summary>
	/// <param name="type">The type.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool HasConstructor(this Type type)
	{
		FuzzingLogsCollector.Log("ReflectionExtensions", "HasConstructor", 117);
		return type.GetConstructors().Length > 0;
	}

	/// <summary>
	/// Gets the constructor that contains the most parameters.
	/// </summary>
	/// <param name="type">The type.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ConstructorInfo GetConstructorWithMostParameters(this Type type)
	{
		FuzzingLogsCollector.Log("ReflectionExtensions", "GetConstructorWithMostParameters", 128);
		return type.GetConstructors()
			.OrderByDescending(c => c.GetParameters().Length)
			.First();
	}

	/// <summary>
	/// Gets a value indicating if the type is a user defined struct.
	/// True if it is a user defined struct, otherwise false.
	/// </summary>
	/// <param name="type">The type.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsUserDefinedStruct(this Type type)
	{
		FuzzingLogsCollector.Log("ReflectionExtensions", "IsUserDefinedStruct", 142);
		return type.IsValueType && !type.IsPrimitive && !type.IsEnum;
	}

	/// <summary>
	/// Gets a string representation of the constructor.
	/// </summary>
	/// <param name="constructor">The constructor.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetDefinition(this ConstructorInfo constructor)
	{
		FuzzingLogsCollector.Log("ReflectionExtensions", "GetDefinition", 153);
		var parameters = constructor.GetParameters();
		var definition = $"{constructor.Name}({string.Join(", ", parameters.Select(p => p.GetDefinition()))})";

		return definition;
	}

	/// <summary>
	/// Gets a string representation of the parameter.
	/// </summary>
	/// <param name="parameter">The parameter.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetDefinition(this ParameterInfo parameter)
	{
		FuzzingLogsCollector.Log("ReflectionExtensions", "GetDefinition", 167);
		return $"{parameter.ParameterType.Name} {parameter.Name}";
	}
}
