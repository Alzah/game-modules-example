using Project.Managers.Scene;
using Project.Systems.Analytics;
using Project.Systems.Logging;
using System;
using Zenject;

namespace Project.Managers.Game.GameStates
{
    public class LevelState : GenericGameState<EGameState>
    {
        private LevelStateParams _levelStateParams;
        private readonly SceneManager _sceneManager;
        private readonly IAnalyticsSystem _analyticsSystem;

        public override EGameState StateType => EGameState.Level;

        public LevelState(SceneManager sceneManager, IAnalyticsSystem analyticsSystem)
        {
            _analyticsSystem = analyticsSystem;
            _sceneManager = sceneManager;
        }

        public override void Enter<TParams>(TParams param = null)
        {
            _levelStateParams = param as LevelStateParams;

            if (_levelStateParams != null)
            {
                var levelNumber = _levelStateParams.LevelNumber;
                var levelType = _levelStateParams.LevelType.ToString();
                var levelDifficulty = _levelStateParams.LevelDifficulty;

                _analyticsSystem.LevelStart(new AnalyticsEventLevelStart(levelNumber, levelType, levelDifficulty));
            }

            _sceneManager.SwitchScene(new SceneDescriptor("Level", UnityEngine.SceneManagement.LoadSceneMode.Additive))
                .Then(scene =>
                {
                    IsEntered = true;
                })
                .Catch(e => LogSystem.Error($"Can't switch scene to Level: {e}"));
        }

        public class Factory : PlaceholderFactory<LevelState> { }

        public class LevelStateParams : GameStateParams
        {
            public int LevelNumber;
            public Enum LevelType;
            public string LevelDifficulty;
            public object OverrideLevel;
        }
    }
}