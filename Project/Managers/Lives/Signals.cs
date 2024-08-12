using Project.Systems.Save;

namespace Project.Managers.Lives.Signals
{
    public class LivesChangedSignal : ISaveRequestSignal
    {
        private int _amount;
        private int _maxAmount;
        private bool _isInfinity;

        private ISaveDataProvider _saveDataProvider;

        public LivesChangedSignal(int amount, int maxAmount, bool isInfinity, ISaveDataProvider saveDataProvider)
        {
            _saveDataProvider = saveDataProvider;

            _amount = amount;
            _maxAmount = maxAmount;
            _isInfinity = isInfinity;
        }

        public int Amount => _amount;

        public int MaxAmount => _maxAmount;

        public bool IsInfinity => _isInfinity;

        public ISaveDataProvider SaveDataProvider => _saveDataProvider;
    }
}