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
    class Tristana
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        public Spell Q, W, E, R;
        public float QMANA, WMANA, EMANA, RMANA;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; }}

        public void LoadMenuOKTW()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 620);
            R = new Spell(SpellSlot.R, 620);
            
            W.SetSkillshot(0.25f, 100, 1225, false, SkillshotType.SkillshotCircle);

            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("wRange", "W range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eInfo", "E info").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("harasQ", "Haras Q").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("focusE", "Focus target with E").SetValue(true));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                Config.SubMenu(Player.ChampionName).SubMenu("E Config").SubMenu("Harras E").AddItem(new MenuItem("harras" + enemy.ChampionName, enemy.ChampionName).SetValue(true));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                Config.SubMenu(Player.ChampionName).SubMenu("E Config").SubMenu("Use E on").AddItem(new MenuItem("useEon" + enemy.ChampionName, enemy.ChampionName).SetValue(true));


            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("nktdE", "NoKeyToDash").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("Wks", "W KS logic (W+E+R calculation)").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("smartW", "SmartCast W key").SetValue(new KeyBind('t', KeyBindType.Press))); //32 == space

            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("autoR", "Auto R KS (E+R calculation)").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("turrentR", "Try R under turrent").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("OnInterruptableSpell", "OnInterruptableSpell").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("useR", "OneKeyToCast R closest person").SetValue(new KeyBind('t', KeyBindType.Press))); //32 == space

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Config.SubMenu(Player.ChampionName).SubMenu("R Config").SubMenu("GapCloser & anti-meele").AddItem(new MenuItem("GapCloser" + enemy.ChampionName, enemy.ChampionName).SetValue(true));

            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("jungle", "Jungle Farm").SetValue(true));
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnLevelUp += Obj_AI_Base_OnLevelUp;
            Orbwalking.BeforeAttack += BeforeAttack;
            Orbwalking.AfterAttack += afterAttack;
            Interrupter2.OnInterruptableTarget +=Interrupter2_OnInterruptableTarget;
        }

        private void afterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (Orbwalker.GetTarget() == null)
                return;
            var ORBtarget = Orbwalker.GetTarget();
            if (ORBtarget.IsValid && ORBtarget is Obj_AI_Hero)
            {
                if (Program.Combo)
                    Q.Cast();
                else if (Program.Farm && Config.Item("harasQ").GetValue<bool>())
                    Q.Cast();
            }
        }

        private void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (Config.Item("focusE").GetValue<bool>())
            {
                foreach (var target in Program.Enemies.Where(target => target.IsValidTarget(900) && target.HasBuff("tristanaechargesound")))
                {
                    if (Orbwalking.InAutoAttackRange(target))
                        Orbwalker.ForceTarget(target);
                }
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (W.IsReady())
            {
                if (Config.Item("smartW").GetValue<KeyBind>().Active)
                    W.Cast(Player.Position.Extend(Game.CursorPos, W.Range), true);
            }
            if (Program.LagFree(1))
            {
                SetMana();
                Jungle();
            }
            if (Program.LagFree(2) && E.IsReady())
                LogicE();
            if (Program.LagFree(3) && R.IsReady() )
                LogicR();
            if (Program.LagFree(4) && W.IsReady())
                LogicW();
        }
        private void LogicW()
        {
            if (Config.Item("Wks").GetValue<bool>())
            {
                foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && OktwCommon.ValidUlt(enemy) && enemy.CountEnemiesInRange(800) < 3 && enemy.Health > enemy.Level * 2))
                {
                    var dmgCombo = Player.GetAutoAttackDamage(enemy) + W.GetDamage(enemy) + GetEDmg(enemy);
                    if (dmgCombo > enemy.Health)
                        Program.CastSpell(W, enemy);
                    else if (R.IsReady() && R.GetDamage(enemy) + dmgCombo > enemy.Health && Player.Mana > RMANA + WMANA)
                        Program.CastSpell(W, enemy);
                }
            }


            if (Program.Combo && Config.Item("nktdE").GetValue<bool>())
            {
                var dashPosition = Player.Position.Extend(Game.CursorPos, W.Range);

                if (Game.CursorPos.Distance(Player.Position) > Player.AttackRange + Player.BoundingRadius * 2 &&  Player.Mana > RMANA + WMANA)
                {
                    W.Cast(dashPosition);
                }
            }
        }

        private void LogicE()
        {
            var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                if (E.GetDamage(t) > t.Health)
                    E.Cast(t);
                else if (E.GetDamage(t) + R.GetDamage(t) > t.Health && Player.Mana > RMANA + EMANA)
                    E.Cast(t);
                else if (Program.Combo && Player.Mana > RMANA + EMANA && Config.Item("useEon" + t.ChampionName).GetValue<bool>())
                    E.Cast(t);
                else if (Program.Farm && Player.Mana > RMANA + EMANA + WMANA + RMANA)
                {
                    foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && Config.Item("harras" + enemy.ChampionName).GetValue<bool>()))
                        E.Cast(t);
                }
            } 
        }
        private void LogicR()
        {
            Obj_AI_Hero bestEnemy = null;
            foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(R.Range) && !OktwCommon.ValidUlt(enemy)))
            {
                if (bestEnemy == null)
                    bestEnemy = enemy;
                else if (Player.Distance(enemy.Position) < Player.Distance(bestEnemy.Position))
                    bestEnemy = enemy;

                if (R.GetDamage(enemy) + GetEDmg(enemy) > enemy.Health + enemy.Level && Config.Item("autoR").GetValue<bool>())
                {
                    R.Cast(enemy);
                    Program.debug("R ks");

                }
                if (Config.Item("turrentR").GetValue<bool>())
                {
                    float pushDistance = 400 + (R.Level * 200);
                    var prepos = Prediction.GetPrediction(enemy, 0.25f);
                    var finalPosition = prepos.CastPosition.Extend(prepos.CastPosition, -pushDistance);
                    if (!finalPosition.UnderTurret(true) && finalPosition.UnderTurret(false))
                    {
                        R.Cast(enemy);
                        Program.debug("R turrent");
                    }
                }
                if (Player.Health < Player.MaxHealth * 0.4 && enemy.IsValidTarget(270) && enemy.IsMelee && Config.Item("GapCloser" + enemy.ChampionName).GetValue<bool>())
                {
                    R.Cast(enemy);
                    Program.debug("R Meele");
                }

            }
            if (Config.Item("useR").GetValue<KeyBind>().Active && bestEnemy!=null)
            {
                R.Cast(bestEnemy);
            }
        }
        private void Jungle()
        {
            if (!Config.Item("jungle").GetValue<bool>() || !Program.LaneClear)
                return;
            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (Player.Mana > RMANA + EMANA + WMANA + RMANA)
                    E.Cast(mob);
                if (Q.IsReady())
                    Q.Cast();
            }
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (R.IsReady() && Config.Item("OnInterruptableSpell").GetValue<bool>())
            {
                if (sender.IsValidTarget(R.Range))
                {
                    R.Cast(sender);
                }
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (R.IsReady())
            {
                var Target = gapcloser.Sender;
                if (Target.IsValidTarget(R.Range) && Config.Item("GapCloser" + Target.ChampionName).GetValue<bool>())
                {
                    R.Cast(Target);
                }
            }
        }

        private float GetEDmg(Obj_AI_Base target)
        {
            if (!target.HasBuff("tristanaechargesound"))
                return 0;
            var dmg = E.GetDamage(target);
            var buffCount = OktwCommon.GetBuffCount(target, "tristanaecharge");
            dmg = dmg + (dmg * 0.3f * buffCount);
            return dmg - (target.HPRegenRate * 4);
        }

        private void Obj_AI_Base_OnLevelUp(Obj_AI_Base sender, EventArgs args)
        {
            var lvl = (7 * (Player.Level - 1));
            Q.Range = 605 + lvl;
            E.Range = 635 + lvl;
            R.Range = 635 + lvl;
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

            if (Player.Health < Player.MaxHealth * 0.3)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
            }
        }

        public static void drawText2(string msg, Vector3 Hero, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(Hero);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1] - 200, color, msg);
        }

        public static void drawText(string msg, Obj_AI_Hero Hero, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(Hero.Position);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1], color, msg);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("watermark").GetValue<bool>())
            {
                Drawing.DrawText(Drawing.Width * 0.2f, Drawing.Height * 0f, System.Drawing.Color.Cyan, "OneKeyToWin AIO - " + Player.ChampionName + " by Sebby");
            }
            if (Config.Item("eInfo").GetValue<bool>())
            {
                foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(2000)))
                {
                    if (GetEDmg(enemy) > enemy.Health)
                        drawText("IS DEAD", enemy, System.Drawing.Color.Yellow);
                    if (enemy.HasBuff("tristanaechargesound"))
                        drawText2("E:  " + String.Format("{0:0.0}", OktwCommon.GetPassiveTime(enemy, "tristanaechargesound")), enemy.Position, System.Drawing.Color.Yellow);
                }
            }
            if (Config.Item("nktdE").GetValue<bool>())
            {

                if (Game.CursorPos.Distance(Player.Position) > Player.AttackRange + Player.BoundingRadius * 2)
                    drawText2("dash: ON ", Player.Position, System.Drawing.Color.Red);
                else
                    drawText2("dash: OFF ", Player.Position, System.Drawing.Color.GreenYellow);
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
    }
}
