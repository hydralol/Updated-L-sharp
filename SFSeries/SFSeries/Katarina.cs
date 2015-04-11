/* 
 * SFKatarina
    ________________________  __.       __               .__               
   /   _____/\_   _____/    |/ _|____ _/  |______ _______|__| ____ _____   
   \_____  \  |    __) |      < \__  \\   __\__  \\_  __ \  |/    \\__  \  
   /        \ |     \  |    |  \ / __ \|  |  / __ \|  | \/  |   |  \/ __ \_
  /_______  / \___  /  |____|__ (____  /__| (____  /__|  |__|___|  (____  /
          \/      \/           \/    \/          \/              \/     \/ 
 * 
 * Features:
 * Perfect Combo
 * Pentakill functionality (Ult canceling)
 * Spell farm
 * Easily customizable
 * 
 * Credits:
 * Snorflake - Making it
 * Fluxy - Re-writing ward jump & Teaching me about vectors and movement packets
 * */




#region References

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LX_Orbwalker;
using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;

// By iSnorflake
namespace SFSeries
{
    internal class Katarina
    {

#endregion

        #region Declares

        //Spells
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Items.Item Dfg;
        public static bool IsChanneling;
        public static int Count;
        //Menu
        public static Menu Config;
        private static Obj_AI_Hero _player;


        // ReSharper disable once UnusedParameter.Local
        public Katarina()
        {
            Game_OnGameLoad();
        }
        #endregion

        #region OnGameLoad
        static void Game_OnGameLoad()
        {
            _player = ObjectManager.Player;
            Q = new Spell(SpellSlot.Q, 675);
            W = new Spell(SpellSlot.W, 375);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 550);

            Dfg = new Items.Item(3128, 750f);
            Program.PrintMessage("Loaded!");
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
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
            Config.SubMenu("Combo").AddItem(new MenuItem("ProcQ", "Proc Q").SetValue(false));

            Config.AddSubMenu(new Menu("Farm", "Farm")); // creds tc-crew
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseQFarm", "Use Q").SetValue(
                        true));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseWFarm", "Use W").SetValue(
                        true));
            var waveclear = new Menu("Waveclear", "WaveclearMenu");
            waveclear.AddItem(new MenuItem("useQW", "Use Q?").SetValue(true));
            waveclear.AddItem(new MenuItem("useWW", "Use W?").SetValue(true));
            Config.AddSubMenu(waveclear); // Thanks to ChewyMoon for the idea of doing the menu this way

            // Misc
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("smartKS", "SMART KS").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("wardKs", "WARD JUMP KS").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("Escape", "Escape").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Misc").AddItem(new MenuItem("UltCancel", "Ult Cancel(EXPERIMENTAL)").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("AlwaysW", "Auto W").SetValue(true));

            // Drawings
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(new Circle(true, Color.FromArgb(150, Color.OrangeRed))));
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after Combo").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit += hero => (float)CalculateDamageDrawing(hero);

            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };
            Config.SubMenu("Drawings").AddItem(dmgAfterComboItem);
            Config.AddSubMenu(new Menu("Exploits", "Exploits"));
            Config.SubMenu("Exploits").AddItem(new MenuItem("QNFE", "Q No-Face").SetValue(true));
            // Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            
            //SkinManager.AddToMenu(ref Config);
           
            Config.AddToMainMenu();
            //Add the events we are going to use
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            LXOrbwalker.BeforeAttack += LXOrbwalker_BeforeAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnGameSendPacket += Game_OnGameSendPacket;

        }

        static void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] != 0x72 || !_player.HasBuff("katarinarsound",true) || Count >= 2) return;
            Count += 1;
            args.Process = false;
        }

        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || args.SData.Name != "KatarinaR") return;
            IsChanneling = true;
            LXOrbwalker.SetMovement(false);

            LXOrbwalker.SetAttack(false);
            Utility.DelayAction.Add(1, () => IsChanneling = false);
        }
        #endregion

        #region BeforeAttack
        static void LXOrbwalker_BeforeAttack(LXOrbwalker.BeforeAttackEventArgs beforeAttackEventArgs)
        {
            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (LXOrbwalker.CurrentMode != LXOrbwalker.Mode.Combo) return;
            if (!Config.Item("ProcQ").GetValue<bool>()) return;
            Q.CastOnUnit(target, Config.Item("QNFE").GetValue<bool>());
        }
        #endregion

        #region OnGameUpdate
        private static void Game_OnGameUpdate(EventArgs args)
        {
            //SkinManager.Update();
            
            if (_player.IsDead) return;
            if (Config.Item("UltCancel").GetValue<bool>())
            {
                if (_player.HasBuff("katarinarsound", true) && Utility.CountEnemysInRange((int) R.Range) == 0)
                    IssueMoveComand();
            }
            if (_player.HasBuff("katarinarsound",true))
            {
                LXOrbwalker.SetMovement(false);
                LXOrbwalker.SetAttack(false);
            }
            if (!_player.HasBuff("katarinarsound",true) && !IsChanneling)
            {
                Count = 0;
                LXOrbwalker.SetMovement(true);
                LXOrbwalker.SetAttack(true);
            }
            if(IsChanneling) return;
            var useWa = Config.Item("AlwaysW").GetValue<bool>() && W.IsReady();
            switch (LXOrbwalker.CurrentMode)
            {
                case LXOrbwalker.Mode.Combo:
                    Combo();
                    break;
                case LXOrbwalker.Mode.Lasthit:
                    Farm();
                    break;
                case LXOrbwalker.Mode.Harass:
                    Harras();
                    break;
                case LXOrbwalker.Mode.LaneClear:
                    WaveClear();
                    break;
            }
            Escape();
            if(useWa)
            AlwaysW();
            SmartKs();

        }

        private static void IssueMoveComand()
        {
            _player.IssueOrder(GameObjectOrder.MoveTo, _player.Position);
        }

        private static void AlwaysW()
        {
// ReSharper disable once UnusedVariable
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(W.Range)).Where(hero => W.IsReady()))
            {
                W.Cast();
            }
        }

        #endregion

        #region Farm
        private static void Farm() // Credits TC-CREW
        {
            if (!Orbwalking.CanMove(40)) return;
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var useQ = Config.Item("UseQFarm").GetValue<bool>();
            var useW = Config.Item("UseWFarm").GetValue<bool>();
            if (useQ && Q.IsReady())
            {
                foreach (var minion in from minion in allMinions where minion.Distance(_player) < Q.Range let predictedHealth = HealthPrediction.GetHealthPrediction(minion, (int)(Q.Delay + (minion.Distance(_player) / Q.Speed)) * 1000) where predictedHealth > 0f && predictedHealth < 0.75 * Q.GetDamage(minion) select minion)
                {

                    Q.CastOnUnit(minion);
                }
            }
            else if (useW && W.IsReady())
            {
                if (!allMinions.Any(minion => minion.IsValidTarget(W.Range) && minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.W))) return;

                W.Cast();
            }
        }
        #endregion

        #region WaveClear
        public static void WaveClear()
        {
            if (!Orbwalking.CanMove(40)) return;

            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var useQ = Config.Item("useQW").GetValue<bool>();
            var useW = Config.Item("useWW").GetValue<bool>();
            if (useQ && Q.IsReady())
            {
                foreach (var minion in allMinions.Where(minion => minion.IsValidTarget(Q.Range)))
                {
                    Q.CastOnUnit(minion, Config.Item("QNFE").GetValue<bool>());
                    return;
                }
            }
            else if (useW && W.IsReady())
            {
                if (!allMinions.Any(minion => minion.IsValidTarget(W.Range))) return;
                W.Cast();
            }
        }
        #endregion

        #region Combo
        private static void Combo()
        {
            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;


            if (!_player.IsChannelingImportantSpell())
            {
                if (Dfg != null)
                {
                
                if (Dfg.IsReady())
                    Dfg.Cast(target);
                }
            if (Q.IsReady() && _player.Distance(target) < Q.Range + target.BoundingRadius && !Config.Item("ProcQ").GetValue<bool>())
                        Q.CastOnUnit(target, Config.Item("QNFE").GetValue<bool>());
                    if (E.IsReady() && _player.Distance(target) < E.Range + target.BoundingRadius)
                        E.CastOnUnit(target, Config.Item("QNFE").GetValue<bool>());
                    if (W.IsReady() && _player.Distance(target) < W.Range)
                        W.Cast();
                }
                if (!Q.IsReady() && R.IsReady() && _player.Distance(target) < (R.Range - 200))
                R.Cast();
                
            

        }
        #endregion

        private static void Harras()
        {
            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;
            if (ObjectManager.Player.Distance(target) < Q.Range && Q.IsReady())
                Q.CastOnUnit(target, true);

            /*if (ObjectManager.Player.Distance(target) < E.Range && E.IsReady())
                E.CastOnUnit(target);*/

            if (ObjectManager.Player.Distance(target) < W.Range && W.IsReady())
                W.Cast();
            
        }
        #region OnDraw
        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(_player.Position, spell.Range, menuItem.Color);
                // Drawing.DrawText(playerPos[0] - 65, playerPos[1] + 20, drawUlt.Color, "Hit R To kill " + UltTarget + "!");

            }
            //Drawing tempoarily disabled
        }
        #endregion

        #region Killsteal

        #endregion

        #region Escape
        private static void Escape() // HUGE CREDITS TO FLUXY FOR FIXING THIS
        {
            var basePos = _player.Position.To2D();
            var newPos = (Game.CursorPos.To2D() - _player.Position.To2D());
            var finalVector = basePos + (newPos.Normalized() * (560));
            var movePos = basePos + (newPos.Normalized() * (100));
            if (!Config.Item("Escape").GetValue<KeyBind>().Active) return;
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, movePos.To3D());
            if (!E.IsReady()) return;
            var castWard = true;
            foreach (var esc in ObjectManager.Get<Obj_AI_Base>().Where(esc => esc.Distance(ObjectManager.Player) <= E.Range))
            {
                if (Vector2.Distance(Game.CursorPos.To2D(), esc.ServerPosition.To2D()) <= 175)
                {
                    E.Cast(esc, true);
                    castWard = false;
                }
                if (!esc.Name.Contains("Ward") || !(Vector2.Distance(finalVector, esc.ServerPosition.To2D()) <= 175))
                    continue;
                E.Cast(esc, true);
                castWard = false;
            }
            var ward = FindBestWardItem();
            if (ward != null && castWard)
            {
                ward.UseItem(finalVector.To3D());
            }
        }
        #endregion

        #region Ward jump stuff

        private static SpellDataInst GetItemSpell(InventorySlot invSlot)
        {
            return ObjectManager.Player.Spellbook.Spells.FirstOrDefault(spell => (int)spell.Slot == invSlot.Slot + 4);
        }
        private static InventorySlot FindBestWardItem()
        {
            var slot = Items.GetWardSlot();
            if (slot == default(InventorySlot)) return null;

            var sdi = GetItemSpell(slot);

            if (sdi != default(SpellDataInst) && sdi.State == SpellState.Ready)
            {
                return slot;
            }
            return null;
        }
        #endregion

        #region GetDamage

        public static double CalculateDamageDrawing(Obj_AI_Base target)
        {
            double totaldamage = 0;
            if (Q.IsReady())
            {
                totaldamage += _player.GetSpellDamage(target, SpellSlot.Q, 2);
            }
            if (E.IsReady() && W.IsReady())
            {
                totaldamage += _player.GetSpellDamage(target, SpellSlot.W);
            }
            if (E.IsReady())
            {
                totaldamage += _player.GetSpellDamage(target, SpellSlot.E);
            }
            if (R.IsReady())
            {
                totaldamage += _player.GetSpellDamage(target, SpellSlot.R) * 8;
            }
            if (!Q.IsReady())
            {
                totaldamage += _player.CalcDamage(target, Damage.DamageType.Magical, (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level * 15) + (0.15 * ObjectManager.Player.FlatMagicDamageMod));
            }
            if (Dfg != null)
            return (Dfg.IsReady() ? totaldamage * 1.2f : totaldamage * 1f);
            return totaldamage;
        }
        protected static int CountEnemiesNearPosition(Vector3 pos, float range)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>().Count(
                    hero => hero.IsEnemy && !hero.IsDead && hero.IsValid && hero.Distance(pos) <= range);
        }
        /*
         * HUGE CREDITS TO xSalice I STOLE THIS FROM HIM AND ITS INCREDIBLE
         * THIS IS ALL HIS WORK NAD YOU SHOULD CREDIT HIM FOR THIS
         * 
         * 
         * 
         */
        public static void SmartKs()
        {
            if (!Config.Item("smartKS").GetValue<bool>())
                return;

            var nearChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where _player.Distance(champ.ServerPosition) <= 1375 && champ.IsEnemy select champ).ToList();
// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            nearChamps.OrderBy(x => x.Health);
            var packets = Config.Item("QNFE").GetValue<bool>();
            foreach (var target in nearChamps.Where(target => target != null && !target.IsDead))
            {
                //dfg
                if (Dfg.IsReady() && _player.GetItemDamage(target, Damage.DamageItems.Dfg) > target.Health + 20 && _player.Distance(target.ServerPosition) <= 750)
                {
                    Dfg.Cast(target);
                    //Game.PrintChat("ks 1");
                    return;
                }

                //dfg + q
                if (_player.Distance(target.ServerPosition) <= Q.Range &&
                    (_player.GetItemDamage(target, Damage.DamageItems.Dfg) + (_player.GetSpellDamage(target, SpellSlot.Q)) * 1.2) > target.Health + 20)
                {
                    if (Dfg.IsReady() && Q.IsReady())
                    {
                        Dfg.Cast(target);

                        Q.Cast(target, packets);
                        //Game.PrintChat("ks 2");
                        return;
                    }
                }

                //dfg + q
                if (_player.Distance(target.ServerPosition) <= E.Range &&
                    (_player.GetItemDamage(target, Damage.DamageItems.Dfg) + (_player.GetSpellDamage(target, SpellSlot.E)) * 1.2) > target.Health + 20)
                {
                    if (Dfg.IsReady() && Q.IsReady())
                    {
                        Dfg.Cast(target);

                        E.Cast(target, packets);
                        //Game.PrintChat("ks 3");
                        return;
                    }
                }

                //E + W
                if (_player.Distance(target.ServerPosition) <= E.Range && (_player.GetSpellDamage(target, SpellSlot.E) + _player.GetSpellDamage(target, SpellSlot.W)) > target.Health + 20)
                {
                    if (E.IsReady() && W.IsReady())
                    {

                        E.Cast(target, packets);
                        W.Cast();
                        //Game.PrintChat("ks 5");
                        return;
                    }
                }

                //E + Q
                if (_player.Distance(target.ServerPosition) <= E.Range && (_player.GetSpellDamage(target, SpellSlot.E) + _player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 20)
                {
                    if (E.IsReady() && Q.IsReady())
                    {

                        E.Cast(target, packets);
                        Q.Cast(target, packets);
                        //Game.PrintChat("ks 6");
                        return;
                    }
                }

                //Q
                if ((_player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 20)
                {
                    if (Q.IsReady() && _player.Distance(target.ServerPosition) <= Q.Range)
                    {

                        Q.Cast(target, packets);
                        //Game.PrintChat("ks 7");
                        return;
                    }
                    if (Q.IsReady() && E.IsReady() && _player.Distance(target.ServerPosition) <= 1375 && Config.Item("wardKs").GetValue<bool>() && CountEnemiesNearPosition(target.ServerPosition, 500) < 3)
                    {

                        JumpKs(target);
                        //Game.PrintChat("wardKS!!!!!");
                        return;
                    }
                }

                //E
                if (_player.Distance(target.ServerPosition) <= E.Range && (_player.GetSpellDamage(target, SpellSlot.E)) > target.Health + 20)
                {
                    if (E.IsReady())
                    {

                        E.Cast(target, packets);
                        //Game.PrintChat("ks 8");
                        return;
                    }
                }

            }
        }
        public static void JumpKs(Obj_AI_Hero target)
        {

            var packets = Config.Item("QNFE").GetValue<bool>();
            foreach (Obj_AI_Minion ward in ObjectManager.Get<Obj_AI_Minion>().Where(ward =>
                E.IsReady() && Q.IsReady() && ward.Name.ToLower().Contains("ward") && ward.Distance(target.ServerPosition) < Q.Range && ward.Distance(_player) < E.Range))
            {
                E.Cast(ward);
                return;
            }

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero =>
                E.IsReady() && Q.IsReady() && hero.Distance(target.ServerPosition) < Q.Range && hero.Distance(_player) < E.Range))
            {
                E.Cast(hero);
                return;
            }

            foreach (Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion =>
                E.IsReady() && Q.IsReady() && minion.Distance(target.ServerPosition) < Q.Range && minion.Distance(_player) < E.Range))
            {
                E.Cast(minion);
                return;
            }

            if (_player.Distance(target) < Q.Range)
            {
                Q.Cast(target, packets);
                return;
            }

            if (E.IsReady() && Q.IsReady())
            {
                Vector3 position = _player.ServerPosition + Vector3.Normalize(target.ServerPosition - _player.ServerPosition) * 590;

                if (target.Distance(position) < Q.Range)
                {
                    InventorySlot invSlot = FindBestWardItem();
                    if (invSlot == null) return;

                    invSlot.UseItem(position);
                }
            }

            if (_player.Distance(target) < Q.Range)
            {
                Q.Cast(target, packets);
            }

        }

    }
}
        #endregion