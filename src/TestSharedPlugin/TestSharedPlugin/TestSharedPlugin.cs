using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;

using ClientPrefsAPI;

namespace TestSharedPlugin
{
	public class TestSharedPlugin : BasePlugin
	{
		public override string ModuleName => "[Test]Shared Plugin for Client Preferences";
		public override string ModuleVersion => "0.DZ.0";
		public override string ModuleAuthor => "DarkerZ [RUS]";
		public override string ModuleDescription => "Example of a plugin for using the API";

		private bool bClientPrefsApiReady = false;

		private IClientPrefsApi? _CP_api;

		public override void Load(bool hotReload)
		{
			_CP_api = IClientPrefsApi.Capability.Get();
			if (_CP_api != null) bClientPrefsApiReady = true;
		}

		[ConsoleCommand("cf_set", "Example of writing data to a database")]
		[CommandHelper(minArgs: 1, usage: "[string]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
		public async void OnSetCookie(CCSPlayerController? player, CommandInfo command)
		{
			if(bClientPrefsApiReady)
			{
				string sValue = command.GetArg(1);
				if (!string.IsNullOrEmpty(sValue)) await _CP_api.SetClientCookie(player.SteamID.ToString(), "lang", sValue);
			}
		}
		[ConsoleCommand("cf_get", "Example of getting data from a database")]
		[CommandHelper(minArgs: 0, usage: "[]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
		public async void OnGetCookie(CCSPlayerController? player, CommandInfo command)
		{
			if (bClientPrefsApiReady)
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
