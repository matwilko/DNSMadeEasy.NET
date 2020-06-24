using System;
using System.Collections.Immutable;
using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	public sealed class Domain
	{
		public DomainName                 Name                         { get; }
		public DomainId                   Id                           { get; }
		public ImmutableArray<NameServer> NameServers                  { get; }
		public bool                       GlobalTrafficDirectorEnabled { get; }
		public SoaId?                     SoaId                        { get; }
		public TemplateId?                TemplateId                   { get; }
		public VanityId?                  VanityId                     { get; }
		public TransferAclId?             TransferAclId                { get; }
		public FolderId                   FolderId                     { get; }
		public DateTime                   LastUpdated                  { get; }
		public DateTime                   CreatedAt                    { get; }
		public ImmutableArray<DomainName> AxfrServer                   { get; }
		public ImmutableArray<DomainName> DelegateNameServers          { get; }
		public bool                       ProcessMulti                 { get; }
		public ImmutableArray<string>     ActiveThirdParties           { get; }
		public int                        PendingActionId              { get; }

		public Domain(DomainName name, DomainId id, ImmutableArray<NameServer> nameServers, bool globalTrafficDirectorEnabled, DateTime lastUpdated, DateTime createdAt, ImmutableArray<DomainName> axfrServer, ImmutableArray<DomainName> delegateNameServers, FolderId folderId, SoaId? soaId = default, TemplateId? templateId = default, VanityId? vanityId = default, TransferAclId? transferAclId = default, bool processMulti = false, ImmutableArray<string> activeThirdParties = default, int pendingActionId = 0)
		{
			Name                         = name;
			Id                           = id;
			NameServers                  = nameServers.EnsureInitialized();
			GlobalTrafficDirectorEnabled = globalTrafficDirectorEnabled;
			SoaId                        = soaId;
			TemplateId                   = templateId;
			VanityId                     = vanityId;
			TransferAclId                = transferAclId;
			FolderId                     = folderId;
			LastUpdated                  = lastUpdated;
			CreatedAt                    = createdAt;
			AxfrServer                   = axfrServer.EnsureInitialized();
			DelegateNameServers          = delegateNameServers.EnsureInitialized();
			ProcessMulti                 = processMulti;
			ActiveThirdParties           = activeThirdParties.EnsureInitialized();
			PendingActionId              = pendingActionId;
		}

		[JsonConstructor]
		internal Domain(DomainName name, DomainId id, ImmutableArray<NameServer> nameServers, bool? gtdEnabled, SoaId? soaId, TemplateId? templateId, VanityId? vanityId, TransferAclId? transferAclId, FolderId folderId, long updated, long created, ImmutableArray<DomainName>? axfrServer, ImmutableArray<DomainName>? delegateNameServers, bool? processMulti, ImmutableArray<string>? activeThirdParties, int? pendingActionId)
		{
			Name                         = name;
			Id                           = id;
			NameServers                  = nameServers.EnsureInitialized();
			GlobalTrafficDirectorEnabled = gtdEnabled ?? false;
			SoaId                        = soaId;
			TemplateId                   = templateId;
			VanityId                     = vanityId;
			TransferAclId                = transferAclId;
			FolderId                     = folderId;
			LastUpdated                  = UnixTime.FromMillisecondTimestamp(updated);
			CreatedAt                    = UnixTime.FromMillisecondTimestamp(created);
			AxfrServer                   = axfrServer.EnsureInitialized();
			DelegateNameServers          = delegateNameServers.EnsureInitialized();
			ProcessMulti                 = processMulti ?? false;
			ActiveThirdParties           = activeThirdParties.EnsureInitialized();
			PendingActionId              = pendingActionId ?? 0;
		}
	}
}