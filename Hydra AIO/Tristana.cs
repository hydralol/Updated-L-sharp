#region
using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Drawing;
using System.Linq;
using SharpDX.Direct3D9;
using Font = SharpDX.Direct3D9.Font;
#endregion

namespace HydraAIO
{
    internal class Tristana : Champion
    {
        public static Spell Q, W, E, R;
        public static Font vText;

        public Tristana()
        {
            Q = new Spell(SpellSlot.Q, 703);

            W = new Spell(SpellSlot.W, 900);
            W.SetSkillshot(.50f, 250f, 1400f, false, SkillshotType.SkillshotCircle);

            E = new Spell(SpellSlot.E, 703);
            R = new Spell(SpellSlot.R, 703);

            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = true;

            vText = new Font(
                Drawing.Direct3DDevice,
                new FontDescription
                {
                    FaceName = "Courier new",
                    Height = 15,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.Default,
                });

            Utils.PrintMessage(" H Tristana loaded.");
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
        }

        public class TristanaData
        {
            public static Obj_AI_Hero GetTarget(float vRange)
            {
                return TargetSelector.GetTarget(vRange, TargetSelector.DamageType.Physical);
            }

            public static double GetWDamage
            {
                get
                {
                    if (W.IsReady())
                    {
                        var wDamage = new double[] { 80, 105, 130, 155, 180 }[W.Level - 1] +
                                      0.5 * Player.FlatMagicDamageMod;
                        if (GetEMarkedCount > 0 && GetEMarkedCount < 4)
                        {
                            return wDamage + (wDamage * GetEMarkedCount * .20);
                        }
                        switch (GetEMarkedCount)
                        {
                            case 0:
                                return wDamage;
                            case 4:
                                return wDamage * 2;
                        }
                    }
                    return 0;
                }
            }

            public static float GetComboDamage
            {
                get
                {
                    var fComboDamage = 0d;
                    var t = GetTarget(W.Range * 2);
                    if (!t.IsValidTarget())
                        return 0;
                    /*
                    if (Q.IsReady())
                    {
                        var baseAttackSpeed = 0.656 + (0.656 / 100 * (Player.Level - 1) * 1.5);
                        var qExtraAttackSpeed = new double[] { 30, 50, 70, 90, 110 }[Q.Level - 1];
                        var attackDelay = (float) (baseAttackSpeed + (baseAttackSpeed / 100 * qExtraAttackSpeed));
                        attackDelay = (float) Math.Round(attackDelay, 2);

                        attackDelay *= 5;
                        attackDelay *= (float) Math.Floor(Player.TotalAttackDamage());
                        fComboDamage += attackDelay;
                    }
                    */
                    if (W.IsReady())
                    {
                        fComboDamage += GetWDamage;
                    }

                    if (E.IsReady())
                    {
                        fComboDamage += new double[] { 0, 60, 70, 80, 90, 100 }[E.Level] * 2;
                    }

                    if (R.IsReady())
                    {
                        fComboDamage += new double[] { 300, 400, 500 }[R.Level - 1] + Player.FlatMagicDamageMod;
                    }
                    return (float)fComboDamage;
                }
            }

            public static Obj_AI_Hero GetEMarkedEnemy
            {
                get
                {
                    return
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                enemy =>
                                    !enemy.IsDead &&
                                    enemy.IsValidTarget(W.Range + Orbwalking.GetRealAutoAttackRange(Player)))
                            .FirstOrDefault(
                                enemy => enemy.Buffs.Any(buff => buff.DisplayName == "TristanaEChargeSound"));
                }
            }

            public static int GetEMarkedCount
            {
                get
                {
                    if (GetEMarkedEnemy == null)
                        return 0;
                    return
                        GetEMarkedEnemy.Buffs.Where(buff => buff.DisplayName == "TristanaECharge")
                            .Select(xBuff => xBuff.Count)
                            .FirstOrDefault();
                }
            }
        }

        public void OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs castedSpell)
        {
            var target = castedSpell.Target as Obj_AI_Hero;
            if (GetValue<bool>("UseRM") && target != null && target.IsValidTarget() && target.IsEnemy)
            {
                var dmg = unit.GetSpellDamage(target, castedSpell.SData.Name);
                var hpLeft = target.Health - dmg;
                if (target.IsValidTarget(R.Range) && R.IsReady())
                {
                    var rDmg = R.GetDamage(target) - 30;
                    if (rDmg > hpLeft && hpLeft > 0)
                    {
                        R.Cast(target);
                    }
                }
            }
        }

        public override void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            var t = args.Target as Obj_AI_Hero;
            if (t != null && ComboActive && args.Unit.IsMe)
            {
                var useQ = Q.IsReady() && GetValue<bool>("UseQC");
                if (useQ && (Player.HealthPercentage() < 70 || t.HealthPercentage() < 60))
                    Q.Cast();

            }
        }

        /*public override void Orbwalking_OnAttack(AttackableUnit unit, AttackableUnit target)
        {
            var t = target as Obj_AI_Hero;
            if (t != null && ComboActive && unit.IsMe)
            {
                var useQ = Q.IsReady() && GetValue<bool>("UseQC");
                if (useQ && (t.HasBuff("TristanaECharge") || Player.HealthPercentage() < 70 || t.HealthPercentage() < 60))
                    Q.Cast();

            }
        }*/

        public override void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var t = target as Obj_AI_Hero;
            if (t != null && ComboActive && unit.IsMe)
            {
                var useE = E.IsReady() && GetValue<bool>("UseEC");
                if (useE && t.IsValidTarget() && canUseE(t))
                {
                    Q.Cast();
                    E.CastOnUnit(t);
                }

            }
        }

        private static bool canUseE(Obj_AI_Hero t)
        {
            if (Player.CountEnemiesInRange(W.Range + (E.Range / 2)) == 1)
                return true;

            return (Program.Config.Item("DontUseE" + t.ChampionName) != null &&
                    Program.Config.Item("DontUseE" + t.ChampionName).GetValue<bool>() == false);
        }

        public override void Game_OnGameUpdate(EventArgs args)
        {
            var getEMarkedEnemy = TristanaData.GetEMarkedEnemy;
            if (getEMarkedEnemy != null)
            {
                Orbwalker.ForceTarget(getEMarkedEnemy);
                TargetSelector.SetTarget(getEMarkedEnemy);
            }
            else
            {
                var attackRange = Orbwalking.GetRealAutoAttackRange(Player);
                TargetSelector.SetTarget(TargetSelector.GetTarget(attackRange, TargetSelector.DamageType.Physical));
            }

            Q.Range = Player.AttackRange + (Player.BoundingRadius * 2);
            E.Range = Player.AttackRange + Player.BoundingRadius;
            R.Range = Player.AttackRange + (Player.BoundingRadius * 2);

            var useW = W.IsReady() && GetValue<bool>("UseWC");
            var useWc = W.IsReady() && GetValue<bool>("UseWCS");
            var useE = E.IsReady() && GetValue<bool>("UseEC");
            var useR = R.IsReady() && GetValue<bool>("UseRC");
            var useRB = R.IsReady() && GetValue<bool>("UseRB");

            if (useRB && Player.HealthPercentage() < 40)
            {
                var enemy =
                    HeroManager.Enemies.OrderBy(target => target.TotalAttackDamage)
                        .LastOrDefault(targ => Player.Distance(targ) <= 275 && targ.IsMelee());
                if(enemy != null && enemy.IsValidTarget())
                {
                    R.CastOnUnit(enemy);
                }
            }

            if (ComboActive)
            {
                Obj_AI_Hero t;
                if (TristanaData.GetEMarkedEnemy != null)
                {
                    t = TristanaData.GetEMarkedEnemy;
                    TargetSelector.SetTarget(TristanaData.GetEMarkedEnemy);
                }
                else
                {
                    t = TristanaData.GetTarget(W.Range);
                }

                /*if (useE && canUseE(t))
                {
                    if (E.IsReady() && t.IsValidTarget(E.Range) && AttackNow)
                    {
                        E.CastOnUnit(t);
                        Q.Cast();
                    }
                }*/

                if (useW)
                {
                    t = TristanaData.GetTarget(W.Range);
                    if (t.IsValidTarget() && t.Health < TristanaData.GetWDamage)
                        W.Cast(t);
                }


                if (useWc)
                {
                    t = TristanaData.GetTarget(W.Range);
                    if (t.IsValidTarget() && TristanaData.GetEMarkedCount == 4)
                        W.Cast(t);
                }

                if (useR)
                {
                    var tar = TristanaData.GetTarget(R.Range);

                    if (!tar.IsValidTarget())
                        return;

                    if (Player.GetSpellDamage(tar, SpellSlot.R) - 30 > tar.Health &&
                        tar.Health > Player.GetAutoAttackDamage(tar, true))
                    {
                        R.CastOnUnit(tar);
                    }
                }
            }
        }

        private static float GetComboDamage(Obj_AI_Hero t)
        {
            return TristanaData.GetComboDamage;
        }

        public override void Drawing_OnDraw(EventArgs args)
        {
            // Draw marked enemy status
            var drawEMarksStatus = Program.Config.SubMenu("Drawings").Item("DrawEMarkStatus").GetValue<bool>();
            var drawEMarkEnemy = Program.Config.SubMenu("Drawings").Item("DrawEMarkEnemy").GetValue<Circle>();
            if (drawEMarksStatus || drawEMarkEnemy.Active)
            {
                var vText1 = vText;
                var getEMarkedEnemy = TristanaData.GetEMarkedEnemy;
                if (getEMarkedEnemy != null)
                {
                    if (drawEMarksStatus)
                    {
                        Utils.DrawText(
                            vText1, "" + TristanaData.GetEMarkedCount,
                            (int)getEMarkedEnemy.HPBarPosition.X + 145, (int)getEMarkedEnemy.HPBarPosition.Y + 5,
                            SharpDX.Color.Red);
                    }

                    if (drawEMarkEnemy.Active)
                    {
                        Render.Circle.DrawCircle(TristanaData.GetEMarkedEnemy.Position, 140f, drawEMarkEnemy.Color, 1);
                    }
                }
            }

            Spell[] spellList = { W };
            foreach (var spell in spellList)
            {
                var menuItem = GetValue<Circle>("Draw" + spell.Slot);
                if (menuItem.Active)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color, 1);
            }

            var drawE = Program.Config.SubMenu("Drawings").Item("DrawE").GetValue<Circle>();
            if (drawE.Active)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color, 1);
            }
        }

        public override bool ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQC" + Id, "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWC" + Id, "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseWCS" + Id, "Complete E stacks with W").SetValue(true));
            config.AddItem(new MenuItem("UseEC" + Id, "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseRC" + Id, "Use R").SetValue(true));

            config.AddSubMenu(new Menu("Don't Use E to", "DontUseE"));
            {
                foreach (var enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                {
                    config.SubMenu("DontUseE")
                        .AddItem(new MenuItem("DontUseE" + enemy.ChampionName, enemy.ChampionName).SetValue(false));
                }
            }
            return true;
        }

        public override bool HarassMenu(Menu config)
        {
            return true;
        }

        public override bool DrawingMenu(Menu config)
        {
            config.AddItem(new MenuItem("DrawW" + Id, "W range").SetValue(new Circle(true, Color.Beige)));

            var drawE = new Menu("Draw E", "DrawE");
            {
                drawE.AddItem(new MenuItem("DrawE", "E range").SetValue(new Circle(true, Color.Beige)));
                drawE.AddItem(
                    new MenuItem("DrawEMarkEnemy", "E Marked Enemy").SetValue(new Circle(true, Color.GreenYellow)));
                drawE.AddItem(new MenuItem("DrawEMarkStatus", "E Marked Status").SetValue(true));
                config.AddSubMenu(drawE);
            }

            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Damage After Combo").SetValue(true);
            Config.AddItem(dmgAfterComboItem);

            return true;
        }

        public override bool MiscMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseRM" + Id, "Use R KillSteal").SetValue(true));
            config.AddItem(new MenuItem("UseRB" + Id, "Use R to Knock Away Melee").SetValue(true));
            return true;
        }

        public override bool ExtrasMenu(Menu config)
        {

            return true;
        }

        public override bool LaneClearMenu(Menu config)
        {
            return true;
        }
    }
}
