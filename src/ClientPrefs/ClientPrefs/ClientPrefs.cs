using ClientPrefsAPI;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Core.Translations;
using MySqlConnector;
using System.Data.SQLite;
using System.Text.Json;

namespace ClientPrefs
{
	public class ClientPrefs : BasePlugin
	{
		public override string ModuleName => "[Core]Client Preferences";
		public override string ModuleVersion => "1.DZ.0";
		public override string ModuleAuthor => "DarkerZ [RUS]";
		public override string ModuleDescription => "Allows to save settings for clients";

		private ClientPrefsAPI ClientPrefsApi = null;
		private IClientPrefsApi? _CP_api;
		private bool bApiReady = false;

		public static Database db;
		private DBConfig dbConfig;

		public override void Load(bool hotReload)
		{
			ClientPrefsApi = new ClientPrefsAPI();
			Capabilities.RegisterPluginCapability(IClientPrefsApi.Capability, () => ClientPrefsApi);
			_CP_api = IClientPrefsApi.Capability.Get();
			if (_CP_api != null) bApiReady = true;

			RegisterEventHandler<EventPlayerConnectFull>(OnEventPlayerConnectFull);
			RegisterEventHandler<EventPlayerDisconnect>(OnEventPlayerDisconnect);

			string sConfig = $"{Path.Join(ModuleDirectory, "db_config.json")}";
			string sData;
			if (File.Exists(sConfig))
			{
				sData = File.ReadAllText(sConfig);
				dbConfig = JsonSerializer.Deserialize<DBConfig>(sData);
			}
			else dbConfig = new DBConfig();
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
							if(await ((DB_SQLite)db).Execute(cmd) > -1) db.bDBReady = true;
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

		[GameEventHandler(mode: HookMode.Post)]
		private HookResult OnEventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
		{
			GetLanguage(@event.Userid);
			return HookResult.Continue;
		}
		private async void GetLanguage(CCSPlayerController player)
		{
			if (db.bDBReady && bApiReady && player.IsValid && !player.IsBot)
			{
				string language = await _CP_api.GetClientCookie(player.SteamID.ToString(), "lang");
				Server.NextFrame(() =>
				{
					if (language != null) player.ExecuteClientCommandFromServer($"css_lang {language}");
				});
			}
		}

		[GameEventHandler(mode: HookMode.Pre)]
		private HookResult OnEventPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
		{
			SetLanguage(@event.Userid);
			return HookResult.Continue;
		}
		private async void SetLanguage(CCSPlayerController player)
		{
			if (db.bDBReady && bApiReady && player.IsValid && (player.Connected == PlayerConnectedState.PlayerConnected) && !player.IsBot)
			{
				string language = player.GetLanguage().Name;
				if (language != null) await _CP_api.SetClientCookie(player.SteamID.ToString(), "lang", language);
			}
		}

		public static void PrintToConsole(string sMessage)
		{
			Console.ForegroundColor = (ConsoleColor)8;
			Console.Write("[");
			Console.ForegroundColor = (ConsoleColor)6;
			Console.Write("ClientPrefs");
			Console.ForegroundColor = (ConsoleColor)8;
			Console.Write("] ");
			Console.ForegroundColor = (ConsoleColor)1;
			Console.WriteLine(sMessage);
			/* Colors:
			 * 0 - No color
			 * 1 - White
			 * 2 - Red-Orange
			 * 3 - Orange
			 * 4 - Yellow
			 * 5 - Dark Green
			 * 6 - Green
			 * 7 - Light Green
			 * 8 - Cyan
			 * 9 - Sky
			 * 10 - Light Blue
			 * 11 - Blue
			 * 12 - Violet
			 * 13 - Pink
			 * 14 - Light Red
			 * 15 - Red
			 */
		}
	}
}
