// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using eShare.Networking;
using eShare.Client.Helpers;
using Microsoft.UI.Xaml;

namespace eShare.Client;

public partial class App : Application
{
	public static bool IsConnected { get; set; } = false;
	public static Networking.Client Client { get; private set; }
	public static StorageHelper Storage { get; private set; } = new();
	public static Window Window { get; private set; } = new MainWindow();

	public App()
	{
		this.InitializeComponent();
	}

	protected override async void OnLaunched(LaunchActivatedEventArgs args)
	{
		await AddressHelper.Update();

		Client = new Networking.Client(Globals.IPAddress.ToString(), 4000);

		Window.ExtendsContentIntoTitleBar = true;
		Window.Closed += OnClosed;
		Window.Title = "eShare";
		Window.Activate();

		new BackdropHelper().ActivateBackdrop();
	}

	private async void OnClosed(object sender, WindowEventArgs args)
	{
		if (IsConnected)
		{
			await Client.Send(new Packet((ushort)PacketIdentifiers.CloseSessionRequest, out var _));
		}

		Storage.SaveApplicationData();
	}
}
