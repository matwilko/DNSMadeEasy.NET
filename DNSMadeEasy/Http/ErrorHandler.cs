using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.HttpStatusCode;
using System.Diagnostics.CodeAnalysis;

namespace DNSMadeEasy.Http
{
	internal sealed class ErrorHandler : DelegatingHandler
	{
		public ErrorHandler(HttpMessageHandler innerHandler) : base(innerHandler)
		{
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

			if (response is null)
				throw new Exception("The request failed for an unknown reason.");

			if (response.IsSuccessStatusCode)
				return response;

			using (response)
			{
				if (response.StatusCode == Forbidden && HasClockSkew(response))
				{
					throw new ClockSkewException("The local time is not well-synchronized with the DNSMadeEasy servers - there is more than a 30 second discrepency - try synchronizing your local time with a remote time server");
				}
				else if (response.StatusCode == Forbidden || response.StatusCode == Unauthorized)
				{
					throw new UnauthorizedAccessException("The given credentials were invalid");
				}
				else if (response.StatusCode == NotFound)
				{
					throw new InvalidOperationException("The requested resource could not be found");
				}
				else if (response.Content.Headers.ContentType?.MediaType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true)
				{
					using var content   = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
					var       errorJson = await JsonSerializer.DeserializeAsync<JsonErrors>(content, cancellationToken: cancellationToken).ConfigureAwait(false);
					if (errorJson?.error is not null && errorJson.error.Count == 1)
						throw new Exception(errorJson.error.Single());
					else if (errorJson?.error is not null && errorJson.error.Count > 1)
						throw new AggregateException("There were multiple errors with your request", errorJson.error.Select(e => new Exception(e)));
					else
						throw new Exception("The request failed, but the error could not be parsed.");
				}
				else
				{
					throw new Exception("The request failed, but the error could not be parsed. See the HttpContent property for the raw error response.")
					{
						Data = { { "HttpContent", await response.Content.ReadAsStringAsync().ConfigureAwait(false) } }
					};
				}
			}
		}

		[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by JSON deserialization")]
		#pragma warning disable IDE1006 // Naming Styles - need to match JSON
		private sealed class JsonErrors
		{
			public IReadOnlyCollection<string>? error { get; set; }
		}
		#pragma warning restore IDE1006 // Naming Styles

		private static bool HasClockSkew(HttpResponseMessage response)
		{
			var requestTime = response.RequestMessage?.Headers.GetDateTimeOffsetHeader("x-dnsme-requestDate");
			var serverTime  = response.Headers.GetDateTimeOffsetHeader("Date");
			var difference  = serverTime > requestTime
				? serverTime - requestTime
				: requestTime - serverTime;
			return difference > TimeSpan.FromSeconds(30);
		}
	}
}
