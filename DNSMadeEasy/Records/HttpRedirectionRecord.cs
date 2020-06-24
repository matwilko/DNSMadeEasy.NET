using System;
using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	public sealed class HttpRedirectionRecord : DnsRecord
	{
		public Uri          Url          { get; }
		public string       Description  { get; }
		public string       Keywords     { get; }
		public string       Title        { get; }
		public RedirectType RedirectType { get; }
		public bool         IsHardLink   { get; }

		[JsonConstructor]
		internal HttpRedirectionRecord(DnsRecordId id, DomainName name, RecordSource source, DomainId sourceId, TimeToLive ttl, GlobalTrafficDirectorLocation? gtdLocation, Uri value, string description, string keywords, string title, RedirectType redirectType, bool hardLink) : base(id, name, source, sourceId, ttl, gtdLocation)
		{
			Url          = value;
			Description  = description;
			Keywords     = keywords;
			Title        = title;
			RedirectType = redirectType;
			IsHardLink   = hardLink;
		}

		public HttpRedirectionRecord(Uri url, string description, string keywords, string title, RedirectType redirectType, bool isHardLink, DnsRecordId id, DomainName name, DomainId parentDomainId, TimeToLive timeToLive, RecordSource recordSource = default, GlobalTrafficDirectorLocation? globalTrafficDirectorLocation = null) : base(id, name, parentDomainId, timeToLive, recordSource, globalTrafficDirectorLocation)
		{
			Url          = url;
			Description  = description;
			Keywords     = keywords;
			Title        = title;
			RedirectType = redirectType;
			IsHardLink   = isHardLink;
		}

		private protected override DnsRecord UpdateFromCache(DomainName newName, DomainName parentDomainName)
		{
			return new HttpRedirectionRecord(Id, newName, Source, ParentDomainId, TimeToLive, GlobalTrafficDirectorLocation, Url, Description, Keywords, Title, RedirectType, IsHardLink);
		}
	}
}