// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using eShare.Client.Pages;
using eShare.Client.Pages.Setup;
using Microsoft.UI.Xaml;
using Windows.UI.ApplicationSettings;

namespace eShare.Client;

public sealed partial class MainWindow : Window
{
	public MainWindow()
	{
		this.InitializeComponent();

		this.DispatcherQueue.TryEnqueue(InitializeWindow);
	}

	private void InitializeWindow()
	{
		App.Window.SetTitleBar(AppTitleBar);

		SettingsPage.SetAppTheme();
		SettingsPage.SetSound();

		ContentFrame.Navigate(typeof(SessionPreparationPage));
	}
}
