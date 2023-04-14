// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using eShare.Networking;
using eShare.Networking.Utilities;

namespace eShare.Server;

/// <summary>
///     Handles the server instance and its resources.
/// </summary>
internal class ServerHandler
{
	public static Dictionary<Guid, Guid> Users = new();
	public static Networking.Server Server { get; private set; } = new(4000);
	public static Database Database { get; private set; } = new("eShare.Users");

	/// <summary>
	///		Starts the server and loads the user database.
	/// </summary>
	public static void Start()
	{
		Server.Disconnected += Server_UserDisconnected;

		Database.Load();
		Server.Start();
	}

	/// <summary>
	///		Shuts down the server and sends disconnection requests to all connected users.
	///		Also updates their status in the user database and saves it.
	/// </summary>
	public static void Shutdown()
	{
		foreach (var user in Users)
		{
			Server.Send(user.Key, new Packet((ushort)PacketIdentifiers.DisconnectionRequest, out var _)).Wait();

			Database.UpdateStatus(user.Value, false);
		}

		Database.Save();
		Server.Shutdown();

		Logger.Log(LogLevel.Information, "Server has been shut down.");
	}

	/// <summary>
	///		Pauses the console and waits for the user to press the Escape key. 
	/// </summary>
	public static void PauseConsole()
	{
		ConsoleKeyInfo consoleKeyInfo;

		do
		{
			Console.CursorVisible = false;
			consoleKeyInfo = Console.ReadKey(true);
		}
		while (consoleKeyInfo.Key != ConsoleKey.Escape);

		Shutdown();
	}

	/// <summary>
	///		Handles the event triggered when a user disconnects from the server.
	/// </summary>
	private static void Server_UserDisconnected(object? sender, Guid guid)
	{
		if (Users.ContainsKey(guid))
		{
			Server.Broadcast(guid, new FriendSessionPacket().Pack(Users[guid], PacketIdentifiers.FriendSessionClosed)).Wait();

			Database.UpdateStatus(Users[guid], false);
			Users.Remove(guid);
		}
	}
}