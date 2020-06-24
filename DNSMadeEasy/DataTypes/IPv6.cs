using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public readonly struct IPv6 : IEquatable<IPv6>
	{
		private readonly IPAddress ip;

		public IPv6(string ip)
		{
			if (ip is null)
				throw new ArgumentNullException(nameof(ip));

			if (!IPAddress.TryParse(ip, out var ipAddress))
				throw new ArgumentException("Not an IP Address", nameof(ip));

			if (ipAddress.AddressFamily != AddressFamily.InterNetworkV6)
				throw new ArgumentException("Not an IPv6 address", nameof(ip));

			this.ip = ipAddress;
		}

		public IPv6(IPAddress ip)
		{
			if (ip is null)
				throw new ArgumentNullException(nameof(ip));

			if (ip.AddressFamily != AddressFamily.InterNetworkV6)
				throw new ArgumentException("Not an IPv6 address", nameof(ip));

			this.ip = ip;
		}

		[JsonParseMethod]
		public static IPv6 Parse(string str) => new IPv6(str);
		public static bool TryParse(string str, out IPv6? ip)
		{
			if (str is null || !IPAddress.TryParse(str, out var ipAddress) || ipAddress.AddressFamily != AddressFamily.InterNetworkV6)
			{
				ip = null;
				return false;
			}

			ip = new IPv6(ipAddress);
			return true;
		}

		public static bool operator ==(IPv6 left, IPv6 right) => left.Equals(right);
		public static bool operator !=(IPv6 left, IPv6 right) => !left.Equals(right);

		public bool Equals(IPv6 other) => ReferenceEquals(ip, other.ip) || ip.Equals(other.ip);

		public override bool Equals(object? obj) => obj is IPv6 other && Equals(other);

		public override int GetHashCode() => ToIPAddress().GetHashCode();

		public override string ToString() => ToIPAddress().ToString();

		public static implicit operator IPAddress(IPv6 ip) => ip.ToIPAddress();
		public static explicit operator IPv6(IPAddress ip) => new IPv6(ip);
		public static IPv6 FromIPAddress(IPAddress ip) => new IPv6(ip);
		public IPAddress ToIPAddress() => ip ?? IPAddress.IPv6Any;

		private string DebuggerDisplay => ToString();

		public static IEqualityComparer<IPv6> EqualityComparer { get; } = new IpEqualityComparer();

		private sealed class IpEqualityComparer : IEqualityComparer<IPv6>
		{
			public bool Equals(IPv6 x, IPv6 y) => x.Equals(y);
			public int GetHashCode(IPv6 obj) => obj.GetHashCode();
		}
	}
}