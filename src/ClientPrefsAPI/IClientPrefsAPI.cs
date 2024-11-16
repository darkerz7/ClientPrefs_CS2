using CounterStrikeSharp.API.Core.Capabilities;

namespace ClientPrefsAPI
{
	public interface IClientPrefsAPI
	{
		public static PluginCapability<IClientPrefsAPI> Capability { get; } = new("clientprefs:api");

		Task<bool> SetClientCookie(string sSteamID, string sCookieName, string sCookieValue);

		Task<string> GetClientCookie(string sSteamID, string sCookieName);
	}
}
