using System.Threading;
using System.Threading.Tasks;

namespace DNSMadeEasy
{
	public abstract class DnsRecord : IDomainCacheUpdatable<DnsRecord>
	{
		public DnsRecordId                    Id                            { get; }
		public DomainName                     Name                          { get; }
		public RecordSource                   Source                        { get; }
		public DomainId                       ParentDomainId                { get; }
		public TimeToLive                     TimeToLive                    { get; }
		public GlobalTrafficDirectorLocation? GlobalTrafficDirectorLocation { get; }

		private protected DnsRecord(DnsRecordId id, DomainName name, RecordSource source, DomainId sourceId, TimeToLive ttl, GlobalTrafficDirectorLocation? gtdLocation)
		{
			Id                            = id;
			Name                          = name;
			Source                        = source;
			ParentDomainId                = sourceId;
			TimeToLive                    = ttl;
			GlobalTrafficDirectorLocation = gtdLocation;
		}

		private protected DnsRecord(DnsRecordId id, DomainName name, DomainId parentDomainId, TimeToLive timeToLive, RecordSource recordSource = default, GlobalTrafficDirectorLocation? globalTrafficDirectorLocation = null)
		{
			Id                            = id;
			Name                          = name;
			Source                        = recordSource;
			ParentDomainId                = parentDomainId;
			TimeToLive                    = timeToLive;
			GlobalTrafficDirectorLocation = globalTrafficDirectorLocation;
		}

		private protected abstract DnsRecord UpdateFromCache(DomainName newName, DomainName parentDomainName);

		async ValueTask<DnsRecord> IDomainCacheUpdatable<DnsRecord>.UpdateFromCache(DomainCache domainCache, CancellationToken cancellationToken)
		{
			var parentDomain = await domainCache.EnsureAndGet(ParentDomainId, cancellationToken).ConfigureAwait(false);
			var newName      = parentDomain.WithSubdomain(Name);
			return UpdateFromCache(newName, parentDomain);
		}
	}
}