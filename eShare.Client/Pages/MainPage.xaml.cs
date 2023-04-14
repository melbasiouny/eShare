// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace eShare.Client.Pages;

public sealed partial class MainPage : Page
{
	public MainPage()
	{
		this.InitializeComponent();
	}

	private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
	{
		var navigationViewItem = args.SelectedItemContainer as NavigationViewItem;

		if (args.IsSettingsSelected) NavigateTo(typeof(SettingsPage), navigationViewItem);
		else if (navigationViewItem.Tag.ToString() == "ChatPage") NavigateTo(typeof(ChatPage), navigationViewItem);
		else if (navigationViewItem.Tag.ToString() == "AccountPage") NavigateTo(typeof(AccountPage), navigationViewItem);
	}

	private void NavigationView_Loaded(object sender, RoutedEventArgs args)
	{
		var navigationViewItem = NavigationView.MenuItems[0] as NavigationViewItem;

		NavigateTo(typeof(ChatPage), navigationViewItem);
	}

	private void NavigateTo(Type page, NavigationViewItem navigationViewItem)
	{
		ContentFrame.Navigate(page);
		NavigationView.SelectedItem = navigationViewItem;
	}
}
