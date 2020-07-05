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
		public async Task TestForEachAsAsyncEnumerableWithAsyncEnumerable()
		{
			var stream = Range(0, 5).Delay(100).ForEachAsAsyncEnumerable(i => GetMoreNumbersFromNumber(i), 2);

			await WriteStreamToOutputAsync(stream);
		}

		[Fact]
		public async Task TestForEachAsAsyncEnumerableWithAsyncEnumerableWithCancellation()
		{
			var cts = new CancellationTokenSource(300);
			var stream = Range(0, 5).Delay(100).ForEachAsAsyncEnumerable(i => GetMoreNumbersFromNumber(i), 2, cts.Token);

			await WriteStreamToOutputAsync(stream);
		}

		[Fact]
		public async Task TestForEachAsAsyncEnumerableWithEnumerables()
		{
			var stream = Enumerable.Range(0, 5).ForEachAsAsyncEnumerable(i => GetMoreNumbersFromNumber(i), 2);

			await WriteStreamToOutputAsync(stream);
		}

		[Fact]
		public async Task TestForEachAsAsyncEnumerableWithEnumerablesWithCancellation()
		{
			var cts = new CancellationTokenSource(300);
			var stream = Enumerable.Range(0, 5).ForEachAsAsyncEnumerable(i => GetMoreNumbersFromNumber(i), 2, cts.Token);

			await WriteStreamToOutputAsync(stream);
		}

		[Fact]
		public async Task TestForEachAsAsyncEnumerableWithEnumerablesWithCancellationOnConsumer()
		{
			var cts = new CancellationTokenSource(300);
			var stream = Enumerable.Range(0, 5).ForEachAsAsyncEnumerable(i => GetMoreNumbersFromNumber(i), 2);

			await WriteStreamToOutputAsync(stream, cts.Token);
		}

		[Fact]
		public async Task TestForEachAsAsyncEnumerableWithEnumerablesWithCancellationOnAsyncFunc()
		{
			var cts = new CancellationTokenSource(300);
			var stream = Enumerable.Range(0, 5).ForEachAsAsyncEnumerable(i => GetMoreNumbersFromNumber(i, cts.Token), 2);

			await WriteStreamToOutputAsync(stream);
		}

		public async IAsyncEnumerable<string> GetMoreNumbersFromNumber(int number, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			string current = "";
			for (int i = 0; i < 4; i++)
			{
				current += number.ToString();
				Write($"yield {current}");
				yield return current;
				await Task.Delay(50, cancellationToken);
			}
		}
	}
}
