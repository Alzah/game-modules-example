using Project.Managers.Levels.Interfaces;
using Project.Systems.Analytics;
using System;
using Zenject;

namespace Project.Managers.Game.GameStates
{
    public class WinState : GenericGameState<EGameState>
    {
        private WinStateParams _levelStateParams;
        private readonly IAnalyticsSystem _analyticsSystem;
        private readonly ILevelProgressManager _levelProgressManager;

        public override EGameState StateType => EGameState.Win;

        public WinState(ILevelProgressManager levelProgressManager,
            IAnalyticsSystem analyticsSystem)
        {
            _levelProgressManager = levelProgressManager;
            _analyticsSystem = analyticsSystem;
        }

        public override void Enter<TParams>(TParams param = null)
        {
            _levelStateParams = param as WinStateParams;

            if (_levelStateParams == null) return;

            var levelNumber = _levelStateParams.LevelNumber;
            var levelType = _levelStateParams.LevelType.ToString();
            var levelDifficulty = _levelStateParams.Difficulty;
            var playerMoves = _levelStateParams.Moves;
            var playerTime = TimeSpan.FromSeconds(_levelStateParams.Time);

            // TODO: moves purchase count
            var movesPurchased = 0;
            // TODO: time purchase count
            var timePurchased = 0;

            _analyticsSystem.LevelWin(new AnalyticsEventLevelWin(levelNumber, levelType, levelDifficulty, playerMoves, playerTime, movesPurchased, timePurchased));
            _levelProgressManager.CompleteLevel(_levelStateParams.LevelType, _levelStateParams.LevelNumber, _levelStateParams.LevelTime, _levelStateParams.RewardId);

            base.Enter(param);
        }

        public override void Tick()
        {
        }

        public class Factory : PlaceholderFactory<WinState> { }

        public class WinStateParams : GameStateParams
        {
            public Enum LevelType;
            public int LevelNumber;
            public float LevelTime;
            public int PenguinsCount;
            public string RewardId;
            public float Time;
            public int Moves;
            public string Difficulty;
        }
    }
}
