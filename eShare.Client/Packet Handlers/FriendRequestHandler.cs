// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using eShare.Networking;
using eShare.Client.Helpers;
using Microsoft.UI.Dispatching;
using System.Linq;

namespace eShare.Client.PacketHandlers;

internal class FriendRequestHandler
{
	public static void HandleFriendRequest(Packet packet, DispatcherQueue dispatcherQueue)
	{
		var request = new FriendRequestPacket();

		request.Unpack(packet);

		dispatcherQueue.TryEnqueue(() =>
		{
			if (App.Storage.IncomingRequests)
			{
				var duplicateRequests = FriendRequestHelper.FriendRequests.FirstOrDefault(friend => friend.Guid == request.SenderGuid);

				if (duplicateRequests == null)
				{
					FriendRequestHelper.FriendRequests.Add(new FriendRequest(request.SenderGuid, request.SenderName, request.SenderProfilePicture));
				}
			}
		});
	}

	public static void HandleAcceptFriendResponse(Packet packet, DispatcherQueue dispatcherQueue)
	{
		var response = new FriendPacket();

		response.Unpack(packet);

		dispatcherQueue.TryEnqueue(() =>
		{
			FriendsHelper.Friends.Add(new Friend(response.Guid, response.ProfileName, true, response.ProfilePicture));

			if (FriendRequestHelper.FriendRequests.Count > 0)
			{
				var friendRequestToRemove = FriendRequestHelper.FriendRequests.FirstOrDefault(request => request.Guid == response.Guid);

				if (friendRequestToRemove != null)
				{
					FriendRequestHelper.FriendRequests.Remove(friendRequestToRemove);
				}
			}

			ChatHelper.CreateChat(response.Guid);
		});
	}
}
