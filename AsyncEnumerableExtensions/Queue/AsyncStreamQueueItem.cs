// Copyright (c) 2020 by RF77 (https://github.com/RF77)
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace AsyncEnumerableExtensions.Queue
{
	public class AsyncStreamQueueItem<TStreamItem, TSourceItem>
	{
		public AsyncStreamQueueItem(TSourceItem source, Func<TSourceItem, IAsyncEnumerable<TStreamItem>> streamProducer)
		{
			Source = source;
			StreamProducer = streamProducer;
		}

		public TSourceItem Source { get; set; }
		public Func<TSourceItem, IAsyncEnumerable<TStreamItem>> StreamProducer { get; set; }
	}
}