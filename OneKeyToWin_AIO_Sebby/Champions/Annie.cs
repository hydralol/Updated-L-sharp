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
    class Annie
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        public Spell Q, W, E, R;
        public float QMANA, WMANA, EMANA, RMANA;

        public GameObject Tibbers;
        public float TibbersTimer = 0;

        private Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }
        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 625f);
            W = new Spell(SpellSlot.W, 600f);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 625f);
            Q.SetTargetted(0.25f, 1400f);
            W.SetSkillshot(0.50f, 250f, 3000, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.20f, 250f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            LoadMenuOKTW();

            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            //Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnCreate += Obj_AI_Base_OnCreate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private void Obj_AI_Base_OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.IsValid && obj.Name == "Tibbers" )
                Tibbers = obj;
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (Config.Item("sup").GetValue<bool>()  && (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit))
            {
                if (((Obj_AI_Base)Orbwalker.GetTarget()).IsMinion) args.Process = false;
            }

            if (Program.Combo && !Config.Item("AACombo").GetValue<bool>())
            {
                var t = args.Target as Obj_AI_Hero;
                args.Process = false;
                if ((Player.GetAutoAttackDamage(t) * 2 > t.Health || Player.Mana < RMANA || !OktwCommon.CanMove(t)))
                    args.Process = true; 
            }

        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (ObjectManager.Player.HasBuff("Recall"))
                return;

            

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target.IsValidTarget() && ( Program.Farm || Config.Item("stun" + target.ChampionName).GetValue<bool>() || !HaveStun))
            {
                if (!HaveTibers && R.IsReady())
                {
                    if (Program.Combo && HaveStun && target.CountEnemiesInRange(400) > 1)
                        R.Cast(target, true, true);
                    else if (Config.Item("rCount").GetValue<Slider>().Value > 0 && Config.Item("rCount").GetValue<Slider>().Value <= target.CountEnemiesInRange(300))
                        R.Cast(target, true, true);
                    else if (Program.Combo && !W.IsReady() && !Q.IsReady()
                        && Q.GetDamage(target) < target.Health
                        && (target.CountEnemiesInRange(400) > 1 || R.GetDamage(target) + Q.GetDamage(target) > target.Health))
                        R.Cast(target, true, true);
                    else if (Program.Combo && Q.GetDamage(target) < target.Health && !OktwCommon.CanMove(target))
                            R.Cast(target, true, true);
                        
                }
                if (W.IsReady() && (Program.Farm || Program.Combo))
                {
                    if (Program.Combo && HaveStun && target.CountEnemiesInRange(250) > 1)
                        W.Cast(target, true, true);
                    else if (!Q.IsReady())
                        W.Cast(target, true, true);
                    else if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Charm) ||
                    target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Taunt))
                    {
                        W.Cast(target, true, true);
                    }
                }
                if (Q.IsReady() && (Program.Farm || Program.Combo))
                {
                    if (HaveStun && Program.Combo && target.CountEnemiesInRange(400) > 1 && (W.IsReady() || R.IsReady()))
                    {
                        return;
                    }
                    else
                        Q.Cast(target, true);
                }
            }
            if (Program.LagFree(1))
            {
                if (Config.Item("sup").GetValue<bool>())
                {
                    if (Q.IsReady() && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Player.Mana > RMANA + QMANA)
                        farmQ();
                }
                else
                {
                    if (Q.IsReady() && (!HaveStun || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear) && (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear))
                        farmQ();
                }
            }

            if (Program.LagFree(2))
            {
                SetMana();
                if (Config.Item("autoE").GetValue<bool>() && E.IsReady() && !HaveStun && Player.Mana > RMANA + EMANA + QMANA + WMANA && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
                    E.Cast();

                if (W.IsReady() && Player.InFountain() && !HaveStun)
                    W.Cast(ObjectManager.Player, true, true);
            }
            if (Program.LagFree(3) )
            {
                if(Config.Item("tibers").GetValue<bool>() && HaveTibers)
                {
                    var BestEnemy = TargetSelector.GetTarget(2000, TargetSelector.DamageType.Magical);
                    if (BestEnemy.IsValidTarget(2000) && Game.Time - TibbersTimer > 2)
                    {
                        Player.IssueOrder(GameObjectOrder.MovePet, BestEnemy.Position);
                        R.CastOnUnit(BestEnemy);
                        TibbersTimer = Game.Time;
                    }
                }
                else
                {
                    Tibbers = null;
                }
            }
        }

        private void farmQ()
        {
            if ( !Config.Item("farmQ").GetValue<bool>())
                return;
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All);
            if (Q.IsReady())
            {
                var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    Q.Cast(mob, true);
                    if (Config.Item("farmW").GetValue<bool>() && ObjectManager.Player.ManaPercentage() > Config.Item("Mana").GetValue<Slider>().Value && W.IsReady())
                        W.Cast(mob, true);
                }
            }

            foreach (var minion in allMinionsQ)
            {
                if (minion.Health > ObjectManager.Player.GetAutoAttackDamage(minion) && minion.Health < Q.GetDamage(minion))
                {
                    Q.Cast(minion);
                    return;
                }
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && ObjectManager.Player.ManaPercentage() > Config.Item("Mana").GetValue<Slider>().Value && Config.Item("farmW").GetValue<bool>() && ObjectManager.Player.Mana > RMANA + QMANA + EMANA + WMANA * 2)
            {
                var Wfarm = W.GetCircularFarmLocation(allMinionsQ, W.Width);
                if (Wfarm.MinionsHit > 2 && W.IsReady())
                    W.Cast(Wfarm.Position);
            }
        }

        private bool HaveTibers
        {
            get { return ObjectManager.Player.HasBuff("infernalguardiantimer"); }
        }

        private bool HaveStun
        {
            get { 
                var buffs = Player.Buffs.Where(buff => (buff.Name.ToLower() == "pyromania" || buff.Name.ToLower() == "pyromania_particle"));
                if (buffs.Any())
                {
                    var buff = buffs.First();
                    if (buff.Name.ToLower() == "pyromania_particle")
                        return true;
                    else
                        return false;
                }
                return false;
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
        private void LoadMenuOKTW()
        {
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("ComboInfo", "Combo Info").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("wRange", "W range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmQ", "Farm Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmW", "Lane clear W").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("Mana", "LaneClear Mana").SetValue(new Slider(60, 100, 30)));

            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("autoE", "Auto E stack stun").SetValue(true));
            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("sup", "Support mode").SetValue(true));
            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("tibers", "TibbersAutoPilot").SetValue(true));
            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("AACombo", "AA in combo").SetValue(false));

            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("rCount", "Auto R stun x enemies").SetValue(new Slider(3, 0, 5)));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Config.SubMenu(Player.ChampionName).SubMenu("Stun in combo").AddItem(new MenuItem("stun" + enemy.ChampionName, enemy.ChampionName).SetValue(true));
        }

        public static void drawText(string msg, Obj_AI_Hero Hero, System.Drawing.Color color)
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
                    if (Q.GetDamage(enemy) > enemy.Health)
                        combo = "Q";
                    else if (Q.GetDamage(enemy) + W.GetDamage(enemy) > enemy.Health)
                        combo = "QW";
                    else if (Q.GetDamage(enemy) + R.GetDamage(enemy) + W.GetDamage(enemy) > enemy.Health)
                        combo = "QWR";
                    else if (Q.GetDamage(enemy) * 2 + R.GetDamage(enemy) + W.GetDamage(enemy) > enemy.Health)
                        combo = "QWRQ";
                    else
                        combo = "haras: " + (int)(enemy.Health - (Q.GetDamage(enemy) * 2 + R.GetDamage(enemy) + W.GetDamage(enemy)));
                    drawText(combo, enemy, System.Drawing.Color.GreenYellow);
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

            if (Config.Item("rRange").GetValue<bool>())
            {
                if (Config.Item("onlyRdy").GetValue<bool>())
                {
                    if (R.IsReady())
                        Utility.DrawCircle(ObjectManager.Player.Position, R.Range + R.Width / 2, System.Drawing.Color.Gray, 1, 1);
                }
                else
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range + R.Width / 2, System.Drawing.Color.Gray, 1, 1);
            }

        }
    }
}
