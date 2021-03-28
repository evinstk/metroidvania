using Nez;

namespace Game
{
    class MusicTrigger : Component, IUpdatable
    {
        public string Track;

        public void Update()
        {
            var player = Entity.Scene.FindEntity("player");
            if (player == null) return;

            var collider = Entity.GetComponent<BoxCollider>();
            if (collider.Bounds.Contains(player.Position))
            {
                var soundSystem = Entity.Scene.FindComponentOfType<SoundSystem>();
                if (soundSystem.BankName != "Music" || soundSystem.EventName != Track)
                {
                    soundSystem.StopMusic(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                    soundSystem.LoadMusic("Music", Track);
                    soundSystem.PlayMusic();
                }
            }
        }
    }
}
