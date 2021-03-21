using Nez;

namespace Game
{
    class Sound : Component
    {
        public FMOD.Studio.EventInstance Event;

        string _bankName;
        string _evt;

        public Sound(string bankName, string evt)
        {
            _bankName = bankName;
            _evt = evt;
        }

        public override void OnAddedToEntity()
        {
            Event = GameContent.LoadSound(_bankName, _evt);
        }

        public override void OnRemovedFromEntity()
        {
            Event.release();
        }
    }
}
