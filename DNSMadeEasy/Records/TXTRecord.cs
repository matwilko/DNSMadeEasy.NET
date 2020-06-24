using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	public sealed class TXTRecord : DnsRecord
	{
		public string Value { get; }

		[JsonConstructor]
		internal TXTRecord(DnsRecordId id, DomainName name, RecordSource source, DomainId sourceId, TimeToLive ttl, GlobalTrafficDirectorLocation? gtdLocation, string value) : base(id, name, source, sourceId, ttl, gtdLocation)
		{
			Value = value[0] == '"'
				? value.Substring(1, value.Length - 2)
				: value;
		}

		public TXTRecord(string value, DnsRecordId id, DomainName name, DomainId parentDomainId, TimeToLive timeToLive, RecordSource recordSource = default, GlobalTrafficDirectorLocation? globalTrafficDirectorLocation = null) : base(id, name, parentDomainId, timeToLive, recordSource, globalTrafficDirectorLocation)
		{
			Value = value;
		}

		private protected override DnsRecord UpdateFromCache(DomainName newName, DomainName parentDomainName)
		{
			return new TXTRecord(Id, newName, Source, ParentDomainId, TimeToLive, GlobalTrafficDirectorLocation, Value);
		}
	}
}