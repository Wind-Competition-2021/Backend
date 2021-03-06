using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Managers;

namespace Server.Security {
	/// <summary>
	///     Class to handle token security from header
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
		/// <param name="manager">Injected from Startup</param>
		public TokenHeaderAuthenticationHandler(
			IOptionsMonitor<AuthenticationSchemeOptions> options,
			ILoggerFactory logger,
			UrlEncoder encoder,
			ISystemClock clock,
			ConfigurationManager manager
		) : base(options, logger, encoder, clock)
			=> ConfigurationManager = manager;

		private ConfigurationManager ConfigurationManager { get; }

		/// <summary>
		///     Verify that token header exist and handle authorization.
		/// </summary>
		protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
			if (!Request.Headers.ContainsKey("Token"))
				return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
			if (!ConfigurationManager.Contains(Request.Headers["token"]))
				return Task.FromResult(AuthenticateResult.Fail("Token Not Found"));
			var identity = new ClaimsIdentity(new Claim[] {new(ClaimTypes.Hash, Request.Headers["token"])}, "ApiKey");
			var principal = new ClaimsPrincipal(identity);
			var ticket = new AuthenticationTicket(principal, Scheme.Name);
			return Task.FromResult(AuthenticateResult.Success(ticket));
		}
	}

	/// <summary>
	///     Class to handle token security from query
	/// </summary>
	public class TokenQueryAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions> {
		/// <summary>
		///     Scheme name for authentication handler.
		/// </summary>
		public const string SchemeName = "TokenQuery";

		/// <summary>
		/// </summary>
		/// <param name="options"></param>
		/// <param name="logger"></param>
		/// <param name="encoder"></param>
		/// <param name="clock"></param>
		/// <param name="manager">Injected from Startup</param>
		public TokenQueryAuthenticationHandler(
			IOptionsMonitor<AuthenticationSchemeOptions> options,
			ILoggerFactory logger,
			UrlEncoder encoder,
			ISystemClock clock,
			ConfigurationManager manager
		) : base(options, logger, encoder, clock)
			=> ConfigurationManager = manager;

		private ConfigurationManager ConfigurationManager { get; }

		/// <summary>
		///     Verify that token query exist and handle authorization.
		/// </summary>
		protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
			if (!Request.Query.ContainsKey("token"))
				return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Query"));
			if (!ConfigurationManager.Contains(Request.Query["token"]))
				return Task.FromResult(AuthenticateResult.Fail("Token Not Found"));
			var identity = new ClaimsIdentity(new Claim[] {new(ClaimTypes.Hash, Request.Query["token"])}, "ApiKey");
			var principal = new ClaimsPrincipal(identity);
			var ticket = new AuthenticationTicket(principal, Scheme.Name);
			return Task.FromResult(AuthenticateResult.Success(ticket));
		}
	}
}