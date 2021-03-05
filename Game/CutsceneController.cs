using Nez;
using System;

namespace Game
{
    class CutsceneController : Component
    {
        public Action<CutsceneController> OnPossess;
        public Action<CutsceneController> OnRelease;

        public void Possess() => OnPossess?.Invoke(this);
        public void Release() => OnRelease?.Invoke(this);
    }
}
