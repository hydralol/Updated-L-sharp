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
    class Draven
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell E, Q, R, W;
        private float QMANA, WMANA, EMANA, RMANA;
        private int axeCatchRange;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private static GameObject RMissile = null;
        public List<GameObject> axeList = new List<GameObject>();

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1000);
            R = new Spell(SpellSlot.R, 3000f);

            E.SetSkillshot(0.25f, 100, 1400, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.4f, 160, 2000, false, SkillshotType.SkillshotLine);

            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("noti", "Draw R helper").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qCatchRange", "Q catch range").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qAxePos", "Q axe position").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range").SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("AXE option").AddItem(new MenuItem("axeCatchRange", "Axe catch range").SetValue(new Slider(500, 200, 2000)));
            Config.SubMenu(Player.ChampionName).SubMenu("AXE option").AddItem(new MenuItem("axeTower", "Don't catch axe under enemy turret combo").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("AXE option").AddItem(new MenuItem("axeTower2", "Don't catch axe under enemy turret farm").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("AXE option").AddItem(new MenuItem("axeEnemy", "Don't catch axe in enemy grup").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("AXE option").AddItem(new MenuItem("axeKill", "Don't catch axe if can kill 2 AA").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("AXE option").AddItem(new MenuItem("axePro", "if axe timeout: force laneclear").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Q config").AddItem(new MenuItem("autoQ", "Auto Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q config").AddItem(new MenuItem("farmQ", "Farm Q").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("W config").AddItem(new MenuItem("autoW", "Auto W").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W config").AddItem(new MenuItem("slowW", "Auto W slow").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("E config").AddItem(new MenuItem("autoE", "Auto E").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E config").AddItem(new MenuItem("autoE2", "Harras E if can hit 2 targets").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("autoR", "Auto R").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("comboR", "Auto R in combo").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("Rcc", "R cc").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("Raoe", "R aoe combo").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("hitchanceR", "VeryHighHitChanceR").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("useR", "Semi-manual cast R key").SetValue(new KeyBind('t', KeyBindType.Press))); //32 == space

            Obj_SpellMissile.OnCreate += SpellMissile_OnCreateOld;
            Obj_SpellMissile.OnDelete += Obj_SpellMissile_OnDelete;
            Orbwalking.BeforeAttack += BeforeAttack;
            GameObject.OnCreate += GameObjectOnOnCreate;
            GameObject.OnDelete += GameObjectOnOnDelete;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += GameOnOnUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (E.IsReady() && sender.IsValidTarget(E.Range))
            {
                E.Cast(sender);
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range))
            {
                E.Cast(gapcloser.Sender);
            }
        }

        private void Obj_SpellMissile_OnDelete(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid<MissileClient>())
                return;
            MissileClient missile = (MissileClient)sender;

            if (missile.IsValid && missile.IsAlly && missile.SData.Name != null && missile.SData.Name == "DravenR")
            {
                RMissile = null;
            }
        }

        private void SpellMissile_OnCreateOld(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid<MissileClient>())
                return;

            MissileClient missile = (MissileClient)sender;

            if (missile.IsValid && missile.IsAlly && missile.SData.Name != null && missile.SData.Name == "DravenR")
            {
                RMissile = sender;
            }
        }

        private void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            //Program.debug("" + OktwCommon.GetBuffCount(Player, "dravenspinningattack"));
            if (Q.IsReady())
            {
                var buffCount = OktwCommon.GetBuffCount(Player, "dravenspinningattack");
                if (Config.Item("autoQ").GetValue<bool>() && args.Target.IsValid<Obj_AI_Hero>()  )
                {
                    if (buffCount + axeList.Count == 0)
                        Q.Cast();
                    else if (Player.Mana > RMANA + QMANA && buffCount == 0)
                        Q.Cast();
                }
                if (Program.Farm && Config.Item("farmQ").GetValue<bool>()  )
                {
                    if (buffCount + axeList.Count == 0 && Player.Mana > RMANA + QMANA + EMANA + WMANA)
                        Q.Cast();
                    else if (Player.Mana > Player.MaxMana * 0.95 && buffCount == 0)
                        Q.Cast();
                }
            }
        }

        private void GameObjectOnOnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Q_reticle_self"))
            {
                axeList.Add(sender);
            }
        }

        private void GameObjectOnOnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Q_reticle_self"))
            {
                axeList.Remove(sender);
            }
        }

        private void GameOnOnUpdate(EventArgs args)
        {
            axeList.RemoveAll(x => !x.IsValid);
            if (ObjectManager.Player.HasBuff("Recall"))
                return;
            if (Program.LagFree(1))
            {
                axeCatchRange = Config.Item("axeCatchRange").GetValue<Slider>().Value;
                SetMana();
                AxeLogic();
                if (Config.Item("axePro").GetValue<bool>() && Player.HasBuff("dravenspinningattack"))
                {
                    var BuffTime = OktwCommon.GetPassiveTime(Player, "dravenspinningattack");
                    if (BuffTime < 1)
                    {
                        Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.LaneClear;
                    }
                    else
                    {
                        Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.None;
                    }
                }
                else
                {
                    Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.None;
                }
            }
            
            //Program.debug("" + OktwCommon.GetBuffCount(Player, "dravenspinningattack"));
            
            if (Program.LagFree(2) && E.IsReady() && Config.Item("autoE").GetValue<bool>() )
                LogicE();

            if (Program.LagFree(3) && W.IsReady())
                LogicW();

            if (Program.LagFree(4) && R.IsReady() && !Player.IsWindingUp)
                LogicR();
        }

        private void LogicW()
        {
            if (Config.Item("autoW").GetValue<bool>() && Program.Combo && Player.Mana > RMANA + EMANA + WMANA + QMANA && Player.CountEnemiesInRange(1000) > 0 && !Player.HasBuff("dravenfurybuff"))
                W.Cast();
            else if (Config.Item("slowW").GetValue<bool>()&& Player.Mana > RMANA + EMANA + WMANA && Player.HasBuffOfType(BuffType.Slow))
                W.Cast();
        }

        private void LogicE()
        {
            foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && !Orbwalking.InAutoAttackRange(enemy) && E.GetDamage(enemy) > enemy.Health))
            {
                Program.CastSpell(E, enemy);
                return;
            }

            var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                if (Program.Combo )
                {
                    if (Player.Mana > RMANA + EMANA)
                    {
                        if (!Orbwalking.InAutoAttackRange(t))
                            Program.CastSpell(E, t);
                        if (Player.Health < Player.MaxHealth * 0.5)
                            Program.CastSpell(E, t);
                    }
                    
                    if(Player.Mana > RMANA + EMANA + QMANA)
                        E.CastIfWillHit(t, 2, true);
                }
                if (Program.Farm && Config.Item("autoE2").GetValue<bool>() && Player.Mana > RMANA + EMANA + WMANA + QMANA)
                {
                    E.CastIfWillHit(t, 2, true);
                }
            }
            foreach (var target in Program.Enemies.Where(target => target.IsValidTarget(E.Range)))
            {
                if (target.IsValidTarget(300) && target.IsMelee)
                {
                    Program.CastSpell(E, t);
                }
            }
        }

        private void LogicR()
        {
            if (Config.Item("useR").GetValue<KeyBind>().Active)
            {
                var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

                if (t.IsValidTarget())
                {
                    R.CastIfWillHit(t, 2, true);
                    R.Cast(t, true, true);
                }
            }
            if (Config.Item("autoR").GetValue<bool>())
            {
                foreach (var target in Program.Enemies.Where(target => target.IsValidTarget(R.Range) && Program.ValidUlt(target) && target.CountAlliesInRange(500) == 0))
                {
                    float predictedHealth = target.Health;
                    double Rdmg = CalculateR(target) ;
                    if (Rdmg * 2 > predictedHealth )
                        Rdmg = Rdmg + getRdmg(target);
                    var qDmg = Q.GetDamage(target);
                    var eDmg = E.GetDamage(target);
                    if (Rdmg > predictedHealth && !Orbwalking.InAutoAttackRange(target))
                    {
                        castR(target);
                        Program.debug("R normal");
                    }
                    else if (Program.Combo && Config.Item("comboR").GetValue<bool>() && Orbwalking.InAutoAttackRange(target) && Rdmg * 2 > predictedHealth)
                    {
                        castR(target);
                        Program.debug("R normal");
                    }
                    else if (!OktwCommon.CanMove(target) && Config.Item("Rcc").GetValue<bool>() &&
                        target.IsValidTarget( E.Range) && Rdmg * 2 > predictedHealth)
                    {
                        R.CastIfWillHit(target, 2, true);
                        Program.debug("R normal");
                    }
                    else if (Program.Combo && Config.Item("Raoe").GetValue<bool>())
                    {
                        R.CastIfWillHit(target, 3, true);
                    }
                    else if (target.IsValidTarget(E.Range) && Rdmg * 2 + qDmg + eDmg > predictedHealth && Config.Item("Raoe").GetValue<bool>())
                    {
                        R.CastIfWillHit(target, 2, true);
                    }
                }
            }
        }

        private void castR(Obj_AI_Hero target)
        {
            if (Config.Item("hitchanceR").GetValue<bool>())
            {
                List<Vector2> waypoints = target.GetWaypoints();
                if (target.Path.Count() < 2 && (Player.Distance(waypoints.Last<Vector2>().To3D()) - Player.Distance(target.Position)) > 300)
                {
                    Program.CastSpell(R, target);
                }
            }
            else
                Program.CastSpell(R, target);
        }

        private float CalculateR(Obj_AI_Base target)
        {
            return (float)Player.CalcDamage(target, Damage.DamageType.Physical, (75 + (100 * R.Level)) + Player.FlatPhysicalDamageMod * 1.1);
        }

        private double getRdmg(Obj_AI_Base target)
        {
            var rDmg = R.GetDamage(target);
            var dmg = 0;
            PredictionOutput output = R.GetPrediction(target);
            Vector2 direction = output.CastPosition.To2D() - Player.Position.To2D();
            direction.Normalize();
            List<Obj_AI_Hero> enemies = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && x.IsValidTarget()).ToList();
            foreach (var enemy in enemies)
            {
                PredictionOutput prediction = R.GetPrediction(enemy);
                Vector3 predictedPosition = prediction.CastPosition;
                Vector3 v = output.CastPosition - Player.ServerPosition;
                Vector3 w = predictedPosition - Player.ServerPosition;
                double c1 = Vector3.Dot(w, v);
                double c2 = Vector3.Dot(v, v);
                double b = c1 / c2;
                Vector3 pb = Player.ServerPosition + ((float)b * v);
                float length = Vector3.Distance(predictedPosition, pb);
                if (length < (R.Width + 100 + enemy.BoundingRadius / 2) && Player.Distance(predictedPosition) < Player.Distance(target.ServerPosition))
                    dmg++;
            }
            var allMinionsR = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, R.Range, MinionTypes.All);
            foreach (var minion in allMinionsR)
            {
                PredictionOutput prediction = R.GetPrediction(minion);
                Vector3 predictedPosition = prediction.CastPosition;
                Vector3 v = output.CastPosition - Player.ServerPosition;
                Vector3 w = predictedPosition - Player.ServerPosition;
                double c1 = Vector3.Dot(w, v);
                double c2 = Vector3.Dot(v, v);
                double b = c1 / c2;
                Vector3 pb = Player.ServerPosition + ((float)b * v);
                float length = Vector3.Distance(predictedPosition, pb);
                if (length < (R.Width + 100 + minion.BoundingRadius / 2) && Player.Distance(predictedPosition) < Player.Distance(target.ServerPosition))
                    dmg++;
            }
            //if (Config.Item("debug").GetValue<bool>())
            //    Game.PrintChat("R collision" + dmg);

            if (dmg > 8)
                return rDmg * 0.6;
            else
                return rDmg - (rDmg * 0.08 * dmg);
        }

        private void AxeLogic()
        {

            var t = TargetSelector.GetTarget(800, TargetSelector.DamageType.Physical);

            if (Config.Item("axeKill").GetValue<bool>() && t.IsValidTarget() && Player.Distance(t.Position) > 400 && Player.GetAutoAttackDamage(t) * 2 > t.Health)
            {
                Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                return;
            }
            if (axeList.Count == 0)
            {
                Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                return;
            }
            
            if (axeList.Count == 1)
            {
                CatchAxe(axeList.First());
                return;
            }
            else
            {
                var bestAxe = axeList.First();
                foreach (var obj in axeList)
                {
                    if (Game.CursorPos.Distance(bestAxe.Position) > Game.CursorPos.Distance(obj.Position))
                        bestAxe = obj;
                }
                CatchAxe(bestAxe);
            }
        }

        private void CatchAxe(GameObject Axe)
        {
            if (Player.Distance(Axe.Position) < 110)
            {
                Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                return;
            }

            if (Config.Item("axeTower").GetValue<bool>() && Program.Combo && Axe.Position.UnderTurret(true))
            {
                Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                return;
            }

            if (Config.Item("axeTower2").GetValue<bool>() && Program.Farm && Axe.Position.UnderTurret(true))
            {
                Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                return;
            }

            if (Config.Item("axeEnemy").GetValue<bool>() && Axe.Position.CountEnemiesInRange(500) > 2)
            {
                Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                return;
            }

            if (Game.CursorPos.Distance(Axe.Position) < axeCatchRange)
            {
                Orbwalker.SetOrbwalkingPoint(Axe.Position);
            }
            else
            {
                Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
            }
        }

        private void SetMana()
        {
            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;
            if (!R.IsReady())
                RMANA = EMANA - Player.PARRegenRate * E.Instance.Cooldown;
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

        public static void drawText2(string msg, Vector3 Hero, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(Hero);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1] - 200, color, msg);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("qAxePos").GetValue<bool>())
            {
                if (Player.HasBuff("dravenspinningattack"))
                {
                    var BuffTime = OktwCommon.GetPassiveTime(Player, "dravenspinningattack");
                    if (BuffTime < 2 )
                    {
                        if ((int)(Game.Time * 10) % 2 == 0)
                        {
                            drawText2("Q:  " + String.Format("{0:0.0}", BuffTime), Player.Position, System.Drawing.Color.Yellow);
                        }
                    }
                    else
                    {
                        drawText2("Q:  " + String.Format("{0:0.0}", BuffTime), Player.Position, System.Drawing.Color.GreenYellow);
                    }
                }
                foreach (var obj in axeList)
                {
                    if (Game.CursorPos.Distance(obj.Position) > axeCatchRange || obj.Position.UnderTurret(true))
                    {
                        Utility.DrawCircle(obj.Position, 150, System.Drawing.Color.OrangeRed, 1, 1);
                    }
                    else if (Player.Distance(obj.Position) > 120)
                    {
                        Utility.DrawCircle(obj.Position, 150, System.Drawing.Color.Yellow, 1, 1);
                    }
                    else if (Player.Distance(obj.Position) < 150)
                    {
                        Utility.DrawCircle(obj.Position, 150, System.Drawing.Color.YellowGreen, 1, 1);
                    }
                }
            }

            if (Config.Item("qCatchRange").GetValue<bool>())
                Utility.DrawCircle(Game.CursorPos, axeCatchRange, System.Drawing.Color.LightSteelBlue, 1, 1);
            
            if (Config.Item("noti").GetValue<bool>() && RMissile != null)
                OktwCommon.DrawLineRectangle(RMissile.Position, Player.Position, (int)R.Width, 1, System.Drawing.Color.White);

            if (Config.Item("eRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>())
                {
                    if (E.IsReady())
                        Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Yellow, 1, 1);
                }
                else
                    Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Yellow, 1, 1);
            }
        }
    }
}
