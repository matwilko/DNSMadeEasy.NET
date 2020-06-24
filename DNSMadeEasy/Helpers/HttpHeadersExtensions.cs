using System;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;

namespace DNSMadeEasy
{
	internal static class HttpHeadersExtensions
	{
		public static int? GetNumericHeader(this HttpHeaders headers, string headerName)
		{
			return headers.TryGetValues(headerName, out var values)
				? int.Parse(values.Single(), NumberStyles.Integer, CultureInfo.InvariantCulture)
				: default(int?);
		}

		public static DateTimeOffset? GetDateTimeOffsetHeader(this HttpHeaders headers, string headerName)
		{
			return headers.TryGetValues(headerName, out var values)
				? DateTimeOffset.ParseExact(values.Single(), "r", CultureInfo.InvariantCulture)
				: default(DateTimeOffset?);
		}

		public static void AddOrReplace(this HttpHeaders headers, string name, string value)
		{
			if (headers is null)
				throw new ArgumentNullException(nameof(headers));

			headers.Remove(name);
			headers.Add(name, value);
		}
	}
}
