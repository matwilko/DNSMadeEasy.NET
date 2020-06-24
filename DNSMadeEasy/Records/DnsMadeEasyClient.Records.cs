using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DNSMadeEasy
{
	partial class DnsMadeEasyClient : IDnsRecordsClient
	{
		public IAsyncEnumerable<DnsRecord> GetAllRecords(DomainId domain, CancellationToken cancellationToken = default)
		{
			return Get<DnsRecord>($"dns/managed/{domain}/records", hasApiResponse: true, cancellationToken).ProcessDomainCache(DomainCache, cancellationToken);
		}

		public async IAsyncEnumerable<DnsRecord> GetAllRecords(DomainId domain, DomainName name, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			var parentDomainName = await DomainCache.EnsureAndGet(domain, cancellationToken).ConfigureAwait(false);
			var subDomain = name.WithoutParent(parentDomainName);

			await foreach (var record in Get<DnsRecord>($"dns/managed/{domain}/records?recordName={subDomain}").ProcessDomainCache(DomainCache, cancellationToken).ConfigureAwait(false))
				yield return record;
		}

		public async IAsyncEnumerable<TRecord> GetAllRecords<TRecord>(DomainId domain, [EnumeratorCancellation] CancellationToken cancellationToken = default) where TRecord : DnsRecord
		{
			var type             = GetTypeString<TRecord>();

			await foreach (var record in Get<DnsRecord>($"dns/managed/{domain}/records?type={type}").ProcessDomainCache(DomainCache, cancellationToken).ConfigureAwait(false))
			{
				if (record is not TRecord typedRecord)
					throw new InvalidOperationException("DNSMadeEasy returned records of the wrong type");

				yield return typedRecord;
			}
		}

		public async IAsyncEnumerable<TRecord> GetAllRecords<TRecord>(DomainId domain, DomainName name, [EnumeratorCancellation] CancellationToken cancellationToken = default) where TRecord : DnsRecord
		{
			var type             = GetTypeString<TRecord>();
			var parentDomainName = await DomainCache.EnsureAndGet(domain, cancellationToken).ConfigureAwait(false);
			var subDomain        = name.WithoutParent(parentDomainName);

			await foreach (var record in Get<DnsRecord>($"dns/managed/{domain}/records?recordName={subDomain}&type={type}").ProcessDomainCache(DomainCache, cancellationToken).ConfigureAwait(false))
			{
				if (record is not TRecord typedRecord)
					throw new InvalidOperationException("DNSMadeEasy returned records of the wrong type");

				yield return typedRecord;
			}
		}

		public async ValueTask<DnsRecords> CreateMultiRecordCollection(DomainId domain)
		{
			var domainName = await DomainCache.EnsureAndGet(domain).ConfigureAwait(false);
			return new DnsRecords(domain, domainName);
		}

		public async Task PutRecords(DnsRecords records)
		{
			if (records is null)
				throw new ArgumentNullException(nameof(records));

			if (records.CreateList.IsEmpty && records.UpdateList.IsEmpty)
				return;

			var creationTask = !records.CreateList.IsEmpty
				? Post($"dns/managed/{records.ParentDomainId}/records/createMulti", records.CreateList)
				: Task.CompletedTask;

			var updateTask = !records.UpdateList.IsEmpty
				? Put($"dns/managed/{records.ParentDomainId}/records/updateMulti", records.UpdateList)
				: Task.CompletedTask;

			await Task.WhenAll(creationTask, updateTask).ConfigureAwait(false);
		}

		private static string GetTypeString<TRecord>() where TRecord : DnsRecord
		{
			if (typeof(TRecord) == typeof(DnsRecord))
				throw new InvalidOperationException("The base type `DnsRecord` cannot be used to filter");
			else if (typeof(TRecord) == typeof(ARecord))
				return "A";
			else if (typeof(TRecord) == typeof(AAAARecord))
				return "AAAA";
			else if (typeof(TRecord) == typeof(ANameRecord))
				return "ANAME";
			else if (typeof(TRecord) == typeof(CNameRecord))
				return "CNAME";
			else if (typeof(TRecord) == typeof(HttpRedirectionRecord))
				return "HTTPRED";
			else if (typeof(TRecord) == typeof(MXRecord))
				return "MX";
			else if (typeof(TRecord) == typeof(NSRecord))
				return "NS";
			else if (typeof(TRecord) == typeof(PTRRecord))
				return "PTR";
			else if (typeof(TRecord) == typeof(SRVRecord))
				return "SRV";
			else if (typeof(TRecord) == typeof(TXTRecord))
				return "TXT";
			else if (typeof(TRecord) == typeof(SPFRecord))
				return "SPF";
			else
				throw new InvalidOperationException("Unreocgnised record type");
		}
	}
}
