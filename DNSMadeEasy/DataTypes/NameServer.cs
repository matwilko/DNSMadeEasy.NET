using System;
using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	public readonly struct NameServer : IEquatable<NameServer>
	{
		public DomainName FullyQualifiedDomainName { get; }
		public IPv4 IPv4 { get; }
		public IPv6 IPv6 { get; }

		[JsonConstructor]
		public NameServer(DomainName fqdn, IPv4 ipv4, IPv6 ipv6)
		{
			FullyQualifiedDomainName = fqdn;
			IPv4 = ipv4;
			IPv6 = ipv6;
		}

		public bool Equals(NameServer other) => FullyQualifiedDomainName.Equals(other.FullyQualifiedDomainName);
		public override bool Equals(object? obj) => obj is NameServer other && Equals(other);
		public override int GetHashCode() => FullyQualifiedDomainName.GetHashCode();
		public static bool operator ==(NameServer left, NameServer right) => left.Equals(right);
		public static bool operator !=(NameServer left, NameServer right) => !left.Equals(right);
	}
}