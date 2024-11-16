using ClientPrefsAPI;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Core.Translations;
using MySqlConnector;
using System.Data.SQLite;
using System.Text.Json;

namespace CS2_ClientPrefs
{
	public class ClientPrefs : BasePlugin
	{
		public override string ModuleName => "[Core]Player Prefs";
		public override string ModuleDescription => "Allows to save settings for players";
		public override string ModuleAuthor => "DarkerZ [RUS]";
		public override string ModuleVersion => "1.DZ.1";

		private CPAPI ClientPrefsApi = null;
		private IClientPrefsAPI _CP_api;

		public static Database db;
		private DBConfig dbConfig;

		public override void OnAllPluginsLoaded(bool hotReload)
		{
			try
			{
				PluginCapability<IClientPrefsAPI> CapabilityCP = new("clientprefs:api");
				_CP_api = IClientPrefsAPI.Capability.Get();
			}
			catch (Exception)
			{
				_CP_api = null;
				PrintToConsole($"API Loading Failed!");
			}
		}

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
			if (dbConfig.TypeDB == "mysql") db = new DB_Mysql($"server={dbConfig.Mysql_Server};port={dbConfig.Mysql_Port};user={dbConfig.Mysql_User};database={dbConfig.Mysql_NameDatabase};password={dbConfig.Mysql_Password};");
			else
			{
				string sDBFile = Path.Join(ModuleDirectory, dbConfig.SQLite_File);
				if (!File.Exists(sDBFile)) File.WriteAllBytes(sDBFile, Array.Empty<byte>());
				db = new DB_SQLite($"Data Source={sDBFile}");
			}
			Task.Run(async () =>
			{
				string sExceptionMessage = await db.TestConnection();
				if (db.bSuccess)
				{
					PrintToConsole($"Database connection established. Type of DB: {db.TypeDB}");

					if (db.TypeDB == "sqlite")
					{
						SQLiteCommand cmd = new SQLiteCommand();
						cmd.CommandText = "CREATE TABLE IF NOT EXISTS clientprefs(SteamID varchar(64) NOT NULL,CookieName varchar(64) NOT NULL,CookieValue varchar(512), PRIMARY KEY (SteamID, CookieName));";
						if (await ((DB_SQLite)db).Execute(cmd) > -1) db.bDBReady = true;
					}
					else if (db.TypeDB == "mysql")
					{
						MySqlCommand cmd = new MySqlCommand();
						cmd.CommandText = "CREATE TABLE IF NOT EXISTS clientprefs(SteamID varchar(64) NOT NULL,CookieName varchar(64) NOT NULL,CookieValue varchar(512), PRIMARY KEY (SteamID, CookieName));";
						if (await ((DB_Mysql)db).Execute(cmd) > -1) db.bDBReady = true;
					}
				}
				else PrintToConsole($"No connection to database. Type of DB: {db.TypeDB}. Error: {sExceptionMessage}");
			});
		}
		private HookResult OnEventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
		{
			GetLanguage(@event.Userid);
			return HookResult.Continue;
		}
		private async void GetLanguage(CCSPlayerController player)
		{
			if (db.bDBReady && _CP_api != null && player.IsValid && !player.IsBot)
			{
				string language = await _CP_api.GetClientCookie(player.SteamID.ToString(), "lang");
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
		private async void SetLanguage(CCSPlayerController player)
		{
			if (db.bDBReady && _CP_api != null && player.IsValid && (player.Connected == PlayerConnectedState.PlayerConnected) && !player.IsBot)
			{
				string language = player.GetLanguage().Name;
				if (language != null) await _CP_api.SetClientCookie(player.SteamID.ToString(), "lang", language);
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
