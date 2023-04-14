// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace eShare.Client.Helpers;

public class ChatHelper
{
	public static int ChunkSize = 1024 * 1024;
	public static bool DownloadInProgress = false;
    public static ObservableCollection<Dictionary<Guid, ObservableCollection<object>>> Chat = new();

    public static void CreateChat(Guid guid)
    {
        if (Chat.Any(friendsChat => friendsChat.ContainsKey(guid)))
        {
            ClearChat(guid);
        }

        var chat = new ObservableCollection<object>();

        Chat.Add(new Dictionary<Guid, ObservableCollection<object>> { { guid, chat } });
    }

    public static void ClearChat(Guid guid)
    {
        var chat = Chat.FirstOrDefault(friendsChat => friendsChat.ContainsKey(guid));

        if (chat != null)
        {
            Chat.Remove(chat);
        }
    }

	public static void AddGIF(Guid guid, string gifSource, HorizontalAlignment horizontalAlignment)
	{
		Chat.FirstOrDefault(friendsChat => friendsChat.ContainsKey(guid)).TryGetValue(guid, out var chat);

		if (chat != null)
		{
			chat.Add(new GIF(gifSource, horizontalAlignment));
		}
	}

    public static void AddMessage(Guid guid, string message, HorizontalAlignment horizontalAlignment)
    {
        Chat.FirstOrDefault(friendsChat => friendsChat.ContainsKey(guid)).TryGetValue(guid, out var chat);

        if (chat != null)
        {
            chat.Add(new Message(message, horizontalAlignment));
        }
    }

	public static void AddAttachment(Guid guid, string identifier, string fileName, string fileSize)
	{
		Chat.FirstOrDefault(friendsChat => friendsChat.ContainsKey(guid)).TryGetValue(guid, out var chat);

		if (chat != null)
		{
			var attachment = new Attachment(string.Empty, fileName, 0, HorizontalAlignment.Left);

			attachment.AttachmentProgress = StatesHelper.AttachmentProgress.Waiting;
			attachment.Identifier = identifier;
			attachment.FileSize = fileSize;
			chat.Add(attachment);
		}
	}

	public static bool SendNotification(string title, string message)
	{
        if (App.Storage.Notifications)
        {
			var appNotification = new AppNotificationBuilder()
			.AddText(title)
			.AddText(message.Substring(0, Math.Min(message.Length, 128)))
			.BuildNotification();

			AppNotificationManager.Default.Show(appNotification);
			return appNotification.Id != 0;
		}
		
        return false;
	}

	public static string ConvertBytesToHumanReadableForm(ulong bytes)
	{
		int order = 0;
		double size = bytes;
		string[] sizes = { "Bytes", "KB", "MB", "GB", "TB" };

		while (size >= 1024 && order < sizes.Length - 1)
		{
			order++;
			size /= 1024;
		}

		return $"{size:0.##} {sizes[order]}";
	}

	public struct FriendChatData
    {
        public Guid Guid { get; set; }
        public ObservableCollection<object> Chat { get; set; }
    }
}

public class GIF
{
	public string Timestamp { get; set; }
	public Thickness Thickness { get; set; }
	public ImageSource GIFSource { get; set; }
	public HorizontalAlignment HorizontalAlignment { get; set; }

	public GIF(string gifSource, HorizontalAlignment horizontalAlignment)
	{
		var gifsPath = Path.Combine(StorageHelper.RootDirectory, "GIFs");
		var gifPath = Path.Combine(gifsPath, gifSource.Substring(gifSource.LastIndexOf('/') + 1));

		if (!Directory.Exists(gifsPath))
		{
			Directory.CreateDirectory(gifsPath);
		}

		try
		{
			DownloadTenorGif(gifSource, gifPath);

			GIFSource = new BitmapImage(new Uri(gifPath));
		}
		catch (Exception)
		{
			GIFSource = null;
		}

		HorizontalAlignment = horizontalAlignment;
		Timestamp = DateTime.Now.ToString("MM/dd/yyyy h:mm tt");

		if (horizontalAlignment == HorizontalAlignment.Left)
		{
			Thickness = new Thickness(0, 0, 128, 0);
		}
		else
		{
			Thickness = new Thickness(128, 0, 0, 0);
		}
	}

	private void DownloadTenorGif(string gifUrl, string saveFilePath)
	{
		using (var httpClient = new HttpClient())
		{
			var response = httpClient.GetAsync(gifUrl).Result;
			response.EnsureSuccessStatusCode();

			using (var stream = response.Content.ReadAsStreamAsync().Result)
			using (var fileStream = new FileStream(saveFilePath, FileMode.Create, FileAccess.Write))
			{
				stream.CopyToAsync(fileStream).Wait();
			}
		}
	}
}

public class Message
{
    public string Text { get; set; }
    public string Timestamp { get; set; }
    public Thickness Thickness { get; set; }
    public SolidColorBrush Background { get; set; }
    public HorizontalAlignment HorizontalAlignment { get; set; }

    public Message(string text, HorizontalAlignment horizontalAlignment)
    {
        Text = text;
        HorizontalAlignment = horizontalAlignment;
        Timestamp = DateTime.Now.ToString("MM/dd/yyyy h:mm tt");

		if (horizontalAlignment == HorizontalAlignment.Left)
		{
			Thickness = new Thickness(0, 0, 128, 0);
		}
		else
		{
			Thickness = new Thickness(128, 0, 0, 0);
		}
	}
}

public class Attachment : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	private double progress;
	private Visibility progressVisibility;

	public string Identifier { get; set; }
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public string FileSize { get; set; }
	public string Timestamp { get; set; }
	public Thickness Thickness { get; set; }
	public ulong ActualSize { get; private set; }
	public HorizontalAlignment HorizontalAlignment { get; set; }
	public StatesHelper.AttachmentProgress AttachmentProgress { get; set; }

	public double Progress
	{
		get { return progress; }
		set
		{
			if (progress != value)
			{
				progress = value;
				OnPropertyChanged(nameof(Progress));
				OnPropertyChanged(nameof(ProgressVisibility));
			}
		}
	}

	public Visibility ProgressVisibility
	{
		get { return progressVisibility; }
		set
		{
			if (progressVisibility != value)
			{
				progressVisibility = value;
				OnPropertyChanged(nameof(ProgressVisibility));
			}
		}
	}

	public Attachment(string filePath, string fileName, ulong fileSize, HorizontalAlignment horizontalAlignment)
    {
		Progress = 0;
        FilePath = filePath;
        FileName = fileName;
		ActualSize = fileSize;
		HorizontalAlignment = horizontalAlignment;
		ProgressVisibility = Visibility.Collapsed;
		Timestamp = DateTime.Now.ToString("MM/dd/yyyy h:mm tt");
        FileSize = ChatHelper.ConvertBytesToHumanReadableForm(fileSize);
		Identifier = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 6).Select(s => s[new Random().Next(s.Length)]).ToArray());

		if (horizontalAlignment == HorizontalAlignment.Left)
		{
			Thickness = new Thickness(0, 0, 128, 0);
		}
		else
		{
			Thickness = new Thickness(128, 0, 0, 0);
		}
	}

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		if (propertyName == nameof(Progress))
		{
			ProgressVisibility = Progress > 0 && Progress < 100 ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}
