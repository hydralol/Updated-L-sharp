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
    class Blitzcrank
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;

        private Spell E, Q, R, W;

        private float QMANA, WMANA, EMANA, RMANA;

        private int grab = 0 , grabS = 0;

        private float grabW = 0;

        public Obj_AI_Hero Player {get { return ObjectManager.Player; }}

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 950);
            W = new Spell(SpellSlot.W, 200);
            E = new Spell(SpellSlot.E, 475);
            R = new Spell(SpellSlot.R, 600);

            Q.SetSkillshot(0.25f, 100f, 1900f, true, SkillshotType.SkillshotLine);

            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("autoW", "Auto W").SetValue(true));
            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("autoE", "Auto E").SetValue(true));
            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("showgrab", "Show statistics").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("ts", "Use common TargetSelector").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("ts1", "ON - only one target"));
            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("ts2", "OFF - all grab-able targets"));

            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("qCC", "Auto Q cc & dash enemy").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("minGrab", "Min range grab").SetValue(new Slider(250, 125, (int)Q.Range)));
            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("maxGrab", "Max range grab").SetValue(new Slider((int)Q.Range, 125, (int)Q.Range)));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Config.SubMenu(Player.ChampionName).SubMenu("Q option").SubMenu("Grab").AddItem(new MenuItem("grab" + enemy.ChampionName, enemy.ChampionName).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("rCount", "Auto R if enemies in range").SetValue(new Slider(3, 0, 5)));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("afterGrab", "Auto R after grab").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("rKs", "R ks").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("inter", "OnPossibleToInterrupt")).SetValue(true);
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("Gap", "OnEnemyGapcloser")).SetValue(true);

            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw when skill rdy").SetValue(true));

            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.BeforeAttack += BeforeAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (R.IsReady() && Config.Item("inter").GetValue<bool>() && sender.IsValidTarget(R.Range))
                R.Cast();
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "RocketGrabMissile")
            {
                grab++;
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("showgrab").GetValue<bool>() && grab > 0)
            {
                Drawing.DrawText(Drawing.Width * 0f, Drawing.Height * 0.4f, System.Drawing.Color.YellowGreen, " grab: " + grab + " grab successful: " + grabS + " grab successful % : " + ((grabS / grab) * 100) + "%");
            }
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

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (R.IsReady() && Config.Item("Gap").GetValue<bool>() && gapcloser.Sender.IsValidTarget(R.Range))
                R.Cast();
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Program.LagFree(1) && Q.IsReady())
                LogicQ();
            if (Program.LagFree(2) && R.IsReady())
                LogicR();
            if (Program.LagFree(3) && W.IsReady() && Config.Item("autoW").GetValue<bool>())
                LogicW();

            if (!Q.IsReady() && Game.Time - grabW > 2)
            {
                foreach (var t in Program.Enemies.Where(t => t.HasBuff("rocketgrab2")))
                {
                    grabS++;
                    grabW = Game.Time;
                    Program.debug("GRAB!!!!");
                }
            }
        }

        private void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (E.IsReady() && args.Target.IsValid<Obj_AI_Hero>() && Config.Item("autoE").GetValue<bool>())
                E.Cast();   
        }

        private void LogicQ()
        {
            float maxGrab = Config.Item("maxGrab").GetValue<Slider>().Value;
            float minGrab =  Config.Item("minGrab").GetValue<Slider>().Value;

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
                        Program.CastSpell(Q,t);

                    if (Config.Item("qCC").GetValue<bool>() )
                    {
                        if(!OktwCommon.CanMove(t))
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
            bool afterGrab = Config.Item("afterGrab").GetValue<bool>();
            foreach (var target in Program.Enemies.Where(target => target.IsValidTarget(R.Range) && target.HasBuff("rocketgrab2")))
            {
                if (rKs && R.GetDamage(target) > target.Health)
                    R.Cast();
                if (afterGrab && target.IsValidTarget(400) && target.HasBuff("rocketgrab2"))
                    R.Cast();
            }
            if (Player.CountEnemiesInRange(R.Range) >= Config.Item("rCount").GetValue<Slider>().Value && Config.Item("rCount").GetValue<Slider>().Value > 0)
                R.Cast();
        }
        private void LogicW()
        {
            foreach (var target in Program.Enemies.Where(target => target.IsValidTarget(R.Range) && target.HasBuff("rocketgrab2")))
                W.Cast();
        }
    }
}
