using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Time.AsyncEnumerable.Extensions.Tests
{
	public class UnitTestBase
	{
		private readonly ITestOutputHelper _output;
		private readonly DateTime _startTime;
		private DateTime _lastTimeStamp;

		public UnitTestBase(ITestOutputHelper output)
		{
			_output = output;
			_startTime = DateTime.Now;
			_lastTimeStamp = _startTime;
		}

		protected IAsyncEnumerable<int> Range(int start, int count)
		{
			return System.Linq.AsyncEnumerable.Range(start, count);
		}

		protected async void WriteStreamToOutput(IAsyncEnumerable<int> replayedStream,
			CancellationToken cancellationToken = default)
		{
			await WriteStreamToOutputAsync(replayedStream, cancellationToken);
		}

		protected async Task WriteStreamToOutputAsync(IAsyncEnumerable<int> replayedStream,
			CancellationToken cancellationToken = default)
		{
			try
			{
				await foreach (var item in replayedStream.WithCancellation(cancellationToken))
				{
					Write($"Received: {item}");
				}
			}
			catch (Exception e)
			{
				Write($"WriteStreamToOutput catched {e.GetType().Name} Exception");
			}

			Write("WriteStreamToOutput completed");
		}

		protected IDisposable MustTakeLongerThan(int ms)
		{
			var stopWatch = Stopwatch.StartNew();
			return new ActionDisposable(() =>
			{
				stopWatch.Stop();
				Assert.True(stopWatch.ElapsedMilliseconds > ms, $"Measured Time: {stopWatch.ElapsedMilliseconds}ms, should be more than {ms}ms");
				Write($"Took {stopWatch.ElapsedMilliseconds}ms");
			});
		}

		protected void Write(string text)
		{
			var now = DateTime.Now;
			var message =
				$"[{Thread.CurrentThread.ManagedThreadId}] {((int) (now - _startTime).TotalMilliseconds).ToString("00,000")}ms (Δ {((int) (now - _lastTimeStamp).TotalMilliseconds).ToString("000")}ms): {text}";
			_output.WriteLine(message);
			Debug.WriteLine(message);
			_lastTimeStamp = now;
		}
	}
}