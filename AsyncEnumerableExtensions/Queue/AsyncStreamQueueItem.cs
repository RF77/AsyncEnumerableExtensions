using System;
using System.Collections.Generic;
using System.Threading;

namespace AsyncEnumerableExtensions.Queue
{
	public class AsyncStreamQueueItem<TStreamItem, TSourceItem>
	{
		public AsyncStreamQueueItem(TSourceItem source, Func<TSourceItem, CancellationToken, IAsyncEnumerable<TStreamItem>> streamProducer)
		{
			Source = source;
			StreamProducer = streamProducer;
		}

		public TSourceItem Source { get; set; }
		public Func<TSourceItem, CancellationToken, IAsyncEnumerable<TStreamItem>> StreamProducer { get; set; }
	}
}