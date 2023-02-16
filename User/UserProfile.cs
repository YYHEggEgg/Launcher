namespace Launcher.User
{
    internal static class UserProfile
    {
        public Dictionary<string, User> profilemap;

        static UserProfile()
        {
            List<KeyValuePair<string, User>> list_profilemap = 
                JsonSerializer.Deserialize<List<KeyValuePair<string, User>>>(
                File.ReadAllText("user_profile.json"));
            profilemap = new(list_profilemap);
        }

        public static void QueryDeviceGuid(string username)
        {
            throw new NotImplementedException();
        }

        public static void SaveUserProfile()
        {
            throw new NotImplementedException();
        }
    }

    internal struct User
    {
        public string Name { get; set; }
        public Guid Device-Guid { get; set; }
    }
}