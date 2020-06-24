using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DNSMadeEasy
{
	internal sealed class DomainCache
	{
		private ConcurrentDictionary<DomainId, DomainName> Cache  { get; } = new ConcurrentDictionary<DomainId, DomainName>();
		private DnsMadeEasyClient                          Client { get; }

		public DomainCache(DnsMadeEasyClient client)
		{
			Client = client;
		}

		public DomainName? Get(DomainId id)
		{
			return Cache.TryGetValue(id, out var domainName)
				? domainName
				: default(DomainName?);
		}

		public void Set(DomainId id, DomainName name)
		{
			Cache.AddOrUpdate(id, name, (_, _) => name);
		}

		public void Remove(DomainId id)
		{
			Cache.TryRemove(id, out _);
		}

		public async ValueTask<DomainName> EnsureAndGet(DomainId id, CancellationToken cancellationToken = default)
		{
			if (Cache.TryGetValue(id, out var domainName))
				return domainName;

			await foreach (var domain in Client.GetAllDomains(cancellationToken))
				Set(domain.Id, domain.Name);

			return Cache.TryGetValue(id, out domainName)
				? domainName
				: throw new InvalidOperationException("DNSMadeEasy returned a DomainId that does not correspond to a domain that the API exposes");
		}
	}

	internal interface IDomainCacheUpdatable<T>
	{
		ValueTask<T> UpdateFromCache(DomainCache domainCache, CancellationToken cancellationToken = default);
	}

	internal static class DomainCacheExtensions
	{
		public static async IAsyncEnumerable<DomainSummary> ProcessDomainCache(this IAsyncEnumerable<DomainSummary> domains, DomainCache domainCache)
		{
			await foreach (var domain in domains)
			{
				domainCache.Set(domain.Id, domain.Name);
				yield return domain;
			}
		}

		public static async Task<Domain> ProcessDomainCache(this Task<Domain> domain, DomainCache domainCache)
		{
			var domainValue = await domain.ConfigureAwait(false);
			domainCache.Set(domainValue.Id, domainValue.Name);
			return domainValue;
		}

		public static async IAsyncEnumerable<T> ProcessDomainCache<T>(this IAsyncEnumerable<T> values, DomainCache domainCache, [EnumeratorCancellation]  CancellationToken cancellationToken = default) where T : IDomainCacheUpdatable<T>
		{
			await foreach (var value in values)
				yield return await value.UpdateFromCache(domainCache, cancellationToken).ConfigureAwait(false);
		}
	}
}
