using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace DNSMadeEasy
{
	public interface IDomainClient
	{
		IAsyncEnumerable<DomainSummary> GetAllDomains(CancellationToken cancellationToken = default);
		Task<Domain> GetDomain(DomainId domainId, CancellationToken cancellationToken = default);
		Task<Domain> GetDomain(DomainName domainName, CancellationToken cancellationToken = default);

		Task UpdateDomain(
			DomainId domainId,
			Value<DomainName> name = default,
			Value<bool> gtdEnabled = default,
			Value<SoaId?> soaId = default,
			Value<TemplateId?> templateId = default,
			Value<VanityId?> vanityId = default,
			Value<TransferAclId?> transferAclId = default,
			Value<FolderId> folderId = default,
			Value<ImmutableArray<DomainName>?> axfrServer = default,
			Value<ImmutableArray<DomainName>?> delegateNameServers = default,
			CancellationToken cancellationToken = default
		);

		Task UpdateDomains(
			IEnumerable<DomainId> domains,
			Value<bool> gtdEnabled = default,
			Value<SoaId?> soaId = default,
			Value<TemplateId?> templateId = default,
			Value<VanityId?> vanityId = default,
			Value<TransferAclId?> transferAclId = default,
			Value<FolderId> folderId = default,
			Value<ImmutableArray<DomainName>?> axfrServer = default,
			Value<ImmutableArray<DomainName>?> delegateNameServers = default,
			CancellationToken cancellationToken = default
		);

		Task UpdateDomains(
			IEnumerable<DomainName> domainNames,
			Value<bool> gtdEnabled = default,
			Value<SoaId?> soaId = default,
			Value<TemplateId?> templateId = default,
			Value<VanityId?> vanityId = default,
			Value<TransferAclId?> transferAclId = default,
			Value<FolderId> folderId = default,
			Value<ImmutableArray<DomainName>?> axfrServer = default,
			Value<ImmutableArray<DomainName>?> delegateNameServers = default,
			CancellationToken cancellationToken = default
		);

		Task<DomainId> CreateDomain(
			DomainName name,
			Value<bool> gtdEnabled = default,
			Value<SoaId?> soaId = default,
			Value<TemplateId?> templateId = default,
			Value<VanityId?> vanityId = default,
			Value<TransferAclId?> transferAclId = default,
			Value<FolderId> folderId = default,
			Value<ImmutableArray<DomainName>?> axfrServer = default,
			Value<ImmutableArray<DomainName>?> delegateNameServers = default,
			CancellationToken cancellationToken = default
		);

		Task<ImmutableArray<DomainId>> CreateDomains(
			IEnumerable<DomainName> domainNames,
			Value<bool> gtdEnabled = default,
			Value<SoaId?> soaId = default,
			Value<TemplateId?> templateId = default,
			Value<VanityId?> vanityId = default,
			Value<TransferAclId?> transferAclId = default,
			Value<FolderId> folderId = default,
			Value<ImmutableArray<DomainName>?> axfrServer = default,
			Value<ImmutableArray<DomainName>?> delegateNameServers = default,
			CancellationToken cancellationToken = default
		);

		Task DeleteDomain(DomainId domainId, CancellationToken cancellationToken = default);
		Task DeleteDomains(IEnumerable<DomainId> domainIds, CancellationToken cancellationToken = default);
	}

	public static class DomainClientExtensions
	{
		public static async Task<Domain> GetDomain(this IDomainClient domainClient, string domainName, CancellationToken cancellationToken = default)
		{
			if (domainClient is null)
				throw new ArgumentNullException(nameof(domainClient));
			if (domainName is null)
				throw new ArgumentNullException(nameof(domainName));

			return await domainClient.GetDomain(DomainName.Parse(domainName), cancellationToken).ConfigureAwait(false);
		}

		public static async Task UpdateDomain(
			this IDomainClient domainClient,
			Domain domain,
			Value<DomainName> name = default,
			Value<bool> gtdEnabled = default,
			Value<SoaId?> soaId = default,
			Value<TemplateId?> templateId = default,
			Value<VanityId?> vanityId = default,
			Value<TransferAclId?> transferAclId = default,
			Value<FolderId> folderId = default,
			Value<ImmutableArray<DomainName>?> axfrServer = default,
			Value<ImmutableArray<DomainName>?> delegateNameServers = default,
			CancellationToken cancellationToken = default
		)
		{
			if (domainClient is null)
				throw new ArgumentNullException(nameof(domainClient));
			if (domain is null)
				throw new ArgumentNullException(nameof(domain));

			await domainClient.UpdateDomain(domain.Id, name, gtdEnabled, soaId, templateId, vanityId, transferAclId, folderId, axfrServer, delegateNameServers, cancellationToken).ConfigureAwait(false);
		}

		public static async Task UpdateDomain(
			this IDomainClient domainClient,
			DomainName domainName,
			Value<DomainName> name = default,
			Value<bool> gtdEnabled = default,
			Value<SoaId?> soaId = default,
			Value<TemplateId?> templateId = default,
			Value<VanityId?> vanityId = default,
			Value<TransferAclId?> transferAclId = default,
			Value<FolderId> folderId = default,
			Value<ImmutableArray<DomainName>?> axfrServer = default,
			Value<ImmutableArray<DomainName>?> delegateNameServers = default,
			CancellationToken cancellationToken = default
		)
		{
			if (domainClient is null)
				throw new ArgumentNullException(nameof(domainClient));

			var domain = await domainClient.GetDomain(domainName, cancellationToken).ConfigureAwait(false);
			await domainClient.UpdateDomain(domain.Id, name, gtdEnabled, soaId, templateId, vanityId, transferAclId, folderId, axfrServer, delegateNameServers, cancellationToken).ConfigureAwait(false);
		}

		public static async Task UpdateDomain(
			this IDomainClient domainClient,
			string domainName,
			Value<DomainName> name = default,
			Value<bool> gtdEnabled = default,
			Value<SoaId?> soaId = default,
			Value<TemplateId?> templateId = default,
			Value<VanityId?> vanityId = default,
			Value<TransferAclId?> transferAclId = default,
			Value<FolderId> folderId = default,
			Value<ImmutableArray<DomainName>?> axfrServer = default,
			Value<ImmutableArray<DomainName>?> delegateNameServers = default,
			CancellationToken cancellationToken = default
		)
		{
			if (domainClient is null)
				throw new ArgumentNullException(nameof(domainClient));
			if (domainName is null)
				throw new ArgumentNullException(nameof(domainName));

			await domainClient.UpdateDomain(DomainName.Parse(domainName), name, gtdEnabled, soaId, templateId, vanityId, transferAclId, folderId, axfrServer, delegateNameServers, cancellationToken).ConfigureAwait(false);
		}

		public static async Task UpdateDomains(
			this IDomainClient domainClient,
			IEnumerable<Domain> domainNames,
			Value<bool> gtdEnabled = default,
			Value<SoaId?> soaId = default,
			Value<TemplateId?> templateId = default,
			Value<VanityId?> vanityId = default,
			Value<TransferAclId?> transferAclId = default,
			Value<FolderId> folderId = default,
			Value<ImmutableArray<DomainName>?> axfrServer = default,
			Value<ImmutableArray<DomainName>?> delegateNameServers = default,
			CancellationToken cancellationToken = default
		)
		{
			if (domainClient is null)
				throw new ArgumentNullException(nameof(domainClient));
			if (domainNames is null)
				throw new ArgumentNullException(nameof(domainNames));

			var domainIds = domainNames.Select(d => d.Id);
			await domainClient.UpdateDomains(domainIds, gtdEnabled, soaId, templateId, vanityId, transferAclId, folderId, axfrServer, delegateNameServers, cancellationToken).ConfigureAwait(false);
		}

		public static async Task UpdateDomains(
			this IDomainClient domainClient,
			IEnumerable<string> domainNames,
			Value<bool> gtdEnabled = default,
			Value<SoaId?> soaId = default,
			Value<TemplateId?> templateId = default,
			Value<VanityId?> vanityId = default,
			Value<TransferAclId?> transferAclId = default,
			Value<FolderId> folderId = default,
			Value<ImmutableArray<DomainName>?> axfrServer = default,
			Value<ImmutableArray<DomainName>?> delegateNameServers = default,
			CancellationToken cancellationToken = default
		)
		{
			if (domainClient is null)
				throw new ArgumentNullException(nameof(domainClient));
			if (domainNames is null)
				throw new ArgumentNullException(nameof(domainNames));

			var convertedDomainNames = domainNames.Select(dn => DomainName.Parse(dn));
			await domainClient.UpdateDomains(convertedDomainNames, gtdEnabled, soaId, templateId, vanityId, transferAclId, folderId, axfrServer, delegateNameServers, cancellationToken).ConfigureAwait(false);
		}

		public static async Task<DomainId> CreateDomain(
			this IDomainClient domainClient,
			string name,
			Value<bool> gtdEnabled = default,
			Value<SoaId?> soaId = default,
			Value<TemplateId?> templateId = default,
			Value<VanityId?> vanityId = default,
			Value<TransferAclId?> transferAclId = default,
			Value<FolderId> folderId = default,
			Value<ImmutableArray<DomainName>?> axfrServer = default,
			Value<ImmutableArray<DomainName>?> delegateNameServers = default,
			CancellationToken cancellationToken = default
		)
		{
			if (domainClient is null)
				throw new ArgumentNullException(nameof(domainClient));
			if (name is null)
				throw new ArgumentNullException(nameof(name));

			var domainName = DomainName.Parse(name);
			return await domainClient.CreateDomain(domainName, gtdEnabled, soaId, templateId, vanityId, transferAclId, folderId, axfrServer, delegateNameServers, cancellationToken).ConfigureAwait(false);
		}

		public static async Task<ImmutableArray<DomainId>> CreateDomains(
			this IDomainClient domainClient,
			IEnumerable<string> domainNames,
			Value<bool> gtdEnabled = default,
			Value<SoaId?> soaId = default,
			Value<TemplateId?> templateId = default,
			Value<VanityId?> vanityId = default,
			Value<TransferAclId?> transferAclId = default,
			Value<FolderId> folderId = default,
			Value<ImmutableArray<DomainName>?> axfrServer = default,
			Value<ImmutableArray<DomainName>?> delegateNameServers = default,
			CancellationToken cancellationToken = default
		)
		{
			if (domainClient is null)
				throw new ArgumentNullException(nameof(domainClient));
			if (domainNames is null)
				throw new ArgumentNullException(nameof(domainNames));

			var convertedDomainNames = domainNames.Select(dn => DomainName.Parse(dn));
			return await domainClient.CreateDomains(convertedDomainNames, gtdEnabled, soaId, templateId, vanityId, transferAclId, folderId, axfrServer, delegateNameServers, cancellationToken).ConfigureAwait(false);
		}

		public static async Task DeleteDomain(this IDomainClient domainClient, Domain domain, CancellationToken cancellationToken = default)
		{
			if (domainClient is null)
				throw new ArgumentNullException(nameof(domainClient));
			if (domain is null)
				throw new ArgumentNullException(nameof(domain));

			await domainClient.DeleteDomain(domain.Id, cancellationToken).ConfigureAwait(false);
		}

		public static async Task DeleteDomain(this IDomainClient domainClient, DomainName domainName, CancellationToken cancellationToken = default)
		{
			if (domainClient is null)
				throw new ArgumentNullException(nameof(domainClient));

			var domain = await domainClient.GetDomain(domainName, cancellationToken).ConfigureAwait(false);
			await domainClient.DeleteDomain(domain.Id, cancellationToken).ConfigureAwait(false);
		}

		public static async Task DeleteDomain(this IDomainClient domainClient, string domainName, CancellationToken cancellationToken = default)
		{
			if (domainClient is null)
				throw new ArgumentNullException(nameof(domainClient));
			if (domainName is null)
				throw new ArgumentNullException(nameof(domainName));

			var domain = await domainClient.GetDomain(domainName, cancellationToken).ConfigureAwait(false);
			await domainClient.DeleteDomain(domain.Id, cancellationToken).ConfigureAwait(false);
		}

		public static async Task DeleteDomains(this IDomainClient domainClient, IEnumerable<Domain> domains, CancellationToken cancellationToken = default)
		{
			if (domainClient is null)
				throw new ArgumentNullException(nameof(domainClient));
			if (domains is null)
				throw new ArgumentNullException(nameof(domains));

			var domainIds = domains.Select(d => d.Id);
			await domainClient.DeleteDomains(domainIds, cancellationToken).ConfigureAwait(false);
		}
	}
}