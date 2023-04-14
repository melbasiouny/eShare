// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace eShare.Client.Helpers;

internal static class AddressHelper
{
	public static async Task Update()
	{
		var client = new HttpClient();

		var requestMessage =
			new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/gists/{Globals.GistID}");

		requestMessage.Headers.Add("Accept", "application/vnd.github+json");
		requestMessage.Headers.Add("Authorization", $"Bearer {Globals.Token}");
		requestMessage.Headers.Add("User-Agent", "Request");

		var responseMessage = await client.SendAsync(requestMessage);
		responseMessage.EnsureSuccessStatusCode();
		var address =
			Regex.Match(await responseMessage.Content.ReadAsStringAsync(), "\"content\":\"(.+?)\"");

		Globals.IPAddress = IPAddress.Parse(address.Groups[1].ToString());
	}
}
