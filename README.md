# AsyncEnumerableExtensions
Additional Extensions to the official System.Linq.Async, System.Interactive.Async libs, using also parts of the async-enumerable-dotnet library by David Karnok.

Due to conflicts, it's difficult to use the offical libs and async-enumerable-dotnet side by side. Therefore this library bases on additional parts of the async-enumerable-dotnet library in a way it can be used side by side.

Finally I add my own extensions, described here later.


Thanks to David Karnok (https://github.com/akarnokd) for using the following parts of his great library:
 - Operators which are not existing in System.Linq.Async and System.Interactive.Async libs
 - Replay- Multicast and UnicastAsyncEnumerables
