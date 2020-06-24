using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	public sealed class AAAARecord : DnsRecord
	{
		public IPv6  Target                    { get; }

		public AAAARecord(IPv6 target, DnsRecordId id, DomainName name, DomainId parentDomainId, TimeToLive timeToLive, RecordSource recordSource = default, GlobalTrafficDirectorLocation? globalTrafficDirectorLocation = null) : base(id, name, parentDomainId, timeToLive, recordSource, globalTrafficDirectorLocation)
		{
			Target                    = target;
		}

		[JsonConstructor]
		internal AAAARecord(DnsRecordId id, DomainName name, DomainId sourceId, TimeToLive ttl, RecordSource source, GlobalTrafficDirectorLocation? gtdLocation, IPv6 value) : base(id, name, source, sourceId, ttl, gtdLocation)
		{
			Target                    = value;
		}

		private protected override DnsRecord UpdateFromCache(DomainName newName, DomainName parentDomainName)
		{
			return new AAAARecord(Id, newName, ParentDomainId, TimeToLive, Source, GlobalTrafficDirectorLocation, Target);
		}
	}
}