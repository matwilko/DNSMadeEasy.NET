using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DNSMadeEasy
{
	partial struct DomainName
	{
		public DomainName WithSubdomain(string subDomain)
		{
			if (subDomain is null)
				throw new ArgumentNullException(nameof(subDomain));

			if (IsWildcard)
				throw new InvalidOperationException("Wildcard subdomains cannot have further subdomains added - wildcards are only valid as the leftmost label.");

			if (!DomainNameRegex.IsMatch(subDomain))
				throw new ArgumentException($"Subdomain name `{subDomain}` is not valid - {NarrowDomainNameError(subDomain)}");

			if (subDomain.Length + 1 + (domain?.Length ?? 0) > 254)
				throw new ArgumentException($"The given subdomain name(s) `{subDomain}` would create an overall domain name that exceeds the domain name total length limit of 255 characters.");

			return Parse(subDomain + "." + domain);
		}

		internal DomainName WithSubdomain(DomainName subDomain)
		{
			if (IsWildcard)
				throw new InvalidOperationException("Wildcard subdomains cannot have further subdomains added - wildcards are only valid as the leftmost label.");

			if (subDomain.domain == null)
				return this;

			if (!DomainNameRegex.IsMatch(subDomain.domain))
				throw new ArgumentException($"Subdomain name `{subDomain}` is not valid - {NarrowDomainNameError(subDomain.domain)}");

			if (subDomain.domain.Length + 1 + (domain?.Length ?? 0) > 254)
				throw new ArgumentException($"The given subdomain name(s) `{subDomain}` would create an overall domain name that exceeds the domain name total length limit of 255 characters.");

			return Parse(subDomain.domain + "." + domain);
		}

		public DomainName? Tld
		{
			get
			{
				if (domain is null)
					return null;

				var lastLabelStart = domain.LastIndexOf('.', domain.Length - 2);
				return lastLabelStart > 0
					? DomainName.Parse(domain.Substring(lastLabelStart + 1))
					: this;
			}
		}

		public DomainName? Parent
		{
			get
			{
				if (domain is null)
					return null;

				var firstDot = domain.IndexOf('.', StringComparison.Ordinal);
				return firstDot == -1
					? Root
					: Parse(domain.Substring(firstDot + 1));
			}
		}

		public DomainName WithWildcard() => WithSubdomain("*");

		internal string WithoutParent(DomainName parent)
		{
			if (!IsSubdomainOf(parent))
				throw new ArgumentException($"`{this}` is not a subdomain of the parent domain `{parent}`");

			if (parent.domain == null)
				return ToString();

			return domain!.Substring(0, domain.Length - parent.domain.Length - 1);
		}

		public LabelEnumerable Labels => new LabelEnumerable(this);
		public ParentEnumerable Parents => new ParentEnumerable(this);
		public ThisAndParentEnumerable ThisAndParents => new ThisAndParentEnumerable(this);

		[DebuggerDisplay("Labels: {Count,nq}", Type = nameof(LabelEnumerable))]
		[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Enumerable/Enumerator - equality not valid")]
		[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Custom enumerable implementation - not a collection itself")]
		public readonly struct LabelEnumerable : IEnumerable<string>
		{
			private DomainName Name { get; }

			public int Count
			{
				get
				{
					if (Name.domain is null)
						return 0;

					var domain = Name.domain.AsSpan();
					var count  = 0;
					while(true)
					{
						var index = domain.IndexOf('.');
						if (index == -1)
							return count + 1;

						domain = domain.Slice(index + 1);
						count++;
					}
				}
			}

			#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
			public string? this[int i]
			{
				get
				{
					if (Name.domain is null)

						throw new IndexOutOfRangeException("The index was out of the valid range of values");


					var domain = Name.domain.AsSpan();
					for (var j = 0; j < i; j++)
					{
						var index = domain.IndexOf('.');
						if (index == -1)
							throw new IndexOutOfRangeException("The index was out of the valid range of values");

						domain = domain.Slice(index + 1);
					}

					var nextDot = domain.IndexOf('.');
					return nextDot == -1
						? domain.ToString()
						: domain.Slice(0, nextDot).ToString();
				}
			}
			#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations

			public LabelEnumerable(DomainName name)
			{
				Name = name;
			}

			LabelEnumerator GetEnumerator() => new LabelEnumerator(Name);
			IEnumerator<string> IEnumerable<string>.GetEnumerator() => GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Enumerable/Enumerator - equality not valid")]
		public struct LabelEnumerator : IEnumerator<string>
		{
			private string? DomainName { get; }
			private byte Offset { get; set; }
			private byte Length { get; set; }

			public string Current => DomainName!.Substring(Offset, Length);

			public LabelEnumerator(DomainName name)
			{
				DomainName = name.domain;
				Offset = 0;
				Length = 0;
			}

			public bool MoveNext()
			{
				if (DomainName is null)
					return false;

				if (Offset + Length == DomainName.Length)
					return false;

				Offset = Offset == 0 && Length == 0
					? (byte) 0
					: (byte) (Offset + Length + 1);
				var nextDot = DomainName.IndexOf('.', Offset);
				Length = nextDot != -1
					? (byte) (nextDot - Offset)
					: (byte) (DomainName.Length - Offset);
				return true;
			}

			object IEnumerator.Current => Current;

			public void Reset() => throw new NotImplementedException();

			public void Dispose()
			{
			}
		}

		[DebuggerDisplay("Parents: {Count,nq}", Type = nameof(ParentEnumerable))]
		[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Enumerable/Enumerator - equality not valid")]
		[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Custom enumerable implementation - not a collection itself")]
		public readonly struct ParentEnumerable : IEnumerable<DomainName>
		{
			private DomainName Name { get; }

			public int Count => new LabelEnumerable(Name).Count;

			public ParentEnumerable(DomainName name)
			{
				Name = name;
			}

			public ParentEnumerator GetEnumerator() => new ParentEnumerator(Name);
			IEnumerator<DomainName> IEnumerable<DomainName>.GetEnumerator() => GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Enumerable/Enumerator - equality not valid")]
		public struct ParentEnumerator : IEnumerator<DomainName>
		{
			public DomainName Current { get; private set; }

			public ParentEnumerator(DomainName name)
			{
				Current = name;
			}

			public bool MoveNext()
			{
				var newCurrent = Current.Parent;
				if (newCurrent.HasValue)
				{
					Current = newCurrent.Value;
					return true;
				}
				else
				{
					return false;
				}
			}

			object IEnumerator.Current => Current;

			public void Reset() => throw new NotImplementedException();

			public void Dispose()
			{
			}
		}

		[DebuggerDisplay("This and Parents: {Count,nq}", Type = nameof(ThisAndParentEnumerable))]
		[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Enumerable/Enumerator - equality not valid")]
		[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Custom enumerable implementation - not a collection itself")]
		public readonly struct ThisAndParentEnumerable : IEnumerable<DomainName>
		{
			private DomainName Name { get; }

			public int Count => new ParentEnumerable(Name).Count + 1;

			public ThisAndParentEnumerable(DomainName name)
			{
				Name = name;
			}

			public ThisAndParentEnumerator GetEnumerator() => new ThisAndParentEnumerator(Name);
			IEnumerator<DomainName> IEnumerable<DomainName>.GetEnumerator() => GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Enumerable/Enumerator - equality not valid")]
		public struct ThisAndParentEnumerator : IEnumerator<DomainName>
		{
			private bool Started { get; set; }
			public DomainName Current { get; private set; }

			public ThisAndParentEnumerator(DomainName name)
			{
				Started = false;
				Current = name;
			}

			public bool MoveNext()
			{
				if (!Started)
				{
					Started = true;
					return true;
				}

				var newCurrent = Current.Parent;
				if (newCurrent.HasValue)
				{
					Current = newCurrent.Value;
					return true;
				}
				else
				{
					return false;
				}
			}

			object IEnumerator.Current => Current;

			public void Reset() => throw new NotImplementedException();

			public void Dispose()
			{
			}
		}
	}
}