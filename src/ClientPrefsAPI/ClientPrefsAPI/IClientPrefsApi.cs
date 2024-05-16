using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;

namespace ClientPrefsAPI
{
	public interface IClientPrefsApi
	{
		public static PluginCapability<IClientPrefsApi> Capability { get; } = new("clientprefs:api");

		Task<bool> SetClientCookie(string sSteamID, string sCookieName, string sCookieValue);

		Task<string> GetClientCookie(string sSteamID, string sCookieName);
	}
}
