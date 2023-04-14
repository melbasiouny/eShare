using eShare.Networking;
using eShare.Client.Helpers;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace eShare.Client.PacketHandlers;

internal class AccountHandler
{
	public static void HandleFriendAccountDeleteRequest(Packet packet, DispatcherQueue dispatcherQueue)
	{
		var request = new FriendAccountDeletePacket();

		request.Unpack(packet);

		dispatcherQueue.TryEnqueue(() =>
		{
			var friendToDelete = FriendsHelper.Friends.FirstOrDefault(friend => friend.Guid == request.Guid);
			var friendRequestToDelete = FriendRequestHelper.FriendRequests.FirstOrDefault(request => request.Guid == request.Guid);

			ChatHelper.ClearChat(request.Guid);

			if (friendToDelete != null)
			{
				FriendsHelper.Friends.Remove(friendToDelete);
			}

			if (friendRequestToDelete != null)
			{
				FriendRequestHelper.FriendRequests.Remove(friendRequestToDelete);
			}
		});
	}

	public static void HandleFriendRemoveRequest(Packet packet, DispatcherQueue dispatcherQueue)
	{
		var request = new FriendAccountDeletePacket();

		request.Unpack(packet);

		dispatcherQueue.TryEnqueue(() =>
		{
			var friendToRemove = FriendsHelper.Friends.FirstOrDefault(friend => friend.Guid == request.Guid);

			if (friendToRemove != null)
			{
				ChatHelper.ClearChat(request.Guid);
				FriendsHelper.Friends.Remove(friendToRemove);
			}
		});
	}

	public static void HandleFriendNameUpdateRequest(Packet packet, DispatcherQueue dispatcherQueue)
	{
		var request = new FriendNameUpdatePacket();

		request.Unpack(packet);

		dispatcherQueue.TryEnqueue(() =>
		{
			var friendToUpdate = FriendsHelper.Friends.FirstOrDefault(friend => friend.Guid == request.Guid);

			if (friendToUpdate != null)
			{
				friendToUpdate.Name = request.Name;
			}
		});
	}

	public static void HandleFriendPictureUpdateRequest(Packet packet, DispatcherQueue dispatcherQueue)
	{
		var request = new FriendPictureUpdatePacket();

		request.Unpack(packet);

		dispatcherQueue.TryEnqueue(() =>
		{
			var friendToUpdate = FriendsHelper.Friends.FirstOrDefault(friend => friend.Guid == request.Guid);

			if (friendToUpdate != null)
			{
				friendToUpdate.ProfilePicture = new BitmapImage(new Uri(ProfilePictureHelper.PopulateProfilePictures().FirstOrDefault(picture => picture.ID == request.ProfilePicture).PictureLocation));
			}
		});
	}
}
