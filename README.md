# [Core]Client Preferences for CounterStrikeSharp
Allows plugin developers to save variables from players to the database

## Features:
1. Async functions
2. SQLite and MySQL support
3. Automatic language setting for players

## Required packages:
1. [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/)
2. [MySqlConnector](https://www.nuget.org/packages/MySqlConnector/2.4.0?_src=template) (2.4.0)
3. [System.Data.SQLite.Core](https://www.nuget.org/packages/System.Data.SQLite.Core/1.0.119?_src=template) (1.0.119)

## Installation:
1. Compile or copy ClientPrefsAPI to `counterstrikesharp/shared/ClientPrefsAPI` folger
2. Compile or copy ClientPrefs to `counterstrikesharp/plugins/ClientPrefs` folger
3. Copy and configure the configuration file `db_config.json` to `counterstrikesharp/plugins/ClientPrefs` folger
4. Install or copy DLL from `Required packages` (`MySqlConnector.dll`, `SQLite.Interop.dll`, `System.Data.SQLite.dll`) to counterstrikesharp/plugins/ClientPrefs folger
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
public async void MyExampleFuncSet(CCSPlayerController? player)
{
	<...>
	if(_CP_api != null)
	{
		<...>
		await _CP_api.SetClientCookie(player.SteamID.ToString(), <sMyCookieName>, <sMyCookieValue>);
		<...>
	}
	<...>
}
```
where:
1. The function must be `asynchronous` (using async/await)
2. The first parameter is a `string` containing the `player's SteamID`
3. The second parameter is a `string` containing the `name of the cookie`
4. The third parameter is a `string` containing the `cookie value`

Returns:
- True if valid data is entered and a connection to the database is established
- False in others

### To get data from database use following function:
```
public async void MyExampleFuncGet(CCSPlayerController? player)
{
	<...>
	if(_CP_api != null)
	{
		<...>
		string sValue = await _CP_api.GetClientCookie(player.SteamID.ToString(), <sMyCookieName>);
		<...>
	}
	<...>
}
```
where:
1. The function must be `asynchronous` (using async/await)
2. The first parameter is a `string` containing the `player's SteamID`
3. The second parameter is a `string` containing the `name of the cookie`

Returns:
- (string)sValue if the input data and database connection are valid
- null in others
