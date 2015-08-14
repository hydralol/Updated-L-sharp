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
    class Syndra
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell Q, W, E, R;
        private float QMANA, WMANA, EMANA, RMANA;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 790);
            W = new Spell(SpellSlot.W, 925);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 675);
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += OnUpdate;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            
        }

        private void OnUpdate(EventArgs args)
        {
           
            var mobs = MinionManager.GetMinions(Player.ServerPosition, 600, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (W.IsReady() )
                {
                    Program.debug("dupa");
                    W.Cast(mob.Position);
                    W.Cast(mob.ServerPosition,true);
                    W.Cast(mob,true);
                    W.CastOnUnit(mob,true);
                    Player.Spellbook.CastSpell(W.Slot, mob);
                    return;
                }
            }
            
        }
    }
}
