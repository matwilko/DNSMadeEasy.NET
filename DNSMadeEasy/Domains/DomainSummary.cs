using System;
using System.Collections.Immutable;
using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	public sealed class DomainSummary
	{
		public DomainName Name { get; }

		public DomainId Id { get; }

		public bool GlobalTrafficDirectorEnabled { get; }
		public FolderId FolderId { get; }

		public DateTime LastUpdated { get; }
		public DateTime CreatedAt { get; }
		public bool ProcessMulti { get; }
		public ImmutableArray<string> ActiveThirdParties { get; }
		public int PendingActionId { get; }

		public DomainSummary(DomainName name, DomainId id, bool globalTrafficDirectorEnabled, FolderId folderId, DateTime lastUpdated, DateTime createdAt, bool processMulti, ImmutableArray<string> activeThirdParties, int pendingActionId)
		{
			Name = name;
			Id = id;
			GlobalTrafficDirectorEnabled = globalTrafficDirectorEnabled;
			FolderId = folderId;
			LastUpdated = lastUpdated;
			CreatedAt = createdAt;
			ProcessMulti = processMulti;
			ActiveThirdParties = activeThirdParties.EnsureInitialized();
			PendingActionId = pendingActionId;
		}

		[JsonConstructor]
		internal DomainSummary(DomainName name, DomainId id, bool gtdEnabled, FolderId folderId, long updated, long created, bool processMulti, ImmutableArray<string> activeThirdParties, int pendingActionId)
		{
			Name = name;
			Id = id;
			GlobalTrafficDirectorEnabled = gtdEnabled;
			FolderId = folderId;
			LastUpdated = UnixTime.FromMillisecondTimestamp(updated);
			CreatedAt = UnixTime.FromMillisecondTimestamp(created);
			ProcessMulti = processMulti;
			ActiveThirdParties = activeThirdParties.EnsureInitialized();
			PendingActionId = pendingActionId;
		}
	}
}