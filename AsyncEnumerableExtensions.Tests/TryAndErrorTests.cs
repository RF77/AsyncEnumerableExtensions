using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Time.AsyncEnumerable.Extensions.Tests
{
	/// <summary>
	/// Not real tests.. more to try things
	/// </summary>
	public class TryAndErrorTests : UnitTestBase
	{
		public TryAndErrorTests(ITestOutputHelper output) : base(output)
		{
		}		
		
		[Fact]
		public async Task Test1()
		{
			var stream = System.Linq.AsyncEnumerable.Range(0, 5).Delay(100);

			await WriteStreamToOutputAsync(stream);
		}
		
	}
}
