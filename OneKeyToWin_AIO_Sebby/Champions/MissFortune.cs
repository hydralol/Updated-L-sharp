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
    class MissFortune
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell E, Q, Q1, R, W;
        private float QMANA, WMANA, EMANA, RMANA;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        private float RCastTime = 0;

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 655f);
            Q1 = new Spell(SpellSlot.Q, 1100f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 800f);
            R = new Spell(SpellSlot.R, 1350f);

            Q1.SetSkillshot(0.25f, 50f, 2000f, true, SkillshotType.SkillshotLine);
            Q.SetTargetted(0.25f, 1400f);
            E.SetSkillshot(0.5f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 200f, 2000f, false, SkillshotType.SkillshotCircle);

            LoadMenuOKTW();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += afterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            //sender.
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "MissFortuneBulletTime")
            {
                RCastTime = Game.Time;
                Program.debug(args.SData.Name);
                Orbwalking.Attack = false;
                Orbwalking.Move = false;
                if (Config.Item("forceBlockMove").GetValue<bool>())
                {
                    OktwCommon.blockMove = true;
                    OktwCommon.blockAttack = true;
                    OktwCommon.blockSpells = true;
                }
            }
        }

        private void afterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
                return;

            if (Player.IsChannelingImportantSpell() || Game.Time - RCastTime < 0.2)
            {
                Orbwalking.Attack = false;
                Orbwalking.Move = false;
                Program.debug("cast R");
                return;
            }

            if (!(target is Obj_AI_Hero))
                return;
            var t = target as Obj_AI_Hero;

            if (Q.IsReady() && t.IsValidTarget(Q.Range))
            {
                if (Q.GetDamage(t) + Player.GetAutoAttackDamage(t) * 3 > t.Health)
                    Q.Cast(t);
                else if (Program.Combo && Player.Mana > RMANA + QMANA + WMANA)
                    Q.Cast(t);
                else if (Program.Farm && Player.Mana > RMANA + QMANA + EMANA + WMANA)
                    Q.Cast(t);
            }
            if (W.IsReady())
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Player.Mana > RMANA + WMANA && Config.Item("autoW").GetValue<bool>())
                    W.Cast();
                else if (Player.Mana > RMANA + WMANA + QMANA && Config.Item("harasW").GetValue<bool>())
                    W.Cast();
            }
        }

        private void Jungle()
        {
            if (Program.LaneClear && Player.Mana > RMANA + WMANA + QMANA )
            {
                var mobs = MinionManager.GetMinions(Player.ServerPosition, 600, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (Q.IsReady() && Config.Item("jungleQ").GetValue<bool>() && Q.GetDamage(mob) > mob.Health)
                    {
                        Q.Cast(mob);
                        return;
                    }
                    if (W.IsReady() && Config.Item("jungleW").GetValue<bool>())
                    {
                        W.Cast();
                        return;
                    }
                    if (E.IsReady() && Config.Item("jungleE").GetValue<bool>())
                    {
                        E.Cast(mob.ServerPosition);
                        return;
                    }
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsChannelingImportantSpell() || Game.Time - RCastTime < 0.3)
            {
                if (Config.Item("forceBlockMove").GetValue<bool>())
                {
                    OktwCommon.blockMove = true;
                    OktwCommon.blockAttack = true;
                    OktwCommon.blockSpells = true;
                }

                Orbwalking.Attack = false;
                Orbwalking.Move = false;
               
                Program.debug("cast R");
                return;
            }
            else
            {
                Orbwalking.Attack = true;
                Orbwalking.Move = true;
                if (Config.Item("forceBlockMove").GetValue<bool>())
                {
                    OktwCommon.blockAttack = false;
                    OktwCommon.blockMove = false;
                    OktwCommon.blockSpells = false;
                }
                if (R.IsReady() && Config.Item("useR").GetValue<KeyBind>().Active)
                {
                    var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                    if (t.IsValidTarget(R.Range))
                    {
                        R.Cast(t, true, true);
                        RCastTime = Game.Time;
                        return;
                    }
                }
            }

            if (Program.LagFree(0))
            {
                SetMana();
                Jungle();
            }

            if (Program.LagFree(1) && !Player.IsWindingUp && Q.IsReady() && Config.Item("autoQ").GetValue<bool>())
                LogicQ();

            if (Program.LagFree(2) && !Player.IsWindingUp && E.IsReady() && Config.Item("autoE").GetValue<bool>())
                LogicE();

            if (Program.LagFree(4) && !Player.IsWindingUp && R.IsReady() && Config.Item("autoR").GetValue<bool>())
                LogicR();
            
        }
        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var t1 = TargetSelector.GetTarget(Q1.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget(Q.Range) && Player.Distance(t.ServerPosition) > 500)
            {
                if (Q.GetDamage(t) + Player.GetAutoAttackDamage(t) > t.Health)
                    Q.Cast(t);
                else if (Q.GetDamage(t) + Player.GetAutoAttackDamage(t) * 3 > t.Health)
                    Q.Cast(t);
                else if (Program.Combo && Player.Mana > RMANA + QMANA + WMANA)
                    Q.Cast(t);
                else if (Program.Farm && Player.Mana > RMANA + QMANA + EMANA + WMANA)
                    Q.Cast(t);
            }
            else if (t1.IsValidTarget(Q1.Range) && Config.Item("harasQ").GetValue<bool>() && Player.Distance(t1.ServerPosition) > Q.Range + 50)
            {
                var poutput = Q1.GetPrediction(t1);
                var col = poutput.CollisionObjects;
                if (col.Count() == 0)
                    return;

                var minionQ = col.Last();
                if (minionQ.IsValidTarget(Q.Range))
                {
                    if (Config.Item("killQ").GetValue<bool>() && Q.GetDamage(minionQ) > minionQ.Health - minionQ.GetAutoAttackDamage(minionQ) * 2)
                        return;
                    if (minionQ.Distance(Player.Position) > 400 && minionQ.Distance(poutput.CastPosition) < 380 && minionQ.Distance(t1.Position) < 380 && minionQ.Distance(poutput.CastPosition) > 150)
                    {
                        if (Q.GetDamage(t1) + Player.GetAutoAttackDamage(t1) > t1.Health)
                            Q.Cast(col.Last());
                        else if (Program.Combo && Player.Mana > RMANA + QMANA + WMANA)
                            Q.Cast(col.Last());
                        else if (Program.Farm && Player.Mana > RMANA + QMANA + EMANA + WMANA + QMANA)
                            Q.Cast(col.Last());
                    }
                }
            }
        }
        private void LogicE()
        {
            var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                if (Program.GetRealDmg(E, t) > t.Health)
                    E.Cast(t, true, true);
                else if (E.GetDamage(t) + Q.GetDamage(t) > t.Health && Player.Mana > QMANA + EMANA + RMANA)
                    E.Cast(t, true, true);
                else if (Program.Combo)
                {
                    if (Player.Mana > RMANA + WMANA + QMANA + EMANA && !Orbwalking.InAutoAttackRange(t))
                        E.Cast(t, true, true);
                    else if (Program.Combo && Player.Mana > RMANA + QMANA + EMANA && Player.CountEnemiesInRange(300) > 0)
                        E.Cast(t, true, true);
                    else if (Program.Combo && Player.Mana > RMANA + QMANA + EMANA && t.CountEnemiesInRange(250) > 1)
                        E.Cast(t, true, true);
                    else if (Player.Mana > RMANA + WMANA + QMANA + EMANA)
                    {
                        foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(E.Range) && !OktwCommon.CanMove(enemy)))
                            E.Cast(enemy, true, true);
                    }
                }
            }   
        }

        private void LogicR()
        {
            var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

            if (t.IsValidTarget(R.Range))
            {
                var rDmg = R.GetDamage(t) + (W.GetDamage(t) * 8);

                if (Player.CountEnemiesInRange(700) == 0 && t.CountAlliesInRange(400) == 0 && Program.ValidUlt(t))
                {
                    var tDis = Player.Distance(t.ServerPosition);
                    if (rDmg * 7 > t.Health && tDis < 800)
                    {
                        R.Cast(t, true, true);
                        RCastTime = Game.Time;
                    }
                    else if (rDmg * 6 > t.Health && tDis < 900)
                    {
                        R.Cast(t, true, true);
                        RCastTime = Game.Time;
                    }
                    else if (rDmg * 5 > t.Health && tDis < 1000)
                    {
                        R.Cast(t, true, true);
                        RCastTime = Game.Time;
                    }
                    else if (rDmg * 4 > t.Health && tDis < 1100)
                    {
                        R.Cast(t, true, true);
                        RCastTime = Game.Time;
                    }
                    else if (rDmg * 3 > t.Health && tDis < 1200)
                    {
                        R.Cast(t, true, true);
                        RCastTime = Game.Time;
                    }
                    else if (rDmg > t.Health && tDis < 1300)
                    {
                        R.Cast(t, true, true);
                        RCastTime = Game.Time;
                    }
                    return;
                }
                else if (rDmg * 8 > t.Health && t.CountEnemiesInRange(300) > 2 && Player.CountEnemiesInRange(700) == 0)
                {
                    R.Cast(t, true, true);
                    RCastTime = Game.Time;
                    return;
                }
                else if (rDmg * 8 > t.Health && !OktwCommon.CanMove(t) && Player.CountEnemiesInRange(600) == 0)
                {
                    R.Cast(t, true, true);
                    RCastTime = Game.Time;
                    return;
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
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("QRange", "Q range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("ERange", "E range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("RRange", "R range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("noti", "Show notification & line").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("autoQ", "Auto Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("harasQ", "Use Q on minion").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("killQ", "Use Q only if can kill minion").SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("autoW", "Auto W").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("harasW", "Haras W").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("autoR", "Auto R").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("forceBlockMove", "Force block player").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("useR", "Semi-manual cast R key").SetValue(new KeyBind('t', KeyBindType.Press))); //32 == space

            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("autoE", "Auto E").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleE", "Jungle clear E").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleQ", "Jungle Q ks").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleW", "Jungle clear W").SetValue(true));
        }

        public static void drawLine(Vector3 pos1, Vector3 pos2, int bold, System.Drawing.Color color)
        {
            var wts1 = Drawing.WorldToScreen(pos1);
            var wts2 = Drawing.WorldToScreen(pos2);

            Drawing.DrawLine(wts1[0], wts1[1], wts2[0], wts2[1], bold, color);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("noti").GetValue<bool>() && R.IsReady())
            {
                var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

                if (t.IsValidTarget())
                {
                    var rDamage = R.GetDamage(t) + (W.GetDamage(t) * 10);
                    if (rDamage * 8 > t.Health)
                    {
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.GreenYellow, "8 x R wave can kill: " + t.ChampionName + " have: " + t.Health + "hp");
                        drawLine(t.Position, Player.Position, 10, System.Drawing.Color.GreenYellow);
                    }
                    else if (rDamage * 5 > t.Health)
                    {
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Orange, "5 x R wave can kill: " + t.ChampionName + " have: " + t.Health + "hp");
                        drawLine(t.Position, Player.Position, 10, System.Drawing.Color.Orange);
                    }
                    else if (rDamage * 3 > t.Health)
                    {
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Yellow, "3 x R wave can kill: " + t.ChampionName + " have: " + t.Health + "hp");
                        drawLine(t.Position, Player.Position, 10, System.Drawing.Color.Yellow);
                    }
                    else if (rDamage > t.Health)
                    {
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Red, "1 x R wave can kill: " + t.ChampionName + " have: " + t.Health + "hp");
                        drawLine(t.Position, Player.Position, 10, System.Drawing.Color.Red);
                    }
                }
            }

            if (Config.Item("watermark").GetValue<bool>())
            {
                Drawing.DrawText(Drawing.Width * 0.2f, Drawing.Height * 0f, System.Drawing.Color.Cyan, "OneKeyToWin AIO - " + Player.ChampionName + " by Sebby");
            }
            if (Config.Item("QRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>())
                {
                    if (W.IsReady())
                        Utility.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
                }
                else
                    Utility.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
            }
            if (Config.Item("ERange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>())
                {
                    if (E.IsReady())
                        Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Orange, 1, 1);
                }
                else
                    Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Orange, 1, 1);
            }
            if (Config.Item("RRange").GetValue<bool>())
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

        public static void drawText(string msg, Obj_AI_Base Hero, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(Hero.Position);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1], color, msg);

        }
    }
}