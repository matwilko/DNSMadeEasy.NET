using System;
using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	public sealed class ANameRecord : DnsRecord
	{
		public  DomainName? TargetDomain          { get; }
		private bool        TargetDomainWasRooted { get; }
		public  IPv4?       TargetIPv4            { get; }
		public  IPv6?       TargetIpv6            { get; }

		[JsonConstructor]
		internal ANameRecord(string value, DnsRecordId id, DomainName name, RecordSource source, DomainId sourceId, TimeToLive ttl, GlobalTrafficDirectorLocation? gtdLocation) : base(id, name, source, sourceId, ttl, gtdLocation)
		{
			if (DomainName.TryParse(value, out var domainName))
			{
				TargetDomain          = domainName;
				TargetDomainWasRooted = value.EndsWith(".", StringComparison.OrdinalIgnoreCase);
			}
			else if (IPv4.TryParse(value, out var ipv4))
			{
				TargetIPv4 = ipv4;
			}
			else if (IPv6.TryParse(value, out var ipv6))
			{
				TargetIpv6 = ipv6;
			}
			else
			{
				throw new FormatException($"Unrecognised value for an ANAME record: `{value}`. Expecting a domain name, IPv4 or IPv6 address.");
			}
		}

		public ANameRecord(DomainName targetDomain, DnsRecordId id, DomainName name, DomainId parentDomainId, TimeToLive timeToLive, RecordSource recordSource = default, GlobalTrafficDirectorLocation? globalTrafficDirectorLocation = null) : base(id, name, parentDomainId, timeToLive, recordSource, globalTrafficDirectorLocation)
		{
			TargetDomain = targetDomain;
		}

		public ANameRecord(IPv4 targetIPv4, DnsRecordId id, DomainName name, DomainId parentDomainId, TimeToLive timeToLive, RecordSource recordSource = default, GlobalTrafficDirectorLocation? globalTrafficDirectorLocation = null) : base(id, name, parentDomainId, timeToLive, recordSource, globalTrafficDirectorLocation)
		{
			TargetIPv4 = targetIPv4;
		}

		public ANameRecord(IPv6 targetIpv6, DnsRecordId id, DomainName name, DomainId parentDomainId, TimeToLive timeToLive, RecordSource recordSource = default, GlobalTrafficDirectorLocation? globalTrafficDirectorLocation = null) : base(id, name, parentDomainId, timeToLive, recordSource, globalTrafficDirectorLocation)
		{
			TargetIpv6 = targetIpv6;
		}

		private protected override DnsRecord UpdateFromCache(DomainName newName, DomainName parentDomainName)
		{
			var newValue = TargetIPv4?.ToString()
			            ?? TargetIpv6.ToString()
			            ?? (TargetDomainWasRooted
				               ? parentDomainName.WithSubdomain(TargetDomain!.Value).ToString()
				               : TargetDomain.ToString());

			return new ANameRecord(newValue!, Id, newName, Source, ParentDomainId, TimeToLive, GlobalTrafficDirectorLocation);
		}
	}
}