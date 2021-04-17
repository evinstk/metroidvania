using Game.Prototypes;
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
            else if (args.Length == 2 && args[0] == "-p")
            {
                using (var prototype = new PrototypeCore(args[1]))
                    prototype.Run();
            }
            else
            {
                using (var game = new Game())
                    game.Run();
            }
        }
    }
}
