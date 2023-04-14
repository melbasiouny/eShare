// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using eShare.Networking;
using eShare.Client.Helpers;
using eShare.Client.Pages.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Windows.Foundation;
using Windows.ApplicationModel.DataTransfer;

namespace eShare.Client.Pages;

public sealed partial class ChatPage : Page
{
	public static Friend SelectedFriend { get; set; } = null;
	private Friend tappedFriend { get; set; }

	public ChatPage()
	{
		this.InitializeComponent();
		this.InitializePage();
	}

	private void InitializePage()
	{
		UpdateRequestsIndicator();

		FriendsList.ItemsSource = FriendsHelper.Friends;
		ContentFrame.Navigate(typeof(DefaultViewPage));

		Friend.FriendStatusUpdated += Friend_FriendStatusUpdated;
		FriendRequestHelper.FriendRequests.CollectionChanged += FriendRequests_CollectionChanged;
	}

	protected override void OnNavigatedFrom(NavigationEventArgs args)
	{
		SelectedFriend = null;
	}

	private void Friend_FriendStatusUpdated(object sender, Guid guid)
	{
		SelectedFriend = (FriendsList.SelectedItem as Friend);

		if (SelectedFriend != null && SelectedFriend.Guid == guid)
		{
			if (SelectedFriend.IsOnline)
			{
				var chat = new ObservableCollection<object>();
				var friendChatData = new ChatHelper.FriendChatData();

				ChatHelper.Chat.FirstOrDefault(friend => friend.TryGetValue(SelectedFriend.Guid, out chat));

				friendChatData.Chat = chat;
				friendChatData.Guid = SelectedFriend.Guid;

				ContentFrame.Navigate(typeof(ChatViewPage), friendChatData);
			}
			else
			{
				SelectedFriend.Notifications = string.Empty;

				ContentFrame.Navigate(typeof(FriendOfflineViewPage));
			}
		}
	}

	private void FriendRequests_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		this.DispatcherQueue.TryEnqueue(() => UpdateRequestsIndicator());
	}

	private void FriendRequestsButton_Click(object sender, RoutedEventArgs args)
	{
		FriendsList.SelectedIndex = -1;

		if (ContentFrame.SourcePageType != typeof(FriendRequestsViewPage))
		{
			ContentFrame.Navigate(typeof(FriendRequestsViewPage));
		}
	}

	private void FriendsList_SelectionChanged(object sender, SelectionChangedEventArgs args)
	{
		SelectedFriend = (FriendsList.SelectedItem as Friend);

		if (FriendsList.SelectedItem != null && SelectedFriend.IsOnline == true)
		{
			var chat = new ObservableCollection<object>();
			var friendChatData = new ChatHelper.FriendChatData();

			ChatHelper.Chat.FirstOrDefault(friend => friend.TryGetValue(SelectedFriend.Guid, out chat));

			friendChatData.Chat = chat;
			friendChatData.Guid = SelectedFriend.Guid;
			SelectedFriend.Notifications = string.Empty;

			ContentFrame.Navigate(typeof(ChatViewPage), friendChatData);
		}
		else if (FriendsList.SelectedItem == null)
		{
			ContentFrame.Navigate(typeof(DefaultViewPage));
		}
		else
		{
			ContentFrame.Navigate(typeof(FriendOfflineViewPage));
		}
	}

	private void UpdateRequestsIndicator()
	{
		RequestsIndicator.Visibility = FriendRequestHelper.FriendRequests.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
	}

	private void FriendsList_RightTapped(object sender, RightTappedRoutedEventArgs args)
	{
		ListViewItem tappeditem = GetListViewItemFromElement(args.OriginalSource as DependencyObject);

		if (tappeditem != null)
		{
			Point mousePosition = args.GetPosition(null);

			tappedFriend = tappeditem.Content as Friend;
			FriendFlyout.ShowAt(null, mousePosition);
		}

		args.Handled = true;
	}

	private ListViewItem GetListViewItemFromElement(DependencyObject element)
	{
		while (element != null && !(element is ListViewItem))
		{
			element = VisualTreeHelper.GetParent(element);
		}

		return element as ListViewItem;
	}

	private async void RemoveFriendFlyoutItem_Click(object sender, RoutedEventArgs args)
	{
		FriendsHelper.Friends.Remove(tappedFriend);

		if (FriendFlyout.IsOpen)
		{
			FriendFlyout.Hide();
		}

		ContentFrame.Navigate(typeof(DefaultViewPage));

		await App.Client.Send(new FriendRemovePacket().Pack(tappedFriend.Guid));
	}

	private void CopyGuidFlyoutItem_Click(object sender, RoutedEventArgs args)
	{
		var dataPackage = new DataPackage();

		dataPackage.SetText(tappedFriend.Guid.ToString());
		Clipboard.SetContent(dataPackage);
	}
}

public class GuidDisplayFormat : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is Guid guid)
		{
			return guid.ToString().Substring(0, 8);
		}

		return "";
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		return null;
	}
}
