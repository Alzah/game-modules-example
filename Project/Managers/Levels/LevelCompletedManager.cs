using Project.Extensions;
using Project.Systems.Save;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace Project.Managers.Levels
{
    internal class LevelCompletedManager<TLevelEnum> : IInitializable, ISaveDataProvider
        where TLevelEnum : Enum
    {
        private LevelCompletedSettings _levelCompletedSettings;
        private CompletedData _completedData;
        private SignalBus _signalBus;

        public LevelCompletedManager(SignalBus signalBus, LevelCompletedSettings levelCompletedSettings)
        {
            _signalBus = signalBus;
            _levelCompletedSettings = levelCompletedSettings;
        }

        public void Initialize()
        {
            var enumDict = new Dictionary<TLevelEnum, int>();
            EnumExtensions.ToArray<TLevelEnum>().ForEach(levelEnum =>
            {
                enumDict.TryAdd(levelEnum, 0);
            });
            _completedData = new CompletedData()
            {
                Levels = new(_levelCompletedSettings.KeepingLevelCount + 1),
                MaxLevel = enumDict
            };

            _signalBus.Fire(new RegisterSaveDataProviderSignal { SaveDataProvider = this });
        }

        public void CompleteLevel(TLevelEnum levelType, int number, float time, string rewardId)
        {
            var maxLevel = _completedData.MaxLevel[levelType];

            if (maxLevel <= number)
            {
                _completedData.MaxLevel.AddOrUpdate(levelType, number);
            }

            var levels = _completedData.Levels;
            levels.Add(new LevelCompleted(levelType, number, time, rewardId));

            if (levels.Count > _levelCompletedSettings.KeepingLevelCount)
            {
                var diff = levels.Count - _levelCompletedSettings.KeepingLevelCount;
                levels.RemoveRange(0, diff);
            }

            _signalBus.AbstractFire(new SaveRequestSignal { SaveDataProvider = this });
        }

        #region ISaveDataProvider

        public string SaveDataKey => "level_completed";

        public IDictionary<string, object> SaveData => _completedData.ToDict;

        public void ReadSaveData(IDictionary<string, object> saveData)
        {
            _completedData.FromDict(saveData);
        }

        #endregion

        protected sealed class CompletedData : ISaveData
        {
            public List<LevelCompleted> Levels = new();
            public Dictionary<TLevelEnum, int> MaxLevel = new();

            public IDictionary<string, object> ToDict
            {
                get
                {
                    return new Dictionary<string, object>
                    {
                        { "levels", Levels },
                        { "max_level", MaxLevel },
                    };
                }
            }

            public void FromDict(IDictionary<string, object> saveData)
            {
                if (saveData.TryGetValue("max_level", out var maxLevel))
                {
                    var genericDict = maxLevel as IDictionary<string, object>;
                    MaxLevel.AddRange(genericDict.ConvertTo<TLevelEnum, int>(), true);
                }

                if (saveData.ContainsKey("levels"))
                {
                    Levels = saveData.GetValue<List<object>>("levels", new List<object>()).Select(x => new LevelCompleted(x)).ToList();
                }
            }
        }

        [Serializable]
        public class LevelCompleted
        {
            public LevelCompleted(TLevelEnum type, int number, float time, string rewardId)
            {
                Type = type;
                Number = number;
                Time = time;
                RewardId = rewardId;
            }

            public TLevelEnum Type;
            public int Number;
            public float Time;
            public string RewardId;

            public LevelCompleted(object dict)
            {
                if (dict is Dictionary<string, object> lcData)
                {
                    Type = (TLevelEnum)Enum.Parse(typeof(TLevelEnum), lcData["Type"].ToString());
                    Number = (int)Convert.ChangeType(lcData["Number"], typeof(int));
                    Time = (float)Convert.ChangeType(lcData["Time"], typeof(float));
                    RewardId = lcData["RewardId"].ToString();
                }
                else
                {
                    throw new InvalidCastException("Invalid cast object to LevelCompleted");
                }
            }
        }
    }

    [Serializable]
    public class LevelCompletedSettings
    {
        public int KeepingLevelCount = 10;
    }
}
