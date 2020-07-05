// Copyright (c) David Karnok & Contributors.
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System.Linq;
using Xunit;

namespace AsyncEnumerableExtensions.Tests.Karnok
{
    public class JustTest
    {
        [Fact]
        public async void Normal()
        {
            await AsyncEnum.Just(1)
                .AssertResult(1);
        }
    }
}
