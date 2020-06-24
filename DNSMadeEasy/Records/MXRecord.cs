using System;
using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	public sealed class MXRecord : DnsRecord
	{
		public  MxLevel    MxLevel         { get; }
		public  DomainName Target          { get; }
		private bool       TargetWasRooted { get; }

		[JsonConstructor]
		internal MXRecord(DnsRecordId id, DomainName name, RecordSource source, DomainId sourceId, TimeToLive ttl, GlobalTrafficDirectorLocation? gtdLocation, MxLevel mxLevel, string value) : base(id, name, source, sourceId, ttl, gtdLocation)
		{
			MxLevel         = mxLevel;
			Target          = DomainName.Parse(value);
			TargetWasRooted = value.EndsWith(".", StringComparison.OrdinalIgnoreCase);
		}

		public MXRecord(MxLevel mxLevel, DomainName target, DnsRecordId id, DomainName name, DomainId parentDomainId, TimeToLive timeToLive, RecordSource recordSource = default, GlobalTrafficDirectorLocation? globalTrafficDirectorLocation = null) : base(id, name, parentDomainId, timeToLive, recordSource, globalTrafficDirectorLocation)
		{
			MxLevel = mxLevel;
			Target  = target;
		}

		private protected override DnsRecord UpdateFromCache(DomainName newName, DomainName parentDomainName)
		{
			var newTarget = !TargetWasRooted
				? parentDomainName.WithSubdomain(Target)
				: Target;

			return new MXRecord(Id, newName, Source, ParentDomainId, TimeToLive, GlobalTrafficDirectorLocation, MxLevel, newTarget.ToString());
		}
	}
}
