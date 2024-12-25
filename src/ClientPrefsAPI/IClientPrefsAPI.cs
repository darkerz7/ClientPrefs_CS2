using CounterStrikeSharp.API.Core.Capabilities;

namespace ClientPrefsAPI
{
	public interface IClientPrefsAPI
	{
		public static PluginCapability<IClientPrefsAPI> Capability { get; } = new("clientprefs:api");

		bool SetClientCookie(string sSteamID, string sCookieName, string sCookieValue);

		string GetClientCookie(string sSteamID, string sCookieName);
	}
}
