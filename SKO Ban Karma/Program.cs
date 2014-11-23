using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.CompilerServices;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SKO_Ban_Karma
{
    class Program
    {
        private const string ChampionName = "Karma";

        private static Spell Q, W, E, R;

        private static Menu SKOMenu;

        private static Orbwalking.Orbwalker Orbwalker;

        private static Obj_AI_Hero Player;

        private static Items.Item BKR, BWC, YOU, DFG, FQC;

        private static SpellSlot IgniteSlot;


        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args) 
        {
            Player = ObjectManager.Player;

            if (Player.BaseSkinName != ChampionName) return;

            Q = new Spell(SpellSlot.Q, 1000f);
            W = new Spell(SpellSlot.W, 654f);
            E = new Spell(SpellSlot.E, 800f);
            R = new Spell(SpellSlot.R, 0f);

            Q.SetSkillshot(0.25f, 70, 1800, true, SkillshotType.SkillshotLine);


            BKR = new Items.Item(3153, 450f);
            BWC = new Items.Item(3144, 450f);
            YOU = new Items.Item(3142, 185f);
            DFG = new Items.Item(3128, 750f);
            FQC = new Items.Item(3092, 850f);

            IgniteSlot = Player.GetSpellSlot("SummonetDot");

            //SKO SKO Ban Karma
            SKOMenu = new Menu(ChampionName, "SKOBanKarma", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "TargetSelector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            SKOMenu.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            SKOMenu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(SKOMenu.SubMenu("Orbwalking"));

            //Combo
            SKOMenu.AddSubMenu(new Menu("Combo", "Combo"));
            SKOMenu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
            SKOMenu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
            SKOMenu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);
            SKOMenu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R")).SetValue(true);
            SKOMenu.SubMenu("Combo").AddItem(new MenuItem("Support", "Support Mode")).SetValue(false);
            SKOMenu.SubMenu("Combo").AddItem(new MenuItem("UseAutoW", "Auto W")).SetValue(true);
            SKOMenu.SubMenu("Combo").AddItem(new MenuItem("AutoWHp", "Auto W HP %")).SetValue<Slider>(new Slider(40, 1, 100));
            SKOMenu.SubMenu("Combo").AddItem(new MenuItem("UseItems", "Use Items")).SetValue(true);
            SKOMenu.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Harass
            SKOMenu.AddSubMenu(new Menu("Harass", "Harass"));
            SKOMenu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
            SKOMenu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W")).SetValue(true);
            SKOMenu.SubMenu("Harass").AddItem(new MenuItem("UseRHarass", "Use R")).SetValue(true);
            SKOMenu.SubMenu("Harass").AddItem(new MenuItem("ActiveHarass", "Harass key").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

            //Farm
            SKOMenu.AddSubMenu(new Menu("Lane Clear", "Lane"));
            SKOMenu.SubMenu("Lane").AddItem(new MenuItem("UseQLane", "Use Q")).SetValue(true);
            SKOMenu.SubMenu("Lane").AddItem(new MenuItem("ActiveLane", "Lane Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            //Kill Steal
            SKOMenu.AddSubMenu(new Menu("KillSteal", "Ks"));
            SKOMenu.SubMenu("Ks").AddItem(new MenuItem("ActiveKs", "Use KillSteal")).SetValue(true);
            SKOMenu.SubMenu("Ks").AddItem(new MenuItem("UseQKs", "Use Q")).SetValue(true);
            SKOMenu.SubMenu("Ks").AddItem(new MenuItem("UseIgnite", "Use Ignite")).SetValue(true);


            //Drawings
            SKOMenu.AddSubMenu(new Menu("Drawings", "Drawings"));
            SKOMenu.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            SKOMenu.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
            SKOMenu.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            SKOMenu.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            SKOMenu.SubMenu("Drawings").AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            SKOMenu.SubMenu("Drawings").AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));

            SKOMenu.AddToMainMenu();

            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;

            Game.PrintChat("<font color='#1d87f2'>SKO Ban Karma Loaded!</font>");
        }

        private static void OnGameUpdate(EventArgs args) 
        {

            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (SKOMenu.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo(target);
            }
            if (SKOMenu.Item("ActiveHarass").GetValue<KeyBind>().Active)
            {
                Harass(target);
            }
            if (SKOMenu.Item("ActiveLane").GetValue<KeyBind>().Active)
            {
                Farm();
            }
            if (SKOMenu.Item("ActiveKs").GetValue<bool>())
            {
                KillSteal(target);
            }
            AutoW(target);
        }

        private static void Combo(Obj_AI_Hero target) {

            if (SKOMenu.Item("UseItems").GetValue<bool>())
            {
                BKR.Cast(target);
                YOU.Cast();
                BWC.Cast(target);
                DFG.Cast(target);
                FQC.Cast(target);

            }
            if (SKOMenu.Item("UseQCombo").GetValue<bool>() && Q.IsReady()) 
            {
                if (SKOMenu.Item("UseRCombo").GetValue<bool>() && R.IsReady()) 
                {
                    if(Player.Distance(target) <= Q.Range && Q.IsReady()){
                    R.Cast();
                    }
                }
                 if (Player.Distance(target) <= Q.Range && Player.HasBuff("KarmaMantra")) 
                  {
                    Q.Cast(target);
                  }else{
                    Q.Cast(target);
                  }
            }
            if (SKOMenu.Item("UseWCombo").GetValue<bool>() && W.IsReady()) 
            {
                if (Player.Distance(target) <= W.Range && !Player.HasBuff("KarmaMantra")) 
                {
                    W.Cast(target);
                }
            }
            if (!Player.HasBuff("KarmaMantra") && SKOMenu.Item("UseECombo").GetValue<bool>() && !SKOMenu.Item("Support").GetValue<bool>())
            {
                if(E.IsReady() && !E.Collision)
                    E.Cast(Player);
            }

        }
        private static void AutoW(Obj_AI_Hero target) {
            if (Player.HasBuff("Recall")) return;
            //Auto W
            if (Player.Health <= (Player.MaxHealth * (SKOMenu.Item("AutoWHp").GetValue<Slider>().Value) / 100))
            {
                if (SKOMenu.Item("AutoW").GetValue<bool>() && W.IsReady())
                {
                    if(R.IsReady() && Player.Distance(target) <= W.Range){
                        R.Cast();
                    }
                    if(Player.Distance(target) <= W.Range && Player.HasBuff("KarmaMantra"))
                    {
                        W.Cast(target);
                    }
                }
            }
        }
        private static void Harass(Obj_AI_Hero target)
        {
            if (SKOMenu.Item("UseItems").GetValue<bool>())
            {
                BKR.Cast(target);
                YOU.Cast();
                BWC.Cast(target);
                DFG.Cast(target);
                FQC.Cast(target);

            }
            if (SKOMenu.Item("UseQHarass").GetValue<bool>() && Q.IsReady())
            {
                if (SKOMenu.Item("UseRHarass").GetValue<bool>() && R.IsReady()) 
                {
                   if(Player.Distance(target) <= Q.Range){
                    R.Cast();
                    }
                }
                 if (Player.Distance(target) <= Q.Range && Player.HasBuff("KarmaMantra")) 
                  {
                    Q.Cast(target);
                  }else{
                    Q.Cast(target);
                  }
            }
            if (SKOMenu.Item("UseWHarass").GetValue<bool>() && W.IsReady())
            {
                if (Player.Distance(target) <= W.Range)
                {
                    W.Cast(target);
                }
            }

        }
        private static void KillSteal(Obj_AI_Hero target)
        {
            var igniteDmg = Damage.GetSummonerSpellDamage(Player, target, Damage.SummonerSpell.Ignite);
            var QDmg = Damage.GetSpellDamage(Player, target, SpellSlot.Q);

            if (target.IsValidTarget())
            {
                if (SKOMenu.Item("UseIgnite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                {
                    if (igniteDmg > target.Health)
                    {
                        Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                    }
                }

                if (SKOMenu.Item("UseQKs").GetValue<bool>() && Player.Distance(target) <= Q.Range)
                {
                    if (Q.IsReady() && target.Health < QDmg) 
                    {
                        Q.Cast();
                    }
                }
            }

        }
        private static void Farm()
        {
            var allminions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);

            if (SKOMenu.Item("UseQLane").GetValue<bool>())
            {
                foreach (var minion in allminions)
                {
                    if (Q.IsReady() && minion.IsValidTarget() && Player.Distance(minion) <= Q.Range)
                    {
                        Q.Cast(minion);
                    }

                }
            }
        }
        private static void OnDraw(EventArgs args)
        {
            if (SKOMenu.Item("CircleLag").GetValue<bool>())
            {
                if (SKOMenu.Item("DrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.White,
                        SKOMenu.Item("CircleThickness").GetValue<Slider>().Value,
                        SKOMenu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (SKOMenu.Item("DrawW").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.White,
                        SKOMenu.Item("CircleThickness").GetValue<Slider>().Value,
                        SKOMenu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (SKOMenu.Item("DrawE").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.White,
                        SKOMenu.Item("CircleThickness").GetValue<Slider>().Value,
                        SKOMenu.Item("CircleQuality").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (SKOMenu.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.White);
                }
                if (SKOMenu.Item("DrawW").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.White);
                }
                if (SKOMenu.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.White);
                }

            }
        }
    }
}
