﻿using DungeonGame;
using DungeonGame.Controllers;
using DungeonGame.Items;
using DungeonGame.Items.Equipment;
using DungeonGame.Monsters;
using DungeonGame.Players;
using NUnit.Framework;
using System.Linq;
using System.Threading;

namespace DungeonGameTests {
	public class WarAbilityUnitTests {
		[Test]
		public void SlashAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Warrior) { _MaxRagePoints = 100, _RagePoints = 100 };
			OutputController.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { _HitPoints = 100, _MaxHitPoints = 100 };
			MonsterBuilder.BuildMonster(monster);
			foreach (IItem item in monster._MonsterItems.Where(item => item is IEquipment eItem && eItem._Equipped)) {
				IEquipment eItem = item as IEquipment;
				eItem._Equipped = false;
			}
			string[] inputInfo = new[] { "ability", "slash" };
			int abilityIndex = player._Abilities.FindIndex(
				f => f._WarAbilityCategory == PlayerAbility.WarriorAbility.Slash);
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Slash", OutputController.Display._Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display._Output[1][2]);
			Assert.AreEqual("Rage Cost: 40", OutputController.Display._Output[2][2]);
			Assert.AreEqual("Instant Damage: 50", OutputController.Display._Output[3][2]);
			string[] input = new[] { "use", "slash" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("slash", abilityName);
			player.UseAbility(monster, input);
			Assert.AreEqual(player._MaxRagePoints - player._Abilities[abilityIndex]._RageCost,
				player._RagePoints);
			int abilityDamage = player._Abilities[abilityIndex]._Offensive._Amount;
			Assert.AreEqual(monster._HitPoints, monster._MaxHitPoints - abilityDamage);
			string abilitySuccessString = $"Your {player._Abilities[abilityIndex]._Name} hit the {monster._Name} for {abilityDamage} physical damage.";
			Assert.AreEqual(abilitySuccessString, OutputController.Display._Output[4][2]);
		}
		[Test]
		public void RendAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Warrior) { _MaxRagePoints = 100, _RagePoints = 100 };
			OutputController.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { _HitPoints = 100, _MaxHitPoints = 100 };
			MonsterBuilder.BuildMonster(monster);
			foreach (IItem item in monster._MonsterItems.Where(item => item is IEquipment eItem && eItem._Equipped)) {
				IEquipment eItem = item as IEquipment;
				eItem._Equipped = false;
			}
			int abilityIndex = player._Abilities.FindIndex(
				f => f._WarAbilityCategory == PlayerAbility.WarriorAbility.Rend);
			string[] inputInfo = new[] { "ability", "rend" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Rend", OutputController.Display._Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display._Output[1][2]);
			Assert.AreEqual("Rage Cost: 25", OutputController.Display._Output[2][2]);
			Assert.AreEqual("Instant Damage: 15", OutputController.Display._Output[3][2]);
			Assert.AreEqual("Damage Over Time: 5", OutputController.Display._Output[4][2]);
			string bleedOverTimeString = $"Bleeding damage over time for {player._Abilities[abilityIndex]._Offensive._AmountMaxRounds} rounds.";
			Assert.AreEqual(bleedOverTimeString, OutputController.Display._Output[5][2]);
			string[] input = new[] { "use", "rend" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("rend", abilityName);
			player.UseAbility(monster, input);
			int? rageCost = player._Abilities[abilityIndex]._RageCost;
			Assert.AreEqual(player._MaxRagePoints - rageCost, player._RagePoints);
			int abilityDamage = player._Abilities[abilityIndex]._Offensive._Amount;
			int abilityDamageOverTime = player._Abilities[abilityIndex]._Offensive._AmountOverTime;
			int abilityCurRounds = player._Abilities[abilityIndex]._Offensive._AmountCurRounds;
			int abilityMaxRounds = player._Abilities[abilityIndex]._Offensive._AmountMaxRounds;
			string abilitySuccessString = $"Your {player._Abilities[abilityIndex]._Name} hit the {monster._Name} for {abilityDamage} physical damage.";
			Assert.AreEqual(abilitySuccessString, OutputController.Display._Output[6][2]);
			string bleedString = $"The {monster._Name} is bleeding!";
			Assert.AreEqual(bleedString, OutputController.Display._Output[7][2]);
			Assert.AreEqual(
				true, monster._Effects[0]._EffectGroup == Effect.EffectType.Bleeding);
			Assert.AreEqual(monster._MaxHitPoints - abilityDamage, monster._HitPoints);
			Assert.AreEqual(abilityCurRounds, monster._Effects[0]._EffectCurRound);
			Assert.AreEqual(abilityMaxRounds, monster._Effects[0]._EffectMaxRound);
			OutputController.Display.ClearUserOutput();
			for (int i = 2; i < 5; i++) {
				monster._Effects[0].BleedingRound(monster);
				int bleedAmount = monster._Effects[0]._EffectAmountOverTime;
				string bleedRoundString = $"The {monster._Name} bleeds for {bleedAmount} physical damage.";
				Assert.AreEqual(bleedRoundString, OutputController.Display._Output[i - 2][2]);
				Assert.AreEqual(i, monster._Effects[0]._EffectCurRound);
				GameController.RemovedExpiredEffectsAsync(monster);
				Thread.Sleep(1000);
			}
			Assert.AreEqual(false, monster._Effects.Any());
			Assert.AreEqual(monster._MaxHitPoints - abilityDamage - abilityDamageOverTime * abilityMaxRounds,
				monster._HitPoints);
		}
		[Test]
		public void ChargeAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Warrior) {
				_MaxRagePoints = 100,
				_RagePoints = 100,
				_InCombat = true
			};
			OutputController.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { _HitPoints = 100, _MaxHitPoints = 100, _InCombat = true };
			MonsterBuilder.BuildMonster(monster);
			foreach (IItem item in monster._MonsterItems.Where(item => item is IEquipment eItem && eItem._Equipped)) {
				IEquipment eItem = item as IEquipment;
				eItem._Equipped = false;
			}
			int abilityIndex = player._Abilities.FindIndex(
				f => f._WarAbilityCategory == PlayerAbility.WarriorAbility.Charge);
			string[] inputInfo = new[] { "ability", "charge" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Charge", OutputController.Display._Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display._Output[1][2]);
			Assert.AreEqual("Rage Cost: 25", OutputController.Display._Output[2][2]);
			Assert.AreEqual("Instant Damage: 15", OutputController.Display._Output[3][2]);
			string abilityInfoString = $"Stuns opponent for {player._Abilities[abilityIndex]._Stun._StunMaxRounds} rounds, preventing their attacks.";
			Assert.AreEqual(abilityInfoString, OutputController.Display._Output[4][2]);
			string[] input = new[] { "use", "charge" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("charge", abilityName);
			player.UseAbility(monster, input);
			int? rageCost = player._Abilities[abilityIndex]._RageCost;
			Assert.AreEqual(player._MaxRagePoints - rageCost, player._RagePoints);
			int abilityDamage = player._Abilities[abilityIndex]._Stun._DamageAmount;
			int abilityCurRounds = player._Abilities[abilityIndex]._Stun._StunCurRounds;
			int abilityMaxRounds = player._Abilities[abilityIndex]._Stun._StunMaxRounds;
			string attackSuccessString = $"You {player._Abilities[abilityIndex]._Name} the {monster._Name} for {abilityDamage} physical damage.";
			Assert.AreEqual(attackSuccessString, OutputController.Display._Output[5][2]);
			string stunString = $"The {monster._Name} is stunned!";
			Assert.AreEqual(stunString, OutputController.Display._Output[6][2]);
			Assert.AreEqual(monster._MaxHitPoints - abilityDamage, monster._HitPoints);
			Assert.AreEqual(
				true, monster._Effects[0]._EffectGroup == Effect.EffectType.Stunned);
			Assert.AreEqual(abilityCurRounds, monster._Effects[0]._EffectCurRound);
			Assert.AreEqual(abilityMaxRounds, monster._Effects[0]._EffectMaxRound);
			OutputController.Display.ClearUserOutput();
			for (int i = 2; i < 4; i++) {
				monster._Effects[0].StunnedRound(monster);
				string stunnedString = $"The {monster._Name} is stunned and cannot attack.";
				Assert.AreEqual(stunnedString, OutputController.Display._Output[i - 2][2]);
				Assert.AreEqual(i, monster._Effects[0]._EffectCurRound);
				GameController.RemovedExpiredEffectsAsync(monster);
				Thread.Sleep(1000);
			}
			Assert.AreEqual(false, monster._Effects.Any());
		}
		[Test]
		public void BlockAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Warrior) {
				_MaxRagePoints = 100,
				_RagePoints = 100,
				_InCombat = true,
				_MaxHitPoints = 100,
				_HitPoints = 100,
				_DodgeChance = 0,
				_Level = 3
			};
			OutputController.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Zombie) { _HitPoints = 100, _MaxHitPoints = 100, _InCombat = true };
			MonsterBuilder.BuildMonster(monster);
			monster._MonsterWeapon._CritMultiplier = 1;
			int abilityIndex = player._Abilities.FindIndex(
				f => f._WarAbilityCategory == PlayerAbility.WarriorAbility.Block);
			string[] inputInfo = new[] { "ability", "block" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Block", OutputController.Display._Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display._Output[1][2]);
			Assert.AreEqual("Rage Cost: 25", OutputController.Display._Output[2][2]);
			Assert.AreEqual("Block Damage: 50", OutputController.Display._Output[3][2]);
			const string blockInfoString =
				"Block damage will prevent incoming damage from opponent until block damage is used up.";
			Assert.AreEqual(blockInfoString, OutputController.Display._Output[4][2]);
			string[] input = new[] { "use", "block" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("block", abilityName);
			player.UseAbility(monster, input);
			int? rageCost = player._Abilities[abilityIndex]._RageCost;
			Assert.AreEqual(player._MaxRagePoints - rageCost, player._RagePoints);
			int blockAmount = player._Abilities[abilityIndex]._Defensive._BlockDamage;
			string blockString = $"You start blocking your opponent's attacks! You will block {blockAmount} damage.";
			Assert.AreEqual(blockString, OutputController.Display._Output[5][2]);
			OutputController.Display.ClearUserOutput();
			Assert.AreEqual(
				true, player._Effects[0]._EffectGroup == Effect.EffectType.BlockDamage);
			Assert.AreEqual(player._MaxHitPoints, player._HitPoints);
			int blockAmountRemaining = player._Effects[0]._EffectAmount;
			Assert.AreEqual(blockAmount, blockAmountRemaining);
			int i = 0;
			while (blockAmountRemaining > 0) {
				monster._MonsterWeapon._Durability = 100;
				int blockAmountBefore = blockAmountRemaining;
				monster.Attack(player);
				blockAmountRemaining = player._Effects.Any() ? player._Effects[0]._EffectAmount : 0;
				if (blockAmountRemaining > 0) {
					Assert.AreEqual(player._MaxHitPoints, player._HitPoints);
					string blockRoundString = $"Your defensive move blocked {(blockAmountBefore - blockAmountRemaining)} damage!";
					Assert.AreEqual(blockRoundString, OutputController.Display._Output[i + (i * 1)][2]);
				} else {
					GameController.RemovedExpiredEffectsAsync(player);
					int attackAmount = monster._MonsterWeapon.Attack() - blockAmountBefore;
					Assert.AreEqual(player._MaxHitPoints - attackAmount, player._HitPoints);
					const string blockEndString = "You are no longer blocking damage!";
					Assert.AreEqual(blockEndString, OutputController.Display._Output[i + 3][2]);
					Thread.Sleep(1000);
					Assert.AreEqual(false, player._Effects.Any());
					string hitString = $"The {monster._Name} hits you for {attackAmount} physical damage.";
					Assert.AreEqual(hitString, OutputController.Display._Output[i + 4][2]);
				}
				i++;
			}
		}
		[Test]
		public void BerserkAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Warrior) {
				_MaxRagePoints = 100,
				_RagePoints = 100,
				_InCombat = true,
				_MaxHitPoints = 100,
				_HitPoints = 100,
				_DodgeChance = 0
			};
			GearController.EquipInitialGear(player);
			OutputController.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { _HitPoints = 100, _MaxHitPoints = 100, _InCombat = true };
			MonsterBuilder.BuildMonster(monster);
			foreach (IItem item in monster._MonsterItems.Where(item => item is IEquipment eItem && eItem._Equipped)) {
				IEquipment eItem = item as IEquipment;
				eItem._Equipped = false;
			}
			int abilityIndex = player._Abilities.FindIndex(
				f => f._WarAbilityCategory == PlayerAbility.WarriorAbility.Berserk);
			string[] inputInfo = new[] { "ability", "berserk" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Berserk", OutputController.Display._Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display._Output[1][2]);
			Assert.AreEqual("Rage Cost: 40", OutputController.Display._Output[2][2]);
			string dmgIncreaseString = $"Damage Increase: {player._Abilities[abilityIndex]._Offensive._Amount}";
			Assert.AreEqual(dmgIncreaseString, OutputController.Display._Output[3][2]);
			string armDecreaseString = $"Armor Decrease: {player._Abilities[abilityIndex]._ChangeAmount._Amount}";
			Assert.AreEqual(armDecreaseString, OutputController.Display._Output[4][2]);
			string dmgInfoString = $"Damage increased at cost of armor decrease for {player._Abilities[abilityIndex]._ChangeAmount._ChangeMaxRound} rounds";
			Assert.AreEqual(dmgInfoString, OutputController.Display._Output[5][2]);
			string[] input = new[] { "use", "berserk" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("berserk", abilityName);
			int baseArmorRating = GearController.CheckArmorRating(player);
			int baseDamage = player.PhysicalAttack(monster);
			player.UseAbility(monster, input);
			int? rageCost = player._Abilities[abilityIndex]._RageCost;
			Assert.AreEqual(player._MaxRagePoints - rageCost, player._RagePoints);
			Assert.AreEqual(2, player._Effects.Count);
			Assert.AreEqual(Effect.EffectType.ChangePlayerDamage, player._Effects[0]._EffectGroup);
			Assert.AreEqual(Effect.EffectType.ChangeArmor, player._Effects[1]._EffectGroup);
			const string berserkString = "You go into a berserk rage!";
			Assert.AreEqual(berserkString, OutputController.Display._Output[6][2]);
			for (int i = 2; i < 6; i++) {
				OutputController.Display.ClearUserOutput();
				int berserkArmorAmount = player._Abilities[abilityIndex]._ChangeAmount._Amount;
				int berserkDamageAmount = player._Abilities[abilityIndex]._Offensive._Amount;
				int berserkArmorRating = GearController.CheckArmorRating(player);
				int berserkDamage = player.PhysicalAttack(monster);
				Assert.AreEqual(berserkArmorRating, baseArmorRating + berserkArmorAmount);
				Assert.AreEqual(berserkDamage, baseDamage + berserkDamageAmount, 5);
				player._Effects[0].ChangePlayerDamageRound(player);
				string changeDmgString = $"Your damage is increased by {berserkDamageAmount}.";
				Assert.AreEqual(changeDmgString, OutputController.Display._Output[0][2]);
				player._Effects[1].ChangeArmorRound();
				string changeArmorString = $"Your armor is decreased by {berserkArmorAmount * -1}.";
				Assert.AreEqual(changeArmorString, OutputController.Display._Output[1][2]);
				GameController.RemovedExpiredEffectsAsync(player);
				Thread.Sleep(1000);
			}
			Assert.AreEqual(false, player._Effects.Any());
		}
		[Test]
		public void DisarmAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Warrior) {
				_MaxRagePoints = 100,
				_RagePoints = 100,
				_MaxHitPoints = 100,
				_HitPoints = 100
			};
			GearController.EquipInitialGear(player);
			OutputController.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { _HitPoints = 100, _MaxHitPoints = 100 };
			MonsterBuilder.BuildMonster(monster);
			int abilityIndex = player._Abilities.FindIndex(
				f => f._WarAbilityCategory == PlayerAbility.WarriorAbility.Disarm);
			string[] inputInfo = new[] { "ability", "disarm" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Disarm", OutputController.Display._Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display._Output[1][2]);
			Assert.AreEqual("Rage Cost: 25", OutputController.Display._Output[2][2]);
			string abilityString = $"{player._Abilities[abilityIndex]._Offensive._Amount}% chance to disarm opponent's weapon.";
			Assert.AreEqual(abilityString, OutputController.Display._Output[3][2]);
			player._Abilities[abilityIndex]._Offensive._Amount = 0; // Set disarm success chance to 0% for test
			string[] input = new[] { "use", "disarm" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("disarm", abilityName);
			player.UseAbility(monster, input);
			int? rageCost = player._Abilities[abilityIndex]._RageCost;
			Assert.AreEqual(player._MaxRagePoints - rageCost, player._RagePoints);
			Assert.AreEqual(true, monster._MonsterWeapon._Equipped);
			string disarmFailString = $"You tried to disarm {monster._Name} but failed!";
			Assert.AreEqual(disarmFailString, OutputController.Display._Output[4][2]);
			player._Abilities[abilityIndex]._Offensive._Amount = 100; // Set disarm success chance to 100% for test
			player.UseAbility(monster, input);
			Assert.AreEqual(player._MaxRagePoints - (rageCost * 2), player._RagePoints);
			Assert.AreEqual(false, monster._MonsterWeapon._Equipped);
			string disarmSuccessString = $"You successfully disarmed {monster._Name}!";
			Assert.AreEqual(disarmSuccessString, OutputController.Display._Output[5][2]);
		}
		[Test]
		public void BandageAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Warrior) {
				_MaxRagePoints = 100,
				_RagePoints = 100,
				_MaxHitPoints = 100,
				_HitPoints = 10
			};
			GearController.EquipInitialGear(player);
			player._Abilities.Add(new PlayerAbility(
				"bandage", 25, 1, PlayerAbility.WarriorAbility.Bandage, 2));
			OutputController.Display.ClearUserOutput();
			int abilityIndex = player._Abilities.FindIndex(
				f => f._WarAbilityCategory == PlayerAbility.WarriorAbility.Bandage);
			string[] inputInfo = new[] { "ability", "bandage" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Bandage", OutputController.Display._Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display._Output[1][2]);
			Assert.AreEqual("Rage Cost: 25", OutputController.Display._Output[2][2]);
			Assert.AreEqual("Heal Amount: 25", OutputController.Display._Output[3][2]);
			Assert.AreEqual("Heal Over Time: 5", OutputController.Display._Output[4][2]);
			string healInfoStringCombat = $"Heal over time will restore health for {player._Abilities[abilityIndex]._Healing._HealMaxRounds} rounds in combat.";
			Assert.AreEqual(healInfoStringCombat, OutputController.Display._Output[5][2]);
			string healInfoStringNonCombat = $"Heal over time will restore health {player._Abilities[abilityIndex]._Healing._HealMaxRounds} times every 10 seconds.";
			Assert.AreEqual(healInfoStringNonCombat, OutputController.Display._Output[6][2]);
			string[] input = new[] { "use", "bandage" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("bandage", abilityName);
			int baseHitPoints = player._HitPoints;
			player.UseAbility(input);
			int? rageCost = player._Abilities[abilityIndex]._RageCost;
			Assert.AreEqual(player._MaxRagePoints - rageCost, player._RagePoints);
			int healAmount = player._Abilities[abilityIndex]._Healing._HealAmount;
			string healString = $"You heal yourself for {healAmount} health.";
			Assert.AreEqual(healString, OutputController.Display._Output[7][2]);
			Assert.AreEqual(Effect.EffectType.Healing, player._Effects[0]._EffectGroup);
			Assert.AreEqual(baseHitPoints + healAmount, player._HitPoints);
			baseHitPoints = player._HitPoints;
			OutputController.Display.ClearUserOutput();
			for (int i = 2; i < 5; i++) {
				player._Effects[0].HealingRound(player);
				int healOverTimeAmt = player._Effects[0]._EffectAmountOverTime;
				string healAmtString = $"You have been healed for {healOverTimeAmt} health.";
				Assert.AreEqual(i, player._Effects[0]._EffectCurRound);
				Assert.AreEqual(healAmtString, OutputController.Display._Output[i - 2][2]);
				Assert.AreEqual(baseHitPoints + ((i - 1) * healOverTimeAmt), player._HitPoints);
			}
			GameController.RemovedExpiredEffectsAsync(player);
			Thread.Sleep(1000);
			Assert.AreEqual(false, player._Effects.Any());
		}
		[Test]
		public void PowerAuraAbilityUnitTest() {
			OutputController.Display.ClearUserOutput();
			Player player = new Player("test", Player.PlayerClassType.Warrior) { _MaxRagePoints = 150, _RagePoints = 150 };
			player._Abilities.Add(new PlayerAbility(
				"power aura", 150, 1, PlayerAbility.WarriorAbility.PowerAura, 6));
			OutputController.Display.ClearUserOutput();
			int abilityIndex = player._Abilities.FindIndex(
				f => f._WarAbilityCategory == PlayerAbility.WarriorAbility.PowerAura);
			string[] inputInfo = new[] { "ability", "power", "aura" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Power Aura", OutputController.Display._Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display._Output[1][2]);
			Assert.AreEqual("Rage Cost: 150", OutputController.Display._Output[2][2]);
			Assert.AreEqual("Power Aura Amount: 15", OutputController.Display._Output[3][2]);
			Assert.AreEqual("Strength is increased by 15 for 10 minutes.", OutputController.Display._Output[4][2]);
			string[] input = new[] { "use", "power", "aura" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("power aura", abilityName);
			int baseStr = player._Strength;
			int? baseRage = player._RagePoints;
			int? baseMaxRage = player._MaxRagePoints;
			player.UseAbility(input);
			int? rageCost = player._Abilities[abilityIndex]._RageCost;
			Assert.AreEqual(baseMaxRage - rageCost, player._RagePoints);
			Assert.AreEqual("You generate a Power Aura around yourself.", OutputController.Display._Output[5][2]);
			OutputController.Display.ClearUserOutput();
			Assert.AreEqual(
				baseRage - player._Abilities[abilityIndex]._RageCost, player._RagePoints);
			Assert.AreEqual(player._Strength, baseStr + player._Abilities[abilityIndex]._ChangeAmount._Amount);
			Assert.AreEqual(
				player._MaxRagePoints, baseMaxRage + player._Abilities[abilityIndex]._ChangeAmount._Amount * 10);
			Assert.AreEqual(Effect.EffectType.ChangeStat, player._Effects[0]._EffectGroup);
			for (int i = 0; i < 600; i++) {
				player._Effects[0].ChangeStatRound();
			}
			GameController.RemovedExpiredEffectsAsync(player);
			Thread.Sleep(1000);
			Assert.AreEqual(false, player._Effects.Any());
			Assert.AreEqual(baseStr, player._Strength);
			Assert.AreEqual(baseMaxRage, player._MaxRagePoints);
			Assert.AreEqual(player._MaxRagePoints - rageCost, player._RagePoints);
		}
		[Test]
		public void WarCryAbilityUnitTest() {
			OutputController.Display.ClearUserOutput();
			Player player = new Player("test", Player.PlayerClassType.Warrior) {
				_MaxRagePoints = 100,
				_RagePoints = 100,
				_InCombat = true
			};
			player._Abilities.Add(new PlayerAbility(
				"war cry", 50, 1, PlayerAbility.WarriorAbility.WarCry, 4));
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { _HitPoints = 100, _MaxHitPoints = 100 };
			MonsterBuilder.BuildMonster(monster);
			int abilityIndex = player._Abilities.FindIndex(
				f => f._WarAbilityCategory == PlayerAbility.WarriorAbility.WarCry);
			string[] inputInfo = new[] { "ability", "war", "cry" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("War Cry", OutputController.Display._Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display._Output[1][2]);
			Assert.AreEqual("Rage Cost: 50", OutputController.Display._Output[2][2]);
			Assert.AreEqual("War Cry Amount: 25", OutputController.Display._Output[3][2]);
			Assert.AreEqual("Opponent's attacks are decreased by 25 for 3 rounds.",
				OutputController.Display._Output[4][2]);
			string[] input = new[] { "use", "war", "cry" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("war cry", abilityName);
			player.UseAbility(input);
			int? rageCost = player._Abilities[abilityIndex]._RageCost;
			Assert.AreEqual(player._MaxRagePoints - rageCost, player._RagePoints);
			Assert.AreEqual("You shout a War Cry, intimidating your opponent, and decreasing incoming damage.",
				OutputController.Display._Output[5][2]);
			OutputController.Display.ClearUserOutput();
			Assert.AreEqual(Effect.EffectType.ChangeOpponentDamage, player._Effects[0]._EffectGroup);
			for (int i = 2; i < 5; i++) {
				int baseAttackDamageM = monster._UnarmedAttackDamage;
				int attackDamageM = baseAttackDamageM;
				int changeDamageAmount = player._Effects[0]._EffectAmountOverTime < attackDamageM ?
					player._Effects[0]._EffectAmountOverTime : attackDamageM;
				attackDamageM -= changeDamageAmount;
				Assert.AreEqual(baseAttackDamageM - changeDamageAmount, attackDamageM);
				player._Effects[0].ChangeOpponentDamageRound(player);
				string changeDmgString = $"Incoming damage is decreased by {-1 * player._Effects[0]._EffectAmountOverTime}.";
				Assert.AreEqual(i, player._Effects[0]._EffectCurRound);
				Assert.AreEqual(changeDmgString, OutputController.Display._Output[i - 2][2]);
			}
			GameController.RemovedExpiredEffectsAsync(player);
			Thread.Sleep(1000);
			Assert.AreEqual(false, player._Effects.Any());
		}
		[Test]
		public void OnslaughtAbilityUnitTest() {
			OutputController.Display.ClearUserOutput();
			Player player = new Player("test", Player.PlayerClassType.Warrior) {
				_MaxRagePoints = 100,
				_RagePoints = 100,
				_InCombat = true
			};
			player._Abilities.Add(new PlayerAbility(
				"onslaught", 25, 1, PlayerAbility.WarriorAbility.Onslaught, 8));
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { _HitPoints = 100, _MaxHitPoints = 100, _InCombat = true };
			MonsterBuilder.BuildMonster(monster);
			int abilityIndex = player._Abilities.FindIndex(
				f => f._WarAbilityCategory == PlayerAbility.WarriorAbility.Onslaught);
			string[] inputInfo = new[] { "ability", "onslaught" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Onslaught", OutputController.Display._Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display._Output[1][2]);
			Assert.AreEqual("Rage Cost: 25", OutputController.Display._Output[2][2]);
			Assert.AreEqual("Instant Damage: 25", OutputController.Display._Output[3][2]);
			Assert.AreEqual(
				"Two attacks are launched which each cause instant damage. Cost and damage are per attack.",
				OutputController.Display._Output[4][2]);
			string[] input = new[] { "use", "onslaught" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("onslaught", abilityName);
			player.UseAbility(monster, input);
			int? rageCost = player._Abilities[abilityIndex]._RageCost;
			int hitAmount = player._Abilities[abilityIndex]._Offensive._Amount;
			Assert.AreEqual(monster._MaxHitPoints - (2 * hitAmount), monster._HitPoints);
			Assert.AreEqual(player._MaxRagePoints - (2 * rageCost), player._RagePoints);
			string attackString = $"Your onslaught hit the {monster._Name} for 25 physical damage.";
			Assert.AreEqual(attackString, OutputController.Display._Output[5][2]);
			Assert.AreEqual(attackString, OutputController.Display._Output[6][2]);
			player._MaxRagePoints = 25;
			player._RagePoints = player._MaxRagePoints;
			monster._MaxHitPoints = 100;
			monster._HitPoints = monster._MaxHitPoints;
			player.UseAbility(monster, input);
			Assert.AreEqual(player._MaxRagePoints - rageCost, player._RagePoints);
			Assert.AreEqual(attackString, OutputController.Display._Output[7][2]);
			const string outOfRageString = "You didn't have enough rage points for the second attack!";
			Assert.AreEqual(outOfRageString, OutputController.Display._Output[8][2]);
		}
	}
}