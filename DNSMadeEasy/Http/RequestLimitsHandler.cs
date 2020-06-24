﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DNSMadeEasy.Http
{
	internal sealed class RequestLimitsHandler : DelegatingHandler
	{
		public  int?                 RequestLimit      { get; private set; }
		public  int?                 RequestsRemaining { get; private set; }
		private DecorrelatedJitterV2 RetryJitter       { get; }

		public RequestLimitsHandler(HttpMessageHandler innerHandler) : base(innerHandler)
		{
			RetryJitter = default;
		}

		public RequestLimitsHandler(HttpMessageHandler innerHandler, TimeSpan medianFirstRetryDelay, int retryCount) : base(innerHandler)
		{
			RetryJitter = new DecorrelatedJitterV2(medianFirstRetryDelay, retryCount);
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			foreach (var timeSpan in RetryJitter)
			{
				if (timeSpan > TimeSpan.Zero)
					await Task.Delay(timeSpan, cancellationToken).ConfigureAwait(false);

				var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

				RequestLimit = response.Headers.GetNumericHeader("x-dnsme-requestLimit");
				RequestsRemaining = response.Headers.GetNumericHeader("x-dnsme-requestsRemaining");

				if (response.StatusCode != HttpStatusCode.BadRequest || RequestsRemaining > 0)
					return response;
			}

			if (RetryJitter.RetryCount > 0)
				throw new RateLimitingException($"Rate limit has been exceeded, despite {RetryJitter.RetryCount:d} retries.");
			else
				throw new RateLimitingException("Rate limit has been exceeded.");
		}

		// Adapted from https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry/blob/master/src/Polly.Contrib.WaitAndRetry/Backoff.DecorrelatedJitterV2.cs
		// Copyright (c) 2019, AppvNext and contributors
		// All rights reserved.
		// Redistribution and use in source and binary forms, with or without
		// modification, are permitted provided that the following conditions are met:
		//    * Redistributions of source code must retain the above copyright
		//      notice, this list of conditions and the following disclaimer.
		//    * Redistributions in binary form must reproduce the above copyright
		//      notice, this list of conditions and the following disclaimer in the
		//      documentation and/or other materials provided with the distribution.
		//    * Neither the name of App vNext nor the
		//      names of its contributors may be used to endorse or promote products
		//      derived from this software without specific prior written permission.

		// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
		// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
		// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
		// DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
		// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
		// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
		// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
		// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
		// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
		// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

		/// <summary>
		/// Generates sleep durations in an exponentially backing-off, jittered manner, making sure to mitigate any correlations.
		/// For example: 850ms, 1455ms, 3060ms.
		/// Per discussion in Polly issue 530, the jitter of this implementation exhibits fewer spikes and a smoother distribution than the AWS jitter formula.
		/// </summary>
		/// <param name="medianFirstRetryDelay">The median delay to target before the first retry, call it f (= f * 2^0).
		/// Choose this value both to approximate the first delay, and to scale the remainder of the series.
		/// Subsequent retries will (over a large sample size) have a median approximating retries at time f * 2^1, f * 2^2 ... f * 2^t etc for try t.
		/// The actual amount of delay-before-retry for try t may be distributed between 0 and f * (2^(t+1) - 2^(t-1)) for t >= 2;
		/// or between 0 and f * 2^(t+1), for t is 0 or 1.</param>
		/// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
		/// <param name="seed">An optional <see cref="Random"/> seed to use.
		/// If not specified, will use a shared instance with a random seed, per Microsoft recommendation for maximum randomness.</param>
		/// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
		private struct DecorrelatedJitterV2 : IEnumerable<TimeSpan>
		{
			// A factor used to scale the median values of the retry times generated by the formula to be _near_ whole seconds, to aid Polly user comprehension.
			// This factor allows the median values to fall approximately at 1, 2, 4 etc seconds, instead of 1.4, 2.8, 5.6, 11.2.
			private const float rpScalingFactor = 1 / 1.4f;

			private readonly float targetTicksScalingFactor;
			private readonly byte  retryCount;

			public int RetryCount => retryCount;

			public DecorrelatedJitterV2(TimeSpan medianFirstRetryDelay, int retryCount)
			{
				if (medianFirstRetryDelay < TimeSpan.Zero)
					throw new ArgumentOutOfRangeException(nameof(medianFirstRetryDelay), medianFirstRetryDelay, "should be >= 0ms");
				if (retryCount < 0)
					throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, "should be >= 0");
				if (retryCount > byte.MaxValue)
					throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, $"should be <= {byte.MaxValue}");

				this.targetTicksScalingFactor = medianFirstRetryDelay.Ticks * rpScalingFactor;
				this.retryCount               = (byte)retryCount;
			}

			public DecorrelatedJitterV2Enumerator GetEnumerator() => new DecorrelatedJitterV2Enumerator(retryCount, targetTicksScalingFactor);

			IEnumerator<TimeSpan> IEnumerable<TimeSpan>.GetEnumerator() => GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		private struct DecorrelatedJitterV2Enumerator : IEnumerator<TimeSpan>
		{
			private static Random Random { get; } = new Random();

			// A factor used within the formula to help smooth the first calculated delay.
			private const float pFactor = 4.0f;



			// Upper-bound to prevent overflow beyond TimeSpan.MaxValue. Potential truncation during conversion from double to long
			// (as described at https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/numeric-conversions)
			// is avoided by the arbitrary subtraction of 1000. Validated by unit-test Backoff_should_not_overflow_to_give_negative_timespan.
			private const double maxTimeSpanDouble = 9223372036854774807; // TimeSpan.MaxValue.Ticks - 1000

			private readonly float targetTicksScalingFactor;
			private          float formulaIntrinsicValue;
			private readonly byte  retryCount;
			private          byte  attempts;

			public DecorrelatedJitterV2Enumerator(byte retryCount, float targetTicksScalingFactor) : this()
			{
				this.retryCount               = retryCount;
				this.targetTicksScalingFactor = targetTicksScalingFactor;
				this.formulaIntrinsicValue    = 0;
				this.attempts                 = 0;
			}

			public bool MoveNext()
			{
				if (attempts > retryCount)
					return false;

				Current = TimeSpan.FromTicks(
					(long)Math.Min(formulaIntrinsicValue * targetTicksScalingFactor, maxTimeSpanDouble)
				);

				var t = attempts + NextDouble();
				formulaIntrinsicValue = (float)(Math.Pow(2, t) * Math.Tanh(Math.Sqrt(pFactor * t))) - formulaIntrinsicValue;
				attempts++;
				return true;
			}

			private static double NextDouble()
			{
				lock (Random)
					return Random.NextDouble();
			}

			public TimeSpan Current { get; private set; }

			void IEnumerator.Reset()
			{
				formulaIntrinsicValue = 0;
				attempts              = 0;
			}

			object IEnumerator.Current => Current;

			void IDisposable.Dispose()
			{
			}
		}
	}
}