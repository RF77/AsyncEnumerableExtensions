// Copyright (c) David Karnok & Contributors.
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncEnumerableExtensions.Karnok.impl
{
    internal sealed class Just<T> : IAsyncEnumerable<T>
    {
        private readonly T _value;

        public Just(T value)
        {
            _value = value;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            return new JustEnumerator(_value);
        }

        private sealed class JustEnumerator : IAsyncEnumerator<T>
        {
            private bool _once;

            public JustEnumerator(T value)
            {
                Current = value;
            }

            public T Current { get; private set; }

            public ValueTask DisposeAsync()
            {
                Current = default;
                return new ValueTask();
            }

            public ValueTask<bool> MoveNextAsync()
            {
                if (_once)
                {
                    return new ValueTask<bool>(false);
                }
                _once = true;
                return new ValueTask<bool>(true);
            }
        }
    }
}
