using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
namespace OneKeyToWin_AIO_Sebby.Core
{
    class OKTWfarmLogic
    {
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        public void LoadOKTW()
        {
            Game.OnUpdate +=Game_OnUpdate;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            
            foreach (var turret in ObjectManager.Get<Obj_AI_Turret>().Where(t => Player.Distance(t.Position)<1000))
            {

                var minions = MinionManager.GetMinions(turret.Position, 900, MinionTypes.All);

                var minions2 = minions.OrderBy(minion => turret.Distance(minion.Position));

                foreach (var minion in minions2.Where(minion => minion.IsValidTarget() && minion.UnderTurret(false)))
                {
                    if (Player.GetAutoAttackDamage(minion) > minion.Health)
                    {
                        Orbwalking.Attack = true;
                    }
                    else
                        Orbwalking.Attack = false;

                    Program.debug("dmg " + turret.GetAutoAttackDamage(minion));
                    Program.debug("dis " + turret.Distance(minion.Position));
                    var hpAfter = minion.Health % turret.GetAutoAttackDamage(minion);
                    Program.debug("need " + hpAfter + " dmg "+Player.GetAutoAttackDamage(minion));
                    if (hpAfter > Player.GetAutoAttackDamage(minion))
                    {
                        Orbwalking.Attack = true;
                        Orbwalker.ForceTarget(minion);
                        return;
                    }
                }
                Orbwalking.Attack = false;
            }
        }


    }
}
