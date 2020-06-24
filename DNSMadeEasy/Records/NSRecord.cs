using System;
using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	public sealed class NSRecord : DnsRecord
	{
		public  DomainName NameServer          { get; }
		private bool       NameServerWasRooted { get; }

		[JsonConstructor]
		internal NSRecord(DnsRecordId id, DomainName name, RecordSource source, DomainId sourceId, TimeToLive ttl, GlobalTrafficDirectorLocation? gtdLocation, string value) : base(id, name, source, sourceId, ttl, gtdLocation)
		{
			NameServer          = DomainName.Parse(value);
			NameServerWasRooted = value.EndsWith(".", StringComparison.OrdinalIgnoreCase);
		}

		public NSRecord(DomainName nameServer, DnsRecordId id, DomainName name, DomainId parentDomainId, TimeToLive timeToLive, RecordSource recordSource = default, GlobalTrafficDirectorLocation? globalTrafficDirectorLocation = null) : base(id, name, parentDomainId, timeToLive, recordSource, globalTrafficDirectorLocation)
		{
			NameServer = nameServer;
		}

		private protected override DnsRecord UpdateFromCache(DomainName newName, DomainName parentDomainName)
		{
			var nameServer = NameServerWasRooted
				? NameServer
				: parentDomainName.WithSubdomain(NameServer.ToString());

			return new NSRecord(Id, newName, Source, ParentDomainId, TimeToLive, GlobalTrafficDirectorLocation, nameServer.ToString());
		}
	}
}