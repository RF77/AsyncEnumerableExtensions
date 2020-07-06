using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AsyncEnumerableExtensions.Karnok;
using Xunit;
using Xunit.Abstractions;

namespace AsyncEnumerableExtensions.Tests
{
	public static class Extensions
	{
		public static async IAsyncEnumerable<T> If<T>(this IAsyncEnumerable<T> source, Func<T, bool> ifSelector,
			Func<IAsyncEnumerable<T>, IAsyncEnumerable<T>> ifFunction, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			var ifSourceStream = new ReplayAsyncEnumerable<T>();
			var elseResultStream = new ReplayAsyncEnumerable<T>();
			var ifResultStream = ifFunction(ifSourceStream);

			HandleStream();

			var result = ifResultStream.MergeConcurrently(elseResultStream);

			await foreach (var item in result.WithCancellation(cancellationToken))
			{
				yield return item;
			}

			async void HandleStream()
			{
				try
				{
					await foreach (var item in source.WithCancellation(cancellationToken))
					{
						if (ifSelector(item))
						{
							await ifSourceStream.Next(item);
						}
						else
						{
							await elseResultStream.Next(item);
						}
					}
				}
				catch (OperationCanceledException)
				{
				}
				catch (Exception e)
				{
					await elseResultStream.Error(e);
				}
				finally
				{
					await ifSourceStream.Complete();
					await elseResultStream.Complete();
				}
			}
		}
	}

	/// <summary>
	///     Not real tests.. more to try things
	/// </summary>
	public class TryAndErrorTests : UnitTestBase
	{
		public TryAndErrorTests(ITestOutputHelper output) : base(output)
		{
		}

		private static int _counter;

		private async IAsyncEnumerable<int> GetNumbersAsync(int amount)
		{
			for (var i = 0; i < amount; i++)
			{
				yield return _counter++;
				await Task.Delay(10);
			}
		}

		private async IAsyncEnumerable<int> GetNumbersWithErrorAsync(int amount)
		{
			for (var i = 0; i < amount; i++)
			{
				if (i == 10)
				{
					throw new InvalidOperationException();
				}
				yield return _counter++;
				await Task.Delay(10);
			}
		}

		private async IAsyncEnumerable<int> AddNumbers(IAsyncEnumerable<int> numbers)
		{
			await foreach (var ab in numbers.Buffer(2))
			{
				if (ab.Count == 2)
				{
					yield return ab[0] + ab[1];
				}
				else
				{
					yield break;
				}
			}
		}

		[Fact]
		public async Task Test1()
		{
			var stream = AsyncEnumerable.Range(0, 5).Delay(100);

			await WriteStreamToOutputAsync(stream);
		}

		[Fact]
		public async Task TestWithIf()
		{
			var stream = GetNumbersAsync(20);

			stream = stream.If(i => i % 2 == 0, AddNumbers);

			await WriteStreamToOutputAsync(stream);
		}

		[Fact]
		public async Task TestWithIfAndCancellation()
		{
			var cts = new CancellationTokenSource(100);

			var stream = GetNumbersAsync(20);

			stream = stream.If(i => i % 2 == 0, AddNumbers);

			await WriteStreamToOutputAsync(stream, cts.Token);
		}


		[Fact]
		public async Task TestWithIfAndError()
		{

			var stream = GetNumbersWithErrorAsync(20);

			stream = stream.If(i => i % 2 == 0, AddNumbers);

			await WriteStreamToOutputAsync(stream);
		}
	}
}