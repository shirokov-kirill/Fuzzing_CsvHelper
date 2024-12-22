// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System.Collections;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using CsvHelper.FuzzingLogger;

namespace CsvHelper;

internal class FastDynamicObject : IDynamicMetaObjectProvider, IDictionary<string, object?>
{
	private readonly Dictionary<string, object?> dict;

	public FastDynamicObject()
	{
		FuzzingLogsCollector.Log("FastDynamicObject", "FastDynamicObject", 19);
		dict = new Dictionary<string, object?>();
	}

	object? IDictionary<string, object?>.this[string key]
	{
		get
		{
			FuzzingLogsCollector.Log("FastDynamicObject", "get", 27);
			if (!dict.ContainsKey(key))
			{
				FuzzingLogsCollector.Log("FastDynamicObject", "get", 30);
				throw new CsvHelperException($"{nameof(FastDynamicObject)} does not contain a definition for '{key}'.");
			}

			FuzzingLogsCollector.Log("FastDynamicObject", "get", 34);
			return dict[key];
		}

		set
		{
			FuzzingLogsCollector.Log("FastDynamicObject", "set", 40);
			SetValue(key, value);
		}
	}

	ICollection<string> IDictionary<string, object?>.Keys => dict.Keys;

	ICollection<object?> IDictionary<string, object?>.Values => dict.Values;

	int ICollection<KeyValuePair<string, object?>>.Count => dict.Count;

	bool ICollection<KeyValuePair<string, object?>>.IsReadOnly => false;

	object? SetValue(string key, object? value)
	{
		FuzzingLogsCollector.Log("FastDynamicObject", "SetValue", 55);
		dict[key] = value;

		return value;
	}

	DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
	{
		FuzzingLogsCollector.Log("FastDynamicObject", "GetMetaObject", 63);
		return new FastDynamicMetaObject(parameter, BindingRestrictions.Empty, this);
	}

	void IDictionary<string, object?>.Add(string key, object? value)
	{
		FuzzingLogsCollector.Log("FastDynamicObject", "Add", 69);
		SetValue(key, value);
	}

	void ICollection<KeyValuePair<string, object?>>.Add(KeyValuePair<string, object?> item)
	{
		FuzzingLogsCollector.Log("FastDynamicObject", "Add", 75);
		SetValue(item.Key, item.Value);
	}

	void ICollection<KeyValuePair<string, object?>>.Clear()
	{
		FuzzingLogsCollector.Log("FastDynamicObject", "Clear", 81);
		dict.Clear();
	}

	bool ICollection<KeyValuePair<string, object?>>.Contains(KeyValuePair<string, object?> item)
	{
		FuzzingLogsCollector.Log("FastDynamicObject", "Contains", 87);
		return dict.Contains(item);
	}

	bool IDictionary<string, object?>.ContainsKey(string key)
	{
		FuzzingLogsCollector.Log("FastDynamicObject", "ContainsKey", 93);
		return dict.ContainsKey(key);
	}

	void ICollection<KeyValuePair<string, object?>>.CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
	{
		FuzzingLogsCollector.Log("FastDynamicObject", "CopyTo", 99);
		if (arrayIndex < 0 || arrayIndex >= array.Length)
		{
			FuzzingLogsCollector.Log("FastDynamicObject", "CopyTo", 102);
			throw new ArgumentOutOfRangeException($"{nameof(arrayIndex)} must be greater than or equal to 0 and less then {nameof(array)} length.");
		}

		if (dict.Count + arrayIndex > array.Length)
		{
			FuzzingLogsCollector.Log("FastDynamicObject", "CopyTo", 108);
			throw new ArgumentException($"The number of elements in {nameof(FastDynamicMetaObject)} is greater than the available space from {nameof(arrayIndex)} to the end of the destination {nameof(array)}.");
		}

		var i = arrayIndex;
		foreach (var pair in dict)
		{
			FuzzingLogsCollector.Log("FastDynamicObject", "CopyTo", 115);
			array[i] = pair;
			i++;
		}
		FuzzingLogsCollector.Log("FastDynamicObject", "CopyTo", 119);
	}

	IEnumerator<KeyValuePair<string, object?>> IEnumerable<KeyValuePair<string, object?>>.GetEnumerator()
	{
		FuzzingLogsCollector.Log("FastDynamicObject", "GetEnumerator", 124);
		return dict.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		FuzzingLogsCollector.Log("FastDynamicObject", "GetEnumerator", 130);
		return dict.GetEnumerator();
	}

	bool IDictionary<string, object?>.Remove(string key)
	{
		FuzzingLogsCollector.Log("FastDynamicObject", "Remove", 136);
		return dict.Remove(key);
	}

	bool ICollection<KeyValuePair<string, object?>>.Remove(KeyValuePair<string, object?> item)
	{
		FuzzingLogsCollector.Log("FastDynamicObject", "Remove", 142);
		return dict.Remove(item.Key);
	}

	bool IDictionary<string, object?>.TryGetValue(string key, out object? value)
	{
		FuzzingLogsCollector.Log("FastDynamicObject", "TryGetValue", 148);
		return dict.TryGetValue(key, out value!);
	}

	private class FastDynamicMetaObject : DynamicMetaObject
	{
		private static readonly MethodInfo getValueMethod = typeof(IDictionary<string, object?>).GetProperty("Item")!.GetGetMethod()!;
		private static readonly MethodInfo setValueMethod = typeof(FastDynamicObject).GetMethod("SetValue", BindingFlags.NonPublic | BindingFlags.Instance)!;

		public FastDynamicMetaObject(Expression expression, BindingRestrictions restrictions) : base(expression, restrictions) { }

		public FastDynamicMetaObject(Expression expression, BindingRestrictions restrictions, object value) : base(expression, restrictions, value) { }

		public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
		{
			FuzzingLogsCollector.Log("FastDynamicMetaObject", "BindGetMember", 163);
			var parameters = new Expression[] { Expression.Constant(binder.Name) };

			var callMethod = CallMethod(getValueMethod, parameters);

			return callMethod;
		}

		public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
		{
			FuzzingLogsCollector.Log("FastDynamicMetaObject", "BindSetMember", 173);
			var parameters = new Expression[] { Expression.Constant(binder.Name), Expression.Convert(value.Expression, typeof(object)) };

			var callMethod = CallMethod(setValueMethod, parameters);

			return callMethod;
		}

		public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
		{
			FuzzingLogsCollector.Log("FastDynamicMetaObject", "BindInvokeMember", 183);
			var parameters = new Expression[] { Expression.Constant(binder.Name) };

			var callMethod = CallMethod(getValueMethod, parameters);

			return callMethod;
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			FuzzingLogsCollector.Log("FastDynamicMetaObject", "GetDynamicMemberNames", 193);
			if (HasValue && Value is IDictionary<string, object?> lookup)
			{
				FuzzingLogsCollector.Log("FastDynamicMetaObject", "GetDynamicMemberNames", 196);
				return lookup.Keys;
			}

			FuzzingLogsCollector.Log("FastDynamicMetaObject", "GetDynamicMemberNames", 200);
			return Array.Empty<string>();
		}

		private DynamicMetaObject CallMethod(MethodInfo method, Expression[] parameters)
		{
			FuzzingLogsCollector.Log("FastDynamicMetaObject", "CallMethod", 206);
			var callMethod = new DynamicMetaObject(Expression.Call(Expression.Convert(Expression, LimitType), method, parameters), BindingRestrictions.GetTypeRestriction(Expression, LimitType));

			return callMethod;
		}
	}
}
