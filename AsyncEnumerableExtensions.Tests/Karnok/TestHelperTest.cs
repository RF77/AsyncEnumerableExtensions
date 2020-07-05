// Copyright (c) David Karnok & Contributors.
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AsyncEnumerableExtensions.Tests.Karnok
{
    public class TestHelperTest
    {
        [Fact]
        public async void AssertResultSet()
        {
            await AsyncEnumerable.Range(1, 3)
                .AssertResultSet(1, 2, 3);
        }

        [Fact]
        public async void AssertResultSet_List()
        {
            await AsyncEnum.FromArray(new List<int>(new [] { 1, 2, 3 }))
                .AssertResultSet(
                    ListComparer<int>.Default,
                    new List<int>(new[] { 1, 2, 3 }));
        }

        [Fact]
        public void HashSet_Contains()
        {
            var set = new HashSet<IList<int>>(ListComparer<int>.Default) {new List<int>(new[] {1, 2, 3})};


            Assert.Contains(new List<int>(new[] { 1, 2, 3 }), set);

            Assert.True(set.Remove(new List<int>(new[] { 1, 2, 3 })));
        }
    }
}
