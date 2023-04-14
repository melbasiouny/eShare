using eShare.Networking;
using eShare.Client.Helpers;
using eShare.Client.Pages.Setup;
using Microsoft.UI.Dispatching;
using System.IO;
using System.Linq;
using System.IO.Compression;

namespace eShare.Client.PacketHandlers;

internal class SessionPreparationHandler
{
	public static void HandleGuidResponse(Packet packet)
	{
		var response = new GuidPacket();

		response.Unpack(packet);
		SessionPreparationPage.TemporaryStorage.Guid = response.Guid;
	}

	public static void HandleContentResponse(Packet packet)
	{
		var response = new ContentPacket();
		var contentPath = Path.Combine(StorageHelper.RootDirectory, "Content");

		response.Unpack(packet);

		if (File.Exists(contentPath))
		{
			File.Delete(contentPath);
		}

		File.WriteAllBytes(contentPath + ".zip", response.Content);
		ZipFile.ExtractToDirectory(contentPath + ".zip", StorageHelper.RootDirectory, true);
		File.Delete(contentPath + ".zip");
	}

	public static void HandleFriendSessionClosed(Packet packet, DispatcherQueue dispatcherQueue)
	{
		var response = new FriendSessionPacket();

		response.Unpack(packet);

		dispatcherQueue.TryEnqueue(() =>
		{
			if (FriendsHelper.Friends.Count > 0)
			{
				ChatHelper.ClearChat(response.Guid);

				var friend = FriendsHelper.Friends.FirstOrDefault(friend => friend.Guid == response.Guid);

				if (friend != null)
				{
					friend?.UpdateStatus(false);
					friend.Notifications = string.Empty;
				}
			}

			if (FriendRequestHelper.FriendRequests.Count > 0)
			{
				var request = FriendRequestHelper.FriendRequests.FirstOrDefault(friendRequest => friendRequest.Guid == response.Guid);

				if (request != null)
				{
					FriendRequestHelper.FriendRequests.Remove(request);
				}
			}
		});
	}

	public static void HandleFriendSessionStarted(Packet packet, DispatcherQueue dispatcherQueue)
	{
		var response = new FriendSessionPacket();

		response.Unpack(packet);

		dispatcherQueue.TryEnqueue(() =>
		{
			if (FriendsHelper.Friends.Count > 0)
			{
				var friend = FriendsHelper.Friends.FirstOrDefault(friend => friend.Guid == response.Guid);

				if (friend != null)
				{
					ChatHelper.CreateChat(response.Guid);

					friend.UpdateStatus(true);
				}
			}
		});
	}

	public static void HandleFriendsListResponse(Packet packet, DispatcherQueue dispatcherQueue)
	{
		var response = new FriendsListPacket();

		response.Unpack(packet);

		dispatcherQueue.TryEnqueue(() =>
		{
			foreach (var friend in response.Friends)
			{
				ChatHelper.CreateChat(friend.Guid);
				FriendsHelper.Friends.Add(new Friend(friend.Guid, friend.ProfileName, friend.Status, friend.ProfilePicture));
			}
		});
	}
}
