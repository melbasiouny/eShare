// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using eShare.Networking;
using eShare.Client.Helpers;
using eShare.Client.PacketHandlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace eShare.Client.Pages.Setup;

public sealed partial class SessionPreparationPage : Page
{
	public static (Guid Guid, string Name) TemporaryStorage;

	public SessionPreparationPage()
	{
		this.InitializeComponent();
		this.InitializePage();
	}

	private void InitializePage()
	{
		App.Client.Connected += Client_Connected;
		App.Client.PacketReceived += Client_PacketReceived;
		App.Client.Disconnected += Client_Disconnected;
		App.Client.ConnectionRefused += Client_Disconnected;

		if (!App.IsConnected)
		{
			Task.Run(App.Client.Connect);
		}
		else
		{
			Client_Connected(this, EventArgs.Empty);
		}
	}

	private async void Client_Connected(object sender, EventArgs args)
	{
		App.IsConnected = true;

		await App.Client.Send(new Packet((ushort)PacketIdentifiers.ContentRequest, out var _));
	}

	private async void Client_PacketReceived(object sender, Packet packet)
	{
		switch (packet.ReadIdentifier())
		{
			case (ushort)PacketIdentifiers.GuidResponse:
				SessionPreparationHandler.HandleGuidResponse(packet);
				NavigateTo(typeof(AccountCreationPage));
				break;

			case (ushort)PacketIdentifiers.ContentResponse:
				SessionPreparationHandler.HandleContentResponse(packet);

				if (App.Storage.Guid == Guid.Empty)
				{
					App.Storage.ResetApplicationData();

					await App.Client.Send(new Packet((ushort)PacketIdentifiers.GuidRequest, out var _));
				}
				else
				{
					await App.Client.Send(new ContinueSessionPacket().Pack(App.Storage.Guid));
				}

				break;

			case (ushort)PacketIdentifiers.CreateSessionResponse:
				App.Storage.Guid = TemporaryStorage.Guid;
				App.Storage.Name = TemporaryStorage.Name;

				NavigateTo(typeof(MainPage));
				break;

			case (ushort)PacketIdentifiers.ContinueSessionResponse:
				await App.Client.Send(new Packet((ushort)PacketIdentifiers.FriendsListRequest, out var _));
				break;

			case (ushort)PacketIdentifiers.FriendSessionClosed:
				SessionPreparationHandler.HandleFriendSessionClosed(packet, this.DispatcherQueue);
				break;

			case (ushort)PacketIdentifiers.FriendSessionStarted:
				SessionPreparationHandler.HandleFriendSessionStarted(packet, this.DispatcherQueue);
				break;

			case (ushort)PacketIdentifiers.FriendsListResponse:
				SessionPreparationHandler.HandleFriendsListResponse(packet, this.DispatcherQueue);
				NavigateTo(typeof(MainPage));
				break;

			case (ushort)PacketIdentifiers.DeleteAccountResponse:
				App.Storage.ResetApplicationData();

				App.Client.Connected -= Client_Connected;
				App.Client.PacketReceived -= Client_PacketReceived;
				App.Client.Disconnected -= Client_Disconnected;
				App.Client.ConnectionRefused -= Client_Disconnected;

				this.DispatcherQueue.TryEnqueue(() =>
				{
					FriendsHelper.Friends.Clear();
					ChatHelper.Chat.Clear();
					FriendRequestHelper.FriendRequests.Clear();

					SettingsPage.SetAppTheme();
					SettingsPage.SetSound();
				});

				NavigateTo(typeof(SessionPreparationPage));
				break;

			case (ushort)PacketIdentifiers.FriendAccountDeleteRequest:
				AccountHandler.HandleFriendAccountDeleteRequest(packet, this.DispatcherQueue);
				break;

			case (ushort)PacketIdentifiers.FriendRemoveRequest:
				AccountHandler.HandleFriendRemoveRequest(packet, this.DispatcherQueue);
				break;

			case (ushort)PacketIdentifiers.FriendNameUpdateRequest:
				AccountHandler.HandleFriendNameUpdateRequest(packet, this.DispatcherQueue);
				break;

			case (ushort)PacketIdentifiers.FriendPictureUpdateRequest:
				AccountHandler.HandleFriendPictureUpdateRequest(packet, this.DispatcherQueue);
				break;

			case (ushort)PacketIdentifiers.FriendRequest:
				FriendRequestHandler.HandleFriendRequest(packet, this.DispatcherQueue);
				break;

			case (ushort)PacketIdentifiers.AcceptFriendResponse:
				FriendRequestHandler.HandleAcceptFriendResponse(packet, this.DispatcherQueue);
				break;

			case (ushort)PacketIdentifiers.GIFReceivedResponse:
				ChatHandler.HandleGIFReceivedResponse(packet, this.DispatcherQueue);
				break;

			case (ushort)PacketIdentifiers.MessageReceivedResponse:
				ChatHandler.HandleMessageReceivedResponse(packet, this.DispatcherQueue);
				break;

			case (ushort)PacketIdentifiers.AttachmentReceivedResponse:
				ChatHandler.HandleAttachmentReceivedResponse(packet, this.DispatcherQueue);
				break;

			case (ushort)PacketIdentifiers.AttachmentDownloadRequest:
				ChatHandler.HandleAttachmentDownloadRequest(packet);
				break;

			case (ushort)PacketIdentifiers.AttachmentDownloadChunkResponse:
				ChatHandler.HandleAttachmentDownloadChunkResponse(packet, this.DispatcherQueue);
				break;

			case (ushort)PacketIdentifiers.AttachmentDownloadChunkRequest:
				ChatHandler.HandleAttachmentDownloadChunkRequest(packet);
				break;

			case (ushort)PacketIdentifiers.DisconnectionRequest:
				App.IsConnected = false;

				App.Client.Connected -= Client_Connected;
				App.Client.PacketReceived -= Client_PacketReceived;
				App.Client.Disconnected -= Client_Disconnected;
				App.Client.ConnectionRefused -= Client_Disconnected;

				Client_Disconnected(this, EventArgs.Empty);
				break;
		}
	}

	private void Client_Disconnected(object sender, EventArgs args)
	{
		App.IsConnected = false;

		this.DispatcherQueue.TryEnqueue(async () =>
		{
			var dialog = new ContentDialog();

			dialog.Title = "Unable to connect to server";
			dialog.Content = "We're sorry, but we were unable to connect to the server. Please check your internet connection and try again.";
			dialog.PrimaryButtonText = "Quit";
			dialog.XamlRoot = Content.XamlRoot;
			dialog.DefaultButton = ContentDialogButton.Primary;
			dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;

			dialog.Loaded += OnLoaded;

			void OnLoaded(object sender, RoutedEventArgs args)
			{
				Rectangle smokeLayer = null;
				DependencyObject popupRoot = VisualTreeHelper.GetParent(dialog);

				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(popupRoot); i++)
				{
					var child = VisualTreeHelper.GetChild(popupRoot, i);

					if (child is Rectangle rectangle) { smokeLayer = rectangle; break; }
				}

				if (smokeLayer != null)
				{
					smokeLayer.Margin = new Thickness(0);
					smokeLayer.RegisterPropertyChangedCallback(MarginProperty, (dependencyObject, dependencyProperty) =>
					{
						if (dependencyProperty == MarginProperty) dependencyObject.ClearValue(dependencyProperty);
					});
				}
			}

			var result = await dialog.ShowAsync();

			if (result == ContentDialogResult.Primary) App.Window.Close();
		});
	}

	private void NavigateTo(Type page)
	{
		this.DispatcherQueue.TryEnqueue(() => Frame.Navigate(page));
	}
}
