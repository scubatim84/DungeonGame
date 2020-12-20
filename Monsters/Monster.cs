﻿using DungeonGame.Controllers;
using DungeonGame.Items;
using DungeonGame.Items.Equipment;
using DungeonGame.Players;
using DungeonGame.Quests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonGame.Monsters {
	public class Monster : IName {
		public enum MonsterType {
			Skeleton,
			Zombie,
			Spider,
			Demon,
			Elemental,
			Vampire,
			Troll,
			Dragon
		}
		public enum ElementalType {
			Fire,
			Air,
			Water
		}
		public enum SkeletonType {
			Warrior,
			Archer,
			Mage
		}
		public string _Name { get; set; }
		public string _Desc { get; set; }
		public int _Level { get; set; }
		public int _MaxHitPoints { get; set; }
		public int _HitPoints { get; set; }
		public int _MaxEnergyPoints { get; set; }
		public int _EnergyPoints { get; set; }
		public int _FireResistance { get; set; }
		public int _FrostResistance { get; set; }
		public int _ArcaneResistance { get; set; }
		public int _ExperienceProvided { get; set; }
		public int _Gold { get; set; }
		public bool _WasLooted { get; set; }
		public bool _InCombat { get; set; }
		public bool _IsStunned { get; set; }
		public int _StatReplenishInterval { get; set; }
		public MonsterType _MonsterCategory { get; set; }
		public ElementalType? _ElementalCategory { get; set; }
		public SkeletonType? _SkeletonCategory { get; set; }
		public int _UnarmedAttackDamage { get; set; }
		public Weapon _MonsterWeapon { get; set; }
		public Quiver _MonsterQuiver { get; set; }
		public Armor _MonsterHeadArmor { get; set; }
		public Armor _MonsterBackArmor { get; set; }
		public Armor _MonsterChestArmor { get; set; }
		public Armor _MonsterWristArmor { get; set; }
		public Armor _MonsterHandsArmor { get; set; }
		public Armor _MonsterWaistArmor { get; set; }
		public Armor _MonsterLegArmor { get; set; }
		public List<IItem> _MonsterItems { get; set; }
		public List<Effect> _Effects { get; set; }
		public List<MonsterSpell> _Spellbook { get; set; }
		public List<MonsterAbility> _Abilities { get; set; }

		// Default constructor for JSON deserialization
		public Monster() { }
		public Monster(int level, MonsterType monsterType) {
			_MonsterItems = new List<IItem>();
			_Effects = new List<Effect>();
			_StatReplenishInterval = 3;
			_UnarmedAttackDamage = 5;
			_Level = level;
			_FireResistance = _Level * 5;
			_FrostResistance = _Level * 5;
			_ArcaneResistance = _Level * 5;
			_MonsterCategory = monsterType;
			int randomNumHitPoint = GameController.GetRandomNumber(20, 40);
			int maxHitPoints = 80 + ((_Level - 1) * randomNumHitPoint);
			_MaxHitPoints = GameController.RoundNumber(maxHitPoints);
			_HitPoints = _MaxHitPoints;
			if (_MonsterCategory == MonsterType.Spider) {
				_Gold = 0;
			} else {
				int randomNumGold = GameController.GetRandomNumber(5, 10);
				_Gold = 10 + ((_Level - 1) * randomNumGold);
			}
			int expProvided = _MaxHitPoints;
			_ExperienceProvided = GameController.RoundNumber(expProvided);
			_MaxEnergyPoints = 100 + (_Level * 10);
			_EnergyPoints = _MaxEnergyPoints;
			switch (_MonsterCategory) {
				case MonsterType.Skeleton:
					int randomSkeletonType = GameController.GetRandomNumber(1, 3);
					_SkeletonCategory = randomSkeletonType switch {
						1 => SkeletonType.Archer,
						2 => SkeletonType.Warrior,
						3 => SkeletonType.Mage,
						_ => throw new ArgumentOutOfRangeException()
					};
					switch (_SkeletonCategory) {
						case SkeletonType.Warrior:
						case SkeletonType.Archer:
							break;
						case SkeletonType.Mage:
							_Spellbook = new List<MonsterSpell> {
								new MonsterSpell("frostbolt", 50, MonsterSpell.SpellType.Frostbolt, _Level),
								new MonsterSpell("fireball", 50, MonsterSpell.SpellType.Fireball, _Level)};
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
					break;
				case MonsterType.Zombie:
					break;
				case MonsterType.Spider:
					_Abilities = new List<MonsterAbility> {
						new MonsterAbility("poison bite", 50, MonsterAbility.Ability.PoisonBite, _Level)};
					break;
				case MonsterType.Demon:
					_Spellbook = new List<MonsterSpell> {
						new MonsterSpell("fireball", 50, MonsterSpell.SpellType.Fireball, _Level)};
					break;
				case MonsterType.Elemental:
					int randomNum = GameController.GetRandomNumber(1, 3);
					int randomPhysicalDmg = GameController.GetRandomNumber(20, 26);
					_UnarmedAttackDamage = randomNum switch {
						1 => randomPhysicalDmg + ((level - 1) * 1),
						2 => randomPhysicalDmg + ((level - 1) * 2),
						3 => randomPhysicalDmg + ((level - 1) * 3),
						_ => throw new ArgumentOutOfRangeException()
					};
					int randomElementalType = GameController.GetRandomNumber(1, 3);
					_ElementalCategory = randomElementalType switch {
						1 => ElementalType.Air,
						2 => ElementalType.Fire,
						3 => ElementalType.Water,
						_ => throw new ArgumentOutOfRangeException()
					};
					_Spellbook = _ElementalCategory switch {
						ElementalType.Fire => new List<MonsterSpell> {
								new MonsterSpell("fireball", 50, MonsterSpell.SpellType.Fireball, _Level)},
						ElementalType.Air => new List<MonsterSpell> {
								new MonsterSpell("lightning", 50, MonsterSpell.SpellType.Lightning, _Level)},
						ElementalType.Water => new List<MonsterSpell> {
								new MonsterSpell("frostbolt", 50, MonsterSpell.SpellType.Frostbolt, _Level)},
						_ => throw new ArgumentOutOfRangeException(),
					};
					break;
				case MonsterType.Vampire:
					_Abilities = new List<MonsterAbility> {
						new MonsterAbility("blood leech", 50, MonsterAbility.Ability.BloodLeech, _Level)};
					break;
				case MonsterType.Troll:
					break;
				case MonsterType.Dragon:
					_Abilities = new List<MonsterAbility> {
						new MonsterAbility("tail whip", 50, MonsterAbility.Ability.TailWhip, _Level)};
					_Spellbook = new List<MonsterSpell> {
						new MonsterSpell("fire breath", 50, MonsterSpell.SpellType.Fireball, _Level)};
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public AttackOption DetermineAttack(Player player, bool addRandomChance) {
			List<AttackOption> attackOptions = new List<AttackOption>();
			if (_MonsterWeapon != null && _MonsterWeapon._Equipped) {
				attackOptions.Add(new
					AttackOption(AttackOption.AttackType.Physical,
						_MonsterWeapon._RegDamage - player.ArmorRating(this), -1));
			} else {
				attackOptions.Add(new
					AttackOption(AttackOption.AttackType.Physical, _UnarmedAttackDamage, -1));
			}
			if (_Spellbook != null) {
				for (int i = 0; i < _Spellbook.Count; i++) {
					if (_EnergyPoints < _Spellbook[i]._EnergyCost) {
						continue;
					}

					switch (_Spellbook[i]._SpellCategory) {
						case MonsterSpell.SpellType.Fireball:
						case MonsterSpell.SpellType.Frostbolt:
						case MonsterSpell.SpellType.Lightning:
							int spellTotalDamage = 0;
							if (_Spellbook[i]._Offensive._AmountOverTime == 0) {
								spellTotalDamage = _Spellbook[i]._Offensive._Amount;
							} else {
								spellTotalDamage = _Spellbook[i]._Offensive._Amount + (_Spellbook[i]._Offensive._AmountOverTime *
									_Spellbook[i]._Offensive._AmountMaxRounds);
							}
							attackOptions.Add(new
								AttackOption(AttackOption.AttackType.Spell, spellTotalDamage, i));
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
			if (_Abilities != null) {
				for (int i = 0; i < _Abilities.Count; i++) {
					if (_EnergyPoints < _Abilities[i]._EnergyCost) {
						continue;
					}

					switch (_Abilities[i]._AbilityCategory) {
						case MonsterAbility.Ability.PoisonBite:
						case MonsterAbility.Ability.BloodLeech:
						case MonsterAbility.Ability.TailWhip:
							int abilityTotalDamage = 0;
							if (_Abilities[i]._Offensive._AmountOverTime == 0) {
								abilityTotalDamage = _Abilities[i]._Offensive._Amount * 2;
							} else {
								abilityTotalDamage = _Abilities[i]._Offensive._Amount + (_Abilities[i]._Offensive._AmountOverTime *
									_Abilities[i]._Offensive._AmountMaxRounds);
							}
							attackOptions.Add(new
								AttackOption(AttackOption.AttackType.Ability, abilityTotalDamage, i));
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}

			// Sort and order attack options so that the one which causes the most damage is at the top
			attackOptions = attackOptions.OrderByDescending(attack => attack._DamageAmount).ToList();

			if (addRandomChance) {
				int randomMonsterAttack = GameController.GetRandomNumber(1, 10);

				// If an element of random chance is required, provide a 50% chance of an attack being selected at random
				if (randomMonsterAttack > 5) {
					int randomAttackChoice = GameController.GetRandomNumber(0, attackOptions.Count - 1);

					return attackOptions[randomAttackChoice];
				}
			}

			// If an element of random chance is not selected, the attack option which causes the most damage is returned
			return attackOptions[0];
		}
		public void Attack(Player player) {
			AttackOption attackOption = DetermineAttack(player, true);
			switch (attackOption._AttackCategory) {
				case AttackOption.AttackType.Physical:
					PhysicalAttack(player);
					break;
				case AttackOption.AttackType.Spell:
					switch (_Spellbook[attackOption._AttackIndex]._SpellCategory) {
						case MonsterSpell.SpellType.Fireball:
							MonsterSpell.CastFireOffense(this, player, attackOption._AttackIndex);
							break;
						case MonsterSpell.SpellType.Frostbolt:
							MonsterSpell.CastFrostOffense(this, player, attackOption._AttackIndex);
							break;
						case MonsterSpell.SpellType.Lightning:
							MonsterSpell.CastArcaneOffense(this, player, attackOption._AttackIndex);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
					break;
				case AttackOption.AttackType.Ability:
					switch (_Abilities[attackOption._AttackIndex]._AbilityCategory) {
						case MonsterAbility.Ability.PoisonBite:
						case MonsterAbility.Ability.TailWhip:
							MonsterAbility.UseOffenseDamageAbility(this, player, attackOption._AttackIndex);
							break;
						case MonsterAbility.Ability.BloodLeech:
							MonsterAbility.UseBloodLeechAbility(this, player, attackOption._AttackIndex);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			GearController.DecreaseArmorDurability(player);
		}
		private void PhysicalAttack(Player player) {
			int attackAmount = 0;
			try {
				if (_MonsterWeapon._Equipped && _MonsterWeapon._WeaponGroup != Weapon.WeaponType.Bow) {
					attackAmount += _MonsterWeapon.Attack();
				}
				if (_MonsterWeapon._Equipped &&
					_MonsterWeapon._WeaponGroup == Weapon.WeaponType.Bow &&
					_MonsterQuiver.HaveArrows()) {
					_MonsterQuiver.UseArrow();
					attackAmount += _MonsterWeapon.Attack();
				}
				if (_MonsterWeapon._Equipped &&
					_MonsterWeapon._WeaponGroup == Weapon.WeaponType.Bow &&
					!_MonsterQuiver.HaveArrows()) {
					attackAmount += _UnarmedAttackDamage;
				}
			} catch (NullReferenceException) {
				if (_MonsterCategory == MonsterType.Elemental) {
					attackAmount += _UnarmedAttackDamage;
				} else {
					string monsterDisarmed = $"The {_Name} is disarmed! They are going hand to hand!";
					OutputController.Display.StoreUserOutput(
						Settings.FormatFailureOutputText(),
						Settings.FormatDefaultBackground(),
						monsterDisarmed);
					attackAmount += _UnarmedAttackDamage;
				}
			}
			int randomChanceToHit = GameController.GetRandomNumber(1, 100);
			double chanceToDodge = player._DodgeChance;
			if (chanceToDodge > 50) {
				chanceToDodge = 50;
			}

			if (randomChanceToHit <= chanceToDodge) {
				string missString = $"The {_Name} missed you!";
				OutputController.Display.StoreUserOutput(
					Settings.FormatAttackFailText(),
					Settings.FormatDefaultBackground(),
					missString);
				return;
			}
			int baseAttackAmount = attackAmount;
			foreach (Effect effect in player._Effects.ToList()) {
				switch (effect._EffectGroup) {
					case Effect.EffectType.Healing:
						break;
					case Effect.EffectType.ChangePlayerDamage:
						break;
					case Effect.EffectType.ChangeArmor:
						break;
					case Effect.EffectType.OnFire:
						break;
					case Effect.EffectType.Bleeding:
						break;
					case Effect.EffectType.Stunned:
						break;
					case Effect.EffectType.Frozen:
						double frozenAttackAmount = attackAmount * effect._EffectMultiplier;
						attackAmount = (int)frozenAttackAmount;
						effect.FrozenRound(player);
						effect._IsEffectExpired = true;
						break;
					case Effect.EffectType.ChangeOpponentDamage:
						int changeDamageAmount = effect._EffectAmountOverTime < attackAmount ?
							effect._EffectAmountOverTime : attackAmount;
						effect.ChangeOpponentDamageRound(player);
						attackAmount += changeDamageAmount;
						break;
					case Effect.EffectType.BlockDamage:
						int blockAmount = effect._EffectAmount < attackAmount ?
							effect._EffectAmount : attackAmount;
						effect.BlockDamageRound(blockAmount);
						attackAmount -= blockAmount;
						break;
					case Effect.EffectType.ReflectDamage:
						int reflectAmount = effect._EffectAmountOverTime < attackAmount ?
							effect._EffectAmountOverTime : attackAmount;
						_HitPoints -= reflectAmount;
						effect.ReflectDamageRound(reflectAmount);
						attackAmount -= reflectAmount;
						break;
					case Effect.EffectType.ChangeStat:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				if (baseAttackAmount > attackAmount && attackAmount - player.ArmorRating(this) <= 0) {
					string effectAbsorbString = $"Your {effect._Name} absorbed all of {_Name}'s attack!";
					OutputController.Display.StoreUserOutput(
						Settings.FormatAttackFailText(),
						Settings.FormatDefaultBackground(),
						effectAbsorbString);
					return;
				}
				GameController.RemovedExpiredEffectsAsync(this);
			}
			if (attackAmount - player.ArmorRating(this) <= 0) {
				string armorAbsorbString = $"Your armor absorbed all of {_Name}'s attack!";
				OutputController.Display.StoreUserOutput(
					Settings.FormatAttackFailText(),
					Settings.FormatDefaultBackground(),
					armorAbsorbString);
			} else if (attackAmount - player.ArmorRating(this) > 0) {
				attackAmount -= player.ArmorRating(this);
				string hitString = $"The {_Name} hits you for {attackAmount} physical damage.";
				OutputController.Display.StoreUserOutput(
					Settings.FormatAttackSuccessText(),
					Settings.FormatDefaultBackground(),
					hitString);
				player._HitPoints -= attackAmount;
			}
		}
		public int ArmorRating(Player player) {
			int totalArmorRating = MonsterController.CheckArmorRating(this);
			int levelDiff = player._Level - _Level;
			double armorMultiplier = 1.00 + (-(double)levelDiff / 5);
			double adjArmorRating = totalArmorRating * armorMultiplier;
			if (_ElementalCategory != null) {
				adjArmorRating += 20;
			}

			return (int)adjArmorRating;
		}
		public void MonsterDeath(Player player) {
			player._InCombat = false;
			_InCombat = false;
			_Effects.Clear();
			string defeatString = $"You have defeated the {_Name}!";
			OutputController.Display.StoreUserOutput(
				Settings.FormatSuccessOutputText(),
				Settings.FormatDefaultBackground(),
				defeatString);
			string expGainString = $"You have gained {_ExperienceProvided} experience!";
			OutputController.Display.StoreUserOutput(
				Settings.FormatSuccessOutputText(),
				Settings.FormatDefaultBackground(),
				expGainString);
			foreach (IItem loot in _MonsterItems.Where(item => item is IEquipment)) {
				IEquipment equippableItem = loot as IEquipment;
				equippableItem._Equipped = false;
			}
			_Name = $"Dead {_Name}";
			_Desc = "A corpse of a monster you killed.";
			player._Experience += _ExperienceProvided;
			PlayerController.LevelUpCheck(player);
			if (player._QuestLog == null) {
				return;
			}

			foreach (Quest quest in player._QuestLog) {
				quest.UpdateQuestProgress(this);
			}
		}
	}
}