// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using System.Net;
using System.Net.Sockets;
using System.Reflection;
using eShare.Networking.Utilities;

namespace eShare.Networking;

/// <summary>
///     Provides functionality for managing a server.
/// </summary>
public class Server
{
    /// <summary>
    ///     Connected clients accessible by their GUID.
    /// </summary>
    private static readonly Dictionary<Guid, Peer> Clients = new();

    /// <summary>
    ///     The server TCP network listener.
    /// </summary>
    private readonly TcpListener _tcpListener;

    /// <summary>
    ///     Packet handlers accessible by their identifier.
    /// </summary>
    private Dictionary<ushort, PacketHandler> _packetHandlers = new();

    /// <summary>
    ///     Initializes a new server using the provided port.
    /// </summary>
    /// <param name="port"> The port number. </param>
    public Server(ushort port)
    {
        FindPacketHandlers();

        _tcpListener = new TcpListener(IPAddress.Any, port);
    }

    /// <summary>
    ///     Invoked when a client gets connected.
    /// </summary>
    public event EventHandler<Guid>? Connected;

    /// <summary>
    ///     Invoked when a client gets disconnected.
    /// </summary>
    public event EventHandler<Guid>? Disconnected;

    /// <summary>
    ///     Invoked when a packet is received.
    /// </summary>
    public event EventHandler<PacketEventArgs>? PacketReceived;

    /// <summary>
    ///     Begins accepting incoming connections.
    /// </summary>
    public void Start()
    {
        Peer.Connected += delegate(object? _, Guid guid) { Connected?.Invoke(this, guid); };

        Peer.Disconnected += delegate(object? _, Guid guid)
        {
            Clients.Remove(guid);
            Disconnected?.Invoke(this, guid);
        };

        Peer.PacketReceived += delegate(object? _, PacketEventArgs packetEventArgs)
        {
            if (_packetHandlers.TryGetValue(packetEventArgs.Packet.ReadIdentifier(), out var packetHandler))
                packetHandler(packetEventArgs.Guid, packetEventArgs.Packet);

            PacketReceived?.Invoke(this, packetEventArgs);
        };

        try
        {
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(ConnectCallback, null);
        }
        catch (SocketException socketException)
        {
            Logger.Log(LogLevel.Error, $"The port number {((IPEndPoint)_tcpListener.LocalEndpoint).Port} is already in use.");
            Environment.Exit(socketException.ErrorCode);
        }

        Logger.Log(LogLevel.Information, "Listening for incoming connections.");
    }

    /// <summary>
    ///     Shuts down the server disconnecting all connected clients.
    /// </summary>
    public void Shutdown()
    {
        foreach (var peer in Clients.Values) peer.Disconnect();

        _tcpListener.Stop();
    }

    /// <summary>
    ///     Disconnects a connected client using their provided GUID.
    /// </summary>
    /// <param name="guid"> The client GUID. </param>
    public void Disconnect(Guid guid)
    {
        if (Clients.TryGetValue(guid, out var peer)) peer.Disconnect();
    }

    /// <summary>
    ///     Sends a packet to a connected client.
    /// </summary>
    /// <param name="guid"> The client GUID. </param>
    /// <param name="packet"> The packet to be sent. </param>
    public async Task Send(Guid guid, Packet packet)
    {
        if (Clients.TryGetValue(guid, out var peer)) await peer.SendAsync(packet);
    }

    /// <summary>
    ///     Sends a packet to all connected clients except the one provided.
    /// </summary>
    /// <param name="guid"> The exception client GUID. </param>
    /// <param name="packet"> The packet to be sent. </param>
    public async Task Broadcast(Guid guid, Packet packet)
    {
        foreach (var peer in Clients.Where(peer => peer.Key != guid))
            await peer.Value.SendAsync(packet);
    }

    /// <summary>
    ///     Invoked when a connection is established.
    /// </summary>
    /// <param name="asyncResult"> The status of the operation. </param>
    private void ConnectCallback(IAsyncResult asyncResult)
    {
        var client = _tcpListener.EndAcceptTcpClient(asyncResult);
        var guid = Guid.NewGuid();

        Clients.Add(guid, new Peer(client, guid));
        Clients[guid].Connect();

        _tcpListener.BeginAcceptTcpClient(ConnectCallback, null);
    }

    /// <summary>
    ///     Retrieves methods that has the <see cref="PacketHandlerAttribute" /> applied.
    /// </summary>
    private void FindPacketHandlers()
    {
        var methodInfos = PacketHandlerAttribute.FindPacketHandlers();

        _packetHandlers = new Dictionary<ushort, PacketHandler>(methodInfos.Length);

        foreach (var methodInfo in methodInfos)
        {
            var packetHandlerAttribute = methodInfo.GetCustomAttribute<PacketHandlerAttribute>();
            var packetHandler = Delegate.CreateDelegate(typeof(PacketHandler), methodInfo, false);

            if (packetHandlerAttribute == null || packetHandler == null) continue;
            if (_packetHandlers.ContainsKey(packetHandlerAttribute.Identifier)) continue;

            _packetHandlers.Add(packetHandlerAttribute.Identifier, (PacketHandler)packetHandler);
            Logger.Log(LogLevel.Information, $"Packet handler for packets of identifier {packetHandlerAttribute.Identifier} found.");
        }
    }

    /// <summary>
    ///     Encapsulates a packet handler method.
    /// </summary>
    /// <param name="guid"> The sender GUID. </param>
    /// <param name="packet"> The packet received. </param>
    private delegate void PacketHandler(Guid guid, Packet packet);
}