// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

namespace eShare.Networking;

/// <summary>
///     Provides the packet identifiers needed for client-server communication.
/// </summary
public enum PacketIdentifiers : ushort
{
	#region Session preparation packets
	/// <summary>
	///		Request for a unique identifier for a new session.
	/// </summary>
	GuidRequest,

	/// <summary>
	///		Response containing the unique identifier for a new session.
	/// </summary>
	GuidResponse,

	/// <summary>
	///		Request for the content of a session.
	/// </summary>
	ContentRequest,

	/// <summary>
	///		Response containing the content of a session.
	/// </summary>
	ContentResponse,

	/// <summary>
	///		Request to create a new session.
	/// </summary>
	CreateSessionRequest,

	/// <summary>
	///		Response indicating whether the session was successfully created.
	/// </summary>
	CreateSessionResponse,

	/// <summary>
	///		Request to continue an existing session.
	/// </summary>
	ContinueSessionRequest,

	/// <summary>
	///		Response indicating whether the session was successfully continued.
	/// </summary>
	ContinueSessionResponse,

	/// <summary>
	///		Request to close a session.
	/// </summary>
	CloseSessionRequest,

	/// <summary>
	///		Notification that a friend's session was closed.
	/// </summary>
	FriendSessionClosed,

	/// <summary>
	///		Notification that a friend's session was started.
	/// </summary>
	FriendSessionStarted,

	/// <summary>
	///		Request to retrieve the list of friends.
	/// </summary>
	FriendsListRequest,

	/// <summary>
	///		Response containing the list of friends.
	/// </summary>
	FriendsListResponse,
	#endregion

	#region Account packets
	/// <summary>
	///		Request to update the name of the user's account.
	/// </summary>
	UpdateAccountNameRequest,

	/// <summary>
	///		Request to update the picture of the user's account.
	/// </summary>
	UpdateAccountPictureRequest,

	/// <summary>
	///		Request to delete the user's account.
	/// </summary>
	DeleteAccountRequest,

	/// <summary>
	///		Response indicating whether the user's account was successfully deleted.
	/// </summary>
	DeleteAccountResponse,

	/// <summary>
	///		Request to delete a friend's account.
	/// </summary>
	FriendAccountDeleteRequest,

	/// <summary>
	///		Request to remove a friend from the user's friend list.
	/// </summary>
	FriendRemoveRequest,

	/// <summary>
	///		Request to update the name of a friend.
	/// </summary>
	FriendNameUpdateRequest,

	/// <summary>
	///		Request to update the picture of a friend.
	/// </summary>
	FriendPictureUpdateRequest,
	#endregion

	#region Friend request packets
	/// <summary>
	///		Request to add a friend.
	/// </summary>
	FriendRequest,

	/// <summary>
	///		Request to accept a friend request.
	/// </summary>
	AcceptFriendRequest,

	/// <summary>
	///		Response indicating whether a friend request was successfully accepted.
	/// </summary>
	AcceptFriendResponse,

	/// <summary>
	///		Notification that a friend request was sent.
	/// </summary>
	OutgoingFriendRequest,
	#endregion

	#region Chat packets
	/// <summary>
	///		Request to send a chat GIF.
	/// </summary>
	GIFSentRequest,

	/// <summary>
	///		Response to the request to send a chat GIF.
	/// </summary>
	GIFReceivedResponse,

	/// <summary>
	///		Request to send a chat message.
	/// </summary>
	MessageSentRequest,

	/// <summary>
	///		Response to the request to send a chat message.
	/// </summary>
	MessageReceivedResponse,

	/// <summary>
	///		Request to send a chat attachment.
	/// </summary>
	AttachmentSentRequest,

	/// <summary>
	///		Response to the request to send a chat attachment.
	/// </summary>
	AttachmentReceivedResponse,

	/// <summary>
	///		Request to download a chat attachment.
	/// </summary>
	AttachmentDownloadRequest,

	/// <summary>
	///		Request to download a chunk of a chat attachment.
	/// </summary>
	AttachmentDownloadChunkRequest,

	/// <summary>
	///		Response to the request to download a chunk of a chat attachment.
	/// </summary>
	AttachmentDownloadChunkResponse,
	#endregion

	#region Other packets
	/// <summary>
	///		Request to disconnect from the server.
	/// </summary>
	DisconnectionRequest
	#endregion
}

#region Session preparation packets
/// <summary>
///		Represents a packet that contains a GUID.
/// </summary>
public struct GuidPacket
{
	public Guid Guid { get; private set; }

	public Packet Pack(Guid guid)
	{
		var response = new Packet((ushort)PacketIdentifiers.GuidResponse, out var binaryWriter);

		binaryWriter.Write(guid.ToString());
		return response;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		Guid = Guid.Parse(binaryReader.ReadString());
	}
}

/// <summary>
///		Represents a packet that contains binary content.
/// </summary>
public struct ContentPacket
{
	public int Length { get; private set; }
	public byte[] Content { get; private set; }

	public Packet Pack(byte[] content)
	{
		var response = new Packet((ushort)PacketIdentifiers.ContentResponse, out var binaryWriter);

		binaryWriter.Write(content.Length);
		binaryWriter.Write(content);
		return response;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		Length = binaryReader.ReadInt32();
		Content = binaryReader.ReadBytes(Length);
	}
}

/// <summary>
///		Represents a packet that is used to create a new session.
/// </summary>
public struct CreateSessionPacket
{
	public Guid Guid { get; private set; }
	public string Name { get; private set; }

	public Packet Pack(Guid guid, string name)
	{
		var request = new Packet((ushort)PacketIdentifiers.CreateSessionRequest, out var binaryWriter);

		binaryWriter.Write(guid.ToString());
		binaryWriter.Write(name);
		return request;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		Guid = Guid.Parse(binaryReader.ReadString());
		Name = binaryReader.ReadString();
	}
}

/// <summary>
///		Represents a packet that is used to continue an existing session.
/// </summary>
public struct ContinueSessionPacket
{
	public Guid Guid { get; private set; }

	public Packet Pack(Guid guid)
	{
		var request = new Packet((ushort)PacketIdentifiers.ContinueSessionRequest, out var binaryWriter);

		binaryWriter.Write(guid.ToString());
		return request;
	}
	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		Guid = Guid.Parse(binaryReader.ReadString());
	}
}

/// <summary>
///		Represents a packet that is used to communicate with a friend's session.
/// </summary>
public struct FriendSessionPacket
{
	public Guid Guid { get; private set; }

	public Packet Pack(Guid guid, PacketIdentifiers packetIdentifier)
	{
		var packet = new Packet((ushort)packetIdentifier, out var binaryWriter);

		binaryWriter.Write(guid.ToString());
		return packet;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		Guid = Guid.Parse(binaryReader.ReadString());
	}
}

/// <summary>
///		Represents a packet that contains a list of friends.
/// </summary>
public struct FriendsListPacket
{
	public List<FriendTemplate> Friends { get; private set; }

	public Packet Pack(List<FriendTemplate> friends)
	{
		var response = new Packet((ushort)PacketIdentifiers.FriendsListResponse, out var binaryWriter);

		binaryWriter.Write(friends.Count);

		foreach (var friend in friends)
		{
			binaryWriter.Write(friend.Guid.ToString());
			binaryWriter.Write(friend.Status);
			binaryWriter.Write(friend.ProfileName);
			binaryWriter.Write(friend.ProfilePicture);
		}

		return response;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();
		var count = binaryReader.ReadInt32();

		Friends = new List<FriendTemplate>();

		for (int friend = 0; friend < count; friend++)
		{
			Friends.Add(new FriendTemplate(
				Guid.Parse(binaryReader.ReadString()),
				binaryReader.ReadBoolean(),
				binaryReader.ReadString(),
				binaryReader.ReadInt32()));
		}
	}
}

/// <summary>
///		Represents a friend in the friends list.
/// </summary>
public struct FriendTemplate
{
	public Guid Guid { get; set; }
	public bool Status { get; set; }
	public string ProfileName { get; set; }
	public int ProfilePicture { get; set; }

	public FriendTemplate(Guid guid, bool status, string profileName, int profilePicture)
	{
		Guid = guid;
		Status = status;
		ProfileName = profileName;
		ProfilePicture = profilePicture;
	}
}
#endregion

#region Account packets
/// <summary>
///		Represents a packet that contains a profile name update request.
/// </summary>
public struct UpdateAccountNamePacket
{
	public string ProfileName { get; private set; }

	public Packet Pack(string profileName)
	{
		var request = new Packet((ushort)PacketIdentifiers.UpdateAccountNameRequest, out var binaryWriter);

		binaryWriter.Write(profileName);
		return request;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		ProfileName = binaryReader.ReadString();
	}
}

/// <summary>
///		Represents a packet that contains a profile picture update request.
/// </summary>
public struct UpdateAccountPicturePacket
{
	public int ProfilePicture { get; private set; }

	public Packet Pack(int profilePicture)
	{
		var request = new Packet((ushort)PacketIdentifiers.UpdateAccountPictureRequest, out var binaryWriter);

		binaryWriter.Write(profilePicture);
		return request;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		ProfilePicture = binaryReader.ReadInt32();
	}
}

/// <summary>
///		Represents a packet that contains a friend name update request.
/// </summary>
public struct FriendNameUpdatePacket
{
	public Guid Guid { get; private set; }
	public string Name { get; private set; }

	public Packet Pack(Guid guid, string name)
	{
		var request = new Packet((ushort)PacketIdentifiers.FriendNameUpdateRequest, out var binaryWriter);

		binaryWriter.Write(guid.ToString());
		binaryWriter.Write(name);
		return request;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		Guid = Guid.Parse(binaryReader.ReadString());
		Name = binaryReader.ReadString();
	}
}

/// <summary>
///		Represents a packet that contains a friend profile picture update request.
/// </summary>
public struct FriendPictureUpdatePacket
{
	public Guid Guid { get; private set; }
	public int ProfilePicture { get; private set; }

	public Packet Pack(Guid guid, int profilePicture)
	{
		var request = new Packet((ushort)PacketIdentifiers.FriendPictureUpdateRequest, out var binaryWriter);

		binaryWriter.Write(guid.ToString());
		binaryWriter.Write(profilePicture);
		return request;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		Guid = Guid.Parse(binaryReader.ReadString());
		ProfilePicture = binaryReader.ReadInt32();
	}
}

/// <summary>
///		Represents a packet that contains a friend account delete request.
/// </summary>
public struct FriendAccountDeletePacket
{
	public Guid Guid { get; private set; }

	public Packet Pack(Guid guid)
	{
		var request = new Packet((ushort)PacketIdentifiers.FriendAccountDeleteRequest, out var binaryWriter);

		binaryWriter.Write(guid.ToString());
		return request;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		Guid = Guid.Parse(binaryReader.ReadString());
	}
}

/// <summary>
///		 Represents a packet that contains a friend account delete request.
/// </summary>
public struct FriendRemovePacket
{
	public Guid Guid { get; private set; }

	public Packet Pack(Guid guid)
	{
		var request = new Packet((ushort)PacketIdentifiers.FriendRemoveRequest, out var binaryWriter);

		binaryWriter.Write(guid.ToString());
		return request;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		Guid = Guid.Parse(binaryReader.ReadString());
	}
}
#endregion

#region Friend request packets
/// <summary>
///		Represents a packet containing information about a friend.
/// </summary>
public struct FriendPacket
{
	public Guid Guid { get; private set; }
	public string ProfileName { get; private set; }
	public int ProfilePicture { get; private set; }

	public Packet Pack(Guid guid, string profileName, int profilePicture, PacketIdentifiers packetIdentifier)
	{
		var packet = new Packet((ushort)packetIdentifier, out var binaryWriter);

		binaryWriter.Write(guid.ToString());
		binaryWriter.Write(profileName);
		binaryWriter.Write(profilePicture);
		return packet;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		Guid = Guid.Parse(binaryReader.ReadString());
		ProfileName = binaryReader.ReadString();
		ProfilePicture = binaryReader.ReadInt32();
	}
}

/// <summary>
///		Represents a packet containing a friend request.
/// </summary>
public struct FriendRequestPacket
{
	public Guid SenderGuid { get; private set; }
	public string SenderName { get; private set; }
	public int SenderProfilePicture { get; private set; }

	public Packet Pack(Guid senderGuid, string senderName, int senderProfilePicture)
	{
		var request = new Packet((ushort)PacketIdentifiers.FriendRequest, out var binaryWriter);

		binaryWriter.Write(senderGuid.ToString());
		binaryWriter.Write(senderName);
		binaryWriter.Write(senderProfilePicture);
		return request;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		SenderGuid = Guid.Parse(binaryReader.ReadString());
		SenderName = binaryReader.ReadString();
		SenderProfilePicture = binaryReader.ReadInt32();
	}
}

/// <summary>
///		Represents a packet containing information about an accepted friend request.
/// </summary>
public struct AcceptFriendRequestPacket
{
	public Guid SenderGuid { get; private set; }
	public Guid ReceiverGuid { get; private set; }

	public Packet Pack(Guid senderGuid, Guid receiverGuid)
	{
		var request = new Packet((ushort)PacketIdentifiers.AcceptFriendRequest, out var binaryWriter);

		binaryWriter.Write(senderGuid.ToString());
		binaryWriter.Write(receiverGuid.ToString());
		return request;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		SenderGuid = Guid.Parse(binaryReader.ReadString());
		ReceiverGuid = Guid.Parse(binaryReader.ReadString());
	}
}

/// <summary>
///		Represents a packet containing information about a friend request sent by the user.
/// </summary>
public struct OutgoingFriendRequestPacket
{
	public Guid ReceiverGuid { get; private set; }

	public Packet Pack(Guid receiverGuid)
	{
		var request = new Packet((ushort)PacketIdentifiers.OutgoingFriendRequest, out var binaryWriter);

		binaryWriter.Write(receiverGuid.ToString());
		return request;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		ReceiverGuid = Guid.Parse(binaryReader.ReadString());
	}
}
#endregion

#region Chat packets
/// <summary>
///		Represents a packet for sending a GIF.
/// </summary>
public struct GIFPacket
{
	public Guid Guid { get; private set; }
	public string GIFSource { get; private set; }

	public Packet Pack(Guid guid, string gifSource, PacketIdentifiers packetIdentifier)
	{
		var packet = new Packet((ushort)packetIdentifier, out var binaryWriter);

		binaryWriter.Write(guid.ToString());
		binaryWriter.Write(gifSource);
		return packet;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		Guid = Guid.Parse(binaryReader.ReadString());
		GIFSource = binaryReader.ReadString();
	}
}

/// <summary>
///		Represents a packet for sending a message.
/// </summary>
public struct MessagePacket
{
	public Guid Guid { get; private set; }
	public string Message { get; private set; }

	public Packet Pack(Guid guid, string message, PacketIdentifiers packetIdentifier)
	{
		var packet = new Packet((ushort)packetIdentifier, out var binaryWriter);

		binaryWriter.Write(guid.ToString());
		binaryWriter.Write(message);
		return packet;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		Guid = Guid.Parse(binaryReader.ReadString());
		Message = binaryReader.ReadString();
	}
}

/// <summary>
///		Represents a packet for sending an attachment.
/// </summary>
public struct AttachmentPacket
{
	public Guid Guid { get; private set; }
	public string Identifier { get; private set; }
	public string FileSize { get; private set; }
	public string FileName { get; private set; }

	public Packet Pack(Guid guid, string identifier, string fileSize, string fileName, PacketIdentifiers packetIdentifier)
	{
		var packet = new Packet((ushort)packetIdentifier, out var binaryWriter);

		binaryWriter.Write(guid.ToString());
		binaryWriter.Write(identifier);
		binaryWriter.Write(fileSize);
		binaryWriter.Write(fileName);
		return packet;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		Guid = Guid.Parse(binaryReader.ReadString());
		Identifier = binaryReader.ReadString();
		FileSize = binaryReader.ReadString();
		FileName = binaryReader.ReadString();
	}
}

/// <summary>
///		Represents a packet for requesting the download of an attachment.
/// </summary>
public struct AttachmentDownloadPacket
{
	public Guid Guid { get; private set; }
	public string Identifier { get; private set; }

	public Packet Pack(Guid guid, string identifier, PacketIdentifiers packetIdentifier)
	{
		var packet = new Packet((ushort)packetIdentifier, out var binaryWriter);

		binaryWriter.Write(guid.ToString());
		binaryWriter.Write(identifier);
		return packet;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		Guid = Guid.Parse(binaryReader.ReadString());
		Identifier = binaryReader.ReadString();
	}
}

/// <summary>
///		Represents a packet for sending a chunk of an attachment during download.
/// </summary>
public struct AttachmentDownloadChunkPacket
{
	public Guid Guid { get; set; }
	public string Identifier { get; private set; }
	public int Chunks { get; private set; }
	public int CurrentChunk { get; private set; }
	public int CurrentChunkLength { get; private set; }
	public byte[] Body { get; private set; }

	public Packet Pack(Guid guid, string identifier, int chunks, int currentChunk, byte[] body)
	{
		var response = new Packet((ushort)PacketIdentifiers.AttachmentDownloadChunkResponse, out var binaryWriter);

		binaryWriter.Write(guid.ToString());
		binaryWriter.Write(identifier);
		binaryWriter.Write(chunks);
		binaryWriter.Write(currentChunk);
		binaryWriter.Write(body.Length);
		binaryWriter.Write(body);
		return response;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		Guid = Guid.Parse(binaryReader.ReadString());
		Identifier = binaryReader.ReadString();
		Chunks = binaryReader.ReadInt32();
		CurrentChunk = binaryReader.ReadInt32();
		CurrentChunkLength = binaryReader.ReadInt32();
		Body = binaryReader.ReadBytes(CurrentChunkLength);
	}

	public Packet Repack()
	{
		var response = new Packet((ushort)PacketIdentifiers.AttachmentDownloadChunkResponse, out var binaryWriter);

		binaryWriter.Write(Guid.ToString());
		binaryWriter.Write(Identifier);
		binaryWriter.Write(Chunks);
		binaryWriter.Write(CurrentChunk);
		binaryWriter.Write(CurrentChunkLength);
		binaryWriter.Write(Body);
		return response;
	}
}

/// <summary>
///		Represents a packet for requesting a chunk of an attachment during download.
/// </summary>
public struct AttachmentDownloadChunkRequestPacket
{
	public Guid Guid { get; set; }
	public string Identifier { get; private set; }
	public int Chunks { get; private set; }
	public int CurrentChunk { get; private set; }

	public Packet Pack(Guid guid, string identifier, int chunks, int currentChunk)
	{
		var response = new Packet((ushort)PacketIdentifiers.AttachmentDownloadChunkRequest, out var binaryWriter);

		binaryWriter.Write(guid.ToString());
		binaryWriter.Write(identifier);
		binaryWriter.Write(chunks);
		binaryWriter.Write(currentChunk);
		return response;
	}

	public void Unpack(Packet packet)
	{
		var binaryReader = packet.Deserialize();

		Guid = Guid.Parse(binaryReader.ReadString());
		Identifier = binaryReader.ReadString();
		Chunks = binaryReader.ReadInt32();
		CurrentChunk = binaryReader.ReadInt32();
	}

	public Packet Repack()
	{
		var response = new Packet((ushort)PacketIdentifiers.AttachmentDownloadChunkRequest, out var binaryWriter);

		binaryWriter.Write(Guid.ToString());
		binaryWriter.Write(Identifier);
		binaryWriter.Write(Chunks);
		binaryWriter.Write(CurrentChunk);
		return response;
	}
}
#endregion