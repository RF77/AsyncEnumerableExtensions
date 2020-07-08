// Copyright (c) 2020 by RF77 (https://github.com/RF77)
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using AsyncEnumerableExtensions.Karnok;

namespace System.Linq
{
	public static class MergeOperator
	{
		public static IAsyncEnumerable<T> MergeConcurrently<T>(this IAsyncEnumerable<IAsyncEnumerable<T>> sources)
		{
			var queue = new UnicastAsyncEnumerable<T>();
			var tasks = new List<Task>();

			StreamSources();
			
			return queue;

			async void StreamSources()
			{
				try
				{
					await foreach (var stream in sources)
					{
						tasks.Add(StreamSource(stream));
					}

					// After the last stream arrived, we have to wait until all of those streams have completely finished with streaming
					await Task.WhenAll(tasks);
				}
				catch (OperationCanceledException)
				{
				}
				catch (Exception e)
				{
					await queue.Error(e);
				}
				finally
				{
					await queue.Complete();
				}
			}

			async Task StreamSource(IAsyncEnumerable<T> source)
			{
				try
				{
					await foreach (var item in source)
					{
						await queue.Next(item);
					}
				}
				catch (OperationCanceledException)
				{
				}
				catch (Exception e)
				{
					await queue.Error(e);
				}
			}
		}
	}
}