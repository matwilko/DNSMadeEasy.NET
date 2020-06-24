using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DNSMadeEasy
{
	partial class DnsMadeEasyClient : IDomainClient
	{
		public IAsyncEnumerable<DomainSummary> GetAllDomains(CancellationToken cancellationToken = default) => Get<DomainSummary>("dns/managed", hasApiResponse: true, cancellationToken).ProcessDomainCache(DomainCache);

		public Task<Domain> GetDomain(DomainId domainId, CancellationToken cancellationToken = default) => GetSingle<Domain>($"dns/managed/{domainId}", hasApiResponse: false, cancellationToken).ProcessDomainCache(DomainCache);

		public Task<Domain> GetDomain(DomainName domainName, CancellationToken cancellationToken = default) => GetSingle<Domain>($"dns/managed/name?domainname={domainName}", hasApiResponse: false, cancellationToken).ProcessDomainCache(DomainCache);

		public async Task UpdateDomain(DomainId domainId, Value<DomainName> name = default, Value<bool> gtdEnabled = default, Value<SoaId?> soaId = default,
			Value<TemplateId?> templateId = default, Value<VanityId?> vanityId = default, Value<TransferAclId?> transferAclId = default, Value<FolderId> folderId = default,
			Value<ImmutableArray<DomainName>?> axfrServer = default, Value<ImmutableArray<DomainName>?> delegateNameServers = default, CancellationToken cancellationToken = default)
		{
			await Put($"dns/managed/{domainId}", new
			{
				name,
				gtdEnabled,
				soaId,
				templateId,
				vanityId,
				transferAclId,
				folderId,
				axfrServer,
				delegateNameServers
			}, cancellationToken).ConfigureAwait(false);

			if (name.HasValue)
				DomainCache.Set(domainId, name);
		}

		public async Task UpdateDomains(ImmutableArray<DomainId> domains, Value<bool> gtdEnabled = default, Value<SoaId?> soaId = default, Value<TemplateId?> templateId = default,
			Value<VanityId?> vanityId = default, Value<TransferAclId?> transferAclId = default, Value<FolderId> folderId = default, Value<ImmutableArray<DomainName>?> axfrServer = default,
			Value<ImmutableArray<DomainName>?> delegateNameServers = default, CancellationToken cancellationToken = default)
		{
			await Put("dns/managed", new
			{
				ids = domains,
				gtdEnabled,
				soaId,
				templateId,
				vanityId,
				transferAclId,
				folderId,
				axfrServer,
				delegateNameServers
			}, cancellationToken).ConfigureAwait(false);
		}

		public Task UpdateDomains(IEnumerable<DomainId> domains, Value<bool> gtdEnabled = default, Value<SoaId?> soaId = default, Value<TemplateId?> templateId = default,
			Value<VanityId?> vanityId = default, Value<TransferAclId?> transferAclId = default, Value<FolderId> folderId = default, Value<ImmutableArray<DomainName>?> axfrServer = default,
			Value<ImmutableArray<DomainName>?> delegateNameServers = default, CancellationToken cancellationToken = default)
		{
			return Put("dns/managed", new
			{
				ids = domains,
				gtdEnabled,
				soaId,
				templateId,
				vanityId,
				transferAclId,
				folderId,
				axfrServer,
				delegateNameServers
			}, cancellationToken);
		}

		public Task UpdateDomains(ImmutableArray<DomainName> domainNames, Value<bool> gtdEnabled = default, Value<SoaId?> soaId = default,
			Value<TemplateId?> templateId = default, Value<VanityId?> vanityId = default, Value<TransferAclId?> transferAclId = default, Value<FolderId> folderId = default,
			Value<ImmutableArray<DomainName>?> axfrServer = default, Value<ImmutableArray<DomainName>?> delegateNameServers = default, CancellationToken cancellationToken = default)
		{
			return Put("dns/managed", new
			{
				names = domainNames,
				gtdEnabled,
				soaId,
				templateId,
				vanityId,
				transferAclId,
				folderId,
				axfrServer,
				delegateNameServers
			}, cancellationToken);
		}

		public Task UpdateDomains(IEnumerable<DomainName> domainNames, Value<bool> gtdEnabled = default, Value<SoaId?> soaId = default,
			Value<TemplateId?> templateId = default, Value<VanityId?> vanityId = default, Value<TransferAclId?> transferAclId = default, Value<FolderId> folderId = default,
			Value<ImmutableArray<DomainName>?> axfrServer = default, Value<ImmutableArray<DomainName>?> delegateNameServers = default, CancellationToken cancellationToken = default)
		{
			return Put("dns/managed", new
			{
				names = domainNames,
				gtdEnabled,
				soaId,
				templateId,
				vanityId,
				transferAclId,
				folderId,
				axfrServer,
				delegateNameServers
			}, cancellationToken);
		}

		public async Task<DomainId> CreateDomain(DomainName name, Value<bool> gtdEnabled = default, Value<SoaId?> soaId = default, Value<TemplateId?> templateId = default,
			Value<VanityId?> vanityId = default, Value<TransferAclId?> transferAclId = default, Value<FolderId> folderId = default, Value<ImmutableArray<DomainName>?> axfrServer = default,
			Value<ImmutableArray<DomainName>?> delegateNameServers = default, CancellationToken cancellationToken = default)
		{
			var createdDomain = await Post<Domain>("dns/managed", new
			{
				name
			}, cancellationToken).ConfigureAwait(false);

			if (gtdEnabled.HasValue || soaId.HasValue || templateId.HasValue || vanityId.HasValue || transferAclId.HasValue || folderId.HasValue || axfrServer.HasValue || delegateNameServers.HasValue)
				await UpdateDomain(createdDomain.Id, default, gtdEnabled, soaId, templateId, vanityId, transferAclId, folderId, axfrServer, delegateNameServers, cancellationToken).ConfigureAwait(false);

			DomainCache.Set(createdDomain.Id, createdDomain.Name);

			return createdDomain.Id;
		}

		public async Task<ImmutableArray<DomainId>> CreateDomains(IEnumerable<DomainName> domainNames, Value<bool> gtdEnabled = default, Value<SoaId?> soaId = default,
			Value<TemplateId?> templateId = default, Value<VanityId?> vanityId = default, Value<TransferAclId?> transferAclId = default, Value<FolderId> folderId = default,
			Value<ImmutableArray<DomainName>?> axfrServer = default, Value<ImmutableArray<DomainName>?> delegateNameServers = default, CancellationToken cancellationToken = default)
		{
			var domainNamesList = domainNames.ToImmutableArray();
			var createdDomainIds = await Post<ImmutableArray<DomainId>>("dns/managed", new
			{
				names = domainNamesList
			}, cancellationToken).ConfigureAwait(false);

			if (gtdEnabled.HasValue || soaId.HasValue || templateId.HasValue || vanityId.HasValue || transferAclId.HasValue || folderId.HasValue || axfrServer.HasValue || delegateNameServers.HasValue)
				await UpdateDomains(createdDomainIds, gtdEnabled, soaId, templateId, vanityId, transferAclId, folderId, axfrServer, delegateNameServers, cancellationToken).ConfigureAwait(false);

			for (var i = 0; i < domainNamesList.Length; i++)
				DomainCache.Set(createdDomainIds[i], domainNamesList[i]);

			return createdDomainIds;
		}

		public async Task DeleteDomain(DomainId domainId, CancellationToken cancellationToken = default)
		{
			await Delete($"dns/managed/{domainId}", cancellationToken).ConfigureAwait(false);
			DomainCache.Remove(domainId);
		}

		public async Task DeleteDomains(ImmutableArray<DomainId> domainIds, CancellationToken cancellationToken = default)
		{
			await Delete("dns/managed", domainIds, cancellationToken).ConfigureAwait(false);
			foreach (var domainId in domainIds)
				DomainCache.Remove(domainId);
		}

		public Task DeleteDomains(IEnumerable<DomainId> domainIds, CancellationToken cancellationToken = default)
		{
			return DeleteDomains(domainIds.ToImmutableArray(), cancellationToken);
		}
	}
}
