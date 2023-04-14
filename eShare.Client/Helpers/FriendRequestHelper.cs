// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using System;
using System.Linq;
using System.Collections.ObjectModel;

namespace eShare.Client.Helpers;

class FriendRequestHelper
{
    public static ObservableCollection<FriendRequest> FriendRequests = new();
}

public class FriendRequest
{
    public Guid Guid { get; set; }
    public string Name { get; set; }
    public string ProfilePictureLocation { get; set; }

    public FriendRequest(Guid guid, string name, int profilePicture)
    {
        Guid = guid;
        Name = name;

		var profilePictures = ProfilePictureHelper.PopulateProfilePictures();
		if (profilePictures != null)
		{
			var profilePictureLocation = profilePictures.FirstOrDefault(picture => picture.ID == profilePicture)?.PictureLocation;
			if (profilePictureLocation != null)
			{
				ProfilePictureLocation = profilePictureLocation;
			}
		}
	}
}
