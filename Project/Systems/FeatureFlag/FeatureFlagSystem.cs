using Project.Extensions;
using Project.Systems.Configs;
using Project.Systems.FeatureFlag.Signals;
using Project.Systems.Save;
using Project.Systems.Versioning;
using Newtonsoft.Json;
using RSG;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace Project.Systems.FeatureFlag
{
    public class FeatureFlagSystem : IInitializable, ISaveDataProvider
    {
        private readonly string _configName = "feature_flag.json";

        private ConfigSystem _configSystem;
        private SignalBus _signalBus;
        private Type _flagType;
        private GameVersion _gameVersion;

        private FeatureFlagConfig _flagConfig = new FeatureFlagConfig();
        private FeatureFlagData _featureFlagData = new FeatureFlagData();
        private Promise _loadPromise;

        public FeatureFlagSystem(SignalBus signalBus, ConfigSystem configSystem, Type flagType, GameVersion gameVersion)
        {
            _configSystem = configSystem;
            _signalBus = signalBus;
            _flagType = flagType;
            _gameVersion = gameVersion;
        }

        public void Initialize()
        {
            _signalBus.Fire(new RegisterSaveDataProviderSignal { SaveDataProvider = this });

            _loadPromise = new Promise();
            _loadPromise
                .Then(() => _configSystem.LoadFromSaveForNameAsync(_configName))
                .Then(response =>
                {
                    if (response.Success)
                    {
                        ParseConfig(response.Data);
                        _signalBus.Fire(new FeatureFlagUpdatedSignal());
                    }
                })
                .Then(() => _configSystem.LoadFromServerForNameAsync(_configName))
                .Then(response =>
                {
                    if (response.Success)
                    {
                        ParseConfig(response.Data);
                        _signalBus.Fire(new FeatureFlagUpdatedSignal());
                        var vers = new Versioning.Version(_gameVersion);
                        _featureFlagData.LastSyncVersion = vers;
                    }
                });
        }

        public bool HasFlag(Enum flag)
        {
            var strFlag = flag.ToFeatureFlagKey();
            var foundFlag = _flagConfig.Flags.FirstOrDefault(x => x.Key.Equals(strFlag));
            return (foundFlag != null && foundFlag.Version.Include(_gameVersion) && foundFlag.Enable) || foundFlag == null;
        }

        public string SaveDataKey => "feature_flag";

        public IDictionary<string, object> SaveData => _featureFlagData.ToDict;

        public void ReadSaveData(IDictionary<string, object> saveData)
        {
            _featureFlagData.FromDict(saveData);

            _loadPromise.Resolve();
        }

        private void ParseConfig(string json)
        {
            var config = JsonConvert.DeserializeObject<FeatureFlagConfig>(json, new FeatureFlagConverter(_flagType));

            _flagConfig = config;
        }

        [Serializable]
        public class FeatureFlagData : ISaveData
        {
            private static string _versionKey = "last_sync_version";

            public Versioning.Version LastSyncVersion = new Versioning.Version("0.0.0.1");

            public IDictionary<string, object> ToDict
            {
                get
                {
                    return new Dictionary<string, object>
                    {
                        { _versionKey, LastSyncVersion.ToString()}
                    };
                }
            }

            public void FromDict(IDictionary<string, object> saveData)
            {
                var lastSync = saveData.GetValue<string>(_versionKey);
                if (!lastSync.IsNullOrEmpty())
                {
                    LastSyncVersion = new Versioning.Version(lastSync);
                }
            }
        }
    }

    public class FeatureFlagConfig
    {
        public FeatureFlag[] Flags = new FeatureFlag[0];
    }

    public class FeatureFlag
    {
        public string Key;
        public SpanVersion Version;
        public bool Enable;

        public override string ToString()
        {
            return $"{Key} {Version}";
        }
    }
}
