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
    class Quinn
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell Q, Q1, W, E, R;
        private float QMANA, WMANA, EMANA, RMANA;

        public Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }
        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 930);
            E = new Spell(SpellSlot.E, 700);
            W = new Spell(SpellSlot.W, 2100);
            R = new Spell(SpellSlot.R, 550);

            Q.SetSkillshot(0.3f, 80f, 1150, true, SkillshotType.SkillshotLine);
            E.SetTargetted(0.25f, 2000f);

            LoadMenuOKTW(); 
            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += afterAttack;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;

        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (E.IsReady() && sender.IsValidTarget(E.Range))
                E.CastOnUnit(sender);
        }

        private void afterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
                return;
            LogicE();
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("AGC").GetValue<bool>() )
            {
                var Target = (Obj_AI_Hero)gapcloser.Sender;
                if (Target.IsValidTarget(E.Range) && E.IsReady())
                {
                    E.Cast(Target, true);
                    Program.debug("E AGC");
                }
                else if (Target.IsValidTarget(Q.Range))
                {
                    Q.Cast(Target, true);
                    Program.debug("Q AGC");
                }
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.Target == null && !Q.IsReady())
                return;
            
            if (sender.IsValid<Obj_AI_Hero>() && sender.IsEnemy && args.Target.IsMe && args.SData.IsAutoAttack())
            {
                var target2 = ObjectManager.Get<Obj_AI_Hero>().Find(x => x.NetworkId == sender.NetworkId);
                if (ActiveR && target2.IsValidTarget(500))
                {
                    Q.Cast();
                    return;
                }

                Q.Cast(target2);
                //Game.PrintChat("" + HpPercentage);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsRecalling() && Player.IsDead)
                return;

            if (Program.LagFree(1))
                SetMana();
            if (Program.LagFree(2) && Q.IsReady())
                LogicQ();

            if (Program.LagFree(3) && E.IsReady() )
            {
                if (ActiveR)
                    LogicE();
                else
                    LogicE2();
            }


            if (Program.LagFree(4) && R.IsReady())
                LogicR();
        }

        
        private void LogicR()
        {
            var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            if (ActiveR)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(650)))
                {
                    if (Program.GetRealDmg(R, enemy) > enemy.Health)
                        R.Cast();
                    
                }
            }
        }

        private void LogicQ()
        {
            if (ActiveR)
            {
                if (ObjectManager.Player.CountEnemiesInRange(500) > 1 && ObjectManager.Player.Mana > EMANA + QMANA)
                    Q.Cast();
                return;
            }
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget(Q.Range))
            {
                if (Program.GetRealDmg(Q, t) > t.Health)
                    Program.CastSpell(Q, t);
                else if (Program.Combo && ObjectManager.Player.Mana > RMANA + QMANA && !Orbwalking.InAutoAttackRange(t))
                    Program.CastSpell(Q, t);
                else if ((Program.Farm && ObjectManager.Player.Mana > RMANA + EMANA + QMANA + WMANA) && !ObjectManager.Player.UnderTurret(true))
                {
                    Program.CastSpell(Q, t);
                }
            }

        }
        private void LogicE2()
        {
            var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget(E.Range))
            {
                if (ObjectManager.Player.Mana > RMANA + EMANA + QMANA && (Program.Combo || Program.Farm) && t.IsDashing() && t.CountEnemiesInRange(600) < 3)
                    E.Cast(t);
            }
        }
        private void LogicE()
        {
            var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if ( t.IsValidTarget(E.Range) && !t.HasBuff("QuinnW"))
            {
                if (E.GetDamage(t) + ObjectManager.Player.GetAutoAttackDamage(t) * 3 > t.Health)
                    E.Cast(t);
                else if (ObjectManager.Player.Mana > RMANA + EMANA && ObjectManager.Player.CountEnemiesInRange(260) > 0)
                    E.Cast(t);
                else if (ObjectManager.Player.Mana > RMANA + EMANA + QMANA && Program.Combo && t.CountEnemiesInRange(1000) < 3)
                    E.Cast(t);
                else if (ObjectManager.Player.Mana > RMANA + EMANA + QMANA && Program.Combo && t.IsDashing())
                    E.Cast(t);
            }
        }

        private bool ActiveR
        {
            get { return (Player.HasBuff("quinnrtimeout") || Player.HasBuff("QuinnRForm")); }
        }
        private void SetMana()
        {
            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R.IsReady())
                RMANA = WMANA - Player.PARRegenRate * W.Instance.Cooldown;
            else
                RMANA = R.Instance.ManaCost; 

            if (Player.Health < Player.MaxHealth * 0.2)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
            }
        }
        private void LoadMenuOKTW()
        {
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("wRange", "W range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range").SetValue(false));
            

            Config.SubMenu(Player.ChampionName).SubMenu("E config").AddItem(new MenuItem("AGC", "AntiGapcloser E,Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("autoW", "Auto W").SetValue(true));


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
            if (Config.Item("wRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>())
                {
                    if (W.IsReady())
                        Utility.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1, 1);
                }
                else
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1, 1);
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
        }
    }
}
