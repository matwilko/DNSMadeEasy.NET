using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	public sealed class ARecord : DnsRecord
	{
		public IPv4    Target                    { get; }
		public bool    IsDynamicDnsEnabled       { get; }
		public string? DynamicDnsPassword        { get; }
		public bool    IsSystemMonitoringEnabled { get; }
		public bool    IsFailoverEnabled         { get; }
		public bool    IsInFailedState           { get; }

		public ARecord(IPv4 target, DnsRecordId id, DomainName name, DomainId parentDomainId, TimeToLive timeToLive, RecordSource recordSource = default, bool isDynamicDnsEnabled = false, string? dynamicDnsPassword = null, GlobalTrafficDirectorLocation? globalTrafficDirectorLocation = null, bool isSystemMonitoringEnabled = false, bool isFailoverEnabled = false, bool isInFailedState = false) : base(id, name, parentDomainId, timeToLive, recordSource, globalTrafficDirectorLocation)
		{
			Target                    = target;
			IsDynamicDnsEnabled       = isDynamicDnsEnabled;
			DynamicDnsPassword        = dynamicDnsPassword;
			IsSystemMonitoringEnabled = isSystemMonitoringEnabled;
			IsFailoverEnabled         = isFailoverEnabled;
			IsInFailedState           = isInFailedState;
		}

		[JsonConstructor]
		internal ARecord(IPv4 value, DnsRecordId id, DomainName name, DomainId sourceId, TimeToLive ttl, bool monitor, bool failover, bool failed, bool dynamicDns, RecordSource source, string? password, GlobalTrafficDirectorLocation? gtdLocation) : base(id, name, source, sourceId, ttl, gtdLocation)
		{
			Target                    = value;
			IsDynamicDnsEnabled       = dynamicDns;
			DynamicDnsPassword        = password;
			IsSystemMonitoringEnabled = monitor;
			IsFailoverEnabled         = failover;
			IsInFailedState           = failed;
		}

		private protected override DnsRecord UpdateFromCache(DomainName newName, DomainName parentDomainName)
		{
			return new ARecord(Target, Id, newName, ParentDomainId, TimeToLive, IsSystemMonitoringEnabled, IsFailoverEnabled, IsInFailedState, IsDynamicDnsEnabled, Source, DynamicDnsPassword, GlobalTrafficDirectorLocation);
		}
	}
}