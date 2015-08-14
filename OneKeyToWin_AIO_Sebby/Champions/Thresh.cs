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
    class Thresh
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;

        private Spell E, Q, R, W;

        private float QMANA, WMANA, EMANA, RMANA;

        private int grab = 0, grabS = 0;

        private float grabW = 0;

        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 1075);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 450);
            R = new Spell(SpellSlot.R, 430);

            Q.SetSkillshot(0.5f, 70, 1900f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 2000, 1900f, false, SkillshotType.SkillshotLine);  

            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("ts", "Use common TargetSelector").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("ts1", "ON - only one target"));
            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("ts2", "OFF - all grab-able targets"));
            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("qCC", "Auto Q cc & dash enemy").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("minGrab", "Min range grab").SetValue(new Slider(250, 125, (int)Q.Range)));
            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("maxGrab", "Max range grab").SetValue(new Slider((int)Q.Range, 125, (int)Q.Range)));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Config.SubMenu(Player.ChampionName).SubMenu("Q option").SubMenu("Grab").AddItem(new MenuItem("grab" + enemy.ChampionName, enemy.ChampionName).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("GapQ", "OnEnemyGapcloser Q")).SetValue(true);

            Config.SubMenu(Player.ChampionName).SubMenu("W option").AddItem(new MenuItem("autoW", "Auto W").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W option").AddItem(new MenuItem("autoW3", "Auto W shield big dmg").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W option").AddItem(new MenuItem("autoW2", "Auto W if Q succesfull").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W option").AddItem(new MenuItem("wCount", "Auto W if x enemies near ally").SetValue(new Slider(3, 0, 5)));

            Config.SubMenu(Player.ChampionName).SubMenu("E option").AddItem(new MenuItem("autoE", "Auto E").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E option").AddItem(new MenuItem("pushE", "Auto push").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E option").AddItem(new MenuItem("inter", "OnPossibleToInterrupt")).SetValue(true);
            Config.SubMenu(Player.ChampionName).SubMenu("E option").AddItem(new MenuItem("Gap", "OnEnemyGapcloser")).SetValue(true);

            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("rCount", "Auto R if x enemies in range").SetValue(new Slider(3, 0, 5)));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("rKs", "R ks").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("comboR", "always R in combo").SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("wRange", "W range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw when skill rdy").SetValue(true));

            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (E.IsReady() && Config.Item("inter").GetValue<bool>() && sender.IsValidTarget(E.Range))
            {
                E.Cast(sender.ServerPosition);
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (E.IsReady() && Config.Item("Gap").GetValue<bool>())
            {
                E.Cast(gapcloser.End);
            }
            else if (Q.IsReady() && Config.Item("GapQ").GetValue<bool>())
            {
                Q.Cast(gapcloser.End);
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!W.IsReady() || !Config.Item("autoW").GetValue<bool>() || !sender.IsEnemy || !sender.IsValidTarget(1500) )
                return;

            double dmg = 0;
            double shieldValue = 20 + (W.Level * 40) + (0.4 * Player.FlatMagicDamageMod);
            foreach (var ally in Program.Allies.Where(ally => ally.IsValid && !ally.IsDead && Player.Distance(ally.ServerPosition) < W.Range + 300))
            {
                if (args.Target != null && args.Target.NetworkId == ally.NetworkId)
                {
                    dmg = dmg + sender.GetSpellDamage(ally, args.SData.Name);
                }

                if ( dmg > 0)
                {
                    if (ally.Health - dmg < ally.CountEnemiesInRange(600) * ally.Level * 20)
                        W.Cast(ally.ServerPosition);
                    else if (ally.Health - dmg < ally.Level * 10)
                        W.Cast(ally.ServerPosition);
                    else if (dmg > shieldValue * 3 && Config.Item("autoW3").GetValue<bool>())
                        W.Cast(ally.ServerPosition);
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Program.LagFree(1) && Q.IsReady())
                LogicQ();
            if (Program.LagFree(2) && E.IsReady() && Config.Item("autoE").GetValue<bool>())
                LogicE();
            if (Program.LagFree(3) && W.IsReady())
                LogicW();
            if (Program.LagFree(4) && R.IsReady())
                LogicR();
        }

        private void LogicE()
        {
            var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget() && !t.HasBuff("ThreshQ"))
            {
                foreach (var buff in t.Buffs)
                {
                    Program.debug("" + buff.Name);
                        
                }
                var revertPosition = t.ServerPosition;
                if (Program.Combo)
                {
                    CastE(false, t);
                }
                else if (Config.Item("pushE").GetValue<bool>())
                {
                    CastE(true, t);
                }
                E.Cast(revertPosition);
            }
        }

        private void LogicQ()
        {
            foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && enemy.HasBuff("ThreshQ")))
            {
                if (Program.Combo)
                {
                    if (W.IsReady() && Config.Item("autoW2").GetValue<bool>())
                    {
                        var allyW = Player;
                        foreach (var ally in Program.Allies.Where(ally => ally.IsValid && !ally.IsDead && Player.Distance(ally.ServerPosition) < W.Range + 300))
                        {
                            if (enemy.Distance(ally.ServerPosition) > 800 && Player.Distance(ally.ServerPosition) > 600)
                            {
                                W.Cast(Prediction.GetPrediction(ally, 1f).CastPosition);
                            }
                        }
                    }
                    if (OktwCommon.GetPassiveTime(enemy, "ThreshQ") < 0.3)
                        Q.Cast();
                }
                return;
            }
            float maxGrab = Config.Item("maxGrab").GetValue<Slider>().Value;
            float minGrab = Config.Item("minGrab").GetValue<Slider>().Value;

            if (Program.Combo && Config.Item("ts").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(maxGrab, TargetSelector.DamageType.Physical);

                if (t.IsValidTarget(maxGrab) && !t.HasBuffOfType(BuffType.SpellImmunity) && !t.HasBuffOfType(BuffType.SpellShield) && Config.Item("grab" + t.ChampionName).GetValue<bool>() && Player.Distance(t.ServerPosition) > minGrab)
                    Program.CastSpell(Q, t);
            }
            foreach (var t in Program.Enemies.Where(t => t.IsValidTarget(maxGrab) && Config.Item("grab" + t.ChampionName).GetValue<bool>()))
            {
                if (!t.HasBuffOfType(BuffType.SpellImmunity) && !t.HasBuffOfType(BuffType.SpellShield) && Player.Distance(t.ServerPosition) > minGrab)
                {
                    if (Program.Combo && !Config.Item("ts").GetValue<bool>())
                        Program.CastSpell(Q, t);

                    if (Config.Item("qCC").GetValue<bool>())
                    {
                        if (!OktwCommon.CanMove(t))
                            Q.Cast(t, true);
                        Q.CastIfHitchanceEquals(t, HitChance.Dashing);
                        Q.CastIfHitchanceEquals(t, HitChance.Immobile);
                    }
                }
            }
        }

        private void LogicR()
        {
            bool rKs = Config.Item("rKs").GetValue<bool>();
            foreach (var target in Program.Enemies.Where(target => target.IsValidTarget(R.Range) && target.HasBuff("rocketgrab2")))
            {
                if (rKs && R.GetDamage(target) > target.Health)
                    R.Cast();
            }
            if (Player.CountEnemiesInRange(R.Range) >= Config.Item("rCount").GetValue<Slider>().Value && Config.Item("rCount").GetValue<Slider>().Value > 0)
                R.Cast();
            if (Config.Item("comboR").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget() && ((Player.UnderTurret(false) && !Player.UnderTurret(true)) || Program.Combo))
                {
                    if (Player.Distance(t.ServerPosition) > Player.Distance(t.Position))
                        R.Cast();
                }
            }
        }

        private void LogicW()
        {
            foreach (var ally in Program.Allies.Where(ally => ally.IsValid && !ally.IsDead && Player.Distance(Prediction.GetPrediction(ally, 1f).CastPosition) < W.Range + 300))
            {
                if (ally.CountEnemiesInRange(700) >= Config.Item("wCount").GetValue<Slider>().Value && Config.Item("wCount").GetValue<Slider>().Value > 0)
                    W.Cast(Prediction.GetPrediction(ally, 1f).CastPosition);
            }
        }

        private void CastE(bool pull, Obj_AI_Base target)
        {
            var eCastPosition = E.GetPrediction(target).CastPosition;
            if (pull)
            {
                E.Cast(eCastPosition);
            }
            else
            {
                E.Cast(Player.Position.Extend(eCastPosition, -300));
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("qRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>())
                {
                    if (Q.IsReady())
                        Utility.DrawCircle(Player.Position, (float)Config.Item("maxGrab").GetValue<Slider>().Value, System.Drawing.Color.Cyan, 1, 1);
                }
                else
                    Utility.DrawCircle(Player.Position, (float)Config.Item("maxGrab").GetValue<Slider>().Value, System.Drawing.Color.Cyan, 1, 1);
            }

            if (Config.Item("wRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>())
                {
                    if (E.IsReady())
                        Utility.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Cyan, 1, 1);
                }
                else
                    Utility.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Cyan, 1, 1);
            }

            if (Config.Item("eRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>())
                {
                    if (E.IsReady())
                        Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Orange, 1, 1);
                }
                else
                    Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Orange, 1, 1);
            }

            if (Config.Item("rRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>())
                {
                    if (R.IsReady())
                        Utility.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Gray, 1, 1);
                }
                else
                    Utility.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Gray, 1, 1);
            }
        }
    }
}
