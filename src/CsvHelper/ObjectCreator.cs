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
/// Efficiently creates instances of object types.
/// </summary>
public class ObjectCreator
{
	private readonly Dictionary<int, Func<object?[], object>> cache = new Dictionary<int, Func<object?[], object>>();

	/// <summary>
	/// Creates an instance of type T using the given arguments.
	/// </summary>
	/// <typeparam name="T">The type to create an instance of.</typeparam>
	/// <param name="args">The constrcutor arguments.</param>
	public T CreateInstance<T>(params object?[] args)
	{
		FuzzingLogsCollector.Log("ObjectCreator", "CreateInstance<T>", 26);
		return (T)CreateInstance(typeof(T), args);
	}

	/// <summary>
	/// Creates an instance of the given type using the given arguments.
	/// </summary>
	/// <param name="type">The type to create an instance of.</param>
	/// <param name="args">The constructor arguments.</param>
	public object CreateInstance(Type type, params object?[] args)
	{
		FuzzingLogsCollector.Log("ObjectCreator", "CreateInstance", 37);
		var func = GetFunc(type, args);

		return func(args);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Func<object?[], object> GetFunc(Type type, object?[] args)
	{
		FuzzingLogsCollector.Log("ObjectCreator", "GetFunc", 46);
		var argTypes = GetArgTypes(args);
		var key = GetConstructorCacheKey(type, argTypes);
		if (!cache.TryGetValue(key, out var func))
		{
			FuzzingLogsCollector.Log("ObjectCreator", "GetFunc", 51);
			cache[key] = func = CreateInstanceFunc(type, argTypes);
		}

		FuzzingLogsCollector.Log("ObjectCreator", "GetFunc", 55);
		return func;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Type[] GetArgTypes(object?[] args)
	{
		FuzzingLogsCollector.Log("ObjectCreator", "GetArgTypes", 62);
		var argTypes = new Type[args.Length];
		for (var i = 0; i < args.Length; i++)
		{
			FuzzingLogsCollector.Log("ObjectCreator", "GetArgTypes", 66);
			argTypes[i] = args[i]?.GetType() ?? typeof(object);
		}

		FuzzingLogsCollector.Log("ObjectCreator", "GetArgTypes", 70);
		return argTypes;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int GetConstructorCacheKey(Type type, Type[] args)
	{
		FuzzingLogsCollector.Log("ObjectCreator", "GetConstructorCacheKey", 77);
		var hashCode = new HashCode();
		hashCode.Add(type.GetHashCode());
		for (var i = 0; i < args.Length; i++)
		{
			FuzzingLogsCollector.Log("ObjectCreator", "GetConstructorCacheKey", 82);
			hashCode.Add(args[i].GetHashCode());
		}

		FuzzingLogsCollector.Log("ObjectCreator", "GetConstructorCacheKey", 86);
		return hashCode.ToHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Func<object?[], object> CreateInstanceFunc(Type type, Type[] argTypes)
	{
		FuzzingLogsCollector.Log("ObjectCreator", "CreateInstanceFunc", 93);
		var parameterExpression = Expression.Parameter(typeof(object[]), "args");

		Expression body;
		if (type.IsValueType)
		{
			FuzzingLogsCollector.Log("ObjectCreator", "CreateInstanceFunc", 99);
			if (argTypes.Length > 0)
			{
				FuzzingLogsCollector.Log("ObjectCreator", "CreateInstanceFunc", 102);
				throw GetConstructorNotFoundException(type, argTypes);
			}

			body = Expression.Convert(Expression.Default(type), typeof(object));
		}
		else
		{
			FuzzingLogsCollector.Log("ObjectCreator", "CreateInstanceFunc", 110);
			var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var constructor = GetConstructor(constructors, type, argTypes);

			var parameters = constructor.GetParameters();
			var parameterTypes = new Type[parameters.Length];
			for (var i = 0; i < parameters.Length; i++)
			{
				FuzzingLogsCollector.Log("ObjectCreator", "CreateInstanceFunc", 118);
				parameterTypes[i] = parameters[i].ParameterType;
			}

			var arguments = new List<Expression>();
			for (var i = 0; i < parameterTypes.Length; i++)
			{
				FuzzingLogsCollector.Log("ObjectCreator", "CreateInstanceFunc", 125);
				var parameterType = parameterTypes[i];
				var arrayIndexExpression = Expression.ArrayIndex(parameterExpression, Expression.Constant(i));
				var convertExpression = Expression.Convert(arrayIndexExpression, parameterType);
				arguments.Add(convertExpression);
			}

			FuzzingLogsCollector.Log("ObjectCreator", "CreateInstanceFunc", 132);
			body = Expression.New(constructor, arguments);
		}

		FuzzingLogsCollector.Log("ObjectCreator", "CreateInstanceFunc", 136);
		var lambda = Expression.Lambda<Func<object?[], object>>(body, new[] { parameterExpression });
		var func = lambda.Compile();

		return func;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ConstructorInfo GetConstructor(ConstructorInfo[] constructors, Type type, Type[] argTypes)
	{
		FuzzingLogsCollector.Log("ObjectCreator", "GetConstructor", 146);
		var matchType = MatchType.Exact;
		var fuzzyMatches = new List<ConstructorInfo>();
		for (var i = 0; i < constructors.Length; i++)
		{
			FuzzingLogsCollector.Log("ObjectCreator", "GetConstructor", 151);
			var constructor = constructors[i];
			var parameters = constructors[i].GetParameters();

			if (parameters.Length != argTypes.Length)
			{
				FuzzingLogsCollector.Log("ObjectCreator", "GetConstructor", 157);
				continue;
			}

			for (var j = 0; j < parameters.Length && j < argTypes.Length; j++)
			{
				FuzzingLogsCollector.Log("ObjectCreator", "GetConstructor", 163);
				var parameterType = parameters[j].ParameterType;
				var argType = argTypes[j];

				if (argType == parameterType)
				{
					FuzzingLogsCollector.Log("ObjectCreator", "GetConstructor", 169);
					matchType = MatchType.Exact;
					continue;
				}

				if (!parameterType.IsValueType && (parameterType.IsAssignableFrom(argType) || argType == typeof(object)))
				{
					FuzzingLogsCollector.Log("ObjectCreator", "GetConstructor", 176);
					matchType = MatchType.Fuzzy;
					continue;
				}

				FuzzingLogsCollector.Log("ObjectCreator", "GetConstructor", 181);
				matchType = MatchType.None;
				break;
			}

			if (matchType == MatchType.Exact)
			{
				FuzzingLogsCollector.Log("ObjectCreator", "GetConstructor", 188);
				// Only possible to have one exact match.
				return constructor;
			}

			if (matchType == MatchType.Fuzzy)
			{
				FuzzingLogsCollector.Log("ObjectCreator", "GetConstructor", 195);
				fuzzyMatches.Add(constructor);
			}
			FuzzingLogsCollector.Log("ObjectCreator", "GetConstructor", 198);
		}

		if (fuzzyMatches.Count == 1)
		{
			FuzzingLogsCollector.Log("ObjectCreator", "GetConstructor", 203);
			return fuzzyMatches[0];
		}

		if (fuzzyMatches.Count > 1)
		{
			FuzzingLogsCollector.Log("ObjectCreator", "GetConstructor", 209);
			throw new AmbiguousMatchException();
		}

		FuzzingLogsCollector.Log("ObjectCreator", "GetConstructor", 213);
		throw GetConstructorNotFoundException(type, argTypes);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static MissingMethodException GetConstructorNotFoundException(Type type, Type[] argTypes)
	{
		FuzzingLogsCollector.Log("ObjectCreator", "GetConstructorNotFoundException", 220);
		var signature = $"{type.FullName}({string.Join(", ", argTypes.Select(a => a.FullName))})";

		FuzzingLogsCollector.Log("ObjectCreator", "GetConstructorNotFoundException", 223);
		throw new MissingMethodException($"Constructor '{signature}' was not found.");
	}

	private enum MatchType
	{
		None = 0,
		Exact = 1,
		Fuzzy = 2
	}
}
