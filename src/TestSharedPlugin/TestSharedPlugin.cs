using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;

using ClientPrefsAPI;
using CounterStrikeSharp.API.Core.Capabilities;

namespace TestSharedPlugin
{
	public class TestSharedPlugin : BasePlugin
	{
		public override string ModuleName => "[Test]Shared Plugin for Player Preferences";
		public override string ModuleVersion => "0.DZ.1";
		public override string ModuleAuthor => "DarkerZ [RUS]";
		public override string ModuleDescription => "Example of a plugin for using the API";

		private IClientPrefsAPI _CP_api;

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
				Console.WriteLine($"[TestSharedPlugin] Client Prefs API Loading Failed!");
			}
		}

		[ConsoleCommand("cf_set", "Example of writing data to a database")]
		[CommandHelper(minArgs: 1, usage: "[string]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
		public async void OnSetCookie(CCSPlayerController? player, CommandInfo command)
		{
			if(_CP_api != null)
			{
				string sValue = command.GetArg(1);
				if (!string.IsNullOrEmpty(sValue)) await _CP_api.SetClientCookie(player.SteamID.ToString(), "lang", sValue);
			}
		}
		[ConsoleCommand("cf_get", "Example of getting data from a database")]
		[CommandHelper(minArgs: 0, usage: "[]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
		public async void OnGetCookie(CCSPlayerController? player, CommandInfo command)
		{
			if (_CP_api != null)
			{
				string sValue = await _CP_api.GetClientCookie(player.SteamID.ToString(), "lang");
				Server.NextFrame(() =>
				{
					if (!string.IsNullOrEmpty(sValue)) command.ReplyToCommand($" Saved Cookie: {sValue}");
					else command.ReplyToCommand($" Cookie not found");
				});
			}
		}
	}
}
