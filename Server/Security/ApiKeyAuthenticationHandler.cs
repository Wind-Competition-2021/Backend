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
	public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions> {
		/// <summary>
		///     scheme name for authentication handler.
		/// </summary>
		public const string SchemeName = "ApiKey";

		public ApiKeyAuthenticationHandler(
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
		protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
			if (!Request.Headers.ContainsKey("token"))
				return AuthenticateResult.Fail("Missing Authorization Header");
			if (!ConfigManager.Contains(Request.Headers["token"]))
				AuthenticateResult.Fail("Token Not Found");
			var identity = new ClaimsIdentity(new Claim[] { });
			var principal = new ClaimsPrincipal(identity);
			var ticket = new AuthenticationTicket(principal, Scheme.Name);
			return AuthenticateResult.Success(ticket);
		}
	}
}