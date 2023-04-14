// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using eShare.Networking;
using eShare.Networking.Utilities;

namespace eShare.Server.PacketHandlers;

/// <summary>
///     Provides methods for handling session preparation packets.
/// </summary>
internal class SessionPreparationHandler
{
	/// <summary>
	///		Handles a packet that requests a unique GUID for a client.
	/// </summary>
	[PacketHandler((ushort)PacketIdentifiers.GuidRequest)]
	static async void HandleGuidRequest(Guid guid, Packet packet)
	{
		Guid response;

		do
		{
			response = Guid.NewGuid();
		} while (ServerHandler.Database.UserExists(response) || ServerHandler.Users.ContainsValue(response));

		ServerHandler.Users.Add(guid, response);

		await ServerHandler.Server.Send(guid, new GuidPacket().Pack(response));
	}

	/// <summary>
	///		Handles a packet that requests client-side content from the server.
	/// </summary>
	[PacketHandler((ushort)PacketIdentifiers.ContentRequest)]
	static async void HandleContentRequest(Guid guid, Packet packet)
	{
		var response = new ContentPacket().Pack(File.ReadAllBytes("Content.zip"));

		await ServerHandler.Server.Send(guid, response);
	}

	/// <summary>
	///		Handles a packet that creates a new user session.
	/// </summary>
	[PacketHandler((ushort)PacketIdentifiers.CreateSessionRequest)]
	static async void HandleCreateSessionRequest(Guid guid, Packet packet)
	{
		var request = new CreateSessionPacket();

		request.Unpack(packet);
		ServerHandler.Database.CreateUser(request.Guid, request.Name, true, 0);

		await ServerHandler.Server.Send(guid, new Packet((ushort)PacketIdentifiers.CreateSessionResponse, out var _));
	}

	/// <summary>
	///		Handles a packet that continues an existing user session.
	/// </summary>
	[PacketHandler((ushort)PacketIdentifiers.ContinueSessionRequest)]
	static async void HandleContinueSessionRequest(Guid guid, Packet packet)
	{
		var request = new ContinueSessionPacket();

		request.Unpack(packet);

		if (ServerHandler.Users.TryGetValue(guid, out var existingGuid)) return;

		ServerHandler.Users.Add(guid, request.Guid);
		ServerHandler.Database.UpdateStatus(request.Guid, true);

		await ServerHandler.Server.Send(guid, new Packet((ushort)PacketIdentifiers.ContinueSessionResponse, out var _));

		var friends = ServerHandler.Database.ViewFriends(ServerHandler.Users[guid]);

		foreach (var friend in friends)
		{
			var friendGuid = ServerHandler.Users.FirstOrDefault(user => user.Value == friend).Key;

			if (friendGuid != Guid.Empty)
			{
				await ServerHandler.Server.Send(friendGuid, new FriendSessionPacket().Pack(ServerHandler.Users[guid], PacketIdentifiers.FriendSessionStarted));
			}
		}
	}

	/// <summary>
	///		Handles a packet that closes an existing user session.
	/// </summary>
	[PacketHandler((ushort)PacketIdentifiers.CloseSessionRequest)]
	static async void HandleCloseSessionRequest(Guid guid, Packet packet)
	{
		if (ServerHandler.Users.ContainsKey(guid))
		{
			await ServerHandler.Server.Broadcast(guid, new FriendSessionPacket().Pack(ServerHandler.Users[guid], PacketIdentifiers.FriendSessionClosed));

			ServerHandler.Database.UpdateStatus(ServerHandler.Users[guid], false);
			ServerHandler.Users.Remove(guid);
		}
	}

	/// <summary>
	///		Handles a packet that requests a list of friends for the user.
	/// </summary>
	[PacketHandler((ushort)PacketIdentifiers.FriendsListRequest)]
	static async void HandleFriendsListRequest(Guid guid, Packet packet)
	{
		var senderGuid = ServerHandler.Users[guid];
		var friendsList = new List<FriendTemplate>();
		var friendsGuid = ServerHandler.Database.ViewFriends(senderGuid);

		foreach (var friend in friendsGuid)
		{
			friendsList.Add(new FriendTemplate(
				friend,
				ServerHandler.Database.GetUserStatus(friend),
				ServerHandler.Database.GetUserName(friend),
				ServerHandler.Database.GetImageId(friend)));
		}

		await ServerHandler.Server.Send(guid, new FriendsListPacket().Pack(friendsList));
	}
}