using System;
using System.Diagnostics;

namespace DNSMadeEasy
{
	partial struct DomainName
	{
		public bool IsWildcard => domain?[0] == '*';
		public bool IsTld => domain?.IndexOf('.', StringComparison.Ordinal) == -1;

		public bool IsSubdomainOf(DomainName otherDomain)
		{
			if (this.domain is null)
				return false;

			if (otherDomain.domain is null)
				return true;

			if (otherDomain.IsWildcard)
				return false;

			return otherDomain.domain.Length < this.domain.Length
			       && this.domain.EndsWith(otherDomain.domain, StringComparison.OrdinalIgnoreCase)
			       && this.domain[this.domain.Length - otherDomain.domain.Length - 1] == '.';
		}

		public bool MatchesWildcardOf(DomainName otherDomain)
		{
			if (!otherDomain.IsWildcard)
				throw new ArgumentException("Given domain is not a wildcard domain name");

			Debug.Assert(otherDomain.Parent != null, "otherDomain.Parent != null");
			#pragma warning disable CS8629 // Nullable value type may be null. Parent is only null for Root domain name, and we've already excluded that with the wildcard check
			return this.Labels.Count == otherDomain.Labels.Count
				   && this.IsSubdomainOf(otherDomain.Parent.Value);
			#pragma warning restore CS8629
		}
	}
}
