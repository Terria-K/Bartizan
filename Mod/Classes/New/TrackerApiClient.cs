using TowerFall;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using SDL2;
#if (STAT_TRACKING)
  using Newtonsoft.Json.Linq;
#endif

namespace Mod
{
  public class TrackerApiClient
  {
    #if (STAT_TRACKING)
      private string apiUrl;
      private string apiKey;
    #endif
    private bool isSetup;

    public TrackerApiClient()
    {
      this.isSetup = false;
      #if (STAT_TRACKING)
        string trackerApiSettingsFile = Path.Combine (GetSavePath(), "tf-tracker-api.txt");
        if (File.Exists (trackerApiSettingsFile)) {
          string[] trackerApiSettings = File.ReadAllLines(trackerApiSettingsFile);
          if (trackerApiSettings.Length < 2) {
            TFGame.Log(new Exception("Invalid tf-tracker-api.txt contents"), false);
          } else {
            this.isSetup = true;
            this.apiUrl = trackerApiSettings[0];
            this.apiKey = trackerApiSettings[1];
          }
        }
      #endif
    }

    // This SHOULD be accessible from TFGame but isn't so I copy/pasted it
    public static string GetSavePath ()
    {
      string text = SDL.SDL_GetPlatform ();
      string result;
      if (text.Equals ("Linux")) {
        string text2 = Environment.GetEnvironmentVariable ("XDG_DATA_HOME");
        if (string.IsNullOrEmpty (text2)) {
          text2 = Environment.GetEnvironmentVariable ("HOME");
          if (string.IsNullOrEmpty (text2)) {
            result = ".";
            return result;
          }
          text2 += "/.local/share";
        }
        text2 += "/TowerFall";
        if (!Directory.Exists (text2)) {
          Directory.CreateDirectory (text2);
        }
        result = text2;
      } else if (text.Equals ("Mac OS X")) {
        string text2 = Environment.GetEnvironmentVariable ("HOME");
        if (string.IsNullOrEmpty (text2)) {
          result = ".";
        } else {
          text2 += "/Library/Application Support/TowerFall";
          if (!Directory.Exists (text2)) {
            Directory.CreateDirectory (text2);
          }
          result = text2;
        }
      } else {
        if (!text.Equals ("Windows")) {
          throw new Exception ("SDL2 platform not handled!");
        }
        result = AppDomain.CurrentDomain.BaseDirectory;
      }
      return result;
    }

    public bool IsSetup()
    {
      return this.isSetup;
    }

    #if (STAT_TRACKING)
      public void GetPlayerNames() {
        Action<string> callback = (response) => {
          JObject playerNames = JObject.Parse(response);
          MyGlobals.playerNames = new PlayerNames(playerNames);
        };
        this.MakeRequest("GET", "active-names", "", callback);
      }

      public void GetRoster() {
        Action<string> callback = (response) => {
          JArray roster = JArray.Parse(response);
          MyGlobals.roster = roster;
        };
        this.MakeRequest("GET", "roster", "", callback);
      }

      public void UpdatePlayer(int playerId, JObject payload)
      {
        Action<string> callback = (response) => {
          JToken payloadColor = payload["color"];
          foreach (JObject player in MyGlobals.roster)
          {
            if (
              payloadColor.Type != JTokenType.Null &&
              (int)player["user_id"] != playerId &&
              player["color"].ToString() == payloadColor.ToString()
            ) {
              // Unset previous player with that color.
              player["color"] = null;
            } else if ((int)player["user_id"] == playerId) {
              player["color"] = payload["color"];
            }
          }
        };
        this.MakeRequest(
          "PATCH",
          "player/" + playerId.ToString(),
          payload.ToString().Replace("\"", "\\\""),
          callback
        );
      }

      public void SaveStats(JObject stats) {
        this.MakeRequest("POST", "matches", stats.ToString().Replace("\"", "\\\"").Replace("'", "'\\''"));
      }

      public void MakeRequest(string method, string path, string payload="", Action<string> callback=null)
      {
        try {
          using (Process process = new Process())
          {
            var commandString = "";
            commandString += (
              "-c \"curl '" + apiUrl + path + "' " +
              "-X" + method + " -H 'Content-Type: application/json' -H 'Accept: application/json' " +
              "-H 'Authorization: ApiKey " + apiKey + "'"
            );
            if (payload != "") {
              commandString += " --data-binary '" + payload + "'";
            }
            commandString += " --compressed\"";
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = commandString;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            if (callback != null) {
              StringBuilder response = new StringBuilder();
              process.OutputDataReceived += (sender, args) => {
                if (String.IsNullOrEmpty(args.Data)) {
                  callback(response.ToString());
                } else {
                  response.AppendLine(args.Data);
                }
              };

              process.BeginOutputReadLine();
            }

            process.WaitForExit();
          }
        } catch (Exception e) {
          TFGame.Log(new Exception(e.Message), false);
        }
      }
    #endif
  }
}