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
    class Lucian
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;

        private Spell E, Q, Q1, R, R1, W, W1;

        private float QMANA, WMANA, EMANA, RMANA;
        private bool passRdy = false;
        private float castR = Game.Time;
        public Obj_AI_Hero Player {get { return ObjectManager.Player; }}

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 675f);
            Q1 = new Spell(SpellSlot.Q, 1100f);
            W = new Spell(SpellSlot.W, 1200);
            E = new Spell(SpellSlot.E, 475f);
            R = new Spell(SpellSlot.R, 1400f);
            R1 = new Spell(SpellSlot.R, 1400f);

            Q1.SetSkillshot(0.40f, 10f, float.MaxValue, true, SkillshotType.SkillshotLine);
            Q.SetTargetted(0.25f, 1400f);
            W.SetSkillshot(0.30f, 80f, 1600f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.1f, 110, 2800, true, SkillshotType.SkillshotLine);
            R1.SetSkillshot(0.1f, 110, 2800, false, SkillshotType.SkillshotLine);

            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("wRange", "W range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range").SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("autoQ", "Auto Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("harasQ", "Use Q on minion").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("autoW", "Auto W").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("ignoreCol", "Ignore collision").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("autoE", "Auto E").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("nktdE", "NoKeyToDash").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("slowE", "Auto SlowBuff E").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("autoR", "Auto R").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("useR", "Semi-manual cast R key").SetValue(new KeyBind('t', KeyBindType.Press))); //32 == space

            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmQ", "LaneClear + jungle Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmW", "LaneClear + jungle  W").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("Mana", "LaneClear + jungle  Mana").SetValue(new Slider(80, 100, 30)));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                Config.SubMenu(Player.ChampionName).SubMenu("Harras").AddItem(new MenuItem("harras" + enemy.ChampionName, enemy.ChampionName).SetValue(true));
            
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += afterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Spellbook.OnCastSpell +=Spellbook_OnCastSpell;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if ( E.IsReady() && ObjectManager.Player.Position.Extend(Game.CursorPos, E.Range).CountEnemiesInRange(400) < 3)
            {
                var Target = (Obj_AI_Hero)gapcloser.Sender;
                if (Target.IsValidTarget(E.Range))
                {
                    E.Cast(ObjectManager.Player.Position.Extend(Game.CursorPos, E.Range), true);
                }
            }
            return;
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.Q || args.Slot == SpellSlot.W || args.Slot == SpellSlot.E)
            {
                passRdy = true;
            }
        }
       
        private void afterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
                return;
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "LucianW" || args.SData.Name == "LucianE" || args.SData.Name == "LucianQ")
                {
                    passRdy = true;
                    Utility.DelayAction.Add(450, Orbwalking.ResetAutoAttackTimer);
                }
                else
                    passRdy = false;

                if (args.SData.Name == "LucianR")
                    castR = Game.Time;
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (R1.IsReady() && Game.Time - castR > 5 && Config.Item("useR").GetValue<KeyBind>().Active)
            {
                var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget(R1.Range))
                {
                    R1.Cast(t);
                    return;
                }
            }
            if (Program.LagFree(0))
            {
                SetMana();
                
            }
            if (Program.LagFree(1) && Q.IsReady() && !passRdy && !SpellLock )
                LogicQ();
            if (Program.LagFree(2) && W.IsReady() && !passRdy && !SpellLock && Config.Item("autoW").GetValue<bool>())
                LogicW();
            if (Program.LagFree(3) && E.IsReady() )
                LogicE();
            if (Program.LagFree(4))
            {
                if (R.IsReady() && Game.Time - castR > 5)
                    LogicR();

                if (!passRdy && !SpellLock)
                farm();
            }
        }

        private double AaDamage(Obj_AI_Hero target)
        {
            if (Player.Level > 12)
                return Player.GetAutoAttackDamage(target) * 1.3;
            else if (Player.Level > 6)
                return Player.GetAutoAttackDamage(target) * 1.4;
            else if (Player.Level > 0)
                return Player.GetAutoAttackDamage(target) * 1.5;
            return 0;
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var t1 = TargetSelector.GetTarget(Q1.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget(Q.Range))
            {
                if (Q.GetDamage(t) + AaDamage(t) > t.Health)
                    Q.Cast(t);
                else if (Program.Combo && Player.Mana > RMANA + QMANA)
                    Q.Cast(t);
                else if (Program.Farm && Config.Item("harras" + t.ChampionName).GetValue<bool>() && Player.Mana > RMANA + QMANA + EMANA + WMANA)
                    Q.Cast(t);
            }
            else if ((Program.Farm || Program.Combo) && Config.Item("harasQ").GetValue<bool>() && t1.IsValidTarget(Q1.Range) && Config.Item("harras" + t1.ChampionName).GetValue<bool>() && Player.Distance(t1.ServerPosition) > Q.Range + 100)
            {
                if (Program.Combo && Player.Mana < RMANA + QMANA)
                    return;
                if (Program.Farm && Player.Mana < RMANA + QMANA + EMANA + WMANA )
                    return;
                if (!OktwCommon.CanHarras())
                    return;
                var prepos = Prediction.GetPrediction(t1, Q1.Delay); 
                if ((int)prepos.Hitchance < 5)
                    return;
                var distance = Player.Distance(prepos.CastPosition);
                var minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
                
                foreach (var minion in minions.Where(minion => minion.IsValidTarget(Q.Range)))
                {
                    if (prepos.CastPosition.Distance(Player.Position.Extend(minion.Position, distance)) < 25)
                    {
                        Q.Cast(minion);
                        return;
                    }
                }
            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                if (Config.Item("ignoreCol").GetValue<bool>() && Orbwalking.InAutoAttackRange(t))
                    W.Collision=false;
                else
                    W.Collision=true;

                var qDmg = Q.GetDamage(t);
                var wDmg = W.GetDamage(t);
                if (Orbwalking.InAutoAttackRange(t))
                {
                    qDmg += (float)AaDamage(t);
                    wDmg += (float)AaDamage(t);
                }
                if (wDmg > t.Health) 
                    Program.CastSpell(W, t);
                else if (wDmg + qDmg > t.Health && Q.IsReady() && Player.Mana > RMANA + WMANA + QMANA)
                    Program.CastSpell(W, t);
                else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Player.Mana > RMANA + WMANA + EMANA + QMANA)
                    Program.CastSpell(W, t);
                else if (Program.Farm && Config.Item("harras" + t.ChampionName).GetValue<bool>() && !Player.UnderTurret(true) && Player.Mana > Player.MaxMana * 0.8 && Player.Mana > RMANA + WMANA + EMANA + QMANA + WMANA)
                    Program.CastSpell(W, t);
                else if ((Program.Combo || Program.Farm) && Player.Mana > RMANA + WMANA + EMANA)
                {
                    foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && !OktwCommon.CanMove(enemy)))
                        W.Cast(enemy, true);
                }
            }
        }
        
        private void LogicR()
        {
            var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

            if (t.IsValidTarget(R.Range) && t.CountAlliesInRange(500) == 0 && Program.ValidUlt(t) && !Orbwalking.InAutoAttackRange(t))
            {
                var rDmg = R.GetDamage(t,1) * NumShots();

                var tDis = Player.Distance(t.ServerPosition);
                if (rDmg * 0.8 > t.Health && tDis < 800 && !Q.IsReady())
                    R.Cast(t, true, true);
                else if (rDmg * 0.7 > t.Health && tDis < 900)
                    R.Cast(t, true, true);
                else if (rDmg * 0.6 > t.Health && tDis < 1000)
                    R.Cast(t, true, true);
                else if (rDmg * 0.5 > t.Health && tDis < 1100)
                    R.Cast(t, true, true);
                else if (rDmg * 0.4 > t.Health && tDis < 1200)
                    R.Cast(t, true, true);
                else if (rDmg * 0.3 > t.Health && tDis < 1300)
                    R.Cast(t, true, true);
                return;
            }
        }

        private void LogicE()
        {
            var dashPosition = Player.Position.Extend(Game.CursorPos, E.Range);
            if (dashPosition.IsWall() || dashPosition.CountEnemiesInRange(800) < 3)
                return;
            if (Game.CursorPos.Distance(Player.Position) > Player.AttackRange + Player.BoundingRadius * 2 && Program.Combo && Config.Item("nktdE").GetValue<bool>() && Player.Mana > RMANA + EMANA - 10)
            {
                if (!passRdy && !SpellLock)
                    E.Cast(Game.CursorPos);
                else if (!Orbwalker.GetTarget().IsValidTarget())
                    E.Cast(Game.CursorPos);
            }

            if ( Player.Mana < RMANA + EMANA || !Config.Item("autoE").GetValue<bool>() || passRdy || SpellLock)
                return;

            foreach (var target in Program.Enemies.Where(target => target.IsValidTarget(270) && target.IsMelee))
            {
                if (target.Position.Distance(Game.CursorPos) > target.Position.Distance(Player.Position))
                    E.Cast(dashPosition, true);
            }

            if (Config.Item("slowE").GetValue<bool>() && Player.HasBuffOfType(BuffType.Slow))
            {
                E.Cast(dashPosition, true);
            }
        }


        public void farm()
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                if (mobs.Count > 0 && Player.Mana > RMANA + WMANA + EMANA + QMANA)
                {
                    var mob = mobs[0];
                    if (Q.IsReady() && Config.Item("farmQ").GetValue<bool>())
                    {
                        Q.Cast(mob);
                        return;
                    }

                    if (W.IsReady() && Config.Item("farmW").GetValue<bool>())
                    {
                        W.Cast(mob);
                        return;
                    }
                }

                if (Player.ManaPercent > Config.Item("Mana").GetValue<Slider>().Value)
                {
                    var minions = MinionManager.GetMinions(Player.ServerPosition, Q1.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
                    if (Q.IsReady() && Config.Item("farmQ").GetValue<bool>())
                    {
                        foreach (var minion in minions)
                        {
                            var poutput = Q1.GetPrediction(minion);
                            var col = poutput.CollisionObjects;
                            
                            if (col.Count() > 2)
                            {
                                var minionQ = col.First();
                                if (minionQ.IsValidTarget(Q.Range))
                                {
                                    Q.Cast(minion);
                                    return;
                                }
                            }
                        }
                    }
                    if (W.IsReady() && Config.Item("farmW").GetValue<bool>())
                    {
                        var Wfarm = W.GetCircularFarmLocation(minions, 150);
                        if (Wfarm.MinionsHit > 3 )
                            W.Cast(Wfarm.Position);
                    }
                }
            }
        }

        private double NumShots()
        {
            double num = 7.5;
            if (R.Level == 1)
                num += 7.5 * Player.AttackSpeedMod * 0.5;
            else if (R.Level == 2)
                num += 9 * Player.AttackSpeedMod * 0.5;
            else if (R.Level == 3)
                num += 10.5 * Player.AttackSpeedMod * 0.5;
            return num ;
        }

        private bool SpellLock
        {
            get
            {
                if (Player.HasBuff("lucianpassivebuff"))
                    return true;
                else
                    return false;
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
        public static void drawText(string msg, Vector3 Hero, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(Hero);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1] - 200, color, msg);
        }
        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("watermark").GetValue<bool>())
            {
                Drawing.DrawText(Drawing.Width * 0.2f, Drawing.Height * 0f, System.Drawing.Color.Cyan, "OneKeyToWin AIO - " + Player.ChampionName + " by Sebby");
            }
            if (Config.Item("nktdE").GetValue<bool>())
            {
                if (Game.CursorPos.Distance(Player.Position) > Player.AttackRange + Player.BoundingRadius * 2)
                    drawText("dash: ON ", Player.Position, System.Drawing.Color.Red);
                else
                    drawText("dash: OFF ", Player.Position, System.Drawing.Color.GreenYellow);
            }

            if (Config.Item("qRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>())
                {
                    if (Q.IsReady())
                        Utility.DrawCircle(ObjectManager.Player.Position, Q1.Range, System.Drawing.Color.Cyan, 1, 1);
                }
                else
                    Utility.DrawCircle(ObjectManager.Player.Position, Q1.Range, System.Drawing.Color.Cyan, 1, 1);
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
                        Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Orange, 1, 1);
                }
                else
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Orange, 1, 1);
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
