using Project.Extensions;
using Project.Managers.Levels.Interfaces;
using Project.Managers.Levels.Signals;
using Project.Systems.Cheats;
using Project.Systems.Logging;
using Project.Systems.Save;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace Project.Managers.Levels
{
    public abstract class LevelManager<TLevelAsset, TLevelEnum> : ILevelTypeManager<TLevelEnum>, ILevelAssetManager<TLevelAsset, TLevelEnum>,
        ILevelProgressManager, ILevelProgressManager<TLevelEnum>,
        IInitializable, IDisposable, ISaveDataProvider
            where TLevelAsset : class
            where TLevelEnum : Enum
    {
        private readonly SignalBus _signalBus;
        private readonly ILevelLoader<TLevelAsset, TLevelEnum> _levelLoader;
        private readonly CheatSystem _cheatSystem;
        private readonly LevelCompletedManager<TLevelEnum> _levelCompletedManager;

        protected LevelData _levelData;
        protected MaxLevelInfo[] _maxLevels;

        public LevelManager(MaxLevelInfo[] maxLevels, ILevelLoader<TLevelAsset, TLevelEnum> levelLoader, LevelCompletedSettings levelCompletedSettings,
            SignalBus signalBus, CheatSystem cheatSystem)
        {
            _maxLevels = maxLevels;
            _levelLoader = levelLoader;
            _signalBus = signalBus;
            _cheatSystem = cheatSystem;
            _levelCompletedManager = new LevelCompletedManager<TLevelEnum>(signalBus, levelCompletedSettings);

            var enumDict = new Dictionary<TLevelEnum, int>();
            EnumExtensions.ToArray<TLevelEnum>().ForEach(levelEnum =>
            {
                enumDict.TryAdd(levelEnum, 1);
            });
            _levelData = new LevelData()
            {
                LevelProgress = enumDict,
            };
        }

        public void Initialize()
        {
            _levelCompletedManager.Initialize();

            RegisterCheats();

            _signalBus.Fire(new RegisterSaveDataProviderSignal { SaveDataProvider = this });
        }

        public void Dispose()
        {
            UnregisterCheats();
        }

        public TLevelAsset GetLevelForType(int level, TLevelEnum type)
        {
            TLevelAsset levelAsset = null;
            try
            {
                levelAsset = _levelLoader.GetLevel(level, type);
            }
            catch (Exception ex)
            {
                LogSystem.Error(ex);
            }

            return levelAsset;
        }

        public TLevelAsset GetCurrentLevelForType(TLevelEnum type)
        {
            TLevelAsset levelAsset = null;
            try
            {
                levelAsset = _levelLoader.GetLevel(GetLevelNumberForType(type), type);
            }
            catch (Exception ex)
            {
                LogSystem.Error(ex);
            }

            return levelAsset;
        }

        public int GetLevelNumberForType(TLevelEnum type)
        {
            return _levelData.LevelProgress[type];
        }

        public void SetLevelNumberForType(TLevelEnum type, int number)
        {
            _levelData.LevelProgress[type] = number;

            _signalBus.AbstractFire(new SaveRequestSignal { SaveDataProvider = this });
        }

        public bool HasLevel(TLevelEnum type)
        {
            var info = _maxLevels.FirstOrDefault(x => x.Type.Equals(type));
            return info?.Max >= _levelData.LevelProgress[type];
        }

        public void CompleteLevel(Enum type, int number, float time, string rewardId)
        {
            try
            {
                var levelType = (TLevelEnum)type;
                CompleteLevel(levelType, number, time, rewardId);
            }
            catch (Exception ex)
            {
                LogSystem.Error(ex);
            }
        }

        public void CompleteLevel(TLevelEnum type, int number, float time, string rewardId)
        {
            _levelCompletedManager.CompleteLevel(type, number, time, rewardId);
            _levelData.LevelProgress[type] = number + 1;
        }

        #region ISaveDataProvider

        public string SaveDataKey => "level_info";

        public IDictionary<string, object> SaveData => _levelData.ToDict;

        public void ReadSaveData(IDictionary<string, object> saveData)
        {
            _levelData.FromDict(saveData);
        }

        #endregion

        #region Cheats

        private void RegisterCheats()
        {
            var changeLevel = new Cheat
            {
                Id = "level.change",
                TitleLocId = "ui.dev.cheat.level.change",

                Params = new CheatIteratorParams
                {
                    Action = isNext =>
                    {
                        var type = default(TLevelEnum);

                        if (isNext && HasLevel(type))
                        {
                            _levelData.LevelProgress[type]++;
                            _signalBus.AbstractFire(new SaveRequestSignal { SaveDataProvider = this });
                            _signalBus.Fire(new LevelChangedSignal { Level = _levelData.LevelProgress[type], LevelType = type });

                            return;
                        }

                        if (!isNext && GetLevelNumberForType(type) > 1)
                        {
                            _levelData.LevelProgress[type]--;
                            _signalBus.AbstractFire(new SaveRequestSignal { SaveDataProvider = this });
                            _signalBus.Fire(new LevelChangedSignal { Level = _levelData.LevelProgress[type], LevelType = type });

                            return;
                        }
                    },
                    SetNewValue = () =>
                    {
                        var type = default(TLevelEnum);
                        var currentLevel = _levelData.LevelProgress[type];

                        return $"{currentLevel}";
                    }
                }
            };

            _cheatSystem.RegisterCheat(changeLevel);
        }

        private void UnregisterCheats()
        {
            _cheatSystem?.UnregisterCheat("level.change");
        }

        #endregion

        [Serializable]
        public sealed class LevelData : ISaveData
        {
            private static string _keyLevelProgress = "level_progress";

            public Dictionary<TLevelEnum, int> LevelProgress;

            public void FromDict(IDictionary<string, object> saveData)
            {
                if (saveData.TryGetValue(_keyLevelProgress, out var levelProgress))
                {
                    var genericDict = levelProgress as IDictionary<string, object>;
                    LevelProgress.AddRange(genericDict.ConvertTo<TLevelEnum, int>(), true);
                }
            }

            public IDictionary<string, object> ToDict
            {
                get
                {
                    return new Dictionary<string, object>
                    {
                        { _keyLevelProgress, LevelProgress},
                    };
                }
            }
        }

        [Serializable]
        public sealed class MaxLevelInfo
        {
            public TLevelEnum Type { get; private set; }
            public int Max { get; private set; }

            public MaxLevelInfo(TLevelEnum type, int max)
            {
                Type = type;
                Max = max;
            }
        }
    }
}
