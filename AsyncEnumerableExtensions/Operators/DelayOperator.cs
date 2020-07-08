using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
	public static class DelayOperator
	{
		public static async IAsyncEnumerable<T> Delay<T>(this IAsyncEnumerable<T> source, TimeSpan delay, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			await foreach (var item in source.WithCancellation(cancellationToken))
			{
				await Task.Delay(delay, cancellationToken);
				//if (cancellationToken.IsCancellationRequested)
				//{
				//	yield break;
				//}

				yield return item;
			}
		}

		public static IAsyncEnumerable<T> Delay<T>(this IAsyncEnumerable<T> source, int ms, CancellationToken cancellationToken = default)
		{
			return Delay(source, TimeSpan.FromMilliseconds(ms), cancellationToken);
		}
	}
}
