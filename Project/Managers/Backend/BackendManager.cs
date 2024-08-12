using Project.Helpers.Network;
using Project.Managers.Async;
using Project.Systems.Logging;
using Project.Systems.Versioning;
using Newtonsoft.Json;
using RSG;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Version = Project.Systems.Versioning.Version;

namespace Project.Managers
{
    public sealed class BackendManager
    {
        private readonly UrlConfiguration _urlConfiguration;

        private readonly AsyncManager _asyncManager;
        private readonly GameVersion _gameVersion;

        private Version _minVersion;

        public BackendManager(UrlConfiguration urlConfiguration, AsyncManager asyncManager, GameVersion gameVersion)
        {
            _urlConfiguration = urlConfiguration;

            _asyncManager = asyncManager;
            _gameVersion = gameVersion;
        }

        /// <summary>
        /// Load min_version.json from CDN, compare with GAME version
        /// </summary>
        /// <returns>GAME version >= MIN version</returns>
        public IPromise<bool> CheckMinVersion()
        {
            var url = $"{_urlConfiguration.CdnUrl}/min_version.json";

            return Get<MinVersionResponse>(url)
                   .Then(ParseMinVersion)
                   .Then(kvp =>
                   {
                       _minVersion = kvp.Value;

                       var isAllowed = _gameVersion >= _minVersion;

                       LogSystem.Trace($"parsed version: {_minVersion} [{kvp.Key}], " +
                                       $"current version: {_gameVersion}, " +
                                       $"isAllowed: {isAllowed}");

                       return isAllowed;
                   });
        }

        private KeyValuePair<string, Version> ParseMinVersion(MinVersionResponse response)
        {
            var platform = "default";
            var minVersion = new Version(response.globalVersion);

            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.OSXEditor:
                {
                    var editorResponse = response.platforms?.FirstOrDefault(p => p.platform == "Editor");

                    if (editorResponse != null)
                    {
                        platform = editorResponse.platform;
                        minVersion = new Version(editorResponse.version);
                    }

                    break;
                }
                case RuntimePlatform.Android:
                {
                    var androidResponse = response.platforms?.FirstOrDefault(p => p.platform == "Android");

                    if (androidResponse != null)
                    {
                        platform = androidResponse.platform;
                        minVersion = new Version(androidResponse.version);
                    }

                    break;
                }
                case RuntimePlatform.IPhonePlayer:
                {
                    var iosResponse = response.platforms?.FirstOrDefault(p => p.platform == "iOS");

                    if (iosResponse != null)
                    {
                        platform = iosResponse.platform;
                        _minVersion = new Version(iosResponse.version);
                    }

                    break;
                }
            }

            return new KeyValuePair<string, Version>(platform, minVersion);
        }

        private IPromise<T> Get<T>(string url)
        {
            // TODO: check what if no file/broken
            return AsyncWebRequest
                   .Download(_asyncManager, url)
                   .Then(JsonConvert.DeserializeObject<T>);
        }

        public Version MinVersion => _minVersion;

        private sealed class MinVersionResponse
        {
            public string globalVersion = "";
            public MinVersionPlatformResponse[] platforms;
        }

        private sealed class MinVersionPlatformResponse
        {
            public string platform = "";
            public string version = "";
        }
    }
}