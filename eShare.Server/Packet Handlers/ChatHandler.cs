// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using eShare.Networking;

namespace eShare.Server.PacketHandlers;

/// <summary>
///     Provides methods for handling message packets.
/// </summary>
internal class ChatHandler
{
	/// <summary>
	///		Handles the request to send a GIF between users.
	/// </summary>
	[PacketHandler((ushort)PacketIdentifiers.GIFSentRequest)]
	static async void HandleGIFSentRequest(Guid guid, Packet packet)
	{
		var request = new GIFPacket();

		request.Unpack(packet);

		if (ServerHandler.Users.ContainsKey(guid))
		{
			var senderGuid = ServerHandler.Users[guid];

			if (ServerHandler.Database.GetUserStatus(request.Guid) && ServerHandler.Users.ContainsValue(request.Guid))
			{
				var receiverGuid = ServerHandler.Users.FirstOrDefault(user => user.Value == request.Guid).Key;

				if (receiverGuid != Guid.Empty)
				{
					await ServerHandler.Server.Send(receiverGuid, new GIFPacket().Pack(senderGuid, request.GIFSource, PacketIdentifiers.GIFReceivedResponse));
				}
			}
		}
	}

	/// <summary>
	///		Handles the request to send a message between users.
	/// </summary>
	[PacketHandler((ushort)PacketIdentifiers.MessageSentRequest)]
	static async void HandleMessageSentRequest(Guid guid, Packet packet)
	{
		var request = new MessagePacket();

		request.Unpack(packet);

		if (ServerHandler.Users.ContainsKey(guid))
		{
			var senderGuid = ServerHandler.Users[guid];

			if (ServerHandler.Database.GetUserStatus(request.Guid) && ServerHandler.Users.ContainsValue(request.Guid))
			{
				var receiverGuid = ServerHandler.Users.FirstOrDefault(user => user.Value == request.Guid).Key;

				if (receiverGuid != Guid.Empty)
				{
					await ServerHandler.Server.Send(receiverGuid, new MessagePacket().Pack(senderGuid, request.Message, PacketIdentifiers.MessageReceivedResponse));
				}
			}
		}
	}

	/// <summary>
	///		Handles the request to send an attachment between users.
	/// </summary>
	[PacketHandler((ushort)PacketIdentifiers.AttachmentSentRequest)]
	static async void HandleAttachmentSentRequest(Guid guid, Packet packet)
	{
		var request = new AttachmentPacket();

		request.Unpack(packet);

		if (ServerHandler.Users.ContainsKey(guid))
		{
			var senderGuid = ServerHandler.Users[guid];

			if (ServerHandler.Database.GetUserStatus(request.Guid) && ServerHandler.Users.ContainsValue(request.Guid))
			{
				var receiverGuid = ServerHandler.Users.FirstOrDefault(user => user.Value == request.Guid).Key;

				if (receiverGuid != Guid.Empty)
				{
					await ServerHandler.Server.Send(receiverGuid, new AttachmentPacket().Pack(senderGuid, request.Identifier, request.FileSize, request.FileName, PacketIdentifiers.AttachmentReceivedResponse));
				}
			}
		}
	}

	/// <summary>
	///		Handles the request to download an attachment from another user.
	/// </summary>
	[PacketHandler((ushort)PacketIdentifiers.AttachmentDownloadRequest)]
	static async void HandleAttachmentDownloadRequest(Guid guid, Packet packet)
	{
		var request = new AttachmentDownloadPacket();

		request.Unpack(packet);

		if (ServerHandler.Users.ContainsKey(guid))
		{
			var senderGuid = ServerHandler.Users[guid];

			if (ServerHandler.Database.GetUserStatus(request.Guid) && ServerHandler.Users.ContainsValue(request.Guid))
			{
				var receiverGuid = ServerHandler.Users.FirstOrDefault(user => user.Value == request.Guid).Key;

				if (receiverGuid != Guid.Empty)
				{
					var relayRequest = new AttachmentDownloadPacket().Pack(senderGuid, request.Identifier, PacketIdentifiers.AttachmentDownloadRequest);

					await ServerHandler.Server.Send(receiverGuid, relayRequest);
				}
			}
		}
	}

	/// <summary>
	///		Handles the response to an attachment download request by sending chunks of the attachment.
	/// </summary>
	[PacketHandler((ushort)PacketIdentifiers.AttachmentDownloadChunkResponse)]
	static async void HandleAttachmentDownloadChunkResponse(Guid guid, Packet packet)
	{
		var response = new AttachmentDownloadChunkPacket();

		response.Unpack(packet);

		if (ServerHandler.Users.ContainsKey(guid))
		{
			var senderGuid = ServerHandler.Users[guid];

			if (ServerHandler.Database.GetUserStatus(response.Guid) && ServerHandler.Users.ContainsValue(response.Guid))
			{
				var receiverGuid = ServerHandler.Users.FirstOrDefault(user => user.Value == response.Guid).Key;

				if (receiverGuid != Guid.Empty)
				{
					response.Guid = senderGuid;

					var relayResponse = response.Repack();

					await ServerHandler.Server.Send(receiverGuid, relayResponse);
				}
			}
		}
	}

	/// <summary>
	///		Handles the request for a chunk of an attachment download.
	/// </summary>
	[PacketHandler((ushort)PacketIdentifiers.AttachmentDownloadChunkRequest)]
	static async void HandleAttachmentDownloadChunkRequest(Guid guid, Packet packet)
	{
		var response = new AttachmentDownloadChunkRequestPacket();

		response.Unpack(packet);

		if (ServerHandler.Users.ContainsKey(guid))
		{
			var senderGuid = ServerHandler.Users[guid];

			if (ServerHandler.Database.GetUserStatus(response.Guid) && ServerHandler.Users.ContainsValue(response.Guid))
			{
				var receiverGuid = ServerHandler.Users.FirstOrDefault(user => user.Value == response.Guid).Key;

				if (receiverGuid != Guid.Empty)
				{
					response.Guid = senderGuid;

					var relayResponse = response.Repack();

					await ServerHandler.Server.Send(receiverGuid, relayResponse);
				}
			}
		}
	}
}