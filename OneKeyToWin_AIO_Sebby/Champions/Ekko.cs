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
    class Ekko
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;

        private Spell E, Q, Q1,  R, W;
        private Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private float QMANA, WMANA, EMANA, RMANA, Wtime = 0, Wtime2 = 0;
        private static GameObject RMissile, WMissile2, WMissile, QMissile = null;
        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 750); 
            Q1 = new Spell(SpellSlot.Q, 1000);
            W = new Spell(SpellSlot.W, 1620);
            E = new Spell(SpellSlot.E, 330f);
            R = new Spell(SpellSlot.R, 375f);

            Q.SetSkillshot(0.25f, 60f, 2200f, false, SkillshotType.SkillshotLine);
            Q1.SetSkillshot(0.5f, 150f, 1000f, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(2.5f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.6f, 375f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("wRange", "W range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("Qhelp", "Show Q,W helper").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells").SetValue(true));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Config.SubMenu(Player.ChampionName).SubMenu("Haras Q").AddItem(new MenuItem("haras" + enemy.ChampionName, enemy.ChampionName).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmQ", "Lane clear Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("Mana", "LaneClear Mana").SetValue(new Slider(80, 100, 30)));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmW", "Farm W").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleQ", "Jungle clear Q").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("W option").AddItem(new MenuItem("autoW", "Auto W").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W option").AddItem(new MenuItem("Waoe", "Cast only if 2 targets").SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("autoR", "Auto R").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("Rdmg", "R dmg % hp").SetValue(new Slider(20, 100, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("rCount", "Auto R if enemies in range").SetValue(new Slider(3, 0, 5)));

            Game.OnUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnCreate += Obj_AI_Base_OnCreate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_SpellMissile.OnCreate += SpellMissile_OnCreateOld;
            Obj_SpellMissile.OnDelete += Obj_SpellMissile_OnDelete;
        }

        private void Obj_SpellMissile_OnDelete(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid<MissileClient>())
                return;
            MissileClient missile = (MissileClient)sender;
            
            if (missile.IsValid && missile.IsAlly && missile.SData.Name != null && (missile.SData.Name == "ekkoqmis" || missile.SData.Name == "ekkoqreturn"))
            {
                QMissile = null;
            }
        }

        private void SpellMissile_OnCreateOld(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid<MissileClient>())
                return;
            MissileClient missile = (MissileClient)sender;
            
            if (missile.IsValid && missile.IsAlly && missile.SData.Name != null && (missile.SData.Name == "ekkoqmis" || missile.SData.Name == "ekkoqreturn"))
            {
                QMissile = sender;
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.Target == null || !sender.IsEnemy || !args.Target.IsMe || !Config.Item("autoR").GetValue<bool>() || R.IsReady() )
                return;
            var dmg = sender.GetSpellDamage(Player, args.SData.Name);
            double HpLeft = Player.Health - dmg;
            double HpPercentage = (dmg * 100) / Player.Health;
            if (Player.Health - dmg < dmg)
            {
                if (HpPercentage >= Config.Item("Rdmg").GetValue<Slider>().Value)
                    R.Cast();

                //Game.PrintChat("" + HpPercentage);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Program.LagFree(0))
            {
                SetMana();
            }

            if (Program.LagFree(1) && Q.IsReady() )
                LogicQ();
            if (Program.LagFree(2) && W.IsReady() && Config.Item("autoW").GetValue<bool>() && Player.Mana > RMANA + WMANA + EMANA + QMANA)
                LogicW();
            if (Program.LagFree(3) && E.IsReady() )
                LogicE();
            if (Program.LagFree(4) && R.IsReady() )
                LogicR();
        }

        private void LogicR()
        {
            if (Config.Item("autoR").GetValue<bool>())
            {
                foreach (var t in Program.Enemies.Where(t =>RMissile != null && RMissile.IsValid && t.IsValidTarget() && RMissile.Position.Distance(Prediction.GetPrediction(t, R.Delay).CastPosition) < 350 && RMissile.Position.Distance(t.ServerPosition) < 350))
                {
                    var comboDmg = R.GetDamage(t) + GetWdmg(t) + Q.GetDamage(t) * 2 + E.GetDamage(t);

                    if (t.Health < comboDmg)
                        R.Cast();
                    Program.debug("ks");
                    if (RMissile.Position.CountEnemiesInRange(R.Range) >= Config.Item("rCount").GetValue<Slider>().Value && Config.Item("rCount").GetValue<Slider>().Value > 0)
                        R.Cast();
                }

                if (Player.Health < Player.CountEnemiesInRange(600) * Player.Level * 15)
                {
                    R.Cast();
                }
            }
        }

        private void LogicE()
        {
            if (Program.Combo && WMissile != null && WMissile.IsValid)
            {
                if (WMissile.Position.CountEnemiesInRange(200) > 0 && WMissile.Position.Distance(Player.ServerPosition) < 100)
                {
                    E.Cast(Player.Position.Extend(WMissile.Position, E.Range), true);
                }
            }

            var t = TargetSelector.GetTarget(800, TargetSelector.DamageType.Magical);

            if (E.IsReady() && ObjectManager.Player.Mana > RMANA + EMANA
                 && ObjectManager.Player.CountEnemiesInRange(260) > 0
                 && ObjectManager.Player.Position.Extend(Game.CursorPos, E.Range).CountEnemiesInRange(500) < 3
                 && t.Position.Distance(Game.CursorPos) > t.Position.Distance(ObjectManager.Player.Position))
            {
                E.Cast(ObjectManager.Player.Position.Extend(Game.CursorPos, E.Range), true);
            }
            else if (Program.Combo && ObjectManager.Player.Health > ObjectManager.Player.MaxHealth * 0.4
                && ObjectManager.Player.Mana > RMANA + EMANA
                && !ObjectManager.Player.UnderTurret(true)
                && ObjectManager.Player.Position.Extend(Game.CursorPos, E.Range).CountEnemiesInRange(700) < 3)
            {
                if (t.IsValidTarget() && Player.Mana > QMANA + EMANA + WMANA && t.Position.Distance(Game.CursorPos) + 300 < t.Position.Distance(Player.Position))
                {
                    E.Cast(Player.Position.Extend(Game.CursorPos, E.Range), true);
                }
            }
            else if (t.IsValidTarget() && Program.Combo  && GetEdmg(t) + GetWdmg(t) > t.Health)
            {
                E.Cast(Player.Position.Extend(t.Position, E.Range), true);
            }
        }

        private void Jungle()
        {
            if (Player.Mana > RMANA + WMANA + RMANA &&  Config.Item("jungleQ").GetValue<bool>())
            {
                var mobs = MinionManager.GetMinions(Player.ServerPosition, 500, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];

                    if (Q.IsReady() )
                    {
                        Q.Cast(mob);
                        return;
                    }
                }
            }
        }
        private void Obj_AI_Base_OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.IsValid  )
            {
                if (obj.Name == "Ekko" && obj.IsAlly)
                    RMissile = obj;
                if (obj.Name == "Ekko_Base_W_Indicator.troy")
                {
                    WMissile = obj;
                    Wtime = Game.Time;
                }
                if (obj.Name == "Ekko_Base_W_Cas.troy")
                {
                    WMissile2 = obj;
                    Wtime2 = Game.Time;
                }
            }     
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var t1 = TargetSelector.GetTarget(Q1.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {

                Program.debug("" + Q.GetDamage(t));
                var qDmg = GetQdmg(t);
                if (qDmg > t.Health)
                    Q.Cast(t, true);
                else if (Program.Combo && ObjectManager.Player.Mana > RMANA + QMANA)
                    Program.CastSpell(Q, t);
                else if (Program.Farm && Config.Item("haras" + t.ChampionName).GetValue<bool>() && Player.Mana > RMANA + WMANA + QMANA + QMANA)
                        Program.CastSpell(Q, t);
                if (Player.Mana > RMANA + QMANA + WMANA )
                {
                    foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                        Q.Cast(enemy, true);
                }

            }
            else if (t1.IsValidTarget())
            {
                Program.debug("" + Q.GetDamage(t1));
                var qDmg = GetQdmg(t1);
                if (qDmg > t1.Health)
                    Q1.Cast(t1, true);
                else if (Program.Combo && ObjectManager.Player.Mana > RMANA + QMANA)
                    Program.CastSpell(Q1, t1);
                else if (Program.Farm && Config.Item("haras" + t1.ChampionName).GetValue<bool>() && Player.Mana > RMANA + WMANA + QMANA + QMANA)
                    Program.CastSpell(Q1, t1);
                if (Player.Mana > RMANA + QMANA + WMANA)
                {
                    foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(Q1.Range) && !OktwCommon.CanMove(enemy)))
                        Q1.Cast(enemy, true);
                }
            }
            else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && ObjectManager.Player.ManaPercentage() > Config.Item("Mana").GetValue<Slider>().Value && Config.Item("farmQ").GetValue<bool>() && ObjectManager.Player.Mana > RMANA + QMANA + WMANA)
            {

                var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q1.Range, MinionTypes.All);
                var Qfarm = Q.GetLineFarmLocation(allMinionsQ, 100);
                if (Qfarm.MinionsHit > 5 && Q1.IsReady())
                    Q.Cast(Qfarm.Position);
                Jungle();
            }
            
        }


        private void LogicW()
        {
            
            var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget() )
            {
                W.CastIfWillHit(t, 2, true);
                if (t.CountEnemiesInRange(250) > 1)
                {
                    Program.CastSpell(W, t);
                }
                if (Config.Item("Waoe").GetValue<bool>())
                    return;
                if (t.HasBuffOfType(BuffType.Slow))
                {
                    Program.CastSpell(W, t);
                }
                if (Program.Combo  && W.GetPrediction(t).CastPosition.Distance(t.Position) > 200)
                    Program.CastSpell(W, t);
            }
            if ((Program.Combo || Program.Farm))
            {
                foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && !OktwCommon.CanMove(enemy)))
                    W.Cast(enemy, true);
            }
        }
        private void SetMana()
        {
            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R.IsReady())
                RMANA = QMANA - ObjectManager.Player.Level * 2;
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

        private double GetQdmg( Obj_AI_Base t)
        {
            double dmg = 90 + (30 * Q.Level) + Player.FlatMagicDamageMod * 0.8;
            return Player.CalcDamage(t, Damage.DamageType.Magical, dmg);
        }
        private double GetEdmg(Obj_AI_Base t)
        {
            double dmg = 20 + (30 * E.Level) + (Player.FlatMagicDamageMod * 0.2);
            return Player.CalcDamage(t, Damage.DamageType.Magical, dmg);
        }
        private double GetWdmg(Obj_AI_Base t)
        {
            if (t.Health < t.MaxHealth * 0.3)
            {
                double hp = t.MaxHealth - t.Health;
                double dmg = ((Player.FlatMagicDamageMod / 45) + 5) * 0.01;
                double dmg2 = hp * dmg;
                return Player.CalcDamage(t, Damage.DamageType.Magical, dmg2);

            }
            else
                return 0;

        }
        private double GetRdmg(Obj_AI_Base t)
        {
            double dmg = 50 + (150 * R.Level) + Player.FlatMagicDamageMod * 1.3;
            return Player.CalcDamage(t, Damage.DamageType.Magical, dmg);
        }

        public static void drawLine(Vector3 pos1, Vector3 pos2, int bold, System.Drawing.Color color)
        {
            var wts1 = Drawing.WorldToScreen(pos1);
            var wts2 = Drawing.WorldToScreen(pos2);

            Drawing.DrawLine(wts1[0], wts1[1], wts2[0], wts2[1], bold, color);
        }

        public static void drawText2(string msg, Vector3 Hero, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(Hero);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1] - 200, color, msg);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (QMissile != null && Config.Item("Qhelp").GetValue<bool>())
            {
                OktwCommon.DrawLineRectangle(QMissile.Position, Player.Position, (int)Q.Width, 1, System.Drawing.Color.White);

                if (WMissile != null && WMissile.IsValid)
                {
                    Utility.DrawCircle(WMissile.Position, 300, System.Drawing.Color.Yellow, 1, 1);
                    drawText2("W:  " + String.Format("{0:0.0}", Wtime + 3 - Game.Time), WMissile.Position, System.Drawing.Color.White);

                }
                if (WMissile2 != null && WMissile2.IsValid)
                {
                    Utility.DrawCircle(WMissile2.Position, 300, System.Drawing.Color.Red, 1, 1);
                    drawText2("W:  " + String.Format("{0:0.0}", Wtime2 + 1 - Game.Time), WMissile2.Position, System.Drawing.Color.Red);

                }
            }
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
                        Utility.DrawCircle(ObjectManager.Player.Position, 800, System.Drawing.Color.Yellow, 1, 1);
                }
                else
                    Utility.DrawCircle(ObjectManager.Player.Position, 800, System.Drawing.Color.Yellow, 1, 1);
            }
            if (Config.Item("rRange").GetValue<bool>())
            {
                if (RMissile != null && RMissile.IsValid)
                {
                    if (Config.Item("rRange").GetValue<bool>())
                    {
                        if (Config.Item("onlyRdy").GetValue<bool>())
                        {
                            if (R.IsReady())
                                Utility.DrawCircle(RMissile.Position, R.Width, System.Drawing.Color.YellowGreen, 1, 1);
                        }
                        else
                            Utility.DrawCircle(RMissile.Position, R.Width, System.Drawing.Color.YellowGreen, 1, 1);

                        drawLine(RMissile.Position, Player.Position, 10, System.Drawing.Color.YellowGreen);
                    }
                }
            }
        }
    }
}
