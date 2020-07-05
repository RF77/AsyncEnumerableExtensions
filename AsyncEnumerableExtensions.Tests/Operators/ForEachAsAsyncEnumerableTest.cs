using Xunit.Abstractions;

namespace AsyncEnumerableExtensions.Tests.Operators
{
	public class ForEachAsAsyncEnumerableTest : UnitTestBase
	{
		public ForEachAsAsyncEnumerableTest(ITestOutputHelper output) : base(output)
		{
		}

		//[Fact]
		//public async System.Threading.Tasks.Task TestForEachAsAsyncEnumerable()
		//{
		//	Range(0, 3).Delay(100).ForEachAsAsyncEnumerable()
		//}

		//public async IAsyncEnumerable<string> GetMoreNumbersFromNumber(int number, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		//{
		//	string current = "";
		//	for (int i = 0; i < 10; i++)
		//	{
		//		current += number.ToString();
		//		yield return current;
		//		await Task.Delay(50, cancellationToken);
		//	}
		//}
	}
}
