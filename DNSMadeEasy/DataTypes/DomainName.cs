using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	[DebuggerDisplay("{" + nameof(domain) + ",nq}", Type = nameof(DomainName))]
	public readonly partial struct DomainName : IEquatable<DomainName>
	{
		private readonly string? domain;

		private const string LabelRegex = @"((?!-)[a-zA-Z0-9\-_]{1,63}(?<!-))";

		private static Regex DomainNameRegex { get; } = new Regex($@"^(\*|{LabelRegex})(\.{LabelRegex})*$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

		private DomainName(string domain)
		{
			this.domain = domain;
		}

		[JsonParseMethod(nameof(ToString))]
		public static DomainName Parse(string domain)
		{
			if (domain is null)
				throw new ArgumentNullException(nameof(domain));

			return domain switch
			{
				"." => Root,
				"" => Root,
				_ when domain.EndsWith(".", StringComparison.Ordinal) && !domain.EndsWith("..", StringComparison.Ordinal) => DomainName.Parse(domain.Substring(0, domain.Length - 1)),
				_ when domain.Length >= 254 => throw new ArgumentException($"Domain name `{domain}` is not valid - domain names can only have a maximum length of 253 characters"),
				_ when !DomainNameRegex.IsMatch(domain) => throw new ArgumentException($"Domain name `{domain}` is not valid - {NarrowDomainNameError(domain)}", nameof(domain)),
				_ when domain.Length <= LongestKnownTld && TryFindTld(domain, out var domainName) => domainName,
				_ => new DomainName(domain)
			};
		}

		public static bool TryParse(string domain, out DomainName? domainName)
		{
			if (domain is null)
			{
				domainName = null;
				return false;
			}

			if (domain == "." || domain.Length == 0)
			{
				domainName = Root;
				return true;
			}
			else if (domain.EndsWith(".", StringComparison.Ordinal) && !domain.EndsWith("..", StringComparison.Ordinal))
			{
				return DomainName.TryParse(domain.Substring(0, domain.Length - 1), out domainName);
			}
			else if (domain.Length >= 254 || !DomainNameRegex.IsMatch(domain))
			{
				domainName = null;
				return false;
			}
			else if (domain.Length <= LongestKnownTld && TryFindTld(domain, out var tld ))
			{
				domainName = tld;
				return true;
			}
			else
			{
				domainName = new DomainName(domain);
				return true;
			}
		}

		private static string NarrowDomainNameError(string domain)
		{
			if (domain[0] == '.')
				return "The domain name cannot start with an empty label";

			for (var i = 0; i < domain.Length; i++)
			{
				var chr = domain[i];
				// * (42)
				// - (45)
				// . (46)
				// 0-9 (48-57), A-Z (65-90)
				// _ (95)
				// a-z (97-122)
				if (chr < 42 || chr == 43 || chr == 44 || chr == 47 || (chr > 57 && chr < 65) || (chr > 90 && chr < 95) || chr == 96 || chr > 122)
					return $"The character `{chr}` at position {i} is not valid in a domain name - only alphanumeric ASCII characters, *, -, . and _ are valid.";
			}

			for (var i = 1; i < domain.Length; i++)
				if (domain[i] == '*')
					return $"A wildcard can only appear as the first label - it is invalid anywhere else in the domain name - found a `*` at character {i}";

			{
				var labelStart = 0;
				var labelEnd = domain.IndexOf('.', labelStart);
				do
				{
					var labelLength = labelEnd - labelStart;
					if (labelLength == 0)
						return $"Domain name labels cannot be zero length - the label starting at character {labelStart} has zero length";

					if (labelLength > 63)
						return $"Domain name labels cannot be greater than 63 characters long - the label `{domain.Substring(labelStart, labelLength)}` starting at character {labelStart} is {labelLength} characters.";

					labelStart = labelEnd + 1;
					var newLabelEnd = domain.IndexOf('.', labelStart);
					labelEnd = newLabelEnd == -1
						? domain.Length
						: newLabelEnd;

				} while (labelEnd != domain.Length);


			}

			return "Unknown issue";
		}

		private static bool TryFindTld(string domain, out DomainName domainName)
		{
			var index = Array.BinarySearch(KnownTlds, domain, StringComparer.OrdinalIgnoreCase);
			if (index >= 0)
			{
				domainName = new DomainName(KnownTlds[index]);
				return true;
			}
			else
			{
				domainName = default;
				return false;
			}
		}

		public static explicit operator string(DomainName domain) => domain.ToString();
		public override string ToString() => domain ?? ".";
		public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(ToString());
		public static DomainName Root => default(DomainName);

		#region Equality

		public bool Equals(DomainName other) => string.Equals(domain, other.domain, StringComparison.OrdinalIgnoreCase);
		public override bool Equals(object? obj) => obj is DomainName other && Equals(other);

		public static bool operator ==(DomainName left, DomainName right) => left.Equals(right);
		public static bool operator !=(DomainName left, DomainName right) => !left.Equals(right);

		public static IEqualityComparer<DomainName> EqualityComparer { get; } = new DomainEqualityComparer();

		private sealed class DomainEqualityComparer : IEqualityComparer<DomainName>
		{
			public bool Equals(DomainName x, DomainName y) => x.Equals(y);
			public int GetHashCode(DomainName obj) => obj.GetHashCode();
		}

		#endregion
	}
}