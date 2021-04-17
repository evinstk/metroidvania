using Nez;

namespace Game.Prototypes
{
    class PrototypeCore : Core
    {
        string _scriptPath;

        public PrototypeCore(string scriptPath)
        {
            _scriptPath = scriptPath;
        }

        protected override void Initialize()
        {
            base.Initialize();

            Scene = new PrototypeScene(_scriptPath);
        }
    }
}
