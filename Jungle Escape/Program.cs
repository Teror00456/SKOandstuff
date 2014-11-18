using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Jungle_Escape
{
    class Program
    {

        private static Dictionary<string, SpellSlot> Spells = new Dictionary<string, SpellSlot>();
        private static Dictionary<string, float> Ranges = new Dictionary<string, float>();
		private static List<Vector3> JunglePos = new List<Vector3>();
        private static Obj_AI_Hero Player;
        private static Menu menu;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

			junglePositions();

			Spells.Add("LeeSin", SpellSlot.Q);
			Spells.Add("Amumu", SpellSlot.Q);
			Spells.Add("Thresh", SpellSlot.Q);

			Ranges.Add("LeeSin", 1100f);
			Ranges.Add("Amumu", 1100f);
			Ranges.Add("Thresh", 1100f);

			if(!Spells.ContainsKey(Player.ChampionName))return;


            menu = new Menu("Jungle Escape","JungleEscape", true);
			menu.AddItem(new MenuItem("EscapeKey", "Escape Key").SetValue<KeyBind>(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
            menu.AddItem(new MenuItem("Draw", "Draw Range").SetValue(true));
            //menu.AddItem(new MenuItem("AutoEs", "Auto Escape").SetValue(true));
            //menu.AddItem(new MenuItem("MinEnemy", "Min Enemys").SetValue<Slider>(new Slider(3, 1, 5)));
			// menu.AddItem(new MenuItem("Debug", "Debug").SetValue<KeyBind>(new KeyBind(32, KeyBindType.Press)));

            menu.AddToMainMenu();

			Game.OnGameUpdate += Game_OnGameUpdate;
			Drawing.OnDraw += Drawing_OnDraw;

			Game.PrintChat("Jungle Escape Loaded!");

		
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {


			if (menu.Item("EscapeKey").GetValue<KeyBind>().Active)
			{	
				Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
				foreach(var castpos in JunglePos)
				{
					if(Player.Spellbook.CanUseSpell(Spells[Player.ChampionName]) == SpellState.Ready && Player.Distance(castpos) < Ranges[Player.ChampionName])
				{
						Player.Spellbook.CastSpell(Spells[Player.ChampionName], castpos);
				}
				}
            }

        }

        private static void Drawing_OnDraw(EventArgs args) 
        {
			if(menu.Item("Draw").GetValue<bool>())
			{
				foreach(var range in Ranges)
				{
					Utility.DrawCircle(Player.Position, Ranges[Player.ChampionName], Color.White, 1, 100);
					
				}
			}
        }

		private static void junglePositions()
		{
			JunglePos.Add(new Vector3(6475f, 5293f, 58.79452f));
			JunglePos.Add(new Vector3(7555f, 3999f, 56.86656f));
			JunglePos.Add(new Vector3(8023f, 2665f, 54.2764f));
			JunglePos.Add(new Vector3(9427f, 4269f, -60.31875f));
			JunglePos.Add(new Vector3(10415f, 6771f, 54.8691f));
			JunglePos.Add(new Vector3(12251f, 6259f, 54.84792f));
			JunglePos.Add(new Vector3(10619f, 8191f, 63.42225f));
			JunglePos.Add(new Vector3(7507f, 9149f, 55.55075f));
			JunglePos.Add(new Vector3(6503f, 10595f, 54.635f));
			JunglePos.Add(new Vector3(6113f, 11949f, 39.59537f));
			JunglePos.Add(new Vector3(4647f, 10263f, -63.06151f));
			JunglePos.Add(new Vector3(3687f, 7559f, 53.87956f));
			JunglePos.Add(new Vector3(1709f, 8171f, 54.92361f));
			JunglePos.Add(new Vector3(3439f, 6295f, 55.6099f));
		}

    }
}
