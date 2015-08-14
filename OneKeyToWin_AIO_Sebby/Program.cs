
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;

namespace OneKeyToWin_AIO_Sebby
{
    class RecallInfo
    {
        public int RecallID { get; set; }
        public float RecallStart { get; set; }
        public int RecallNum { get; set; }
    }
    class VisableInfo
    {
        public int VisableID { get; set; }
        public Vector3 LastPosition { get; set; }
        public float time { get; set; }
        public Vector3 PredictedPos { get; set; }
    }

    class champions { public Obj_AI_Hero Player; }

    internal class Program
    {
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;

        public static Spell Q, W, E, R, DrawSpell;

        public static string championMsg;
        public static float JungleTime, DrawSpellTime=0;
        public static Obj_AI_Hero jungler = ObjectManager.Player;
        public static int timer, HitChanceNum = 4, tickNum = 4, tickIndex = 0;
        public static Obj_SpawnPoint enemySpawn;
        public static Core.PredictionOutput DrawSpellPos;
        public static bool 
            tickSkip = true,
            RangeFix = true,
            FastMode = true,
            ColFix = true,
            NewWay = true,
            tryAA = true,
            IgnoreNoMove = true;

        public static List<RecallInfo> RecallInfos = new List<RecallInfo>();

        public static List<VisableInfo> VisableInfo = new List<VisableInfo>();
        public static List<Obj_AI_Hero> Enemies = new List<Obj_AI_Hero>();
        public static List<Obj_AI_Hero> Allies = new List<Obj_AI_Hero>();

        public static Items.Item 
            WardN = new Items.Item(2044, 600f),
            TrinketN = new Items.Item(3340, 600f),
            SightStone = new Items.Item(2049, 600f),
            FarsightOrb = new Items.Item(3342, 4000f),
            ScryingOrb = new Items.Item(3363, 3500f);

        static void Main(string[] args) { CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;}

        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        private static void GameOnOnGameLoad(EventArgs args)
        {
            Q = new Spell(SpellSlot.Q);
            E = new Spell(SpellSlot.E);
            W = new Spell(SpellSlot.W);
            R = new Spell(SpellSlot.R);

            Config = new Menu("OneKeyToWin AIO", "OneKeyToWin_AIO" + ObjectManager.Player.ChampionName, true);
            Config.SubMenu("About OKTW©").AddItem(new MenuItem("watermark", "Watermark").SetValue(true));
            Config.SubMenu("About OKTW©").AddItem(new MenuItem("debug", "Debug").SetValue(false));
            //Config.SubMenu("About OKTW©").SubMenu("Performance OKTW©").AddItem(new MenuItem("pre", "OneSpellOneTick©").SetValue(true));
            //Config.SubMenu("About OKTW©").SubMenu("Performance OKTW©").AddItem(new MenuItem("0", "OneSpellOneTick© is tick management"));
            //Config.SubMenu("About OKTW©").SubMenu("Performance OKTW©").AddItem(new MenuItem("1", "ON - increase fps"));
           // Config.SubMenu("About OKTW©").SubMenu("Performance OKTW©").AddItem(new MenuItem("2", "OFF - normal mode"));
            Config.SubMenu("About OKTW©").AddItem(new MenuItem("0", "OneKeyToWin© by Sebby"));
            Config.SubMenu("About OKTW©").AddItem(new MenuItem("1", "visit joduska.me"));
            Config.SubMenu("About OKTW©").AddItem(new MenuItem("2", "DONATE: kaczor.sebastian@gmail.com"));

            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("3", "Annie "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("4", "Jinx "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("5", "Ezreal "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("6", "KogMaw "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("7", "Sivir "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("8", "Ashe "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("9", "Miss Fortune "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("10", "Quinn "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("11", "Graves "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("12", "Urgot "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("13", "Orianna "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("14", "Caitlyn "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("15", "Anivia "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("16", "Darius "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("17", "Corki "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("18", "Vayne "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("19", "Lucian "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("20", "Ekko "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("21", "Twitch "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("22", "Tristana "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("23", "Xerath "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("24", "Kayle "));
            Config.SubMenu("About OKTW©").SubMenu("Supported champions:").AddItem(new MenuItem("25", "Thresh "));

            Config.SubMenu("Utility, Draws OKTW©").AddItem(new MenuItem("disableDraws", "Disable Utility draws").SetValue(false));
            Config.SubMenu("Utility, Draws OKTW©").SubMenu("Draw AAcirlce OKTW© style").AddItem(new MenuItem("OrbDraw", "Draw AAcirlce OKTW© style").SetValue(false));
            Config.SubMenu("Utility, Draws OKTW©").SubMenu("Draw AAcirlce OKTW© style").AddItem(new MenuItem("orb", "Orbwalker target OKTW© style").SetValue(true));
            Config.SubMenu("Utility, Draws OKTW©").SubMenu("Draw AAcirlce OKTW© style").AddItem(new MenuItem("1", "pls disable Orbwalking > Drawing > AAcirlce"));
            Config.SubMenu("Utility, Draws OKTW©").SubMenu("Draw AAcirlce OKTW© style").AddItem(new MenuItem("2", "My HP: 0-30 red, 30-60 orange,60-100 green"));

            Config.SubMenu("Utility, Draws OKTW©").SubMenu("GankTimer").AddItem(new MenuItem("timer", "GankTimer").SetValue(true));

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                Config.SubMenu("Utility, Draws OKTW©").SubMenu("GankTimer").SubMenu("Custome jungler (select one)").AddItem(new MenuItem("ro" + enemy.ChampionName, enemy.ChampionName).SetValue(false));

            Config.SubMenu("Utility, Draws OKTW©").SubMenu("GankTimer").AddItem(new MenuItem("1", "RED - be careful"));
            Config.SubMenu("Utility, Draws OKTW©").SubMenu("GankTimer").AddItem(new MenuItem("2", "ORANGE - you have time"));
            Config.SubMenu("Utility, Draws OKTW©").SubMenu("GankTimer").AddItem(new MenuItem("3", "GREEN - jungler visable"));
            Config.SubMenu("Utility, Draws OKTW©").SubMenu("GankTimer").AddItem(new MenuItem("4", "CYAN jungler dead - take objectives"));

            Config.SubMenu("Utility, Draws OKTW©").SubMenu("ChampionInfo").AddItem(new MenuItem("championInfo", "Game Info").SetValue(true));
            Config.SubMenu("Utility, Draws OKTW©").SubMenu("ChampionInfo").AddItem(new MenuItem("ShowKDA", "Show KDA").SetValue(true));
            Config.SubMenu("Utility, Draws OKTW©").SubMenu("ChampionInfo").AddItem(new MenuItem("GankAlert", "Gank Alert").SetValue(true));

            Config.SubMenu("Utility, Draws OKTW©").SubMenu("ChampionInfo").AddItem(new MenuItem("posX", "posX").SetValue(new Slider(20, 100, 0)));
            Config.SubMenu("Utility, Draws OKTW©").SubMenu("ChampionInfo").AddItem(new MenuItem("posY", "posY").SetValue(new Slider(10, 100, 0)));

            Config.SubMenu("Utility, Draws OKTW©").AddItem(new MenuItem("HpBar", "Dmg BAR OKTW© style").SetValue(true));
            Config.SubMenu("Utility, Draws OKTW©").AddItem(new MenuItem("ShowClicks", "Show enemy clicks").SetValue(true));

            Config.SubMenu("AutoWard OKTW©").AddItem(new MenuItem("AutoWard", "Auto Ward").SetValue(true));
            Config.SubMenu("AutoWard OKTW©").AddItem(new MenuItem("AutoWardBlue", "Auto Blue Trinket").SetValue(true));
            Config.SubMenu("AutoWard OKTW©").AddItem(new MenuItem("AutoWardCombo", "Only combo mode").SetValue(true));

            Config.SubMenu("Prediction OKTW©").SubMenu("Custome Prediction 4").AddItem(new MenuItem("RangeFix", "1 MaxRange Fix", true).SetValue(true));
            Config.SubMenu("Prediction OKTW©").SubMenu("Custome Prediction 4").AddItem(new MenuItem("FastMode", "2 Fast Cast Mode", true).SetValue(true));
            Config.SubMenu("Prediction OKTW©").SubMenu("Custome Prediction 4").AddItem(new MenuItem("ColFix", "3 Custome Collision(can drop fps)", true).SetValue(false));
            Config.SubMenu("Prediction OKTW©").SubMenu("Custome Prediction 4").AddItem(new MenuItem("NewWay", "4 Cast only on new pathway", true).SetValue(false));
            Config.SubMenu("Prediction OKTW©").SubMenu("Custome Prediction 4").AddItem(new MenuItem("tryAA", "5 Cast if target autoattacking", true).SetValue(true));
            Config.SubMenu("Prediction OKTW©").SubMenu("Custome Prediction 4").AddItem(new MenuItem("IgnoreNoMove", "6 Ignore Not-Moving targets", true).SetValue(false));
            Config.SubMenu("Prediction OKTW©").AddItem(new MenuItem("0", "0 - common normal"));
            Config.SubMenu("Prediction OKTW©").AddItem(new MenuItem("1", "1 - common high"));
            Config.SubMenu("Prediction OKTW©").AddItem(new MenuItem("2", "2 - common high + max range fix"));
            Config.SubMenu("Prediction OKTW©").AddItem(new MenuItem("3", "3 - OKTW + max range fix + waypionts analyzer "));
            Config.SubMenu("Prediction OKTW©").AddItem(new MenuItem("4", "4 - OKTW Custome Prediction 4"));
            Config.SubMenu("Prediction OKTW©").AddItem(new MenuItem("5", "5 - OKTW NewCommon Prediction concept"));
            Config.SubMenu("Prediction OKTW©").AddItem(new MenuItem("debugPred", "Draw Aiming 3,4").SetValue(false));
            Config.SubMenu("Prediction OKTW©").AddItem(new MenuItem("Hit", "Prediction OKTW©", true).SetValue(new Slider(4, 5, 0)));

            new Summoners().LoadOKTW();
            new Activator().LoadOKTW();
            new Core.OKTWward().LoadOKTW();
            new Core.AutoLvlUp().LoadOKTW();
            new OktwCommon().LoadOKTW();
            new Core.OneKeyToBrain().LoadOKTW();

            //new Core.OKTWfarmLogic().LoadOKTW();
            if (Config.Item("debug").GetValue<bool>())
            {
                new Core.OKTWlab().LoadOKTW();
            }

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
                
            switch (Player.ChampionName)
            {
                case "Jinx":
                    new Jinx().LoadOKTW();
                    break;
                case "Sivir":
                    new Sivir().LoadOKTW();
                    break;
                case "Ezreal":
                    new Ezreal().LoadOKTW();
                    break;
                case "KogMaw":
                    new KogMaw().LoadOKTW();
                    break;
                case "Annie":
                    new Annie().LoadOKTW();
                    break;
                case "Ashe":
                    new Ashe().LoadOKTW();
                    break;
                case "MissFortune":
                    new MissFortune().LoadOKTW();
                    break;
                case "Quinn":
                    new Quinn().LoadOKTW();
                    break;
                case "Kalista":
                    new Kalista().LoadOKTW();
                    break;
                case "Caitlyn":
                    new Caitlyn().LoadOKTW();
                    break;
                case "Graves":
                    new Graves().LoadOKTW();
                    break;
                case "Urgot":
                    new Urgot().LoadOKTW();
                    break;
                case "Anivia":
                    new Anivia().LoadOKTW();
                    break;
                case "Orianna":
                    new Orianna().LoadOKTW();
                    break;
                case "Ekko":
                    new Ekko().LoadOKTW();
                    break;
                case "Vayne":
                    new Vayne().LoadOKTW();
                    break;
                case "Lucian":
                    new Lucian().LoadOKTW();
                    break;
                case "Darius":
                    new Champions.Darius().LoadOKTW();
                    break;
                case "Blitzcrank":
                    new Champions.Blitzcrank().LoadOKTW();
                    break;
                case "Corki":
                    new Champions.Corki().LoadOKTW();
                    break;
                case "Varus":
                    new Champions.Varus().LoadOKTW();
                    break;
                case "Twitch":
                    new Champions.Twitch().LoadOKTW();
                    break;
                case "Tristana":
                    new Champions.Tristana().LoadMenuOKTW();
                    break;
                case "Xerath":
                    new Champions.Xerath().LoadOKTW();
                    break;
                case "Syndra":
                    new Champions.Syndra().LoadOKTW();
                    break;
                case "Kayle":
                    new Champions.Kayle().LoadOKTW();
                    break;
                case "Thresh":
                    new Champions.Thresh().LoadOKTW();
                    break;
                case "Draven":
                    new Champions.Draven().LoadOKTW();
                    break;
            }

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (IsJungler(hero) && hero.IsEnemy)
                {
                    jungler = hero;
                }
                if (hero.IsEnemy)
                    Enemies.Add(hero);
                if (hero.IsAlly)
                    Allies.Add(hero);
            }

            //new AfkMode().LoadOKTW();
            Config.AddToMainMenu();
            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnTeleport += Obj_AI_Base_OnTeleport;
            Drawing.OnDraw += OnDraw;
        }

        private static void OnUpdate(EventArgs args)
        {
            tickIndex++;

            if (tickIndex > 4)
                tickIndex = 0;

            if (LagFree(0))
            {
                //tickSkip = Config.Item("pre").GetValue<bool>();
                HitChanceNum = Config.Item("Hit", true).GetValue<Slider>().Value;
                IgnoreNoMove = Config.Item("IgnoreNoMove", true).GetValue<bool>();
                RangeFix = Config.Item("RangeFix", true).GetValue<bool>();
                FastMode = Config.Item("FastMode", true).GetValue<bool>();
                ColFix = Config.Item("ColFix", true).GetValue<bool>();
                NewWay = Config.Item("NewWay", true).GetValue<bool>();
                tryAA = Config.Item("tryAA", true).GetValue<bool>();

                JunglerTimer();
                if (!Player.IsRecalling())
                    AutoWard();
            }
        }

        public static void AutoWard()
        {
            foreach (var enemy in Enemies.Where(enemy => enemy.IsValid))
            {
                if (enemy.IsVisible && !enemy.IsDead && enemy != null && enemy.IsValidTarget())
                {
                    if (Prediction.GetPrediction(enemy, 0.4f).CastPosition != null)
                    {
                        var prepos = Prediction.GetPrediction(enemy, 0.4f).CastPosition;
                        VisableInfo.RemoveAll(x => x.VisableID == enemy.NetworkId);
                        VisableInfo.Add(new VisableInfo() { VisableID = enemy.NetworkId, LastPosition = enemy.Position, time = Game.Time, PredictedPos = prepos });
                    }
                }
                else if (enemy.IsDead)
                {
                    VisableInfo.RemoveAll(x => x.VisableID == enemy.NetworkId);
                }
                else if (!enemy.IsDead)
                {
                    var need = VisableInfo.Find(x => x.VisableID == enemy.NetworkId);
                    if (need == null || need.PredictedPos == null)
                        return;

                    if (Player.ChampionName == "Quinn" && W.IsReady() && Game.Time - need.time > 0.5 && Game.Time - need.time < 4 && need.PredictedPos.Distance(Player.Position) < 1500 && Config.Item("autoW").GetValue<bool>())
                    {
                        W.Cast();
                        return;
                    }
                    if (Player.ChampionName == "Ashe" && E.IsReady() && Player.Spellbook.GetSpell(SpellSlot.E).Ammo > 1 && Player.CountEnemiesInRange(800) == 0 && Game.Time - need.time > 3 && Game.Time - need.time < 1 && Config.Item("autoE").GetValue<bool>())
                    {
                        if (need.PredictedPos.Distance(Player.Position) < 3000)
                        {
                            E.Cast(ObjectManager.Player.Position.Extend(need.PredictedPos, 5000));
                            return;
                        }

                    }
                    if (Player.ChampionName == "MissFortune" && E.IsReady() && Game.Time - need.time > 0.5 && Game.Time - need.time < 2 && Combo && Player.Mana > 200f)
                    {
                        if (need.PredictedPos.Distance(Player.Position) < 800)
                        {
                            E.Cast(ObjectManager.Player.Position.Extend(need.PredictedPos, 800));
                            return;
                        }
                    }
                    if (Player.ChampionName == "Kalista" && W.IsReady() && Game.Time - need.time > 3 && Game.Time - need.time < 4 && !Combo && Config.Item("autoW").GetValue<bool>() && ObjectManager.Player.Mana > 300f)
                    {
                        if (need.PredictedPos.Distance(Player.Position) > 1500 && need.PredictedPos.Distance(Player.Position) < 4000)
                        {
                            W.Cast(ObjectManager.Player.Position.Extend(need.PredictedPos, 5500));
                            return;
                        }

                    }
                    if (Player.ChampionName == "Caitlyn" && W.IsReady() && Game.Time - need.time < 2 && Player.Mana > 200f && !Player.IsWindingUp && Config.Item("bushW").GetValue<bool>())
                    {
                        if (need.PredictedPos.Distance(Player.Position) < 800)
                        {
                            W.Cast(need.PredictedPos);
                            return;
                        }
                    }
                    if (Game.Time - need.time < 4 )
                    {
                        if (Config.Item("AutoWardCombo").GetValue<bool>() && !Combo)
                            return;
                        if (NavMesh.IsWallOfGrass(need.PredictedPos, 0))
                        {
                            if (need.PredictedPos.Distance(Player.Position) < 600 && Config.Item("AutoWard").GetValue<bool>())
                            {
                                if (TrinketN.IsReady())
                                {
                                    TrinketN.Cast(need.PredictedPos);
                                    need.time = Game.Time - 5;
                                }
                                else if (SightStone.IsReady())
                                {
                                    SightStone.Cast(need.PredictedPos);
                                    need.time = Game.Time - 5;
                                }
                                else if (WardN.IsReady())
                                {
                                    WardN.Cast(need.PredictedPos);
                                    need.time = Game.Time - 5;
                                }
                            }
                            if (need.PredictedPos.Distance(Player.Position) < 1400 && Config.Item("AutoWardBlue").GetValue<bool>())
                            {
                                if (FarsightOrb.IsReady())
                                {
                                    FarsightOrb.Cast(need.PredictedPos);
                                    need.time = Game.Time - 5;
                                }
                                else if (ScryingOrb.IsReady())
                                {
                                    ScryingOrb.Cast(need.PredictedPos);
                                    need.time = Game.Time - 5;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void JunglerTimer()
        {
            if (Config.Item("timer").GetValue<bool>() && jungler != null && jungler.IsValid)
            {
                foreach (var enemy in Enemies.Where(enemy => enemy.IsValid))
                {
                    if (Config.Item("ro" + enemy.ChampionName) != null && Config.Item("ro" + enemy.ChampionName).GetValue<bool>())
                        jungler = enemy;
                }

                if (jungler.IsDead)
                {
                    enemySpawn = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy);
                    timer = (int)(enemySpawn.Position.Distance(ObjectManager.Player.Position) / 370);
                }
                else if (jungler.IsVisible && jungler.IsValid)
                {
                    float Way = 0;
                    var JunglerPath = Player.GetPath(Player.Position, jungler.Position);
                    var PointStart = Player.Position;
                    if (JunglerPath == null)
                        return;
                    foreach (var point in JunglerPath)
                    {
                        if (PointStart.Distance(point) > 0)
                        {
                            Way += PointStart.Distance(point);
                            PointStart = point;
                        }
                    }
                    timer = (int)(Way / jungler.MoveSpeed);
                }
            }
        }

        public static bool LagFree(int offset)
        {
            //if (!tickSkip)
               // return true;
            if (tickIndex == offset)
                return true;
            else
                return false;
        }

        public static bool Farm { get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear) || (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed) || (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit); } }

        public static bool Combo { get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo); } }

        public static bool LaneClear { get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear); } }

        private static bool IsJungler(Obj_AI_Hero hero) { return hero.Spellbook.Spells.Any(spell => spell.Name.ToLower().Contains("smite")); }

        public static bool ValidUlt(Obj_AI_Hero target)
        {
            if (target.HasBuffOfType(BuffType.PhysicalImmunity) || target.HasBuffOfType(BuffType.SpellImmunity)
            || target.IsZombie || target.HasBuffOfType(BuffType.Invulnerability) || target.HasBuffOfType(BuffType.SpellShield))
                return false;
            else
                return true;
        }

        public static float GetRealDmg(Spell QWER, Obj_AI_Hero target)
        {
            if (Orbwalking.InAutoAttackRange(target) || target.CountAlliesInRange(300) > 0)
                return QWER.GetDamage(target) + (float)ObjectManager.Player.GetAutoAttackDamage(target) * 2;
            else
                return QWER.GetDamage(target);
        }

        public static void CastSpell(Spell QWER, Obj_AI_Base target)
        {
            if (HitChanceNum == 5)
            {
                Core.SkillshotType CoreType2 = Core.SkillshotType.SkillshotLine;
                bool aoe2 = false;
                if (QWER.Type == SkillshotType.SkillshotCircle)
                {
                    CoreType2 = Core.SkillshotType.SkillshotCircle;
                    aoe2 = true;
                }
                if (QWER.Width > 100)
                    aoe2 = true;
                var predInput2 = new Core.PredictionInput
                {
                    Aoe = aoe2,
                    Collision = QWER.Collision,
                    Speed = QWER.Speed,
                    Delay = QWER.Delay,
                    Range = QWER.Range,
                    From = Player.ServerPosition,
                    Radius = QWER.Width,
                    Unit = target,
                    Type = CoreType2
                };
                var poutput2 = Core.Prediction.GetPrediction(predInput2);

                //var poutput2 = QWER.GetPrediction(target);
                if (Game.Time - DrawSpellTime > 0.5)
                {
                    DrawSpell = QWER;
                    DrawSpellTime = Game.Time;

                }

                DrawSpellPos = poutput2;
                if(poutput2.Hitchance == Core.HitChance.VeryHigh)
                    QWER.Cast(poutput2.CastPosition);
                return;
            }

            if (target.Path.Count() > 1)
                return;
            
            var poutput = QWER.GetPrediction(target);
            
            //var poutput2 = QWER.GetPrediction(target);
            if (Game.Time - DrawSpellTime > 0.5)
            {
                DrawSpell = QWER;
                DrawSpellTime = Game.Time;
                
            }
            
            //DrawSpellPos = poutput;
            if (ColFix  && HitChanceNum == 4)
            {
                if (QWER.Collision && OktwCommon.GetCollision(target, QWER, false, true))
                    return;
            }
            else
            {
                var col = poutput.CollisionObjects.Count(ColObj => ColObj.IsEnemy && ColObj.IsMinion && !ColObj.IsDead);
                if (col > 0)
                {
                    return;
                }
            }

            if ((int)poutput.Hitchance > 4 && target.HasBuffOfType(BuffType.Slow))
            {
                QWER.Cast(poutput.CastPosition);
                return;
            }

            if (target.HasBuff("Recall") || poutput.Hitchance == HitChance.Immobile )
            {
                QWER.Cast(poutput.CastPosition);
                return;
            }

            if (poutput.Hitchance == HitChance.Dashing && QWER.Delay < 0.30f)
            {
                QWER.Cast(poutput.CastPosition);
                return;
            }
            
            if (HitChanceNum == 4)
            {
                if ((int)poutput.Hitchance < 5)
                    return;

                if (NewWay && (int)poutput.Hitchance < 6)
                    return;             

                float fixRange;
                
                if (RangeFix)
                    fixRange = (target.MoveSpeed * (Player.ServerPosition.Distance(target.ServerPosition) / QWER.Speed + QWER.Delay)) / 2;
                else
                    fixRange = 0;

                if (target.IsWindingUp)
                {
                    
                    if (!tryAA)
                        return;
                    debug("IsWinding: ");
                    if (Player.Distance(target.ServerPosition) < QWER.Range - fixRange)
                    {
                        if (FastMode)
                            QWER.Cast(poutput.CastPosition);
                        else
                            QWER.Cast(target);                        
                    }
                    
                    return;
                }
                else if (target.Path.Count() == 0 && target.Position == target.ServerPosition )
                {
                    
                    if (IgnoreNoMove)
                        return;
                    debug("NotMove");
                    if (Player.Distance(target.ServerPosition) < QWER.Range - fixRange)
                    {
                        if (FastMode)
                            QWER.Cast(poutput.CastPosition);
                        else
                            QWER.Cast(target);
                    }

                    return;
                }

                var LastWaypiont = target.GetWaypoints().Last().To3D();

                if (target.ServerPosition.Distance(Player.ServerPosition) < LastWaypiont.Distance(Player.ServerPosition) - fixRange)
                {
                    if (FastMode)
                        QWER.Cast(poutput.CastPosition);
                    else
                        QWER.Cast(target);

                    debug("Run" );
                }
                else if (Player.Distance(target.ServerPosition) < QWER.Range - fixRange)
                {
                    float BackToFront = ((target.MoveSpeed * QWER.Delay) + (Player.Distance(target.ServerPosition) / QWER.Speed));
                    float SiteToSite = (BackToFront * 2) - QWER.Width;

                    if ((target.ServerPosition.Distance(LastWaypiont) > SiteToSite
                        || Math.Abs(Player.Distance(LastWaypiont) - Player.Distance(target.ServerPosition)) > BackToFront)
                        || Player.Distance(target.ServerPosition) < SiteToSite + target.BoundingRadius * 2
                        || Player.Distance(LastWaypiont) < BackToFront)
                    {
                        if (FastMode)
                            QWER.Cast(poutput.CastPosition);
                        else
                            QWER.Cast(target);

                        debug("good 2");
                    }
                    else
                        debug("ignore 2");
                }
                else
                    debug("fixed " + fixRange);
            }
            else if (HitChanceNum == 3)
            {
                if ((int)poutput.Hitchance < 5)
                    return;
                if ( QWER.Delay > 0.4)
                {
                    if ((int)poutput.Hitchance < 6 || target.IsWindingUp)
                        return;
                }
                var fixRange = (target.MoveSpeed * (Player.ServerPosition.Distance(target.ServerPosition) / QWER.Speed + QWER.Delay)) / 2;
                if (QWER.Delay < 0.3 && (QWER.Speed > 1500 || QWER.Type == SkillshotType.SkillshotCircle) && (target.IsWindingUp || (int)poutput.Hitchance == 6))
                {
                    if (Player.Distance(target.ServerPosition) < QWER.Range - fixRange)
                    {
                        QWER.Cast(poutput.CastPosition);
                    }
                    return;
                }

                if (target.Path.Count() == 0 && target.Position == target.ServerPosition && !target.IsWindingUp)
                {
                    if (Player.Distance(target.ServerPosition) < QWER.Range - fixRange)
                    {

                        QWER.Cast(poutput.CastPosition);
                    }
                    return;
                }
                var waypoints = target.GetWaypoints().Last<Vector2>().To3D();

                float BackToFront = ((target.MoveSpeed * QWER.Delay) + (Player.Distance(target.ServerPosition) / QWER.Speed));
                float SiteToSite = (BackToFront * 2) - QWER.Width;

                if ((target.ServerPosition.Distance(waypoints) > SiteToSite
                    || Math.Abs(Player.Distance(waypoints) - Player.Distance(target.Position)) > BackToFront)
                    || Player.Distance(target.Position) < SiteToSite + target.BoundingRadius * 2
                    || Player.Distance(waypoints) < BackToFront
                    )
                {
                    
                    if (waypoints.Distance(Player.Position) <= target.Distance(Player.Position))
                    {
                        if (Player.Distance(target.ServerPosition) < QWER.Range - fixRange)
                        {
                            QWER.Cast(poutput.CastPosition);
                        }
                    }
                    else
                    {
                        QWER.Cast(poutput.CastPosition);
                    }
                }

            }
            else if (HitChanceNum == 0)
                QWER.Cast(target, true);
            else if (HitChanceNum == 1)
            {
                if ((int)poutput.Hitchance > 4)
                    QWER.Cast(poutput.CastPosition);
            }
            else if (HitChanceNum == 2)
            {
                List<Vector2> waypoints = target.GetWaypoints();
                if (waypoints.Last<Vector2>().To3D().Distance(poutput.CastPosition) > QWER.Width && (int)poutput.Hitchance > 4)
                {
                    if (waypoints.Last<Vector2>().To3D().Distance(Player.Position) <= target.Distance(Player.Position) || (target.Path.Count() == 0 && target.Position == target.ServerPosition))
                    {
                        if (Player.Distance(target.ServerPosition) < QWER.Range - (poutput.CastPosition.Distance(target.ServerPosition) + target.BoundingRadius))
                        {
                            QWER.Cast(poutput.CastPosition);
                        }
                    }
                    else if ((int)poutput.Hitchance == 5)
                    {
                        QWER.Cast(poutput.CastPosition);
                    }
                }
            }
        }

        private static void Obj_AI_Base_OnTeleport(GameObject sender, GameObjectTeleportEventArgs args)
        {
            
            var unit = sender as Obj_AI_Hero;

            if (unit == null || !unit.IsValid || unit.IsAlly)
                return;
            
            var recall = Packet.S2C.Teleport.Decoded(unit, args);

            if (recall.Type == Packet.S2C.Teleport.Type.Recall)
            {
                switch (recall.Status)
                {
                    case Packet.S2C.Teleport.Status.Start:
                        RecallInfos.RemoveAll(x => x.RecallID == sender.NetworkId);
                        RecallInfos.Add(new RecallInfo() { RecallID = sender.NetworkId, RecallStart = Game.Time, RecallNum = 0 });

                        break;
                    case Packet.S2C.Teleport.Status.Abort:
                        RecallInfos.RemoveAll(x => x.RecallID == sender.NetworkId);
                        RecallInfos.Add(new RecallInfo() { RecallID = sender.NetworkId, RecallStart = Game.Time, RecallNum = 1 });

                        break;
                    case Packet.S2C.Teleport.Status.Finish:
                        RecallInfos.RemoveAll(x => x.RecallID == sender.NetworkId);
                        if (jungler.NetworkId == sender.NetworkId)
                        {
                            enemySpawn = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy);
                            timer = (int)(enemySpawn.Position.Distance(ObjectManager.Player.Position) / 370);
                        }
                        RecallInfos.Add(new RecallInfo() { RecallID = sender.NetworkId, RecallStart = Game.Time, RecallNum = 2 });
                        break;
                }
            }
        }

        public static void drawText(string msg, Vector3 Hero, System.Drawing.Color color, int weight = 0)
        {
            var wts = Drawing.WorldToScreen(Hero);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1] + weight, color, msg);
        }
        public static void drawLine(Vector3 pos1, Vector3 pos2, int bold, System.Drawing.Color color)
        {
            var wts1 = Drawing.WorldToScreen(pos1);
            var wts2 = Drawing.WorldToScreen(pos2);

            Drawing.DrawLine(wts1[0], wts1[1], wts2[0], wts2[1], bold, color);
        }
        public static void debug(string msg)
        {
            if (Config.Item("debug").GetValue<bool>())
            {
                Console.WriteLine(msg);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (Config.Item("disableDraws").GetValue<bool>())
                return;
            var debugPred = Config.Item("debugPred").GetValue<bool>();
            if (debugPred && Game.Time - DrawSpellTime < 0.5)
            {
                if (DrawSpell.Type == SkillshotType.SkillshotLine)
                    OktwCommon.DrawLineRectangle(DrawSpellPos.CastPosition, Player.Position, (int)DrawSpell.Width, 1, System.Drawing.Color.DimGray);
                if (DrawSpell.Type == SkillshotType.SkillshotCircle)
                    Utility.DrawCircle(DrawSpellPos.CastPosition, DrawSpell.Width, System.Drawing.Color.DimGray, 4, 1);

                drawText("Aiming " + DrawSpellPos.Hitchance, Player.Position.Extend(DrawSpellPos.CastPosition, 400), System.Drawing.Color.Gray);
            }

            if (Config.Item("timer").GetValue<bool>() && jungler != null)
            {
                if (jungler == Player)
                    drawText("Jungler not detected", Player.Position, System.Drawing.Color.Yellow, 100);
                else if (jungler.IsDead)
                    drawText("Jungler dead " + timer, Player.Position, System.Drawing.Color.Cyan, 100);
                else if (jungler.IsVisible)
                    drawText("Jungler visable " + timer, Player.Position, System.Drawing.Color.GreenYellow, 100);
                else
                {
                    if (timer > 0)
                        drawText("Jungler in jungle " + timer, Player.Position, System.Drawing.Color.Orange, 100);
                    else
                        drawText("Be careful " + timer, Player.Position, System.Drawing.Color.Red, 100);
                    if (Game.Time - JungleTime >= 1)
                    {
                        timer = timer - 1;
                        JungleTime = Game.Time;
                    }
                }
            }
            var HpBar = Config.Item("HpBar").GetValue<bool>();

            var championInfo = Config.Item("championInfo").GetValue<bool>();
            var GankAlert = Config.Item("GankAlert").GetValue<bool>();
            var ShowKDA = Config.Item("ShowKDA").GetValue<bool>();
            var ShowClicks = Config.Item("ShowClicks").GetValue<bool>();
            float posY = ((float)Config.Item("posY").GetValue<Slider>().Value * 0.01f) * Drawing.Height;
            float posX = ((float)Config.Item("posX").GetValue<Slider>().Value * 0.01f) * Drawing.Width;
            float positionDraw = 0;
            float positionGang = 500;

            int Width = 103;
            int Height = 8;
            int XOffset = 10;
            int YOffset = 20;

            var FillColor = System.Drawing.Color.GreenYellow;
            var Color = System.Drawing.Color.Azure;

            foreach (var enemy in Enemies)
            {
                if (enemy.IsValidTarget())
                {
                    if (ShowClicks)
                    {
                        List<Vector2> waypoints = enemy.GetWaypoints();
                        drawLine(enemy.Position, waypoints.Last<Vector2>().To3D(), 1, System.Drawing.Color.Red);

                    }
                }

                if (HpBar && enemy.IsHPBarRendered && Render.OnScreen(Drawing.WorldToScreen(enemy.Position)))
                {

                    var barPos = enemy.HPBarPosition;

                    float QdmgDraw = 0, WdmgDraw = 0, EdmgDraw = 0, RdmgDraw = 0, damage = 0; ;

                    if (Q.IsReady())
                        damage = damage + Q.GetDamage(enemy);

                    if (W.IsReady() && Player.ChampionName != "Kalista")
                        damage = damage + W.GetDamage(enemy);

                    if (E.IsReady())
                        damage = damage + E.GetDamage(enemy);

                    if (R.IsReady())
                        damage = damage + R.GetDamage(enemy);

                    if (Q.IsReady())
                        QdmgDraw = (Q.GetDamage(enemy) / damage);

                    if (W.IsReady() && Player.ChampionName != "Kalista")
                        WdmgDraw = (W.GetDamage(enemy) / damage);

                    if (E.IsReady())
                        EdmgDraw = (E.GetDamage(enemy) / damage);

                    if (R.IsReady())
                        RdmgDraw = (R.GetDamage(enemy) / damage);

                    var percentHealthAfterDamage = Math.Max(0, enemy.Health - damage) / enemy.MaxHealth;

                    var yPos = barPos.Y + YOffset;
                    var xPosDamage = barPos.X + XOffset + Width * percentHealthAfterDamage;
                    var xPosCurrentHp = barPos.X + XOffset + Width * enemy.Health / enemy.MaxHealth;

                    float differenceInHP = xPosCurrentHp - xPosDamage;
                    var pos1 = barPos.X + XOffset + (107 * percentHealthAfterDamage);

                    for (int i = 0; i < differenceInHP; i++)
                    {
                        if (Q.IsReady() && i < QdmgDraw * differenceInHP)
                            Drawing.DrawLine(pos1 + i, yPos, pos1 + i, yPos + Height, 1, System.Drawing.Color.Cyan);
                        else if (W.IsReady() && i < (QdmgDraw + WdmgDraw) * differenceInHP)
                            Drawing.DrawLine(pos1 + i, yPos, pos1 + i, yPos + Height, 1, System.Drawing.Color.Orange);
                        else if (E.IsReady() && i < (QdmgDraw + WdmgDraw + EdmgDraw) * differenceInHP)
                            Drawing.DrawLine(pos1 + i, yPos, pos1 + i, yPos + Height, 1, System.Drawing.Color.Yellow);
                        else if (R.IsReady() && i < (QdmgDraw + WdmgDraw + EdmgDraw + RdmgDraw) * differenceInHP)
                            Drawing.DrawLine(pos1 + i, yPos, pos1 + i, yPos + Height, 1, System.Drawing.Color.YellowGreen);
                    }
                }

                var kolor = System.Drawing.Color.GreenYellow;

                if (enemy.IsDead)
                    kolor = System.Drawing.Color.Gray;
                else if (!enemy.IsVisible)
                    kolor = System.Drawing.Color.OrangeRed;

                var kolorHP = System.Drawing.Color.GreenYellow;

                if (enemy.IsDead)
                    kolorHP = System.Drawing.Color.GreenYellow;
                else if ((int)enemy.HealthPercent < 30)
                    kolorHP = System.Drawing.Color.Red;
                else if ((int)enemy.HealthPercent < 60)
                    kolorHP = System.Drawing.Color.Orange;

                if (championInfo)
                {
                    positionDraw += 15;
                    foreach (RecallInfo rerecall in RecallInfos)
                    {
                        if (rerecall.RecallID == enemy.NetworkId && Game.Time - rerecall.RecallStart < 8)
                        {
                            var time = (Game.Time - rerecall.RecallStart) * 10;
                            if (rerecall.RecallNum == 2)
                                Drawing.DrawText(posX + 150, posY + positionDraw, System.Drawing.Color.GreenYellow, "rerecall finish");
                            else if (rerecall.RecallNum == 1)
                                Drawing.DrawText(posX + 150, posY + positionDraw, System.Drawing.Color.Yellow, "rerecall stop");
                            else if (rerecall.RecallNum == 0)
                            {
                                Drawing.DrawLine(posX + 150, posY + positionDraw, posX + 230 - time, posY + positionDraw, 12, System.Drawing.Color.Red);
                                Drawing.DrawLine(posX + 230 - time, posY + positionDraw, posX + 230, posY + positionDraw, 12, System.Drawing.Color.Black);
                            }
                        }
                    }

                    if (ShowKDA)
                        Drawing.DrawText(posX - 30, posY + positionDraw, kolor, " " + enemy.ChampionsKilled + "/" + enemy.Deaths + "/" + enemy.Assists + " " + enemy.MinionsKilled);
                    Drawing.DrawText(posX + 60, posY + positionDraw, kolor, enemy.ChampionName);

                    Drawing.DrawText(posX - 70, posY + positionDraw, kolor, enemy.Level + " lvl");

                }
                var Distance = Player.Distance(enemy.Position);
                if (GankAlert && !enemy.IsDead && Distance > 1200)
                {

                    var wts = Drawing.WorldToScreen(ObjectManager.Player.Position.Extend(enemy.Position, positionGang));

                    wts[0] = wts[0] - (enemy.ChampionName.Count<char>()) * 5;
                    wts[1] = wts[1] + 15;
                    if ((int)enemy.HealthPercent > 0)
                        Drawing.DrawLine(wts[0], wts[1], (wts[0] + ((int)enemy.HealthPercent) / 2) + 1, wts[1], 12, kolorHP);
                    if ((int)enemy.HealthPercent < 100)
                        Drawing.DrawLine((wts[0] + ((int)enemy.HealthPercent) / 2), wts[1], wts[0] + 50, wts[1], 12, System.Drawing.Color.White);
                    if (Distance > 3500 && enemy.IsVisible)
                        drawText(enemy.ChampionName, Player.Position.Extend(enemy.Position, positionGang), System.Drawing.Color.GreenYellow);
                    else if (!enemy.IsVisible)
                        drawText(enemy.ChampionName, Player.Position.Extend(enemy.Position, positionGang), System.Drawing.Color.Gray);
                    else
                        drawText(enemy.ChampionName, Player.Position.Extend(enemy.Position, positionGang), System.Drawing.Color.Red);
                    if (Distance < 3500 && enemy.IsVisible && !Render.OnScreen(Drawing.WorldToScreen(Player.Position.Extend(enemy.Position, Distance + 500))))
                    {
                        drawLine(Player.Position.Extend(enemy.Position, 100), Player.Position.Extend(enemy.Position, positionGang - 100), (int)((3500 - Distance) / 300), System.Drawing.Color.OrangeRed);

                    }
                    else if (Distance < 3500 && !enemy.IsVisible && !Render.OnScreen(Drawing.WorldToScreen(Player.Position.Extend(enemy.Position, Distance + 500))))
                    {
                        var need=VisableInfo.Find(x => x.VisableID == enemy.NetworkId);
                        if (need != null && Game.Time - need.time < 5)
                        {
                            drawLine(Player.Position.Extend(enemy.Position, 100), Player.Position.Extend(enemy.Position, positionGang - 100), (int)((3500 - Distance) / 300), System.Drawing.Color.Gray);
                        }
                    }
                }
                positionGang = positionGang + 100;
            }

            if (Config.Item("OrbDraw").GetValue<bool>())
            {
                if (Player.HealthPercentage() > 60)
                    Utility.DrawCircle(ObjectManager.Player.Position, Player.AttackRange + ObjectManager.Player.BoundingRadius * 2, System.Drawing.Color.GreenYellow, 2, 1);
                else if (Player.HealthPercentage() > 30)
                    Utility.DrawCircle(ObjectManager.Player.Position, ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius * 2, System.Drawing.Color.Orange, 3, 1);
                else
                    Utility.DrawCircle(ObjectManager.Player.Position, ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius * 2, System.Drawing.Color.Red, 4, 1);
            }

            if (Config.Item("orb").GetValue<bool>())
            {
                var orbT = Orbwalker.GetTarget();

                if (orbT.IsValidTarget())
                {
                    if (orbT.Health > orbT.MaxHealth * 0.6)
                        Utility.DrawCircle(orbT.Position, orbT.BoundingRadius, System.Drawing.Color.GreenYellow, 5, 1);
                    else if (orbT.Health > orbT.MaxHealth * 0.3)
                        Utility.DrawCircle(orbT.Position, orbT.BoundingRadius, System.Drawing.Color.Orange, 5, 1);
                    else
                        Utility.DrawCircle(orbT.Position, orbT.BoundingRadius, System.Drawing.Color.Red, 5, 1);
                }
            }
        }
    }
}