using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Launcher.User
{
    internal static class UserProfile
    {
        public static UserCollection collection;
        public static Guid NOT_FOUND_GUID => Guid.Empty;

        static UserProfile()
        {
            collection = File.Exists("user_profile.json")
                ? UserCollection.GetFromFile("user_profile.json")
                : new UserCollection("user_profile.json");
        }

        /// <summary>
        /// Query the device-Guid for a user. If not exists, it will be automatically created.
        /// </summary>
        public static Guid QueryDeviceGuid(string username)
        {
            return collection.SearchUserByName(username).Device_Guid;
        }

        /// <summary>
        /// Query the device-Guid for a user. If not exists, method will return NOT_FOUND_GUID.
        /// </summary>
        public static Guid QueryDeviceGuidByToken(string token)
        {
            var user = collection.SearchUserByToken(token);
            return user != null ? user.Device_Guid : NOT_FOUND_GUID;
        }
    }
}