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
    class Urgot
    {

        private Menu Config = Program.Config;
        private static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell Q, Q2, W, E, R;
        private float QMANA, WMANA, EMANA, RMANA;
        private double OverFarm = 0, lag = 0;
        private int FarmId;

        private int Muramana = 3042, Tear = 3070, Manamune = 3004;

        private Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 980);
            Q2 = new Spell(SpellSlot.Q, 1200);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 900);
            R = new Spell(SpellSlot.R, 850);

            Q.SetSkillshot(0.25f, 60f, 1600f, true, SkillshotType.SkillshotLine);
            Q2.SetSkillshot(0.25f, 60f, 1600f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 200f, 1750f, false, SkillshotType.SkillshotCircle);
            LoadMenuOKTW();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.BeforeAttack += BeforeAttack;
            //Orbwalking.AfterAttack += afterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnInterruptableSpell;
        }

        private void LoadMenuOKTW()
        {
            Config.SubMenu("Items").AddItem(new MenuItem("mura", "Auto Muramana").SetValue(true));
            Config.SubMenu("Items").AddItem(new MenuItem("stack", "Stack Tear if full mana").SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range").SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("E config").AddItem(new MenuItem("autoE", "Auto E haras").SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("autoR", "Auto R under turrent").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("inter", "OnPossibleToInterrupt R").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("Rhp", "dont R if under % hp").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("useR", "Semi-manual cast R key").SetValue(new KeyBind('t', KeyBindType.Press))); //32 == space
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Config.SubMenu(Player.ChampionName).SubMenu("R option").SubMenu("GapCloser R").AddItem(new MenuItem("GapCloser" + enemy.ChampionName, enemy.ChampionName, true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range").SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range").SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("W Shield Config").AddItem(new MenuItem("autoW", "Auto W").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W Shield Config").AddItem(new MenuItem("Waa", "Auto W befor AA").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W Shield Config").AddItem(new MenuItem("AGC", "AntiGapcloserW").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W Shield Config").AddItem(new MenuItem("Wdmg", "W dmg % hp").SetValue(new Slider(10, 100, 0)));


            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmQ", "Farm Q").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("LC", "LaneClear").SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("Mana", "LaneClear Mana").SetValue(new Slider(60, 100, 20)));
        }

        private void OnInterruptableSpell(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (Config.Item("inter").GetValue<bool>() && R.IsReady() && unit.IsValidTarget(R.Range) && Player.HealthPercentage() >= Config.Item("Rhp").GetValue<Slider>().Value)
                R.Cast(unit);
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (R.IsReady())
            {
                var Target = (Obj_AI_Hero)gapcloser.Sender;
                if (Config.Item("GapCloser" + Target.ChampionName).GetValue<bool>() && Target.IsValidTarget(R.Range))
                {
                    R.Cast(Target, true);
                }
            }
            if (Config.Item("AGC").GetValue<bool>() && W.IsReady() && Player.Mana > RMANA + WMANA)
            {
                var Target = (Obj_AI_Hero)gapcloser.Sender;
                if (Target.IsValidTarget(E.Range))
                    W.Cast();
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.Target == null || !sender.IsEnemy || !args.Target.IsMe || !Config.Item("autoW").GetValue<bool>() || W.IsReady() || Player.Mana > RMANA + WMANA)
                return;
            var dmg = sender.GetSpellDamage(Player, args.SData.Name);
            double HpLeft = Player.Health - dmg;
            double HpPercentage = (dmg * 100) / Player.Health;
            double shieldValue = 20 + W.Level * 40 + 0.08 * Player.MaxMana + 0.8 * Player.FlatMagicDamageMod;
            if (Player.Health - dmg < dmg)
            {
                if (HpPercentage >= Config.Item("Wdmg").GetValue<Slider>().Value)
                    W.Cast();
                else if (dmg > shieldValue)
                    W.Cast();
                //Game.PrintChat("" + HpPercentage);
            }
        }

        private void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (FarmId != args.Target.NetworkId)
                FarmId = args.Target.NetworkId;
            if (W.IsReady() && Config.Item("Waa").GetValue<bool>() && args.Target.IsValid<Obj_AI_Hero>() && Player.Mana > WMANA + QMANA * 4)
                W.Cast();

            if (Config.Item("mura").GetValue<bool>())
            {
                int Mur = Items.HasItem(Muramana) ? 3042 : 3043;
                if (!Player.HasBuff("Muramana") && args.Target.IsEnemy && args.Target.IsValid<Obj_AI_Hero>() && Items.HasItem(Mur) && Items.CanUseItem(Mur) && Player.Mana > RMANA + EMANA + QMANA + WMANA)
                    Items.UseItem(Mur);
                else if (Player.HasBuff("Muramana") && Items.HasItem(Mur) && Items.CanUseItem(Mur))
                    Items.UseItem(Mur);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("useR").GetValue<KeyBind>().Active )
            {
                var tr = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                if (tr.IsValidTarget())
                    R.Cast(tr);
            }

            if (Program.LagFree(0))
            {
                SetMana();
                
            }

            if (Program.LagFree(1) && !Player.IsWindingUp && E.IsReady())
                LogicE();

            if (Program.LagFree(2) && !Player.IsWindingUp && Q.IsReady())
                LogicQ();

            if (Program.LagFree(3) && !Player.IsWindingUp && Q.IsReady())
                LogicQ2();

            if (Program.LagFree(4) && !Player.IsWindingUp && R.IsReady())
                LogicR();
        }

        private void LogicQ2()
        {
            if (Program.Farm && Config.Item("farmQ").GetValue<bool>() && Player.Mana > RMANA + EMANA + WMANA + QMANA * 3)
                farmQ();
            else if (Config.Item("stack").GetValue<bool>() && !Player.HasBuff("Recall") && Player.Mana > Player.MaxMana * 0.95 && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None && (Items.HasItem(Tear) || Items.HasItem(Manamune)))
                Q.Cast(Player.ServerPosition);
        }
        private void LogicQ()
        {
            if (Program.Combo || Program.Farm)
            {
                foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(Q2.Range) && enemy.HasBuff("urgotcorrosivedebuff")))
                {
                    if ((Player.Mana > WMANA + QMANA * 4 || Q.GetDamage(enemy) * 3 > enemy.Health) && W.IsReady())
                    {
                        W.Cast();
                        Program.debug("W");
                    }
                    Program.debug("E");
                    Q2.Cast(enemy.ServerPosition);
                    return;
                }
            }

            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (Player.CountEnemiesInRange(Q.Range - 200) == 0)
                t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            else
                t = TargetSelector.GetTarget(Q.Range - 200, TargetSelector.DamageType.Physical);

            if (t.IsValidTarget())
            {
                if (t.IsValidTarget(W.Range) && Q.GetDamage(t) + E.GetDamage(t) > t.Health)
                    Program.CastSpell(Q, t);
                else if (Program.Combo && Player.Mana > RMANA + QMANA)
                    Program.CastSpell(Q, t);
                else if ((Program.Farm && Player.Mana > RMANA + EMANA + QMANA + WMANA) && !Player.UnderTurret(true))
                {
                    foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range)))
                        Program.CastSpell(Q, enemy);
                }
                else if ((Program.Combo || Program.Farm) && ObjectManager.Player.Mana > RMANA + QMANA + EMANA)
                {
                    foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && !OktwCommon.CanMove(enemy)))
                        Q.Cast(enemy, true);
                }
            }
        }

        private void LogicR()
        {
            R.Range = 400 + 150 * R.Level;
            if (Player.UnderTurret(false) && !ObjectManager.Player.UnderTurret(true) && Player.HealthPercentage() >= Config.Item("Rhp").GetValue<Slider>().Value && Config.Item("autoR").GetValue<bool>())
            {
                foreach (var target in Program.Enemies.Where(target => target.IsValidTarget(R.Range) && Program.ValidUlt(target)))
                {
                    if ( target.CountEnemiesInRange(700) < 2 + Player.CountAlliesInRange(700))
                    {
                        R.Cast(target);
                    }
                }
            }
        }

        private void LogicE()
        {
            var qCd = Q.Instance.CooldownExpires - Game.Time;

            var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                var qDmg = Q.GetDamage(t);
                var eDmg = E.GetDamage(t);
                if (eDmg > t.Health)
                    E.Cast(t);
                else if (eDmg + qDmg > t.Health && Player.Mana > EMANA + QMANA)
                    Program.CastSpell(E, t);
                else if (eDmg + 3 * qDmg > t.Health && Player.Mana > EMANA + QMANA * 3)
                    Program.CastSpell(E, t);
                else if (Program.Combo && Player.Mana > EMANA + QMANA * 2 && qCd < 0.5f)
                    Program.CastSpell(E, t);
                else if (Program.Farm && Player.Mana > RMANA + EMANA + QMANA * 5 && Config.Item("autoE").GetValue<bool>())
                    Program.CastSpell(E, t);
                else if ((Program.Combo || Program.Farm) && Player.Mana > RMANA + WMANA + EMANA)
                {
                    foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && !OktwCommon.CanMove(enemy)))
                        E.Cast(enemy, true, true);
                }
            }
        }
        public void farmQ()
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                var mobs = MinionManager.GetMinions(Player.ServerPosition, 800, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    Q.Cast(mob, true);
                    return;
                }
            }

            if (!Config.Item("farmQ").GetValue<bool>())
                return;

            var minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
            foreach (var minion in minions.Where(minion => FarmId != minion.NetworkId && !Orbwalker.InAutoAttackRange(minion) && minion.Health < Q.GetDamage(minion)))
            {
                Program.CastSpell(Q, minion);
                FarmId = minion.NetworkId;
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && !Orbwalking.CanAttack() && (Player.ManaPercentage() > Config.Item("Mana").GetValue<Slider>().Value || ObjectManager.Player.UnderTurret(false)))
            {
                foreach (var minion in minions.Where(minion => FarmId != minion.NetworkId && Orbwalker.InAutoAttackRange(minion) && minion.Health < Q.GetDamage(minion) * 0.8 && minion.Health > minion.FlatPhysicalDamageMod))
                    Program.CastSpell(Q, minion);
            }
        }

        private void SetMana()
        {
            if (Player.Health < Player.MaxHealth * 0.2)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
                return;
            }

            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R.IsReady())
                RMANA = QMANA - Player.PARRegenRate * Q.Instance.Cooldown;
            else
                RMANA = R.Instance.ManaCost;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("watermark").GetValue<bool>())
                Drawing.DrawText(Drawing.Width * 0.2f, Drawing.Height * 0f, System.Drawing.Color.Cyan, "OneKeyToWin AIO - " + Player.ChampionName + " by Sebby");

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
            if (Config.Item("rRange").GetValue<bool>())
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
    }
}
