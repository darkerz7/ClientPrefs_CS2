using ClientPrefsAPI;
using MySqlConnector;
using System.Data;
using System.Data.SQLite;

namespace ClientPrefs
{
	internal class ClientPrefsAPI : IClientPrefsApi
	{
		async Task<bool> IClientPrefsApi.SetClientCookie(string sSteamID, string sCookieName, string sCookieValue)
		{
			if (!string.IsNullOrEmpty(sSteamID) && !string.IsNullOrEmpty(sCookieName) && !string.IsNullOrEmpty(sCookieValue) && ClientPrefs.db.bDBReady)
			{
				if (ClientPrefs.db.TypeDB == "sqlite")
				{
					SQLiteCommand cmd = new SQLiteCommand();
					cmd.CommandText = "INSERT OR REPLACE INTO clientprefs(SteamID,CookieName,CookieValue) VALUES(@steam,@cname,@cvalue);";
					cmd.Parameters.Add(new SQLiteParameter("@steam", sSteamID));
					cmd.Parameters.Add(new SQLiteParameter("@cname", sCookieName));
					cmd.Parameters.Add(new SQLiteParameter("@cvalue", sCookieValue));
					await ((DB_SQLite)ClientPrefs.db).Execute(cmd);
				}
				else if (ClientPrefs.db.TypeDB == "mysql")
				{
					MySqlCommand cmd = new MySqlCommand();
					cmd.CommandText = "REPLACE INTO clientprefs(SteamID,CookieName,CookieValue) VALUES(@steam,@cname,@cvalue);";
					cmd.Parameters.Add(new MySqlParameter("@steam", sSteamID));
					cmd.Parameters.Add(new MySqlParameter("@cname", sCookieName));
					cmd.Parameters.Add(new MySqlParameter("@cvalue", sCookieValue));
					await ((DB_Mysql)ClientPrefs.db).Execute(cmd);
				}
				return true;
			}
			return false;
		}

		async Task<string> IClientPrefsApi.GetClientCookie(string sSteamID, string sCookieName)
		{
			if (!string.IsNullOrEmpty(sSteamID) && !string.IsNullOrEmpty(sCookieName) && ClientPrefs.db.bDBReady)
			{
				string sValue = null;
				if (ClientPrefs.db.TypeDB == "sqlite")
				{
					using (SQLiteCommand cmd = new SQLiteCommand())
					{
						cmd.CommandText = "SELECT CookieValue FROM clientprefs WHERE SteamID = @steam AND CookieName = @cname;";
						cmd.Parameters.Add(new SQLiteParameter("@steam", sSteamID));
						cmd.Parameters.Add(new SQLiteParameter("@cname", sCookieName));
						using (DataTableReader reader = await ((DB_SQLite)ClientPrefs.db).Query(cmd))
						{
							if (reader != null && reader.HasRows)
							{
								await reader.ReadAsync();
								sValue = await reader.GetFieldValueAsync<string>(0);
							}
						}
					}
				}
				else if (ClientPrefs.db.TypeDB == "mysql")
				{
					using (MySqlCommand cmd = new MySqlCommand())
					{
						cmd.CommandText = "SELECT CookieValue FROM clientprefs WHERE SteamID = @steam AND CookieName = @cname;";
						cmd.Parameters.Add(new MySqlParameter("@steam", sSteamID));
						cmd.Parameters.Add(new MySqlParameter("@cname", sCookieName));
						using (DataTableReader reader = await ((DB_Mysql)ClientPrefs.db).Query(cmd))
						{
							if (reader != null && reader.HasRows)
							{
								await reader.ReadAsync();
								sValue = await reader.GetFieldValueAsync<string>(0);
							}
						}
					}
				}
				return sValue;
			}
			return null;
		}
	}
}
