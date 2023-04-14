// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using eShare.Networking;
using eShare.Client.Helpers;
using WinRT.Interop;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.ApplicationModel.DataTransfer;
using System;
using System.IO;
using System.Runtime.InteropServices;
using static eShare.Client.Helpers.ChatHelper;

namespace eShare.Client.Pages.Views;

public sealed partial class ChatViewPage : Page
{
	private object tappedChatItem { get; set; }
	private ChatHelper.FriendChatData friendChatData { get; set; }

	public ChatViewPage()
	{
		this.InitializeComponent();
	}

	private void InitializePage()
	{
		ChatBox.ItemsSource = friendChatData.Chat;
	}

	protected override void OnNavigatedTo(NavigationEventArgs args)
	{
		base.OnNavigatedTo(args);

		if (args.Parameter != null)
		{
			friendChatData = (ChatHelper.FriendChatData)args.Parameter;

			this.InitializePage();
		}
	}

	private async void SendButton_Click(object sender, RoutedEventArgs args)
	{
		if (MessageBox.Text.EndsWith(".gif"))
		{
			await App.Client.Send(new GIFPacket().Pack(friendChatData.Guid, MessageBox.Text, PacketIdentifiers.GIFSentRequest));

			friendChatData.Chat.Add(new GIF(MessageBox.Text, HorizontalAlignment.Right));
		}
		else
		{
			await App.Client.Send(new MessagePacket().Pack(friendChatData.Guid, MessageBox.Text, PacketIdentifiers.MessageSentRequest));

			friendChatData.Chat.Add(new Message(MessageBox.Text, HorizontalAlignment.Right));
		}
		
		MessageBox.Text = string.Empty;
	}

	private async void AttachButton_Click(object sender, RoutedEventArgs args)
	{
		var fileOpenPicker = new FileOpenPicker();

		fileOpenPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
		fileOpenPicker.FileTypeFilter.Add("*");

		if (Window.Current == null)
		{
			IntPtr hwnd = GetActiveWindow();
			InitializeWithWindow.Initialize(fileOpenPicker, hwnd);
		}

		var file = await fileOpenPicker.PickSingleFileAsync();

		if (file != null)
		{
			var attachment = new Attachment(file.Path, file.Name, (await file.GetBasicPropertiesAsync()).Size, HorizontalAlignment.Right);

			attachment.AttachmentProgress = StatesHelper.AttachmentProgress.Completed;

			await App.Client.Send(new AttachmentPacket().Pack(friendChatData.Guid, attachment.Identifier, attachment.FileSize, attachment.FileName, PacketIdentifiers.AttachmentSentRequest));

			friendChatData.Chat.Add(attachment);
		}
	}

	private void MessageBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
	{
		SendButton.IsEnabled = !string.IsNullOrEmpty(sender.Text.Trim());
	}

	private void ChatItem_RightTapped(object sender, RightTappedRoutedEventArgs args)
	{
		ListViewItem tappeditem = GetListViewItemFromElement(args.OriginalSource as DependencyObject);

		if (tappeditem?.Content is Message tappedMessage)
		{
			tappedChatItem = tappedMessage;

			AddFriend.Visibility = ValidationHelper.ValidateGuid(tappedMessage.Text) ? Visibility.Visible : Visibility.Collapsed;
			OpenLink.Visibility = IsLink(tappedMessage.Text) ? Visibility.Visible : Visibility.Collapsed;
			MessageFlyout.ShowAt(null, args.GetPosition(null));
		}
		else if (tappeditem?.Content is Attachment tappedAttachment)
		{
			tappedChatItem = tappedAttachment;

			OpenAttachmentFolder.Visibility = tappedAttachment.Progress == 100 ? Visibility.Visible : Visibility.Collapsed;
			OpenAttachment.Visibility = tappedAttachment.AttachmentProgress == StatesHelper.AttachmentProgress.Completed ? Visibility.Visible : Visibility.Collapsed;
			DownloadAttachment.Visibility = tappedAttachment.AttachmentProgress == StatesHelper.AttachmentProgress.Waiting ? Visibility.Visible : Visibility.Collapsed;

			DownloadAttachment.IsEnabled = ChatHelper.DownloadInProgress ? false : true;

			if (OpenAttachment.Visibility == Visibility.Visible || DownloadAttachment.Visibility == Visibility.Visible)
			{
				AttachmentFlyout.ShowAt(null, args.GetPosition(null));
			}
		}

		args.Handled = true;
	}

	private void CopyMessageButton_Click(object sender, RoutedEventArgs args)
	{
		var dataPackage = new DataPackage();

		dataPackage.SetText((tappedChatItem as Message).Text);
		Clipboard.SetContent(dataPackage);
	}

	private async void AddFriendButton_Click(object sender, RoutedEventArgs args)
	{
		await App.Client.Send(new OutgoingFriendRequestPacket().Pack(Guid.Parse((tappedChatItem as Message).Text)));
	}

	private async void OpenAttachmentButton_Click(object sender, RoutedEventArgs args)
	{
		try
		{
			await Launcher.LaunchUriAsync(new Uri((tappedChatItem as Attachment).FilePath));
		}
		catch (Exception) { }
	}

	private async void DownloadAttachmentButton_Click(object sender, RoutedEventArgs args)
	{
		ChatHelper.DownloadInProgress = true;
		(tappedChatItem as Attachment).AttachmentProgress = StatesHelper.AttachmentProgress.InProgress;

		await App.Client.Send(new AttachmentDownloadPacket().Pack(friendChatData.Guid, (tappedChatItem as Attachment).Identifier, PacketIdentifiers.AttachmentDownloadRequest));
	}

	private async void OpenLink_Click(object sender, RoutedEventArgs args)
	{
		Uri uri = new Uri((tappedChatItem as Message).Text);

		await Launcher.LaunchUriAsync(uri);
	}

	private async void OpenAttachmentFolderButton_Click(object sender, RoutedEventArgs args)
	{
		try
		{
			StorageFolder attachmentsFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName((tappedChatItem as Attachment).FilePath));
			await Launcher.LaunchFolderAsync(attachmentsFolder);
		}
		catch (Exception) { }
	}

	private ListViewItem GetListViewItemFromElement(DependencyObject element)
	{
		while (element != null && !(element is ListViewItem))
		{
			element = VisualTreeHelper.GetParent(element);
		}

		return element as ListViewItem;
	}

	static bool IsLink(string text)
	{
		Uri uriResult;
		return Uri.TryCreate(text, UriKind.Absolute, out uriResult)
			   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
	}

	[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto, PreserveSig = true, SetLastError = false)]
	public static extern IntPtr GetActiveWindow();
}

public class ChatTemplateSelector : DataTemplateSelector
{
	public DataTemplate GIFTemplate { get; set; }
	public DataTemplate MessageTemplate { get; set; }
	public DataTemplate AttachmentTemplate { get; set; }

	protected override DataTemplate SelectTemplateCore(object chat, DependencyObject container)
	{
		if (chat is GIF)
		{
			return GIFTemplate;
		}
		else if (chat is Message)
		{
			return MessageTemplate;
		}
		else if (chat is Attachment)
		{
			return AttachmentTemplate;
		}

		return null;
	}
}
