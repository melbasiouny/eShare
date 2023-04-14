// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using eShare.Networking.Utilities;

namespace eShare.Server;

internal static class AddressHelper
{
	public static async Task Update()
	{
		try
		{
			var client = new HttpClient();

			var responseMessage = await client.GetAsync("https://ifconfig.me/ip");
			responseMessage.EnsureSuccessStatusCode();
			var address = await responseMessage.Content.ReadAsStringAsync();

			var requestMessage =
				new HttpRequestMessage(HttpMethod.Patch, $"https://api.github.com/gists/{Globals.GistID}");
			requestMessage.Headers.Add("Accept", "application/vnd.github+json");
			requestMessage.Headers.Add("Authorization", $"Bearer {Globals.Token}");
			requestMessage.Headers.Add("User-Agent", "Request");
			requestMessage.Content = new StringContent($"{{\"files\":{{\"Address\":{{\"content\": \"{address}\"}}}}}}");
			responseMessage = await client.SendAsync(requestMessage);
			responseMessage.EnsureSuccessStatusCode();

			Logger.Log(LogLevel.Information, $"Server IP address updated successfully.");
		}
		catch
		{
			Logger.Log(LogLevel.Error, $"Unable to update server IP address.");
		}
	}
}
