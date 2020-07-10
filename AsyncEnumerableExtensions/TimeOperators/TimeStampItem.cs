// Copyright (c) 2020 by RF77 (https://github.com/RF77)
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System;

namespace AsyncEnumerableExtensions.TimeOperators
{
	/// <summary>
	/// Any type of item with an timestamp
	/// </summary>
	public class TimeStampItem<T>
	{
		public TimeStampItem(DateTime timeStamp, T item)
		{
			TimeStamp = timeStamp;
			Item = item;
		}

		public DateTime TimeStamp { get; }

		public T Item { get; }
	}
}
