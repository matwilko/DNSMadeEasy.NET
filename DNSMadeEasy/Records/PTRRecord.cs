using System;
using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	public sealed class PTRRecord : DnsRecord
	{
		public DomainName Target { get; }
		private bool TargetWasRooted { get; }

		[JsonConstructor]
		internal PTRRecord(DnsRecordId id, DomainName name, RecordSource source, DomainId sourceId, TimeToLive ttl, GlobalTrafficDirectorLocation? gtdLocation, string value) : base(id, name, source, sourceId, ttl, gtdLocation)
		{
			Target          = DomainName.Parse(value);
			TargetWasRooted = value.EndsWith(".", StringComparison.OrdinalIgnoreCase);
		}

		public PTRRecord(DomainName target, DnsRecordId id, DomainName name, DomainId parentDomainId, TimeToLive timeToLive, RecordSource recordSource = default, GlobalTrafficDirectorLocation? globalTrafficDirectorLocation = null) : base(id, name, parentDomainId, timeToLive, recordSource, globalTrafficDirectorLocation)
		{
			Target = target;
		}

		private protected override DnsRecord UpdateFromCache(DomainName newName, DomainName parentDomainName)
		{
			var newTarget = !TargetWasRooted
				? parentDomainName.WithSubdomain(Target)
				: Target;

			return new PTRRecord(Id, newName, Source, ParentDomainId, TimeToLive, GlobalTrafficDirectorLocation, newTarget.ToString());
		}
	}
}