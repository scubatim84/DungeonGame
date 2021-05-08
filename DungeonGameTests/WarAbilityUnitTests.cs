﻿using DungeonGame;
using DungeonGame.Controllers;
using DungeonGame.Effects;
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
			Player player = new Player("test", Player.PlayerClassType.Warrior) { MaxRagePoints = 100, RagePoints = 100 };
			OutputController.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { HitPoints = 100, MaxHitPoints = 100 };
			MonsterBuilder.BuildMonster(monster);
			foreach (IItem item in monster.MonsterItems.Where(item => item is IEquipment eItem && eItem.Equipped)) {
				IEquipment eItem = item as IEquipment;
				eItem.Equipped = false;
			}
			string[] inputInfo = new[] { "ability", "slash" };
			int abilityIndex = player.Abilities.FindIndex(
				f => f.WarAbilityCategory == PlayerAbility.WarriorAbility.Slash);
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Slash", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Rage Cost: 40", OutputController.Display.Output[2][2]);
			Assert.AreEqual("Instant Damage: 50", OutputController.Display.Output[3][2]);
			string[] input = new[] { "use", "slash" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("slash", abilityName);
			player.UseAbility(monster, input);
			Assert.AreEqual(player.MaxRagePoints - player.Abilities[abilityIndex].RageCost,
				player.RagePoints);
			int abilityDamage = player.Abilities[abilityIndex].Offensive._Amount;
			Assert.AreEqual(monster.HitPoints, monster.MaxHitPoints - abilityDamage);
			string abilitySuccessString = $"Your {player.Abilities[abilityIndex].Name} hit the {monster.Name} for {abilityDamage} physical damage.";
			Assert.AreEqual(abilitySuccessString, OutputController.Display.Output[4][2]);
		}
		[Test]
		public void RendAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Warrior) { MaxRagePoints = 100, RagePoints = 100 };
			OutputController.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { HitPoints = 100, MaxHitPoints = 100 };
			MonsterBuilder.BuildMonster(monster);
			foreach (IItem item in monster.MonsterItems.Where(item => item is IEquipment eItem && eItem.Equipped)) {
				IEquipment eItem = item as IEquipment;
				eItem.Equipped = false;
			}
			int abilityIndex = player.Abilities.FindIndex(
				f => f.WarAbilityCategory == PlayerAbility.WarriorAbility.Rend);
			string[] inputInfo = new[] { "ability", "rend" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Rend", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Rage Cost: 25", OutputController.Display.Output[2][2]);
			Assert.AreEqual("Instant Damage: 15", OutputController.Display.Output[3][2]);
			Assert.AreEqual("Damage Over Time: 5", OutputController.Display.Output[4][2]);
			string bleedOverTimeString = $"Bleeding damage over time for {player.Abilities[abilityIndex].Offensive._AmountMaxRounds} rounds.";
			Assert.AreEqual(bleedOverTimeString, OutputController.Display.Output[5][2]);
			string[] input = new[] { "use", "rend" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("rend", abilityName);
			player.UseAbility(monster, input);
			int? rageCost = player.Abilities[abilityIndex].RageCost;
			Assert.AreEqual(player.MaxRagePoints - rageCost, player.RagePoints);
			int abilityDamage = player.Abilities[abilityIndex].Offensive._Amount;
			int abilityDamageOverTime = player.Abilities[abilityIndex].Offensive._AmountOverTime;
			int abilityCurRounds = player.Abilities[abilityIndex].Offensive._AmountCurRounds;
			int abilityMaxRounds = player.Abilities[abilityIndex].Offensive._AmountMaxRounds;
			string abilitySuccessString = $"Your {player.Abilities[abilityIndex].Name} hit the {monster.Name} for {abilityDamage} physical damage.";
			Assert.AreEqual(abilitySuccessString, OutputController.Display.Output[6][2]);
			string bleedString = $"The {monster.Name} is bleeding!";
			Assert.AreEqual(bleedString, OutputController.Display.Output[7][2]);
			Assert.AreEqual(true, monster.Effects[0] is BleedingEffect);
			Assert.AreEqual(monster.MaxHitPoints - abilityDamage, monster.HitPoints);
			Assert.AreEqual(abilityCurRounds, monster.Effects[0].CurrentRound);
			Assert.AreEqual(abilityMaxRounds, monster.Effects[0].MaxRound);
			BleedingEffect bleedEffect = monster.Effects[0] as BleedingEffect;
			OutputController.Display.ClearUserOutput();
			for (int i = 2; i < 5; i++) {
				bleedEffect.ProcessRound();
				int bleedAmount = bleedEffect.BleedDamageOverTime;
				string bleedRoundString = $"The {monster.Name} bleeds for {bleedAmount} physical damage.";
				Assert.AreEqual(bleedRoundString, OutputController.Display.Output[i - 2][2]);
				Assert.AreEqual(i, monster.Effects[0].CurrentRound);
				GameController.RemovedExpiredEffectsAsync(monster);
				Thread.Sleep(1000);
			}
			Assert.AreEqual(false, monster.Effects.Any());
			Assert.AreEqual(monster.MaxHitPoints - abilityDamage - (abilityDamageOverTime * abilityMaxRounds),
				monster.HitPoints);
		}
		[Test]
		public void ChargeAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Warrior) {
				MaxRagePoints = 100,
				RagePoints = 100,
				InCombat = true
			};
			OutputController.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { HitPoints = 100, MaxHitPoints = 100, InCombat = true };
			MonsterBuilder.BuildMonster(monster);
			foreach (IItem item in monster.MonsterItems.Where(item => item is IEquipment eItem && eItem.Equipped)) {
				IEquipment eItem = item as IEquipment;
				eItem.Equipped = false;
			}
			int abilityIndex = player.Abilities.FindIndex(
				f => f.WarAbilityCategory == PlayerAbility.WarriorAbility.Charge);
			string[] inputInfo = new[] { "ability", "charge" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Charge", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Rage Cost: 25", OutputController.Display.Output[2][2]);
			Assert.AreEqual("Instant Damage: 15", OutputController.Display.Output[3][2]);
			string abilityInfoString = $"Stuns opponent for {player.Abilities[abilityIndex].Stun._StunMaxRounds} rounds, preventing their attacks.";
			Assert.AreEqual(abilityInfoString, OutputController.Display.Output[4][2]);
			string[] input = new[] { "use", "charge" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("charge", abilityName);
			player.UseAbility(monster, input);
			int? rageCost = player.Abilities[abilityIndex].RageCost;
			Assert.AreEqual(player.MaxRagePoints - rageCost, player.RagePoints);
			int abilityDamage = player.Abilities[abilityIndex].Stun._DamageAmount;
			int abilityCurRounds = player.Abilities[abilityIndex].Stun._StunCurRounds;
			int abilityMaxRounds = player.Abilities[abilityIndex].Stun._StunMaxRounds;
			string attackSuccessString = $"You {player.Abilities[abilityIndex].Name} the {monster.Name} for {abilityDamage} physical damage.";
			Assert.AreEqual(attackSuccessString, OutputController.Display.Output[5][2]);
			string stunString = $"The {monster.Name} is stunned!";
			Assert.AreEqual(stunString, OutputController.Display.Output[6][2]);
			Assert.AreEqual(monster.MaxHitPoints - abilityDamage, monster.HitPoints);
			Assert.AreEqual(true, monster.Effects[0] is StunnedEffect);
			Assert.AreEqual(abilityCurRounds, monster.Effects[0].CurrentRound);
			Assert.AreEqual(abilityMaxRounds, monster.Effects[0].MaxRound);
			StunnedEffect stunnedEffect = monster.Effects[0] as StunnedEffect;
			OutputController.Display.ClearUserOutput();
			for (int i = 2; i < 4; i++) {
				stunnedEffect.ProcessRound();
				string stunnedString = $"The {monster.Name} is stunned and cannot attack.";
				Assert.AreEqual(stunnedString, OutputController.Display.Output[i - 2][2]);
				Assert.AreEqual(i, monster.Effects[0].CurrentRound);
				GameController.RemovedExpiredEffectsAsync(monster);
				Thread.Sleep(1000);
			}
			Assert.AreEqual(false, monster.Effects.Any());
		}
		[Test]
		public void BlockAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Warrior) {
				MaxRagePoints = 100,
				RagePoints = 100,
				InCombat = true,
				MaxHitPoints = 100,
				HitPoints = 100,
				DodgeChance = 0,
				Level = 3
			};
			OutputController.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Zombie) { HitPoints = 100, MaxHitPoints = 100, InCombat = true };
			MonsterBuilder.BuildMonster(monster);
			monster.MonsterWeapon._CritMultiplier = 1;
			int abilityIndex = player.Abilities.FindIndex(
				f => f.WarAbilityCategory == PlayerAbility.WarriorAbility.Block);
			string[] inputInfo = new[] { "ability", "block" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Block", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Rage Cost: 25", OutputController.Display.Output[2][2]);
			Assert.AreEqual("Block Damage: 50", OutputController.Display.Output[3][2]);
			const string blockInfoString =
				"Block damage will prevent incoming damage from opponent until block damage is used up.";
			Assert.AreEqual(blockInfoString, OutputController.Display.Output[4][2]);
			string[] input = new[] { "use", "block" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("block", abilityName);
			player.UseAbility(monster, input);
			int? rageCost = player.Abilities[abilityIndex].RageCost;
			Assert.AreEqual(player.MaxRagePoints - rageCost, player.RagePoints);
			int blockAmount = player.Abilities[abilityIndex].Defensive._BlockDamage;
			string blockString = $"You start blocking your opponent's attacks! You will block {blockAmount} damage.";
			Assert.AreEqual(blockString, OutputController.Display.Output[5][2]);
			OutputController.Display.ClearUserOutput();
			Assert.AreEqual(true, player.Effects[0] is BlockDamageEffect);
			Assert.AreEqual(player.MaxHitPoints, player.HitPoints);
			BlockDamageEffect blockDmgEffect = player.Effects[0] as BlockDamageEffect;
			int blockAmountRemaining = blockDmgEffect.BlockAmount;
			Assert.AreEqual(blockAmount, blockAmountRemaining);
			int i = 0;
			while (blockAmountRemaining > 0) {
				monster.MonsterWeapon._Durability = 100;
				int blockAmountBefore = blockAmountRemaining;
				monster.Attack(player);
				blockAmountRemaining = player.Effects.Any() ? blockDmgEffect.BlockAmount : 0;
				if (blockAmountRemaining > 0) {
					Assert.AreEqual(player.MaxHitPoints, player.HitPoints);
					string blockRoundString = $"Your defensive move blocked {(blockAmountBefore - blockAmountRemaining)} damage!";
					Assert.AreEqual(blockRoundString, OutputController.Display.Output[i + (i * 1)][2]);
				} else {
					GameController.RemovedExpiredEffectsAsync(player);
					int attackAmount = monster.MonsterWeapon.Attack() - blockAmountBefore;
					Assert.AreEqual(player.MaxHitPoints - attackAmount, player.HitPoints);
					const string blockEndString = "You are no longer blocking damage!";
					Assert.AreEqual(blockEndString, OutputController.Display.Output[i + 3][2]);
					Thread.Sleep(1000);
					Assert.AreEqual(false, player.Effects.Any());
					string hitString = $"The {monster.Name} hits you for {attackAmount} physical damage.";
					Assert.AreEqual(hitString, OutputController.Display.Output[i + 4][2]);
				}
				i++;
			}
		}
		[Test]
		public void BerserkAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Warrior) {
				MaxRagePoints = 100,
				RagePoints = 100,
				InCombat = true,
				MaxHitPoints = 100,
				HitPoints = 100,
				DodgeChance = 0
			};
			GearController.EquipInitialGear(player);
			OutputController.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { HitPoints = 100, MaxHitPoints = 100, InCombat = true };
			MonsterBuilder.BuildMonster(monster);
			foreach (IItem item in monster.MonsterItems.Where(item => item is IEquipment eItem && eItem.Equipped)) {
				IEquipment eItem = item as IEquipment;
				eItem.Equipped = false;
			}
			int abilityIndex = player.Abilities.FindIndex(
				f => f.WarAbilityCategory == PlayerAbility.WarriorAbility.Berserk);
			string[] inputInfo = new[] { "ability", "berserk" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Berserk", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Rage Cost: 40", OutputController.Display.Output[2][2]);
			string dmgIncreaseString = $"Damage Increase: {player.Abilities[abilityIndex].Offensive._Amount}";
			Assert.AreEqual(dmgIncreaseString, OutputController.Display.Output[3][2]);
			string armDecreaseString = $"Armor Decrease: {player.Abilities[abilityIndex].ChangeAmount._Amount}";
			Assert.AreEqual(armDecreaseString, OutputController.Display.Output[4][2]);
			string dmgInfoString = $"Damage increased at cost of armor decrease for {player.Abilities[abilityIndex].ChangeAmount._ChangeMaxRound} rounds";
			Assert.AreEqual(dmgInfoString, OutputController.Display.Output[5][2]);
			string[] input = new[] { "use", "berserk" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("berserk", abilityName);
			int baseArmorRating = GearController.CheckArmorRating(player);
			int baseDamage = player.PhysicalAttack(monster);
			player.UseAbility(monster, input);
			int? rageCost = player.Abilities[abilityIndex].RageCost;
			Assert.AreEqual(player.MaxRagePoints - rageCost, player.RagePoints);
			Assert.AreEqual(2, player.Effects.Count);
			Assert.AreEqual(true, player.Effects[0] is ChangePlayerDamageEffect);
			Assert.AreEqual(true, player.Effects[1] is ChangeArmorEffect);
			ChangePlayerDamageEffect changePlayerDmgEffect = player.Effects[0] as ChangePlayerDamageEffect;
			ChangeArmorEffect changeArmorEffect = player.Effects[1] as ChangeArmorEffect;
			const string berserkString = "You go into a berserk rage!";
			Assert.AreEqual(berserkString, OutputController.Display.Output[6][2]);
			for (int i = 2; i < 6; i++) {
				OutputController.Display.ClearUserOutput();
				int berserkArmorAmount = player.Abilities[abilityIndex].ChangeAmount._Amount;
				int berserkDamageAmount = player.Abilities[abilityIndex].Offensive._Amount;
				int berserkArmorRating = GearController.CheckArmorRating(player);
				int berserkDamage = player.PhysicalAttack(monster);
				Assert.AreEqual(berserkArmorRating, baseArmorRating + berserkArmorAmount);
				Assert.AreEqual(berserkDamage, baseDamage + berserkDamageAmount, 5);
				changePlayerDmgEffect.ProcessChangePlayerDamageRound(player);
				string changeDmgString = $"Your damage is increased by {berserkDamageAmount}.";
				Assert.AreEqual(changeDmgString, OutputController.Display.Output[0][2]);
				changeArmorEffect.ProcessChangeArmorRound();
				string changeArmorString = $"Your armor is decreased by {berserkArmorAmount * -1}.";
				Assert.AreEqual(changeArmorString, OutputController.Display.Output[1][2]);
				GameController.RemovedExpiredEffectsAsync(player);
				Thread.Sleep(1000);
			}
			Assert.AreEqual(false, player.Effects.Any());
		}
		[Test]
		public void DisarmAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Warrior) {
				MaxRagePoints = 100,
				RagePoints = 100,
				MaxHitPoints = 100,
				HitPoints = 100
			};
			GearController.EquipInitialGear(player);
			OutputController.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { HitPoints = 100, MaxHitPoints = 100 };
			MonsterBuilder.BuildMonster(monster);
			int abilityIndex = player.Abilities.FindIndex(
				f => f.WarAbilityCategory == PlayerAbility.WarriorAbility.Disarm);
			string[] inputInfo = new[] { "ability", "disarm" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Disarm", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Rage Cost: 25", OutputController.Display.Output[2][2]);
			string abilityString = $"{player.Abilities[abilityIndex].Offensive._Amount}% chance to disarm opponent's weapon.";
			Assert.AreEqual(abilityString, OutputController.Display.Output[3][2]);
			player.Abilities[abilityIndex].Offensive._Amount = 0; // Set disarm success chance to 0% for test
			string[] input = new[] { "use", "disarm" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("disarm", abilityName);
			player.UseAbility(monster, input);
			int? rageCost = player.Abilities[abilityIndex].RageCost;
			Assert.AreEqual(player.MaxRagePoints - rageCost, player.RagePoints);
			Assert.AreEqual(true, monster.MonsterWeapon.Equipped);
			string disarmFailString = $"You tried to disarm {monster.Name} but failed!";
			Assert.AreEqual(disarmFailString, OutputController.Display.Output[4][2]);
			player.Abilities[abilityIndex].Offensive._Amount = 100; // Set disarm success chance to 100% for test
			player.UseAbility(monster, input);
			Assert.AreEqual(player.MaxRagePoints - (rageCost * 2), player.RagePoints);
			Assert.AreEqual(false, monster.MonsterWeapon.Equipped);
			string disarmSuccessString = $"You successfully disarmed {monster.Name}!";
			Assert.AreEqual(disarmSuccessString, OutputController.Display.Output[5][2]);
		}
		[Test]
		public void BandageAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Warrior) {
				MaxRagePoints = 100,
				RagePoints = 100,
				MaxHitPoints = 100,
				HitPoints = 10
			};
			GearController.EquipInitialGear(player);
			player.Abilities.Add(new PlayerAbility(
				"bandage", 25, 1, PlayerAbility.WarriorAbility.Bandage, 2));
			OutputController.Display.ClearUserOutput();
			int abilityIndex = player.Abilities.FindIndex(
				f => f.WarAbilityCategory == PlayerAbility.WarriorAbility.Bandage);
			string[] inputInfo = new[] { "ability", "bandage" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Bandage", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Rage Cost: 25", OutputController.Display.Output[2][2]);
			Assert.AreEqual("Heal Amount: 25", OutputController.Display.Output[3][2]);
			Assert.AreEqual("Heal Over Time: 5", OutputController.Display.Output[4][2]);
			string healInfoStringCombat = $"Heal over time will restore health for {player.Abilities[abilityIndex].Healing._HealMaxRounds} rounds in combat.";
			Assert.AreEqual(healInfoStringCombat, OutputController.Display.Output[5][2]);
			string healInfoStringNonCombat = $"Heal over time will restore health {player.Abilities[abilityIndex].Healing._HealMaxRounds} times every 10 seconds.";
			Assert.AreEqual(healInfoStringNonCombat, OutputController.Display.Output[6][2]);
			string[] input = new[] { "use", "bandage" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("bandage", abilityName);
			int baseHitPoints = player.HitPoints;
			player.UseAbility(input);
			int? rageCost = player.Abilities[abilityIndex].RageCost;
			Assert.AreEqual(player.MaxRagePoints - rageCost, player.RagePoints);
			int healAmount = player.Abilities[abilityIndex].Healing._HealAmount;
			string healString = $"You heal yourself for {healAmount} health.";
			Assert.AreEqual(healString, OutputController.Display.Output[7][2]);
			Assert.AreEqual(true, player.Effects[0] is HealingEffect);
			Assert.AreEqual(baseHitPoints + healAmount, player.HitPoints);
			baseHitPoints = player.HitPoints;
			HealingEffect healEffect = player.Effects[0] as HealingEffect;
			OutputController.Display.ClearUserOutput();
			for (int i = 2; i < 5; i++) {
				healEffect.ProcessHealingRound(player);
				int healOverTimeAmt = healEffect.HealOverTimeAmount;
				string healAmtString = $"You have been healed for {healOverTimeAmt} health.";
				Assert.AreEqual(i, player.Effects[0].CurrentRound);
				Assert.AreEqual(healAmtString, OutputController.Display.Output[i - 2][2]);
				Assert.AreEqual(baseHitPoints + ((i - 1) * healOverTimeAmt), player.HitPoints);
			}
			GameController.RemovedExpiredEffectsAsync(player);
			Thread.Sleep(1000);
			Assert.AreEqual(false, player.Effects.Any());
		}
		[Test]
		public void PowerAuraAbilityUnitTest() {
			OutputController.Display.ClearUserOutput();
			Player player = new Player("test", Player.PlayerClassType.Warrior) { MaxRagePoints = 150, RagePoints = 150 };
			player.Abilities.Add(new PlayerAbility(
				"power aura", 150, 1, PlayerAbility.WarriorAbility.PowerAura, 6));
			OutputController.Display.ClearUserOutput();
			int abilityIndex = player.Abilities.FindIndex(
				f => f.WarAbilityCategory == PlayerAbility.WarriorAbility.PowerAura);
			string[] inputInfo = new[] { "ability", "power", "aura" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Power Aura", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Rage Cost: 150", OutputController.Display.Output[2][2]);
			Assert.AreEqual("Power Aura Amount: 15", OutputController.Display.Output[3][2]);
			Assert.AreEqual("Strength is increased by 15 for 10 minutes.", OutputController.Display.Output[4][2]);
			string[] input = new[] { "use", "power", "aura" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("power aura", abilityName);
			int baseStr = player.Strength;
			int? baseRage = player.RagePoints;
			int? baseMaxRage = player.MaxRagePoints;
			player.UseAbility(input);
			int? rageCost = player.Abilities[abilityIndex].RageCost;
			Assert.AreEqual(baseMaxRage - rageCost, player.RagePoints);
			Assert.AreEqual("You generate a Power Aura around yourself.", OutputController.Display.Output[5][2]);
			OutputController.Display.ClearUserOutput();
			Assert.AreEqual(
				baseRage - player.Abilities[abilityIndex].RageCost, player.RagePoints);
			Assert.AreEqual(player.Strength, baseStr + player.Abilities[abilityIndex].ChangeAmount._Amount);
			Assert.AreEqual(
				player.MaxRagePoints, baseMaxRage + player.Abilities[abilityIndex].ChangeAmount._Amount * 10);
			Assert.AreEqual(true, player.Effects[0] is ChangeStatEffect);
			ChangeStatEffect changeStatEffect = player.Effects[0] as ChangeStatEffect;
			for (int i = 0; i < 600; i++) {
				changeStatEffect.ProcessChangeStatRound(player);
			}
			GameController.RemovedExpiredEffectsAsync(player);
			Thread.Sleep(1000);
			Assert.AreEqual(false, player.Effects.Any());
			Assert.AreEqual(baseStr, player.Strength);
			Assert.AreEqual(baseMaxRage, player.MaxRagePoints);
			Assert.AreEqual(player.MaxRagePoints - rageCost, player.RagePoints);
		}
		[Test]
		public void WarCryAbilityUnitTest() {
			OutputController.Display.ClearUserOutput();
			Player player = new Player("test", Player.PlayerClassType.Warrior) {
				MaxRagePoints = 100,
				RagePoints = 100,
				InCombat = true
			};
			player.Abilities.Add(new PlayerAbility(
				"war cry", 50, 1, PlayerAbility.WarriorAbility.WarCry, 4));
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { HitPoints = 100, MaxHitPoints = 100 };
			MonsterBuilder.BuildMonster(monster);
			int abilityIndex = player.Abilities.FindIndex(
				f => f.WarAbilityCategory == PlayerAbility.WarriorAbility.WarCry);
			string[] inputInfo = new[] { "ability", "war", "cry" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("War Cry", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Rage Cost: 50", OutputController.Display.Output[2][2]);
			Assert.AreEqual("War Cry Amount: 25", OutputController.Display.Output[3][2]);
			Assert.AreEqual("Opponent's attacks are decreased by 25 for 3 rounds.",
				OutputController.Display.Output[4][2]);
			string[] input = new[] { "use", "war", "cry" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("war cry", abilityName);
			player.UseAbility(input);
			int? rageCost = player.Abilities[abilityIndex].RageCost;
			Assert.AreEqual(player.MaxRagePoints - rageCost, player.RagePoints);
			Assert.AreEqual("You shout a War Cry, intimidating your opponent, and decreasing incoming damage.",
				OutputController.Display.Output[5][2]);
			OutputController.Display.ClearUserOutput();
			Assert.AreEqual(true, player.Effects[0] is ChangeMonsterDamageEffect);
			ChangeMonsterDamageEffect changeMonsterDmgEffect = player.Effects[0] as ChangeMonsterDamageEffect;
			for (int i = 2; i < 5; i++) {
				int baseAttackDamageM = monster.UnarmedAttackDamage;
				int attackDamageM = baseAttackDamageM;
				int changeDamageAmount = changeMonsterDmgEffect.ChangeAmount < attackDamageM ?
					changeMonsterDmgEffect.ChangeAmount : attackDamageM;
				attackDamageM -= changeDamageAmount;
				Assert.AreEqual(baseAttackDamageM - changeDamageAmount, attackDamageM);
				changeMonsterDmgEffect.ProcessChangeMonsterDamageRound(player);
				string changeDmgString = $"Incoming damage is decreased by {-1 * changeMonsterDmgEffect.ChangeAmount}.";
				Assert.AreEqual(i, player.Effects[0].CurrentRound);
				Assert.AreEqual(changeDmgString, OutputController.Display.Output[i - 2][2]);
			}
			GameController.RemovedExpiredEffectsAsync(player);
			Thread.Sleep(1000);
			Assert.AreEqual(false, player.Effects.Any());
		}
		[Test]
		public void OnslaughtAbilityUnitTest() {
			OutputController.Display.ClearUserOutput();
			Player player = new Player("test", Player.PlayerClassType.Warrior) {
				MaxRagePoints = 100,
				RagePoints = 100,
				InCombat = true
			};
			player.Abilities.Add(new PlayerAbility(
				"onslaught", 25, 1, PlayerAbility.WarriorAbility.Onslaught, 8));
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { HitPoints = 100, MaxHitPoints = 100, InCombat = true };
			MonsterBuilder.BuildMonster(monster);
			int abilityIndex = player.Abilities.FindIndex(
				f => f.WarAbilityCategory == PlayerAbility.WarriorAbility.Onslaught);
			string[] inputInfo = new[] { "ability", "onslaught" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Onslaught", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Rage Cost: 25", OutputController.Display.Output[2][2]);
			Assert.AreEqual("Instant Damage: 25", OutputController.Display.Output[3][2]);
			Assert.AreEqual(
				"Two attacks are launched which each cause instant damage. Cost and damage are per attack.",
				OutputController.Display.Output[4][2]);
			string[] input = new[] { "use", "onslaught" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("onslaught", abilityName);
			player.UseAbility(monster, input);
			int? rageCost = player.Abilities[abilityIndex].RageCost;
			int hitAmount = player.Abilities[abilityIndex].Offensive._Amount;
			Assert.AreEqual(monster.MaxHitPoints - (2 * hitAmount), monster.HitPoints);
			Assert.AreEqual(player.MaxRagePoints - (2 * rageCost), player.RagePoints);
			string attackString = $"Your onslaught hit the {monster.Name} for 25 physical damage.";
			Assert.AreEqual(attackString, OutputController.Display.Output[5][2]);
			Assert.AreEqual(attackString, OutputController.Display.Output[6][2]);
			player.MaxRagePoints = 25;
			player.RagePoints = player.MaxRagePoints;
			monster.MaxHitPoints = 100;
			monster.HitPoints = monster.MaxHitPoints;
			player.UseAbility(monster, input);
			Assert.AreEqual(player.MaxRagePoints - rageCost, player.RagePoints);
			Assert.AreEqual(attackString, OutputController.Display.Output[7][2]);
			const string outOfRageString = "You didn't have enough rage points for the second attack!";
			Assert.AreEqual(outOfRageString, OutputController.Display.Output[8][2]);
		}
	}
}