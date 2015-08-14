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
    class Twitch
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        public Spell Q, W, E, R;
        public float QMANA, WMANA, EMANA, RMANA;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        private int count = 0, countE = 0;
        private float grabTime = Game.Time;

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 0);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 1200);
            R = new Spell(SpellSlot.R, 975);

            W.SetSkillshot(0.25f, 100f, 1410f, false, SkillshotType.SkillshotCircle);
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("notif", "Notification (timers)").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("countQ", "Auto Q if x enemies are going in your direction 0-disable").SetValue(new Slider(3, 5, 0)));

            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("autoW", "AutoW").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("Eks", "E ks").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("countE", "Auto E if x stacks & out range AA").SetValue(new Slider(6, 6, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("5e", "Always E if 6 stacks").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("jungleE", "Jungle ks E").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("Rks", "R KS out range AA").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("countR", "Auto R if x enemies (combo)").SetValue(new Slider(3, 5, 0)));
           
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            //AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            // Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            //Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (Program.LagFree(0))
            {
                SetMana();
            }
            if (Program.LagFree(1) && E.IsReady() )
                LogicE();
            if (Program.LagFree(2) && Q.IsReady() && !Player.IsWindingUp)
                LogicQ();
            if (Program.LagFree(3) && W.IsReady() && !Player.IsWindingUp)
                LogicW();
            if (Program.LagFree(4) && R.IsReady() && Program.Combo)
                LogicR();
        }

        private void LogicR()
        {
            var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget() )
            {
                if (!Orbwalking.InAutoAttackRange(t) && Config.Item("Rks").GetValue<bool>() && Player.GetAutoAttackDamage(t) * 4 > t.Health)
                    R.Cast();

                if (t.CountEnemiesInRange(450) >= Config.Item("countR").GetValue<Slider>().Value && 0 != Config.Item("countR").GetValue<Slider>().Value)
                    R.Cast();

            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {

                if (Program.Combo && Player.Mana > WMANA + RMANA + EMANA && (Player.GetAutoAttackDamage(t) * 2 < t.Health || !Orbwalking.InAutoAttackRange(t)))
                    Program.CastSpell(W, t);
                else if ((Program.Combo || Program.Farm) && Player.Mana > RMANA + WMANA + EMANA)
                {
                    foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && !OktwCommon.CanMove(enemy)))
                        W.Cast(enemy, true);
                }
            }
        }

        private void LogicQ()
        {
            if (Config.Item("countQ").GetValue<Slider>().Value == 0 || Player.Mana < RMANA + QMANA)
                return;
            var count = 0;
            foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(3000)))
            {
                List<Vector2> waypoints = enemy.GetWaypoints();

                if (Player.Distance( waypoints.Last<Vector2>().To3D()) < 600)
                    count++;
            }
            if (count >= Config.Item("countQ").GetValue<Slider>().Value)
                Q.Cast();
        }

        private void LogicE()
        {
            foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && enemy.HasBuff("twitchdeadlyvenom")))
            {
                if (Config.Item("Eks").GetValue<bool>() && E.GetDamage(enemy) + passiveDmg(enemy) > enemy.Health)
                    E.Cast();
                if (Player.Mana > RMANA + EMANA)
                {
                    int buffsNum = OktwCommon.GetBuffCount(enemy, "twitchdeadlyvenom");
                    if (Config.Item("5e").GetValue<bool>() && buffsNum == 6 )
                         E.Cast();
                    if (!Orbwalking.InAutoAttackRange(enemy) && 0 < Config.Item("countE").GetValue<Slider>().Value && buffsNum >= Config.Item("countE").GetValue<Slider>().Value)
                        E.Cast();
                }
            }
            JungleE();
        }

        private float passiveDmg(Obj_AI_Base target)
        {
            if (!target.HasBuff("twitchdeadlyvenom"))
                return 0;
            float dmg = 6;
            if (Player.Level < 13)
                dmg = 4;
            if (Player.Level < 9)
                dmg = 3;
            if (Player.Level < 5)
                dmg = 2;
            float buffTime = OktwCommon.GetPassiveTime(target, "twitchdeadlyvenom");
            return (dmg * OktwCommon.GetBuffCount(target, "twitchdeadlyvenom") * buffTime) - target.HPRegenRate * buffTime;
        }

        private void JungleE()
        {
            if (!Config.Item("jungleE").GetValue<bool>() || Player.Mana < RMANA + EMANA)
                return;

            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (E.IsKillable(mob))
                    E.Cast();
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

        public static void drawText(string msg, Obj_AI_Hero Hero, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(Hero.Position);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1], color, msg);
        }

        public static void drawText(string msg, Vector3 Hero, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(Hero);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1] - 200, color, msg);
        }
        public static void drawText2(string msg, Vector3 Hero, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(Hero);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1] - 200, color, msg);
        }
        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("notif").GetValue<bool>())
            {
                if (Player.HasBuff("TwitchHideInShadows"))
                    drawText2("Q:  " + String.Format("{0:0.0}", OktwCommon.GetPassiveTime(Player, "TwitchHideInShadows")), Player.Position, System.Drawing.Color.Yellow);
                if (Player.HasBuff("twitchhideinshadowsbuff"))
                    drawText2("Q AS buff:  " + String.Format("{0:0.0}", OktwCommon.GetPassiveTime(Player, "twitchhideinshadowsbuff")), Player.Position, System.Drawing.Color.YellowGreen);
                if (Player.HasBuff("TwitchFullAutomatic"))
                    drawText2("R ACTIVE:  " + String.Format("{0:0.0}", OktwCommon.GetPassiveTime(Player, "TwitchFullAutomatic")), Player.Position, System.Drawing.Color.OrangeRed);

            }
            foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(2000) && enemy.HasBuff("twitchdeadlyvenom")))
            {
                if (passiveDmg(enemy) > enemy.Health)
                    drawText("IS DEAD", enemy, System.Drawing.Color.Yellow);
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
