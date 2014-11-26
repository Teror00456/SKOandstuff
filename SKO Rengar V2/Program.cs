using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SKO_Rengar_V2
{
	class Program
	{
		private static Obj_AI_Hero Player;
		private static Spell Q, W, E, R;
		private static Items.Item BWC, BRK, RO, YMG, STD, TMT, HYD;
		private static bool PacketCast;
		private static Menu SKOMenu;
		private static bool Recall; 
		private static SpellSlot IgniteSlot, TeleportSlot;

		public static void Main (string[] args)
		{
			CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
		}

		private static void Game_OnGameLoad(EventArgs args)
		{
			Player = ObjectManager.Player;
			if (Player.ChampionName != "Rengar")
				return;

			SKOMenu = new Menu ("SKO Rengar","SKORengar", true);

			var SKOTs = new Menu ("Target Selector","TargetSelector");
			SimpleTs.AddToMenu(SKOTs);

			var OrbMenu = new Menu ("Orbwalker", "Orbwalker");
			LXOrbwalker.AddToMenu (OrbMenu);


			var Combo = new Menu ("Combo", "Combo");
			Combo.AddItem(new MenuItem("CPrio", "Empowered Priority").SetValue(new StringList(new[] {"Q", "W", "E"}, 2)));
			Combo.AddItem(new MenuItem ("UseQ", "Use Q").SetValue(true));
			Combo.AddItem(new MenuItem ("UseW", "Use W").SetValue(true));
			Combo.AddItem(new MenuItem ("UseE", "Use E").SetValue(true));
			Combo.AddItem(new MenuItem ("UseItemsCombo", "Use Items").SetValue(true));
			Combo.AddItem(new MenuItem ("UseAutoW", "Auto W").SetValue(true));
			Combo.AddItem(new MenuItem ("HpAutoW", "Min hp").SetValue(new Slider(20,1,100)));
			Combo.AddItem (new MenuItem("TripleQ", "Triple Q").SetValue(new KeyBind(OrbMenu.Item("Flee_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
			Combo.AddItem(new MenuItem("activeCombo", "Combo!").SetValue(new KeyBind(OrbMenu.Item("Combo_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));

			var Harass = new Menu("Harass", "Harass");
			Harass.AddItem(new MenuItem("HPrio", "Empowered Priority").SetValue(new StringList(new[] {"W", "E"}, 1)));
			Harass.AddItem(new MenuItem("UseWH", "Use W").SetValue(true));
			Harass.AddItem(new MenuItem("UseEH", "Use E").SetValue(true));
			Harass.AddItem(new MenuItem ("UseItemsHarass", "Use Items").SetValue(true));
			Harass.AddItem(new MenuItem("activeHarass","Harass!").SetValue(new KeyBind(OrbMenu.Item("Harass_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));

			var JLClear = new Menu("Jungle/Lane Clear", "JLClear");
			JLClear.AddItem(new MenuItem("FPrio", "Empowered Priority").SetValue(new StringList(new[] {"Q", "W", "E"}, 0)));
			JLClear.AddItem(new MenuItem("UseQC", "Use Q").SetValue(true));
			JLClear.AddItem(new MenuItem("UseWC", "Use W").SetValue(true));
			JLClear.AddItem(new MenuItem("UseEC", "Use E").SetValue(true));
			JLClear.AddItem(new MenuItem("Save", "Save Ferocity").SetValue(false));
			JLClear.AddItem(new MenuItem("UseItemsClear", "Use Items").SetValue(true));
			JLClear.AddItem(new MenuItem("activeClear","Clear!").SetValue(new KeyBind(OrbMenu.Item("LaneClear_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));

			var TROLLZINHONASRANKEDS = new Menu("KillSteal", "TristanaKillZiggs");
			TROLLZINHONASRANKEDS.AddItem(new MenuItem("Foguinho", "Use Ignite").SetValue(true));
			TROLLZINHONASRANKEDS.AddItem(new MenuItem("UseQKs", "Use Q").SetValue(true));
			TROLLZINHONASRANKEDS.AddItem(new MenuItem("UseWKs", "Use W").SetValue(true));
			TROLLZINHONASRANKEDS.AddItem(new MenuItem("UseEKs", "Use E").SetValue(true));
			TROLLZINHONASRANKEDS.AddItem(new MenuItem("UseFlashKs", "Use Flash KillSteal(Kappa)").SetValue(false));

			var CHUPARUNSCUEPA = new Menu("Drawing", "Drawing");
			CHUPARUNSCUEPA.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(true));
			CHUPARUNSCUEPA.AddItem(new MenuItem("DrawW", "Draw W").SetValue(true));
			CHUPARUNSCUEPA.AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
			CHUPARUNSCUEPA.AddItem(new MenuItem("DrawR", "Draw R").SetValue(true));
			CHUPARUNSCUEPA.AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
			CHUPARUNSCUEPA.AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
			CHUPARUNSCUEPA.AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));

			var Misc = new Menu("Misc", "Misc");
			Misc.AddItem(new MenuItem("UsePacket","Use Packet").SetValue(true));
			Misc.AddItem(new MenuItem("TpREscape", "R + TP Escape").SetValue<KeyBind>(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

			Game.PrintChat("<font color='#07B88C'>SKO Rengar V2 Loaded!</font>");

			SKOMenu.AddSubMenu(SKOTs);
			SKOMenu.AddSubMenu(OrbMenu);
			SKOMenu.AddSubMenu(Combo);
			SKOMenu.AddSubMenu(Harass);
			SKOMenu.AddSubMenu(JLClear);
			SKOMenu.AddSubMenu(TROLLZINHONASRANKEDS);
			SKOMenu.AddSubMenu(CHUPARUNSCUEPA);
			SKOMenu.AddSubMenu(Misc);
			SKOMenu.AddToMainMenu();

			W = new Spell(SpellSlot.W, 450f);
			E = new Spell(SpellSlot.E, 980f);
			R = new Spell(SpellSlot.R, 2000f);

			E.SetSkillshot(0.25f, 70f, 1500f, true, SkillshotType.SkillshotLine);

			HYD = new Items.Item(3074, 420f);
			TMT = new Items.Item(3077, 420f);
			BRK = new Items.Item(3153, 450f);
			BWC = new Items.Item(3144, 450f);
			RO = new Items.Item(3143, 500f);

			PacketCast = SKOMenu.Item("UsePacket").GetValue<bool>();

			IgniteSlot = Player.GetSpellSlot("SummonerDot");
			TeleportSlot = Player.GetSpellSlot("SummonerTeleport");

			Game.OnGameUpdate += Game_OnGameUpdate;
			Drawing.OnDraw += Draw_OnDraw;
		}


		private static void Game_OnGameUpdate(EventArgs args)
		{
			TPREscape();

			if(SKOMenu.Item("activeClear").GetValue<KeyBind>().Active)
			{
				Clear();
			}
			var tqtarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Physical);
			if(SKOMenu.Item("TripleQ").GetValue<KeyBind>().Active)
			{
				TripleQ(tqtarget);
			}

			var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);

			Q = new Spell(SpellSlot.Q, Player.AttackRange+100);
			YMG = new Items.Item(3142, Player.AttackRange+50);
			STD = new Items.Item(3131, Player.AttackRange+50);

			AutoHeal();
			KillSteal(target);

			if(SKOMenu.Item("activeCombo").GetValue<KeyBind>().Active)
			{

				if(target.IsValidTarget())
				{
					if(Player.Mana <= 4)
					{	
						if(SKOMenu.Item("UseQ").GetValue<bool>() && Q.IsReady() && Player.Distance(target) <= Q.Range)
						{
							Q.Cast();
						}	
						if(SKOMenu.Item("UseW").GetValue<bool>() && W.IsReady() && Player.Distance(target) <= W.Range){
							W.Cast();
						}
						if(SKOMenu.Item("UseE").GetValue<bool>() && E.IsReady() && Player.Distance(target) <= E.Range){
							PredE(target);
						}
					}
					if(Player.Mana == 5)
					{
						if(SKOMenu.Item("UseQ").GetValue<bool>() && SKOMenu.Item("CPrio").GetValue<StringList>().SelectedIndex == 0 && Q.IsReady() && Player.Distance(target) <= Q.Range)
						{
							Q.Cast();
						}
						if(SKOMenu.Item("UseW").GetValue<bool>() && SKOMenu.Item("CPrio").GetValue<StringList>().SelectedIndex == 1 && W.IsReady() && Player.Distance(target) <= W.Range)
						{
							W.Cast();
						}
						if(SKOMenu.Item("UseE").GetValue<bool>() && SKOMenu.Item("CPrio").GetValue<StringList>().SelectedIndex == 2 && E.IsReady() && Player.Distance(target) <= E.Range)
						{
							PredE(target);
						}

						//E if !Q.IsReady()
						if(SKOMenu.Item("UseE").GetValue<bool>() && !Q.IsReady() && E.IsReady() && Player.Distance(target) > Q.Range)
						{
							PredE(target);
						}
					}
					if(SKOMenu.Item("UseItemsCombo").GetValue<bool>()){
						if(Player.Distance(target) < Player.AttackRange+50){
							TMT.Cast();
							HYD.Cast();
							STD.Cast();
						}
						BWC.Cast(target);
						BRK.Cast(target);
						RO.Cast(target);
						YMG.Cast();
					}

				}

			}
			if(SKOMenu.Item("activeHarass").GetValue<KeyBind>().Active)
			{
				Harass();
			}



		}


		private static void Draw_OnDraw(EventArgs args)
		{
			if (SKOMenu.Item("CircleLag").GetValue<bool>())
			{
				if (SKOMenu.Item("DrawQ").GetValue<bool>())
				{
					Utility.DrawCircle(Player.Position, Q.Range, Color.White,
						SKOMenu.Item("CircleThickness").GetValue<Slider>().Value,
						SKOMenu.Item("CircleQuality").GetValue<Slider>().Value);
				}
				if (SKOMenu.Item("DrawW").GetValue<bool>())
				{
					Utility.DrawCircle(Player.Position, W.Range, Color.White,
						SKOMenu.Item("CircleThickness").GetValue<Slider>().Value,
						SKOMenu.Item("CircleQuality").GetValue<Slider>().Value);
				}
				if (SKOMenu.Item("DrawE").GetValue<bool>())
				{
					Utility.DrawCircle(Player.Position, E.Range, Color.White,
						SKOMenu.Item("CircleThickness").GetValue<Slider>().Value,
						SKOMenu.Item("CircleQuality").GetValue<Slider>().Value);
				}
				if (SKOMenu.Item("DrawR").GetValue<bool>())
				{
					Utility.DrawCircle(Player.Position, R.Range, Color.White,
						SKOMenu.Item("CircleThickness").GetValue<Slider>().Value,
						SKOMenu.Item("CircleQuality").GetValue<Slider>().Value);
				}
			}
			else
			{
				if (SKOMenu.Item("DrawQ").GetValue<bool>())
				{
					Drawing.DrawCircle(Player.Position, Q.Range, Color.Green);
				}
				if (SKOMenu.Item("DrawW").GetValue<bool>())
				{
					Drawing.DrawCircle(Player.Position, W.Range, Color.Green);
				}
				if (SKOMenu.Item("DrawE").GetValue<bool>())
				{
					Drawing.DrawCircle(Player.Position, E.Range, Color.Green);
				}
				if (SKOMenu.Item("DrawR").GetValue<bool>())
				{
					Drawing.DrawCircle(Player.Position, R.Range, Color.Green);
				}
			}
		}

		private static void KillSteal(Obj_AI_Hero target)
		{
			var IgniteDmg = Damage.GetSummonerSpellDamage(Player, target, Damage.SummonerSpell.Ignite);
			var QDmg = Damage.GetSpellDamage(Player, target, SpellSlot.Q);
			var WDmg = Damage.GetSpellDamage(Player, target, SpellSlot.Q);
			var EDmg = Damage.GetSpellDamage(Player, target, SpellSlot.Q);

			if(target.IsValidTarget())
			{
				if(SKOMenu.Item("Foguinho").GetValue<bool>() && Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
				{
					if(IgniteDmg > target.Health && Player.Distance(target) < 600)
					{
						Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
					}
				}
			}

			if(SKOMenu.Item("UseQKs").GetValue<bool>() && Q.IsReady() )
			{
				if(QDmg > target.Health && Player.Distance(target) <= Q.Range)
				{
					Q.Cast();
				}
			}
			if(SKOMenu.Item("UseWKs").GetValue<bool>() && W.IsReady() )
			{
				if(WDmg > target.Health && Player.Distance(target) <= W.Range)
				{
					W.Cast();
				}
			}
			if(SKOMenu.Item("UseEKs").GetValue<bool>() && E.IsReady() )
			{
				if(EDmg > target.Health && Player.Distance(target) <= E.Range)
				{
					PredE(target);
				}
			}
		}


		private static void TPREscape()
		{
			if (SKOMenu.Item("TpREscape").GetValue<KeyBind>().Active) 
			{
				if (R.IsReady() && Player.SummonerSpellbook.CanUseSpell(TeleportSlot) == SpellState.Ready) 
				{
					R.Cast();

					foreach (Obj_AI_Turret turrenttp in ObjectManager.Get<Obj_AI_Turret>()) 
					{
						if (turrenttp.IsAlly && turrenttp.Name == "Turret_T1_C_02_A" || turrenttp.Name == "Turret_T2_C_01_A")
						{
							Player.SummonerSpellbook.CastSpell(TeleportSlot, turrenttp);
						}
					}
				}
			}
		}

		private static void Clear()
		{
			var allminions = MinionManager.GetMinions(Player.ServerPosition, 1000, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);

			foreach(var minion in allminions)
			{
				if(minion.IsValidTarget()){
					if(Player.Mana <= 4)
					{
						if(Q.IsReady() && SKOMenu.Item("UseQC").GetValue<bool>() && Player.Distance(minion) <= Q.Range)
						{
							Q.Cast();
						}
						if(W.IsReady() && SKOMenu.Item("UseWC").GetValue<bool>() && Player.Distance(minion) <= W.Range)
						{
							W.Cast();
						}
						if(E.IsReady() && SKOMenu.Item("UseEC").GetValue<bool>() && Player.Distance(minion) <= E.Range)
						{
							E.Cast(minion, PacketCast);
						}
					}
					if(Player.Mana == 5)
					{
						if(SKOMenu.Item("Save").GetValue<bool>())return;
						if(SKOMenu.Item("FPrio").GetValue<StringList>().SelectedIndex == 0 && Q.IsReady() && SKOMenu.Item("UseQC").GetValue<bool>() && Player.Distance(minion) <= Q.Range)
						{
							Q.Cast();
						}
						if(SKOMenu.Item("FPrio").GetValue<StringList>().SelectedIndex == 1 && W.IsReady() && SKOMenu.Item("UseWC").GetValue<bool>() && Player.Distance(minion) <= W.Range)
						{
							W.Cast();
						}
						if(SKOMenu.Item("FPrio").GetValue<StringList>().SelectedIndex == 2 && E.IsReady() && SKOMenu.Item("UseEC").GetValue<bool>() && Player.Distance(minion) <= E.Range)
						{
							E.Cast(minion, PacketCast);
						}
					}
					if(SKOMenu.Item("UseItemsClear").GetValue<bool>()){
						if(Player.Distance(minion) < Player.AttackRange+50){
							TMT.Cast();
							HYD.Cast();
						}
						YMG.Cast();
					}
				}
			}
		}

		private static void Harass()
		{
			var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
			if(target.IsValidTarget())
			{
				if(Player.Mana <= 4)
				{
					if(SKOMenu.Item("UseWH").GetValue<bool>() && W.IsReady() && Player.Distance(target) <= W.Range){
						W.Cast();
					}
					if(SKOMenu.Item("UseEH").GetValue<bool>() && E.IsReady() && Player.Distance(target) <= E.Range){
						PredE(target);
					}
				}
				if(Player.Mana == 5)
				{
					if(SKOMenu.Item("UseWH").GetValue<bool>() && SKOMenu.Item("HPrio").GetValue<StringList>().SelectedIndex == 0 && W.IsReady())
					{
						W.Cast();
					}
					if(SKOMenu.Item("UseEH").GetValue<bool>() && SKOMenu.Item("HPrio").GetValue<StringList>().SelectedIndex == 1 && E.IsReady()){
						PredE(target);
					}
				}
				if(SKOMenu.Item("UseItemsHarass").GetValue<bool>()){
					if(Player.Distance(target) < Player.AttackRange+50){
						TMT.Cast();
						HYD.Cast();
						STD.Cast();
					}
					BWC.Cast(target);
					BRK.Cast(target);
					RO.Cast(target);
					YMG.Cast();

				}
			}
		}

		private static void PredE(Obj_AI_Hero target)
		{
			var pred = E.GetPrediction(target);
			if(pred.Hitchance >= HitChance.Medium){
				E.Cast(pred.CastPosition, PacketCast);
			}
		}

		private static void TripleQ(Obj_AI_Hero target)
		{
			if(target.IsValidTarget()){
				if(Player.Mana == 5 && R.IsReady() && Player.Distance(target) <= R.Range && Q.IsReady())
				{
					R.Cast();
				}
				if(Player.Mana < 5)
				{
					Drawing.DrawText(Player.Position.X, Player.Position.Y, Color.Red, "R is not ready, or you do not have 5 ferocity");
				}


				if(Player.Mana == 5 && Player.HasBuff("RengarR") && Q.IsReady() && Player.Distance(target) <= Q.Range)
				{
					Q.Cast();
				}
				if(Player.Mana == 5 && !Player.HasBuff("RengarR") && Q.IsReady() && Player.Distance(target) <= Q.Range)
				{
					Q.Cast();
				}
				if(Player.Mana <= 4)
				{
					if(Q.IsReady() && Player.Distance(target) <= Q.Range)
					{
						Q.Cast();
					}
					if(W.IsReady() && Player.Distance(target) <= W.Range)
					{
						W.Cast();
					}
					if(E.IsReady() && Player.Distance(target) <= E.Range)
					{
						E.Cast(target, PacketCast);
					}
				}
				if(Player.Distance(target) < Player.AttackRange+50){
					TMT.Cast();
					HYD.Cast();
					STD.Cast();
				}
				BWC.Cast(target);
				BRK.Cast(target);
				RO.Cast(target);
				YMG.Cast();

			}
		}

		private static void AutoHeal()
		{
			if (Player.HasBuff("Recall")) return;
			if(Player.Mana <= 4)return;

			if(SKOMenu.Item("UseAutoW").GetValue<bool>() && Player.Mana == 5 && !Recall)
			{
				if(W.IsReady() && Player.Health < (Player.MaxHealth * (SKOMenu.Item("HpAutoW").GetValue<Slider>().Value) / 100))
				{
					W.Cast();
				}
			}
		}

	}
}
