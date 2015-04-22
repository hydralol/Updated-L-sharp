#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion

namespace HydraAIO
{
    internal class Champion
    {
        public bool ComboActive;
        public Menu Config;
        public bool Orbwalk = true;
        public bool HarassActive;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public string Id = "";
        public bool LaneClearActive;
        public bool ToggleActive;
        public Orbwalking.Orbwalker Orbwalker;
        private static int _lastMoveCommandT;
        private const int Delay = 150;
        private static readonly Random Random = new Random(DateTime.Now.Millisecond);
        public bool AttackNow;
        public float AttackReadiness;
        public bool UsesMana = true;
        private const float MinDistance = 400;

        public T GetValue<T>(string item)
        {
            return Config.Item(item + Id).GetValue<T>();
        }

        public virtual bool ComboMenu(Menu config)
        {
            return false;
        }

        public virtual bool HarassMenu(Menu config)
        {
            return false;
        }

        public virtual bool LaneClearMenu(Menu config)
        {
            return false;
        }

        public virtual bool MiscMenu(Menu config)
        {
            return false;
        }

        public virtual bool ExtrasMenu(Menu config)
        {
            return false;
        }

        public virtual bool DrawingMenu(Menu config)
        {
            return false;
        }

        public virtual bool MainMenu(Menu config)
        {
            return false;
        }

        public virtual void Drawing_OnDraw(EventArgs args)
        {
        }

        public virtual void Game_OnGameUpdate(EventArgs args)
        {
            
        }

        public virtual void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            
        }

        public virtual void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
        }

        public virtual void Orbwalking_OnAttack(AttackableUnit unit, AttackableUnit target)
        {

        }

        public static float PhysHealth(Obj_AI_Base tar)
        {
            return tar.Health *
                   (1 + (((tar.Armor * Player.PercentArmorPenetrationMod) - Player.FlatArmorPenetrationMod) / 100));
        }

        public static void CastSpellAoEShot(Spell spell, Obj_AI_Base target, int spellAoE)
        {
            var po = spell.GetPrediction(target);
            if (po.CollisionObjects.Count > 0)
            {
                var firstCol = po.CollisionObjects.OrderBy(unit => unit.Distance(Player.ServerPosition)).First();
                if (firstCol.IsValidTarget() && (/*firstCol.Distance(target.ServerPosition) < spellAoE ||*/ firstCol.Distance(po.UnitPosition) < spellAoE))
                {
                    spell.Cast(po.CastPosition);
                }
            }
            else
            {
                spell.Cast(po.CastPosition);
            }
        }

        public void MoveByTarget(Obj_AI_Base target)
        {
            if (target.IsValidTarget())
            {
                var enemyPos = target.Position;
                var mousePos = Game.CursorPos;
                var ePos = enemyPos + (enemyPos - mousePos) * (-(Orbwalking.GetRealAutoAttackRange(target) / 2) / enemyPos.Distance(mousePos));
                if (Player.Distance(ePos) > 100)
                {
                    if (Environment.TickCount - _lastMoveCommandT < Delay)
                    {
                        return;
                    }

                    _lastMoveCommandT = Environment.TickCount;

                    Player.IssueOrder(GameObjectOrder.MoveTo, ePos);
                }
            }
        }

        public void MoveTo(Vector3 pVector3, float holdAreaRadius = 0)
        {
            if (Environment.TickCount - _lastMoveCommandT < Delay)
            {
                return;
            }

            _lastMoveCommandT = Environment.TickCount;

            if (Player.ServerPosition.Distance(pVector3) < holdAreaRadius)
            {
                if (Player.Path.Count() <= 1)
                {
                    return;
                }

                Player.IssueOrder(GameObjectOrder.HoldPosition, Player.ServerPosition);

                return;
            }

            var point = Player.ServerPosition +
                        ((Random.NextFloat(0.6f, 1) + 0.2f) * MinDistance) *
                        (pVector3.To2D() - Player.ServerPosition.To2D()).Normalized().To3D();

            Player.IssueOrder(GameObjectOrder.MoveTo, point);
        }
    }
}
