// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace eShare.Client.Helpers;

internal class FriendsHelper
{
    public static ObservableCollection<Friend> Friends = new();

	public static void IncrementNotifications(Friend friend)
	{
		if (string.IsNullOrEmpty(friend.Notifications))
		{
			friend.Notifications = "1";
		}
		else
		{
			int count;

			if (int.TryParse(friend.Notifications, out count))
			{
				friend.Notifications = (count + 1).ToString();
			}
		}
	}
}

public class Friend : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;
    public static event EventHandler<Guid> FriendStatusUpdated;

	private string name;
    private bool isOnline;
    private string notifications;
    private SolidColorBrush status;
	private ImageSource profilePicture;
    private Visibility notificationsVisible;

	public Guid Guid { get; set; }

	public string Name
	{
		get { return name; }
		set
		{
			if (name != value)
			{
				name = value;
				OnPropertyChanged(nameof(Name));
			}
		}
	}

	public bool IsOnline
	{
		get { return isOnline; }
		set
		{
			if (isOnline != value)
			{
				isOnline = value;
				OnPropertyChanged(nameof(Status));
				OnPropertyChanged(nameof(IsOnline));
			}
		}
	}

	public string Notifications
	{
		get { return notifications; }
		set
		{
			if (notifications != value)
			{
				notifications = value;
				OnPropertyChanged(nameof(Notifications));
				OnPropertyChanged(nameof(NotificationsVisible));
			}
		}
	}

	public SolidColorBrush Status
	{
		get { return status; }
		set
		{
			if (status != value)
			{
				status = value;
				OnPropertyChanged(nameof(Status));
			}
		}
	}

	public ImageSource ProfilePicture
	{
		get { return profilePicture; }
		set
		{
			if (profilePicture != value)
			{
				profilePicture = value;
				OnPropertyChanged(nameof(ProfilePicture));
			}
		}
	}

	public Visibility NotificationsVisible
    {
		get { return notificationsVisible; }
		set
		{
			if (notificationsVisible != value)
			{
				notificationsVisible = value;
				OnPropertyChanged(nameof(NotificationsVisible));
			}
		}
	}

    public Friend(Guid guid, string name, bool isOnline, int profilePicture)
    {
        Guid = guid;
        Name = name;
        IsOnline = isOnline;
		Notifications = string.Empty;
		NotificationsVisible = Visibility.Collapsed;
		Status = IsOnline ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
		ProfilePicture = new BitmapImage(new Uri(ProfilePictureHelper.PopulateProfilePictures().FirstOrDefault(picture => picture.ID == profilePicture).PictureLocation));
    }

    public void UpdateStatus(bool status)
    {
        IsOnline = status;
        Status = IsOnline ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		if (propertyName == nameof(IsOnline))
		{
			FriendStatusUpdated?.Invoke(this, Guid);
		}
		else if (propertyName == nameof(Notifications))
		{
			NotificationsVisible = string.IsNullOrEmpty(Notifications) ? Visibility.Collapsed : Visibility.Visible;
		}
	}
}
