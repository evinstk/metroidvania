using System;

namespace Game
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "-e")
            {
                using (var editor = new EditorCore())
                    editor.Run();
            }
            else
            {
                using (var game = new Game())
                    game.Run();
            }
        }
    }
}
