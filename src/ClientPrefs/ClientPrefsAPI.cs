using ClientPrefsAPI;

namespace CS2_ClientPrefs
{
	internal class CPAPI : IClientPrefsAPI
	{
		public bool SetClientCookie(string sSteamID, string sCookieName, string sCookieValue)
		{
			if (!string.IsNullOrEmpty(sSteamID) && !string.IsNullOrEmpty(sCookieName) && !string.IsNullOrEmpty(sCookieValue) && ClientPrefs.db.bDBReady)
			{
#pragma warning disable CS8625
				if (ClientPrefs.dbConfig.TypeDB.CompareTo("mysql") == 0)
					ClientPrefs.db.AnyDB.QueryAsync("REPLACE INTO clientprefs(SteamID,CookieName,CookieValue) VALUES('{ARG}','{ARG}','{ARG}');", new List<string>([sSteamID, sCookieName, sCookieValue]), null, true);
				else if (ClientPrefs.dbConfig.TypeDB.CompareTo("sqlite") == 0)
					ClientPrefs.db.AnyDB.QueryAsync("INSERT OR REPLACE INTO clientprefs(SteamID,CookieName,CookieValue) VALUES('{ARG}','{ARG}','{ARG}');", new List<string>([sSteamID, sCookieName, sCookieValue]), null, true);
				else if (ClientPrefs.dbConfig.TypeDB.CompareTo("postgre") == 0)
					ClientPrefs.db.AnyDB.QueryAsync("INSERT INTO clientprefs(SteamID,CookieName,CookieValue) VALUES('{ARG}','{ARG}','{ARG}') ON CONFLICT (SteamID,CookieName) DO UPDATE SET CookieValue='{ARG}';", new List<string>([sSteamID, sCookieName, sCookieValue, sCookieValue]), null, true);
				else
					return false;
#pragma warning restore CS8625
				return true;
			}
			return false;
		}
		public string GetClientCookie(string sSteamID, string sCookieName)
		{
			if (!string.IsNullOrEmpty(sSteamID) && !string.IsNullOrEmpty(sCookieName) && ClientPrefs.db.bDBReady)
			{
				var res = ClientPrefs.db.AnyDB.Query("SELECT CookieValue FROM clientprefs WHERE SteamID = '{ARG}' AND CookieName = '{ARG}';", new List<string>([sSteamID, sCookieName]));
				if (res.Count == 0) return null;
				return res[0][0];
			}
			return null;
		}
	}
}
