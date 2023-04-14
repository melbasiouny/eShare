// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Reflection;

namespace eShare.Client.Pages;

public sealed partial class SettingsPage : Page
{
	public string Version
	{
		get
		{
			var version = Assembly.GetEntryAssembly().GetName().Version;
			return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
		}
	}

	public SettingsPage()
	{
		this.InitializeComponent();
		this.InitializePage();
	}

	private void InitializePage()
	{
		Sound.IsOn = App.Storage.Sound;
		Notifications.IsOn = App.Storage.Notifications;
		AppTheme.SelectedIndex = App.Storage.AppTheme switch
		{
			"Light" => 0,
			"Dark" => 1,
			_ => 2
		};
	}

	public static void SetSound()
	{
		if (App.Storage.Sound)
		{
			ElementSoundPlayer.State = ElementSoundPlayerState.On;
			ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.On;
		}
		else
		{
			ElementSoundPlayer.State = ElementSoundPlayerState.Off;
			ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.Off;
		}
	}

	public static void SetAppTheme()
	{
		(App.Window.Content as FrameworkElement).RequestedTheme = App.Storage.AppTheme switch
		{
			"Light" => ElementTheme.Light,
			"Dark" => ElementTheme.Dark,
			_ => ElementTheme.Default
		};
	}

	private void Sound_Toggled(object sender, RoutedEventArgs args)
	{
		App.Storage.Sound = Sound.IsOn;

		SetSound();
	}

	private void Notifications_Toggled(object sender, RoutedEventArgs args)
	{
		App.Storage.Notifications = Notifications.IsOn;
	}

	private void AppTheme_SelectionChanged(object sender, SelectionChangedEventArgs args)
	{
		App.Storage.AppTheme = ((ComboBoxItem)AppTheme.SelectedItem)?.Tag?.ToString();

		SetAppTheme();
	}
}
