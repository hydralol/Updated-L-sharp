using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

using SharpDX;
using Color = System.Drawing.Color;

namespace Balista
{
    internal class Program
    {
        /*
         * This is my first assembly/script whatever you wanna call it written for LeagueSharp(L#).
         * This can and will be improved as I progress to learn more about LeagueSharp.Common etc.
         * I've made a few programs before and I always struggle with the most important thing: Performance.
         * So if you think one of my assemblies/programs can be improved please contact me. :)
        */

        public static string BalistaVersion = "1.0.0.4";
        public static Spell R;
        private static Obj_AI_Hero Player;
        public static Menu menu;


        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static bool BlitzInGame()
        {
            return ObjectManager.Get<Obj_AI_Hero>().Any(h => h.IsAlly && !h.IsMe && h.ChampionName == "Blitzcrank");
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            //Check to see if it is Kalista.
            if (Player.ChampionName != "Kalista") return;

            //Check if you have a Blitzfriend :)
            if(!BlitzInGame()) return;

            R = new Spell(SpellSlot.R, 1500);
            R.SetSkillshot(0.50f, 1500, float.MaxValue, false, SkillshotType.SkillshotCircle);

            menu = new Menu("Balista", "Balista", true);
            {
                menu.AddItem(new MenuItem("useToggle", "Toggle").SetValue(false));
                menu.AddItem(new MenuItem("useOnComboKey", "Enabled").SetValue(new KeyBind(32, KeyBindType.Press)));
            }
            Menu targetMenu = new Menu("Target Selector", "Target Selector");
            {
                foreach (
                    Obj_AI_Hero enem in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(enem => enem.IsValid && enem.IsEnemy))
                {
                    targetMenu.AddItem(new MenuItem("target"+enem.ChampionName, enem.ChampionName).SetValue(true));
                }
            }
            Menu drawMenu = new Menu("Drawings", "Drawings");
            {
                drawMenu.AddItem(new MenuItem("minBRange", "Balista Min Range", true).SetValue(new Circle(false, Color.Chartreuse)));
                drawMenu.AddItem(new MenuItem("maxBRange", "Balista Max Range", true).SetValue(new Circle(false, Color.Green)));
            }
            Menu misc = new Menu("Misc", "misc");
            {
                misc.AddItem(new MenuItem("minRange", "Min Range to Balista", true).SetValue(new Slider(700, 100, 1449)));
                misc.AddItem(new MenuItem("maxRange", "Max Range to Balista", true).SetValue(new Slider(1500, 100, 1500)));
                misc.AddItem(new MenuItem("usePackets", "Use Packets").SetValue(false));
            }

            menu.AddSubMenu(targetMenu);
            menu.AddSubMenu(drawMenu);
            menu.AddSubMenu(misc);
            menu.AddToMainMenu();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            //This is art.. =.='
            Game.PrintChat("<font size='21'><b><font color='#f8c906'>B</font><font color='#00bce5'>alista:</font></b></font><font size='17'> v" + BalistaVersion + " <font color='#189108'>loaded!</font> - <font color='#cccccc'>Hydralolz</font></font>");
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (menu.Item("minBRange", true).GetValue<Circle>().Active)
            Render.Circle.DrawCircle(Player.Position, menu.Item("minRange", true).GetValue<Slider>().Value, menu.Item("minBRange", true).GetValue<Circle>().Color, 3);
            if (menu.Item("maxBRange", true).GetValue<Circle>().Active)
                Render.Circle.DrawCircle(Player.Position, menu.Item("maxRange", true).GetValue<Slider>().Value, menu.Item("maxBRange", true).GetValue<Circle>().Color, 3);
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead || !menu.Item("useToggle").GetValue<bool>() &&
                !menu.Item("useOnComboKey").GetValue<KeyBind>().Active || !R.IsReady()) return;

            var blitzfriend =
                ObjectManager.Get<Obj_AI_Hero>()
                    .SingleOrDefault(
                        x =>
                            x.IsAlly && Player.Distance(x.ServerPosition) < menu.Item("maxRange", true).GetValue<Slider>().Value &&
                            Player.Distance(x.ServerPosition) >= menu.Item("minRange", true).GetValue<Slider>().Value &&
                            x.ChampionName == "Blitzcrank");

            if (blitzfriend == null)
                return;


            foreach (
                Obj_AI_Hero enem in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(enem => enem.IsValid && enem.IsEnemy && enem.Distance(Player) <= 2450f)) //+950f is blitz Q range.
            {
                if (menu.Item("target" + enem.ChampionName).GetValue<bool>() && enem.Health > 200)
                {
                    if (enem.Buffs != null)
                    {
                        for (int i = 0; i < enem.Buffs.Count(); i++)
                        {
                            if (enem.Buffs[i].Name == "rocketgrab2" && enem.Buffs[i].IsActive)
                            {
                                if (R.IsReady())
                                {
                                    R.Cast(menu.Item("usePackets").GetValue<bool>());
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
