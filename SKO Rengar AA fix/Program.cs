using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SKO_Rengar_V2_AA_Dix
{
    class Program
    {
        private static Obj_AI_Hero player;
        private static Spell Q, W, E, R;
        private static Items.Item BWC, BRK, RO, YMG, STD, TMT, HYD, DFG;
        private static bool PacketCast;
        private static Menu SKOMenu;
        private static SpellSlot IgniteSlot, TeleportSlot;
		private static Obj_AI_Hero target;
        private static Orbwalking.Orbwalker Orbwalker;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            player = ObjectManager.Player;
            if (player.ChampionName != "Rengar")
                return;

            IgniteSlot = player.GetSpellSlot("SummonerDot");
            TeleportSlot = player.GetSpellSlot("SummonerTeleport");

            W = new Spell(SpellSlot.W, 450f);
            E = new Spell(SpellSlot.E, 980f);
            R = new Spell(SpellSlot.R, 2000f);

            E.SetSkillshot(0.25f, 70f, 1500f, true, SkillshotType.SkillshotLine);

            HYD = new Items.Item(3074, 420f);
            TMT = new Items.Item(3077, 420f);
            BRK = new Items.Item(3153, 450f);
            BWC = new Items.Item(3144, 450f);
            RO = new Items.Item(3143, 500f);
            DFG = new Items.Item(3128, 750f);

            SKOMenu = new Menu("SKO Rengar", "SKORengar", true);

            var SKOTs = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(SKOTs);

            SKOMenu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            new Orbwalking.Orbwalker(SKOMenu.SubMenu("Orbwalking"));


            var Combo = new Menu("Combo", "Combo");
            Combo.AddItem(new MenuItem("CPrio", "Empowered Priority").SetValue(new StringList(new[] { "Q", "W", "E" }, 2)));
            Combo.AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            Combo.AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            Combo.AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            Combo.AddItem(new MenuItem("UseEEm", "Use Empowered E if target is out of Q Range").SetValue(false));
            Combo.AddItem(new MenuItem("UseAutoW", "Auto W").SetValue(true));
            Combo.AddItem(new MenuItem("HpAutoW", "Min hp").SetValue(new Slider(20, 1, 100)));
            Combo.AddItem(new MenuItem("TripleQ", "Triple Q").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
            Combo.AddItem(new MenuItem("activeCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));


            var items = new Menu("Items", "Items");
            items.AddItem(new MenuItem("Hydra", "Hydra").SetValue(true));
            items.AddItem(new MenuItem("BOTRK", "BOTRK").SetValue(true));
            items.AddItem(new MenuItem("RO", "Randuin's Omen").SetValue(true));
            items.AddItem(new MenuItem("SOD", "Sword of the Divine").SetValue(true));
            items.AddItem(new MenuItem("YMU", "Youmuu's Ghostblade").SetValue(true));
            items.AddItem(new MenuItem("DFG", "Deathfire Grasp").SetValue(true));

            var Harass = new Menu("Harass", "Harass");
            Harass.AddItem(new MenuItem("HPrio", "Empowered Priority").SetValue(new StringList(new[] { "W", "E" }, 1)));
            Harass.AddItem(new MenuItem("UseWH", "Use W").SetValue(true));
            Harass.AddItem(new MenuItem("UseEH", "Use E").SetValue(true));
            Harass.AddItem(new MenuItem("activeHarass", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            var JLClear = new Menu("Jungle/Lane Clear", "JLClear");
            JLClear.AddItem(new MenuItem("FPrio", "Empowered Priority").SetValue(new StringList(new[] { "Q", "W", "E" }, 0)));
            JLClear.AddItem(new MenuItem("UseQC", "Use Q").SetValue(true));
            JLClear.AddItem(new MenuItem("UseWC", "Use W").SetValue(true));
            JLClear.AddItem(new MenuItem("UseEC", "Use E").SetValue(true));
            JLClear.AddItem(new MenuItem("Save", "Save Ferocity").SetValue(false));
            JLClear.AddItem(new MenuItem("activeClear", "Clear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            var KillSteal = new Menu("KillSteal", "KillSteal");
            KillSteal.AddItem(new MenuItem("Foguinho", "Use Ignite").SetValue(true));
            KillSteal.AddItem(new MenuItem("UseQKs", "Use Q").SetValue(true));
            KillSteal.AddItem(new MenuItem("UseWKs", "Use W").SetValue(true));
            KillSteal.AddItem(new MenuItem("UseEKs", "Use E").SetValue(true));
            KillSteal.AddItem(new MenuItem("UseFlashKs", "Use Flash KillSteal(Kappa)").SetValue(false));

            var drawc = new Menu("Drawing", "Drawing");
            drawc.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(true));
            drawc.AddItem(new MenuItem("DrawW", "Draw W").SetValue(true));
            drawc.AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            drawc.AddItem(new MenuItem("DrawR", "Draw R").SetValue(true));
            drawc.AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            drawc.AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            drawc.AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));

            var Misc = new Menu("Misc", "Misc");
            Misc.AddItem(new MenuItem("UsePacket", "Use Packet").SetValue(true));
            Misc.AddItem(new MenuItem("TpREscape", "R + TP Escape").SetValue<KeyBind>(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));


            SKOMenu.AddSubMenu(SKOTs);
            // SKOMenu.AddSubMenu(OrbMenu);
            SKOMenu.AddSubMenu(Combo);
            SKOMenu.AddSubMenu(Harass);
            SKOMenu.AddSubMenu(JLClear);
            SKOMenu.AddSubMenu(items);
            SKOMenu.AddSubMenu(KillSteal);
            SKOMenu.AddSubMenu(drawc);
            SKOMenu.AddSubMenu(Misc);
            SKOMenu.AddToMainMenu();

            Game.PrintChat("<font color='#07B88C'>SKO Rengar V2</font> Loaded!");

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += OnDraw;
        }


        private static void Game_OnGameUpdate(EventArgs args)
        {
            //Checks
            PacketCast = SKOMenu.Item("UsePacket").GetValue<bool>();
            TpREscape();
            AutoHeal();
            Q = new Spell(SpellSlot.Q, player.AttackRange + 100);
            YMG = new Items.Item(3142, player.AttackRange + 50);
            STD = new Items.Item(3131, player.AttackRange + 50);
            

            if (SKOMenu.Item("activeClear").GetValue<KeyBind>().Active)
            {
                Clear();
            }
				

			foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.IsEnemy && !unit.IsDead && unit.IsValidTarget()))
            {
                if (player.Distance(enemy) <= W.Range)
                {
                    target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                }
                else if (player.Distance(enemy) <= E.Range)
                {
                    target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical); 
                }
                else if(player.Distance(enemy) <= R.Range)
                {
                    target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                }
            }

			if (SKOMenu.Item("TripleQ").GetValue<KeyBind>().Active)
			{
				TripleQ(target);
			}

            if (SKOMenu.Item("activeCombo").GetValue<KeyBind>().Active)
            {

                Combo(target);

            }
            if (SKOMenu.Item("activeHarass").GetValue<KeyBind>().Active)
            {
                Harass(target);
            }

            KillSteal(target);

        }

        private static void OnDraw(EventArgs args)
        {
            if (SKOMenu.Item("CircleLag").GetValue<bool>())
            {
                if (SKOMenu.Item("DrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(player.Position, Q.Range, Color.White,
                        SKOMenu.Item("CircleThickness").GetValue<Slider>().Value,
                        SKOMenu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (SKOMenu.Item("DrawW").GetValue<bool>())
                {
                    Utility.DrawCircle(player.Position, W.Range, Color.White,
                        SKOMenu.Item("CircleThickness").GetValue<Slider>().Value,
                        SKOMenu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (SKOMenu.Item("DrawE").GetValue<bool>())
                {
                    Utility.DrawCircle(player.Position, E.Range, Color.White,
                        SKOMenu.Item("CircleThickness").GetValue<Slider>().Value,
                        SKOMenu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (SKOMenu.Item("DrawR").GetValue<bool>())
                {
                    Utility.DrawCircle(player.Position, R.Range, Color.White,
                        SKOMenu.Item("CircleThickness").GetValue<Slider>().Value,
                        SKOMenu.Item("CircleQuality").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (SKOMenu.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(player.Position, Q.Range, Color.Green);
                }
                if (SKOMenu.Item("DrawW").GetValue<bool>())
                {
                    Drawing.DrawCircle(player.Position, W.Range, Color.Green);
                }
                if (SKOMenu.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(player.Position, E.Range, Color.Green);
                }
                if (SKOMenu.Item("DrawR").GetValue<bool>())
                {
                    Drawing.DrawCircle(player.Position, R.Range, Color.Green);
                }
            }
        }

        private static void Combo(Obj_AI_Hero unitHero)
        {
            if(!unitHero.IsValidTarget()) return;

            if (player.Mana <= 4)
            {
                if (SKOMenu.Item("UseQ").GetValue<bool>() && player.Distance(unitHero) <= Q.Range)
                {
                    CastQ(unitHero);
                }
                if (SKOMenu.Item("UseW").GetValue<bool>() && player.Distance(unitHero) <= W.Range)
                {
                    CastW(unitHero);
                }
                if (SKOMenu.Item("UseE").GetValue<bool>() && player.Distance(unitHero) <= E.Range)
                {
                    CastE(unitHero);
                }
            }

            if (player.Mana == 5)
            {
                if (SKOMenu.Item("UseQ").GetValue<bool>() && SKOMenu.Item("CPrio").GetValue<StringList>().SelectedIndex == 0 && player.Distance(unitHero) <= Q.Range)
                {
                    CastQ(unitHero);
                }
                if (SKOMenu.Item("UseW").GetValue<bool>() && SKOMenu.Item("CPrio").GetValue<StringList>().SelectedIndex == 1 && player.Distance(unitHero) <= W.Range)
                {
                    CastW(unitHero);
                }
                if (SKOMenu.Item("UseE").GetValue<bool>() && SKOMenu.Item("CPrio").GetValue<StringList>().SelectedIndex == 2 && player.Distance(unitHero) <= E.Range)
                {
                    CastE(unitHero);
                }

                if (SKOMenu.Item("UseEEm").GetValue<bool>() && player.Distance(unitHero) > Q.Range+100f || !Q.IsReady())
                {
                    CastE(unitHero);
                }
            }
            UseItems(target);
        }

        private static void Harass(Obj_AI_Hero unitHero)
        {
            if (!unitHero.IsValidTarget()) return;

            if (player.Mana <= 4)
            {
                if (SKOMenu.Item("UseWH").GetValue<bool>() && player.Distance(unitHero) <= W.Range)
                {
                    CastW(unitHero);
                }
                if (SKOMenu.Item("UseEH").GetValue<bool>() && player.Distance(unitHero) <= E.Range)
                {
                    CastE(unitHero);
                }
            }
            if (player.Mana == 5)
            {
                if (SKOMenu.Item("UseWH").GetValue<bool>() && SKOMenu.Item("HPrio").GetValue<StringList>().SelectedIndex == 0)
                {
                    CastW(unitHero);
                }
                if (SKOMenu.Item("UseEH").GetValue<bool>() && SKOMenu.Item("HPrio").GetValue<StringList>().SelectedIndex == 1)
                {
                    CastE(unitHero);
                }
            }
            UseItems(target);
        }

        private static void KillSteal(Obj_AI_Hero unitHero)
        {
            if(!unitHero.IsValidTarget()) return;

            var igniteDmg = player.GetSummonerSpellDamage(unitHero, Damage.SummonerSpell.Ignite);
            var qDmg = player.GetSpellDamage(unitHero, SpellSlot.Q);
            var wDmg = player.GetSpellDamage(unitHero, SpellSlot.Q);
            var eDmg = player.GetSpellDamage(unitHero, SpellSlot.Q);

                if (SKOMenu.Item("Foguinho").GetValue<bool>() && player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                {
                    if (igniteDmg > unitHero.Health && player.Distance(unitHero) < 600)
                    {
                        player.Spellbook.CastSpell(IgniteSlot, unitHero);
                    }
                }
          

            if (SKOMenu.Item("UseQKs").GetValue<bool>())
            {
                if (qDmg > unitHero.Health && player.Distance(unitHero) <= Q.Range)
                {
                    CastQ(unitHero);
                }
            }
            if (SKOMenu.Item("UseWKs").GetValue<bool>())
            {
                if (wDmg > unitHero.Health && player.Distance(unitHero) <= W.Range)
                {
                    CastW(unitHero);
                }
            }
            if (SKOMenu.Item("UseEKs").GetValue<bool>())
            {
                if (eDmg > unitHero.Health && player.Distance(unitHero) <= E.Range)
                {
                    CastE(unitHero);
                }
            }
        }

        private static void Clear()
        {
            var allminions = MinionManager.GetMinions(player.ServerPosition, 1000, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);

            foreach (var minion in allminions.Where(minion => minion.IsValidTarget()))
            {
                if (player.Mana <= 4)
                {
                    if (Q.IsReady() && SKOMenu.Item("UseQC").GetValue<bool>() && player.Distance(minion) <= Q.Range)
                    {
                        Q.Cast();
                    }
                    if (W.IsReady() && SKOMenu.Item("UseWC").GetValue<bool>() && player.Distance(minion) <= W.Range)
                    {
                        W.Cast();
                    }
                    if (E.IsReady() && SKOMenu.Item("UseEC").GetValue<bool>() && player.Distance(minion) <= E.Range)
                    {
                        E.Cast(minion, PacketCast);
                    }
                }
                if (player.Mana == 5)
                {
                    if (SKOMenu.Item("Save").GetValue<bool>()) return;
                    if (Q.IsReady() && SKOMenu.Item("FPrio").GetValue<StringList>().SelectedIndex == 0 && SKOMenu.Item("UseQC").GetValue<bool>() && player.Distance(minion) <= Q.Range)
                    {
                        Q.Cast();
                    }
                    if (W.IsReady() && SKOMenu.Item("FPrio").GetValue<StringList>().SelectedIndex == 1 && SKOMenu.Item("UseWC").GetValue<bool>() && player.Distance(minion) <= W.Range)
                    {
                        W.Cast();
                    }
                    if (E.IsReady() && SKOMenu.Item("FPrio").GetValue<StringList>().SelectedIndex == 2 && SKOMenu.Item("UseEC").GetValue<bool>() && player.Distance(minion) <= E.Range)
                    {
                        E.Cast(minion, PacketCast);
                    }
                }
                UseItems(minion, true);
            }
        }

		private static void TripleQ(Obj_AI_Hero unitHero)
        {
            if(!unitHero.IsValidTarget()) return;

			Orbwalking.Orbwalk(unitHero, Game.CursorPos);

               /* if(player.Mana <= 4 || !R.IsReady())
                {
                 var splayer = Drawing.WorldToScreen(player.ServerPosition);
                Drawing.DrawText(splayer.X, splayer.Y, Color.Red, "R is not ready or you do not have 5 ferocity");
                }*/
                

            if (player.Mana == 5 && R.IsReady() && player.Distance(unitHero) <= R.Range && Q.IsReady())
            {
                R.Cast();
            }
            if (player.Mana == 5 && player.HasBuff("RengarR") && player.Distance(unitHero) <= Q.Range)
            {
                CastQ(unitHero);
            }
            if (player.Mana == 5 && !player.HasBuff("RengarR") && player.Distance(unitHero) <= Q.Range)
            {
                CastQ(unitHero);
            }
            if (player.Mana <= 4)
            {
                if (player.Distance(unitHero) <= Q.Range)
                {
                    CastQ(unitHero);
                }
                if (player.Distance(unitHero) <= W.Range)
                {
                    CastW(unitHero);
                }
                if (player.Distance(unitHero) <= E.Range)
                {
                    CastE(unitHero);
                }
            }
            UseItems(target);
        }


        private static void AutoHeal()
        {
            if (player.HasBuff("Recall") || player.Mana <= 4) return;

            if (SKOMenu.Item("UseAutoW").GetValue<bool>())
            {
                if (W.IsReady() && player.Health < (player.MaxHealth * (SKOMenu.Item("HpAutoW").GetValue<Slider>().Value) / 100))
                {
                    W.Cast();
                }
            }
        }

        private static void TpREscape()
        {
            if (!SKOMenu.Item("TpREscape").GetValue<KeyBind>().Active) return;


            if (R.IsReady() && player.Spellbook.CanUseSpell(TeleportSlot) == SpellState.Ready)
            {
                R.Cast();

                foreach (Obj_AI_Turret turrenttp in ObjectManager.Get<Obj_AI_Turret>().Where(turrenttp => turrenttp.IsAlly && turrenttp.Name == "Turret_T1_C_02_A" || turrenttp.Name == "Turret_T2_C_01_A"))
                {
                    player.Spellbook.CastSpell(TeleportSlot, turrenttp);
                }
            }
        }

        private static void CastQ(Obj_AI_Hero unitHeroQ)
        {
            if (!Q.IsReady() || !unitHeroQ.IsValidTarget(Q.Range)) return;
            try
            {
                //if(LXOrbwalker.IsAutoAttackReset(_player.Spellbook.GetSpell(SpellSlot.Q).SData.Name))
                //Utility.DelayAction.Add(260, LXOrbwalker.ResetAutoAttackTimer);
                //Orbwalking.ResetAutoAttackTimer();
                Q.Cast(PacketCast);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e.Message);
            }
        }

        private static void CastW(Obj_AI_Hero unitHeroW)
        {
            if (!W.IsReady() || !unitHeroW.IsValidTarget(W.Range)) return;
            try
            {
                W.Cast(PacketCast);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e.ToString());
            }
        }

        private static void CastE(Obj_AI_Hero unitHeroE)
        {
            if (!E.IsReady() || !unitHeroE.IsValidTarget(E.Range)) return;
            try
            {
                var epred = E.GetPrediction(unitHeroE);
                if (epred.Hitchance >= HitChance.High)
                {
                    E.Cast(epred.CastPosition, PacketCast);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e.ToString());
            }
        }

        private static void UseItems(Obj_AI_Base unit, bool isMinion = false)
        {
         if(!unit.IsValidTarget()) return;


            if (SKOMenu.Item("Hydra").GetValue<bool>() && player.Distance(unit) < HYD.Range)
            {
                HYD.Cast();
            }
            if (SKOMenu.Item("Hydra").GetValue<bool>() && player.Distance(unit) < TMT.Range)
            {
                TMT.Cast();
            }
            if (SKOMenu.Item("BOTRK").GetValue<bool>() && player.Distance(unit) <= BRK.Range)
            {
                if(isMinion) return;
                BRK.Cast(unit);
            }
            if (SKOMenu.Item("BOTRK").GetValue<bool>() && player.Distance(unit) <= BWC.Range)
            {
                
                BWC.Cast(unit);
            }
            if (SKOMenu.Item("RO").GetValue<bool>() && player.Distance(unit) <= RO.Range)
            {
                if (isMinion) return;
                RO.Cast();
            }
            if (SKOMenu.Item("DFG").GetValue<bool>() && player.Distance(unit) <= DFG.Range)
            {
                if(isMinion) return;
                DFG.Cast(unit);
            }
            if (SKOMenu.Item("YMU").GetValue<bool>() && player.Distance(unit) <= YMG.Range)
            {
                YMG.Cast();
            }
            if (SKOMenu.Item("SOD").GetValue<bool>() && player.Distance(unit) <= STD.Range)
            {
                STD.Cast();
            }
        }

    }
}
