using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Patcher;

namespace Wizard
{
  static class Program
  {
    static void TryCopy(string src, string dest)
    {
      if (!File.Exists(dest)) {
        File.Copy(src, dest);
      }
    }

    static IEnumerable<string> RelativeEnumerateFiles(string path)
    {
      path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      return Directory.EnumerateFiles(path).Select(file => file.Substring(path.Length + 1));
    }

    static bool IsProbablyTowerFallDir(string path)
    {
      return File.Exists(Path.Combine(path, "TowerFall.exe"));
    }

    [STAThread]
    static void Main()
    {
      bool noArgs = Environment.GetCommandLineArgs().Count() == 1;
      try {
        Environment.CurrentDirectory = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        string destPath = "";
        if (noArgs) {
          Application.EnableVisualStyles();
          Application.SetCompatibleTextRenderingDefault(false);

          destPath = (Environment.GetEnvironmentVariable("ProgramFiles(x86)") ?? "") + @"\Steam\SteamApps\common\TowerFall";
          if (!IsProbablyTowerFallDir(destPath)) {
            destPath = (Environment.GetEnvironmentVariable("HOME") ?? "") + "/.steam/steam/SteamApps/common/TowerFall";
          }

          if (!IsProbablyTowerFallDir(destPath)) {
            Console.WriteLine("Could not locate TowerFall directory. Try specifying the path as an argument.");
            return;
          }
        } else {
          destPath = Environment.GetCommandLineArgs()[1];
        }

        // If backup exists, restore the original TowerFall.exe before patching
        if (File.Exists(Path.Combine(destPath, "TowerFall-Original.exe"))) {
          File.Copy(Path.Combine(destPath, "TowerFall-Original.exe"), Path.Combine(destPath, "TowerFall.exe"), overwrite: true);
        } else {
          File.Copy(Path.Combine(destPath, "TowerFall.exe"), Path.Combine(destPath, "TowerFall-Original.exe"), overwrite: true);
        }

        Patcher.Patcher.Patch("Mod.dll", destPath, Path.Combine(destPath, "TowerFall.exe"));

        File.Copy(Path.Combine("Mod.dll"), Path.Combine(destPath, "Mod.dll"), overwrite: true);
        File.Copy(Path.Combine("modAtlas.xml"), Path.Combine(destPath, "Content", "Atlas", "modAtlas.xml"), overwrite: true);
        File.Copy(Path.Combine("modAtlas.png"), Path.Combine(destPath, "Content", "Atlas", "modAtlas.png"), overwrite: true);

        Console.WriteLine("Success!");
      } catch (Exception e) {
        throw e;
      }
    }
  }
}
