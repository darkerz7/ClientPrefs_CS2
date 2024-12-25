# [Core]Client Preferences for CounterStrikeSharp
Allows plugin developers to save variables from players to the database

## Features:
1. Async functions
2. SQLite/MySQL/PostgreSQL support
3. Automatic language setting for players

## Required packages:
1. [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/)
2. [AnyBaseLibCS2](https://github.com/NickFox007/AnyBaseLibCS2) (0.9.1)

## Installation:
1. Install [AnyBaseLibCS2](https://github.com/NickFox007/AnyBaseLibCS2)
2. Compile or copy ClientPrefsAPI to `counterstrikesharp/shared/ClientPrefsAPI` folger
3. Compile or copy ClientPrefs to `counterstrikesharp/plugins/ClientPrefs` folger
4. Copy and configure the configuration file `db_config.json` to `counterstrikesharp/plugins/ClientPrefs` folger
5. Restart server

## Example:
### Add the dependency ClientPrefsApi to your project:
```
using ClientPrefsAPI;

<My plugin class>: BasePlugin
{
	<...>
	private IClientPrefsAPI _CP_api;

	public override void OnAllPluginsLoaded(bool hotReload)
	{
		<...>
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
		<...>
	}
	<...>
}
```

### To write values to database use following function:
```
public void MyExampleFuncSet(CCSPlayerController? player)
{
	<...>
	if(_CP_api != null)
	{
		<...>
		_CP_api.SetClientCookie(player.SteamID.ToString(), <sMyCookieName>, <sMyCookieValue>);
		<...>
	}
	<...>
}
```
where:
1. The first parameter is a `string` containing the `player's SteamID`
2. The second parameter is a `string` containing the `name of the cookie`
3. The third parameter is a `string` containing the `cookie value`

Returns:
- True if valid data is entered and a connection to the database is established
- False in others

### To get data from database use following function:
```
public void MyExampleFuncGet(CCSPlayerController? player)
{
	<...>
	if(_CP_api != null)
	{
		<...>
		string sValue = _CP_api.GetClientCookie(player.SteamID.ToString(), <sMyCookieName>);
		<...>
	}
	<...>
}
```
where:
1. The first parameter is a `string` containing the `player's SteamID`
2. The second parameter is a `string` containing the `name of the cookie`

Returns:
- (string)sValue if the input data and database connection are valid
- null in others
