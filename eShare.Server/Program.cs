// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using eShare.Server;
using eShare.Networking.Utilities;

Logger.Initialize(Console.WriteLine);

await AddressHelper.Update();

ServerHandler.Start();
ServerHandler.PauseConsole();