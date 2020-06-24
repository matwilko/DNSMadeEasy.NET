using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using DNSMadeEasy.Http;
using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	public sealed partial class DnsMadeEasyClient : IDisposable
	{
		private const string               ProductionBaseUri = "https://api.dnsmadeeasy.com/V2.0/";
		private const string               SandboxBaseUri    = "https://api.sandbox.dnsmadeeasy.com/V2.0/";
		private       HttpClient           Client               { get; }
		private       RequestLimitsHandler RequestLimitsHandler { get; }
		private       DomainCache          DomainCache          { get; }

		public int? RequestLimit => RequestLimitsHandler.RequestLimit;
		public int? RequestsRemaining => RequestLimitsHandler.RequestsRemaining;

		public TimeSpan Timeout
		{
			get => Client.Timeout;
			set => Client.Timeout = value;
		}

		private static JsonSerializerOptions SerializerOptions { get; } = CreateSerializerOptions();

		public DnsMadeEasyClient(string apiKey, string secretKey, bool enableRetryOnRateLimit = true, bool sandbox = false, HttpMessageHandler? handler = null)
			: this (
				apiKey,
				secretKey,
				medianFirstRetryDelay: enableRetryOnRateLimit
					? TimeSpan.FromSeconds(10)
					: TimeSpan.Zero,
				retryCount: enableRetryOnRateLimit
					? 5
					: 0,
				sandbox,
				handler
			)
		{
		}

		[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Incorrect warning")]
		public DnsMadeEasyClient(string apiKey, string secretKey, TimeSpan medianFirstRetryDelay, int retryCount, bool sandbox = false, HttpMessageHandler? handler = null)
		{
			var baseHandler      = handler ?? new HttpClientHandler();
			var apiKeyHandler    = new ApiKeyHandler(apiKey, secretKey, baseHandler);
			RequestLimitsHandler = new RequestLimitsHandler(apiKeyHandler, medianFirstRetryDelay, retryCount);
			var errorHandler     = new ErrorHandler(RequestLimitsHandler);

			Client      = new HttpClient(errorHandler)
			{
				BaseAddress = new Uri(sandbox ? SandboxBaseUri : ProductionBaseUri),
				DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("application/json")} },
				Timeout = TimeSpan.FromMinutes(6)
			};
			DomainCache = new DomainCache(this);
		}

		private static JsonSerializerOptions CreateSerializerOptions()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(CustomJsonConverterFactory.Instance);
			options.Converters.Add(new ApiResponseConverterFactory());
			options.Converters.Add(new DnsRecordJsonConverter());
			if (NullableImmutableArrayConverterFactory.IsNeeded)
			{
				options.Converters.Add(new NullableImmutableArrayConverterFactory());
			}

			return options;
		}

#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable CA1054 // Uri parameters should not be strings
		public async Task<string> Get(string url)

		{
			using var request = await Client.GetAsync(url).ConfigureAwait(false);
			return await request.Content.ReadAsStringAsync().ConfigureAwait(false);
		}

		public async Task<string> Post(string url, string content)
		{
			using var request = await Client.PostAsync(url, new StringContent(content)).ConfigureAwait(false);
			return await request.Content.ReadAsStringAsync().ConfigureAwait(false);
		}
#pragma warning restore CA2000 // Dispose objects before losing scope
#pragma warning restore CA1054 // Uri parameters should not be strings

		public void Dispose()
		{
			Client.Dispose();
		}
	}
}
