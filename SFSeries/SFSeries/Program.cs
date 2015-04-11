using LeagueSharp;
using LeagueSharp.Common;
using System;

namespace SFSeries
{
    class Program
    {
        public static string ChampionName;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            ChampionName = ObjectManager.Player.BaseSkinName;

            switch (ChampionName)
            {
                case "Katarina":
                    new Katarina();
                    break;
                case "Darius":
                    new Darius();
                    break;
                case "Kennen": // Currently disabled, not finished yet
                    new Kennen();
                    break;
                case "Singed":
                    new Singed();
                    break;
                
                default:
                    PrintMessage("This champion is not supported");
                    break;
            }
        }
        public static void PrintMessage(string msg) // Credits to ChewyMoon, and his Brain.exe
        {
            Game.PrintChat("<font color=\"#6699ff\"><b>SFSeries: </b></font> <font color=\"#FFFFFF\">" + msg + "</font>");
        }
    }
}
