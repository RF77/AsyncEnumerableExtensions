// Copyright (c) David Karnok & Contributors.
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncEnumerableExtensions.Karnok.impl
{
    internal sealed class FromEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly IEnumerable<T> _source;

        public FromEnumerable(IEnumerable<T> source)
        {
            _source = source;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            return new FromEnumerableEnumerator(_source.GetEnumerator(), cancellationToken);
        }

        private sealed class FromEnumerableEnumerator : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _source;

            private readonly CancellationToken _ct;

            public T Current => _source.Current;

            public FromEnumerableEnumerator(IEnumerator<T> source, CancellationToken ct)
            {
                _source = source;
                _ct = ct;
            }

            public ValueTask DisposeAsync()
            {
                _source.Dispose();
                return new ValueTask();
            }

            public ValueTask<bool> MoveNextAsync()
            {
                if (!_ct.IsCancellationRequested && _source.MoveNext())
                {
                    return new ValueTask<bool>(true);
                }
                return new ValueTask<bool>(false);
            }
        }
    }
}
