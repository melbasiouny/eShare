// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace eShare.Client.Helpers;

internal class ProfilePictureHelper
{
    public static List<ProfilePicture> PopulateProfilePictures()
    {
		var profilePictures = new List<ProfilePicture>();

        try
        {
            profilePictures = Directory.GetFiles(Path.Combine(StorageHelper.RootDirectory, "Content/Profile Pictures/"))
                               .Select(picture => new ProfilePicture { ID = int.Parse(Regex.Match(picture, @"\((?<ID>\d+)\)").Groups["ID"].Value), PictureLocation = picture })
                               .ToList();
        }
        catch (Exception) { }

        return profilePictures;
    }
}

public class ProfilePicture
{
    public int ID { get; set; }
    public string PictureLocation { get; set; }
}
