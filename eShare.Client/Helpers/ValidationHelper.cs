// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace eShare.Client.Helpers;

internal class ValidationHelper
{
    public static bool ValidateGuid(string guid) => Guid.TryParse(guid, out var result) && result != App.Storage.Guid && !FriendsHelper.Friends.Any(friend => friend.Guid == result);

    public static bool ValidateName(string name) => Regex.IsMatch(name, @"^(?=.{2,24}$)(?![\d\s])\b(?=[a-zA-Z_-]*[a-zA-Z])[a-zA-Z][a-zA-Z_-]*\b(?:\s\b(?=[a-zA-Z_-]*[a-zA-Z])[a-zA-Z][a-zA-Z_-]*\b)*$") && name != App.Storage.Name;
}
