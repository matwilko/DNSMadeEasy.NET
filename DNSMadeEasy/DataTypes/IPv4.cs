using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public readonly struct IPv4 : IEquatable<IPv4>
	{
		private readonly IPAddress ip;

		public IPv4(string ip)
		{
			if (ip is null)
				throw new ArgumentNullException(nameof(ip));

			if (!IPAddress.TryParse(ip, out var ipAddress))
				throw new ArgumentException("Not an IP Address", nameof(ip));

			if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
				throw new ArgumentException("Not an IPv4 address", nameof(ip));

			this.ip = ipAddress;
		}

		public IPv4(IPAddress ip)
		{
			if (ip is null)
				throw new ArgumentNullException(nameof(ip));

			if (ip.AddressFamily != AddressFamily.InterNetwork)
				throw new ArgumentException("Not an IPv4 address", nameof(ip));

			this.ip = ip;
		}

		[JsonParseMethod(writeMethod: nameof(ToString))]
		public static IPv4 Parse(string str) => new IPv4(str);

		public static bool TryParse(string str, out IPv4? ip)
		{
			if (str is null || !IPAddress.TryParse(str, out var ipAddress) || ipAddress.AddressFamily != AddressFamily.InterNetwork)
			{
				ip = null;
				return false;
			}

			ip = new IPv4(ipAddress);
			return true;
		}

		public static bool operator ==(IPv4 left, IPv4 right) => left.Equals(right);
		public static bool operator !=(IPv4 left, IPv4 right) => !left.Equals(right);

		public bool Equals(IPv4 other) => ReferenceEquals(ip, other.ip) || ip.Equals(other.ip);

		public override bool Equals(object? obj) => obj is IPv4 other && Equals(other);

		public override int GetHashCode() => ToIPAddress().GetHashCode();

		public override string ToString() => ToIPAddress().ToString();

		public static implicit operator IPAddress(IPv4 ip) => ip.ToIPAddress();
		public static explicit operator IPv4(IPAddress ip) => new IPv4(ip);

		public static IPv4 FromIPAddress(IPAddress ip) => new IPv4(ip);
		public IPAddress ToIPAddress() => ip ?? IPAddress.Any;

		private string DebuggerDisplay => ToString();

		public static IEqualityComparer<IPv4> EqualityComparer { get; } = new IpEqualityComparer();

		private sealed class IpEqualityComparer : IEqualityComparer<IPv4>
		{
			public bool Equals(IPv4 x, IPv4 y) => x.Equals(y);
			public int GetHashCode(IPv4 obj) => obj.GetHashCode();
		}
	}
}