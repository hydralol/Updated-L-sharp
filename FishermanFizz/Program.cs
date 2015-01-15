using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace FishermanFizz
{
    internal class Program
    {
        private const string ChampionName = "Fizz";
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, E2, R;
        public static SpellSlot IgniteSlot;
        public static Items.Item DFG;
        public static int JumpState;
        public static float Time;
        public static bool Called;

        public static Menu Config;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
            Obj_AI_Base.OnProcessSpellCast += OnProcSpell;
            Orbwalking.BeforeAttack += BAttack;
        }

        private static void OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != ChampionName)
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 550);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 400);
            E2 = new Spell(SpellSlot.E, 400);
            R = new Spell(SpellSlot.R, 1200);  //1275 True

            DFG = Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline ? new Items.Item(3188, 750) : new Items.Item(3128, 750);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            E.SetSkillshot(0.5f, 120, 1300, false, SkillshotType.SkillshotCircle);
            E2.SetSkillshot(0.5f, 400, 1300, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.5f, 250f, 1200f, false, SkillshotType.SkillshotLine);

            Config = new Menu("FishermanFizz", "FishermanFizz", true);

            Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseDFG", "DFG Shark Target").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQh", "Use Q").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWh", "Use W").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEh", "Use E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQlc", "Use Q").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseElc", "Use E").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("LMana", "Min Mana").SetValue(new Slider(50, 100, 0)));

            Config.AddSubMenu(new Menu("JungleClear", "JungleClear"));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("UseQj", "Use Q").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("UseWj", "Use W").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("UseEj", "Use E").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("JungleClearActive", "JungleClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("qRange", "Q range").SetValue(new Circle(true, Color.FromArgb(255, 125, 200, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("eRange", "E range").SetValue(new Circle(true, Color.FromArgb(200, 125, 200, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("rRange", "R range").SetValue(new Circle(true, Color.FromArgb(200, 125, 200, 255))));

            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("UseIgnite", "Ignite Killable").SetValue(true));

            Config.AddToMainMenu();

            Drawing.OnDraw += OnDraw;
            Game.OnGameUpdate += OnGameUpdate;
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (Time + 1f < Game.Time && !Called)
            {
                Called = true;
                JumpState = 0;
            }
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                Harass();
            }
            if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
            {
                LaneClear();
            }
            if (Config.Item("JungleClearActive").GetValue<KeyBind>().Active)
            {
                JungleClear();
            }
            if (Config.Item("UseIgnite").GetValue<bool>())
            {
                IgniteKS();
            }
        }

        private static void Combo()
        {
            Obj_AI_Hero qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            Obj_AI_Hero eTarget = TargetSelector.GetTarget(800, TargetSelector.DamageType.Magical);
            Obj_AI_Hero rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            if (rTarget.IsValidTarget(R.Range) && R.IsReady())
            {
                DFG.Cast(rTarget);
            }
            if (qTarget != null && Config.Item("UseQ").GetValue<bool>() &&
                (qTarget.IsValidTarget(Q.Range) && Q.IsReady()))
            {
                Q.CastOnUnit(qTarget);
            }
            if (eTarget != null && Config.Item("UseE").GetValue<bool>() && (eTarget.IsValidTarget(800) && E.IsReady()))
            {
                if (Vector3.Distance(Player.ServerPosition, eTarget.ServerPosition) < 800 && JumpState != 1 &&
                    E.GetPrediction(eTarget).Hitchance >= HitChance.High)
                {
                    E.Cast(eTarget, true);
                }

                if (Vector3.Distance(Player.ServerPosition, eTarget.ServerPosition) < E.Range &&
                    Vector3.Distance(Player.ServerPosition, eTarget.ServerPosition) > 300 && JumpState == 1 &&
                    E2.GetPrediction(eTarget).Hitchance >= HitChance.High)
                {
                    E2.Cast(eTarget, true);
                }
            }
            if (rTarget == null || !Config.Item("UseR").GetValue<bool>()) return;
            if (!rTarget.IsValidTarget(R.Range) || !R.IsReady()) return;
            if (!(R.GetDamage(rTarget) > rTarget.Health) &&
                !(R.GetDamage(rTarget) + W.GetDamage(rTarget) + E.GetDamage(rTarget) + Q.GetDamage(rTarget) >
                  rTarget.Health)) return;
            if (R.GetPrediction(rTarget).Hitchance < HitChance.High) return;
            R.Cast(rTarget, true);
        }

        private static void Harass()
        {
            Obj_AI_Hero qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            Obj_AI_Hero eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (qTarget != null && Config.Item("UseQh").GetValue<bool>() &&
                (qTarget.IsValidTarget(Q.Range) && Q.IsReady()))
            {
                Q.CastOnUnit(qTarget);
            }

            if (eTarget == null || !Config.Item("UseEh").GetValue<bool>()) return;
            if (!eTarget.IsValidTarget(E.Range * 2) || !E.IsReady()) return;
            if (Vector3.Distance(Player.ServerPosition, eTarget.ServerPosition) < E.Range && JumpState != 1)
            {
                if (E.GetPrediction(eTarget).Hitchance >= HitChance.High)
                {
                    E.Cast(eTarget, true);
                }
            }

            if (!(Vector3.Distance(Player.ServerPosition, eTarget.ServerPosition) < E.Range) ||
                !(Vector3.Distance(Player.ServerPosition, eTarget.ServerPosition) > 300) || JumpState != 1) return;
            if (E2.GetPrediction(eTarget).Hitchance >= HitChance.High)
            {
                E2.Cast(eTarget, true);
            }
        }


        private static void LaneClear()
        {
            var minions = MinionManager.GetMinions(Player.ServerPosition, 800);
            var eMinions = MinionManager.GetMinions(Player.ServerPosition, E.Range + E.Width + 30);
            var FMana = Config.Item("LMana").GetValue<Slider>().Value;
            var MPercent = Player.Mana * 100 / Player.MaxMana;
            var useQlc = Config.Item("UseQlc").GetValue<bool>();
            var useElc = Config.Item("UseElc").GetValue<bool>();

            foreach (var min in minions.Where(min => useQlc && MPercent >= FMana).Where(min => Q.IsReady() && Q.GetDamage(min) >= min.Health))
            {
                Q.CastOnUnit(min, true);
            }
            if (!useElc || !(MPercent >= FMana)) return;
            if (!E.IsReady() || JumpState != 0) return;
            var ePos = E.GetCircularFarmLocation(eMinions);
            if (ePos.MinionsHit >= 1)
                E.Cast(ePos.Position, true);
        }

        private static void JungleClear()
        {
            var qJungle = MinionManager.GetMinions(Player.ServerPosition, 800, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var eJungle = MinionManager.GetMinions(Player.ServerPosition, E.Range + E.Width + 30, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var wJungle = MinionManager.GetMinions(Player.ServerPosition, Player.AttackRange, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var useQj = Config.Item("UseQj").GetValue<bool>();
            var useWj = Config.Item("UseWj").GetValue<bool>();
            var useEj = Config.Item("UseEj").GetValue<bool>();

            foreach (var jmob in qJungle.Where(jmob => useQj).Where(jmob => Q.IsReady()))
            {
                Q.CastOnUnit(jmob, true);
            }
            if (useWj && wJungle.Count >= 1)
            {
                W.Cast();
            }
            if (!useEj) return;
            if (!E.IsReady() || JumpState != 0) return;
            var ePos = E.GetCircularFarmLocation(eJungle);
            if (ePos.MinionsHit >= 1)
                E.Cast(ePos.Position, true);
        }

        private static void IgniteKS()
        {
            foreach (var Champion in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (!(Player.Distance(Champion) <= 600) || IgniteSlot == SpellSlot.Unknown ||
                    Player.Spellbook.CanUseSpell(IgniteSlot) != SpellState.Ready ||
                    !(ObjectManager.Player.GetSummonerSpellDamage(Champion, Damage.SummonerSpell.Ignite) - 5 > Champion.Health)) continue;
                Player.Spellbook.CastSpell(IgniteSlot, Champion);
            }
        }

        private static void BAttack(Orbwalking.BeforeAttackEventArgs args)
        {

            if (args.Target.Type != GameObjectType.obj_AI_Hero) return;
            if (!Config.Item("UseW").GetValue<bool>() && !Config.Item("UseWh").GetValue<bool>()) return;
            W.Cast();
        }

        private static void OnProcSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.Name != Player.Name) return;
            if (args.SData.Name != "FizzJump") return;
            JumpState = 1;
            Time = Game.Time;
            Called = false;
        }

        private static void OnDraw(EventArgs args)
        {
            var qCircle = Config.Item("qRange").GetValue<Circle>();
            var eCircle = Config.Item("eRange").GetValue<Circle>();
            var rCircle = Config.Item("rRange").GetValue<Circle>();

            if (qCircle.Active && Q.IsReady())
                Utility.DrawCircle(Player.Position, Q.Range, qCircle.Color);
            if (eCircle.Active && E.IsReady())
                Utility.DrawCircle(Player.Position, E.Range, rCircle.Color);
            if (rCircle.Active && R.IsReady())
                Utility.DrawCircle(Player.Position, R.Range, rCircle.Color);
        }
    }

    //Credits
    //fueledbyflux: Bass Logic, Tweaked and rewritten by me.
    //TC-Crew Darius: Ignite killable Steal
}