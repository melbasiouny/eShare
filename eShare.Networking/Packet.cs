// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using System.IO.Compression;

namespace eShare.Networking;

/// <summary>
///     Provides functionality for converting packets to bytes and vice versa.
/// </summary>
public sealed class Packet
{
    /// <summary>
    ///     The maximum size of a packet.
    /// </summary>
    public const uint Size = 1 << 24;

    /// <summary>
    ///     The packet internal memory stream.
    /// </summary>
    private readonly MemoryStream _memoryStream = new();

    /// <summary>
    ///     The packet header.
    /// </summary>
    private (ushort identifier, bool compressed) _header;

    /// <summary>
    ///     Initializes a new packet using the provided buffer.
    /// </summary>
    /// <param name="buffer"> The packet raw buffer. </param>
    public Packet(byte[] buffer)
    {
        using var binaryReader = new BinaryReader(new MemoryStream(buffer));

        _header = (binaryReader.ReadUInt16(), binaryReader.ReadBoolean());
        _memoryStream = new MemoryStream(binaryReader.ReadBytes(buffer.Length));
    }

    /// <summary>
    ///     Initializes a new empty packet using the provided identifier.
    /// </summary>
    /// <param name="identifier"> The packet identifier. </param>
    /// <param name="binaryWriter"> The packet binary writer. </param>
    public Packet(ushort identifier, out BinaryWriter binaryWriter)
    {
        _header = (identifier, false);

        binaryWriter = new BinaryWriter(_memoryStream);
    }

    /// <summary>
    ///     Serializes the packet into an array of bytes suitable for transmission.
    /// </summary>
    /// <returns> The packet as a byte array. </returns>
    public byte[] Serialize()
    {
        using var memoryStream = new MemoryStream();
        using var binaryWriter = new BinaryWriter(memoryStream);

        var buffer = Compress(out _header.compressed);

        binaryWriter.Write(_header.identifier);
        binaryWriter.Write(_header.compressed);

        binaryWriter.Write(buffer);

        return memoryStream.ToArray();
    }

    /// <summary>
    ///     Deserializes the packet from the raw state.
    /// </summary>
    /// <returns> The packet binary reader. </returns>
    public BinaryReader Deserialize()
    {
        _memoryStream.Seek(0, SeekOrigin.Begin);

        var binaryReader = new BinaryReader(_memoryStream);

        return _header.compressed
            ? new BinaryReader(Decompress(binaryReader.ReadBytes(_memoryStream.ToArray().Length)))
            : binaryReader;
    }

    /// <summary>
    ///     Reads the packet identifier.
    /// </summary>
    /// <returns> The packet identifier. </returns>
    public ushort ReadIdentifier()
    {
        return _header.identifier;
    }

    /// <summary>
    ///     Attempts compressing the packet into a byte array.
    /// </summary>
    /// <param name="compressible"> Determines whether the packet is compressible or not. </param>
    /// <returns> The compressed packet if compressible. </returns>
    private byte[] Compress(out bool compressible)
    {
        using var destinationMemoryStream = new MemoryStream();
        using var sourceMemoryStream = new MemoryStream(_memoryStream.ToArray());

        using (var gZipStream = new GZipStream(destinationMemoryStream, CompressionMode.Compress))
        {
            sourceMemoryStream.CopyTo(gZipStream);
        }

        compressible = destinationMemoryStream.ToArray().Length <= _memoryStream.Length;

        return compressible ? destinationMemoryStream.ToArray() : _memoryStream.ToArray();
    }

    /// <summary>
    ///     Decompresses the provided packet using the provided raw buffer.
    /// </summary>
    /// <param name="buffer"> The packet raw buffer. </param>
    /// <returns> The decompressed packet memory stream. </returns>
    private static MemoryStream Decompress(byte[] buffer)
    {
        var destinationMemoryStream = new MemoryStream();
        var sourceMemoryStream = new MemoryStream(buffer);

        using (var gZipStream = new GZipStream(sourceMemoryStream, CompressionMode.Decompress))
        {
            gZipStream.CopyTo(destinationMemoryStream);
        }

        destinationMemoryStream.Seek(0, SeekOrigin.Begin);

        return destinationMemoryStream;
    }
}

/// <summary>
///     Provides the packet identifiers used internally.
/// </summary>
public enum InternalPacketIdentifiers : ushort
{
	DisconnectionRequest = ushort.MaxValue
}