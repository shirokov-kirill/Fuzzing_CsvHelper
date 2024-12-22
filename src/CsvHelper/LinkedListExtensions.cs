// Copyright 2009-2024 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper

using CsvHelper.FuzzingLogger;

namespace CsvHelper;

internal static class LinkedListExtensions
{
	public static void Drop<T>(this LinkedList<T> list, LinkedListNode<T>? node)
	{
		FuzzingLogsCollector.Log("LinkedListExtensions", "Drop", 14);
		if (list.Count == 0 || node == null)
		{
			FuzzingLogsCollector.Log("LinkedListExtensions", "Drop", 17);
			return;
		}

		while (list.Count > 0)
		{
			FuzzingLogsCollector.Log("LinkedListExtensions", "Drop", 23);
			var nodeToRemove = list.Last;
			list.RemoveLast();
			if (nodeToRemove == node)
			{
				FuzzingLogsCollector.Log("LinkedListExtensions", "Drop", 28);
				break;
			}
		}
		FuzzingLogsCollector.Log("LinkedListExtensions", "Drop", 32);
	}
}
