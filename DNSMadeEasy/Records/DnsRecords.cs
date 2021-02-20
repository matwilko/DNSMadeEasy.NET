using System;
using System.Collections.Immutable;

namespace DNSMadeEasy
{
	public sealed class DnsRecords
	{
		internal DomainId              ParentDomainId { get; }
		private  DomainName            ParentDomain   { get; }
		internal ImmutableList<object> CreateList     { get; }
		internal ImmutableList<object> UpdateList     { get; }

		internal DnsRecords(DomainId parentDomainId, DomainName parentDomain)
			: this(parentDomainId, parentDomain, ImmutableList<object>.Empty, ImmutableList<object>.Empty)
		{
		}

		public DnsRecords(DomainId parentDomainId, DomainName parentDomain, ImmutableList<object> createList, ImmutableList<object> updateList)
		{
			ParentDomainId = parentDomainId;
			ParentDomain   = parentDomain;
			CreateList     = createList;
			UpdateList     = updateList;
		}

		private DnsRecords AddCreate(object obj) => new DnsRecords(ParentDomainId, ParentDomain, CreateList.Add(obj), UpdateList);
		private DnsRecords AddUpdate(object obj) => new DnsRecords(ParentDomainId, ParentDomain, CreateList, UpdateList.Add(obj));

		public DnsRecords CreateARecord(DomainName name, IPv4 target, TimeToLive timeToLive, string? dynamicDnsPassword = null, GlobalTrafficDirectorLocation globalTrafficDirectorLocation = default, bool isSystemMonitoringEnabled = false, bool isFailoverEnabled = false)
		{
			if (!name.IsSubdomainOf(ParentDomain))
				throw new ArgumentException($"The specified domain `{name}` is not a subdomain of the parent `{ParentDomain}`", nameof(name));

			return AddCreate(new
			{
				type        = "A",
				name        = name.WithoutParent(ParentDomain),
				value       = target,
				ttl         = timeToLive,
				dynamicDns  = dynamicDnsPassword != null,
				password    = dynamicDnsPassword,
				gtdLocation = globalTrafficDirectorLocation,
				monitor     = isSystemMonitoringEnabled,
				failover    = isFailoverEnabled
			});
		}

		public DnsRecords Update(ARecord record, Value<DomainName> name = default, Value<IPv4> target = default, Value<TimeToLive> timeToLive = default, Value<string?> dynamicDnsPassword = default, Value<GlobalTrafficDirectorLocation> globalTrafficDirectorLocation = default, Value<bool> isSystemMonitoringEnabled = default, Value<bool> isFailoverEnabled = default)
		{
			if (record is null)
				throw new ArgumentNullException(nameof(record));

			if (record.ParentDomainId != ParentDomainId)
				throw new ArgumentException("The specified A record does not belong to this domain", nameof(record));

			if (name.HasValue && !((DomainName)name).IsSubdomainOf(ParentDomain))
				throw new ArgumentException($"The specified domain `{(DomainName)name}` is not a subdomain of the parent `{ParentDomain}`", nameof(name));

			return AddUpdate(new
			{
				id         = record.Id,
				name       = name.HasValue
								? ((DomainName)name).WithoutParent(ParentDomain)
								: record.Name.WithoutParent(ParentDomain),
				value      = target.HasValue
								? (IPv4)target
								: record.Target,
				ttl        = timeToLive.HasValue
								? (TimeToLive)timeToLive
								: record.TimeToLive,
				dynamicDns = dynamicDnsPassword.HasValue
								? (Value<bool>)(((string?)dynamicDnsPassword) != null)
								: default(Value<bool>),
				password    = dynamicDnsPassword,
				gtdLocation = globalTrafficDirectorLocation,
				monitor     = isSystemMonitoringEnabled,
				failover    = isFailoverEnabled
			});
		}

		public DnsRecords CreateCNameRecord(DomainName name, DomainName target, TimeToLive timeToLive, GlobalTrafficDirectorLocation globalTrafficDirectorLocation = default)
		{
			if (!name.IsSubdomainOf(ParentDomain))
				throw new ArgumentException($"The specified domain `{name}` is not a subdomain of the parent `{ParentDomain}`", nameof(name));

			return AddCreate(new
			{
				type        = "CNAME",
				name        = name.WithoutParent(ParentDomain),
				value       = target.IsSubdomainOf(ParentDomain)
								? target.WithoutParent(ParentDomain)
								: target + ".",
				ttl         = timeToLive,
				gtdLocation = globalTrafficDirectorLocation
			});
		}

		public DnsRecords Update(CNameRecord record, Value<DomainName> name = default, Value<DomainName> target = default, Value<TimeToLive> timeToLive = default, Value<GlobalTrafficDirectorLocation> globalTrafficDirectorLocation = default)
		{
			if (record is null)
				throw new ArgumentNullException(nameof(record));

			if (record.ParentDomainId != ParentDomainId)
				throw new ArgumentException("The specified CNAME record does not belong to this domain", nameof(record));

			if (name.HasValue && !((DomainName)name).IsSubdomainOf(ParentDomain))
				throw new ArgumentException($"The specified domain `{name}` is not a subdomain of the parent `{ParentDomain}`", nameof(name));

			return AddUpdate(new
			{
				id   = record.Id,
				name = name.HasValue
					? ((DomainName)name).WithoutParent(ParentDomain)
					: record.Name.WithoutParent(ParentDomain),
				value = ProcessValue(record, target),
				ttl = timeToLive.HasValue
					? (TimeToLive)timeToLive
					: record.TimeToLive,
				gtdLocation = globalTrafficDirectorLocation
			});

			string ProcessValue(CNameRecord record, Value<DomainName> target)
			{
				if (!target.HasValue)
				{
					return record.Target.IsSubdomainOf(ParentDomain)
						? record.Target.WithoutParent(ParentDomain)
						: record.Target + ".";
				}
				else
				{
					var targetValue = (DomainName)target;
					return targetValue.IsSubdomainOf(ParentDomain)
						? targetValue.WithoutParent(ParentDomain)
						: targetValue + ".";
				}
			}
		}

		public DnsRecords CreateTXTRecord(DomainName name, string value, TimeToLive timeToLive, GlobalTrafficDirectorLocation globalTrafficDirectorLocation = default)
		{
			if (!name.IsSubdomainOf(ParentDomain))
				throw new ArgumentException($"The specified domain `{name}` is not a subdomain of the parent `{ParentDomain}`", nameof(name));

			if (string.IsNullOrEmpty(value))
				throw new ArgumentException("TXT record value cannot be empty or null");

			return AddCreate(new
			{
				type        = "TXT",
				name        = name.WithoutParent(ParentDomain),
				value,
				ttl         = timeToLive,
				gtdLocation = globalTrafficDirectorLocation
			});
		}

		public DnsRecords Update(TXTRecord record, Value<DomainName> name = default, Value<string> value = default, Value<TimeToLive> timeToLive = default, Value<GlobalTrafficDirectorLocation> globalTrafficDirectorLocation = default)
		{
			if (record is null)
				throw new ArgumentNullException(nameof(record));

			if (record.ParentDomainId != ParentDomainId)
				throw new ArgumentException("The specified TXT record does not belong to this domain", nameof(record));

			if (name.HasValue && !((DomainName)name).IsSubdomainOf(ParentDomain))
				throw new ArgumentException($"The specified domain `{name}` is not a subdomain of the parent `{ParentDomain}`", nameof(name));

			if (value.HasValue && string.IsNullOrEmpty(value))
				throw new ArgumentException("TXT record value cannot be empty or null");

			return AddUpdate(new
			{
				id          = record.Id,
				type        = "TXT",
				name        = name.HasValue
								? ((DomainName)name).WithoutParent(ParentDomain)
								: record.Name.WithoutParent(ParentDomain),
				value       = value.HasValue
								? (string)value
								: record.Value,
				ttl         = timeToLive.HasValue
					? (TimeToLive)timeToLive
					: record.TimeToLive,
				gtdLocation = globalTrafficDirectorLocation
			});
		}
	}
}
