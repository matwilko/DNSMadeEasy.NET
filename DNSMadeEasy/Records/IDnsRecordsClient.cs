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

		ValueTask<DnsRecords> CreateMultiRecordCollection(DomainId domain);

		Task PutRecords(DnsRecords records);
	}
}
