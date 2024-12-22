// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper

using CsvHelper.FuzzingLogger;

namespace CsvHelper;

/// <summary>
/// Creates objects from a given type.
/// </summary>
public class ObjectResolver : IObjectResolver
{
	private static IObjectResolver current;
	private readonly ObjectCreator objectCreator = new ObjectCreator();

	/// <summary>
	/// Gets or sets the current resolver.
	/// Use an instance of this instead if at all possible.
	/// </summary>
	public static IObjectResolver Current
	{
		get
		{
			FuzzingLogsCollector.Log("ObjectResolver", "get", 26);
			return current;
		}
		set
		{
			FuzzingLogsCollector.Log("ObjectResolver", "set", 31);
			if (value == null)
			{
				FuzzingLogsCollector.Log("ObjectResolver", "set", 34);
				throw new InvalidOperationException($"{nameof(IObjectResolver)} cannot be null.");
			}

			FuzzingLogsCollector.Log("ObjectResolver", "set", 38);
			current = value;
		}
	}

	/// <summary>
	/// A value indicating if the resolver's <see cref="CanResolve"/>
	/// returns false that an object will still be created using
	/// CsvHelper's object creation. True to fallback, otherwise false.
	/// Default value is true.
	/// </summary>
	public bool UseFallback { get; private set; }

	/// <summary>
	/// A function that returns a value indicating if the resolver
	/// is able to resolve the given type. True if the type can be
	/// resolved, otherwise false.
	/// </summary>
	public Func<Type, bool> CanResolve { get; private set; }

	/// <summary>
	/// The function that creates an object from a given type.
	/// </summary>
	public Func<Type, object[], object> ResolveFunction { get; private set; }

	static ObjectResolver()
	{
		FuzzingLogsCollector.Log("ObjectResolver", "ObjectResolver", 65);
		var objectCreator = new ObjectCreator();
		var locker = new object();
		current = new ObjectResolver(type => true, (type, args) =>
		{
			FuzzingLogsCollector.Log("ObjectResolver", "ObjectResolver", 70);
			lock (locker)
			{
				FuzzingLogsCollector.Log("ObjectResolver", "ObjectResolver", 73);
				return objectCreator.CreateInstance(type, args);
			}
		});
	}

	/// <summary>
	/// Creates an instance of the object resolver using default values.
	/// </summary>
	public ObjectResolver()
	{
		CanResolve = type => true;
		ResolveFunction = ResolveWithObjectCreator;
		UseFallback = true;
	}

	/// <summary>
	/// Creates an instance of the object resolver using the given can create function
	/// and create function.
	/// </summary>
	/// <param name="canResolve">A function that returns a value indicating if the resolver
	/// is able to resolve the given type. True if the type can be
	/// resolved, otherwise false.</param>
	/// <param name="resolveFunction">The function that creates an object from a given type.</param>
	/// <param name="useFallback">A value indicating if the resolver's <see cref="CanResolve"/>
	/// returns false that an object will still be created using
	/// CsvHelper's object creation. True to fallback, otherwise false.
	/// Default value is true.</param>
	public ObjectResolver(Func<Type, bool> canResolve, Func<Type, object[], object> resolveFunction, bool useFallback = true)
	{
		CanResolve = canResolve ?? throw new ArgumentNullException(nameof(canResolve));
		ResolveFunction = resolveFunction ?? throw new ArgumentNullException(nameof(resolveFunction));
		UseFallback = useFallback;
	}

	/// <summary>
	/// Creates an object from the given type using the <see cref="ResolveFunction"/>
	/// function. If <see cref="CanResolve"/> is false, the object will be
	/// created using CsvHelper's default object creation. If <see cref="UseFallback"/>
	/// is false, an exception is thrown.
	/// </summary>
	/// <param name="type">The type to create an instance from. The created object
	/// may not be the same type as the given type.</param>
	/// <param name="constructorArgs">Constructor arguments used to create the type.</param>
	public object Resolve(Type type, params object[] constructorArgs)
	{
		FuzzingLogsCollector.Log("ObjectResolver", "Resolve", 119);
		if (CanResolve(type))
		{
			FuzzingLogsCollector.Log("ObjectResolver", "Resolve", 122);
			return ResolveFunction(type, constructorArgs);
		}

		if (UseFallback)
		{
			FuzzingLogsCollector.Log("ObjectResolver", "Resolve", 128);
			return objectCreator.CreateInstance(type, constructorArgs);
		}

		FuzzingLogsCollector.Log("ObjectResolver", "Resolve", 132);
		throw new CsvHelperException($"Type '{type.FullName}' can't be resolved and fallback is turned off.");
	}

	/// <summary>
	/// Creates an object from the given type using the <see cref="ResolveFunction"/>
	/// function. If <see cref="CanResolve"/> is false, the object will be
	/// created using CsvHelper's default object creation. If <see cref="UseFallback"/>
	/// is false, an exception is thrown.
	/// </summary>
	/// <typeparam name="T">The type to create an instance from. The created object
	/// may not be the same type as the given type.</typeparam>
	/// <param name="constructorArgs">Constructor arguments used to create the type.</param>
	public T Resolve<T>(params object[] constructorArgs)
	{
		FuzzingLogsCollector.Log("ObjectResolver", "Resolve<T>", 147);
		return (T)Resolve(typeof(T), constructorArgs);
	}

	private object ResolveWithObjectCreator(Type type, params object[] args)
	{
		FuzzingLogsCollector.Log("ObjectResolver", "ResolveWithObjectCreator", 153);
		return objectCreator.CreateInstance(type, args);
	}
}
