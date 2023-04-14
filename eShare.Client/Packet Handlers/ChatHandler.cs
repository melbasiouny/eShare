// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using eShare.Networking;
using eShare.Client.Pages;
using eShare.Client.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Dispatching;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace eShare.Client.PacketHandlers;

internal class ChatHandler
{
	public static void HandleGIFReceivedResponse(Packet packet, DispatcherQueue dispatcherQueue)
	{
		var response = new GIFPacket();

		response.Unpack(packet);

		dispatcherQueue.TryEnqueue(() =>
		{
			if (FriendsHelper.Friends.Count > 0)
			{
				var friend = FriendsHelper.Friends.FirstOrDefault(friend => friend.Guid == response.Guid);

				if (friend != null && friend.IsOnline)
				{
					if (ChatPage.SelectedFriend != null && ChatPage.SelectedFriend.Guid != friend.Guid)
					{
						FriendsHelper.IncrementNotifications(friend);
						ChatHelper.SendNotification(friend.Name, response.GIFSource);
					}
					else if (ChatPage.SelectedFriend == null)
					{
						FriendsHelper.IncrementNotifications(friend);
						ChatHelper.SendNotification(friend.Name, response.GIFSource);
					}

					ChatHelper.AddGIF(friend.Guid, response.GIFSource, HorizontalAlignment.Left);
				}
			}
		});
	}

	public static void HandleMessageReceivedResponse(Packet packet, DispatcherQueue dispatcherQueue)
	{
		var response = new MessagePacket();

		response.Unpack(packet);

		dispatcherQueue.TryEnqueue(() =>
		{
			if (FriendsHelper.Friends.Count > 0)
			{
				var friend = FriendsHelper.Friends.FirstOrDefault(friend => friend.Guid == response.Guid);

				if (friend != null && friend.IsOnline)
				{
					if (ChatPage.SelectedFriend != null && ChatPage.SelectedFriend.Guid != friend.Guid)
					{
						FriendsHelper.IncrementNotifications(friend);
						ChatHelper.SendNotification(friend.Name, response.Message);
					}
					else if (ChatPage.SelectedFriend == null)
					{
						FriendsHelper.IncrementNotifications(friend);
						ChatHelper.SendNotification(friend.Name, response.Message);
					}

					ChatHelper.AddMessage(friend.Guid, response.Message, HorizontalAlignment.Left);
				}
			}
		});
	}

	public static void HandleAttachmentReceivedResponse(Packet packet, DispatcherQueue dispatcherQueue)
	{
		var response = new AttachmentPacket();

		response.Unpack(packet);

		dispatcherQueue.TryEnqueue(() =>
		{
			if (FriendsHelper.Friends.Count > 0)
			{
				var friend = FriendsHelper.Friends.FirstOrDefault(friend => friend.Guid == response.Guid);

				if (friend != null && friend.IsOnline)
				{
					if (ChatPage.SelectedFriend != null && ChatPage.SelectedFriend.Guid != friend.Guid)
					{
						FriendsHelper.IncrementNotifications(friend);
						ChatHelper.SendNotification(friend.Name, response.FileName);
					}
					else if (ChatPage.SelectedFriend == null)
					{
						FriendsHelper.IncrementNotifications(friend);
						ChatHelper.SendNotification(friend.Name, response.FileName);
					}

					ChatHelper.AddAttachment(friend.Guid, response.Identifier, response.FileName, response.FileSize);
				}
			}
		});
	}

	public async static void HandleAttachmentDownloadRequest(Packet packet)
	{
		var request = new AttachmentDownloadPacket();

		request.Unpack(packet);

		if (FriendsHelper.Friends.Count > 0)
		{
			var friend = FriendsHelper.Friends.FirstOrDefault(friend => friend.Guid == request.Guid);

			if (friend != null && friend.IsOnline)
			{
				var attachmentRequest = GetAttachment(request.Guid, request.Identifier);

				if (attachmentRequest != null)
				{
					await Task.Run(async () =>
					{
						var chunks = CalculateChunks(attachmentRequest.ActualSize);
						var attachmentChunk = ReadChunkFromFile(attachmentRequest.FilePath, 0);

						var attachmentDownloadChunkResponse = new AttachmentDownloadChunkPacket().Pack(
							friend.Guid,
							attachmentRequest.Identifier,
							chunks.Length,
							0,
							attachmentChunk);

						await App.Client.Send(attachmentDownloadChunkResponse);
					});
				}
			}
		}
	}

	public static void HandleAttachmentDownloadChunkResponse(Packet packet, DispatcherQueue dispatcherQueue)
	{
		var response = new AttachmentDownloadChunkPacket();

		response.Unpack(packet);

		dispatcherQueue.TryEnqueue(async () =>
		{
			if (FriendsHelper.Friends.Count > 0)
			{
				var friend = FriendsHelper.Friends.FirstOrDefault(friend => friend.Guid == response.Guid);

				if (friend != null && friend.IsOnline)
				{
					var attachment = GetAttachment(response.Guid, response.Identifier);

					if (attachment != null)
					{
						string directoryPath = Path.Combine(StorageHelper.RootDirectory, "Attachments", friend.Name);

						attachment.FilePath = Path.Combine(directoryPath, attachment.FileName);

						Directory.CreateDirectory(directoryPath);

						attachment.Progress = (double)(response.CurrentChunk + 1) / response.Chunks * 100;

						using (FileStream fileStream = new FileStream(attachment.FilePath, FileMode.Append, FileAccess.Write))
						{
							fileStream.Write(response.Body, 0, response.CurrentChunkLength);
						}

						if (response.CurrentChunk < response.Chunks)
						{
							await App.Client.Send(new AttachmentDownloadChunkRequestPacket().Pack(
								   friend.Guid,
								   response.Identifier,
								   response.Chunks,
								   response.CurrentChunk + 1));
						}
						else
						{
							attachment.Progress = 100;
							ChatHelper.DownloadInProgress = false;
							attachment.AttachmentProgress = StatesHelper.AttachmentProgress.Completed;
						}
					}
				}
			}
		});
	}

	public async static void HandleAttachmentDownloadChunkRequest(Packet packet)
	{
		var request = new AttachmentDownloadChunkRequestPacket();

		request.Unpack(packet);

		if (FriendsHelper.Friends.Count > 0)
		{
			var friend = FriendsHelper.Friends.FirstOrDefault(friend => friend.Guid == request.Guid);

			if (friend != null && friend.IsOnline)
			{
				var attachmentRequest = GetAttachment(request.Guid, request.Identifier);

				if (attachmentRequest != null)
				{
					await Task.Run(async () =>
					{
						var chunks = CalculateChunks(attachmentRequest.ActualSize);
						var attachmentChunk = ReadChunkFromFile(attachmentRequest.FilePath, request.CurrentChunk);

						var attachmentDownloadChunkResponse = new AttachmentDownloadChunkPacket().Pack(
							friend.Guid,
							attachmentRequest.Identifier,
							chunks.Length,
							request.CurrentChunk,
							attachmentChunk);

						await App.Client.Send(attachmentDownloadChunkResponse);
					});
				}
			}
		}
	}

	public static Attachment GetAttachment(Guid guid, string identifier)
	{
		foreach (var chatDict in ChatHelper.Chat)
		{
			if (chatDict.TryGetValue(guid, out var chatItems))
			{
				foreach (var chatItem in chatItems)
				{
					if (chatItem is Attachment attachment && attachment.Identifier == identifier)
					{
						return attachment;
					}
				}
			}
		}

		return null;
	}

	private static int[] CalculateChunks(ulong fileSize)
	{
		int numChunks = (int)Math.Ceiling((double)fileSize / ChatHelper.ChunkSize);
		int[] chunkSizes = new int[numChunks];

		for (int i = 0; i < numChunks; i++)
		{
			chunkSizes[i] = (int)Math.Min((double)ChatHelper.ChunkSize, fileSize - (ulong)i * (uint)ChatHelper.ChunkSize);
		}

		return chunkSizes;
	}

	private static byte[] ReadChunkFromFile(string filePath, int chunkIndex)
	{
		byte[] chunkBytes = new byte[ChatHelper.ChunkSize];

		using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			fileStream.Seek((long)chunkIndex * ChatHelper.ChunkSize, SeekOrigin.Begin);
			int bytesRead = fileStream.Read(chunkBytes, 0, ChatHelper.ChunkSize);

			if (bytesRead < ChatHelper.ChunkSize)
			{
				Array.Resize(ref chunkBytes, bytesRead);
			}
		}

		return chunkBytes;
	}
}
