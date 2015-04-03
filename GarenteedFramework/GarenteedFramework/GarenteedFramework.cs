using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using LeagueSharp;
namespace GarenteedFramework
{
    public class GarenteedFramework
    {
        //Variables for the spells and the plugin.
        private static FrameWorkPlugin myDerived;
        private static Spell Q = new Spell(SpellSlot.Q);
        private static Spell W = new Spell(SpellSlot.W);
        private static Spell E = new Spell(SpellSlot.E);
        private static Spell R = new Spell(SpellSlot.R);
        private static int deathLogicNumber = 9999;
        private string champName = "";

        //Pass in the interface object and set the class variables.
        public GarenteedFramework(FrameWorkPlugin init)
        {
            myDerived = init;
        }
        //Loads when assembly is injected.
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        //Whenever the game loads, this code is done.
        private static void Game_OnGameLoad(EventArgs args)
        {
            //Let them know it loaded using Garenteed Framework
            Game.PrintChat("Made using Garenteed Framework by Nouser");
            Game.OnGameUpdate += OnGameUpdate;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            //Custom code for on game load (ie Game.PrintChat("Herp Derp Script by Degrec using cracked scripts");)
            myDerived.ExpandGameLoad(args);
        }

        private static void OnGameUpdate(EventArgs args)
        {
            //Check if their deaths are higher than the number of deaths you set as having logic.
            if(ObjectManager.Player.Deaths>=deathLogicNumber)
                myDerived.DeathsLogic();
            //Shopping logic.
            //if (Utility.InShopRange() || ObjectManager.Player.IsDead)
                //myDerived.ShopLogic();
        }
        //Set the champ name for any comparisons needed for later framework expansion.
        public void SetName(string name)
        {
            champName = name;
        }
        //Set number of deaths for death logic.
        public void SetDeathLogicNumber(int number)
        {
            deathLogicNumber = number;
        }
        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            //This checks the you are the sender.
            if (sender.IsMe)
            {
                //Works whenever you cast your Q.
                if (args.SData.Name.Equals(Q.ToString()))
                {
                    myDerived.QLogic(sender,args);
                }
                //Works whenever you cast your W.
                else if (args.SData.Name.Equals(W.ToString()))
                {
                    myDerived.WLogic(sender, args);
                }
                //Works whenever you cast your E.
                else if (args.SData.Name.Equals(E.ToString()))
                {
                    myDerived.ELogic(sender, args);
                }
                //Works whenever you cast your R.
                else if (args.SData.Name.Equals(R.ToString()))
                {
                    myDerived.RLogic(sender, args);
                }
            }
        }
    }

    public interface FrameWorkPlugin
    {
        //Implement these methods for the appropriate action.
        void ExpandGameLoad(EventArgs args);
        void QLogic(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args);
        void WLogic(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args);
        void ELogic(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args);
        void RLogic(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args);
        void DeathsLogic();
        void ShopLogic();
    }
}
