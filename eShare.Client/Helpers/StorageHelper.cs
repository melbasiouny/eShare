// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using Windows.Storage;
using System;

namespace eShare.Client.Helpers;

public class StorageHelper
{
    #region Account
    public Guid Guid { get; set; }
    public string Name { get; set; }
    public int ProfilePicture { get; set; }
    public bool IncomingRequests { get; set; }
    #endregion

    #region Settings
    public bool Sound { get; set; }
    public string AppTheme { get; set; }
    public bool Notifications { get; set; }
    #endregion

    public static string RootDirectory { get; private set; } = ApplicationData.Current.LocalFolder.Path;
    
    private ApplicationDataContainer container = ApplicationData.Current.LocalSettings;

    public StorageHelper()
    {
        Guid = GetValue<Guid>("Guid");
        Name = GetValue<string>("Name");
        ProfilePicture = GetValue<int>("ProfilePicture");
        IncomingRequests = GetValue<bool>("IncomingRequests", true);

        Sound = GetValue<bool>("Sound", true);
        AppTheme = GetValue<string>("AppTheme");
        Notifications = GetValue<bool>("Notifications", false);
    }

    public void SaveApplicationData()
    {
        container.Values["Guid"] = Guid;
        container.Values["Name"] = Name;
        container.Values["ProfilePicture"] = ProfilePicture;
        container.Values["IncomingRequests"] = IncomingRequests;

        container.Values["Sound"] = Sound;
        container.Values["AppTheme"] = AppTheme;
        container.Values["Notifications"] = Notifications;
    }

    public void ResetApplicationData()
    {
        Guid = Guid.Empty;
        Name = string.Empty;
        ProfilePicture = 0;
        IncomingRequests = true;

        Sound = true;
        AppTheme = string.Empty;
		Notifications = false;
	}

    private T GetValue<T>(string key, T defaultValue = default(T)) => container.Values[key] is T value ? value : default;
}
