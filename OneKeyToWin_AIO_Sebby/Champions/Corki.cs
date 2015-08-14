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
    class Corki
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;

        private Spell E, Q, R2, R1, W;

        private float QMANA, WMANA, EMANA, RMANA;

        private bool passRdy = false;

        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 850);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 800);
            R2 = new Spell(SpellSlot.R, 1250);
            R1 = new Spell(SpellSlot.R, 1250);

            Q.SetSkillshot(0f, 170f, 1000f, false, SkillshotType.SkillshotCircle);

            R2.SetSkillshot(0.2f, 40f, 2000f, true, SkillshotType.SkillshotLine);
            R1.SetSkillshot(0.2f, 40f, 2000f, true, SkillshotType.SkillshotLine);

            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("wRange", "W range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range").SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("nktdE", "NoKeyToDash").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("autoE", "Auto E").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("autoR", "Auto R").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("minionR", "Try R on minion").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("useR", "Semi-manual cast R key").SetValue(new KeyBind('t', KeyBindType.Press))); //32 == space
            
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                Config.SubMenu(Player.ChampionName).SubMenu("Harras").AddItem(new MenuItem("harras" + enemy.ChampionName, enemy.ChampionName).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmQ", "LaneClear + jungle Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmR", "LaneClear + jungle  R").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("Mana", "LaneClear  Mana").SetValue(new Slider(80, 100, 30)));

            Game.OnUpdate += Game_OnGameUpdate;

            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.BeforeAttack += BeforeAttack;
            //Orbwalking.AfterAttack += afterAttack;
            //Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            //Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            //AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

        }

        private void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {

            if (E.IsReady() && Sheen() && args.Target.IsValid<Obj_AI_Hero>() && Config.Item("autoE").GetValue<bool>())
            {
                
                if (Program.Combo && Player.Mana > EMANA + RMANA)
                {
                    E.Cast(args.Target.Position);
                    Program.debug("ss");
                }
                if (!Q.IsReady() && !R1.IsReady() && args.Target.Health < Player.FlatPhysicalDamageMod * 2)
                    E.Cast();
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {

            if (Program.LagFree(0))
            {
                SetMana();
                farm();
            }
            if (Program.LagFree(1) && Q.IsReady() && Sheen() && !Player.IsWindingUp)
                LogicQ();
            if (Program.LagFree(2) && W.IsReady() && Program.Combo)
                LogicW();
            if (Program.LagFree(4) && R1.IsReady() && Sheen() && !Player.IsWindingUp)
                LogicR();
        }

        private void LogicR()
        {
            Spell R = R2;
            float rSplash = 150;
            if (bonusR)
            {
                R = R1;
                rSplash = 300;
            }
            
            var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

            if (t.IsValidTarget())
            {
                var rDmg = R.GetDamage(t);
                var qDmg = Q.GetDamage(t);
                if (rDmg * 2> t.Health)
                    CastR(R, t);
                else if (t.IsValidTarget(Q.Range) && qDmg + rDmg > t.Health)
                    CastR(R, t);
                if (Player.Spellbook.GetSpell(SpellSlot.R).Ammo > 1)
                {
                    foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(R.Range) && enemy.CountEnemiesInRange(rSplash) > 1))
                        t = enemy;

                    if ((Program.Combo && Player.Mana > RMANA * 3) )
                    {
                        CastR(R, t);
                    }
                    else if ((Program.Farm && Player.Mana > RMANA + EMANA + QMANA + WMANA) && !Player.UnderTurret(true) && Player.Spellbook.GetSpell(SpellSlot.R).Ammo > 3)
                    {
                        foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(R.Range) && Config.Item("harras" + enemy.ChampionName).GetValue<bool>()))
                            CastR(R, enemy);
                    }
                    if ((Program.Combo || Program.Farm) && ObjectManager.Player.Mana > RMANA + QMANA + EMANA)
                    {
                        foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(R.Range) && !OktwCommon.CanMove(enemy)))
                            R.Cast(enemy, true);
                    }
                }
            }
        }

        private void CastR(Spell R , Obj_AI_Hero t)
        {
            Program.CastSpell(R, t);
            if (Config.Item("minionR").GetValue<bool>())
            {
                // collision + predictio R
                var poutput = R.GetPrediction(t);
                var col = poutput.CollisionObjects.Count(ColObj => ColObj.IsEnemy && ColObj.IsMinion && !ColObj.IsDead);

                //hitchance
                var prepos = Prediction.GetPrediction(t, 0.4f);

                if (col == 0 && (int)prepos.Hitchance < 5)
                    return;

                float rSplash = 140;
                if (bonusR)
                    rSplash = 290f;
                
                var minions = MinionManager.GetMinions(Player.ServerPosition, R.Range - rSplash, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
                foreach (var minion in minions.Where(minion => minion.Distance(poutput.CastPosition) < rSplash))
                {
                    R.Cast(minion);
                    return;
                }
            }
        }

        private void LogicW()
        {
            var dashPosition = Player.Position.Extend(Game.CursorPos, W.Range);

            if (Game.CursorPos.Distance(Player.Position) > Player.AttackRange + Player.BoundingRadius * 2 && Program.Combo && Config.Item("nktdE").GetValue<bool>() && Player.Mana > RMANA + WMANA - 10)
            {
                W.Cast(dashPosition);
            }
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                var qDmg = Q.GetDamage(t);
                var rDmg = R1.GetDamage(t);
                if (qDmg > t.Health)
                    Q.Cast(t);
                else if (rDmg + qDmg > t.Health && Player.Mana > RMANA + QMANA)
                    Program.CastSpell(Q, t);
                else if (rDmg + 2 * qDmg > t.Health && Player.Mana > QMANA + RMANA * 2)
                    Program.CastSpell(Q, t);
                else if (Program.Combo && ObjectManager.Player.Mana > RMANA + QMANA)
                    Program.CastSpell(Q, t);
                else if (Program.Farm && ObjectManager.Player.Mana > RMANA + EMANA + WMANA + RMANA)
                {
                    foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && Config.Item("harras" + enemy.ChampionName).GetValue<bool>()))
                        Program.CastSpell(Q, enemy);
                }

                if ((Program.Combo || Program.Farm) && ObjectManager.Player.Mana > RMANA + WMANA + EMANA)
                {
                    foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                        Q.Cast(enemy, true, true);
                }
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

                    if (R1.IsReady() && Config.Item("farmR").GetValue<bool>())
                    {
                        R1.Cast(mob);
                        return;
                    }
                }

                if (!Player.IsWindingUp && Player.ManaPercentage() > Config.Item("Mana").GetValue<Slider>().Value)
                {
                    var minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
                    var Wfarm = Q.GetCircularFarmLocation(minions, 200);
                    var rfarm = R1.GetCircularFarmLocation(minions, 100);
                    if (R1.IsReady() && Config.Item("farmR").GetValue<bool>() && Player.Spellbook.GetSpell(SpellSlot.R).Ammo > 1)
                    {
                        if (rfarm.MinionsHit > 1)
                            R1.Cast(rfarm.Position);
                    }
                    if (Q.IsReady() && Config.Item("farmQ").GetValue<bool>())
                    {
                        var Rfarm = Q.GetCircularFarmLocation(minions, 100);
                        if (Wfarm.MinionsHit > 2)
                            Q.Cast(Rfarm.Position);
                    }
                }
            }
        }

        private bool Sheen()
        {
            var target = Orbwalker.GetTarget();

            if (target.IsValidTarget() && Player.HasBuff("sheen") && target is Obj_AI_Hero)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool bonusR { get { return Player.HasBuff("corkimissilebarragecounterbig"); } }

        private void SetMana()
        {
            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R1.IsReady())
                RMANA = QMANA - Player.PARRegenRate * Q.Instance.Cooldown;
            else
                RMANA = R1.Instance.ManaCost;

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
                    if (R1.IsReady())
                        Utility.DrawCircle(ObjectManager.Player.Position, R2.Range, System.Drawing.Color.Gray, 1, 1);
                }
                else
                    Utility.DrawCircle(ObjectManager.Player.Position, R2.Range, System.Drawing.Color.Gray, 1, 1);
            }
        }
    }
}
