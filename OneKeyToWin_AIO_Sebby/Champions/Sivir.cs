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
    class Sivir
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        public Spell E, Q, Qc, W, R;
        public float QMANA, WMANA, EMANA, RMANA;
        private static GameObject QMissile = null;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 1180f);
            Qc = new Spell(SpellSlot.Q, 1180f);
            W = new Spell(SpellSlot.W, float.MaxValue);
            E = new Spell(SpellSlot.E, float.MaxValue);
            R = new Spell(SpellSlot.R, 25000f);

            Q.SetSkillshot(0.25f, 90f, 1350f, false, SkillshotType.SkillshotLine);
            Qc.SetSkillshot(0.25f, 90f, 1350f, true, SkillshotType.SkillshotLine);

            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("notif", "Notification (timers)").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("noti", "Show KS notification").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("Qhelp", "Show Q helper").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmQ", "Lane clear Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmW", "Lane clear W").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("Mana", "LaneClear Mana").SetValue(new Slider(80, 100, 30)));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleQ", "Jungle clear Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleW", "Jungle clear W").SetValue(true));

            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("harasW", "Harras W").SetValue(true));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Config.SubMenu(Player.ChampionName).SubMenu("Harras Q").AddItem(new MenuItem("haras" + enemy.ChampionName, enemy.ChampionName).SetValue(true));
            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("autoR", "Auto R").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("E Shield Config").AddItem(new MenuItem("autoE", "Auto E").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Shield Config").AddItem(new MenuItem("AGC", "AntiGapcloserE").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Shield Config").AddItem(new MenuItem("Edmg", "E dmg % hp").SetValue(new Slider(0, 100, 0)));

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalker_AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Obj_SpellMissile.OnCreate += SpellMissile_OnCreateOld;
            Obj_SpellMissile.OnDelete += Obj_SpellMissile_OnDelete;
        }

        private void Obj_SpellMissile_OnDelete(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid<MissileClient>())
                return;
            MissileClient missile = (MissileClient)sender;

            if (missile.IsValid && missile.IsAlly && missile.SData.Name != null && (missile.SData.Name == "SivirQMissile" || missile.SData.Name == "SivirQMissileReturn"))
            {
                QMissile = null;
            }
        }

        private void SpellMissile_OnCreateOld(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid<MissileClient>())
                return;

            MissileClient missile = (MissileClient)sender;

            if (missile.IsValid && missile.IsAlly && missile.SData.Name != null && (missile.SData.Name == "SivirQMissile" || missile.SData.Name == "SivirQMissileReturn"))
            {
               QMissile = sender;
            }
        }

        public void Orbwalker_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe && Orbwalker.GetTarget().IsValidTarget())
                return;
            
            if (W.IsReady())
            {
                var t = TargetSelector.GetTarget(800, TargetSelector.DamageType.Physical);
                if (Program.Combo && target is Obj_AI_Hero && Player.Mana > RMANA + WMANA)
                    W.Cast();
                else if (Config.Item("harasW").GetValue<bool>() && (target is Obj_AI_Hero || t.IsValidTarget()) && Player.Mana > RMANA + WMANA + QMANA)
                    W.Cast();
                else if (Config.Item("farmW").GetValue<bool>() && Program.Farm && Player.Mana > RMANA + WMANA + QMANA && !Player.UnderTurret(true))
                {
                    if (farmW() && Program.LaneClear)
                        W.Cast();
                    else if (Program.Farm)
                    {
                        var minions = MinionManager.GetMinions(Player.Position, Player.AttackRange, MinionTypes.All);

                        if (minions == null || minions.Count == 0)
                            return;

                        int countMinions = 0;

                        foreach (var minion in minions.Where(minion => minion.Health < Player.GetAutoAttackDamage(minion) + W.GetDamage(minion)))
                        {
                            countMinions++;
                        }

                        if (countMinions > 1)
                            W.Cast();
                    }
                }
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!E.IsReady() || !sender.IsEnemy)
                return;
            
            if (!E.IsReady() || args.Target == null || !sender.IsEnemy || !args.Target.IsMe || !sender.IsValid<Obj_AI_Hero>() || args.SData.Name == "TormentedSoil")
                return;

            var dmg = sender.GetSpellDamage(ObjectManager.Player, args.SData.Name);
            double HpLeft = ObjectManager.Player.Health - dmg;
            double HpPercentage = (dmg * 100) / Player.Health;

            if ( HpPercentage >= Config.Item("Edmg").GetValue<Slider>().Value && sender.IsEnemy && args.Target.IsMe && !args.SData.IsAutoAttack() && Config.Item("autoE").GetValue<bool>() )
            {
                E.Cast();
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var Target = (Obj_AI_Hero)gapcloser.Sender;
            if (Config.Item("AGC").GetValue<bool>() && E.IsReady() && Target.IsValidTarget(5000))
                E.Cast();
            return;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Program.LagFree(0))
            {
                SetMana();
            }
           
            if (Program.LagFree(1) && Q.IsReady() && !Player.IsWindingUp)
            {
                LogicQ();
            }

            if (Program.LagFree(2) && R.IsReady() && Program.Combo && Config.Item("autoR").GetValue<bool>())
            {
                LogicR();
            }

            if (Program.LagFree(3) && Program.LaneClear)
            {
                Jungle();
            }
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                var qDmg = Q.GetDamage(t) * 1.9;
                if (Orbwalking.InAutoAttackRange(t))
                    qDmg = qDmg + Player.GetAutoAttackDamage(t) * 3;
                if (qDmg > t.Health)
                    Q.Cast(t, true);
                else if (Program.Combo && Player.Mana > RMANA + QMANA)
                    Program.CastSpell(Q, t);
                else if (Program.Farm && Config.Item("haras" + t.ChampionName).GetValue<bool>() && !Player.UnderTurret(true))
                {
                     if (Player.Mana > Player.MaxMana * 0.9)
                        Program.CastSpell(Q, t);
                     else if (ObjectManager.Player.Mana > RMANA + WMANA + QMANA + QMANA)
                        Program.CastSpell(Qc, t);
                     else if (Player.Mana > RMANA + WMANA + QMANA + QMANA)
                     {
                         Q.CastIfWillHit(t, 2, true);
                         if(Program.LaneClear)
                             Program.CastSpell(Q, t);
                     }
                }
                if (Player.Mana > RMANA + QMANA + WMANA && Q.IsReady())
                {
                    foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                        Q.Cast(enemy, true);
                }
            }
            else if (Program.LaneClear && Player.ManaPercentage() > Config.Item("Mana").GetValue<Slider>().Value && Config.Item("farmQ").GetValue<bool>() && ObjectManager.Player.Mana > RMANA + QMANA + WMANA)
            {
                var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All);
                var Qfarm = Q.GetLineFarmLocation(allMinionsQ, 100);
                if (Qfarm.MinionsHit > 5 && Q.IsReady())
                    Q.Cast(Qfarm.Position);
            }
        }
        private void LogicR()
        {
            var t = TargetSelector.GetTarget(800, TargetSelector.DamageType.Physical);
            if (Player.CountEnemiesInRange(800f) > 2)
                R.Cast();
            else if (t.IsValidTarget() && Orbwalker.GetTarget() == null && Program.Combo && Player.GetAutoAttackDamage(t) * 2 > t.Health && !Q.IsReady() && t.CountEnemiesInRange(800) < 3)
                R.Cast();
        }

        private void Jungle()
        {
            if ( Player.Mana > RMANA  + WMANA + RMANA )
            {
                var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 600, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (W.IsReady() && Config.Item("jungleW").GetValue<bool>())
                    {
                        W.Cast();
                        return;
                    }
                    if (Q.IsReady() && Config.Item("jungleQ").GetValue<bool>())
                    {
                        Q.Cast(mob);
                        return;
                    }
                }
            }
        }

        private bool farmW()
        {
            var allMinions = MinionManager.GetMinions(Player.ServerPosition, 1300, MinionTypes.All);
            int num = 0;
            foreach (var minion in allMinions)
            {
                num++;
            }
            if (num > 4)
                return true;
            else
                return false;
        }

        private void SetMana()
        {
            QMANA = 10 * Q.Level + 60;
            WMANA = 60;
            EMANA = 0;
            if (!R.IsReady())
                RMANA = QMANA - Player.PARRegenRate * 8;
            else
                RMANA = 100;

            if (Player.Health < Player.MaxHealth * 0.2)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
            }
        }

        public static void drawText2(string msg, Vector3 Hero, int high, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(Hero);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1] - high, color, msg);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (QMissile != null && Config.Item("Qhelp").GetValue<bool>())
                OktwCommon.DrawLineRectangle(QMissile.Position, Player.Position, (int)Q.Width, 1, System.Drawing.Color.White);

            if (Config.Item("notif").GetValue<bool>())
            {
                if (Player.HasBuff("sivirwmarker"))
                {
                    var color = System.Drawing.Color.Yellow;
                    var buffTime = OktwCommon.GetPassiveTime(Player, "sivirwmarker");
                    if (buffTime<1)
                        color = System.Drawing.Color.Red;
                    drawText2("W:  " + String.Format("{0:0.0}", buffTime), Player.Position, 175, color);
                }
                if (Player.HasBuff("SivirE"))
                {
                    var color = System.Drawing.Color.Aqua;
                    var buffTime = OktwCommon.GetPassiveTime(Player, "SivirE");
                    if (buffTime < 1)
                        color = System.Drawing.Color.Red;
                    drawText2("E:  " + String.Format("{0:0.0}", buffTime), Player.Position, 200, color);
                }
                if (Player.HasBuff("SivirR"))
                {
                    var color = System.Drawing.Color.GreenYellow;
                    var buffTime = OktwCommon.GetPassiveTime(Player, "SivirR");
                    if (buffTime < 1)
                        color = System.Drawing.Color.Red;
                    drawText2("R:  " + String.Format("{0:0.0}", buffTime), Player.Position, 225, color);
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

            if (Config.Item("noti").GetValue<bool>())
            {
                var target = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);
                if (target.IsValidTarget())
                {
                    if (Q.GetDamage(target) * 2 > target.Health)
                    {
                        Render.Circle.DrawCircle(target.ServerPosition, 200, System.Drawing.Color.Red);
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.4f, System.Drawing.Color.Red, "Q kill: " + target.ChampionName + " have: " + target.Health + "hp");
                    }
                }
            }
        }
    }
}
