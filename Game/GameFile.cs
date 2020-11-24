using System;
using System.IO;

namespace Game
{
    static class GameFile
    {
        public static string LocalDir => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Metroidvania/";

        public static string SaveFile => "save.json";
        public static string SavePath => LocalDir + SaveFile;

        public static string ConfigFile => "config.json";
        public static string ConfigPath => LocalDir + ConfigFile;

        static GameFile()
        {
            Directory.CreateDirectory(LocalDir);
        }
    }
}
