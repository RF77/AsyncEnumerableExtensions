using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AsyncEnumerableExtensions.Tests.Operators
{
	public class ForEachAsAsyncEnumerableTest : UnitTestBase
	{
		public ForEachAsAsyncEnumerableTest(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async System.Threading.Tasks.Task TestForEachAsAsyncEnumerableWithAsyncEnumerable()
		{
			var stream = Range(0, 5).Delay(200).ForEachAsAsyncEnumerable(i => GetMoreNumbersFromNumber(i), 2);

			await WriteStreamToOutputAsync(stream);
		}

		[Fact]
		public async System.Threading.Tasks.Task TestForEachAsAsyncEnumerableWithEnumerables()
		{
			var stream = Enumerable.Range(0, 5).ForEachAsAsyncEnumerable(i => GetMoreNumbersFromNumber(i), 2);

			await WriteStreamToOutputAsync(stream);
		}

		public async IAsyncEnumerable<string> GetMoreNumbersFromNumber(int number, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			string current = "";
			for (int i = 0; i < 9; i++)
			{
				current += number.ToString();
				yield return current;
				await Task.Delay(50, cancellationToken);
			}
		}
	}
}
