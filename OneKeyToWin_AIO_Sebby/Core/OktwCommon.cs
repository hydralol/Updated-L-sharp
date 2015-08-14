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
    class OktwCommon
    {
        public static bool 
            blockMove = false,
            blockAttack = false,
            blockSpells = false;

        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private static List<Obj_AI_Base> minions;
        public void LoadOKTW()
        {
            Obj_AI_Base.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
            Spellbook.OnCastSpell +=Spellbook_OnCastSpell;
            
        }

        public static bool CanHarras()
        {
            if ( Player.IsWindingUp)
                return false;
            //if (!Program.Farm)
              //  return true;
            minions = MinionManager.GetMinions(Player.Position, Player.AttackRange+300, MinionTypes.All);
            //public static List<Obj_AI_Base> GetMinions(Vector3 from, float range, MinionTypes type = MinionTypes.All, MinionTeam team = MinionTeam.Enemy, MinionOrderTypes order = MinionOrderTypes.Health);
            var minionsAlly = MinionManager.GetMinions(Player.Position, Player.AttackRange + 200, MinionTypes.All, MinionTeam.Ally);

            if (minions == null || minions.Count == 0)
                return true;

            var minion = minions.First(minion2 => minion2.IsValidTarget());

            if (minion.Health < Player.GetAutoAttackDamage(minion) + minionsAlly.Count * minion.GetAutoAttackDamage(minion))
                return false;
            else
                return true;
            
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (blockSpells)
            {
                args.Process = false;
            }
        }

        private void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (!sender.IsMe)
                return;

            if (blockMove  && !args.IsAttackMove)
            {
                args.Process = false;
            } 
            if (blockAttack && args.IsAttackMove)
            {
                args.Process = false;
            }
        }
        public static bool IsFaced(Obj_AI_Hero target)
        {
            Vector2 LastWaypoint = target.GetWaypoints().Last();
            if (LastWaypoint.Distance(Player.Position) < target.Distance(Player.Position))
                return true;

            return false;
        }

        public static bool CanMove(Obj_AI_Hero target)
        {
            if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup) ||
                target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockback) ||
                target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Suppression) ||
                target.IsStunned || target.IsChannelingImportantSpell())
            {
                Program.debug("!canMov" + target.ChampionName);
                return false;
            }
            else
                return true;
        }

        public static bool ValidUlt(Obj_AI_Hero target)
        {
            if (target.HasBuffOfType(BuffType.PhysicalImmunity) || target.HasBuffOfType(BuffType.SpellImmunity)
            || target.IsZombie || target.HasBuffOfType(BuffType.Invulnerability) || target.HasBuffOfType(BuffType.SpellShield) || !target.HasBuff("deathdefiedbuff"))
                return false;
            else
                return true;
        }

        public static int CountEnemiesInRangeDeley(Vector3 position, float range, float delay)
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

        public static void DrawLineRectangle(Vector3 start2, Vector3 end2, int radius, float width, System.Drawing.Color color)
        {
            Vector2 start = start2.To2D();
            Vector2 end = end2.To2D();
            var dir = (end - start).Normalized();
            var pDir = dir.Perpendicular();

            var rightStartPos = start + pDir * radius;
            var leftStartPos = start - pDir * radius;
            var rightEndPos = end + pDir * radius;
            var leftEndPos = end - pDir * radius;

            var rStartPos = Drawing.WorldToScreen(new Vector3(rightStartPos.X, rightStartPos.Y, ObjectManager.Player.Position.Z));
            var lStartPos = Drawing.WorldToScreen(new Vector3(leftStartPos.X, leftStartPos.Y, ObjectManager.Player.Position.Z));
            var rEndPos = Drawing.WorldToScreen(new Vector3(rightEndPos.X, rightEndPos.Y, ObjectManager.Player.Position.Z));
            var lEndPos = Drawing.WorldToScreen(new Vector3(leftEndPos.X, leftEndPos.Y, ObjectManager.Player.Position.Z));

            Drawing.DrawLine(rStartPos, rEndPos, width, color);
            Drawing.DrawLine(lStartPos, lEndPos, width, color);
            Drawing.DrawLine(rStartPos, lStartPos, width, color);
            Drawing.DrawLine(lEndPos, rEndPos, width, color);
        }

        public static List<Vector3> CirclePoints(float CircleLineSegmentN, float radius, Vector3 position)
        {
            List<Vector3> points = new List<Vector3>();
            var bestPoint = ObjectManager.Player.Position;
            for (var i = 1; i <= CircleLineSegmentN; i++)
            {
                var angle = i * 2 * Math.PI / CircleLineSegmentN;
                var point = new Vector3(position.X + radius * (float)Math.Cos(angle), position.Y + radius * (float)Math.Sin(angle), position.Z);
                points.Add(point);
            }
            return points;
        }

        public static bool GetCollision(Obj_AI_Base target, Spell QWER, bool champion, bool minion)
        {
            var rDmg = QWER.GetDamage(target);
            int collision = 0;
            PredictionOutput output = QWER.GetPrediction(target);
            Vector2 direction = output.CastPosition.To2D() - ObjectManager.Player.Position.To2D();
            direction.Normalize();
            if (champion)
            {
                foreach (var enemy in Program.Enemies.Where(x => x.IsEnemy && x.IsValidTarget()))
                {
                    PredictionOutput prediction = QWER.GetPrediction(enemy);
                    Vector3 predictedPosition = prediction.CastPosition;
                    Vector3 v = output.CastPosition - ObjectManager.Player.ServerPosition;
                    Vector3 w = predictedPosition - ObjectManager.Player.ServerPosition;
                    double c1 = Vector3.Dot(w, v);
                    double c2 = Vector3.Dot(v, v);
                    double b = c1 / c2;
                    Vector3 pb = ObjectManager.Player.ServerPosition + ((float)b * v);
                    float length = Vector3.Distance(predictedPosition, pb);
                    if (length < QWER.Width )
                        return true;
                }
            }
            if (minion)
            {
                var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, QWER.Range, MinionTypes.All);
                foreach (var enemy in allMinions.Where(x => x.IsEnemy && x.IsValidTarget()))
                {
                    PredictionOutput prediction = QWER.GetPrediction(enemy);
                    Vector3 predictedPosition = prediction.CastPosition;
                    Vector3 v = output.CastPosition - ObjectManager.Player.ServerPosition;
                    Vector3 w = predictedPosition - ObjectManager.Player.ServerPosition;
                    double c1 = Vector3.Dot(w, v);
                    double c2 = Vector3.Dot(v, v);
                    double b = c1 / c2;
                    Vector3 pb = ObjectManager.Player.ServerPosition + ((float)b * v);
                    float length = Vector3.Distance(predictedPosition, pb);
                    if (length < QWER.Width)
                        return true;
                }
            }
            return false;
        }

        public static int GetBuffCount(Obj_AI_Base target, String buffName)
        {
            foreach (var buff in target.Buffs.Where(buff => buff.Name == buffName))
            {
                if (buff.Count == 0)
                    return 1;
                else
                    return buff.Count;
            }
            return 0;
        }


        public static float GetPassiveTime(Obj_AI_Base target, String buffName)
        {
            return
                target.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Name == buffName)
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault() - Game.Time;
        }

        public static int WayPointAnalysis(Obj_AI_Base unit , Spell QWER)
        {
            int HC = 0;

            if (QWER.Delay < 0.25f)
                HC = 2;
            else
                HC = 1;

            if (unit.Path.Count() == 1)
                HC = 2;

            return HC;

        }
    }
}
