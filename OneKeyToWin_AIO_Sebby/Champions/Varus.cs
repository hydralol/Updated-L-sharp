﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Varus
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell Q, W, E, R;
        private float QMANA, WMANA, EMANA, RMANA;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public float AArange = ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius * 2;
        float CastTime = Game.Time;
        bool CanCast = true;

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 925);
            W = new Spell(SpellSlot.Q, 0);
            E = new Spell(SpellSlot.E, 975);
            R = new Spell(SpellSlot.R, 1050);


            Q.SetSkillshot(0.25f, 70, 1900, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.35f, 120, 1500, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 120, 1950, false, SkillshotType.SkillshotLine);
            Q.SetCharged("VarusQ", "VarusQ", 925, 1600, 1.5f);


            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range").SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range").SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("autoQ", "Auto Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("maxQ", "Cast Q only max range").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("fastQ", "Fast cast Q").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("autoE", "Auto E").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("autoR", "Auto R").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("rCount", "Auto R if enemies in range (combo mode)").SetValue(new Slider(3, 0, 5)));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("useR", "Semi-manual cast R key").SetValue(new KeyBind('t', KeyBindType.Press))); //32 == space

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                Config.SubMenu(Player.ChampionName).SubMenu("R Config").SubMenu("GapCloser R").AddItem(new MenuItem("GapCloser" + enemy.ChampionName, enemy.ChampionName).SetValue(false));

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                Config.SubMenu(Player.ChampionName).SubMenu("Harras").AddItem(new MenuItem("harras" + enemy.ChampionName, enemy.ChampionName).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmQ", "Lane clear Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmE", "Lane clear E").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("Mana", "LaneClear Mana").SetValue(new Slider(80, 100, 30)));

            Game.OnUpdate += Game_OnGameUpdate;

            Drawing.OnDraw += Drawing_OnDraw;
            //Orbwalking.BeforeAttack += BeforeAttack;
            //Orbwalking.AfterAttack += afterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            //Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("watermark").GetValue<bool>())
            {
                Drawing.DrawText(Drawing.Width * 0.2f, Drawing.Height * 0f, System.Drawing.Color.Cyan, "OneKeyToWin AIO - " + Player.ChampionName + " by Sebby");
            }

            if (Config.Item("qRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>())
                {
                    if (Q.IsReady())
                        Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
                }
                else
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
            }

            if (Config.Item("eRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>())
                {
                    if (E.IsReady())
                        Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow, 1, 1);
                }
                else
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow, 1, 1);
            }
            if (Config.Item("rRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>())
                {
                    if (R.IsReady())
                        Utility.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray, 1, 1);
                }
                else
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray, 1, 1);
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (R.IsReady() && Config.Item("GapCloser" + gapcloser.Sender.ChampionName).GetValue<bool>())
            {
                var Target = gapcloser.Sender;
                if (Target.IsValidTarget(R.Range))
                {
                    R.Cast(Target.ServerPosition, true);
                    //Program.debug("AGC " );
                }
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "VarusQ" || args.SData.Name == "VarusE" || args.SData.Name == "VarusR")
                {
                    CastTime = Game.Time;
                    CanCast = false;
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {

            if (R.IsReady())
            {
                if (Config.Item("useR").GetValue<KeyBind>().Active)
                {
                    var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                    if (t.IsValidTarget())
                        R.Cast(t);
                }
            }
            if (Program.LagFree(0))
            {
                SetMana();
                if (!CanCast)
                {
                    if (Game.Time - CastTime > 1)
                    {
                        CanCast = true;
                        return;
                    }
                    var t = Orbwalker.GetTarget() as Obj_AI_Base;
                    if (t.IsValidTarget())
                    {
                        if (OktwCommon.GetBuffCount(t, "varuswdebuff") < 3)
                            CanCast = true;
                    }
                    else
                    {
                        CanCast = true;
                    }
                }
            }

            if (Program.LagFree(1) && E.IsReady() && Config.Item("autoQ").GetValue<bool>() && !Player.IsWindingUp)
                LogicE();
            if (Program.LagFree(2) && Q.IsReady() && Config.Item("autoE").GetValue<bool>() && !Player.IsWindingUp)
                LogicQ();
            if (Program.LagFree(3) && R.IsReady() && Config.Item("autoR").GetValue<bool>())
                LogicR();
            if (Program.LagFree(4))
                Farm();
        }

        private void Farm()
        {
            if (Program.LaneClear && E.IsReady() && Config.Item("farmE").GetValue<bool>())
            {
                var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                if (mobs.Count > 0 && Player.Mana > RMANA + EMANA + QMANA && OktwCommon.GetBuffCount(mobs[0], "varuswdebuff") == 3)
                {
                    E.Cast(mobs[0]);
                    return;
                }

                if (Player.ManaPercentage() > Config.Item("Mana").GetValue<Slider>().Value)
                {
                    var allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All);
                    var Efarm = Q.GetCircularFarmLocation(allMinionsE, E.Width);
                    if (Efarm.MinionsHit > 3)
                    {
                        E.Cast(Efarm.Position);
                        return;
                    }
                }
            }
        }

        private void LogicR()
        {
            foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(R.Range)))
            {

                if (enemy.CountEnemiesInRange(400) >= Config.Item("rCount").GetValue<Slider>().Value && Config.Item("rCount").GetValue<Slider>().Value > 0)
                {
                    R.Cast(enemy, true, true);
                    Program.debug("R AOE");
                }
                if ((enemy.CountAlliesInRange(600) == 0 || Player.Health < Player.MaxHealth * 0.5) && R.GetDamage(enemy) + GetWDmg(enemy) + Q.GetDamage(enemy) > enemy.Health && OktwCommon.ValidUlt(enemy))
                {
                    Program.CastSpell(R, enemy);
                    Program.debug("R KS");
                }
            }
            if (Player.Health < Player.MaxHealth * 0.5)
            {
                foreach (var target in Program.Enemies.Where(target => target.IsValidTarget(270) && target.IsMelee && Config.Item("GapCloser" + target.ChampionName).GetValue<bool>()))
                {

                    Program.CastSpell(R, target);

                }
            }
        }

        private void LogicQ()
        {

            foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(1600) && Q.GetDamage(enemy) + GetWDmg(enemy) > enemy.Health))
            {
                if (enemy.IsValidTarget(R.Range))
                    CastQ(enemy);
                return;
            }

            if (Config.Item("maxQ").GetValue<bool>() && (Q.Range < 1500) && Player.CountEnemiesInRange(AArange) == 0)
                return;

            var t = Orbwalker.GetTarget() as Obj_AI_Hero;
            if (!t.IsValidTarget())
                t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (t.IsValidTarget())
            {
                if (Q.IsCharging)
                {
                    if (Config.Item("fastQ").GetValue<bool>())
                        Q.Cast(Q.GetPrediction(t).CastPosition);

                    if (GetQEndTime() > 2)
                        Program.CastSpell(Q, t);
                    else
                        Q.Cast(Q.GetPrediction(t).CastPosition);
                    return;
                }

                if ((OktwCommon.GetBuffCount(t, "varuswdebuff") == 3 && CanCast && !E.IsReady()) || !Orbwalking.InAutoAttackRange(t))
                {
                    if ((Program.Combo || (OktwCommon.GetBuffCount(t, "varuswdebuff") == 3 && Program.Farm)) && Player.Mana > RMANA + QMANA)
                    {
                        CastQ(t);
                    }
                    else if (Program.Farm && Player.Mana > RMANA + EMANA + QMANA + QMANA && Config.Item("harras" + t.ChampionName).GetValue<bool>() && !Player.UnderTurret(true) && OktwCommon.CanHarras())
                    {
                        CastQ(t);
                    }
                    else if ((Program.Combo || Program.Farm) && Player.Mana > RMANA + WMANA)
                    {
                        foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                            CastQ(enemy);
                    }
                }
            }
            else if (Q.Range > 1500 && Player.CountEnemiesInRange(1450) == 0 && Program.LaneClear && (Q.IsCharging || (Player.ManaPercentage() > Config.Item("Mana").GetValue<Slider>().Value && Config.Item("farmQ").GetValue<bool>() && Player.Mana > RMANA + QMANA + WMANA)))
            {
                var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All);
                var Qfarm = Q.GetLineFarmLocation(allMinionsQ, Q.Width);
                if (Qfarm.MinionsHit > 3 || (Q.IsCharging && Qfarm.MinionsHit > 0))
                    Q.Cast(Qfarm.Position);
            }
        }

        private void LogicE()
        {
            foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && E.GetDamage(enemy) + GetWDmg(enemy) > enemy.Health))
            {
                Program.CastSpell(E, enemy);
            }
            var t = Orbwalker.GetTarget() as Obj_AI_Hero;
            if (!t.IsValidTarget())
                t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                if ((OktwCommon.GetBuffCount(t, "varuswdebuff") == 3 && CanCast) || !Orbwalking.InAutoAttackRange(t))
                {
                    if (Program.Combo && Player.Mana > RMANA + QMANA)
                    {
                        Program.CastSpell(E, t);
                    }
                    else if ((Program.Combo || Program.Farm) && Player.Mana > RMANA + WMANA)
                    {
                        foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && !OktwCommon.CanMove(enemy)))
                            E.Cast(enemy);
                    }
                }
            }
        }

        private float GetQEndTime()
        {
            return
                Player.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Name == "VarusQ")
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault() - Game.Time;
        }

        private float GetWDmg(Obj_AI_Base target)
        {
            return (OktwCommon.GetBuffCount(target, "varuswdebuff") * W.GetDamage(target, 1));
        }

        private void CastQ(Obj_AI_Base target)
        {
            if (!Q.IsCharging)
            {
                if (target.IsValidTarget(Q.Range - 300))
                    Q.StartCharging();
            }
            else
            {
                if (GetQEndTime() > 1)
                    Program.CastSpell(Q, target);
                else
                    Q.Cast(Q.GetPrediction(target).CastPosition);
                return;
            }
        }

        private void SetMana()
        {
            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R.IsReady())
                RMANA = QMANA - Player.PARRegenRate * Q.Instance.Cooldown;
            else
                RMANA = R.Instance.ManaCost;

            if (ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.2)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
            }
        }
    }
}