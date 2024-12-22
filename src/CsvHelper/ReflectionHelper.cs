// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using CsvHelper.FuzzingLogger;

namespace CsvHelper;

/// <summary>
/// Common reflection tasks.
/// </summary>
internal static class ReflectionHelper
{
	/// <summary>
	/// Gets the <see cref="PropertyInfo"/> from the type where the property was declared.
	/// </summary>
	/// <param name="type">The type the property belongs to.</param>
	/// <param name="property">The property to search.</param>
	/// <param name="flags">Flags for how the property is retrieved.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PropertyInfo GetDeclaringProperty(Type type, PropertyInfo property, BindingFlags flags)
	{
		FuzzingLogsCollector.Log("ReflectionHelper", "GetDeclaringProperty", 28);
		if (property.DeclaringType != type)
		{
			FuzzingLogsCollector.Log("ReflectionHelper", "GetDeclaringProperty", 31);
			var declaringProperty = property.DeclaringType!.GetProperty(property.Name, flags)!;
			return GetDeclaringProperty(property.DeclaringType, declaringProperty, flags);
		}

		FuzzingLogsCollector.Log("ReflectionHelper", "GetDeclaringProperty", 36);
		return property;
	}

	/// <summary>
	/// Gets the <see cref="FieldInfo"/> from the type where the field was declared.
	/// </summary>
	/// <param name="type">The type the field belongs to.</param>
	/// <param name="field">The field to search.</param>
	/// <param name="flags">Flags for how the field is retrieved.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static FieldInfo GetDeclaringField(Type type, FieldInfo field, BindingFlags flags)
	{
		FuzzingLogsCollector.Log("ReflectionHelper", "GetDeclaringField", 49);
		if (field.DeclaringType != type)
		{
			FuzzingLogsCollector.Log("ReflectionHelper", "GetDeclaringField", 52);
			var declaringField = field.DeclaringType!.GetField(field.Name, flags)!;
			return GetDeclaringField(field.DeclaringType, declaringField, flags);
		}

		FuzzingLogsCollector.Log("ReflectionHelper", "GetDeclaringField", 57);
		return field;
	}

	/// <summary>
	/// Walk up the inheritance tree collecting properties. This will get a unique set of properties in the
	/// case where parents have the same property names as children.
	/// </summary>
	/// <param name="type">The <see cref="Type"/> to get properties for.</param>
	/// <param name="flags">The flags for getting the properties.</param>
	/// <param name="overwrite">If true, parent class properties that are hidden by `new` child properties will be overwritten.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<PropertyInfo> GetUniqueProperties(Type type, BindingFlags flags, bool overwrite = false)
	{
		FuzzingLogsCollector.Log("ReflectionHelper", "GetUniqueProperties", 71);
		var ignoreBase = type.GetCustomAttribute(typeof(IgnoreBaseAttribute)) != null;

		var properties = new Dictionary<string, PropertyInfo>();

		flags |= BindingFlags.DeclaredOnly;
		var currentType = type;
		while (currentType != null)
		{
			FuzzingLogsCollector.Log("ReflectionHelper", "GetUniqueProperties", 80);
			var currentProperties = currentType.GetProperties(flags);
			foreach (var property in currentProperties)
			{
				FuzzingLogsCollector.Log("ReflectionHelper", "GetUniqueProperties", 84);
				if (!properties.ContainsKey(property.Name) || overwrite)
				{
					FuzzingLogsCollector.Log("ReflectionHelper", "GetUniqueProperties", 87);
					properties[property.Name] = property;
				}
			}

			if (ignoreBase)
			{
				FuzzingLogsCollector.Log("ReflectionHelper", "GetUniqueProperties", 94);
				break;
			}

			currentType = currentType.BaseType;
		}

		FuzzingLogsCollector.Log("ReflectionHelper", "GetUniqueProperties", 101);
		return properties.Values.ToList();
	}

	/// <summary>
	/// Walk up the inheritance tree collecting fields. This will get a unique set of fields in the
	/// case where parents have the same field names as children.
	/// </summary>
	/// <param name="type">The <see cref="Type"/> to get fields for.</param>
	/// <param name="flags">The flags for getting the fields.</param>
	/// <param name="overwrite">If true, parent class fields that are hidden by `new` child fields will be overwritten.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<FieldInfo> GetUniqueFields(Type type, BindingFlags flags, bool overwrite = false)
	{
		FuzzingLogsCollector.Log("ReflectionHelper", "GetUniqueFields", 115);
		var ignoreBase = type.GetCustomAttribute(typeof(IgnoreBaseAttribute)) != null;

		var fields = new Dictionary<string, FieldInfo>();

		flags |= BindingFlags.DeclaredOnly;
		var currentType = type;
		while (currentType != null)
		{
			FuzzingLogsCollector.Log("ReflectionHelper", "GetUniqueFields", 124);
			var currentFields = currentType.GetFields(flags);
			foreach (var field in currentFields)
			{
				FuzzingLogsCollector.Log("ReflectionHelper", "GetUniqueFields", 128);
				if (!fields.ContainsKey(field.Name) || overwrite)
				{
					FuzzingLogsCollector.Log("ReflectionHelper", "GetUniqueFields", 131);
					fields[field.Name] = field;
				}
			}

			if (ignoreBase)
			{
				FuzzingLogsCollector.Log("ReflectionHelper", "GetUniqueFields", 138);
				break;
			}

			currentType = currentType.BaseType;
		}

		FuzzingLogsCollector.Log("ReflectionHelper", "GetUniqueFields", 145);
		return fields.Values.ToList();
	}

	/// <summary>
	/// Gets the property from the expression.
	/// </summary>
	/// <typeparam name="TModel">The type of the model.</typeparam>
	/// <typeparam name="TProperty">The type of the property.</typeparam>
	/// <param name="expression">The expression.</param>
	/// <returns>The <see cref="PropertyInfo"/> for the expression.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static MemberInfo GetMember<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression)
	{
		FuzzingLogsCollector.Log("ReflectionHelper", "GetMember<TModel, TProperty>", 159);
		var member = GetMemberExpression(expression.Body)?.Member;
		var property = member as PropertyInfo;
		if (property != null)
		{
			FuzzingLogsCollector.Log("ReflectionHelper", "GetMember<TModel, TProperty>", 164);
			return property;
		}

		var field = member as FieldInfo;
		if (field != null)
		{
			FuzzingLogsCollector.Log("ReflectionHelper", "GetMember<TModel, TProperty>", 171);
			return field;
		}

		FuzzingLogsCollector.Log("ReflectionHelper", "GetMember<TModel, TProperty>", 175);
		throw new ConfigurationException($"'{member?.Name}' is not a member.");
	}

	/// <summary>
	/// Gets the member inheritance chain as a stack.
	/// </summary>
	/// <typeparam name="TModel">The type of the model.</typeparam>
	/// <typeparam name="TProperty">The type of the property.</typeparam>
	/// <param name="expression">The member expression.</param>
	/// <returns>The inheritance chain for the given member expression as a stack.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Stack<MemberInfo> GetMembers<TModel, TProperty>(Expression<Func<TModel, TProperty?>> expression)
	{
		FuzzingLogsCollector.Log("ReflectionHelper", "GetMembers<TModel, TProperty>", 189);
		var stack = new Stack<MemberInfo>();

		var currentExpression = expression.Body!;
		while (true)
		{
			FuzzingLogsCollector.Log("ReflectionHelper", "GetMember<TModel, TProperty>", 195);
			var memberExpression = GetMemberExpression(currentExpression);
			if (memberExpression == null)
			{
				FuzzingLogsCollector.Log("ReflectionHelper", "GetMember<TModel, TProperty>", 199);
				break;
			}

			stack.Push(memberExpression.Member);
			currentExpression = memberExpression.Expression!;
		}

		FuzzingLogsCollector.Log("ReflectionHelper", "GetMember<TModel, TProperty>", 207);
		return stack;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static MemberExpression? GetMemberExpression(Expression expression)
	{
		FuzzingLogsCollector.Log("ReflectionHelper", "GetMemberExpression", 214);
		MemberExpression? memberExpression = null;
		if (expression.NodeType == ExpressionType.Convert)
		{
			FuzzingLogsCollector.Log("ReflectionHelper", "GetMemberExpression", 218);
			var body = (UnaryExpression)expression;
			memberExpression = body.Operand as MemberExpression;
		}
		else if (expression.NodeType == ExpressionType.MemberAccess)
		{
			FuzzingLogsCollector.Log("ReflectionHelper", "GetMemberExpression", 224);
			memberExpression = expression as MemberExpression;
		}

		FuzzingLogsCollector.Log("ReflectionHelper", "GetMemberExpression", 228);
		return memberExpression;
	}
}
