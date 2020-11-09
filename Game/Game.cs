namespace FE
{
    public class Game : Nez.Core
    {
        protected override void Initialize()
        {
            base.Initialize();
            Window.AllowUserResizing = true;
            ExitOnEscapeKeypress = false;
            Scene = new MainScene("TestMap.tmx", "start");
        }
    }
}
