using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	public sealed class SPFRecord : DnsRecord
	{
		public string Value { get; }

		[JsonConstructor]
		internal SPFRecord(DnsRecordId id, DomainName name, RecordSource source, DomainId sourceId, TimeToLive ttl, GlobalTrafficDirectorLocation? gtdLocation, string value) : base(id, name, source, sourceId, ttl, gtdLocation)
		{
			Value = value;
		}

		public SPFRecord(string value, DnsRecordId id, DomainName name, DomainId parentDomainId, TimeToLive timeToLive, RecordSource recordSource = default, GlobalTrafficDirectorLocation? globalTrafficDirectorLocation = null) : base(id, name, parentDomainId, timeToLive, recordSource, globalTrafficDirectorLocation)
		{
			Value = value;
		}

		private protected override DnsRecord UpdateFromCache(DomainName newName, DomainName parentDomainName)
		{
			return new SPFRecord(Id, newName, Source, ParentDomainId, TimeToLive, GlobalTrafficDirectorLocation, Value);
		}
	}
}