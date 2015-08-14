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
    class Caitlyn
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell E, Q, Qc, R, W;
        private float QMANA, WMANA, EMANA, RMANA;

        private float QCastTime = 0;

        public Obj_AI_Hero Player { get { return ObjectManager.Player; }}

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 1250f);
            Qc = new Spell(SpellSlot.Q, 1100f);
            W = new Spell(SpellSlot.W, 800f);
            E = new Spell(SpellSlot.E, 980f);
            R = new Spell(SpellSlot.R, 3000f);


            Q.SetSkillshot(0.65f, 90f, 2200f, false, SkillshotType.SkillshotLine);
            Qc.SetSkillshot(0.65f, 90f, 2200f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(1.5f, 1f, 1750f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 80f, 1600f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.7f, 200f, 1500f, false, SkillshotType.SkillshotCircle);

            LoadMenuOKTW();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            //Orbwalking.BeforeAttack += BeforeAttack;
            //Orbwalking.AfterAttack += afterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }


        private void LoadMenuOKTW()
        {
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("noti", "Show notification & line").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("wRange", "W range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("autoW", "Auto W on hard CC").SetValue(true));  
            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("telE", "Auto W teleport").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("bushW", "Auto W bush").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("autoE", "Auto E").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("useE", "Dash E HotKeySmartcast").SetValue(new KeyBind('t', KeyBindType.Press)));

            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("autoQ", "Reduce Q use").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("farmQ", "Lane clear Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("Mana", "LaneClear Mana").SetValue(new Slider(80, 100, 30)));

            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("autoR", "Auto R KS").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("useR", "Semi-manual cast R key").SetValue(new KeyBind('t', KeyBindType.Press)));

            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("AGC", "Anti Gapcloser E,W").SetValue(true));
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (unit.IsMe && (args.SData.Name == "CaitlynPiltoverPeacemaker" || args.SData.Name == "CaitlynEntrapment"))
            {
                QCastTime = Game.Time;
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("AGC").GetValue<bool>() && Player.Mana > RMANA + WMANA)
            {
                var Target = (Obj_AI_Hero)gapcloser.Sender;
                if (E.IsReady() && Target.IsValidTarget(E.Range) && Player.Position.Extend(Game.CursorPos, 400).CountEnemiesInRange(800) < 3)
                    E.Cast(Target, true);
                else if (W.IsReady() && Target.IsValidTarget(W.Range))
                    Program.CastSpell(W, Target);
                return;
            }
            return;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {

            if (Config.Item("useR").GetValue<KeyBind>().Active && R.IsReady())
            {
                var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                    R.CastOnUnit(t);
            }

            if (Program.LagFree(0))
            {
                SetMana();
                R.Range = (500 * R.Level) + 1500;
                //debug("" + ObjectManager.Player.AttackRange);
            }
            
            if (Program.LagFree(1) && E.IsReady() && !Player.IsWindingUp)
                LogicE();
            if (Program.LagFree(2) && W.IsReady())
                LogicW();
            if (Program.LagFree(3) && Q.IsReady() && !Player.IsWindingUp)
                LogicQ();
            if (Program.LagFree(4) && R.IsReady() && Config.Item("autoR").GetValue<bool>() && !ObjectManager.Player.UnderTurret(true) && Game.Time - QCastTime > 1)
                LogicR();
        }

        private void LogicR()
        {
            bool cast = false;

            foreach (var target in Program.Enemies.Where(target => target.IsValidTarget(R.Range) && Program.ValidUlt(target) && target.CountEnemiesInRange(500) == 1 && target.CountAlliesInRange(500) == 0))
            {
                float predictedHealth = target.Health + target.HPRegenRate * 2;
                var Rdmg = R.GetDamage(target);
                if (Rdmg > predictedHealth && GetRealDistance(target) > bonusRange() + 400 + target.BoundingRadius )
                {
                    cast = true;
                    PredictionOutput output = R.GetPrediction(target);
                    Vector2 direction = output.CastPosition.To2D() - Player.Position.To2D();
                    direction.Normalize();
                    List<Obj_AI_Hero> enemies = Program.Enemies.Where(x => x.IsValidTarget()).ToList();
                    foreach (var enemy in enemies)
                    {
                        if (enemy.SkinName == target.SkinName || !cast)
                            continue;
                        PredictionOutput prediction = R.GetPrediction(enemy);
                        Vector3 predictedPosition = prediction.CastPosition;
                        Vector3 v = output.CastPosition - Player.ServerPosition;
                        Vector3 w = predictedPosition - Player.ServerPosition;
                        double c1 = Vector3.Dot(w, v);
                        double c2 = Vector3.Dot(v, v);
                        double b = c1 / c2;
                        Vector3 pb = Player.ServerPosition + ((float)b * v);
                        float length = Vector3.Distance(predictedPosition, pb);
                        if (length < (400 + enemy.BoundingRadius) && Player.Distance(predictedPosition) < Player.Distance(target.ServerPosition))
                            cast = false;
                    }
                    if (cast)
                        R.CastOnUnit(target);
                }
            }
        }

        private void LogicW()
        {
            if (Player.Mana > RMANA + WMANA)
            {
                if (Config.Item("autoW").GetValue<bool>())
                    foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && !OktwCommon.CanMove(enemy)))
                        W.Cast(enemy.Position, true);
                
                if (Config.Item("telE").GetValue<bool>())
                    foreach (var Object in ObjectManager.Get<Obj_AI_Base>().Where(Obj => Obj.Distance(Player.ServerPosition) < W.Range  && Obj.Team != Player.Team && (Obj.HasBuff("teleport_target", true) || Obj.HasBuff("Pantheon_GrandSkyfall_Jump", true))))
                        W.Cast(Object.Position, true);
            }
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget(Q.Range))
            {

                float predictedHealth = t.Health + t.HPRegenRate * 2;
                double Qdmg = Q.GetDamage(t);

                if (GetRealDistance(t) > bonusRange() + 150 && Qdmg > predictedHealth && Player.CountEnemiesInRange(400) == 0)
                {
                    Program.CastSpell(Q, t);
                    Program.debug("Q KS");
                }
                else if (Program.Combo && ObjectManager.Player.Mana > RMANA + QMANA + EMANA + 10 && Player.CountEnemiesInRange(bonusRange() + 100 + t.BoundingRadius) == 0 && !Config.Item("autoQ").GetValue<bool>())
                    Program.CastSpell(Q, t);
                if ((Program.Combo || Program.Farm) && Player.Mana > RMANA + QMANA && Player.CountEnemiesInRange(bonusRange()) == 0 && OktwCommon.CanHarras())
                {
                    foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                        Q.Cast(enemy, true);
                    
                    if (t.HasBuffOfType(BuffType.Slow))
                        Program.CastSpell(Q, t);
                    else if (Player.Mana >Player.MaxMana * 0.8 )
                        Program.CastSpell(Q, t);
                }

                if ((Program.Combo || Program.Farm) && Player.CountEnemiesInRange(bonusRange() + 100) == 0 && Player.Mana > RMANA + EMANA + WMANA + QMANA && OktwCommon.CanHarras())
                {
                    Q.CastIfWillHit(t, 2, true);
                }
            }
            else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && ObjectManager.Player.ManaPercentage() > Config.Item("Mana").GetValue<Slider>().Value && Config.Item("farmQ").GetValue<bool>() && ObjectManager.Player.Mana > RMANA + QMANA + EMANA + WMANA)
            {
                var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All);
                var Qfarm = Q.GetLineFarmLocation(allMinionsQ, 100);
                if (Qfarm.MinionsHit > 5 )
                    Q.Cast(Qfarm.Position);
            }
        }

        private void LogicE()
        {
            var t = TargetSelector.GetTarget(E.Range - 100, TargetSelector.DamageType.Physical);

            var t2 = TargetSelector.GetTarget(1100, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget() && Config.Item("autoE").GetValue<bool>() &&  Player.Position.Extend(Game.CursorPos, 400).CountEnemiesInRange(800) < 3)
            {
                var eDmg = E.GetDamage(t);
                float predictedHealth = HealthPrediction.GetHealthPrediction(t, (int)(R.Delay + (Player.Distance(t.ServerPosition) / Q.Speed) * 1000));
                double Qdmg = Q.GetDamage(t);

                if (Qdmg + eDmg > t.Health
                    && Qdmg < t.Health && ObjectManager.Player.Mana > EMANA + QMANA && Q.IsReady()
                    && t.Position.Distance(ObjectManager.Player.ServerPosition) > t.Position.Distance(ObjectManager.Player.Position)
                    && ObjectManager.Player.Position.Distance(t.ServerPosition) < ObjectManager.Player.Position.Distance(t.Position))
                {
                    E.Cast(t, true);
                    Program.debug("E + Q combo");
                }
                else if (
                     ObjectManager.Player.Mana > RMANA + EMANA
                    && ObjectManager.Player.CountEnemiesInRange(200) > 0
                    && ObjectManager.Player.Position.Extend(Game.CursorPos, 400).CountEnemiesInRange(500) < 3
                    && t2.Position.Distance(Game.CursorPos) > t2.Position.Distance(ObjectManager.Player.Position))
                {

                    var position = ObjectManager.Player.ServerPosition - (Game.CursorPos - ObjectManager.Player.ServerPosition);
                    E.Cast(position, true);
                    Program.debug("E mele escape");
                }

                else if (ObjectManager.Player.Mana > RMANA + EMANA && GetRealDistance(t) < 500 && ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.3)
                    E.Cast(t, true);
            }

            if (Config.Item("useE").GetValue<KeyBind>().Active)
            {
                var position = ObjectManager.Player.ServerPosition - (Game.CursorPos - ObjectManager.Player.ServerPosition);
                E.Cast(position, true);
            }
        }
        private float GetRealRange(GameObject target)
        {
            return 680f + Player.BoundingRadius + target.BoundingRadius;
        }
        private float GetRealDistance(GameObject target)
        {
            return Player.ServerPosition.Distance(target.Position) + ObjectManager.Player.BoundingRadius + target.BoundingRadius;
        }
        public float bonusRange()
        {
            return 720f + Player.BoundingRadius;
        }
        private void SetMana()
        {
            if (Player.Health < Player.MaxHealth * 0.2)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
                return;
            }

            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R.IsReady())
                RMANA = QMANA - Player.PARRegenRate * Q.Instance.Cooldown;
            else
                RMANA = R.Instance.ManaCost;
        }

        public static void drawLine(Vector3 pos1, Vector3 pos2, int bold, System.Drawing.Color color)
        {
            var wts1 = Drawing.WorldToScreen(pos1);
            var wts2 = Drawing.WorldToScreen(pos2);

            Drawing.DrawLine(wts1[0], wts1[1], wts2[0], wts2[1], bold, color);
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
            if (Config.Item("noti").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

                if (t.IsValidTarget() && R.IsReady())
                {
                    var rDamage = R.GetDamage(t);
                    if (rDamage > t.Health)
                    {
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Red, "Ult can kill: " + t.ChampionName + " have: " + t.Health + "hp");
                        drawLine(t.Position, Player.Position, 10, System.Drawing.Color.Yellow);
                    }
                }

                var tw = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                if (tw.IsValidTarget())
                {
                    if (Q.GetDamage(tw)> tw.Health)
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.4f, System.Drawing.Color.Red, "Q can kill: " + t.ChampionName + " have: " + t.Health + "hp");
                }
            }
        }
    }
}
