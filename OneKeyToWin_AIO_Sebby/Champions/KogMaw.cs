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
    class KogMaw
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        public Spell Q, W, E, R;
        public float QMANA, WMANA, EMANA, RMANA;

        public bool attackNow = true;

        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 980);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 1200);
            R = new Spell(SpellSlot.R, 1800);

            Q.SetSkillshot(0.25f, 50f, 2000f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(1.5f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            LoadMenuOKTW();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.BeforeAttack += BeforeAttack;
            Orbwalking.AfterAttack += afterAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("AGC").GetValue<bool>() && E.IsReady() && ObjectManager.Player.Mana > RMANA + EMANA)
            {
                var Target = (Obj_AI_Hero)gapcloser.Sender;
                if (Target.IsValidTarget(E.Range))
                {
                    E.Cast(Target, true);
                    Program.debug("E AGC");
                }
            }
            return;
        }

        private void afterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
                return;
            attackNow = true;
        }

        private void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            attackNow = false;
        }
        private void Game_OnUpdate(EventArgs args)
        {
            if (Program.LagFree(0))
            {
                R.Range = 800 + 300 * ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Level;
                W.Range = 760 +  20 * ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level;
                SetMana();
                if (Player.IsZombie && Program.Combo)
                {
                    var t = TargetSelector.GetTarget(800, TargetSelector.DamageType.Physical);
                    if (t.IsValidTarget())
                        Player.IssueOrder(GameObjectOrder.MoveTo, t.ServerPosition);
                }
            }
            if (Program.LagFree(1) && E.IsReady() && !Player.IsWindingUp)
                LogicE();

            if (Program.LagFree(2) && Q.IsReady() && !Player.IsWindingUp)
                LogicQ();

            if (Program.LagFree(3) && W.IsReady() && !Player.IsWindingUp)
                LogicW();

            if (Program.LagFree(4) && R.IsReady() && !Player.IsWindingUp)
                LogicR();
            
        }

        private void LogicR()
        {
            if (Config.Item("autoR").GetValue<bool>() && Sheen())
            {
                R.Range = 800 + 300 * ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Level;
                var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                if (target.IsValidTarget(R.Range) && Program.ValidUlt(target))
                {
                    if (Config.Item("Raa").GetValue<bool>() && Orbwalking.InAutoAttackRange(target))
                        return;

                    double Rdmg = R.GetDamage(target) + (R.GetDamage(target) * target.CountAlliesInRange(500));
                    // Overkill protection
                    if (target.Health < R.GetDamage(target) * target.CountAlliesInRange(500) * 0.2)
                        Rdmg = 0;

                    var harasStack = Config.Item("harasStack").GetValue<Slider>().Value;
                    var comboStack = Config.Item("comboStack").GetValue<Slider>().Value;

                    if (R.GetDamage(target) > target.Health)
                        Program.CastSpell(R, target);
                    else if (Program.Combo && Rdmg * 2 > target.Health && ObjectManager.Player.Mana > RMANA * 3)
                        Program.CastSpell(R, target);
                    else if (GetRStacks() < comboStack + 2 && ObjectManager.Player.Mana > RMANA * 3)
                    {
                        foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(R.Range)))
                        {
                            if (!OktwCommon.CanMove(enemy))
                                R.Cast(enemy, true);
                            else
                                R.CastIfHitchanceEquals(enemy, HitChance.Immobile, true);
                        }
                    }

                    if (target.HasBuffOfType(BuffType.Slow) && Config.Item("Rslow").GetValue<bool>() && GetRStacks() < comboStack + 1 && ObjectManager.Player.Mana > RMANA + WMANA + EMANA + QMANA)
                        Program.CastSpell(R, target);
                    else if (Program.Combo && GetRStacks() < comboStack && ObjectManager.Player.Mana > RMANA + WMANA + EMANA + QMANA)
                        Program.CastSpell(R, target);
                    else if (Program.Farm && GetRStacks() < harasStack && ObjectManager.Player.Mana > RMANA + WMANA + EMANA + QMANA)
                        Program.CastSpell(R, target);
                }
            }
        }

        private void LogicW()
        {
            if (Config.Item("autoW").GetValue<bool>())
            {
                W.Range = 650 + 110 + 20 * ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level;
                if (ObjectManager.Player.CountEnemiesInRange(W.Range) > 0 && Sheen())
                {
                    if (Program.Combo)
                        W.Cast();
                    else if (Program.Farm && Config.Item("harasW").GetValue<bool>())
                        W.Cast();
                    else if (Program.Farm && ObjectManager.Player.CountEnemiesInRange(ObjectManager.Player.AttackRange) > 0)
                        W.Cast();
                }
            }
        }

        private void LogicQ()
        {
            if (Sheen())
            {
                var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                {
                    var qDmg = Q.GetDamage(t);
                    var eDmg = E.GetDamage(t);
                    if (t.IsValidTarget(W.Range) && qDmg + eDmg > t.Health)
                        Program.CastSpell(Q, t);
                    else if (Program.Combo && ObjectManager.Player.Mana > RMANA + QMANA * 2 + EMANA)
                        Program.CastSpell(Q, t);
                    else if ((Program.Farm && ObjectManager.Player.Mana > RMANA + EMANA + QMANA * 2 + WMANA) && !ObjectManager.Player.UnderTurret(true))
                        Program.CastSpell(Q, t);
                    else if ((Program.Combo || Program.Farm) && ObjectManager.Player.Mana > RMANA + QMANA + EMANA)
                    {
                        foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(Q.Range)))
                        {
                            if (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) ||
                             enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) ||
                             enemy.HasBuffOfType(BuffType.Taunt) || enemy.HasBuffOfType(BuffType.Slow) || enemy.HasBuff("Recall"))
                            {
                                Q.Cast(enemy, true);
                            }
                        }
                    }
                }
            }
        }

        private void LogicE()
        {
            if ( Sheen())
            {
                //W.Cast(ObjectManager.Player);
                var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                {
                    var qDmg = Q.GetDamage(t);
                    var eDmg = E.GetDamage(t);
                    if (eDmg > t.Health)
                        Program.CastSpell(E, t);
                    else if (eDmg + qDmg > t.Health && Q.IsReady())
                        Program.CastSpell(E, t);
                    else if (Program.Combo && ObjectManager.Player.Mana > RMANA + WMANA + EMANA + QMANA)
                        Program.CastSpell(E, t);
                    else if (Program.Farm && Config.Item("HarrasE").GetValue<bool>() &&  ObjectManager.Player.Mana > RMANA + WMANA + EMANA + QMANA + EMANA)
                        Program.CastSpell(E, t);
                    else if ((Program.Combo || Program.Farm) && ObjectManager.Player.Mana > RMANA + WMANA + EMANA)
                    {
                        foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && !OktwCommon.CanMove(enemy)))
                                E.Cast(enemy, true);
                    }
                }
            }
        }

        private bool Sheen()
        {
            var target = Orbwalker.GetTarget();
            if (!(target is Obj_AI_Hero))
                attackNow = true;
            if (target.IsValidTarget() && Player.HasBuff("sheen") && Config.Item("sheen").GetValue<bool>() && target is Obj_AI_Hero)
            {
                Program.debug("shen true");
                return false;
            }
            else if (target.IsValidTarget() && Config.Item("AApriority").GetValue<bool>() && target is Obj_AI_Hero && !attackNow)
            {
                
                return false;
            }
            else
            {
                return true;
            }
        }

        private int GetRStacks()
        {
            foreach (var buff in ObjectManager.Player.Buffs)
            {
                if (buff.Name == "kogmawlivingartillerycost")
                    return buff.Count;
            }
            return 0;
        }

        private void SetMana()
        {
            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;
            RMANA = R.Instance.ManaCost;

            if (ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.2)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
            }
        }

        private void drawText(string msg, Obj_AI_Hero Hero, System.Drawing.Color color)
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

            if (Config.Item("ComboInfo").GetValue<bool>())
            {
                var combo = "haras";
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget()))
                {
                    if (R.GetDamage(enemy) > enemy.Health)
                    {
                        combo = "KILL R";
                        drawText(combo, enemy, System.Drawing.Color.GreenYellow);
                    }
                    else
                    {
                        combo = (int)(enemy.Health / R.GetDamage(enemy)) + " R";
                        drawText(combo, enemy, System.Drawing.Color.Red);
                    }
                }
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
        }

        private void LoadMenuOKTW()
        {
            Config.SubMenu(Player.ChampionName).SubMenu("E config").AddItem(new MenuItem("AGC", "AntiGapcloserE").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E config").AddItem(new MenuItem("HarrasE", "Harras E").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("W config").AddItem(new MenuItem("autoW", "Auto W").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W config").AddItem(new MenuItem("harasW", "Haras W on max range").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("autoR", "Auto R").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("comboStack", "Max combo stack R").SetValue(new Slider(2, 10, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("harasStack", "Max haras stack R").SetValue(new Slider(1, 10, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("Rcc", "R cc").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("Rslow", "R slow").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("Raoe", "R aoe").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("Raa", "R only out off AA range").SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("ComboInfo", "R killable info").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("wRange", "W range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells").SetValue(true));

            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("sheen", "Sheen logic").SetValue(true));
            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("AApriority", "AA priority over spell").SetValue(true));

        }
    }
}
