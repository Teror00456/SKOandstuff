using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SKO_Ban_Karma
{
    class Program
    {
        private const string ChampionName = "Karma";

        private static Spell _q, _w, _e, _r;

        private static Menu _skoMenu;

        private static Obj_AI_Hero _player;

        private static Items.Item _bkr, _bwc, _you, _dfg, _fqc;

        private static SpellSlot _igniteSlot;


        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args) 
        {
            _player = ObjectManager.Player;

            if (_player.BaseSkinName != ChampionName) return;

            _q = new Spell(SpellSlot.Q, 910f);
            _w = new Spell(SpellSlot.W, 654f);
            _e = new Spell(SpellSlot.E, 800f);
            _r = new Spell(SpellSlot.R, 0f);

			_q.SetSkillshot(0.25f, 90f, 1700f, true, SkillshotType.SkillshotLine);

            _bkr = new Items.Item(3153, 450f);
            _bwc = new Items.Item(3144, 450f);
            _you = new Items.Item(3142, 185f);
            _dfg = new Items.Item(3128, 750f);
            _fqc = new Items.Item(3092, 850f);

            _igniteSlot = _player.GetSpellSlot("SummonetDot");

            //SKO SKO Ban Karma
            _skoMenu = new Menu(ChampionName, "SKOBanKarma", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _skoMenu.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            _skoMenu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            new Orbwalking.Orbwalker(_skoMenu.SubMenu("Orbwalking"));

            //Combo
            _skoMenu.AddSubMenu(new Menu("Combo", "Combo"));
            _skoMenu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
            _skoMenu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
            _skoMenu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);
            _skoMenu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R")).SetValue(true);
            _skoMenu.SubMenu("Combo").AddItem(new MenuItem("Support", "Support Mode")).SetValue(false);
            _skoMenu.SubMenu("Combo").AddItem(new MenuItem("UseAutoW", "Auto W")).SetValue(true);
            _skoMenu.SubMenu("Combo").AddItem(new MenuItem("AutoWHp", "Auto W HP %")).SetValue(new Slider(40, 1));
            _skoMenu.SubMenu("Combo").AddItem(new MenuItem("UseItems", "Use Items")).SetValue(true);
            _skoMenu.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Harass
            _skoMenu.AddSubMenu(new Menu("Harass", "Harass"));
            _skoMenu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
            _skoMenu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W")).SetValue(true);
            _skoMenu.SubMenu("Harass").AddItem(new MenuItem("UseRHarass", "Use R")).SetValue(true);
            _skoMenu.SubMenu("Harass").AddItem(new MenuItem("ActiveHarass", "Harass key").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            //Farm
            _skoMenu.AddSubMenu(new Menu("Lane Clear", "Lane"));
            _skoMenu.SubMenu("Lane").AddItem(new MenuItem("UseQLane", "Use Q")).SetValue(true);
            _skoMenu.SubMenu("Lane").AddItem(new MenuItem("ActiveLane", "Lane Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            //Kill Steal
            _skoMenu.AddSubMenu(new Menu("KillSteal", "Ks"));
            _skoMenu.SubMenu("Ks").AddItem(new MenuItem("UseQKs", "Use Q")).SetValue(true);
            _skoMenu.SubMenu("Ks").AddItem(new MenuItem("UseIgnite", "Use Ignite")).SetValue(true);


            //Drawings
            _skoMenu.AddSubMenu(new Menu("Drawings", "Drawings"));
            _skoMenu.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            _skoMenu.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
            _skoMenu.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            _skoMenu.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            _skoMenu.SubMenu("Drawings").AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            _skoMenu.SubMenu("Drawings").AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));

            _skoMenu.AddToMainMenu();

            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;

            Game.PrintChat("SKO Ban Karma v2");
        }

        private static void OnGameUpdate(EventArgs args) 
        {

			if (_skoMenu.Item("ActiveLane").GetValue<KeyBind>().Active)
			{
				Farm();
			}

            var target = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
            if (_skoMenu.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo(target);

            }
            if (_skoMenu.Item("ActiveHarass").GetValue<KeyBind>().Active)
            {
                Harass(target);
            }

			KillSteal(target);
            AutoW(target);
        }

        private static void Combo(Obj_AI_Hero target) {

            if (_skoMenu.Item("UseItems").GetValue<bool>())
            {
                _bkr.Cast(target);
                _you.Cast();
                _bwc.Cast(target);
                _dfg.Cast(target);
                _fqc.Cast(target);

            }
			if (_skoMenu.Item("UseWCombo").GetValue<bool>() && _w.IsReady()) 
			{
				if (_player.Distance(target) <= _w.Range) 
				{
					_w.Cast(target);
				}
			}
			if (!_skoMenu.Item("Support").GetValue<bool>() && _skoMenu.Item("UseECombo").GetValue<bool>() && _e.IsReady())
			{
				if(!_player.HasBuff("KarmaMantra")){
					_e.Cast(_player);
				}

			}

			if(_player.HasBuff("KarmaMantra")){
				_q.Range = 1100f;
			}else{
				_q.Range = 940f;
			}
            if (!_skoMenu.Item("UseQCombo").GetValue<bool>() || !_q.IsReady()) return;
            var pred = _q.GetPrediction(target);
            if(pred.Hitchance == HitChance.Collision){
                //detuks
                Obj_AI_Base fistCol = pred.CollisionObjects.OrderBy(unit => unit.Distance(_player.ServerPosition)).First();
                if (fistCol.Distance(pred.CastPosition) < (180 - fistCol.BoundingRadius / 2) && fistCol.Distance(target.ServerPosition) < (200 - fistCol.BoundingRadius / 2))
                {
					if (_q.IsReady() && _skoMenu.Item("UseRCombo").GetValue<bool>() && _r.IsReady())
                    {
                        _r.Cast();
                    }

                    _q.Cast(pred.CastPosition);
                }
            }else if(pred.Hitchance != HitChance.Impossible && pred.CollisionObjects.Count == 0)
            {
				if (_q.IsReady() && _skoMenu.Item("UseRCombo").GetValue<bool>() && _r.IsReady())
                {
					if(!_q.IsReady())return;
                    _r.Cast();
                }
                _q.Cast(pred.CastPosition);
            }
        }
        private static void AutoW(Obj_AI_Hero target) {
            if (_player.HasBuff("Recall")) return;
            //Auto W
            if (_player.Health <= (_player.MaxHealth * (_skoMenu.Item("AutoWHp").GetValue<Slider>().Value) / 100))
            {
                if (_skoMenu.Item("AutoW").GetValue<bool>() && _w.IsReady())
                {
                    if(_r.IsReady() && _player.Distance(target) <= _w.Range){
                        _r.Cast();
                    }
                    if(_player.Distance(target) <= _w.Range && _player.HasBuff("KarmaMantra"))
                    {
                        _w.Cast(target);
                    }
                }
            }
        }
        private static void Harass(Obj_AI_Hero target)
        {
            if (_skoMenu.Item("UseItems").GetValue<bool>())
            {
                _bkr.Cast(target);
                _you.Cast();
                _bwc.Cast(target);
                _dfg.Cast(target);
                _fqc.Cast(target);

            }
				if(_player.HasBuff("KarmaMantra")){
					_q.Range = 1100f;
				}else{
					_q.Range = 940f;
				}
				if (!_skoMenu.Item("UseQHarass").GetValue<bool>() || !_q.IsReady()) return;
				var pred = _q.GetPrediction(target);
				if(pred.Hitchance == HitChance.Collision){
					//detuks
					Obj_AI_Base fistCol = pred.CollisionObjects.OrderBy(unit => unit.Distance(_player.ServerPosition)).First();
					if (fistCol.Distance(pred.CastPosition) < (180 - fistCol.BoundingRadius / 2) && fistCol.Distance(target.ServerPosition) < (200 - fistCol.BoundingRadius / 2))
					{
						if (_q.IsReady() && _skoMenu.Item("UseRHarass").GetValue<bool>() && _r.IsReady())
						{
							_r.Cast();
						}

						_q.Cast(pred.CastPosition);
					}
				}else if(pred.Hitchance != HitChance.Impossible && pred.CollisionObjects.Count == 0)
				{
					if (_q.IsReady() && _skoMenu.Item("UseRHarass").GetValue<bool>() && _r.IsReady())
					{
						if(!_q.IsReady())return;
						_r.Cast();
					}
					_q.Cast(pred.CastPosition);
            }
            if (_skoMenu.Item("UseWHarass").GetValue<bool>())
            {
				if(!_w.IsReady() && _player.HasBuff("KarmaMantra"))
					return;
                if (_player.Distance(target) <= _w.Range)
                {
                    _w.Cast(target);
                }
            }

        }
        private static void KillSteal(Obj_AI_Hero target)
        {
            var igniteDmg = _player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            var qDmg = _player.GetSpellDamage(target, SpellSlot.Q);

            if (target.IsValidTarget())
            {
                if (_skoMenu.Item("UseIgnite").GetValue<bool>() && _igniteSlot != SpellSlot.Unknown &&
                _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                {
                    if (igniteDmg > target.Health)
                    {
                        _player.Spellbook.CastSpell(_igniteSlot, target);
                    }
                }

                if (_skoMenu.Item("UseQKs").GetValue<bool>() && _player.Distance(target) <= _q.Range)
                {
                    if (_q.IsReady() && target.Health < qDmg) 
                    {
                        _q.Cast();
                    }
                }
            }

        }
        private static void Farm()
        {
            var allminions = MinionManager.GetMinions(_player.ServerPosition, _q.Range);

            if (!_skoMenu.Item("UseQLane").GetValue<bool>()) return;
            foreach (var minion in allminions)
            {
                if (_q.IsReady() && minion.IsValidTarget() && _player.Distance(minion) <= _q.Range)
                {
                    _q.Cast(minion);
                }

            }
        }
			
        private static void OnDraw(EventArgs args)
        {
            if (_skoMenu.Item("CircleLag").GetValue<bool>())
            {
                if (_skoMenu.Item("DrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.White,
                        _skoMenu.Item("CircleThickness").GetValue<Slider>().Value,
                        _skoMenu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_skoMenu.Item("DrawW").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.White,
                        _skoMenu.Item("CircleThickness").GetValue<Slider>().Value,
                        _skoMenu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_skoMenu.Item("DrawE").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.White,
                        _skoMenu.Item("CircleThickness").GetValue<Slider>().Value,
                        _skoMenu.Item("CircleQuality").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (_skoMenu.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.White);
                }
                if (_skoMenu.Item("DrawW").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.White);
                }
                if (_skoMenu.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.White);
                }

            }
        }
    }
}
