// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using eShare.Networking;

namespace eShare.Server.PacketHandlers;

/// <summary>
///     Provides methods for handling friend request packets.
/// </summary>
internal class FriendRequestHandler
{
	/// <summary>
	///		Handles outgoing friend requests.
	/// </summary>
	[PacketHandler((ushort)PacketIdentifiers.OutgoingFriendRequest)]
	static async void HandleOutgoingFriendRequest(Guid guid, Packet packet)
	{
		var request = new OutgoingFriendRequestPacket();

		request.Unpack(packet);

		if (ServerHandler.Users.ContainsValue(request.ReceiverGuid))
		{
			if (ServerHandler.Users.TryGetValue(guid, out var senderGuid))
			{
				var senderName = ServerHandler.Database.GetUserName(senderGuid);
				var senderImageId = ServerHandler.Database.GetImageId(senderGuid);

				var friendRequest = new FriendRequestPacket().Pack(
					senderGuid,
					senderName,
					senderImageId);

				var receiverKeyValuePair = ServerHandler.Users.FirstOrDefault(user => user.Value == request.ReceiverGuid);
				if (!receiverKeyValuePair.Equals(default(KeyValuePair<Guid, Guid>)))
				{
					var receiverGuid = receiverKeyValuePair.Key;
					await ServerHandler.Server.Send(receiverGuid, friendRequest);
				}
			}
		}
	}

	/// <summary>
	///		Handles accepted friend requests.
	/// </summary>
	[PacketHandler((ushort)PacketIdentifiers.AcceptFriendRequest)]
	static async void HandleAcceptFriendRequest(Guid guid, Packet packet)
	{
		var request = new AcceptFriendRequestPacket();

		request.Unpack(packet);

		if (ServerHandler.Users.ContainsValue(request.SenderGuid) && ServerHandler.Users.ContainsValue(request.ReceiverGuid))
		{
			ServerHandler.Database.AddFriend(request.SenderGuid, request.ReceiverGuid);
			ServerHandler.Database.AddFriend(request.ReceiverGuid, request.SenderGuid);

			var senderGuid = ServerHandler.Users[guid];
			var receiverGuid = ServerHandler.Users.FirstOrDefault(user => user.Value == request.ReceiverGuid).Key;

			if (senderGuid != Guid.Empty && receiverGuid != Guid.Empty)
			{
				var friendGuid = ServerHandler.Users[receiverGuid];

				if (friendGuid != Guid.Empty)
				{
					var senderResponse = new FriendPacket().Pack(
						senderGuid,
						ServerHandler.Database.GetUserName(senderGuid),
						ServerHandler.Database.GetImageId(senderGuid),
						PacketIdentifiers.AcceptFriendResponse);

					var friendResponse = new FriendPacket().Pack(
						friendGuid,
						ServerHandler.Database.GetUserName(friendGuid),
						ServerHandler.Database.GetImageId(friendGuid),
						PacketIdentifiers.AcceptFriendResponse);

					await ServerHandler.Server.Send(guid, friendResponse);
					await ServerHandler.Server.Send(receiverGuid, senderResponse);
				}
			}
		}
	}
}