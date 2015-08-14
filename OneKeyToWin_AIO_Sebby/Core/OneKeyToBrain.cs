using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace OneKeyToWin_AIO_Sebby.Core
{
    class ChampionInfo
    {
        public int NetworkId { get; set; }
        public Vector3 LastVisablePos { get; set; }
        public float LastVisableTime { get; set; }
        public Vector3 PredictedPos { get; set; }
    }

    class OneKeyToBrain
    {
        private Menu Config = Program.Config;
        public Font Text, TextBold;
        public List<ChampionInfo> ChampionInfoList = new List<ChampionInfo>();

        public void LoadOKTW()
        {
            TextBold = new Font(
                Drawing.Direct3DDevice,
                new FontDescription
                {

                    FaceName = "Impact",
                    Height = 36,
                    Weight = FontWeight.Normal,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.Default
                });

            Text = new Font(
                Drawing.Direct3DDevice,
                new FontDescription
                {
                    FaceName = "Calibri",
                    Height = 16,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.ClearType
                });

            Config.SubMenu("Utility, Draws OKTW©").AddItem(new MenuItem("SS", "SS notification").SetValue(true));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += OnUpdate;
        }

        private void OnUpdate(EventArgs args)
        {
            if (!Program.LagFree(4))
                return;
            foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValid))
            {
                var ChampionInfoOne = ChampionInfoList.Find(x => x.NetworkId == enemy.NetworkId);
                if (enemy.IsVisible && !enemy.IsDead && enemy != null && enemy.IsValidTarget())
                {

                    var prepos = Prediction.GetPrediction(enemy, 0.4f).CastPosition;

                    
                    if (ChampionInfoOne == null)
                    {
                        ChampionInfoList.Add(new ChampionInfo() { NetworkId = enemy.NetworkId, LastVisablePos = enemy.Position, LastVisableTime = Game.Time, PredictedPos = prepos });
                    }
                    else
                    {
                        ChampionInfoOne.NetworkId = enemy.NetworkId;
                        ChampionInfoOne.LastVisablePos = enemy.Position;
                        ChampionInfoOne.LastVisableTime = Game.Time;
                        ChampionInfoOne.PredictedPos = prepos;
                    }
                    
                }
                if (enemy.IsDead)
                {
                    if (ChampionInfoOne != null)
                    {
           
                        ChampionInfoOne.NetworkId = enemy.NetworkId;
                        ChampionInfoOne.LastVisablePos = enemy.Position;
                        ChampionInfoOne.LastVisableTime = Game.Time;
                    }
                }
            }
        }

        public void DrawText(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int)vPosX, (int)vPosY, vColor);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("disableDraws").GetValue<bool>())
                return;

            float offset = 0;
            foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValid))
            {
                offset += 0.15f;
                if (!enemy.IsVisible && !enemy.IsDead)
                {
                    if (Config.Item("SS").GetValue<bool>())
                    {
                        var ChampionInfoOne = ChampionInfoList.Find(x => x.NetworkId == enemy.NetworkId);
                        if (ChampionInfoOne != null && enemy != Program.jungler)
                        {
                            if (Game.Time - ChampionInfoOne.LastVisableTime > 3 && Game.Time - ChampionInfoOne.LastVisableTime < 7)
                            {
                                if ((int)(Game.Time * 10) % 2 == 0)
                                {
                                    DrawText(TextBold, "SS " + enemy.ChampionName + " " + (int)(Game.Time - ChampionInfoOne.LastVisableTime), Drawing.Width * offset, Drawing.Height * 0.01f, SharpDX.Color.OrangeRed);
                                }
                            }
                            if (Game.Time - ChampionInfoOne.LastVisableTime >= 7)
                            {
                                DrawText(TextBold, "SS " + enemy.ChampionName + " " + (int)(Game.Time - ChampionInfoOne.LastVisableTime), Drawing.Width * offset, Drawing.Height * 0.01f, SharpDX.Color.OrangeRed);
                            }
                        }
                    }
                }
            }
        }
    }
}
