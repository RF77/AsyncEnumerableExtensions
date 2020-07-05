﻿// Copyright (c) David Karnok & Contributors.
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Interactive.Async.Karnok;
using System.Threading;

namespace AsyncEnumerableExtensions.Karnok.impl
{
    internal sealed class Replay<TSource, TResult> : IAsyncEnumerable<TResult>
    {
        private readonly IAsyncEnumerable<TSource> _source;

        private readonly Func<IAsyncEnumerable<TSource>, IAsyncEnumerable<TResult>> _func;

        public Replay(IAsyncEnumerable<TSource> source, Func<IAsyncEnumerable<TSource>, IAsyncEnumerable<TResult>> func)
        {
            _source = source;
            _func = func;
        }

        public IAsyncEnumerator<TResult> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            var subject = new ReplayAsyncEnumerable<TSource>();
            IAsyncEnumerable<TResult> result;
            try
            {
                result = _func(subject);
            }
            catch (Exception ex)
            {
                return new Error<TResult>.ErrorEnumerator(ex);
            }
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var en = new MulticastEnumerator<TSource, TResult>(_source.GetAsyncEnumerator(cts.Token), subject, result.GetAsyncEnumerator(cancellationToken), cts);
            return en;
        }
    }
}
