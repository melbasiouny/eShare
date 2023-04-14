// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using eShare.Networking;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

namespace eShare.Client.Pages.Setup;

public sealed partial class AccountSharingPage : Page
{
	public AccountSharingPage()
	{
		this.InitializeComponent();
		this.InitializePage();
	}

	private void InitializePage()
	{
		Header.Text = $"Hello, {SessionPreparationPage.TemporaryStorage.Name}!";
		AccountGuid.Text = $"{SessionPreparationPage.TemporaryStorage.Guid} ";
	}

	private async void GetStartedButton_Click(object sender, RoutedEventArgs args)
	{
		await App.Client.Send(new CreateSessionPacket().Pack(SessionPreparationPage.TemporaryStorage.Guid, SessionPreparationPage.TemporaryStorage.Name));
	}

	private void GuidCopyButton_Click(object sender, RoutedEventArgs args)
	{
		var dataPackage = new DataPackage();

		dataPackage.SetText(SessionPreparationPage.TemporaryStorage.Guid.ToString());
		Clipboard.SetContent(dataPackage);
	}
}
