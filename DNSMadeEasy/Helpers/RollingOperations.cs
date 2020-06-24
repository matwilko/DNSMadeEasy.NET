using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DNSMadeEasy
{
	internal static class RollingOperations
	{
		public static async IAsyncEnumerable<T> Roll<T>(int numOfOperations, int concurrency, Func<int, CancellationToken, Task<T>> taskGenerator, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			if (numOfOperations == 1)
			{
				yield return await taskGenerator(0, cancellationToken).ConfigureAwait(false);
				yield break;
			}

			if (concurrency > numOfOperations)
			{
				var tasks = new Task<T>[numOfOperations];
				for (var i = 0; i < numOfOperations; i++) 
					tasks[i] = taskGenerator(i, cancellationToken);

				await Task.WhenAll(tasks).ConfigureAwait(false);
				
				for (var i = 0; i < numOfOperations; i++)
				{
					cancellationToken.ThrowIfCancellationRequested();
					yield return await tasks[i].ConfigureAwait(false);
				}

				yield break;
			}

			var operations = new Task<T>[concurrency];
			var operationsStarted = 0;
			var operationsCompleted = 0;

			for (var i = 0; i < concurrency; i++)
			{
				operations[i] = taskGenerator(operationsStarted++, cancellationToken);
			}

			while (operationsCompleted < numOfOperations)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (operationsStarted == numOfOperations)
					await Task.WhenAll(operations).ConfigureAwait(false);
				else
					await Task.WhenAny(operations).ConfigureAwait(false);

				for (var i = 0; i < operations.Length; i++)
				{
					cancellationToken.ThrowIfCancellationRequested();

					var operation = operations[i];

					if (!operation.IsCompleted)
						continue;

					operationsCompleted++;
					if (operationsStarted < numOfOperations)
						operations[i] = taskGenerator(operationsStarted++, cancellationToken);

					yield return await operation.ConfigureAwait(false);
				}
			}
		}
	}
}
