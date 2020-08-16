// Copyright (c) 2020 by RF77 (https://github.com/RF77)
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AsyncEnumerableExtensions.Karnok;

namespace System.Linq
{
	public static class BufferOperator
	{
		/// <summary>
		/// Buffer to either max items or until time span is reached
		/// </summary>
		/// <typeparam name="T">async stream type</typeparam>
		/// <param name="source">async stream</param>
		/// <param name="maxSize">As soon as this amount of items is reached, a buffer will be returned. If the maxTime will be reached, a buffer with less items will be returned</param>
		/// <param name="maxTime">Collect items as long until the time span is over. If the amount fo items exceeds the defined maxSize, the buffer will be return earlier</param>
		/// <param name="cancellationToken"></param>
		/// <returns>a buffer of items with maxSize items or less if the maxTime is over</returns>
		public static async IAsyncEnumerable<IList<T>> Buffer<T>(
			this IAsyncEnumerable<T> source, int maxSize, TimeSpan maxTime, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (maxSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxSize));
			}

			List<T> buffer = new List<T>(maxSize);
			var lastBufferTime = DateTime.Now;

			await foreach (var item in source.WithCancellation(cancellationToken))
			{
				buffer.Add(item);
				var now = DateTime.Now;
				if (buffer.Count >= maxSize || (now - lastBufferTime) >= maxTime)
				{
					yield return buffer;
					buffer = new List<T>(maxSize);
					lastBufferTime = now;
				}
			}

			if (buffer.Any())
			{
				yield return buffer;
			}
		}
	}
}