// Authors: Ali Noureddine
//          Abdelrahman Hashad

using System.Text.Json;

namespace eShare.Server;

internal class Database
{
	private readonly string _filename;

	private List<User>? _users;

	public Database(string filename)
	{
		_users = new List<User>();
		_filename = filename;
	}

	public void Save()
	{
		var json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
		File.WriteAllText(_filename, json);
	}

	public void CreateUser(Guid guid, string name, bool online, int imageId)
	{
		var user = new User(guid, online, name, imageId);
		_users!.Add(user);
	}

	public bool UserExists(Guid guid)
	{
		var user = _users!.Find(u => u.Guid == guid);
		return user != null ? true : false;
	}

	public void DeleteUser(Guid guid)
	{
		if (_users != null)
		{
			foreach (var user in _users)
			{
				user.Friends.Remove(guid);
			}

			_users.RemoveAll(u => u.Guid == guid);
		}
	}

	public void AddFriend(Guid userId, Guid friendId)
	{
		if (userId == friendId) return;

		var user = _users!.Find(u => u.Guid == userId);

		var friend = _users.Find(u => u.Guid == friendId);

		if (user != null)
		{
			var tmp = user.Friends.Contains(friendId);

			if (tmp) return;
		}

		if (user == null || friend == null)
		{
			return;
		}

		if (user.Guid != Guid.Empty && friend.Guid != Guid.Empty) user.Friends.Add(friendId);
	}


	public void Load()
	{
		if (File.Exists(_filename))
		{
			var jsonStrng = File.ReadAllText(_filename);
			_users = JsonSerializer.Deserialize<List<User>>(jsonStrng);
		}
	}

	public void UpdateUserName(Guid userId, string name)
	{
		var user = _users!.Find(u => u.Guid == userId);
		if (user != null) user.Name = name;
	}

	public void RemoveFriend(Guid userId, Guid friendId)
	{
		var user = _users!.Find(u => u.Guid == userId);
		if (user != null) user.Friends.Remove(friendId);
	}

	public void UpdateStatus(Guid userId, bool online)
	{
		var user = _users!.Find(x => x.Guid == userId);
		if (user != null) user.IsOnline = online;
	}

	public string GetUserName(Guid userId)
	{
		var user = _users!.Find(x => x.Guid == userId);

		if (user != null) return user.Name;

		return string.Empty;
	}

	public bool GetUserStatus(Guid userId)
	{
		var user = _users!.Find(x => x.Guid == userId);

		if (user != null) return user.IsOnline ? true : false;

		return false;
	}

	public int GetImageId(Guid userId)
	{
		var user = _users!.Find(u => u.Guid == userId);
		if (user != null) return user.ImageId;
		return -1;
	}

	public void SetImageId(Guid userId, int imageId)
	{
		var user = _users!.Find(u => u.Guid == userId);
		if (user != null) user.ImageId = imageId;
	}

	public List<Guid> ViewFriends(Guid userId)
	{
		var user = _users!.Find(u => u.Guid == userId);
		return user != null ? user.Friends : new List<Guid>();
	}
}

public class User
{
	public User()
	{
		Name = string.Empty;
		Friends = new List<Guid>();
	}

	public User(Guid id, bool isOnline, string name, int imageId)
	{
		Friends = new List<Guid>();

		Guid = id;
		IsOnline = isOnline;
		Name = name;
		ImageId = imageId;
	}

	public Guid Guid { get; set; }
	public bool IsOnline { get; set; }
	public string Name { get; set; }
	public int ImageId { get; set; }
	public List<Guid> Friends { get; set; }
}