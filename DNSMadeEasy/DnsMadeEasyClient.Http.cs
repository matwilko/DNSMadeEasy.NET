using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DNSMadeEasy.Http;

namespace DNSMadeEasy
{
	partial class DnsMadeEasyClient
	{
		private static async Task<T> DeserializeResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
		{
			using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
			return await JsonSerializer.DeserializeAsync<T>(stream, SerializerOptions, cancellationToken).ConfigureAwait(false);
		}
#pragma warning disable CA1054 // Uri parameters should not be strings

		private async Task<T> GetJson<T>(string uri, CancellationToken cancellationToken = default)
		{
			using var response = await Client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
			return await DeserializeResponse<T>(response).ConfigureAwait(false);
		}

		private async IAsyncEnumerable<T> Get<T>(string uri, bool hasApiResponse = true, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			if (!hasApiResponse)
			{
				foreach (var item in await GetJson<IEnumerable<T>>(uri, cancellationToken).ConfigureAwait(false))
				{
					cancellationToken.ThrowIfCancellationRequested();
					yield return item;
				}
			}
			else
			{
				var apiResponse = await GetJson<ApiResponse<T>>(uri, cancellationToken).ConfigureAwait(false);
				if (apiResponse.TotalPages == 1)
				{
					foreach (var item in apiResponse.Data)
					{
						cancellationToken.ThrowIfCancellationRequested();
						yield return item;
					}
				}
				else
				{
					if (apiResponse.TotalPages - 1 > RequestsRemaining)
						throw new RateLimitingException("There are not enough requests remaining to complete this operation without being rate limited.");

					var pageUri = uri.Contains('?', StringComparison.Ordinal)
						? uri + "&page="
						: uri + "?page=";

					var allResponses = RollingOperations.Roll(apiResponse.TotalPages - 1, 10,
						(i, ct) => GetJson<ApiResponse<T>>(pageUri + (i + 2), cancellationToken)
					, cancellationToken);

					foreach (var item in apiResponse.Data)
					{
						cancellationToken.ThrowIfCancellationRequested();
						yield return item;
					}

					await foreach (var response in allResponses.WithCancellation(cancellationToken).ConfigureAwait(false))
					{
						foreach (var item in response.Data)
						{
							cancellationToken.ThrowIfCancellationRequested();
							yield return item;
						}
					}
				}
			}
		}

		private async Task<T> GetSingle<T>(string uri, bool hasApiResponse = true, CancellationToken cancellationToken = default)
		{
			if (!hasApiResponse)
			{
				return await GetJson<T>(uri, cancellationToken).ConfigureAwait(false);
			}
			else
			{
				var apiResponse = await GetJson<ApiSingleResponse<T>>(uri, cancellationToken).ConfigureAwait(false);
				return apiResponse.Data;
			}
		}

		private async Task Post(string uri, object data, CancellationToken cancellationToken = default)
		{
			using var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = new JsonContent(data) };
			using var response = await Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
		}

		private async Task<T> Post<T>(string uri, object data, CancellationToken cancellationToken = default)
		{
			using var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = new JsonContent(data) };
			using var response = await Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
			return await DeserializeResponse<T>(response, cancellationToken).ConfigureAwait(false);
		}

		private async Task Put(string uri, object data, CancellationToken cancellationToken = default)
		{
			using var request = new HttpRequestMessage(HttpMethod.Put, uri) { Content = new JsonContent(data) };
			using var response = await Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
		}

		private async Task<T> Put<T>(string uri, object data, CancellationToken cancellationToken = default)
		{
			using var request = new HttpRequestMessage(HttpMethod.Put, uri) { Content = new JsonContent(data) };
			using var response = await Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
			return await DeserializeResponse<T>(response, cancellationToken).ConfigureAwait(false);
		}

		public async Task Delete(string uri, CancellationToken cancellationToken = default)
		{
			using var request  = new HttpRequestMessage(HttpMethod.Delete, uri);
			using var response = await Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
		}


		public async Task Delete(string uri, object data, CancellationToken cancellationToken = default)

		{
			using var request = new HttpRequestMessage(HttpMethod.Delete, uri) { Content = new JsonContent(data) };
			using var response = await Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
		}

		private async Task<T> Delete<T>(string uri, CancellationToken cancellationToken = default)
		{
			using var request  = new HttpRequestMessage(HttpMethod.Delete, uri);
			using var response = await Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
			return await DeserializeResponse<T>(response, cancellationToken).ConfigureAwait(false);
		}

		private async Task<T> Delete<T>(string uri, object data, CancellationToken cancellationToken = default)
		{
			using var request = new HttpRequestMessage(HttpMethod.Delete, uri) { Content = new JsonContent(data) };
			using var response = await Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
			return await DeserializeResponse<T>(response, cancellationToken).ConfigureAwait(false);
		}

#pragma warning restore CA1054 // Uri parameters should not be strings

		private sealed class JsonContent : HttpContent
		{
			private object Data { get; }

			public JsonContent(object data)
			{
				Data = data ?? throw new ArgumentNullException(nameof(data));
				Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
			}

			protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
			{
				return JsonSerializer.SerializeAsync(stream, Data, Data.GetType(), SerializerOptions);
			}

			protected override bool TryComputeLength(out long length)
			{
				length = 0;
				return false;
			}
		}
	}
}
