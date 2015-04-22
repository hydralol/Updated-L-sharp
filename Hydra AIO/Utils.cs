#region
using System;
using LeagueSharp;
using Color = SharpDX.Color;
using Font = SharpDX.Direct3D9.Font;
#endregion

namespace HydraAIO
{
    internal static class Utils
    {
        public static void PrintMessage(string message)
        {
            Game.PrintChat("<font color='#70DBDB'>Hydra AIO:</font> <font color='#FFFFFF'>" + message + "</font>");
        }

        public static void DrawText(Font vFont, String vText, int vPosX, int vPosY, Color vColor)
        {
            vFont.DrawText(null, vText, vPosX + 2, vPosY + 2, vColor != Color.Black ? Color.Black : Color.White);
            vFont.DrawText(null, vText, vPosX, vPosY, vColor);
        }
    }
}
