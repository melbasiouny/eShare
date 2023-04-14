// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

namespace eShare.Networking;

/// <summary>
///     Contains event data for received packets.
/// </summary>
public class PacketEventArgs : EventArgs
{
    /// <summary>
    ///     The sender GUID.
    /// </summary>
    public readonly Guid Guid;

    /// <summary>
    ///     The received packet.
    /// </summary>
    public readonly Packet Packet;

    /// <summary>
    ///     Initializes the event data using the provided GUID, packet and identifier.
    /// </summary>
    /// <param name="guid"> The sender GUID. </param>
    /// <param name="packet"> The received packet. </param>
    public PacketEventArgs(Guid guid, Packet packet)
    {
        Guid = guid;
        Packet = packet;
    }
}