﻿using DungeonGame;
using DungeonGame.Controllers;
using DungeonGame.Coordinates;
using DungeonGame.Effects;
using DungeonGame.Items;
using DungeonGame.Items.Equipment;
using DungeonGame.Monsters;
using DungeonGame.Players;
using DungeonGame.Rooms;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DungeonGameTests {
	public class ArcAbilityUnitTests {
		[Test]
		public void DistanceShotAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Archer) {
				MaxComboPoints = 100,
				ComboPoints = 100,
				MaxHitPoints = 100,
				HitPoints = 10
			};
			RoomController._Rooms = new Dictionary<Coordinate, IRoom> {
				{new Coordinate(0, 0, 0), new DungeonRoom(1, 1)},
				{new Coordinate(0, 1, 0), new DungeonRoom(1, 1)},
				{new Coordinate(1, 0, 0), new DungeonRoom(1, 1)}
			};
			Coordinate roomOneCoord = new Coordinate(0, 1, 0);
			Coordinate roomTwoCoord = new Coordinate(1, 0, 0);
			RoomController._Rooms[roomTwoCoord]._Monster = null;
			RoomController._Rooms[roomOneCoord]._Monster = new Monster(3, Monster.MonsterType.Demon) { HitPoints = 100, MaxHitPoints = 100 };
			Monster monster = RoomController._Rooms[roomOneCoord]._Monster;
			MonsterBuilder.BuildMonster(monster);
			RoomController.SetPlayerLocation(player, 0, 0, 0);
			GearController.EquipInitialGear(player);
			OutputController.Display.ClearUserOutput();
			string[] inputInfo = new[] { "ability", "distance" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Distance Shot", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Combo Cost: 25", OutputController.Display.Output[2][2]);
			Assert.AreEqual("Instant Damage: 25", OutputController.Display.Output[3][2]);
			Assert.AreEqual("50% chance to hit monster in attack direction.",
				OutputController.Display.Output[4][2]);
			Assert.AreEqual("Usage example if monster is in room to north. 'use distance north'",
				OutputController.Display.Output[5][2]);
			string[] input = new[] { "use", "distance", "south" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("distance south", abilityName);
			int arrowCount = player.PlayerQuiver.Quantity;
			player.UseAbility(input);
			Assert.AreEqual(arrowCount, player.PlayerQuiver.Quantity);
			Assert.AreEqual("You can't attack in that direction!", OutputController.Display.Output[6][2]);
			Assert.AreEqual(player.MaxComboPoints, player.ComboPoints);
			input = new[] { "use", "distance", "east" };
			player.UseAbility(input);
			Assert.AreEqual(arrowCount, player.PlayerQuiver.Quantity);
			Assert.AreEqual("There is no monster in that direction to attack!",
				OutputController.Display.Output[7][2]);
			Assert.AreEqual(player.MaxComboPoints, player.ComboPoints);
			Assert.AreEqual(player.MaxComboPoints, player.ComboPoints);
			int abilityIndex = player.Abilities.FindIndex(
				f => f.ArcAbilityCategory == PlayerAbility.ArcherAbility.Distance);
			int? comboCost = player.Abilities[abilityIndex].ComboCost;
			player.Abilities[abilityIndex].Offensive._ChanceToSucceed = 0;
			input = new[] { "use", "distance", "north" };
			player.UseAbility(input);
			Assert.AreEqual(arrowCount - 1, player.PlayerQuiver.Quantity);
			string missString = $"You tried to shoot {monster.Name} from afar but failed!";
			Assert.AreEqual(missString, OutputController.Display.Output[8][2]);
			Assert.AreEqual(player.MaxComboPoints - comboCost, player.ComboPoints);
			player.Abilities[abilityIndex].Offensive._ChanceToSucceed = 100;
			player.ComboPoints = player.MaxComboPoints;
			arrowCount = player.PlayerQuiver.Quantity;
			player.UseAbility(input);
			Assert.AreEqual(arrowCount - 1, player.PlayerQuiver.Quantity);
			int abilityDmg = player.Abilities[abilityIndex].Offensive._Amount;
			string shootString = $"You successfully shot {monster.Name} from afar for {abilityDmg} damage!";
			Assert.AreEqual(shootString, OutputController.Display.Output[9][2]);
			Assert.AreEqual(player.MaxComboPoints - comboCost, player.ComboPoints);
			Assert.AreEqual(monster.MaxHitPoints - abilityDmg, monster.HitPoints);
		}
		[Test]
		public void GutShotAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Archer) {
				MaxComboPoints = 100,
				ComboPoints = 100,
				MaxHitPoints = 100,
				HitPoints = 10
			};
			GearController.EquipInitialGear(player);
			OutputController.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { HitPoints = 100, MaxHitPoints = 100 };
			MonsterBuilder.BuildMonster(monster);
			foreach (IItem item in monster.MonsterItems.Where(item => item is IEquipment eItem && eItem.Equipped)) {
				IEquipment eItem = item as IEquipment;
				eItem.Equipped = false;
			}
			int abilityIndex = player.Abilities.FindIndex(
				f => f.ArcAbilityCategory == PlayerAbility.ArcherAbility.Gut);
			string[] inputInfo = new[] { "ability", "gut" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Gut Shot", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Combo Cost: 25", OutputController.Display.Output[2][2]);
			Assert.AreEqual("Instant Damage: 15", OutputController.Display.Output[3][2]);
			Assert.AreEqual("Damage Over Time: 5", OutputController.Display.Output[4][2]);
			string bleedOverTimeString = $"Bleeding damage over time for {player.Abilities[abilityIndex].Offensive._AmountMaxRounds} rounds.";
			Assert.AreEqual(bleedOverTimeString, OutputController.Display.Output[5][2]);
			string[] input = new[] { "use", "gut" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("gut", abilityName);
			int arrowCount = player.PlayerQuiver.Quantity;
			player.UseAbility(monster, input);
			Assert.AreEqual(arrowCount - 1, player.PlayerQuiver.Quantity);
			int? comboCost = player.Abilities[abilityIndex].ComboCost;
			Assert.AreEqual(player.MaxComboPoints - comboCost, player.ComboPoints);
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
			OutputController.Display.ClearUserOutput();
			for (int i = 2; i < 5; i++) {
				BleedingEffect bleedEffect = monster.Effects[0] as BleedingEffect;
				bleedEffect.ProcessBleedingRound(monster);
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
		public void PreciseShotAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Archer) {
				MaxComboPoints = 100,
				ComboPoints = 100,
				MaxHitPoints = 100,
				HitPoints = 10
			};
			GearController.EquipInitialGear(player);
			OutputController.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { HitPoints = 100, MaxHitPoints = 100 };
			MonsterBuilder.BuildMonster(monster);
			foreach (IItem item in monster.MonsterItems.Where(item => item is IEquipment eItem && eItem.Equipped)) {
				IEquipment eItem = item as IEquipment;
				eItem.Equipped = false;
			}
			string[] inputInfo = new[] { "ability", "precise" };
			int abilityIndex = player.Abilities.FindIndex(
				f => f.ArcAbilityCategory == PlayerAbility.ArcherAbility.Precise);
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Precise Shot", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Combo Cost: 40", OutputController.Display.Output[2][2]);
			Assert.AreEqual("Instant Damage: 50", OutputController.Display.Output[3][2]);
			string[] input = new[] { "use", "precise" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("precise", abilityName);
			int arrowCount = player.PlayerQuiver.Quantity;
			player.UseAbility(monster, input);
			Assert.AreEqual(arrowCount - 1, player.PlayerQuiver.Quantity);
			int? comboCost = player.Abilities[abilityIndex].ComboCost;
			Assert.AreEqual(player.MaxComboPoints - comboCost, player.ComboPoints);
			int abilityDamage = player.Abilities[abilityIndex].Offensive._Amount;
			Assert.AreEqual(monster.HitPoints, monster.MaxHitPoints - abilityDamage);
			string abilitySuccessString = $"Your {player.Abilities[abilityIndex].Name} hit the {monster.Name} for {abilityDamage} physical damage.";
			Assert.AreEqual(abilitySuccessString, OutputController.Display.Output[4][2]);
		}
		[Test]
		public void StunShotAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Archer) {
				MaxComboPoints = 100,
				ComboPoints = 100,
				MaxHitPoints = 100,
				HitPoints = 10
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
				f => f.ArcAbilityCategory == PlayerAbility.ArcherAbility.Stun);
			string[] inputInfo = new[] { "ability", "stun" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Stun Shot", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Combo Cost: 25", OutputController.Display.Output[2][2]);
			Assert.AreEqual("Instant Damage: 15", OutputController.Display.Output[3][2]);
			string abilityInfoString = $"Stuns opponent for {player.Abilities[abilityIndex].Stun._StunMaxRounds} rounds, preventing their attacks.";
			Assert.AreEqual(abilityInfoString, OutputController.Display.Output[4][2]);
			string[] input = new[] { "use", "stun" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("stun", abilityName);
			int arrowCount = player.PlayerQuiver.Quantity;
			player.UseAbility(monster, input);
			Assert.AreEqual(arrowCount - 1, player.PlayerQuiver.Quantity);
			int? comboCost = player.Abilities[abilityIndex].ComboCost;
			Assert.AreEqual(player.MaxComboPoints - comboCost, player.ComboPoints);
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
			OutputController.Display.ClearUserOutput();
			for (int i = 2; i < 5; i++) {
				StunnedEffect stunnedEffect = monster.Effects[0] as StunnedEffect;
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
		public void WoundShotAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Archer) {
				MaxComboPoints = 100,
				ComboPoints = 100,
				MaxHitPoints = 100,
				HitPoints = 10
			};
			GearController.EquipInitialGear(player);
			OutputController.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { HitPoints = 100, MaxHitPoints = 100 };
			MonsterBuilder.BuildMonster(monster);
			foreach (IItem item in monster.MonsterItems.Where(item => item is IEquipment eItem && eItem.Equipped)) {
				IEquipment eItem = item as IEquipment;
				eItem.Equipped = false;
			}
			int abilityIndex = player.Abilities.FindIndex(
				f => f.ArcAbilityCategory == PlayerAbility.ArcherAbility.Wound);
			string[] inputInfo = new[] { "ability", "wound" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Wound Shot", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Combo Cost: 40", OutputController.Display.Output[2][2]);
			Assert.AreEqual("Instant Damage: 5", OutputController.Display.Output[3][2]);
			Assert.AreEqual("Damage Over Time: 10", OutputController.Display.Output[4][2]);
			string bleedOverTimeString = $"Bleeding damage over time for {player.Abilities[abilityIndex].Offensive._AmountMaxRounds} rounds.";
			Assert.AreEqual(bleedOverTimeString, OutputController.Display.Output[5][2]);
			string[] input = new[] { "use", "wound" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("wound", abilityName);
			int arrowCount = player.PlayerQuiver.Quantity;
			player.UseAbility(monster, input);
			Assert.AreEqual(arrowCount - 1, player.PlayerQuiver.Quantity);
			int? comboCost = player.Abilities[abilityIndex].ComboCost;
			Assert.AreEqual(player.MaxComboPoints - comboCost, player.ComboPoints);
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
			OutputController.Display.ClearUserOutput();
			for (int i = 2; i < 7; i++) {
				BleedingEffect bleedEffect = monster.Effects[0] as BleedingEffect;
				bleedEffect.ProcessBleedingRound(monster);
				int bleedAmount = bleedEffect.BleedDamageOverTime;
				string bleedRoundString = $"The {monster.Name} bleeds for {bleedAmount} physical damage.";
				Assert.AreEqual(bleedRoundString, OutputController.Display.Output[i - 2][2]);
				Assert.AreEqual(i, monster.Effects[0].CurrentRound);
				GameController.RemovedExpiredEffectsAsync(monster);
			}
			Thread.Sleep(1000);
			Assert.AreEqual(false, monster.Effects.Any());
			Assert.AreEqual(monster.MaxHitPoints - abilityDamage - (abilityDamageOverTime * abilityMaxRounds),
				monster.HitPoints);
		}
		[Test]
		public void DoubleShotAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Archer) {
				MaxComboPoints = 100,
				ComboPoints = 100,
				MaxHitPoints = 100,
				HitPoints = 10
			};
			GearController.EquipInitialGear(player);
			OutputController.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { HitPoints = 100, MaxHitPoints = 100, InCombat = true };
			MonsterBuilder.BuildMonster(monster);
			int abilityIndex = player.Abilities.FindIndex(
				f => f.ArcAbilityCategory == PlayerAbility.ArcherAbility.Double);
			string[] inputInfo = new[] { "ability", "double" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Double Shot", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Combo Cost: 25", OutputController.Display.Output[2][2]);
			Assert.AreEqual("Instant Damage: 25", OutputController.Display.Output[3][2]);
			Assert.AreEqual(
				"Two arrows are fired which each cause instant damage. Cost and damage are per arrow.",
				OutputController.Display.Output[4][2]);
			string[] input = new[] { "use", "double" };
			string abilityName = InputController.ParseInput(input);
			Assert.AreEqual("double", abilityName);
			int arrowCount = player.PlayerQuiver.Quantity;
			player.UseAbility(monster, input);
			Assert.AreEqual(arrowCount - 2, player.PlayerQuiver.Quantity);
			int? comboCost = player.Abilities[abilityIndex].ComboCost;
			int hitAmount = player.Abilities[abilityIndex].Offensive._Amount;
			Assert.AreEqual(monster.MaxHitPoints - (2 * hitAmount), monster.HitPoints);
			Assert.AreEqual(player.MaxComboPoints - (2 * comboCost), player.ComboPoints);
			string attackString = $"Your double shot hit the {monster.Name} for 25 physical damage.";
			Assert.AreEqual(attackString, OutputController.Display.Output[5][2]);
			Assert.AreEqual(attackString, OutputController.Display.Output[6][2]);
			player.MaxComboPoints = 25;
			player.ComboPoints = player.MaxComboPoints;
			monster.MaxHitPoints = 100;
			monster.HitPoints = monster.MaxHitPoints;
			arrowCount = player.PlayerQuiver.Quantity;
			player.UseAbility(monster, input);
			Assert.AreEqual(arrowCount - 1, player.PlayerQuiver.Quantity);
			Assert.AreEqual(player.MaxComboPoints - comboCost, player.ComboPoints);
			Assert.AreEqual(attackString, OutputController.Display.Output[7][2]);
			const string outOfComboString = "You didn't have enough combo points for the second shot!";
			Assert.AreEqual(outOfComboString, OutputController.Display.Output[8][2]);
		}
		[Test]
		public void BandageAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Archer) {
				MaxComboPoints = 100,
				ComboPoints = 100,
				MaxHitPoints = 100,
				HitPoints = 10
			};
			GearController.EquipInitialGear(player);
			player.Abilities.Add(new PlayerAbility(
				"bandage", 25, 1, PlayerAbility.ArcherAbility.Bandage, 2));
			OutputController.Display.ClearUserOutput();
			int abilityIndex = player.Abilities.FindIndex(
				f => f.ArcAbilityCategory == PlayerAbility.ArcherAbility.Bandage);
			string[] inputInfo = new[] { "ability", "bandage" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Bandage", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Combo Cost: 25", OutputController.Display.Output[2][2]);
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
			int? comboCost = player.Abilities[abilityIndex].ComboCost;
			Assert.AreEqual(player.MaxComboPoints - comboCost, player.ComboPoints);
			int healAmount = player.Abilities[abilityIndex].Healing._HealAmount;
			string healString = $"You heal yourself for {healAmount} health.";
			Assert.AreEqual(healString, OutputController.Display.Output[7][2]);
			Assert.AreEqual(true, player.Effects[0] is HealingEffect);
			Assert.AreEqual(baseHitPoints + healAmount, player.HitPoints);
			baseHitPoints = player.HitPoints;
			OutputController.Display.ClearUserOutput();
			for (int i = 2; i < 5; i++) {
				HealingEffect healEffect = player.Effects[0] as HealingEffect;
				healEffect.ProcessHealingRound(player);
				int healOverTimeAmt = healEffect.HealOverTimeAmount;
				string healAmtString = $"You have been healed for {healOverTimeAmt} health.";
				Assert.AreEqual(i, player.Effects[0].CurrentRound);
				Assert.AreEqual(healAmtString, OutputController.Display.Output[i - 2][2]);
				Assert.AreEqual(baseHitPoints + ((i - 1) * healOverTimeAmt), player.HitPoints);
			}
			GameController.RemovedExpiredEffectsAsync(player);
			Assert.AreEqual(false, player.Effects.Any());
		}
		[Test]
		public void SwiftAuraAbilityUnitTest() {
			OutputController.Display.ClearUserOutput();
			Player player = new Player("placeholder", Player.PlayerClassType.Archer);
			GearController.EquipInitialGear(player);
			OutputController.Display.ClearUserOutput();
			player.Abilities.Add(new PlayerAbility(
				"swift aura", 150, 1, PlayerAbility.ArcherAbility.SwiftAura, 6));
			string[] input = new[] { "use", "swift", "aura" };
			PlayerController.AbilityInfo(player, input);
			Assert.AreEqual("Swift Aura", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Combo Cost: 150", OutputController.Display.Output[2][2]);
			Assert.AreEqual("Swift Aura Amount: 15", OutputController.Display.Output[3][2]);
			Assert.AreEqual("Dexterity is increased by 15 for 10 minutes.", OutputController.Display.Output[4][2]);
			int baseDex = player.Dexterity;
			int? baseCombo = player.ComboPoints;
			int? baseMaxCombo = player.MaxComboPoints;
			int abilityIndex = player.Abilities.FindIndex(f => f.Name == InputController.ParseInput(input));
			player.UseAbility(input);
			Assert.AreEqual(player.Dexterity, baseDex + player.Abilities[abilityIndex].ChangeAmount._Amount);
			Assert.AreEqual(
				baseCombo - player.Abilities[abilityIndex].ComboCost, player.ComboPoints);
			Assert.AreEqual(
				player.MaxComboPoints, baseMaxCombo + (player.Abilities[abilityIndex].ChangeAmount._Amount * 10));
			Assert.AreEqual("You generate a Swift Aura around yourself.", OutputController.Display.Output[5][2]);
			ChangeStatEffect changeStatEffect = player.Effects[0] as ChangeStatEffect;
			for (int i = 1; i < 601; i++) {
				Assert.AreEqual(i, player.Effects[0].CurrentRound);
				changeStatEffect.ProcessChangeStatRound(player);
			}
			GameController.RemovedExpiredEffectsAsync(player);
			Thread.Sleep(1000);
			Assert.AreEqual(false, player.Effects.Any());
			Assert.AreEqual(baseDex, player.Dexterity);
			Assert.AreEqual(baseMaxCombo, player.MaxComboPoints);
			Assert.AreEqual(baseCombo - player.Abilities[abilityIndex].ComboCost, player.ComboPoints);
		}
		[Test]
		public void ImmolatingArrowAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Archer) {
				MaxComboPoints = 100,
				ComboPoints = 100,
				MaxHitPoints = 100,
				HitPoints = 10,
				InCombat = true
			};
			GearController.EquipInitialGear(player);
			OutputController.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { HitPoints = 100, MaxHitPoints = 100, InCombat = true, FireResistance = 0 };
			MonsterBuilder.BuildMonster(monster);
			player.PlayerWeapon._CritMultiplier = 1; // Remove crit chance to remove "noise" in test
			player.Abilities.Add(new PlayerAbility(
				"immolating arrow", 35, 1, PlayerAbility.ArcherAbility.ImmolatingArrow, 8));
			string[] input = new[] { "use", "immolating", "arrow" };
			PlayerController.AbilityInfo(player, input);
			Assert.AreEqual("Immolating Arrow", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Combo Cost: 35", OutputController.Display.Output[2][2]);
			Assert.AreEqual("Instant Damage: 25", OutputController.Display.Output[3][2]);
			Assert.AreEqual("Damage Over Time: 5", OutputController.Display.Output[4][2]);
			Assert.AreEqual("Fire damage over time for 3 rounds.", OutputController.Display.Output[5][2]);
			int arrowCount = player.PlayerQuiver.Quantity;
			player.UseAbility(monster, input);
			Assert.AreEqual(arrowCount - 1, player.PlayerQuiver.Quantity);
			int index = player.Abilities.FindIndex(
				f => f.ArcAbilityCategory == PlayerAbility.ArcherAbility.ImmolatingArrow);
			Assert.AreEqual(player.ComboPoints, player.MaxComboPoints - player.Abilities[index].ComboCost);
			string attackString = $"Your immolating arrow hit the {monster.Name} for 25 physical damage.";
			Assert.AreEqual(attackString, OutputController.Display.Output[6][2]);
			Assert.AreEqual(monster.HitPoints,
				monster.MaxHitPoints - player.Abilities[index].Offensive._Amount);
			OutputController.Display.ClearUserOutput();
			Assert.AreEqual(true, monster.Effects[0] is BurningEffect);
			Assert.AreEqual(3, monster.Effects[0].MaxRound);
			BurningEffect burnEffect = monster.Effects[0] as BurningEffect;
			for (int i = 0; i < 3; i++) {
				int baseHitPoints = monster.HitPoints;
				burnEffect.ProcessBurningRound(monster);
				Assert.AreEqual(i + 2, monster.Effects[0].CurrentRound);
				Assert.AreEqual(monster.HitPoints, baseHitPoints - burnEffect.FireDamageOverTime);
				string burnString = $"The {monster.Name} burns for {burnEffect.FireDamageOverTime} fire damage.";
				Assert.AreEqual(burnString, OutputController.Display.Output[i][2]);
				GameController.RemovedExpiredEffectsAsync(monster);
				Thread.Sleep(1000);
			}
			Assert.AreEqual(false, monster.Effects.Any());
		}
		[Test]
		public void AmbushAbilityUnitTest() {
			Player player = new Player("test", Player.PlayerClassType.Archer) {
				MaxComboPoints = 100,
				ComboPoints = 100,
				MaxHitPoints = 100,
				HitPoints = 10
			};
			GearController.EquipInitialGear(player);
			OutputController.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon) { HitPoints = 100, MaxHitPoints = 100 };
			MonsterBuilder.BuildMonster(monster);
			player.Abilities.Add(new PlayerAbility(
				"ambush", 75, 1, PlayerAbility.ArcherAbility.Ambush, 4));
			string[] inputInfo = new[] { "ability", "ambush" };
			PlayerController.AbilityInfo(player, inputInfo);
			Assert.AreEqual("Ambush", OutputController.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputController.Display.Output[1][2]);
			Assert.AreEqual("Combo Cost: 75", OutputController.Display.Output[2][2]);
			Assert.AreEqual("Instant Damage: 50", OutputController.Display.Output[3][2]);
			Assert.AreEqual("A surprise attack is launched, which initiates combat.",
				OutputController.Display.Output[4][2]);
			string[] input = new[] { "use", "ambush", monster.Name };
			player.InCombat = true;
			int arrowCount = player.PlayerQuiver.Quantity;
			player.UseAbility(monster, input);
			Assert.AreEqual($"You can't ambush {monster.Name}, you're already in combat!",
				OutputController.Display.Output[5][2]);
			player.InCombat = false;
			player.UseAbility(monster, input);
			int index = player.Abilities.FindIndex(
				f => f.ArcAbilityCategory == PlayerAbility.ArcherAbility.Ambush);
			int abilityDamage = player.Abilities[index].Offensive._Amount;
			string attackString = "Your ambush hit the " + monster.Name + " for " + abilityDamage + " physical damage.";
			Assert.AreEqual(arrowCount - 1, player.PlayerQuiver.Quantity);
			Assert.AreEqual(attackString, OutputController.Display.Output[6][2]);
			Assert.AreEqual(monster.HitPoints, monster.MaxHitPoints - abilityDamage);
		}
	}
}