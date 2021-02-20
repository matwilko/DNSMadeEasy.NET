using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DNSMadeEasy
{
	public interface IDnsRecordsClient
	{
		IAsyncEnumerable<DnsRecord> GetAllRecords(DomainId domain, CancellationToken cancellationToken = default);
		IAsyncEnumerable<DnsRecord> GetAllRecords(DomainId domain, DomainName name, CancellationToken cancellationToken = default);

		IAsyncEnumerable<TRecord> GetAllRecords<TRecord>(DomainId domain, CancellationToken cancellationToken = default) where TRecord : DnsRecord;
		IAsyncEnumerable<TRecord> GetAllRecords<TRecord>(DomainId domain, DomainName name, CancellationToken cancellationToken = default) where TRecord : DnsRecord;

		Task DeleteRecord(DnsRecord record, CancellationToken cancellationToken = default);
		Task DeleteRecords(IEnumerable<DnsRecord> records, CancellationToken cancellationToken = default);

		ValueTask<DnsRecords> CreateMultiRecordCollection(DomainId domain, CancellationToken cancellationToken = default);

		Task PutRecords(DnsRecords records, CancellationToken cancellationToken = default);

		public Task CreateARecord(DomainId parentDomainId, DomainName name, IPv4 target, TimeToLive timeToLive, string? dynamicDnsPassword = null, GlobalTrafficDirectorLocation globalTrafficDirectorLocation = default, bool isSystemMonitoringEnabled = false, bool isFailoverEnabled = false, CancellationToken cancellationToken = default);
		public Task CreateCNameRecord(DomainId parentDomainId, DomainName name, DomainName target, TimeToLive timeToLive, GlobalTrafficDirectorLocation globalTrafficDirectorLocation = default, CancellationToken cancellationToken = default);
		public Task CreateTXTRecord(DomainId parentDomainId, DomainName name, string value, TimeToLive timeToLive, GlobalTrafficDirectorLocation globalTrafficDirectorLocation = default, CancellationToken cancellationToken = default);
	}
}
