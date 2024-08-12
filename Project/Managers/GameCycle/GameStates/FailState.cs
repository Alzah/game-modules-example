using Project.Extensions;
using Project.Systems.Analytics;
using System;
using Zenject;

namespace Project.Managers.Game.GameStates
{
    public class FailState : GenericGameState<EGameState>
    {
        private readonly GameManager _gameManager;
        private readonly IAnalyticsSystem _analyticsSystem;
        private FailStateParams _levelStateParams;

        public override EGameState StateType => EGameState.Fail;

        public FailState(GameManager gameManager, IAnalyticsSystem analyticsSystem)
        {
            _analyticsSystem = analyticsSystem;
            _gameManager = gameManager;
        }

        public override void Enter<TParams>(TParams param = null)
        {
            _levelStateParams = param as FailStateParams;

            if (_levelStateParams != null && _gameManager.GetPreviousState().IsGameplay())
            {
                var levelNumber = _levelStateParams.LevelNumber;
                var levelType = _levelStateParams.LevelType.ToString();
                var levelDifficulty = _levelStateParams.Difficulty;
                var playerMoves = _levelStateParams.Moves;
                var playerTime = TimeSpan.FromSeconds(_levelStateParams.Time);
                var reason = _levelStateParams.Reason.ToString();

                //TODO: send time and move purchase on level
                var timePurchased = 0;
                var movesPurchased = 0;

                _analyticsSystem.LevelFail(new AnalyticsEventLevelFail(levelNumber, levelType, levelDifficulty, playerMoves, playerTime, movesPurchased, timePurchased, reason));
            }

            base.Enter(param);
        }

        public class Factory : PlaceholderFactory<FailState> { }

        public class FailStateParams : GameStateParams
        {
            public Enum LevelType;
            public int LevelNumber;
            public EFailReason Reason;
            public float Time;
            public int Moves;
            public string Difficulty;
        }

        public enum EFailReason
        {
            NoMoves,
            ZeroStep,
            Timeout
        }
    }
}