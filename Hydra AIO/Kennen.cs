#region

using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX.Direct3D9;
using Font = SharpDX.Direct3D9.Font;
#endregion

namespace HydraAIO
{
    internal class Kennen : Champion
    {
        public static Spell Q;
        public static Spell W;
        public static Spell R;

        public static Font VText;

        public Kennen()
        {
            Utils.PrintMessage("H Kennen loaded");

            Q = new Spell(SpellSlot.Q, 1000f);
            W = new Spell(SpellSlot.W, 900f);
            R = new Spell(SpellSlot.R, 550f);
            Q.SetSkillshot(0.26f, 50f, 1700f, true, SkillshotType.SkillshotLine);

            VText = new Font(
                Drawing.Direct3DDevice,
                new FontDescription
                {
                    FaceName = "Courier new",
                    Height = 15,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.Default,
                });
        }

        public override void Drawing_OnDraw(EventArgs args)
        {
            Spell[] spellList = { Q, W, R };
            foreach (var spell in spellList)
            {
                var menuItem = GetValue<Circle>("Draw" + spell.Slot);
                if (menuItem.Active)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }
            if (GetValue<KeyBind>("DrawHarassToggleStatus").Active)
            {
                DrawHarassToggleStatus();
            }
        }

        public override void Game_OnGameUpdate(EventArgs args)
        {
            Orbwalker.SetAttack(!(Player.AttackRange < 0 || Player.Spellbook.GetSpell(SpellSlot.E).Name == "kennenlrcancel"));

            var targets =
                    ObjectManager.Get<Obj_AI_Hero>().Where(t => (t.IsValidTarget(900) && StormMarkStacks(t) > 0) || (Player.HasBuff("KennenShurikenStorm") && t.IsValidTarget(550))).ToList();
            var useW = GetValue<bool>("UseWH");
            if (useW)
            {
                foreach (Obj_AI_Hero enemy in targets)
                {
                    if (StormMarkStacks(enemy) > 0 && (StormMarkStacks(enemy) == 2 || StormMarkTimeLeft(enemy) < 0.5 || (Player.Distance(enemy) > 800 && Prediction.GetPrediction(enemy, 1).UnitPosition.Distance(Prediction.GetPrediction(Player, 1).UnitPosition) > 900) || (GetValue<bool>("UseWM") && enemy.Health < GetSurgeDamage(enemy))))
                    {
                        W.Cast();
                    }
                }
                if (targets.Count >= 3)
                {
                    W.Cast();
                }
            }

            if (LaneClearActive)
            {
                var useQ = GetValue<bool>("UseQL");

                if (Q.IsReady() && useQ)
                {
                    var vMinions = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range);
                    foreach (Obj_AI_Base minions in
                        vMinions.Where(
                            minions => minions.Health < ObjectManager.Player.GetSpellDamage(minions, SpellSlot.Q) && (Orbwalker.GetTarget() == null || minions.ToString() != Orbwalker.GetTarget().ToString())))
                        Q.Cast(minions);
                }
            }

            if ((!ComboActive && !HarassActive && !GetValue<KeyBind>("UseQHT").Active)) return;
            var tar = (Obj_AI_Hero)Orbwalker.GetTarget() ??
                        TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (tar.IsValidTarget() && AttackNow)
            {
                Q.Cast(tar);
            }
        }

        private void DrawHarassToggleStatus()
        {
            var xHarassStatus = "";
            if (GetValue<KeyBind>("UseQHT").Active)
                xHarassStatus = "Q";

            Utils.DrawText(
                VText, xHarassStatus, (int)Player.HPBarPosition.X + 145,
                (int)Player.HPBarPosition.Y + 5, SharpDX.Color.White);
        }

        public static double GetAttackDamage(Obj_AI_Base targ)
        {
            var damage = HasLightningShuriken() ? (0.4 + 0.1 * W.Level)*Player.TotalAttackDamage : 0;
            return Player.CalcDamage(targ, Damage.DamageType.Magical, damage) 
                + Player.CalcDamage(targ, Damage.DamageType.Physical, Player.TotalAttackDamage);
        }

        public static double GetSurgeDamage(Obj_AI_Base targ)
        {
            var damage = (65 + 30 * W.Level) + (Player.FlatMagicDamageMod * 0.55);
            return Player.CalcDamage(targ, Damage.DamageType.Magical, damage);
        }

        public static float StormMarkTimeLeft(Obj_AI_Base targ)
        {
            var buff = targ.Buffs.Find(b => b.Name == "kennenmarkofstorm");
            return (buff != null) ? buff.EndTime - Game.Time : 100f;
        }

        public static int StormMarkStacks(Obj_AI_Base targ)
        {
            var buff = targ.Buffs.Find(b => b.Name == "kennenmarkofstorm");
            return (buff != null) ? buff.Count : 0;
        }

        public static bool HasLightningShuriken()
        {
            return Player.HasBuff("kennendoublestrikelive");
        }

        public override bool ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQC" + Id, "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWC" + Id, "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseRC" + Id, "Use R").SetValue(true));
            return true;
        }

        public override bool HarassMenu(Menu config)
        {
            config.AddItem(
                new MenuItem("UseQHT" + Id, "Use Q (Toggle)").SetValue(new KeyBind("H".ToCharArray()[0],
                    KeyBindType.Toggle)));
            config.AddItem(new MenuItem("UseQH" + Id, "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWH" + Id, "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseRH" + Id, "Use R").SetValue(true));
            config.AddItem(new MenuItem("DrawHarassToggleStatus" + Id, "Draw Toggle Status").SetValue(true));
            return true;
        }

        public override bool DrawingMenu(Menu config)
        {
            config.AddItem(
                new MenuItem("DrawQ" + Id, "Q range").SetValue(new Circle(true,
                    Color.FromArgb(100, 255, 0, 255))));
            config.AddItem(
                new MenuItem("DrawW" + Id, "W range").SetValue(new Circle(false,
                    Color.FromArgb(100, 255, 0, 255))));
            config.AddItem(
                new MenuItem("DrawR" + Id, "R range").SetValue(new Circle(false,
                    Color.FromArgb(100, 255, 0, 255))));
            return true;
        }

        public override bool MiscMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseWM" + Id, "Use W To Killsteal").SetValue(true));
            return true;
        }

        public override bool ExtrasMenu(Menu config)
        {

            return true;
        }
        public override bool LaneClearMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQL" + Id, "Use Q").SetValue(true));
            return true;
        }

    }
}
