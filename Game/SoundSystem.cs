using Nez;

namespace Game
{
    class SoundSystem : Component
    {
        FMOD.Studio.EventInstance _music;

        public void LoadMusic(string bankName, string evt)
        {
            ReleaseMusic();
            _music = GameContent.LoadSound(bankName, evt);
        }

        public void PlayMusic()
        {
            _music.start();
        }

        public void StopMusic(FMOD.Studio.STOP_MODE stopMode)
        {
            _music.stop(stopMode);
        }

        public override void OnRemovedFromEntity()
        {
            ReleaseMusic();
        }

        void ReleaseMusic()
        {
            if (_music.hasHandle())
            {
                _music.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _music.release();
            }
        }
    }
}
