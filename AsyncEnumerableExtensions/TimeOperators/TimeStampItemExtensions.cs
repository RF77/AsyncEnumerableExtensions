// Copyright (c) 2020 by RF77 (https://github.com/RF77)
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using AsyncEnumerableExtensions.TimeOperators;

namespace System.Linq
{
	public static class TimeStampItemExtensions
	{
		/// <summary>
		/// Consumes a stream and and add a time stamp with the current time (local time)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static IAsyncEnumerable<TimeStampItem<T>> AddCurrentTime<T>(this IAsyncEnumerable<T> stream)
		{
			return stream.Select(i => new TimeStampItem<T>(DateTime.Now, i));
		}

		/// <summary>
		/// Converts a stream back to items only (removes the timestamp)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static IAsyncEnumerable<T> RemoveTimeStamp<T>(this IAsyncEnumerable<TimeStampItem<T>> stream)
		{
			return stream.Select(i => i.Content);
		}
	}
}