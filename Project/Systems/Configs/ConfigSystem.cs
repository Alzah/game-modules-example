using Project.Extensions;
using Project.Helpers;
using Project.Helpers.Network;
using Project.Managers.Async;
using Project.Systems.Logging;
using Project.Systems.Save;
using Newtonsoft.Json;
using RSG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Project.Systems.Configs
{
    public class ConfigSystem : IInitializable, ISaveDataProvider
    {
        private ConfigsData _configsData = new ConfigsData();
        private string _configsDirectory;
        private ConfigConverter _converter = new ConfigConverter(true);

        private SignalBus _signalBus;
        private AsyncManager _asyncManager;
        private UrlConfiguration _urlConfiguration;

        public ConfigSystem(AsyncManager asyncManager, SignalBus signalBus, UrlConfiguration urlConfiguration, string configsDirectory)
        {
            _configsDirectory = $"{Application.persistentDataPath}/{configsDirectory}";
            _asyncManager = asyncManager;
            _signalBus = signalBus;
            _urlConfiguration = urlConfiguration;
        }

        public void Initialize()
        {
            _signalBus?.Fire(new RegisterSaveDataProviderSignal { SaveDataProvider = this });

            LogSystem.Trace($"ConfigsDirectory: {_configsDirectory}");
        }

        public IPromise<ConfigResponce> LoadFromServerForNameAsync(string filename)
        {
            var url = _urlConfiguration.GetConfigUrlForName(filename);
            return LoadConfigFromServerAsync(url, filename);
        }

        public IPromise<ConfigResponce> LoadFromServerForUrlAsync(string url)
        {
            var filename = PathExtensions.GetFilename(url);
            return LoadConfigFromServerAsync(url, filename);
        }

        public IPromise<ConfigResponce> LoadFromSaveForNameAsync(string filename)
        {
            return LoadConfigFromSaveAsync(filename);
        }

        public IPromise<ConfigResponce> LoadFromSaveForUrlAsync(string url)
        {
            var filename = PathExtensions.GetFilename(url);
            return LoadConfigFromSaveAsync(filename);
        }

        private IPromise<ConfigResponce> LoadConfigFromServerAsync(string url, string filename)
        {
            var filepath = Path.Combine(_configsDirectory, filename);

            var config = FindConfig(filename);
            bool hasOldConfig = config is not null;

            if (!hasOldConfig)
            {
                config = new ConfigInfo
                {
                    filename = filename,
                    etag = "",
                    url = url,
                    data = ""
                };
            }

            if (hasOldConfig)
            {
                var reguest = AsyncWebRequest.GetETagIfNoneMatch(_asyncManager, url, config.etag)
                    .Then(match =>
                    {
                        return match
                            ? config.IsLoad ? ReturnResolveResponce(config.data) : ReadAndCached(filepath, config)
                            : FullDownloadAndSave(url, filepath, config);
                    });
                return reguest;
            }
            else
            {
                return FullDownloadAndSave(url, filepath, config);
            }
        }


        private IPromise<ConfigResponce> LoadConfigFromSaveAsync(string filename)
        {
            var info = FindConfig(filename);

            if (info == null)
            {
                return ReturnResolveResponce();
            }
            var filepath = Path.Combine(_configsDirectory, filename);
            return ReadAndCached(filepath, info);
        }

        private IPromise<ConfigResponce> FullDownloadAndSave(string url, string filepath, ConfigInfo config)
        {
            return AsyncWebRequest.DownloadWithFullResponse(_asyncManager, url)
                .Then(responce =>
                {
                    config.data = responce.Data;
                    config.etag = responce.ETag;
                    var json = JsonConvert.SerializeObject(config, _converter);

                    return FileSystemHelper.WriteToStorageAsync(filepath, json);
                })
                .Then(json =>
                {
                    var index = _configsData.Configs.IndexOf(config);
                    if (index < 0)
                    {
                        _configsData.Configs.Add(config);
                    }
                    else
                    {
                        _configsData.Configs[index] = config;
                    }
                    return new ConfigResponce(config.data);
                });
        }

        private IPromise<ConfigResponce> ReadAndCached(string filepath, ConfigInfo config)
        {
            return FileSystemHelper.ReadFromStorageAsync(filepath)
                .Then(json =>
                {
                    var deserializeInfo = JsonConvert.DeserializeObject<ConfigInfo>(json, _converter);
                    config.data = deserializeInfo.data;
                    return new ConfigResponce(config.data);
                });
        }

        private IPromise<ConfigResponce> ReturnResolveResponce(string data = null)
        {
            var promise = new Promise<ConfigResponce>();
            promise.Resolve(data.IsNullOrEmpty() ? new ConfigResponce() : new ConfigResponce(data));
            return promise;
        }


        private ConfigInfo FindConfig(string filename)
        {
            return _configsData.Configs.FirstOrDefault(x => x.filename.Equals(filename));
        }

        #region ISaveDataProvider

        public string SaveDataKey => "configs";

        public IDictionary<string, object> SaveData => _configsData.ToDict;

        public void ReadSaveData(IDictionary<string, object> saveData)
        {
            _configsData.FromDict(saveData);
        }

        #endregion

        [Serializable]
        private sealed class ConfigsData : ISaveData
        {
            private static string _configsKey = "list_configs";

            public List<ConfigInfo> Configs = new();

            public void FromDict(IDictionary<string, object> saveData)
            {
                try
                {
                    if (saveData.TryGetValue(_configsKey, out var configs))
                    {
                        var list = new List<ConfigInfo>();
                        var configList = configs as List<object>;
                        foreach (var config in configList)
                        {
                            var generic = config as IDictionary<string, object>;
                            list.Add(ConfigInfo.Parse(generic));
                        }

                        Configs = list;
                    }
                }
                catch (Exception ex)
                {
                    LogSystem.Trace($"ConfigsData parse error! Configs empty");
                    LogSystem.Trace(ex);
                }
            }

            public IDictionary<string, object> ToDict
            {
                get
                {
                    return new Dictionary<string, object>
                    {
                        { _configsKey, Configs},
                    };
                }
            }
        }
    }

    public class ConfigResponce
    {
        public ConfigResponce()
        {
            Success = false;
            Data = null;
        }

        public ConfigResponce(string data)
        {
            Success = true;
            Data = data;
        }

        public bool Success;
        public string Data;
    }
}