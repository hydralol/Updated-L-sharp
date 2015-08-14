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
    class Orianna
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell E, Q, R, W, QR;
        private float QMANA, WMANA, EMANA, RMANA;
        private Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        private float RCastTime = 0;
        private Vector3 BallPos;
        private int FarmId;
        private bool Rsmart = false;

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 840);
            W = new Spell(SpellSlot.W, 210);
            E = new Spell(SpellSlot.E, 1095);
            R = new Spell(SpellSlot.R, 380);
            QR = new Spell(SpellSlot.Q, 825);

            Q.SetSkillshot(0.05f, 60f, 1150f, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.25f, 210f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 100f, 1700f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.6f, 375f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            QR.SetSkillshot(0.6f, 400f, 100f, false, SkillshotType.SkillshotCircle);

            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("wRange", "W range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("E Shield Config").AddItem(new MenuItem("autoW", "Auto E").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Shield Config").AddItem(new MenuItem("hadrCC", "Auto E hard CC").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Shield Config").AddItem(new MenuItem("poison", "Auto E poison").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Shield Config").AddItem(new MenuItem("Wdmg", "E dmg % hp").SetValue(new Slider(10, 100, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("E Shield Config").AddItem(new MenuItem("AGC", "AntiGapcloserE").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmQ", "Farm Q out range aa minion").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("Mana", "LaneClear Mana").SetValue(new Slider(60, 100, 20)));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("clearQ", "LaneClear Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("clearW", "LaneClear W").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("clearE", "LaneClear E").SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("rCount", "Auto R x enemies").SetValue(new Slider(3, 0, 5)));
            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("smartR", "Semi-manual cast R key").SetValue(new KeyBind('t', KeyBindType.Press)));
            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("OPTI", "OnPossibleToInterrupt R").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("Rturrent", "auto R under turrent").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("Rks", "R ks").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("Rlifesaver", "auto R life saver").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("Rblock", "Block R if 0 hit ").SetValue(true));

            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("W", "Auto W SpeedUp logic").SetValue(true));
            Game.OnUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnCreate += Obj_AI_Base_OnCreate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget +=Interrupter2_OnInterruptableTarget;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Config.Item("OPTI").GetValue<bool>())
                return;
            if (R.IsReady() && sender.Distance(BallPos) < R.Range)
            {
                R.Cast();
                Program.debug("interupt");
            }
            else if (Q.IsReady() && Player.Mana > RMANA + QMANA && sender.IsValidTarget(Q.Range))
                Q.Cast(sender.ServerPosition);
        }


        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (Config.Item("Rblock").GetValue<bool>() && args.Slot == SpellSlot.R && CountEnemiesInRangeDeley(BallPos, R.Width, R.Delay) == 0)
                args.Process = false;
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var Target = (Obj_AI_Hero)gapcloser.Sender;
            if (Config.Item("AGC").GetValue<bool>() && E.IsReady() && Target.IsValidTarget(800) && Player.Mana > RMANA + EMANA)
                E.CastOnUnit(Player);
            return;
        }
        
        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.HasBuff("Recall") || Player.IsDead)
                return;
            
            bool hadrCC = true, poison = true;
            if (Program.LagFree(0))
            {
                SetMana();
                hadrCC = Config.Item("hadrCC").GetValue<bool>();
                poison = Config.Item("poison").GetValue<bool>();
            }

            Obj_AI_Hero best = Player;

            foreach (var ally in Program.Allies.Where(ally => ally.IsValid && !ally.IsDead))
            {
                if (ally.HasBuff("orianaghostself") || ally.HasBuff("orianaghost"))
                    BallPos = ally.ServerPosition;

                if (Program.LagFree(4) )
                {
                    if (E.IsReady() && Player.Mana > RMANA + EMANA && ally.Distance(Player.Position) < E.Range)
                    {
                        if (ally.Health < ally.CountEnemiesInRange(600) * ally.Level * 20)
                        {
                            E.CastOnUnit(ally);
                        }
                        else if (HardCC(ally) && hadrCC)
                        {
                            E.CastOnUnit(ally);
                        }
                        else if (ally.HasBuffOfType(BuffType.Poison))
                        {
                            E.CastOnUnit(ally);
                        }
                    }
                    if (W.IsReady() && Player.Mana > RMANA + WMANA && BallPos.Distance(ally.ServerPosition) < 240 && ally.Health < ally.CountEnemiesInRange(600) * ally.Level * 20)
                        W.Cast();

                    if ((ally.Health < best.Health || ally.CountEnemiesInRange(300) > 0) && ally.Distance(Player.Position) < E.Range && ally.CountEnemiesInRange(700) > 0)
                        best = ally;

                    if (Program.LagFree(1) && E.IsReady() && Player.Mana > RMANA + EMANA && ally.Distance(Player.Position) < E.Range && ally.CountEnemiesInRange(R.Width) >= Config.Item("rCount").GetValue<Slider>().Value)
                    {
                        E.CastOnUnit(ally);
                    }
                }
            }
            /*
            foreach (var ally in HeroManager.Allies.Where(ally => ally.IsValid && ally.Distance(Player.Position) < 1000))
            {
                foreach (var buff in ally.Buffs)
                {
                        Program.debug(buff.Name);
                }

            }
            */
            if ((Config.Item("smartR").GetValue<KeyBind>().Active || Rsmart) && R.IsReady())
            {
                Rsmart = true;
                var target = TargetSelector.GetTarget(Q.Range + 100, TargetSelector.DamageType.Physical);
                if (target.IsValidTarget())
                {
                    if (CountEnemiesInRangeDeley(BallPos, R.Width, R.Delay) > 1)
                        R.Cast();
                    else if (Q.IsReady())
                        QR.Cast(target, true, true);
                    else if (CountEnemiesInRangeDeley(BallPos, R.Width, R.Delay) > 0)
                        R.Cast();
                }
                else
                    Rsmart = false;
            }
            else
                Rsmart = false;

            if (Program.LagFree(1))
            {
                LogicQ();
                LogicFarm();
            }
            if (Program.LagFree(2) && R.IsReady())
                LogicR();
            if (Program.LagFree(3) && W.IsReady() )
                LogicW();
            if (Program.LagFree(4) && E.IsReady())
                LogicE(best); 
        }

        private void LogicE(Obj_AI_Hero best)
        {
            var ta = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Physical);

            if ( Program.Combo && ta.IsValidTarget()  && !W.IsReady() && CountEnemiesInRangeDeley(BallPos, 100, 0.1f) > 0 && Player.Mana > RMANA + EMANA)
            {
                E.CastOnUnit(best);
                Program.debug(best.ChampionName);
            }
        }
        private void LogicR()
        {
            var Rturrent = Config.Item("Rturrent").GetValue<bool>();
            var Rks = Config.Item("Rks").GetValue<bool>();
            var Rlifesaver = Config.Item("Rlifesaver").GetValue<bool>();
            foreach (var t in Program.Enemies.Where(t => t.IsValidTarget() && BallPos.Distance(Prediction.GetPrediction(t, R.Delay).CastPosition) < R.Width && BallPos.Distance(t.ServerPosition) < R.Width))
            {
                if (Rks)
                {
                    var comboDmg = R.GetDamage(t);
                    if (t.IsValidTarget(Q.Range))
                        comboDmg += Q.GetDamage(t);
                    if (W.IsReady())
                        comboDmg += W.GetDamage(t);
                    if (t.Health < comboDmg)
                        R.Cast();
                    Program.debug("ks");
                }
                if (Rturrent && BallPos.UnderTurret(false) && !BallPos.UnderTurret(true))
                {
                    R.Cast();
                    Program.debug("Rturrent");
                }
                if (Rlifesaver && Player.Health < Player.CountEnemiesInRange(800) * Player.Level * 20 && Player.Distance(BallPos) > t.Distance(Player.Position))
                {
                    R.Cast();
                    Program.debug("ls");
                }

            }
            int countEnemies=CountEnemiesInRangeDeley(BallPos, R.Width, R.Delay);
            if (countEnemies >= Config.Item("rCount").GetValue<Slider>().Value && BallPos.CountEnemiesInRange(R.Width) == countEnemies)
                R.Cast();
            
        }

        private void LogicW()
        {
            foreach (var t in Program.Enemies.Where(t => t.IsValidTarget() && BallPos.Distance(t.ServerPosition) < 250 && t.Health < W.GetDamage(t)))
            {
                W.Cast();
                return;
            }
            if (CountEnemiesInRangeDeley(BallPos, W.Width, 0f) > 0 && Player.Mana > RMANA + WMANA)
            {
                W.Cast();
                return;
            }
            if (Config.Item("W").GetValue<bool>() && !Program.Farm && !Program.Combo && ObjectManager.Player.Mana > Player.MaxMana * 0.95 && Player.HasBuff("orianaghostself"))
                W.Cast();
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget() && Q.IsReady())
            {
                if (Q.GetDamage(t) + W.GetDamage(t) > t.Health)
                    CastQ(t);
                else if (Program.Combo && Player.Mana > RMANA + QMANA - 10)
                    CastQ(t);
                else if (Program.Farm && Player.Mana > RMANA + QMANA + WMANA + EMANA)
                    CastQ(t);
            }
            if (Config.Item("W").GetValue<bool>() && !t.IsValidTarget() && Program.Combo && Player.Mana > RMANA + 3 * QMANA + WMANA + EMANA + WMANA)
            {
                if (W.IsReady() && Player.HasBuff("orianaghostself"))
                {
                    W.Cast();
                }
                else if (E.IsReady() && !Player.HasBuff("orianaghostself"))
                {
                    E.CastOnUnit(Player);
                }
            }
        }
        private void LogicFarm()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All);
            if (Program.Farm && Config.Item("farmQ").GetValue<bool>() && Player.Mana > RMANA + QMANA + WMANA + EMANA)
            {
                foreach (var minion in allMinions.Where(minion => minion.IsValidTarget(Q.Range) && !Orbwalker.InAutoAttackRange(minion) && minion.Health < Q.GetDamage(minion) && minion.Health > minion.FlatPhysicalDamageMod))
                {
                    Q.Cast(minion);
                }
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Player.Mana > RMANA + QMANA)
            {
                 var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 800, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (Q.IsReady())
                        Q.Cast(mob.Position);
                    if (W.IsReady() && BallPos.Distance(mob.Position) < W.Width)
                        W.Cast();
                    else if (E.IsReady())
                        E.CastOnUnit(Player);
                    return;
                }
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                && (Player.ManaPercentage() > Config.Item("Mana").GetValue<Slider>().Value || (Player.UnderTurret(false) && !Player.UnderTurret(true) && Player.ManaPercentage() > 20)))
            {

                var Qfarm = Q.GetCircularFarmLocation(allMinions, 100);

                var QWfarm = Q.GetCircularFarmLocation(allMinions, W.Width);
                if (Qfarm.MinionsHit + QWfarm.MinionsHit == 0)
                    return;
                if (Config.Item("clearQ").GetValue<bool>())
                {
                    if (Qfarm.MinionsHit > 2 && !W.IsReady() && Q.IsReady())
                    {
                            Q.Cast(Qfarm.Position);
                    }
                    else if (QWfarm.MinionsHit > 2 && Q.IsReady())
                        Q.Cast(QWfarm.Position);
                }

                foreach (var minion in allMinions)
                {
                    if (W.IsReady() && minion.Distance(BallPos) < W.Range && minion.Health < W.GetDamage(minion) && Config.Item("clearW").GetValue<bool>())
                        W.Cast();
                    if (!W.IsReady() && E.IsReady() && minion.Distance(BallPos) < E.Width && Config.Item("clearE").GetValue<bool>())
                        E.CastOnUnit(Player);
                }
                
            }
        }

        private void CastQ(Obj_AI_Hero target)
        {

            float distance = Vector3.Distance(BallPos, target.ServerPosition);

            float delay = (distance / Q.Speed + Q.Delay);

            var prepos = Prediction.GetPrediction(target, delay);
            
            if ((int)prepos.Hitchance > 4)
            {
                if (prepos.CastPosition.Distance(prepos.CastPosition) < Q.Range)
                {
                    
                    Q.Cast(prepos.CastPosition);
                    
                }
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {

            if (sender.IsMe && args.SData.Name == "OrianaIzunaCommand")
                BallPos = args.End;

            
             if (args.Target == null 
                || !args.Target.IsValid 
                || !sender.IsEnemy 
                || !args.Target.IsAlly 
                || !Config.Item("autoW").GetValue<bool>() 
                || Player.Mana < EMANA + RMANA
                || args.Target.Position.Distance(Player.Position) > E.Range)
                return;

            foreach (var ally in Program.Allies.Where(ally => ally.IsValid && ally.NetworkId == args.Target.NetworkId))
            {
                var dmg = sender.GetSpellDamage(ally, args.SData.Name);
                double HpLeft = ally.Health - dmg;
                if (E.IsReady())
                {
                    
                    
                    double HpPercentage = (dmg * 100) / ally.Health;
                    double shieldValue = 60 + E.Level * 40 + 0.4 * Player.FlatMagicDamageMod;
                    if (HpPercentage >= Config.Item("Wdmg").GetValue<Slider>().Value)
                        E.CastOnUnit(ally);
                    else if (dmg > shieldValue)
                        E.CastOnUnit(ally);
                }
                //Game.PrintChat("" + HpPercentage);
            }   
        }

        private int CountEnemiesInRangeDeley(Vector3 position, float range, float delay)
        {
            int count = 0;
            foreach (var t in Program.Enemies.Where(t => t.IsValidTarget()))
            {
                Vector3 prepos = Prediction.GetPrediction(t, delay).CastPosition;
                if (position.Distance(prepos) < range)
                    count++;
            }
            return count;
        }
        private void Obj_AI_Base_OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.IsValid && obj.IsAlly && obj.Name == "TheDoomBall")
            {
                BallPos = obj.Position;
            }
        }

        private bool HardCC(Obj_AI_Hero target)
        {
            if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup) ||
                target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockback) ||
                target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Suppression) ||
                target.IsStunned )
            {
                return true;

            }
            else
                return false;
        }
        private void SetMana()
        {
            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;
            RMANA = R.Instance.ManaCost;

            if (!R.IsReady())
                RMANA = QMANA - ObjectManager.Player.Level * 2;
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

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("watermark").GetValue<bool>())
            {
                Drawing.DrawText(Drawing.Width * 0.2f, Drawing.Height * 0f, System.Drawing.Color.Cyan, "OneKeyToWin AIO - " + Player.ChampionName + " by Sebby");
            }
            if (BallPos.IsValid())
            {
                if (Config.Item("wRange").GetValue<bool>())
                {
                    if (Config.Item("onlyRdy").GetValue<bool>())
                    {
                        if (W.IsReady())
                            Utility.DrawCircle(BallPos, W.Range, System.Drawing.Color.Orange, 1, 1);
                    }
                    else
                        Utility.DrawCircle(BallPos, W.Range, System.Drawing.Color.Orange, 1, 1);
                }

                if (Config.Item("rRange").GetValue<bool>())
                {
                    if (Config.Item("onlyRdy").GetValue<bool>())
                    {
                        if (R.IsReady())
                            Utility.DrawCircle(BallPos, R.Range, System.Drawing.Color.Gray, 1, 1);
                    }
                    else
                        Utility.DrawCircle(BallPos, R.Range, System.Drawing.Color.Gray, 1, 1);
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
        }
    }
}
