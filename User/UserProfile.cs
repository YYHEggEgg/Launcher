using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace Launcher.User
{
    internal static class UserProfile
    {
        public static UserCollection collection;
        public static Guid NOT_FOUND_GUID => Guid.Empty;

        /// <summary>
        /// Device-GUID Header for being added to the request. If not set, default value is <see cref="null"/>.
        /// </summary>
        public static string Device_Guid_Header { get; private set; }

        static UserProfile()
        {
            collection = File.Exists("user_profile.json")
                ? UserCollection.GetFromFile("user_profile.json")
                : new UserCollection("user_profile.json");
            Device_Guid_Header = null;
        }

        /// <summary>
        /// Query the device-Guid for a user. If not exists, it will be automatically created.
        /// </summary>
        public static Guid QueryDeviceGuid(string username)
        {
            return collection.SearchUserByName(username).Device_Guid;
        }

        /// <summary>
        /// Query the device-Guid for a user. If not exists, method will return <see cref="NOT_FOUND_GUID"/>.
        /// </summary>
        public static Guid QueryDeviceGuidByToken(string token)
        {
            var user = collection.SearchUserByToken(token);
            return user != null ? user.Device_Guid : NOT_FOUND_GUID;
        }

        /// <summary>
        /// For better performance, you may invoke this method before invoking <see cref="UrlNotify(string, bool, string)"/>. It's also OK if you don't care the performance and invoke <see cref="UrlNotify(string, bool, string)"/>.
        /// </summary>
        /// <param name="endpoint">This param should only have endpoint without protocol and host, e.g. <c>/account/risky/api/check?</c>.</param>
        /// <param name="isRequest">If the endpoint is request.</param>
        /// <returns>If the return value isn't <see cref="UrlEndpointType.Other"/>, you may invoke the <see cref="UrlNotify(UrlEndpointType, string)"/> method with this enum value.</returns>
        public static UrlEndpointType IsUrlNeedNotify(string endpoint, bool isRequest)
        {
            if (endpoint.StartsWith("/hk4e_cn/mdk/shield/api/login") ||
                endpoint.StartsWith("/hk4e_global/mdk/shield/api/login"))
            {
                return isRequest ? UrlEndpointType.LoginRequest : UrlEndpointType.LoginResponse;
            }
            else if (endpoint.StartsWith("/hk4e_cn/mdk/shield/api/verify") ||
                endpoint.StartsWith("/hk4e_global/mdk/shield/api/verify"))
            {
                return isRequest ? UrlEndpointType.TokenVerifyRequest : UrlEndpointType.TokenVerifyResponse;
            }
            else if (endpoint.StartsWith("/query_cur_region") && isRequest)
            {
                return UrlEndpointType.QueryCurrRegionHttpRequest;
            }
            else
            {
                return UrlEndpointType.Other;
            }
        }

        /// <summary>
        /// Invoke when a request arrived.
        /// </summary>
        /// <param name="endpointType">Get it from <see cref="IsUrlNeedNotify(string, bool)"/>.</param>
        /// <param name="body">The request/response body.</param>
        public static void UrlNotify(UrlEndpointType endpointType, string body)
        {
            if (endpointType == UrlEndpointType.Other) return;
            switch (endpointType)
            {
                case UrlEndpointType.LoginRequest:
                    try
                    {
                        JObject jo = JObject.Parse(body);
                        string username = (string)jo["account"];
                        Device_Guid_Header = collection.SearchUserByName(username).Device_Guid.ToString();
                    }
                    catch (Exception ex)
                    {
                        Debug.Fail($"UrlNotify({endpointType}) fail: {ex}");
                    }
                    break;
                case UrlEndpointType.LoginResponse:
                    try
                    {
                        JObject jo = JObject.Parse(body);
                        string token = (string)jo["token"];
                        collection.SetUserToken()
                    }
                    catch (Exception ex)
                    {
                        Debug.Fail($"UrlNotify({endpointType}) fail: {ex}");
                    }
                    break;
            }
        }

        /// <summary>
        /// Invoke when a request arrived.
        /// </summary>
        /// <param name="endpoint">This param should only have endpoint without protocol and host, e.g. <c>/account/risky/api/check?</c>.</param>
        /// <param name="isRequest">If the endpoint is request.</param>
        /// <param name="body">The request/response body.</param>
        public static void UrlNotify(string endpoint, bool isRequest, string body)
        {
            UrlNotify(IsUrlNeedNotify(endpoint, isRequest), body);
        }

        /// <summary>
        /// Endpoint type split for <see cref="UrlNotify(UrlEndpointType, string)"/>.
        /// </summary>
        public enum UrlEndpointType
        {
            /// <summary>
            /// Normal url and don't need notify.
            /// </summary>
            Other = 0,
            /// <summary>
            /// Request like <c>/hk4e_*/mdk/shield/api/login</c>
            /// </summary>
            LoginRequest = 1,
            /// <summary>
            /// Request like <c>/hk4e_*/mdk/shield/api/verify</c>
            /// </summary>
            TokenVerifyRequest = 2,
            /// <summary>
            /// <c>/query_cur_region?...</c>
            /// </summary>
            QueryCurrRegionHttpRequest = 3,

            /// <summary>
            /// Response like <c>/hk4e_*/mdk/shield/api/login</c>
            /// </summary>
            LoginResponse = 4,
            /// <summary>
            /// Response like <c>/hk4e_*/mdk/shield/api/verify</c>
            /// </summary>
            TokenVerifyResponse = 5,
        }
    }
}