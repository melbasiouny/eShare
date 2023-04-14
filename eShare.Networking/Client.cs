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
///     Provides functionality for managing a client.
/// </summary>
public class Client
{
    /// <summary>
    ///     The remote TCP host.
    /// </summary>
    private readonly (string ipAddress, ushort port) _host;

    /// <summary>
    ///     The client TCP network service.
    /// </summary>
    private readonly TcpClient _tcpClient;

    /// <summary>
    ///     Packet handlers accessible by their identifier.
    /// </summary>
    private Dictionary<ushort, PacketHandler> _packetHandlers = new();

    /// <summary>
    ///     The client connected peer.
    /// </summary>
    private Peer? _peer;

    /// <summary>
    ///     Initializes a new client using the provided IP address and port.
    /// </summary>
    /// <param name="ipAddress"> The server IP address. </param>
    /// <param name="port"> The port number. </param>
    public Client(string ipAddress, ushort port)
    {
        FindPacketHandlers();

        _host = (ipAddress, port);
        _tcpClient = new TcpClient();
    }

    /// <summary>
    ///     Connects the client.
    /// </summary>
    public void Connect()
    {
        try
        {
            _tcpClient.Connect(IPAddress.Parse(_host.ipAddress), _host.port);
            _peer = new Peer(_tcpClient);
        }
        catch (Exception exception)
        {
            Logger.Log(LogLevel.Error, exception.Message);
            ConnectionRefused?.Invoke(this, EventArgs.Empty);
        }

        Peer.Connected += delegate { Connected?.Invoke(this, EventArgs.Empty); };

        Peer.Disconnected += delegate { Disconnected?.Invoke(this, EventArgs.Empty); };

        Peer.PacketReceived += delegate(object? _, PacketEventArgs packetEventArgs)
        {
            if (_packetHandlers.TryGetValue(packetEventArgs.Packet.ReadIdentifier(), out var packetHandler)) packetHandler(packetEventArgs.Packet);
            PacketReceived?.Invoke(this, packetEventArgs.Packet);
        };

        _peer?.Connect();
    }

    /// <summary>
    ///     Disconnects the client.
    /// </summary>
    public void Disconnect()
    {
        _peer?.Disconnect();
    }

    /// <summary>
    ///     Sends a packet to the server.
    /// </summary>
    /// <param name="packet"> The packet to be sent. </param>
    public async Task Send(Packet packet)
    {
        await _peer!.SendAsync(packet);
    }

    /// <summary>
    ///     Invoked when the client gets connected.
    /// </summary>
    public event EventHandler? Connected;

    /// <summary>
    ///     Invoked when the client gets disconnected.
    /// </summary>
    public event EventHandler? Disconnected;

    /// <summary>
    ///     Invoked when the client is actively refused connection.
    /// </summary>
    public event EventHandler? ConnectionRefused;

    /// <summary>
    ///     Invoked when the client receives a packet.
    /// </summary>
    public event EventHandler<Packet>? PacketReceived;

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
    /// <param name="packet"> The packet received. </param>
    private delegate void PacketHandler(Packet packet);
}