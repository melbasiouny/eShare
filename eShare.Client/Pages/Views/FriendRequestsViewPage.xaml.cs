// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using eShare.Networking;
using eShare.Client.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Specialized;

namespace eShare.Client.Pages.Views;

public sealed partial class FriendRequestsViewPage : Page
{
	public FriendRequestsViewPage()
	{
		this.InitializeComponent();
		this.InitializePage();
	}

	private void InitializePage()
	{
		SendFriendRequestButton.IsEnabled = false;
		IncomingRequestsNotice.IsOpen = !App.Storage.IncomingRequests;
		FriendRequestsList.ItemsSource = FriendRequestHelper.FriendRequests;
		FriendRequestHelper.FriendRequests.CollectionChanged += FriendRequests_CollectionChanged;
		DefaultView.Visibility = FriendRequestHelper.FriendRequests.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
	}

	private void FriendRequests_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		DefaultView.Visibility = FriendRequestHelper.FriendRequests.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
	}

	private async void SendFriendRequestButton_Click(object sender, RoutedEventArgs args)
	{
		await App.Client.Send(new OutgoingFriendRequestPacket().Pack(Guid.Parse(FriendGuid.Text)));

		FriendGuid.Text = string.Empty;
	}

	private async void AcceptButton_Click(object sender, RoutedEventArgs args)
	{
		var friendRequestGuid = (Guid)((Button)sender).Tag;

		await App.Client.Send(new AcceptFriendRequestPacket().Pack(App.Storage.Guid, friendRequestGuid));
	}

	private void RejectButton_Click(object sender, RoutedEventArgs args)
	{
		var friendRequestItem = (FriendRequest)((Button)sender).DataContext;

		if (friendRequestItem != null)
		{
			FriendRequestHelper.FriendRequests.Remove(friendRequestItem);
		}
	}

	private void FriendGuid_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
	{
		SendFriendRequestButton.IsEnabled = ValidationHelper.ValidateGuid(FriendGuid.Text);
	}
}
