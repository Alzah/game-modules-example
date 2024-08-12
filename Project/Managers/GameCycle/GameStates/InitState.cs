using Project.Extensions;
using Project.Helpers;
using Project.Managers.Async;
using Project.Managers.NetworkState;
using Project.Managers.NetworkTime;
using Project.Managers.Player;
using Project.Systems.AbTest;
using Project.Systems.Analytics;
using Project.Systems.Localization;
using Project.Systems.Logging;
using Project.Systems.Save;
using Project.Systems.UI.Windows;
using Project.Systems.Versioning;
using System.Collections;
using UnityEngine;
using Zenject;

namespace Project.Managers.Game.GameStates
{
    public class InitState : GenericGameState<EGameState>
    {
        private readonly AbTestSystem _abTestSystem;
        private readonly AsyncManager _asyncManager;
        private readonly GameVersion _gameVersion;
        private readonly BackendManager _backendManager;
        private readonly PlayerManager _playerManager;
        private readonly LocalizationSystem _localizationSystem;
        private readonly NetworkTimeManager _networkTimeManager;
        private readonly NetworkStateManager _networkStateManager;
        private readonly IWindowManager _windowManager;
        private readonly ISaveSystem _saveSystem;
        private readonly IAnalyticsSystem _analyticsSystem;
        private readonly IAnalyticsUserPropertiesCollector _analyticsUserPropertiesCollector;

        private Coroutine _startSequence;

        public override EGameState StateType => EGameState.Init;

        public InitState(AbTestSystem abTestSystem, AsyncManager asyncManager, GameVersion gameVersion, BackendManager backendManager,
            PlayerManager playerManager, LocalizationSystem localizationSystem,
            NetworkTimeManager networkTimeManager, NetworkStateManager networkStateManager,
            IWindowManager windowManager, ISaveSystem saveSystem,
            IAnalyticsSystem analyticsSystem, IAnalyticsUserPropertiesCollector analyticsUserPropertiesCollector)
        {
            _abTestSystem = abTestSystem;
            _asyncManager = asyncManager;
            _gameVersion = gameVersion;
            _backendManager = backendManager;
            _playerManager = playerManager;
            _localizationSystem = localizationSystem;
            _networkTimeManager = networkTimeManager;
            _networkStateManager = networkStateManager;

            _windowManager = windowManager;
            _saveSystem = saveSystem;
            _analyticsSystem = analyticsSystem;
            _analyticsUserPropertiesCollector = analyticsUserPropertiesCollector;
        }

        public override void Enter<TParams>(TParams param = null)
        {
            // set frame rate
            SetFrameRate();

            // select language by system language
            SelectLanguage();

            _startSequence = _asyncManager.StartCoroutine(StartSequence());
        }

        public override void Exit()
        {
            if (_startSequence != null)
            {
                _asyncManager.StopCoroutine(_startSequence);
                _startSequence = null;
            }

            base.Exit();
        }
        private void SetFrameRate()
        {
            var targetFrameRate = 60;

            if (Application.isEditor)
            {
                targetFrameRate = -1;
            }
            else
            {
                switch (_gameVersion.Type)
                {
                    case EBuildType.Dev:
                    {
                        targetFrameRate = 120;
                        break;
                    }
                }
            }

            Application.targetFrameRate = targetFrameRate;
        }

        private void SelectLanguage()
        {
            var language = ELanguage.En;

            switch (Application.systemLanguage)
            {
                case SystemLanguage.Russian:
                case SystemLanguage.Belarusian:
                case SystemLanguage.Ukrainian:
                {
                    language = ELanguage.Ru;
                    break;
                }
            }

            _localizationSystem.SwitchTextLanguage(language);
        }

        private IEnumerator StartSequence()
        {
            LogSystem.Trace($"Game started with Version: {_gameVersion}, userId: {_playerManager.UserId}, session: {_playerManager.Sessions}");

            yield return new WaitForEndOfFrame();

            // wait network check
            yield return new WaitWhile(() => _networkStateManager.IsPending);

            var isNetworkAvailable = _networkStateManager.IsAvailable;

            if (isNetworkAvailable)
            {
                // check min version
                var isAllowedByMinVersion = false;

                var minVersionPromise = _backendManager.CheckMinVersion().Then(isAllowed =>
                {
                    isAllowedByMinVersion = isAllowed;
                });

                yield return new WaitWhilePending(minVersionPromise);

                if (!isAllowedByMinVersion)
                {
                    LogSystem.Error($"Min version error. Current [{_gameVersion}] < Min version [{_backendManager.MinVersion}]");

                    _windowManager.Open<ErrorWindow>(new ErrorWindowParams
                    {
                        TextTitle = _localizationSystem.Get("ui.common.error"),
                        TextDescription = _localizationSystem.Get("ui.error.game.min_version", $"{_backendManager.MinVersion}"),
                        TextButton = _localizationSystem.Get("ui.common.ok"),
                        OnCloseClick = UnityUtils.ApplicationQuit
                    });

                    yield break;
                }
            }

            // load save data
            _saveSystem.Load();

            // wait all systems && managers load
            yield return new WaitUntil(() => _saveSystem.IsInitialized);

            _abTestSystem.Run();

            // collect analytics user properties
            _analyticsUserPropertiesCollector.CollectOnStart();

            if (_playerManager.IsFirstLaunch)
            {
                // first launch, reset flag, send analytics
                _playerManager.FirstLaunch();
                _analyticsSystem.FirstStart();
            }
            else
            {
                // send previous session info to analytics
                var previousSessionNumber = _playerManager.Sessions;
                var previousSessionDate = _playerManager.SessionLastUTC;

                _analyticsSystem.SessionEnd(new AnalyticsEventSessionEnd(previousSessionNumber, previousSessionDate));

                // increase session number
                _playerManager.IncreaseSession();
            }

            // send current session info to analytics 
            var currentSessionNumber = _playerManager.Sessions;
            var currentSessionDate = _networkTimeManager.DateTimeUTC;

            _analyticsSystem.SessionStart(new AnalyticsEventSessionStart(currentSessionNumber, currentSessionDate));

            IsEntered = true;
        }

        public class Factory : PlaceholderFactory<InitState>
        {
        }
    }
}
