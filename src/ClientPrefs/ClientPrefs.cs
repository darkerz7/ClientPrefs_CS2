using ClientPrefsAPI;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Core.Translations;
using System.Text.Json;

namespace CS2_ClientPrefs
{
	public class ClientPrefs : BasePlugin
	{
		public override string ModuleName => "[Core]Player Prefs";
		public override string ModuleDescription => "Allows to save settings for players";
		public override string ModuleAuthor => "DarkerZ [RUS]";
		public override string ModuleVersion => "2.DZ.0";

		private CPAPI ClientPrefsApi = null;

		public static Database db;
		public static DBConfig dbConfig;

		public override void Load(bool hotReload)
		{
			try
			{
				ClientPrefsApi = new CPAPI();
				Capabilities.RegisterPluginCapability(IClientPrefsAPI.Capability, () => ClientPrefsApi);

			}
			catch (Exception)
			{
				ClientPrefsApi = null;
				PrintToConsole($"API Register Failed!");
			}


			RegisterEventHandler<EventPlayerConnectFull>(OnEventPlayerConnectFull);
			RegisterEventHandler<EventPlayerDisconnect>(OnEventPlayerDisconnect);

			Init_DB(ModuleDirectory);
		}
		public override void Unload(bool hotReload)
		{
			DeregisterEventHandler<EventPlayerConnectFull>(OnEventPlayerConnectFull);
			DeregisterEventHandler<EventPlayerDisconnect>(OnEventPlayerDisconnect);
			db.AnyDB.Close();
		}

		private void Init_DB(string ModuleDirectory)
		{
			string sConfig = $"{Path.Join(ModuleDirectory, "db_config.json")}";
			string sData;
			if (File.Exists(sConfig))
			{
				sData = File.ReadAllText(sConfig);
				dbConfig = JsonSerializer.Deserialize<DBConfig>(sData);
			}
			if (dbConfig == null) dbConfig = new DBConfig();
			if (dbConfig.TypeDB == "mysql") db = new DB_Mysql(dbConfig.SQL_NameDatabase, $"{dbConfig.SQL_Server}:{dbConfig.SQL_Port}", dbConfig.SQL_User, dbConfig.SQL_Password);
			else if (dbConfig.TypeDB == "postgre") db = new DB_PosgreSQL(dbConfig.SQL_NameDatabase, $"{dbConfig.SQL_Server}:{dbConfig.SQL_Port}", dbConfig.SQL_User, dbConfig.SQL_Password);
			else
			{
				dbConfig.TypeDB = "sqlite";
				string sDBFile = Path.Join(ModuleDirectory, dbConfig.SQLite_File);
				db = new DB_SQLite(sDBFile);
			}
			if (db.bSuccess)
			{
				PrintToConsole($"Database connection established. Type of DB: {dbConfig.TypeDB}");
#pragma warning disable CS8625
				db.AnyDB.QueryAsync("CREATE TABLE IF NOT EXISTS clientprefs(SteamID varchar(64) NOT NULL,CookieName varchar(64) NOT NULL,CookieValue varchar(512), PRIMARY KEY (SteamID, CookieName));", null, (_) =>
				{
					db.bDBReady = true;
				}, true);
			}
#pragma warning restore CS8625
			else PrintToConsole($"No connection to database. Type of DB: {dbConfig.TypeDB}.");
		}
		private HookResult OnEventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
		{
			GetLanguage(@event.Userid);
			return HookResult.Continue;
		}
		private void GetLanguage(CCSPlayerController player)
		{
			if (db.bDBReady && ClientPrefsApi != null && player.IsValid && !player.IsBot)
			{
				string language = ClientPrefsApi.GetClientCookie(player.SteamID.ToString(), "lang");
				Server.NextFrame(() =>
				{
					if (language != null) player.ExecuteClientCommandFromServer($"css_lang {language}");
				});
			}
		}
		private HookResult OnEventPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
		{
			SetLanguage(@event.Userid);
			return HookResult.Continue;
		}
		private void SetLanguage(CCSPlayerController player)
		{
			if (db.bDBReady && ClientPrefsApi != null && player.IsValid && (player.Connected == PlayerConnectedState.PlayerConnected) && !player.IsBot)
			{
				string language = player.GetLanguage().Name;
				if (language != null) ClientPrefsApi.SetClientCookie(player.SteamID.ToString(), "lang", language);
			}
		}
		public static void PrintToConsole(string sMessage, int iColor = 1)
		{
			Console.ForegroundColor = (ConsoleColor)8;
			Console.Write("[");
			Console.ForegroundColor = (ConsoleColor)6;
			Console.Write("ClientPrefs");
			Console.ForegroundColor = (ConsoleColor)8;
			Console.Write("] ");
			Console.ForegroundColor = (ConsoleColor)iColor;
			Console.WriteLine(sMessage, false);
			Console.ResetColor();
			/* Colors:
				* 0 - No color		1 - White		2 - Red-Orange		3 - Orange
				* 4 - Yellow		5 - Dark Green	6 - Green			7 - Light Green
				* 8 - Cyan			9 - Sky			10 - Light Blue		11 - Blue
				* 12 - Violet		13 - Pink		14 - Light Red		15 - Red */
		}
	}
}
