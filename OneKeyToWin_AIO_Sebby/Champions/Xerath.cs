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
    class Xerath
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell Q, W, E, R;
        private float QMANA, WMANA, EMANA, RMANA;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private Vector3 Rtarget;
        private float lastR = 0;

        private Items.Item
            FarsightOrb = new Items.Item(3342, 4000f),
            ScryingOrb = new Items.Item(3363, 3500f);

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 1550);
            W = new Spell(SpellSlot.W, 1100);
            E = new Spell(SpellSlot.E, 1050);
            R = new Spell(SpellSlot.R, 675);

            Q.SetSkillshot(0.6f, 100f, float.MaxValue, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.7f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 60f, 1400f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.7f, 120f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            Q.SetCharged("XerathArcanopulseChargeUp", "XerathArcanopulseChargeUp", 700, 1550, 1.5f);

            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("noti", "Show notification & line").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("wRange", "W range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range").SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("autoQ", "Auto Q").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("autoW", "Auto W").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("harrasW", "Harras W").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("autoE", "Auto E").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("harrasE", "Harras E").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("autoR", "Auto R 2 x dmg R").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("useR", "Semi-manual cast R key").SetValue(new KeyBind('t', KeyBindType.Press))); //32 == space
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("trinkiet", "Auto blue trinkiet").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("delayR", "custome R delay ms (1000ms = 1 sec)").SetValue(new Slider(0, 3000, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("MaxRangeR", "Max R range").SetValue(new Slider(5600, 5600, 0)));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                Config.SubMenu(Player.ChampionName).SubMenu("Harras").AddItem(new MenuItem("harras" + enemy.ChampionName, enemy.ChampionName).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("separate", "Separate laneclear from harras").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmQ", "Lane clear Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmW", "Lane clear W").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("Mana", "LaneClear Mana").SetValue(new Slider(80, 100, 30)));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleE", "Jungle clear E").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleQ", "Jungle clear Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleW", "Jungle clear W").SetValue(true));
            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("force", "Force passive use in combo on minion").SetValue(true));

            Game.OnUpdate += Game_OnGameUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.BeforeAttack +=Orbwalking_BeforeAttack;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R )
            {
                if (Config.Item("trinkiet").GetValue<bool>() && !IsCastingR)
                {
                    if (Player.Level < 9)
                        ScryingOrb.Range = 2500;
                    else
                        ScryingOrb.Range = 3500;

                    if (ScryingOrb.IsReady())
                        ScryingOrb.Cast(Rtarget);
                    if(FarsightOrb.IsReady())
                        FarsightOrb.Cast(Rtarget);                  
                }
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "xerathlocuspulse")
                {
                    lastR = Game.Time;
                }
            }
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Target.IsValid<Obj_AI_Minion>() && !Player.HasBuff("xerathascended2onhit") && Program.Combo)
            {
                args.Process = false;
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Player.Distance(gapcloser.Sender.ServerPosition) < E.Range)
            {
                E.Cast(gapcloser.Sender);
            }
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Player.Distance(sender.ServerPosition) < E.Range)
            {
                E.Cast(sender);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            //Program.debug(""+OktwCommon.GetPassiveTime(Player, "XerathArcanopulseChargeUp"));
            if (IsCastingR)
            {
                OktwCommon.blockAttack = true;
                OktwCommon.blockMove = true;
            }
            else
            {
                OktwCommon.blockAttack = false;
                OktwCommon.blockMove = false;
            }

            if (Program.LagFree(1))
            {
                SetMana();
                Jungle();
                if (!Player.HasBuff("xerathascended2onhit"))
                    Orbwalker.ForceTarget(null);
                if (Program.Combo && Config.Item("force").GetValue<bool>() && Player.Mana < Player.MaxMana && Player.HasBuff("xerathascended2onhit") && !Orbwalker.GetTarget().IsValidTarget())
                {
                    var allMinions = MinionManager.GetMinions(Player.ServerPosition, Player.AttackRange + Player.BoundingRadius * 2, MinionTypes.All);
                    foreach (var minion in allMinions)
                    {
                        Orbwalker.ForceTarget(minion);
                    }
                }
            }

            if (E.IsReady() && Config.Item("autoE").GetValue<bool>())
                LogicE();
            if (Program.LagFree(2) && W.IsReady() && !Player.IsWindingUp && Config.Item("autoW").GetValue<bool>())
                LogicW();
            if (Program.LagFree(3) && R.IsReady() && Config.Item("autoR").GetValue<bool>())
                LogicR();
            if (Program.LagFree(4) && Q.IsReady() && !Player.IsWindingUp && Config.Item("autoQ").GetValue<bool>())
                LogicQ();
        }

        private void LogicR()
        {
            R.Range = 2000 + R.Level * 1200;
            if (R.Range > Config.Item("MaxRangeR").GetValue<Slider>().Value)
                R.Range = Config.Item("MaxRangeR").GetValue<Slider>().Value;
            
            var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget() )
            {
                if (Config.Item("useR").GetValue<KeyBind>().Active && !IsCastingR)
                {
                    R.Cast();
                }
                if (!t.IsValidTarget(W.Range) && !IsCastingR && t.CountAlliesInRange(500) == 0 && Player.CountEnemiesInRange(1100) == 0)
                {
                    if (R.GetDamage(t) * 2 > t.Health)
                    {
                        R.Cast();
                    }
                }
                if (Game.Time - lastR > 0.001 * (float)Config.Item("delayR").GetValue<Slider>().Value && IsCastingR)
                {
                    Program.CastSpell(R, t);
                     
                }
                Rtarget = R.GetPrediction(t).CastPosition;
            }
            else if (Game.Time - lastR > 0.001 * (float)Config.Item("delayR").GetValue<Slider>().Value && IsCastingR)
            {
                R.Cast(Rtarget);
            } 
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                var qDmg = Q.GetDamage(t);
                var wDmg = W.GetDamage(t);
                if (wDmg > t.Health)
                {
                    Program.CastSpell(W, t);
                }
                else if (wDmg + qDmg > t.Health && Player.Mana > WMANA  + QMANA)
                    Program.CastSpell(W, t);
                else if (Program.Combo && Player.Mana > RMANA + WMANA )
                    Program.CastSpell(W, t);
                else if (Program.Farm && Config.Item("harrasW").GetValue<bool>() && Config.Item("harras" + t.ChampionName).GetValue<bool>() && !Player.UnderTurret(true) && (Player.Mana > Player.MaxMana * 0.8 || W.Level > Q.Level) && Player.Mana > RMANA + WMANA + EMANA + QMANA + WMANA && OktwCommon.CanHarras())
                    Program.CastSpell(W, t);
                else if ((Program.Combo || Program.Farm))
                {
                    foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && !OktwCommon.CanMove(enemy)))
                        W.Cast(enemy, true);
                }
            }
            else if (Program.LaneClear && Player.ManaPercentage() > Config.Item("Mana").GetValue<Slider>().Value && Config.Item("farmW").GetValue<bool>() && Player.Mana > RMANA + QMANA + WMANA)
            {
                var allMinions = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All);
                var farmPos = W.GetCircularFarmLocation(allMinions, W.Width);
                if (farmPos.MinionsHit > 1)
                    W.Cast(farmPos.Position);
            }
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (t.IsValidTarget() && !(Config.Item("separate").GetValue<bool>() && Program.LaneClear))
            {
                if (Q.IsCharging)
                {
                    if (OktwCommon.GetPassiveTime(Player, "XerathArcanopulseChargeUp") < 2 || t.IsValidTarget(W.Range))
                        Q.Cast(Q.GetPrediction(t).CastPosition);
                        
                    else
                        Program.CastSpell(Q, t);
                    return;
                }
                else if (t.IsValidTarget(Q.Range - 300))
                {
                    if (Program.Combo && Player.Mana > EMANA + QMANA)
                    {
                        Q.StartCharging();
                    }
                    else if (t.IsValidTarget(1200) && Program.Farm && Player.Mana > RMANA + EMANA + QMANA + QMANA && Config.Item("harras" + t.ChampionName).GetValue<bool>() && !Player.UnderTurret(true) && OktwCommon.CanHarras())
                    {
                        Q.StartCharging();
                    }
                    else if ((Program.Combo || Program.Farm) && Player.Mana > RMANA + WMANA)
                    {
                        foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                            Q.StartCharging();
                    }
                }
            }
            else if (Q.Range > 1000 && Player.CountEnemiesInRange(1450) == 0 && Program.LaneClear && (Q.IsCharging || (Player.ManaPercentage() > Config.Item("Mana").GetValue<Slider>().Value && Config.Item("farmQ").GetValue<bool>() && Player.Mana > RMANA + QMANA + WMANA)))
            {
                var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All);
                var Qfarm = Q.GetLineFarmLocation(allMinionsQ, Q.Width);
                if (Qfarm.MinionsHit > 3 || (Q.IsCharging && Qfarm.MinionsHit > 0))
                    Q.Cast(Qfarm.Position);
            }
        }

        private void LogicE()
        {
            foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && E.GetDamage(enemy) + Q.GetDamage(enemy) + W.GetDamage(enemy) > enemy.Health))
            {
                Program.CastSpell(E, enemy);
            }
            var t = Orbwalker.GetTarget() as Obj_AI_Hero;
            if (!t.IsValidTarget())
                t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                if (Program.Combo && Player.Mana > RMANA + EMANA)
                    Program.CastSpell(E, t);
                if (Program.Farm && Config.Item("harrasE").GetValue<bool>() && Player.Mana > RMANA + EMANA + WMANA + EMANA)
                    Program.CastSpell(E, t);
                foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && !OktwCommon.CanMove(enemy)))
                    E.Cast(enemy);
            }
        }

        private bool IsCastingR
        {
            get
            {
                return Player.HasBuff("XerathLocusOfPower2", true) ||
                       (ObjectManager.Player.LastCastedSpellName() == "XerathLocusOfPower2" &&
                        Utils.TickCount - ObjectManager.Player.LastCastedSpellT() < 500);
            }
        }

        private void Jungle()
        {
            if (Program.LaneClear && Player.Mana > RMANA + WMANA + RMANA + WMANA)
            {
                var mobs = MinionManager.GetMinions(Player.ServerPosition, 600, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (E.IsReady() && Config.Item("jungleE").GetValue<bool>())
                    {
                        E.Cast(mob.ServerPosition);
                        return;
                    }
                    if (W.IsReady() && Config.Item("jungleW").GetValue<bool>())
                    {
                        W.Cast(mob.ServerPosition);
                        return;
                    }
                    if (Q.IsReady() && Config.Item("jungleQ").GetValue<bool>())
                    {
                        Q.Cast(mob.ServerPosition);
                        return;
                    }
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
                RMANA = R.Instance.ManaCost;

            RMANA = RMANA - (30 + Player.Level * 3 + Player.Level);

            if (Player.Health < Player.MaxHealth * 0.2 || Q.IsCharging)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
            }
        }

        public static void drawLine(Vector3 pos1, Vector3 pos2, int bold, System.Drawing.Color color)
        {
            var wts1 = Drawing.WorldToScreen(pos1);
            var wts2 = Drawing.WorldToScreen(pos2);

            Drawing.DrawLine(wts1[0], wts1[1], wts2[0], wts2[1], bold, color);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("qRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>())
                {
                    if (Q.IsReady())
                        Utility.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
                }
                else
                    Utility.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
            }

            if (Config.Item("wRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>())
                {
                    if (W.IsReady())
                        Utility.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Orange, 1, 1);
                }
                else
                    Utility.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Orange, 1, 1);
            }

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

            if (Config.Item("noti").GetValue<bool>() && R.IsReady())
            {
                var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

                if (t.IsValidTarget())
                {
                    var rDamage = R.GetDamage(t);
                    if (rDamage * 3 > t.Health)
                    {
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Red, "3 x Ult can kill: " + t.ChampionName + " have: " + t.Health + "hp");
                        drawLine(t.Position, Player.Position, 10, System.Drawing.Color.Yellow);
                    }
                    else if (rDamage * 2 > t.Health)
                    {
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Red, "2 x Ult can kill: " + t.ChampionName + " have: " + t.Health + "hp");
                        drawLine(t.Position, Player.Position, 10, System.Drawing.Color.Yellow);
                    }
                    else if (rDamage > t.Health)
                    {
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Red, "1 x Ult can kill: " + t.ChampionName + " have: " + t.Health + "hp");
                        drawLine(t.Position, Player.Position, 10, System.Drawing.Color.Yellow);
                    }
                }
            }
        }
    }
}
