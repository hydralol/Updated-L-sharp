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
    class Graves
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell E, Q, Q1, R, W , R1;
        private float QMANA, WMANA, EMANA, RMANA;

        public bool Esmart = false;
        public float OverKill = 0;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; }}

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 850);
            W = new Spell(SpellSlot.W, 940f);
            E = new Spell(SpellSlot.E, 450f);
            R = new Spell(SpellSlot.R, 1000f);
            R1 = new Spell(SpellSlot.R, 1500f);

            Q.SetSkillshot(0.25f, 50f, 2000f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.35f, 150f, 1650f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 120f, 2100f, false, SkillshotType.SkillshotLine);
            R1.SetSkillshot(0.25f, 100f, 2100f, false, SkillshotType.SkillshotLine);

            LoadMenuOKTW();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Orbwalking.AfterAttack += Orbwalker_AfterAttack;
        }

        private void LoadMenuOKTW()
        {
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("wRange", "W range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells").SetValue(true));

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Config.SubMenu(Player.ChampionName).SubMenu("Haras").AddItem(new MenuItem("haras" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Q config").AddItem(new MenuItem("autoQ", "Auto Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q config").AddItem(new MenuItem("Qafter", "Q only after AA").SetValue(true));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Config.SubMenu(Player.ChampionName).SubMenu("Q config").SubMenu("Harras").AddItem(new MenuItem("haras" + enemy.ChampionName, enemy.ChampionName).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("W config").AddItem(new MenuItem("autoW", "Auto W").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("E config").AddItem(new MenuItem("autoE", "Auto E").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E config").AddItem(new MenuItem("smartE", "SmartCast E key").SetValue(new KeyBind('t', KeyBindType.Press))); //32 == space

            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("autoR", "Auto R").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("fastR", "Fast R ks Combo").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("useR", "Semi-manual cast R key").SetValue(new KeyBind('t', KeyBindType.Press))); //32 == space

            Config.SubMenu(Player.ChampionName).SubMenu("AntiGapcloser").AddItem(new MenuItem("AGCE", "AntiGapcloserE").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("AntiGapcloser").AddItem(new MenuItem("AGCW", "AntiGapcloserW").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmQ", "Lane clear Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("Mana", "LaneClear Mana").SetValue(new Slider(80, 100, 30)));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleQ", "Jungle clear Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleW", "Jungle clear W").SetValue(true));
        }

        public void Orbwalker_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
                return;
            Jungle();
            if (Q.IsReady() && Config.Item("Qafter").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                {
                    if (Program.Combo && ObjectManager.Player.Mana > RMANA + QMANA)
                        Program.CastSpell(Q, t);
                    else if ((Program.Farm && Config.Item("haras" + t.ChampionName).GetValue<bool>() && ObjectManager.Player.Mana > RMANA + EMANA + WMANA + QMANA + QMANA) && t.IsValidTarget(Q.Range - 100) && Config.Item("haras" + t.BaseSkinName).GetValue<bool>())
                        Program.CastSpell(Q, t);
                }
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (ObjectManager.Player.Mana > RMANA + EMANA )
            {
                var Target = (Obj_AI_Hero)gapcloser.Sender;
                if (Target.IsValidTarget(E.Range) )
                {
                    if (E.IsReady() && Config.Item("AGCE").GetValue<bool>() && ObjectManager.Player.Position.Extend(Game.CursorPos, E.Range).CountEnemiesInRange(400) < 3)
                    {
                        E.Cast(ObjectManager.Player.Position.Extend(Game.CursorPos, E.Range), true);
                        Program.debug("E AGC");
                    }
                    else if (W.IsReady() && Config.Item("AGCW").GetValue<bool>())
                    {
                        W.Cast(gapcloser.End);
                        Program.debug("W AGC");
                    }
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            
            if (Config.Item("useR").GetValue<KeyBind>().Active && R.IsReady())
            {
                var t = TargetSelector.GetTarget(1800, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                    R1.Cast(t, true);
            }

            if (E.IsReady())
            {
                if (Config.Item("smartE").GetValue<KeyBind>().Active)
                    Esmart = true;

                if (Esmart && ObjectManager.Player.Position.Extend(Game.CursorPos, E.Range).CountEnemiesInRange(500) < 4)
                    E.Cast(ObjectManager.Player.Position.Extend(Game.CursorPos, E.Range), true);
            }
            else
                Esmart = false;


            if (Program.LagFree(0))
            {
                SetMana();
            }
            if (Program.LagFree(1) && E.IsReady() && !Player.IsWindingUp && Config.Item("autoE").GetValue<bool>())
                LogicE();
            if (Program.LagFree(2) && Q.IsReady() && !Player.IsWindingUp && Config.Item("autoQ").GetValue<bool>())
                LogicQ();
            if (Program.LagFree(3) && W.IsReady() && !Player.IsWindingUp && Config.Item("autoW").GetValue<bool>())
                LogicW();
            if (Program.LagFree(4) && R.IsReady() && !Player.IsWindingUp && Config.Item("autoR").GetValue<bool>())
                LogicR();
        }

        private void Jungle()
        {
            if (Program.LaneClear && Player.Mana > RMANA + WMANA + QMANA + EMANA)
            {
                var mobs = MinionManager.GetMinions(Player.ServerPosition, 600, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (Q.IsReady() && Config.Item("jungleQ").GetValue<bool>() )
                    {
                        Q.Cast(mob.Position);
                        return;
                    }
                    if (W.IsReady() && Config.Item("jungleW").GetValue<bool>())
                    {
                        W.Cast(mob.Position);
                        return;
                    }
                }
            }
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                if (Program.GetRealDmg(Q, t) > t.Health)
                {
                    Q.Cast(t, true);
                    OverKill = Game.Time;
                    Program.debug("Q ks");
                }

                else if (Program.GetRealDmg(Q, t) + Program.GetRealDmg(R, t) > t.Health && R.IsReady())
                {
                    Program.CastSpell(Q, t);
                    if (Config.Item("fastR").GetValue<bool>() && Program.GetRealDmg(Q, t) < t.Health)
                        Program.CastSpell(R, t);
                    Program.debug("Q + R ks");
                }

                else if (!Config.Item("Qafter").GetValue<bool>())
                {
                    if (Program.Combo && Player.Mana > RMANA + QMANA)
                        Program.CastSpell(Q, t);
                    else if ((Program.Farm && Config.Item("haras" + t.ChampionName).GetValue<bool>() && Player.Mana > RMANA + EMANA + WMANA + QMANA + QMANA) && t.IsValidTarget(Q.Range - 100) && Config.Item("haras" + t.BaseSkinName).GetValue<bool>())
                        Program.CastSpell(Q, t);
                }

                if ((Program.Combo || Program.Farm) && Player.Mana > RMANA + QMANA + EMANA)
                {
                    foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                        Q.Cast(enemy, true, true);
                }
            }
            else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && ObjectManager.Player.ManaPercentage() > Config.Item("Mana").GetValue<Slider>().Value && Config.Item("farmQ").GetValue<bool>() && ObjectManager.Player.Mana > RMANA + QMANA + EMANA + WMANA)
            {
                var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All);
                var Qfarm = Q.GetCircularFarmLocation(allMinionsQ, 200);
                if (Qfarm.MinionsHit > 3)
                    Q.Cast(Qfarm.Position);
            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                if (Program.GetRealDmg(W, t) > t.Health)
                {
                    W.Cast(t, true, true);
                    return;
                }
                else if (W.GetDamage(t) + Q.GetDamage(t) > t.Health && Player.Mana > QMANA + WMANA + RMANA)
                {
                    W.Cast(t, true, true);
                }
                else if (Program.Combo)
                {
                    if (ObjectManager.Player.Mana > RMANA + WMANA + QMANA + EMANA && !Orbwalking.InAutoAttackRange(t))
                        W.Cast(t, true, true);
                    else if (Program.Combo && ObjectManager.Player.Mana > RMANA + QMANA + WMANA && ObjectManager.Player.CountEnemiesInRange(300) > 0)
                        W.Cast(t, true, true);
                    else if (Program.Combo && ObjectManager.Player.Mana > RMANA + QMANA + WMANA && t.CountEnemiesInRange(250) > 1)
                        W.Cast(t, true, true);
                    else if (ObjectManager.Player.Mana > RMANA + WMANA + QMANA + EMANA)
                    {
                        foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && !OktwCommon.CanMove(enemy)))
                            W.Cast(enemy, true, true);
                    }
                }
            }
        }

        private void LogicE()
        {
            if (Player.Position.Extend(Game.CursorPos, E.Range).CountEnemiesInRange(500) > 2 && ObjectManager.Player.Mana < RMANA + EMANA)
                return;
            var t = TargetSelector.GetTarget(E.Range + Q.Range, TargetSelector.DamageType.Physical);
            var t2 = TargetSelector.GetTarget(900f, TargetSelector.DamageType.Physical);
            if (ObjectManager.Player.CountEnemiesInRange(240) > 0
                && t.Position.Distance(Game.CursorPos) > t.Position.Distance(ObjectManager.Player.Position))
                E.Cast(ObjectManager.Player.Position.Extend(Game.CursorPos, E.Range), true);
            else if (E.IsReady() && (Game.Time - OverKill > 0.4) && ObjectManager.Player.Position.Extend(Game.CursorPos, E.Range).CountEnemiesInRange(700) < 3 && Orbwalker.ActiveMode.ToString() == "Combo" && ObjectManager.Player.Health > ObjectManager.Player.MaxHealth * 0.4 && !ObjectManager.Player.UnderTurret(true))
            {
                if (t.IsValidTarget()
                && Q.IsReady()
                && Q.GetDamage(t) + ObjectManager.Player.GetAutoAttackDamage(t) * 3 > t.Health
                && t.Position.Distance(Game.CursorPos) + 200 < t.Position.Distance(ObjectManager.Player.Position)
                && !Orbwalking.InAutoAttackRange(t)
                )
                {
                    E.Cast(Game.CursorPos, true);
                    Program.debug("E + aa + Q");
                }
                else if (t2.IsValidTarget()
                 && ObjectManager.Player.GetAutoAttackDamage(t2) * 2 > t2.Health
                 && !Orbwalking.InAutoAttackRange(t2)
                 && t2.Position.Distance(Game.CursorPos) + 200 < t2.Position.Distance(ObjectManager.Player.Position)
                 )
                {
                    E.Cast(Game.CursorPos, true);
                    Program.debug("E + aa");
                }
            }
        }

        private void LogicR()
        {
            bool cast = false;
            double secoundDmgR = 0.80;
            foreach (var target in Program.Enemies.Where(target => target.IsValidTarget(R1.Range) && Program.ValidUlt(target)))
            {

                float predictedHealth = target.Health + target.HPRegenRate ;
                double Rdmg = Program.GetRealDmg(R,target) + (R.GetDamage(target) * target.CountAlliesInRange(400) * 0.2);
                var collisionTarget = target;
                cast = true;
                PredictionOutput output = R.GetPrediction(target);
                Vector2 direction = output.CastPosition.To2D() - Player.Position.To2D();
                direction.Normalize();
                List<Obj_AI_Hero> enemies = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && x.IsValidTarget()).ToList();
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
                    if (length < (120 + enemy.BoundingRadius) && Player.Distance(predictedPosition) < Player.Distance(target.ServerPosition))
                    {
                        cast = false;
                        collisionTarget = enemy;
                    }
                }
                if (cast
                    && Rdmg > predictedHealth
                    && target.IsValidTarget(R.Range)
                    && (!Orbwalking.InAutoAttackRange(target) || ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.6))
                {
                    Program.CastSpell(R, target);
                    Program.debug("Rdmg");
                }
                else if (cast
                    && Rdmg * secoundDmgR > predictedHealth
                    && target.IsValidTarget(R1.Range)
                    && target.CountAlliesInRange(300) == 0 && (!Orbwalking.InAutoAttackRange(target) || ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.6))
                {
                    Program.CastSpell(R, target);
                    Program.debug("Rdmg 0.7");
                }
                else if (!cast && Rdmg * secoundDmgR > predictedHealth && target.IsValidTarget(Player.Distance(collisionTarget.Position) + 700))
                {
                    Program.CastSpell(R, target);
                    Program.debug("Rdmg 0.7 collision");
                }
                else if (cast && Config.Item("fastR").GetValue<bool>() && Rdmg > predictedHealth && Orbwalking.InAutoAttackRange(target) && Program.Combo)
                {
                    Program.CastSpell(R, target);
                    Program.debug("R fast");
                }
                
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
                RMANA = R.Instance.ManaCost; ;

            if (ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.2)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
            }
        }
        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("watermark").GetValue<bool>())
                Drawing.DrawText(Drawing.Width * 0.2f, Drawing.Height * 0f, System.Drawing.Color.Cyan, "OneKeyToWin AIO - " + Player.ChampionName + " by Sebby");

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
        }
    }
}
