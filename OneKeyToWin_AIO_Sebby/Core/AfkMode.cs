using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace OneKeyToWin_AIO_Sebby
{
    class AfkMode
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private Vector3 destination;
        public void LoadOKTW()
        {
            Game.OnUpdate += Game_OnGameUpdate;
            Config.SubMenu("AfkMode").AddItem(new MenuItem("AfkMode", "AfkMode BETA").SetValue(false));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team == Player.Team))
                Config.SubMenu("AfkMode").SubMenu("Fallow").AddItem(new MenuItem("ally" + enemy.ChampionName, enemy.ChampionName).SetValue(false));
            
            Config.SubMenu("AfkMode").SubMenu("Mode").AddItem(new MenuItem("laneclear", "leaneclear").SetValue(true));
            Config.SubMenu("AfkMode").SubMenu("Mode").AddItem(new MenuItem("mixed", "Mixed").SetValue(false));
            Config.SubMenu("AfkMode").SubMenu("Mode").AddItem(new MenuItem("combo", "Combo").SetValue(false));
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("AfkMode").GetValue<bool>())
            {

                Orbwalking.OrbwalkingMode ActiveMode = Orbwalking.OrbwalkingMode.LaneClear;
                if (Config.Item("laneclear").GetValue<bool>())
                    ActiveMode = Orbwalking.OrbwalkingMode.LaneClear;
                else if (Config.Item("mixed").GetValue<bool>())
                    ActiveMode = Orbwalking.OrbwalkingMode.Mixed;
                else if (Config.Item("combo").GetValue<bool>())
                    ActiveMode = Orbwalking.OrbwalkingMode.Combo;

                Obj_AI_Hero fallow = Player;
                foreach (var ally in Program.Allies.Where(ally => ally.IsValid))
                {
                    if (Config.Item("ally" + ally.ChampionName).GetValue<bool>() )
                    {
                        fallow = ally;
                    }
                }

                if (fallow != Player)
                {
                    if (fallow.HasBuff("Recall") && fallow.InFountain())
                    {
                        Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                    }
                    List<Vector2> waypoints = fallow.GetWaypoints();

                    Orbwalker.ActiveMode = ActiveMode;
                    Orbwalker.SetOrbwalkingPoint(waypoints.Last<Vector2>().To3D());
                }
                else
                {
                    Orbwalker.ActiveMode = ActiveMode;
                    Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                }
            }
            else
            {
                Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.None;
            }
        }
        
    }
}
