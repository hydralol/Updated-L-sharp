using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Darius
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        public Spell Q, W, E, R;
        private float QMANA, WMANA, EMANA, RMANA;
        private Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 400);
            W = new Spell(SpellSlot.W, 145);
            E = new Spell(SpellSlot.E, 540);
            R = new Spell(SpellSlot.R, 460);

            E.SetSkillshot(0.1f, 50f * (float)Math.PI / 180, float.MaxValue, false, SkillshotType.SkillshotCone);

            LoadMenuOKTW();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.BeforeAttack += BeforeAttack;
            Interrupter.OnPossibleToInterrupt += OnInterruptableSpell;
        }

        private void LoadMenuOKTW()
        {
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw when skill rdy").SetValue(true));

            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("farmQ", "Farm Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("haras", "Harras Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("autoR", "Auto R").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("useR", "Semi-manual cast R key").SetValue(new KeyBind('t', KeyBindType.Press))); //32 == space

        }

        private void OnInterruptableSpell(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (E.IsReady()  && unit.IsValidTarget(E.Range))
                E.Cast(unit);
        }

        private void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (W.IsReady() && args.Target.IsValid<Obj_AI_Hero>() && Player.Mana > RMANA + WMANA)
                W.Cast();
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (R.IsReady() && Config.Item("useR").GetValue<KeyBind>().Active)
            {
                var targetR = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.True);
                if (targetR.IsValidTarget())
                    R.Cast(targetR, true);
            }
            
            if (Program.LagFree(0))
            {
                SetMana();
            }

            if (Program.LagFree(2) && Q.IsReady())
                LogicQ();
            if (Program.LagFree(3) && E.IsReady())
                LogicE();
            if (Program.LagFree(4) && R.IsReady() && Config.Item("autoR").GetValue<bool>())
                LogicR();
        }

        private void LogicE()
        {
            if (Player.Mana > RMANA + EMANA )
            {
                var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                if (target.IsValidTarget() && ((Player.UnderTurret(false) && !Player.UnderTurret(true)) || Program.Combo) )
                {
                    if ((target.Path.Count() > 0 || (ObjectManager.Player.Distance(target.ServerPosition) > 460 && target.Path.Count() == 0)) && ObjectManager.Player.Distance(target.ServerPosition) >= ObjectManager.Player.Distance(target.Position) && ObjectManager.Player.Distance(target.ServerPosition) > 260)
                        E.Cast(target, true, true);
                }
            }
        }

        private void LogicQ()
        {
            if (Player.CountEnemiesInRange(Q.Range) > 0)
            {
                if (Player.Mana > RMANA + QMANA && Program.Combo)
                    Q.Cast();
                else if (Program.Farm && ObjectManager.Player.Mana > RMANA + QMANA + EMANA + WMANA && Config.Item("haras").GetValue<bool>())
                    Q.Cast();
                if (!R.IsReady())
                {
                    var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                    if (target.IsValidTarget() && Player.Distance(target.Position) < Q.Range && Q.GetDamage(target) > target.Health)
                        Q.Cast();
                }
            }
            else if (Config.Item("farmQ").GetValue<bool>() && Player.Mana > RMANA + QMANA + EMANA + WMANA && Program.LaneClear)
            {
                var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All);
                foreach (var minion in allMinionsQ)
                    if (Player.Distance(minion.ServerPosition) > 300 && minion.Health < Player.GetSpellDamage(minion, SpellSlot.Q) * 0.6)
                        Q.Cast();
            }
        }

        private void LogicR()
        {
            foreach (var target in Program.Enemies.Where(target => Program.ValidUlt(target) && target.IsValidTarget(R.Range) ))
            {
                if (R.GetDamage(target) - target.Level > target.Health)
                {
                    R.Cast(target, true);
                }
                else
                {
                    foreach (var buff in target.Buffs)
                    {
                        if (buff.Name == "dariushemo")
                        {
                            if (R.GetDamage(target) * (1 + (float)buff.Count / 5) - 1 > target.Health)
                                R.CastOnUnit(target, true);
                            else if (Player.Health < Player.MaxHealth * 0.4 && Player.GetSpellDamage(target, SpellSlot.R, 1) * 1.2 * ((1 + buff.Count / 5) - 1) > target.Health)
                                R.CastOnUnit(target, true);
                        }
                    }
                }
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("qRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>() && Q.IsReady())
                    if (Q.IsReady())
                        Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
                    else
                        Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
            }

            if (Config.Item("eRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>() && E.IsReady())
                    if (E.IsReady())
                        Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Orange, 1, 1);
                    else
                        Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Orange, 1, 1);
            } 
            if (Config.Item("rRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>() && R.IsReady())
                    if (R.IsReady())
                        Utility.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Red, 1, 1);
                    else
                        Utility.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Red, 1, 1);
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
