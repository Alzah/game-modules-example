using Project.Extensions;
using Project.Managers.Game;
using Project.Managers.Game.GameStates;
using Project.Managers.Game.Signals;
using Project.Managers.NetworkTime;
using Project.Managers.Wealth.Lives.Signals;
using Project.Systems.Cheats;
using Project.Systems.Save;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Project.Managers.Lives
{
    public sealed class LivesManager : IInitializable, IDisposable, ISaveDataProvider
    {
        private readonly SignalBus _signalBus;
        private readonly NetworkTimeManager _networkTimeManager;

        private readonly CheatSystem _cheatSystem;

        private readonly Settings _settings;
        private readonly Data _data = new();

        private bool _isInitialized;
        private float _timeout;

        public LivesManager(Settings settings, SignalBus signalBus, NetworkTimeManager networkTimeManager, 
            [InjectOptional] CheatSystem cheatSystem)
        {
            _settings = settings;
            _signalBus = signalBus;
            _networkTimeManager = networkTimeManager;
            _cheatSystem = cheatSystem;
        }

        public void Initialize()
        {
            RegisterCheats();

            _signalBus?.Subscribe<GameStateChangedSignal>(OnGameStateChanged);
            _signalBus?.Subscribe<ApplicationPauseSignal>(OnApplicationPause);
            _signalBus?.Subscribe<NetworkTimeChangedSignal>(OnNetworkTimeChanged);
        }

        public void Dispose()
        {
            _isInitialized = false;

            UnregisterCheats();

            if (IsInfinity)
            {
                _data.VerificateInfinityTime = _networkTimeManager.TotalSeconds;
            }
            else
            {
                if (_data.IsLastGameplayState && IsFull)
                {
                    _data.LastRestoreTime = _networkTimeManager.TotalSeconds;
                }
            }

            _signalBus?.TryUnsubscribe<GameStateChangedSignal>(OnGameStateChanged);
            _signalBus?.TryUnsubscribe<ApplicationPauseSignal>(OnApplicationPause);
            _signalBus?.TryUnsubscribe<NetworkTimeChangedSignal>(OnNetworkTimeChanged);
        }

        public void Tick()
        {
            if (!_isInitialized) { return; }

            if (_data != null)
            {
                if (!IsFull)
                {
                    _timeout -= Time.unscaledDeltaTime;
                    if (_timeout < 0)
                    {
                        _data.LastRestoreTime = _networkTimeManager.TotalSeconds;
                        Increase(ELivesUsageType.Default, 1);
                        _timeout = _settings.RestTime;
                    }
                }

                if (IsInfinity)
                {
                    _data.LastInfinityTime -= Time.unscaledDeltaTime;
                    if (_data.LastInfinityTime < 0)
                    {
                        Increase(ELivesUsageType.Default, _data.MaxCount);
                    }
                }
            }
        }


        /// <summary>
        /// Add lives in manager
        /// </summary>
        /// <param name="livesUsageType">Type live: Default/Infinite</param>
        /// <param name="amount">
        /// Default - pieces
        /// Infinite - minutes
        /// </param>
        /// <returns>Success</returns>
        public bool Increase(ELivesUsageType livesUsageType, int amount)
        {
            bool hasIncrease = false;
            switch (livesUsageType)
            {
                case ELivesUsageType.Default:
                    if (_data.Count < _data.MaxCount)
                    {
                        _data.Count += amount;
                        if (_data.Count > _data.MaxCount)
                        {
                            _data.Count = _data.MaxCount;
                        }
                        _timeout = _settings.RestTime;
                        hasIncrease = true;
                    }
                    break;
                case ELivesUsageType.Infinite:
                    _data.LastInfinityTime = IsInfinity ? _data.LastInfinityTime + (amount * 60) : amount * 60;
                    _data.VerificateInfinityTime = _networkTimeManager.TotalSeconds;
                    hasIncrease = true;
                    break;
                default:
                    throw new NotImplementedException($"Unknown ELivesUsageType {livesUsageType} or something went wrong");
            }

            if (hasIncrease)
            {
                _signalBus.AbstractFire(new LivesChangedSignal(CurrentLives, MaximumLives, IsInfinity, ));
            }
            return hasIncrease;
        }

        /// <summary>
        /// Spend one live
        /// </summary>
        public void Spend()
        {
            SpendInner(true);
        }

        public int CurrentLives => _data.Count;

        public int MaximumLives => _data.MaxCount;

        public bool IsFull => CurrentLives == MaximumLives;

        public bool HasLive => CurrentLives > 0 || IsInfinity;

        public bool IsInfinity => _data.LastInfinityTime > 0;

        public float RestoreTime => _timeout;

        public double InfinityTime => _data.LastInfinityTime;

        public Sprite Icon => _settings.Icon;

        #region ISaveDataProvider

        public string SaveDataKey => "lives";

        public IDictionary<string, object> SaveData
        {
            get
            {
                if (IsInfinity)
                {
                    _data.VerificateInfinityTime = _networkTimeManager.TotalSeconds;
                }

                return _data.ToDict;
            }
        }

        public void ReadSaveData(IDictionary<string, object> saveData)
        {
            if (saveData == null) return;

            _data.FromDict(saveData);

            if (_data.IsLastGameplayState)
            {
                SpendInner();
                _data.IsLastGameplayState = false;
            }
        }

        #endregion

        private void SpendInner(bool needRestoreTime = false)
        {
            if (!IsInfinity)
            {
                if (IsFull && needRestoreTime)
                {
                    _data.LastRestoreTime = _networkTimeManager.TotalSeconds;
                }
                _data.Count--;
                if (_data.Count < 0)
                {
                    _data.Count = 0;
                }
                _signalBus.AbstractFire(new LivesChangedSignal(CurrentLives, MaximumLives, false, this));
            }
        }

        private void VerificateTime()
        {
            if (!IsInfinity)
            {
                if (_data.LastRestoreTime > _networkTimeManager.TotalSeconds)
                {
                    // If time in past
                    _data.LastRestoreTime = _networkTimeManager.TotalSeconds;
                }

                var spanInSeconds = _networkTimeManager.TotalSeconds - _data.LastRestoreTime;

                var restTime = _settings.RestTime;
                var restoreLive = (long)(spanInSeconds / restTime);
                var timeToNext = restTime - (float)(spanInSeconds % restTime);

                _data.LastRestoreTime += (long)(restoreLive * restTime);

                _timeout = timeToNext;

                if (restoreLive > 0)
                {
                    Increase(ELivesUsageType.Default, (int)restoreLive);
                }
            }
            else
            {
                if (_data.VerificateInfinityTime > _networkTimeManager.TotalSeconds)
                {
                    // If time in past
                    _data.VerificateInfinityTime = _networkTimeManager.TotalSeconds;
                }

                _data.LastInfinityTime -= _networkTimeManager.TotalSeconds - _data.VerificateInfinityTime;
                _data.VerificateInfinityTime = _networkTimeManager.TotalSeconds;
            }

            _signalBus.AbstractFire(new LivesChangedSignal(CurrentLives, MaximumLives, IsInfinity, this));

            _isInitialized = true;
        }

        private void OnGameStateChanged(GameStateChangedSignal signal)
        {
            if (signal.PreviousState.IsGameplay())
            {
                if (signal.CurrentState.IsLoading() || signal.CurrentState.Equals(EGameState.Fail))
                {
                    Spend();
                }
            }
            if (_isInitialized)
            {
                var isGameplay = signal.CurrentState.IsGameplay();
                _data.IsLastGameplayState = isGameplay;
                _signalBus.AbstractFire(new LivesChangedSignal(CurrentLives, MaximumLives, IsInfinity, this));
            }
        }

        private void OnApplicationPause(ApplicationPauseSignal status)
        {
            if (status.IsPaused)
            {
                if (IsInfinity)
                {
                    _data.LastRestoreTime = _networkTimeManager.TotalSeconds;
                }
            }
        }

        private void OnNetworkTimeChanged(NetworkTimeChangedSignal signal)
        {
            VerificateTime();
        }

        #region Cheats

        private void RegisterCheats()
        {
            var lives = new Cheat
            {
                Id = "lives",
                TitleLocId = "ui.dev.cheat.lives",

                Params = new CheatIteratorParams
                {
                    UsePreDefined = true,
                    PreDefinedValues = Enum.GetValues(typeof(CheatLivesCount)).Cast<object>().ToArray(),
                    PreDefinedAction = selectedValue =>
                    {
                        var enumState = (CheatLivesCount)selectedValue;

                        _data.LastRestoreTime = _networkTimeManager.TotalSeconds;
                        _data.LastInfinityTime = 0;
                        _data.Count = 0;
                        _timeout = _settings.RestTime;

                        switch (enumState)
                        {
                            default:
                            {
                                _signalBus.AbstractFire(new LivesChangedSignal(CurrentLives, MaximumLives, false, this));
                                break;
                            }
                            case CheatLivesCount.Half:
                            {
                                Increase(ELivesUsageType.Default, MaximumLives / 2);
                                break;
                            }
                            case CheatLivesCount.Full:
                            {
                                Increase(ELivesUsageType.Default, MaximumLives);
                                break;
                            }
                            case CheatLivesCount.Infinity1m:
                            {
                                Increase(ELivesUsageType.Infinite, 1);
                                break;
                            }
                            case CheatLivesCount.Infinity5m:
                            {
                                Increase(ELivesUsageType.Infinite, 5);
                                break;
                            }
                            case CheatLivesCount.Infinity30m:
                            {
                                Increase(ELivesUsageType.Infinite, 30);
                                break;
                            }
                        }
                    }
                }
            };

            _cheatSystem?.RegisterCheat(lives);
        }

        private void UnregisterCheats()
        {
            _cheatSystem?.UnregisterCheat("lives");
        }

        internal enum CheatLivesCount
        {
            None,
            Half,
            Full,
            Infinity1m,
            Infinity5m,
            Infinity30m
        }

        #endregion

        [Serializable]
        public sealed class Settings
        {
            [ShowAssetPreview(28, 28)]
            [SerializeField] private Sprite _icon;
            [SerializeField] private int _maxCount = 5;
            [SerializeField] private float _restTime = 600;

            public Sprite Icon => _icon;

            /// <summary>
            /// Default max life count
            /// </summary>
            public int MaxCount => _maxCount;
            /// <summary>
            /// Restore time in seconds
            /// </summary>
            public float RestTime => _restTime;
        }

        [Serializable]
        public sealed class Data : ISaveData
        {
            public int Count = 5;
            public int MaxCount = 5;
            public double LastInfinityTime;
            public double VerificateInfinityTime = DateTime.UtcNow.Ticks / 100000;
            public double LastRestoreTime = DateTime.UtcNow.Ticks / 100000;
            public bool IsLastGameplayState = false;

            public void FromDict(IDictionary<string, object> saveData)
            {
                if (saveData == null) return;

                Count = saveData.GetValue<int>("count", Count);
                MaxCount = saveData.GetValue<int>("max_count", MaxCount);
                LastInfinityTime = saveData.GetValue<double>("inf_time", LastInfinityTime);
                VerificateInfinityTime = saveData.GetValue<double>("verif_inf_time", VerificateInfinityTime);
                LastRestoreTime = saveData.GetValue<double>("rest_time", LastRestoreTime);
                IsLastGameplayState = saveData.GetValue<bool>("is_gameplay", IsLastGameplayState);
            }

            public IDictionary<string, object> ToDict
            {
                get
                {
                    return new Dictionary<string, object>
                    {
                        { "count", Count },
                        { "max_count", MaxCount },
                        { "inf_time", LastInfinityTime },
                        { "verif_inf_time", VerificateInfinityTime },
                        { "rest_time", LastRestoreTime },
                        { "is_gameplay", IsLastGameplayState }
                    };
                }
            }
        }
    }
}