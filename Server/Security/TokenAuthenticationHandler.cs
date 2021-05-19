using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Managers;

namespace Server.Security {
	/// <summary>
	///     class to handle api_key security.
	/// </summary>
	public class TokenHeaderAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions> {
		/// <summary>
		///     scheme name for authentication handler.
		/// </summary>
		public const string SchemeName = "TokenHeader";

		/// <summary>
		/// </summary>
		/// <param name="options"></param>
		/// <param name="logger"></param>
		/// <param name="encoder"></param>
		/// <param name="clock"></param>
		/// <param name="manager"></param>
		public TokenHeaderAuthenticationHandler(
			IOptionsMonitor<AuthenticationSchemeOptions> options,
			ILoggerFactory logger,
			UrlEncoder encoder,
			ISystemClock clock,
			ConfigManager manager
		) : base(options, logger, encoder, clock)
			=> ConfigManager = manager;

		private ConfigManager ConfigManager { get; }

		/// <summary>
		///     verify that require api key header exist and handle authorization.
		/// </summary>
		protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
			if (!Request.Headers.ContainsKey("Token"))
				return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
			if (!ConfigManager.Contains(Request.Headers["token"]))
				return Task.FromResult(AuthenticateResult.Fail("Token Not Found"));
			var identity = new ClaimsIdentity();
			var principal = new ClaimsPrincipal(identity);
			var ticket = new AuthenticationTicket(principal, Scheme.Name);
			return Task.FromResult(AuthenticateResult.Success(ticket));
		}
	}

	/// <summary>
	/// </summary>
	public class TokenQueryAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions> {
		/// <summary>
		///     scheme name for authentication handler.
		/// </summary>
		public const string SchemeName = "TokenQuery";

		/// <summary>
		/// </summary>
		/// <param name="options"></param>
		/// <param name="logger"></param>
		/// <param name="encoder"></param>
		/// <param name="clock"></param>
		/// <param name="manager"></param>
		public TokenQueryAuthenticationHandler(
			IOptionsMonitor<AuthenticationSchemeOptions> options,
			ILoggerFactory logger,
			UrlEncoder encoder,
			ISystemClock clock,
			ConfigManager manager
		) : base(options, logger, encoder, clock)
			=> ConfigManager = manager;

		/// <summary>
		/// </summary>
		private ConfigManager ConfigManager { get; }

		/// <summary>
		///     verify that require api key header exist and handle authorization.
		/// </summary>
		protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
			if (!Request.Query.ContainsKey("token"))
				return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Query"));
			if (!ConfigManager.Contains(Request.Headers["token"]))
				return Task.FromResult(AuthenticateResult.Fail("Token Not Found"));
			var identity = new ClaimsIdentity(Array.Empty<Claim>());
			var principal = new ClaimsPrincipal(identity);
			var ticket = new AuthenticationTicket(principal, Scheme.Name);
			return Task.FromResult(AuthenticateResult.Success(ticket));
		}
	}
}