using System;
using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	public sealed class SRVRecord : DnsRecord
	{
		public  string     Service         { get; }
		public  string     Protocol        { get; }
		public  Priority   Priority        { get; }
		public  Weight     Weight          { get; }
		public  Port       Port            { get; }
		public  DomainName Target          { get; }
		private bool       TargetWasRooted { get; }

		public SRVRecord(Priority priority, Weight weight, Port port, DomainName target, DnsRecordId id, DomainName name, DomainId parentDomainId, TimeToLive timeToLive, RecordSource recordSource = default, GlobalTrafficDirectorLocation? globalTrafficDirectorLocation = null) : base(id, name, parentDomainId, timeToLive, recordSource, globalTrafficDirectorLocation)
		{
			Priority = priority;
			Weight   = weight;
			Port     = port;
			Target   = target;
			Service  = name.Labels[0]!;
			Protocol = name.Labels[1]!;
		}

		[JsonConstructor]
		internal SRVRecord(DnsRecordId id, DomainName name, RecordSource source, DomainId sourceId, TimeToLive ttl, GlobalTrafficDirectorLocation? gtdLocation, Priority priority, Weight weight, Port port, string value) : base(id, name, source, sourceId, ttl, gtdLocation)
		{
			Priority        = priority;
			Weight          = weight;
			Port            = port;
			Target          = DomainName.Parse(value);
			TargetWasRooted = value.EndsWith(".", StringComparison.OrdinalIgnoreCase);
			Service         = name.Labels[0]!;
			Protocol        = name.Labels[1]!;
		}

		private protected override DnsRecord UpdateFromCache(DomainName newName, DomainName parentDomainName)
		{
			var newTarget = !TargetWasRooted
				? parentDomainName.WithSubdomain(Target)
				: Target;

			return new SRVRecord(Id, newName, Source, ParentDomainId, TimeToLive, GlobalTrafficDirectorLocation, Priority, Weight, Port, newTarget.ToString());
		}
	}
}