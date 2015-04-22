using System;
using System.Linq;
using System.Windows.Forms;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using Menu = LeagueSharp.Common.Menu;
using MenuItem = LeagueSharp.Common.MenuItem;


namespace HydraAIO
{
    internal class Program
    {
        public static Menu Config;
        public static Menu QuickSilverMenu;
        public static Champion CClass;
        public static Activator AActivator;
        public static double ActivatorTime;
        public static int QssTime;
        public static Random Rnd = new Random();

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Config = new Menu("Hydra AIO", "Hydra AIO", true);
            CClass = new Champion();
            AActivator = new Activator();
            
            var baseType = CClass.GetType();

            var championName = ObjectManager.Player.ChampionName.ToLowerInvariant();

            switch (championName)
            {
             
                case "kennen":
                    CClass = new Kennen();
                    break;
                case "tristana":
                    CClass = new Tristana();
                    break;
            }


            CClass.Id = ObjectManager.Player.BaseSkinName;
            CClass.Config = Config;

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            var orbwalking = Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            if (CClass.Orbwalk)
            {
                CClass.Orbwalker = new Orbwalking.Orbwalker(orbwalking);
            }

            /* Menu Summoners */
            var summoners = Config.AddSubMenu(new Menu("Summoners", "Summoners"));
            var summonersHeal = summoners.AddSubMenu(new Menu("Heal", "Heal"));
            {
                summonersHeal.AddItem(new MenuItem("SUMHEALENABLE", "Enable").SetValue(true));
                summonersHeal.AddItem(new MenuItem("SUMHEALSLIDER", "Min. Heal Per.").SetValue(new Slider(20, 99, 1)));
            }

            var summonersBarrier = summoners.AddSubMenu(new Menu("Barrier", "Barrier"));
            {
                summonersBarrier.AddItem(new MenuItem("SUMBARRIERENABLE", "Enable").SetValue(true));
                summonersBarrier.AddItem(
                    new MenuItem("SUMBARRIERSLIDER", "Min. Heal Per.").SetValue(new Slider(20, 99, 1)));
            }

            var summonersIgnite = summoners.AddSubMenu(new Menu("Ignite", "Ignite"));
            {
                summonersIgnite.AddItem(new MenuItem("SUMIGNITEENABLE", "Enable").SetValue(true));
            }
            /* Menu Items */            
            var items = Config.AddSubMenu(new Menu("Items", "Items"));
            items.AddItem(new MenuItem("BOTRK", "BOTRK").SetValue(true));
            items.AddItem(new MenuItem("GHOSTBLADE", "Ghostblade").SetValue(true));
            items.AddItem(new MenuItem("SWORD", "Sword of the Divine").SetValue(true));
            items.AddItem(new MenuItem("MURAMANA", "Muramana").SetValue(true));
            QuickSilverMenu = new Menu("QSS", "QuickSilverSash");
            items.AddSubMenu(QuickSilverMenu);
            QuickSilverMenu.AddItem(new MenuItem("AnyStun", "Any Stun").SetValue(true));
            QuickSilverMenu.AddItem(new MenuItem("AnySlow", "Any Slow").SetValue(true));
            QuickSilverMenu.AddItem(new MenuItem("AnySnare", "Any Snare").SetValue(true));
            QuickSilverMenu.AddItem(new MenuItem("AnyTaunt", "Any Taunt").SetValue(true));
            foreach (var t in AActivator.BuffList)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                {
                    if (t.ChampionName == enemy.ChampionName)
                        QuickSilverMenu.AddItem(new MenuItem(t.BuffName, t.DisplayName).SetValue(t.DefaultValue));
                }
            }

            // If Champion is supported draw the extra menus
            if (baseType != CClass.GetType())
            {
                var combo = new Menu("Combo", "Combo");
                if (CClass.ComboMenu(combo))
                {
                    Config.AddSubMenu(combo);
                }

                var harass = new Menu("Harass", "Harass");
                if (CClass.HarassMenu(harass))
                {
                    harass.AddItem(new MenuItem("HarassMana", "Min. Mana Percent").SetValue(new Slider(50, 100, 0)));
                    Config.AddSubMenu(harass);
                }

                var laneclear = new Menu("LaneClear", "LaneClear");
                if (CClass.LaneClearMenu(laneclear))
                {
                    laneclear.AddItem(
                        new MenuItem("LaneClearMana", "Min. Mana Percent").SetValue(new Slider(50, 100, 0)));
                    Config.AddSubMenu(laneclear);
                }

                var misc = new Menu("Misc", "Misc");
                if (CClass.MiscMenu(misc))
                {
                    Config.AddSubMenu(misc);
                }
                
                var extras = new Menu("Extras", "Extras");
                if (CClass.ExtrasMenu(extras))
                {
                    new PotionManager(extras);
                    extras.AddItem(
                        new MenuItem("atkmove", "Custom Attack Move").SetValue(true));
                    Config.AddSubMenu(extras);
                }

                var drawing = new Menu("Drawings", "Drawings");
                if (CClass.DrawingMenu(drawing))
                {
                    Config.AddSubMenu(drawing);
                }

            }

            CClass.MainMenu(Config);

            Config.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Game.OnWndProc += GameOnOnWndProc;
            Orbwalking.AfterAttack += OrbwalkingAfterAttack;
            Orbwalking.OnAttack += OrbwalkingOnAttack;
            Orbwalking.BeforeAttack += OrbwalkingBeforeAttack;
            
        }

        private static void GameOnOnWndProc(WndEventArgs args)
        {
            if (!Config.Item("atkmove").GetValue<bool>())
                return;
            if (args.Msg != (uint) WindowsMessages.WM_LBUTTONDOWN || Control.ModifierKeys != Keys.Alt)
            {
                return;
            }
            var target = ObjectManager.Get<Obj_AI_Base>()
                .Where(enemy => enemy.IsValidTarget() && enemy.Distance(Game.CursorPos) < 400)
                .OrderBy(h => h.Distance(Game.CursorPos, true))
                .FirstOrDefault();
            if (target != null && target.Type != GameObjectType.obj_AI_Hero)
            {
                target = TargetSelector.GetTarget(
                    target.Distance(Game.CursorPos) + 20, TargetSelector.DamageType.Physical, false, null,
                    Game.CursorPos) ?? target;
            }
            if (target != null)
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
            else
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.AttackTo, Game.CursorPos);
                VirtualMouse.ShiftClick(Game.CursorPos);
                ObjectManager.Player.IssueOrder(GameObjectOrder.AttackTo, Game.CursorPos);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {            
            if (CClass != null)
            {
                CClass.Drawing_OnDraw(args);
            }
            //Render.Circle.DrawCircle(ObjectManager.Player.Position, 1000, Color.Orange);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (CClass.Orbwalker.GetTarget() == null || !Orbwalking.InAutoAttackRange(CClass.Orbwalker.GetTarget()))
                CClass.AttackNow = true;

            CClass.AttackReadiness = 1 - (Math.Max(0,
                (Orbwalking.LastAATick + ObjectManager.Player.AttackDelay * 1000 - LeagueSharp.Common.Utils.TickCount +
                     (float)Game.Ping / 2 + 25)) / (ObjectManager.Player.AttackDelay * 1000 + (float)Game.Ping / 2 + 25));

            if (ObjectManager.Player.IsDead)
            {
                QssTime = 0;
                return;
            }

            CheckChampionBuff();
            if (QssTime != 0 && LeagueSharp.Common.Utils.TickCount > QssTime)
            {
                if (Items.HasItem(ItemData.Quicksilver_Sash.Id)) Items.UseItem(ItemData.Quicksilver_Sash.Id);
                if (Items.HasItem(ItemData.Mercurial_Scimitar.Id)) Items.UseItem(ItemData.Mercurial_Scimitar.Id);
                if (Items.HasItem(ItemData.Dervish_Blade.Id)) Items.UseItem(ItemData.Dervish_Blade.Id);
                QssTime = 0;
            }
            
            //Update the combo and harass values.
            if (CClass.Orbwalk)
            {
                CClass.ComboActive = CClass.Config.Item("Orbwalk").GetValue<KeyBind>().Active;

                var vHarassManaPer = Config.Item("HarassMana").GetValue<Slider>().Value;
                CClass.HarassActive = CClass.Config.Item("Farm").GetValue<KeyBind>().Active &&
                                      (!CClass.UsesMana || ObjectManager.Player.ManaPercentage() >= vHarassManaPer);

                CClass.ToggleActive = !CClass.UsesMana || ObjectManager.Player.ManaPercentage() >= vHarassManaPer;

                var vLaneClearManaPer = Config.Item("LaneClearMana").GetValue<Slider>().Value;
                CClass.LaneClearActive = CClass.Config.Item("LaneClear").GetValue<KeyBind>().Active &
                                         (!CClass.UsesMana || ObjectManager.Player.ManaPercentage() >= vLaneClearManaPer);
            }

            CClass.Game_OnGameUpdate(args);
            
            UseSummoners();

            //Items
            if (((!CClass.ComboActive)))
            {
                return;
            }
            var botrk = Config.Item("BOTRK").GetValue<bool>();
            var ghostblade = Config.Item("GHOSTBLADE").GetValue<bool>();
            var target = CClass.Orbwalker.GetTarget() as Obj_AI_Hero;

            if (botrk)
            {
                if (target != null && target.IsValidTarget() &&
                    target.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 450)
                {
                    var hasCutGlass = Items.HasItem(ItemData.Bilgewater_Cutlass.Id);
                    var hasBotrk = Items.HasItem(ItemData.Blade_of_the_Ruined_King.Id);

                    if (hasBotrk || hasCutGlass)
                    {
                        var itemId = hasCutGlass ? ItemData.Bilgewater_Cutlass.Id : ItemData.Blade_of_the_Ruined_King.Id;
                        if (ObjectManager.Player.HealthPercentage() < 60 || target.HealthPercentage() < 70)
                            Items.UseItem(itemId, target);
                    }
                }
                if (target != null && target.IsValidTarget() &&
                    target.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 650)
                {
                    var hasBotrk = Items.HasItem(ItemData.Hextech_Gunblade.Id);
                    if (hasBotrk)
                    {
                        Items.UseItem(ItemData.Hextech_Gunblade.Id, target);
                    }
                }
            
            }

            if (ghostblade && target.IsValidTarget() &&
                Orbwalking.InAutoAttackRange(target) && (ObjectManager.Player.HealthPercentage() < 60 || target.HealthPercentage() < 70))
                Items.UseItem(ItemData.Youmuus_Ghostblade.Id);

        }
        
        public static void UseSummoners()
        {
            if (ObjectManager.Player.IsDead)
                return;
                
            const int xDangerousRange = 1100;

            if (Config.Item("SUMHEALENABLE").GetValue<bool>())
            {
                var xSlot = ObjectManager.Player.GetSpellSlot("summonerheal");
                var xCanUse = ObjectManager.Player.Health <=
                              ObjectManager.Player.MaxHealth/100*Config.Item("SUMHEALSLIDER").GetValue<Slider>().Value;

                if (xCanUse && !ObjectManager.Player.InShop() && 
                    (xSlot != SpellSlot.Unknown || ObjectManager.Player.Spellbook.CanUseSpell(xSlot) == SpellState.Ready) 
                    && ObjectManager.Player.CountEnemiesInRange(xDangerousRange) > 0) 
                {
                    ObjectManager.Player.Spellbook.CastSpell(xSlot);
                }
            }
            
            if (Config.Item("SUMBARRIERENABLE").GetValue<bool>())
            {
                var xSlot = ObjectManager.Player.GetSpellSlot("summonerbarrier");
                var xCanUse = ObjectManager.Player.Health <=
                              ObjectManager.Player.MaxHealth/100*Config.Item("SUMBARRIERSLIDER").GetValue<Slider>().Value;

                if (xCanUse && !ObjectManager.Player.InShop() && 
                    (xSlot != SpellSlot.Unknown || ObjectManager.Player.Spellbook.CanUseSpell(xSlot) == SpellState.Ready) 
                    && ObjectManager.Player.CountEnemiesInRange(xDangerousRange) > 0) 
                {
                    ObjectManager.Player.Spellbook.CastSpell(xSlot);
                }
            }
            
            if (Config.Item("SUMIGNITEENABLE").GetValue<bool>())
            {
                var xSlot = ObjectManager.Player.GetSpellSlot("summonerdot");
                var t = CClass.Orbwalker.GetTarget() as Obj_AI_Hero;
                
                if (t != null && xSlot != SpellSlot.Unknown &&
                    ObjectManager.Player.Spellbook.CanUseSpell(xSlot) == SpellState.Ready)
                {
                    if (ObjectManager.Player.Distance(t) < 650 &&
                        ObjectManager.Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite) >=
                        t.Health)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(xSlot, t);
                    }
                }
            }
        }
        private static void OrbwalkingAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
                CClass.AttackNow = true;
            CClass.Orbwalking_AfterAttack(unit, target);
        }

        private static void OrbwalkingOnAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (Config.Item("MURAMANA").GetValue<bool>() && (Items.HasItem(ItemData.Muramana.Id) || Items.HasItem(ItemData.Muramana2.Id)))
            {
                if (target != null &&
                    ((target.Type == GameObjectType.obj_AI_Hero && !ObjectManager.Player.HasBuff("Muramana", true)) ||
                     (target.Type != GameObjectType.obj_AI_Hero && ObjectManager.Player.HasBuff("Muramana", true))))
                {
                    Items.UseItem(ItemData.Muramana.Id);
                    Items.UseItem(ItemData.Muramana2.Id);
                }
            }

            CClass.Orbwalking_OnAttack(unit, target);

        }

        private static void OrbwalkingBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            CClass.AttackNow = false;
            CClass.Orbwalking_BeforeAttack(args);
        }

        private static void CheckChampionBuff()
        {
            foreach (var t1 in ObjectManager.Player.Buffs)
            {
                foreach (var t in QuickSilverMenu.Items)
                {
                    if (QuickSilverMenu.Item(t.Name).GetValue<bool>())
                    {
                        if (t1.Name.ToLower().Contains(t.Name.ToLower()))
                        {
                            var t2 = t1;
                            foreach (var bx in AActivator.BuffList.Where(bx => bx.BuffName == t2.Name))
                            {
                                if (bx.Delay > 0)
                                {
                                    if (ActivatorTime + bx.Delay < (int) Game.Time)
                                        ActivatorTime = (int) Game.Time;

                                    if (ActivatorTime + bx.Delay <= (int) Game.Time)
                                    {
                                        if (QssTime == 0)
                                            QssTime = LeagueSharp.Common.Utils.TickCount + Rnd.Next(50, 150);
                                        ActivatorTime = (int) Game.Time;
                                    }
                                }
                                else
                                {
                                    if (QssTime == 0)
                                        QssTime = LeagueSharp.Common.Utils.TickCount + Rnd.Next(50, 150);
                                }

                            }
                        }
                    }
                    if (QuickSilverMenu.Item("AnySlow").GetValue<bool>() &&
                        ObjectManager.Player.HasBuffOfType(BuffType.Slow) && QssTime == 0)
                    {
                        QssTime = LeagueSharp.Common.Utils.TickCount + Rnd.Next(50, 150);
                    }
                    if (QuickSilverMenu.Item("AnySnare").GetValue<bool>() &&
                        ObjectManager.Player.HasBuffOfType(BuffType.Snare) && QssTime == 0)
                    {
                        QssTime = LeagueSharp.Common.Utils.TickCount + Rnd.Next(100, 150);
                    }
                    if (QuickSilverMenu.Item("AnyStun").GetValue<bool>() &&
                        ObjectManager.Player.HasBuffOfType(BuffType.Stun) && QssTime == 0)
                    {
                        QssTime = LeagueSharp.Common.Utils.TickCount + Rnd.Next(100, 150);
                    }
                    if (QuickSilverMenu.Item("AnyTaunt").GetValue<bool>() &&
                        ObjectManager.Player.HasBuffOfType(BuffType.Taunt) && QssTime == 0)
                    {
                        QssTime = LeagueSharp.Common.Utils.TickCount + Rnd.Next(50, 150);
                    }
                }
            }           
        }
    }
}
