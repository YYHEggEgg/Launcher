using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.User
{
    internal class UserCollection
    {
        private Dictionary<string, User> profilemap; // key = username
        private Dictionary<string, User> tokenmap; // key = token
        private List<User> users;

        public DateTime Last_save_time { get; private set; }
        private object fileLock = null;

        private string autosavePath;

        public UserCollection(string autosavePath = null)
        {
            users = new List<User>();
            profilemap = new Dictionary<string, User>();
            tokenmap = new Dictionary<string, User>();

            this.autosavePath = autosavePath;
        }

        public UserCollection(List<User> input_users, string autosavePath = null)
        {
            users = input_users;
            foreach (var user in users)
            {
                profilemap.Add(user.Name, user);
                if (user.Token != null)
                {
                    tokenmap.Add(user.Token, user);
                }
            }

            this.autosavePath = autosavePath;
        }

        /// <summary>
        /// Search User By <paramref name="username"/>. If not exists, a new instance will be automatically created.
        /// </summary>
        /// <returns>The <see cref="User"/> instance.</returns>
        public User SearchUserByName(string username)
        {
            if (username == null)
                throw new ArgumentNullException("username");
            string serialize_json = null;
            bool shouldsave = false;
            lock (profilemap)
            {
                if (!profilemap.ContainsKey(username))
                {
                    var newuser = new User { Name = username, Device_Guid = Guid.NewGuid() };
                    profilemap.Add(username, newuser);
                    lock (users) users.Add(newuser);
                    shouldsave = (DateTime.Now - Last_save_time) > TimeSpan.FromSeconds(5);
                    // If not attacked, usually shouldn't have account create in 5s.
                    // Debug.Assert won't affect Release.
                    Debug.Assert(shouldsave);
                    Save();
                }
                return profilemap[username];
            }
        }

        /// <summary>
        /// Search User By <paramref name="deviceToken"/>.
        /// </summary>
        /// <returns>The <see cref="User"/> instance, or <see cref="null"/> if not exists.</returns>
        public User SearchUserByToken(string deviceToken)
        {
            if (deviceToken == null)
                throw new ArgumentNullException("deviceToken");
            lock (tokenmap)
            {
                return tokenmap.ContainsKey(deviceToken) ? tokenmap[deviceToken] : null;
            }
        }

        public void SetUserToken(string username, string newtoken)
        {
            lock (profilemap)
            {
                string oldtoken = profilemap[username].Token;
                profilemap[username].Token = newtoken;
                lock (tokenmap)
                {
                    if (oldtoken != null)
                    {
                        tokenmap.Remove(oldtoken);
                    }
                    tokenmap.Add(newtoken, profilemap[username]);
                }
            }
            Save();
        }

        public void Save()
        {
            lock (users)
                lock (fileLock)
                    if (autosavePath != null)
                        File.WriteAllText(autosavePath, JsonConvert.SerializeObject(users));
        }

        #region Serialization
        /// <summary>
        /// Get a <see cref="UserCollection"/> instance with autosave to <paramref name="filePath"/>.
        /// </summary>
        public static UserCollection GetFromFile(string filePath)
        {
            var list_profile = JsonConvert.DeserializeObject<List<User>>(File.ReadAllText(filePath));
            return new UserCollection(list_profile, filePath);
        }
        #endregion
    }

    internal class User
    {
        public string Name { get; set; }
        public string Token { get; set; }
        public Guid Device_Guid { get; set; }
    }
}
