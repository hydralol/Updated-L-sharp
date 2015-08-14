using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace OneKeyToWin_AIO_Sebby
{
    class Activator
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public int Muramana = 3042;
        public int Manamune = 3004;

        public static Items.Item

            //Cleans
            Mikaels = new Items.Item(3222, 600f),
            Quicksilver = new Items.Item(3140, 0),
            Mercurial = new Items.Item(3139, 0),
            Dervish = new Items.Item(3137, 0),
            //REGEN
            Potion = new Items.Item(2003, 0),
            ManaPotion = new Items.Item(2004, 0),
            Flask = new Items.Item(2041, 0),
            Biscuit = new Items.Item(2010, 0),
            //attack
            Botrk = new Items.Item(3153, 550f),
            Cutlass = new Items.Item(3144, 550f),
            Youmuus = new Items.Item(3142, 650f),
            Hydra = new Items.Item(3074, 440f),
            Hydra2 = new Items.Item(3077, 440f),
            Hextech = new Items.Item(3146, 700f),
            //def
            Randuin = new Items.Item(3143, 500f);
        
        public void LoadOKTW()
        {
            Config.SubMenu("Activator OKTW©").AddItem(new MenuItem("pots", "Potion, ManaPotion, Flask, Biscuit").SetValue(true));

            Config.SubMenu("Activator OKTW©").SubMenu("Offensives").SubMenu("Botrk").AddItem(new MenuItem("Botrk", "Botrk").SetValue(true));
            Config.SubMenu("Activator OKTW©").SubMenu("Offensives").SubMenu("Botrk").AddItem(new MenuItem("BotrkKS", "Botrk KS").SetValue(true));
            Config.SubMenu("Activator OKTW©").SubMenu("Offensives").SubMenu("Botrk").AddItem(new MenuItem("BotrkLS", "Botrk LifeSaver").SetValue(true));
            Config.SubMenu("Activator OKTW©").SubMenu("Offensives").SubMenu("Botrk").AddItem(new MenuItem("BotrkCombo", "Botrk always in combo").SetValue(false));

            Config.SubMenu("Activator OKTW©").SubMenu("Offensives").SubMenu("Cutlass").AddItem(new MenuItem("Cutlass", "Cutlass").SetValue(true));
            Config.SubMenu("Activator OKTW©").SubMenu("Offensives").SubMenu("Cutlass").AddItem(new MenuItem("CutlassKS", "Cutlass KS").SetValue(true));
            Config.SubMenu("Activator OKTW©").SubMenu("Offensives").SubMenu("Cutlass").AddItem(new MenuItem("CutlassCombo", "Cutlass always in combo").SetValue(true));

            Config.SubMenu("Activator OKTW©").SubMenu("Offensives").SubMenu("Cutlass").AddItem(new MenuItem("Hextech", "Hextech").SetValue(true));
            Config.SubMenu("Activator OKTW©").SubMenu("Offensives").SubMenu("Cutlass").AddItem(new MenuItem("HextechKS", "Hextech KS").SetValue(true));
            Config.SubMenu("Activator OKTW©").SubMenu("Offensives").SubMenu("Cutlass").AddItem(new MenuItem("HextechCombo", "Hextech always in combo").SetValue(true));

            Config.SubMenu("Activator OKTW©").SubMenu("Offensives").SubMenu("Youmuus").AddItem(new MenuItem("Youmuus", "Youmuus").SetValue(true));
            Config.SubMenu("Activator OKTW©").SubMenu("Offensives").SubMenu("Youmuus").AddItem(new MenuItem("YoumuusR", "LucianR, TwitchR, AsheQ").SetValue(true));
            Config.SubMenu("Activator OKTW©").SubMenu("Offensives").SubMenu("Youmuus").AddItem(new MenuItem("YoumuusKS", "Youmuus KS").SetValue(true));
            Config.SubMenu("Activator OKTW©").SubMenu("Offensives").SubMenu("Youmuus").AddItem(new MenuItem("YoumuusCombo", "Youmuus always in combo").SetValue(false));

            Config.SubMenu("Activator OKTW©").SubMenu("Offensives").SubMenu("Hydra").AddItem(new MenuItem("Hydra", "Hydra").SetValue(true));

            Config.SubMenu("Activator OKTW©").SubMenu("Offensives").SubMenu("Muramana").AddItem(new MenuItem("Muramana", "Muramana").SetValue(true));

            // DEF
            Config.SubMenu("Activator OKTW©").SubMenu("Defensives").AddItem(new MenuItem("Randuin", "Randuin").SetValue(true));

            // CLEANSERS 
            Config.SubMenu("Activator OKTW©").SubMenu("Cleansers").AddItem(new MenuItem("Clean", "Quicksilver, Mikaels, Mercurial, Dervish").SetValue(true));
            Config.SubMenu("Activator OKTW©").SubMenu("Cleansers").AddItem(new MenuItem("cleanHP", "Use only under % HP").SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("Activator OKTW©").SubMenu("Cleansers").SubMenu("Buff type").AddItem(new MenuItem("CleanSpells", "ZedR FizzR MordekaiserR PoppyR VladimirR").SetValue(true));
            Config.SubMenu("Activator OKTW©").SubMenu("Cleansers").SubMenu("Buff type").AddItem(new MenuItem("Stun", "Stun").SetValue(true));
            Config.SubMenu("Activator OKTW©").SubMenu("Cleansers").SubMenu("Buff type").AddItem(new MenuItem("Snare", "Snare").SetValue(true));
            Config.SubMenu("Activator OKTW©").SubMenu("Cleansers").SubMenu("Buff type").AddItem(new MenuItem("Knockup", "Knockup").SetValue(true));
            Config.SubMenu("Activator OKTW©").SubMenu("Cleansers").SubMenu("Buff type").AddItem(new MenuItem("Knockback", "Knockback").SetValue(true));
            Config.SubMenu("Activator OKTW©").SubMenu("Cleansers").SubMenu("Buff type").AddItem(new MenuItem("Charm", "Charm").SetValue(true));
            Config.SubMenu("Activator OKTW©").SubMenu("Cleansers").SubMenu("Buff type").AddItem(new MenuItem("Fear", "Fear").SetValue(true));
            Config.SubMenu("Activator OKTW©").SubMenu("Cleansers").SubMenu("Buff type").AddItem(new MenuItem("Suppression", "Suppression").SetValue(true));
            Config.SubMenu("Activator OKTW©").SubMenu("Cleansers").SubMenu("Buff type").AddItem(new MenuItem("Taunt", "Taunt").SetValue(true));

            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            //Drawing.OnDraw += Drawing_OnDraw;
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!Youmuus.IsReady() || !Config.Item("YoumuusR").GetValue<bool>())
                return;
            if (args.Slot == SpellSlot.R && (Player.ChampionName == "Twitch" || Player.ChampionName == "Lucian"))
            {
                Youmuus.Cast();
            }
            if (args.Slot == SpellSlot.Q && (Player.ChampionName == "Ashe" ))
            {
                Youmuus.Cast();
            }
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (Config.Item("Muramana").GetValue<bool>())
            {
                int Mur = Items.HasItem(Muramana) ? 3042 : 3043;
                if (Items.HasItem(Mur) && args.Target.IsEnemy && args.Target.IsValid<Obj_AI_Hero>() && Items.CanUseItem(Mur) && Player.Mana > Player.MaxMana * 0.3)
                {
                    if (!ObjectManager.Player.HasBuff("Muramana"))
                        Items.UseItem(Mur);
                }
                else if (ObjectManager.Player.HasBuff("Muramana") && Items.HasItem(Mur) && Items.CanUseItem(Mur))
                    Items.UseItem(Mur);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            Cleansers();
            
            if (!Program.LagFree(0))
                return;

            if (Config.Item("pots").GetValue<bool>())
                PotionManagement();

            Offensive();
        }

        private void Cleansers()
        {
            if (!Quicksilver.IsReady() && !Mikaels.IsReady() && !Mercurial.IsReady() && !Dervish.IsReady())
                return;

            if (Player.HealthPercent >= (float)Config.Item("cleanHP").GetValue<Slider>().Value || !Config.Item("Clean").GetValue<bool>())
                return;

            if (Player.HasBuff("zedulttargetmark") || Player.HasBuff("FizzMarinerDoom") || Player.HasBuff("MordekaiserChildrenOfTheGrave") || Player.HasBuff("PoppyDiplomaticImmunity") || Player.HasBuff("VladimirHemoplague"))
                Clean();

            if (Player.HasBuffOfType(BuffType.Stun) && Config.Item("Stun").GetValue<bool>())
                Clean();
            if (Player.HasBuffOfType(BuffType.Snare) && Config.Item("Snare").GetValue<bool>())
                Clean();
            if (Player.HasBuffOfType(BuffType.Knockup) && Config.Item("Knockup").GetValue<bool>())
                Clean();
            if (Player.HasBuffOfType(BuffType.Charm) && Config.Item("Charm").GetValue<bool>())
                Clean();
            if (Player.HasBuffOfType(BuffType.Fear) && Config.Item("Fear").GetValue<bool>())
                Clean();
            if (Player.HasBuffOfType(BuffType.Stun) && Config.Item("Stun").GetValue<bool>())
                Clean();
            if (Player.HasBuffOfType(BuffType.Knockback) && Config.Item("Knockback").GetValue<bool>())
                Clean();
            if (Player.HasBuffOfType(BuffType.Taunt) && Config.Item("Taunt").GetValue<bool>())
                Clean();
            if (Player.HasBuffOfType(BuffType.Suppression) && Config.Item("Suppression").GetValue<bool>())
                Clean();
        }

        private void Clean()
        {
            if (Quicksilver.IsReady())
                Quicksilver.Cast();
            else if (Mikaels.IsReady())
                Mikaels.Cast(Player);
            else if (Mercurial.IsReady())
                Mercurial.Cast();
            else if (Dervish.IsReady())
                Dervish.Cast(); 
        }

        private void Defensive()
        {
            if (Config.Item("Randuin").GetValue<bool>())
            {
                if (Randuin.IsReady() && Player.CountEnemiesInRange(Randuin.Range) > 0)
                    Randuin.Cast();
            }
        }
        private void Offensive()
        {
            if (Botrk.IsReady() && Config.Item("Botrk").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(Botrk.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                {
                    if (Config.Item("BotrkKS").GetValue<bool>() && Player.CalcDamage(t, Damage.DamageType.Physical, t.MaxHealth * 0.1) > t.Health)
                        Botrk.Cast(t);
                    if (Config.Item("BotrkLS").GetValue<bool>() && Player.Health < Player.MaxHealth * 0.5)
                        Botrk.Cast(t);
                    if (Config.Item("BotrkCombo").GetValue<bool>() && Program.Combo)
                        Botrk.Cast(t);
                }
            }

            if (Hextech.IsReady() && Config.Item("Hextech").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(Hextech.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget())
                {
                    if (Config.Item("HextechKS").GetValue<bool>() && Player.CalcDamage(t, Damage.DamageType.Magical, 150 + Player.FlatMagicDamageMod * 0.4) > t.Health)
                        Hextech.Cast(t);
                    if (Config.Item("HextechCombo").GetValue<bool>() && Program.Combo)
                        Hextech.Cast(t);
                }
            }

            if (Cutlass.IsReady() && Config.Item("Cutlass").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(Cutlass.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget())
                {
                    if (Config.Item("CutlassKS").GetValue<bool>() && Player.CalcDamage(t, Damage.DamageType.Magical, 100) > t.Health)
                        Cutlass.Cast(t);
                    if (Config.Item("CutlassCombo").GetValue<bool>() && Program.Combo)
                        Cutlass.Cast(t);
                }
            }

            if (Youmuus.IsReady() && Config.Item("Youmuus").GetValue<bool>())
            {
                var t = Orbwalker.GetTarget();

                if (t.IsValidTarget() && t is Obj_AI_Hero)
                {
                    if (Config.Item("YoumuusKS").GetValue<bool>() && t.Health < Player.MaxHealth * 0.6)
                        Youmuus.Cast();
                    if (Config.Item("YoumuusCombo").GetValue<bool>() && Program.Combo)
                        Youmuus.Cast();
                }
            }

            if (Config.Item("Hydra").GetValue<bool>())
            {
                if (Hydra.IsReady() && Player.CountEnemiesInRange(Hydra.Range) > 0)
                    Hydra.Cast();
                else if (Hydra2.IsReady() && Player.CountEnemiesInRange(Hydra2.Range) > 0)
                    Hydra2.Cast();
            }
        }

        private void PotionManagement()
        {
            if (!Player.InFountain() && !Player.HasBuff("Recall"))
            {
                if (ManaPotion.IsReady() && !Player.HasBuff("FlaskOfCrystalWater"))
                {
                    if (Player.CountEnemiesInRange(1200) > 0 && Player.Mana < 200)
                        ManaPotion.Cast();
                }
                if (Player.HasBuff("RegenerationPotion") || Player.HasBuff("ItemMiniRegenPotion") || Player.HasBuff("ItemCrystalFlask"))
                    return;

                if (Flask.IsReady())
                {
                    if (Player.CountEnemiesInRange(700) > 0 && Player.Health + 200 < Player.MaxHealth)
                        Flask.Cast();
                    else if (Player.Health < Player.MaxHealth * 0.6)
                        Flask.Cast();
                    else if (Player.CountEnemiesInRange(1200) > 0 && Player.Mana < 200 && !Player.HasBuff("FlaskOfCrystalWater"))
                        Flask.Cast();
                    return;
                }

                if (Potion.IsReady())
                {
                    if (Player.CountEnemiesInRange(700) > 0 && Player.Health + 200 < Player.MaxHealth)
                        Potion.Cast();
                    else if (Player.Health < Player.MaxHealth * 0.6)
                        Potion.Cast();
                    return;
                }

                if (Biscuit.IsReady() )
                {
                    if (Player.CountEnemiesInRange(700) > 0 && Player.Health + 200 < Player.MaxHealth)
                        Biscuit.Cast();
                    else if (Player.Health < Player.MaxHealth * 0.6)
                        Biscuit.Cast();
                    return;
                }
            }
        }
    }
}
