// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using System.Reflection;

namespace eShare.Networking;

/// <summary>
///     Provides functionality for handling a <see cref="Packet" /> by its identifier.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class PacketHandlerAttribute : Attribute
{
    /// <summary>
    ///     The packet handler identifier.
    /// </summary>
    internal readonly ushort Identifier;

    /// <summary>
    ///     Initializes a new packet handler using the provided identifier.
    /// </summary>
    /// <param name="identifier"> The packet handler identifier. </param>
    public PacketHandlerAttribute(ushort identifier)
    {
        Identifier = identifier;
    }

    /// <summary>
    ///     Retrieves methods that has the <see cref="PacketHandlerAttribute" /> applied.
    /// </summary>
    /// <returns> An array containing packet handler methods. </returns>
    internal static MethodInfo[] FindPacketHandlers()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            .Where(info => info.GetCustomAttributes(typeof(PacketHandlerAttribute), false).Length > 0)
            .ToArray();
    }
}