// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using eShare.Networking;
using eShare.Client.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.DataTransfer;

namespace eShare.Client.Pages;

public sealed partial class AccountPage : Page
{
	private ObservableCollection<ProfilePicture> profilePictures;

	public AccountPage()
	{
		this.InitializeComponent();
		this.InitializePage();
	}

	private void InitializePage()
	{
		AccountGuid.Text = $"{App.Storage.Guid} ";
		IncomingRequests.IsOn = App.Storage.IncomingRequests;
		ProfileName.Text = AccountName.Text = $"{App.Storage.Name} ";

		profilePictures = new(ProfilePictureHelper.PopulateProfilePictures());
		ProfilePictures.ItemsSource = profilePictures;

		if (profilePictures.Count > 0)
		{
			ProfilePictures.SelectedItem = profilePictures.FirstOrDefault(picture => picture.ID == App.Storage.ProfilePicture);
			Profile.ProfilePicture = new BitmapImage(new Uri((ProfilePictures.SelectedItem as ProfilePicture).PictureLocation));
		}
	}

	private async void ProfilePictures_ItemClick(object sender, ItemClickEventArgs args)
	{
		var clickedItem = args.ClickedItem as ProfilePicture;

		if (clickedItem.ID != App.Storage.ProfilePicture)
		{
			App.Storage.ProfilePicture = clickedItem.ID;
			Profile.ProfilePicture = new BitmapImage(new Uri(clickedItem.PictureLocation, UriKind.RelativeOrAbsolute));

			await App.Client.Send(new UpdateAccountPicturePacket().Pack(clickedItem.ID));
		}
	}

	private void IncomingRequests_Toggled(object sender, RoutedEventArgs args)
	{
		App.Storage.IncomingRequests = IncomingRequests.IsOn;
	}

	private async void DeleteButton_Click(object sender, RoutedEventArgs args)
	{
		await App.Client.Send(new Packet((ushort)PacketIdentifiers.DeleteAccountRequest, out var _));
	}

	private void AccountGuidButton_Click(object sender, RoutedEventArgs args)
	{
		var dataPackage = new DataPackage();

		dataPackage.SetText(App.Storage.Guid.ToString());
		Clipboard.SetContent(dataPackage);
	}

	private void RenameButton_Click(object sender, RoutedEventArgs args)
	{
		Accept.IsEnabled = false;
		AccountName.IsEnabled = true;
		AccountName.Text = App.Storage.Name;
		Rename.Visibility = Visibility.Collapsed;
		Accept.Visibility = Cancel.Visibility = Visibility.Visible;
	}

	private async void RenameConfirmButton_Click(object sender, RoutedEventArgs args)
	{
		AccountName.IsEnabled = false;
		Rename.Visibility = Visibility.Visible;
		Accept.Visibility = Cancel.Visibility = Visibility.Collapsed;

		switch ((sender as Button).Tag.ToString())
		{
			case "Accept":
				App.Storage.Name = ProfileName.Text = AccountName.Text;

				await App.Client.Send(new UpdateAccountNamePacket().Pack(App.Storage.Name));
				break;

			case "Cancel":
				AccountName.Text = $"{App.Storage.Name} ";
				break;
		}
	}

	private void AccountName_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
	{
		Accept.IsEnabled = ValidationHelper.ValidateName(sender.Text);
	}
}
