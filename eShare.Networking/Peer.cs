// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using System.Net.Sockets;
using eShare.Networking.Utilities;

namespace eShare.Networking;

/// <summary>
///     Provides base functionality for network communications.
/// </summary>
internal class Peer
{
    /// <summary>
    ///     The peer GUID.
    /// </summary>
    private readonly Guid _guid;

    /// <summary>
    ///     The peer TCP network service and stream.
    /// </summary>
    private readonly (TcpClient tcpClient, NetworkStream networkStream) _peer;

    /// <summary>
    ///     The peer asynchronous operations.
    /// </summary>
    private (Task task, CancellationTokenSource cancellationTokenSource) _task;

    /// <summary>
    ///     Initializes a new peer using the provided TCP client.
    /// </summary>
    /// <param name="tcpClient"> The peer TCP client. </param>
    /// <param name="guid"> The peer GUID. </param>
    public Peer(TcpClient tcpClient, Guid guid = default)
    {
        _guid = guid;
        _peer.tcpClient = tcpClient;
        _peer.networkStream = _peer.tcpClient.GetStream();
        _peer.tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
    }

    /// <summary>
    ///     Invoked when the peer gets connected.
    /// </summary>
    public static event EventHandler<Guid>? Connected;

    /// <summary>
    ///     Invoked when the peer gets disconnected.
    /// </summary>
    public static event EventHandler<Guid>? Disconnected;

    /// <summary>
    ///     Invoked when the peer receives a packet.
    /// </summary>
    public static event EventHandler<PacketEventArgs>? PacketReceived;

    /// <summary>
    ///     Connects the peer.
    /// </summary>
    public void Connect()
    {
        _task.cancellationTokenSource = new CancellationTokenSource();
        _task.task = ReceiveAsync(_task.cancellationTokenSource.Token);

        Connected?.Invoke(this, _guid);
        Logger.Log(LogLevel.Information, $"Established connection with {_peer.tcpClient.Client.RemoteEndPoint}.");
    }

    /// <summary>
    ///     Disconnects the peer.
    /// </summary>
    public void Disconnect()
    {
		SendAsync(new Packet((ushort)InternalPacketIdentifiers.DisconnectionRequest, out var _)).Wait();
		Logger.Log(LogLevel.Information, $"Disconnected from {_peer.tcpClient.Client.RemoteEndPoint}.");

        _task.cancellationTokenSource.Cancel();
        _task.task.Wait();

        Disconnected?.Invoke(this, _guid);
    }

    /// <summary>
    ///     Sends a packet to the connected peer.
    /// </summary>
    /// <param name="packet"> The packet to be sent. </param>
    public async Task SendAsync(Packet packet)
    {
        if (_peer.networkStream == null || !_peer.networkStream.CanWrite)
        {
            Disconnected?.Invoke(this, _guid);
            Logger.Log(LogLevel.Error, $"Unable to send packet with identifier {packet.ReadIdentifier()}. The network stream is closed or not writable.");
            return;
        }

        var buffer = packet.Serialize();

        if (buffer.Length > Packet.Size)
        {
            Logger.Log(LogLevel.Warning, $"Dismissing large packet with identifier {packet.ReadIdentifier()}. Packet size {buffer.Length} exceeds the maximum size of {Packet.Size}.");
            return;
        }

        try
        {
            await _peer.networkStream.WriteAsync(BitConverter.GetBytes(buffer.Length)).ConfigureAwait(false);
            await _peer.networkStream.WriteAsync(buffer).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            Disconnected?.Invoke(this, _guid);
            Logger.Log(LogLevel.Error, $"Unable to send packet with identifier {packet.ReadIdentifier()}: {exception.Message}");
        }
        
        await Task.Delay(1);
    }

    /// <summary>
    ///     Receives packets from connected peers.
    /// </summary>
    /// <param name="cancellationToken"> The operation cancellation token. </param>
    private async Task ReceiveAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[Packet.Size];

        try
        {
            while (true)
            {
                var readAsync = await _peer.networkStream.ReadAsync(buffer.AsMemory(0, sizeof(uint)), cancellationToken).ConfigureAwait(false);
                var length = BitConverter.ToUInt32(buffer, 0);
                var offset = 0;

                if (readAsync == 0) throw new InvalidOperationException("Ambiguous packet length.");

                do
                {
                    readAsync = await _peer.networkStream.ReadAsync(buffer.AsMemory(offset, (int)(length - offset)), cancellationToken).ConfigureAwait(false);

                    if (readAsync == 0) throw new IOException("Peer unexpectedly ended the connection.");

                    offset += readAsync;
                } while (offset < length);

                var receivedPacket = new Packet(buffer.AsSpan(0, (int)length).ToArray());

                if (receivedPacket.ReadIdentifier() == ushort.MaxValue) throw new IOException("Peer ended the connection.");

                PacketReceived?.Invoke(this, new PacketEventArgs(_guid, receivedPacket));
                Logger.Log(LogLevel.Information, $"Packet received from {_peer.tcpClient.Client.RemoteEndPoint} with identifier {receivedPacket.ReadIdentifier()} and length {length} bytes.");
            }
        }
        catch (IOException)
        {
            Disconnected?.Invoke(this, _guid);
            Logger.Log(LogLevel.Warning, $"Peer {_peer.tcpClient.Client.RemoteEndPoint} disconnected.");

            if (_peer.tcpClient.Connected)
            {
                _peer.tcpClient.Client.Shutdown(SocketShutdown.Both);
                _peer.tcpClient.Client.Dispose();
            }
        }
        catch (Exception exception) when (exception is OperationCanceledException || exception is InvalidOperationException)
        {
            Logger.Log(LogLevel.Warning, $"Receive operation was cancelled.");
            return;
        }
    }
}