// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using eShare.Client.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace eShare.Client.Pages.Setup;

public sealed partial class AccountCreationPage : Page
{
	public AccountCreationPage()
	{
		this.InitializeComponent();
	}

	private void ContinueButton_Click(object sender, RoutedEventArgs args)
	{
		SessionPreparationPage.TemporaryStorage.Name = AccountName.Text;

		NavigateTo(typeof(AccountSharingPage));
	}

	private void AccountName_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
	{
		ContinueButton.IsEnabled = ValidationHelper.ValidateName(sender.Text);
	}

	private void NavigateTo(Type page)
	{
		this.DispatcherQueue.TryEnqueue(() => Frame.Navigate(page));
	}
}
