using System;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DNSMadeEasy.Http
{
	internal sealed class ApiKeyHandler : DelegatingHandler
	{
		private string ApiKey { get; }
		private HMACSHA1 HMAC { get; }

		public ApiKeyHandler(string apiKey, string secretKey, HttpMessageHandler innerHandler) : base(innerHandler)
		{
			if (apiKey is null)
				throw new ArgumentNullException(nameof(apiKey));
			if (secretKey is null)
				throw new ArgumentNullException(nameof(secretKey));
			if (innerHandler is null)
				throw new ArgumentNullException(nameof(innerHandler));

			if (string.IsNullOrWhiteSpace(apiKey))
				throw new ArgumentException("API Key must be specified", nameof(apiKey));
			if (string.IsNullOrWhiteSpace(secretKey))
				throw new ArgumentException("Secret Key must be specified", nameof(secretKey));

			ApiKey = apiKey;

#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms - external specification
			HMAC = new HMACSHA1(Encoding.UTF8.GetBytes(secretKey));
#pragma warning restore CA5350 // Do Not Use Weak Cryptographic Algorithms
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var currentTimeStr = DateTime.UtcNow.ToString("r", CultureInfo.InvariantCulture);
			request.Headers.AddOrReplace("x-dnsme-apikey", ApiKey);
			request.Headers.AddOrReplace("x-dnsme-requestdate", currentTimeStr);

			var hashBytes = GetHash(Encoding.UTF8.GetBytes(currentTimeStr));
			request.Headers.AddOrReplace("x-dnsme-hmac", Hex.ToString(hashBytes));

			return base.SendAsync(request, cancellationToken);
		}

		private byte[] GetHash(byte[] bytes)
		{
			lock(HMAC)
				return HMAC.ComputeHash(bytes);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				HMAC.Dispose();
			}
		}
	}
}