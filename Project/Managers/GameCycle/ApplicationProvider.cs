using UnityEngine;
using Zenject;

namespace Project.Managers.Game
{
    public sealed class ApplicationFocusSignal
    {
        public bool HasFocus;
    }

    public sealed class ApplicationPauseSignal
    {
        public bool IsPaused;
    }

    public sealed class ApplicationProvider : MonoBehaviour
    {
        private SignalBus _signalBus;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            //LogSystem.Trace($"Application focus is {hasFocus}");
            _signalBus?.Fire(new ApplicationFocusSignal { HasFocus = hasFocus });
        }

        private void OnApplicationPause(bool isPaused)
        {
            //LogSystem.Trace($"Application pause is {isPaused}");
            _signalBus?.Fire(new ApplicationPauseSignal { IsPaused = isPaused });
        }
    }
}