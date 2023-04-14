// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using eShare.Networking;

namespace eShare.Server.PacketHandlers;

/// <summary>
///     Provides methods for handling account packets.
/// </summary>
internal class AccountHandler
{
	/// <summary>
	///		Handles the request to update the name of an account.
	/// </summary>
	[PacketHandler((ushort)PacketIdentifiers.UpdateAccountNameRequest)]
	static async void HandleUpdateAccountNameRequest(Guid guid, Packet packet)
	{
		if (ServerHandler.Users.ContainsKey(guid))
		{
			var request = new UpdateAccountNamePacket();
			var senderGuid = ServerHandler.Users[guid];

			request.Unpack(packet);
			ServerHandler.Database.UpdateUserName(senderGuid, request.ProfileName);

			var friends = ServerHandler.Database.ViewFriends(senderGuid);

			foreach (var friend in friends)
			{
				var friendGuid = ServerHandler.Users.FirstOrDefault(user => user.Value == friend).Key;

				if (friendGuid != Guid.Empty)
				{
					await ServerHandler.Server.Send(friendGuid, new FriendNameUpdatePacket().Pack(senderGuid, request.ProfileName));
				}
			}
		}
	}

	/// <summary>
	///		Handles the request to update the profile picture of an account.
	/// </summary>
	[PacketHandler((ushort)PacketIdentifiers.UpdateAccountPictureRequest)]
	static async void HandleUpdateAccountPictureRequest(Guid guid, Packet packet)
	{
		if (ServerHandler.Users.ContainsKey(guid))
		{
			var request = new UpdateAccountPicturePacket();
			var senderGuid = ServerHandler.Users[guid];

			request.Unpack(packet);
			ServerHandler.Database.SetImageId(ServerHandler.Users[guid], request.ProfilePicture);

			var friends = ServerHandler.Database.ViewFriends(senderGuid);

			foreach (var friend in friends)
			{
				var friendGuid = ServerHandler.Users.FirstOrDefault(user => user.Value == friend).Key;

				if (friendGuid != Guid.Empty)
				{
					await ServerHandler.Server.Send(friendGuid, new FriendPictureUpdatePacket().Pack(senderGuid, request.ProfilePicture));
				}
			}
		}
	}

	/// <summary>
	///		Handles the request to delete an account.
	/// </summary>
	[PacketHandler((ushort)PacketIdentifiers.DeleteAccountRequest)]
	static async void HandleDeleteAccountRequest(Guid guid, Packet packet)
	{
		if (ServerHandler.Users.ContainsKey(guid))
		{
			var senderGuid = ServerHandler.Users[guid];
			var friends = ServerHandler.Database.ViewFriends(senderGuid);

			foreach (var friend in friends)
			{
				var friendGuid = ServerHandler.Users.FirstOrDefault(user => user.Value == friend).Key;

				if (friendGuid != Guid.Empty)
				{
					ServerHandler.Database.RemoveFriend(friend, senderGuid);

					await ServerHandler.Server!.Send(friendGuid, new FriendAccountDeletePacket().Pack(senderGuid));
				}
			}

			ServerHandler.Database.DeleteUser(senderGuid);
			ServerHandler.Users.Remove(guid);

			await ServerHandler.Server.Send(guid, new Packet((ushort)PacketIdentifiers.DeleteAccountResponse, out var _));
		}
	}

	/// <summary>
	///		Handles the request to remove a friend from an account's friend list.
	/// </summary>
	[PacketHandler((ushort)PacketIdentifiers.FriendRemoveRequest)]
	static async void HandleFriendRemoveRequest(Guid guid, Packet packet)
	{
		if (ServerHandler.Users.ContainsKey(guid))
		{
			var request = new FriendRemovePacket();
			var senderGuid = ServerHandler.Users[guid];

			request.Unpack(packet);
			ServerHandler.Database.RemoveFriend(senderGuid, request.Guid);
			ServerHandler.Database.RemoveFriend(request.Guid, senderGuid);

			if (ServerHandler.Database.GetUserStatus(request.Guid) && ServerHandler.Users.ContainsValue(request.Guid))
			{
				var receiverGuid = ServerHandler.Users.FirstOrDefault(user => user.Value == request.Guid).Key;

				if (receiverGuid != Guid.Empty)
				{
					await ServerHandler.Server.Send(receiverGuid, new FriendRemovePacket().Pack(senderGuid));
				}
			}
		}
	}
}