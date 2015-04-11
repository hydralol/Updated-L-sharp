using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Linq;
using LX_Orbwalker;
using Color = System.Drawing.Color;

namespace SFSeries
{
    class Darius
    {

        //Orbwalker instance

        //Spells
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        //Menu
        public static Menu Config;
        private static Obj_AI_Hero _player;


        public Darius()
        {
            OnGameLoaded();
        }

        private static void OnGameLoaded()
        {
            _player = ObjectManager.Player;
            Q = new Spell(SpellSlot.Q, 425);
            W = new Spell(SpellSlot.W, 125);
            E = new Spell(SpellSlot.E, 540);
            R = new Spell(SpellSlot.R, 460);



            Game.PrintChat("Darius Loaded! By iSnorflake V2");
            //Create the menu
            Config = new Menu("SFSeries", "SFSeries", true);

            //Orbwalker submenu
            var orbwalkerMenu = new Menu("Orbwalker", "LX_Orbwalker");
            LXOrbwalker.AddToMenu(orbwalkerMenu);
            Config.AddSubMenu(orbwalkerMenu);

            //Add the targer selector to the menu.
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);



            //Combo menu
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            
            // Drawings
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(new Circle(true, Color.FromArgb(150, Color.Crimson))));
            Config.AddSubMenu(new Menu("Exploits", "Exploits"));
            Config.SubMenu("Exploits").AddItem(new MenuItem("NFE", "No-Face Exploit").SetValue(true));
            // Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.AddToMainMenu();
            //Add the events we are going to use
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            LXOrbwalker.AfterAttack += LXOrbwalker_AfterAttack;



        }

        private static void LXOrbwalker_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (!unit.IsMe ||
                (LXOrbwalker.CurrentMode != LXOrbwalker.Mode.Combo && LXOrbwalker.CurrentMode != LXOrbwalker.Mode.Harass))
                return;
            if (!(target is Obj_AI_Hero)) return;
            
                W.Cast();
            Orbwalking.ResetAutoAttackTimer();
            
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (_player.IsDead) return;
            switch (LXOrbwalker.CurrentMode)
            {
                case LXOrbwalker.Mode.Combo:
                    Combo();
                    break;
                case LXOrbwalker.Mode.Harass:
                    Harras();
                    break;
                case LXOrbwalker.Mode.LaneClear:
                    LaneClear();
                    break;
            }
        }

        private static void LaneClear()
        {
            if (!Orbwalking.CanMove(40)) return;

            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (!Q.IsReady()) return;
// ReSharper disable once UnusedVariable
            foreach (var minion in allMinions.Where(minion => minion.Distance(_player) < Q.Range))
            {
                Q.Cast();
            }
        }

        private static void Harras()
        {
            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
            if (target == null) return;

            if (Q.IsReady() && _player.Distance(target) < Q.Range + target.BoundingRadius)
            {
                Q.Cast();
            }
            if (E.IsReady() && _player.Distance(target) < E.Range)
            {
                E.Cast(target.ServerPosition, Config.Item("NFE").GetValue<bool>());
            }
        }

        private static void Combo()
        {
            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
            if (target == null) return;

            if (Q.IsReady() && _player.Distance(target) < Q.Range + target.BoundingRadius)
            {
                Q.Cast();
            }
            if (E.IsReady() && _player.Distance(target) < E.Range)
            {
                E.Cast(target.ServerPosition, Config.Item("NFE").GetValue<bool>());
            }
            if (!R.IsReady() || !(_player.GetSpellDamage(target, SpellSlot.R, 2) > target.Health)) return;
            Game.PrintChat("Damage: " + (_player.GetSpellDamage(target,SpellSlot.R,2)));
            R.CastOnUnit(target, Config.Item("NFE").GetValue<bool>());
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("QRange").GetValue<Circle>().Active)
            {
                Utility.DrawCircle(_player.Position, Q.Range, Config.Item("QRange").GetValue<Circle>().Color);
            }
            if (Config.Item("ERange").GetValue<Circle>().Active)
            {
                Utility.DrawCircle(_player.Position, E.Range, Config.Item("ERange").GetValue<Circle>().Color);
            }
        }
    }
}
