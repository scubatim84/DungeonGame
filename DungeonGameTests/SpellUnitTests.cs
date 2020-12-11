﻿using System.Linq;
using System.Threading;
using DungeonGame;
using NUnit.Framework;

namespace DungeonGameTests
{
	public class SpellUnitTests
	{
		[Test]
		public void FireballSpellUnitTest()
		{
			Player player = new Player("test", Player.PlayerClassType.Mage)
			{ _MaxManaPoints = 100, _ManaPoints = 100, _InCombat = true };
			GearHandler.EquipInitialGear(player);
			OutputHandler.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon)
			{ _HitPoints = 50, _MaxHitPoints = 100, _FireResistance = 0 };
			MonsterBuilder.BuildMonster(monster);
			player._PlayerWeapon._CritMultiplier = 1; // Remove crit chance to remove "noise" in test
			string[] inputInfo = new[] { "spell", "fireball" };
			int spellIndex = player._Spellbook.FindIndex(
				f => f._SpellCategory == PlayerSpell.SpellType.Fireball);
			PlayerHandler.SpellInfo(player, inputInfo);
			Assert.AreEqual("Fireball", OutputHandler.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputHandler.Display.Output[1][2]);
			Assert.AreEqual("Mana Cost: 35", OutputHandler.Display.Output[2][2]);
			Assert.AreEqual("Instant Damage: 25", OutputHandler.Display.Output[3][2]);
			Assert.AreEqual("Damage Over Time: 5", OutputHandler.Display.Output[4][2]);
			Assert.AreEqual("Fire damage over time will burn for 3 rounds.",
				OutputHandler.Display.Output[5][2]);
			OutputHandler.Display.ClearUserOutput();
			string[] input = new[] { "cast", "fireball" };
			string spellName = InputHandler.ParseInput(input);
			Assert.AreEqual("fireball", spellName);
			player.CastSpell(monster, spellName);
			Assert.AreEqual(player._MaxManaPoints - player._Spellbook[spellIndex]._ManaCost, player._ManaPoints);
			Assert.AreEqual(25, monster._HitPoints);
			Assert.AreEqual(3, monster._Effects[0]._EffectMaxRound);
			Assert.AreEqual($"You hit the {monster._Name} for {player._Spellbook[spellIndex]._Offensive._Amount} fire damage.",
				OutputHandler.Display.Output[0][2]);
			Assert.AreEqual($"The {monster._Name} bursts into flame!",
				OutputHandler.Display.Output[1][2]);
			for (int i = 2; i < 5; i++)
			{
				monster._Effects[0].OnFireRound(monster);
				Assert.AreEqual(
					$"The {monster._Name} burns for {monster._Effects[0]._EffectAmountOverTime} fire damage.",
					OutputHandler.Display.Output[i][2]);
				Assert.AreEqual(i, monster._Effects[0]._EffectCurRound);
				GameHandler.RemovedExpiredEffectsAsync(monster);
				Thread.Sleep(1000);
			}
			Assert.AreEqual(false, monster._Effects.Any());
			Assert.AreEqual(10, monster._HitPoints);
		}
		[Test]
		public void FrostboltSpellUnitTest()
		{
			Player player = new Player("test", Player.PlayerClassType.Mage) { _MaxManaPoints = 100, _ManaPoints = 100 };
			GearHandler.EquipInitialGear(player);
			OutputHandler.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon)
			{ _HitPoints = 100, _MaxHitPoints = 100, _FrostResistance = 0 };
			MonsterBuilder.BuildMonster(monster);
			foreach (IEquipment item in monster._MonsterItems.Where(item => item._Equipped))
			{
				item._Equipped = false;
			}
			string[] inputInfo = new[] { "spell", "frostbolt" };
			int spellIndex = player._Spellbook.FindIndex(
				f => f._SpellCategory == PlayerSpell.SpellType.Frostbolt);
			PlayerHandler.SpellInfo(player, inputInfo);
			Assert.AreEqual("Frostbolt", OutputHandler.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputHandler.Display.Output[1][2]);
			Assert.AreEqual("Mana Cost: 25", OutputHandler.Display.Output[2][2]);
			Assert.AreEqual("Instant Damage: 15", OutputHandler.Display.Output[3][2]);
			Assert.AreEqual("Frost damage will freeze opponent for 2 rounds.",
				OutputHandler.Display.Output[4][2]);
			Assert.AreEqual("Frozen opponents take 1.5x physical, arcane and frost damage.",
				OutputHandler.Display.Output[5][2]);
			OutputHandler.Display.ClearUserOutput();
			string[] input = new[] { "cast", "frostbolt" };
			string spellName = InputHandler.ParseInput(input);
			Assert.AreEqual("frostbolt", spellName);
			player._PlayerWeapon._Durability = 100;
			double baseDamage = player.PhysicalAttack(monster);
			player.CastSpell(monster, spellName);
			Assert.AreEqual(player._MaxManaPoints - player._Spellbook[spellIndex]._ManaCost, player._ManaPoints);
			Assert.AreEqual(85, monster._HitPoints);
			Assert.AreEqual(1, monster._Effects[0]._EffectCurRound);
			Assert.AreEqual(2, monster._Effects[0]._EffectMaxRound);
			string attackString = $"You hit the {monster._Name} for {player._Spellbook[spellIndex]._Offensive._Amount} frost damage.";
			Assert.AreEqual(attackString, OutputHandler.Display.Output[0][2]);
			string frozenString = $"The {monster._Name} is frozen. Physical, frost and arcane damage to it will be double!";
			Assert.AreEqual(frozenString, OutputHandler.Display.Output[1][2]);
			int monsterHitPointsBefore = monster._HitPoints;
			double totalBaseDamage = 0.0;
			double totalFrozenDamage = 0.0;
			double multiplier = monster._Effects[0]._EffectMultiplier;
			for (int i = 2; i < 4; i++)
			{
				monster._Effects[0].FrozenRound(monster);
				Assert.AreEqual(i, monster._Effects[0]._EffectCurRound);
				Assert.AreEqual(frozenString, OutputHandler.Display.Output[i][2]);
				player._PlayerWeapon._Durability = 100;
				double frozenDamage = player.PhysicalAttack(monster);
				monster._HitPoints -= (int)frozenDamage;
				totalBaseDamage += baseDamage;
				totalFrozenDamage += frozenDamage;
			}
			GameHandler.RemovedExpiredEffectsAsync(monster);
			Thread.Sleep(1000);
			Assert.AreEqual(false, monster._Effects.Any());
			int finalBaseDamageWithMod = (int)(totalBaseDamage * multiplier);
			int finalTotalFrozenDamage = (int)totalFrozenDamage;
			Assert.AreEqual(finalTotalFrozenDamage, finalBaseDamageWithMod, 7);
			Assert.AreEqual(monster._HitPoints, monsterHitPointsBefore - (int)totalFrozenDamage);
		}
		[Test]
		public void LightningSpellUnitTest()
		{
			Player player = new Player("test", Player.PlayerClassType.Mage) { _MaxManaPoints = 100, _ManaPoints = 100 };
			GearHandler.EquipInitialGear(player);
			OutputHandler.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon)
			{ _HitPoints = 100, _MaxHitPoints = 100, _ArcaneResistance = 0 };
			MonsterBuilder.BuildMonster(monster);
			foreach (IEquipment item in monster._MonsterItems.Where(item => item._Equipped))
			{
				item._Equipped = false;
			}
			string[] inputInfo = new[] { "spell", "lightning" };
			int spellIndex = player._Spellbook.FindIndex(
				f => f._SpellCategory == PlayerSpell.SpellType.Lightning);
			PlayerHandler.SpellInfo(player, inputInfo);
			Assert.AreEqual("Lightning", OutputHandler.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputHandler.Display.Output[1][2]);
			Assert.AreEqual("Mana Cost: 25", OutputHandler.Display.Output[2][2]);
			Assert.AreEqual("Instant Damage: 35", OutputHandler.Display.Output[3][2]);
			string[] input = new[] { "cast", "lightning" };
			string spellName = InputHandler.ParseInput(input);
			Assert.AreEqual("lightning", spellName);
			player._PlayerWeapon._Durability = 100;
			player.CastSpell(monster, spellName);
			Assert.AreEqual(player._MaxManaPoints - player._Spellbook[spellIndex]._ManaCost, player._ManaPoints);
			int arcaneSpellDamage = player._Spellbook[spellIndex]._Offensive._Amount;
			Assert.AreEqual(monster._HitPoints, monster._MaxHitPoints - arcaneSpellDamage);
			string attackSuccessString = $"You hit the {monster._Name} for {arcaneSpellDamage} arcane damage.";
			Assert.AreEqual(attackSuccessString, OutputHandler.Display.Output[4][2]);
		}
		[Test]
		public void HealSpellUnitTest()
		{
			Player player = new Player("test", Player.PlayerClassType.Mage)
			{ _MaxManaPoints = 100, _ManaPoints = 100, _MaxHitPoints = 100, _HitPoints = 50 };
			GearHandler.EquipInitialGear(player);
			OutputHandler.Display.ClearUserOutput();
			string[] inputInfo = new[] { "spell", "heal" };
			int spellIndex = player._Spellbook.FindIndex(
				f => f._SpellCategory == PlayerSpell.SpellType.Heal);
			PlayerHandler.SpellInfo(player, inputInfo);
			Assert.AreEqual("Heal", OutputHandler.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputHandler.Display.Output[1][2]);
			Assert.AreEqual("Mana Cost: 25", OutputHandler.Display.Output[2][2]);
			Assert.AreEqual("Heal Amount: 50", OutputHandler.Display.Output[3][2]);
			string[] input = new[] { "cast", "heal" };
			string spellName = InputHandler.ParseInput(input);
			Assert.AreEqual("heal", spellName);
			player.CastSpell(spellName);
			Assert.AreEqual(player._MaxManaPoints - player._Spellbook[spellIndex]._ManaCost, player._ManaPoints);
			string healString = $"You heal yourself for {player._Spellbook[spellIndex]._Healing._HealAmount} health.";
			Assert.AreEqual(healString, OutputHandler.Display.Output[4][2]);
			Assert.AreEqual(player._MaxHitPoints, player._HitPoints);
		}
		[Test]
		public void RejuvenateSpellUnitTest()
		{
			Player player = new Player("test", Player.PlayerClassType.Mage)
			{
				_MaxManaPoints = 100,
				_ManaPoints = 100,
				_MaxHitPoints = 100,
				_HitPoints = 50
			};
			GearHandler.EquipInitialGear(player);
			OutputHandler.Display.ClearUserOutput();
			string[] inputInfo = new[] { "spell", "rejuvenate" };
			int spellIndex = player._Spellbook.FindIndex(
				f => f._SpellCategory == PlayerSpell.SpellType.Rejuvenate);
			PlayerHandler.SpellInfo(player, inputInfo);
			Assert.AreEqual("Rejuvenate", OutputHandler.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputHandler.Display.Output[1][2]);
			Assert.AreEqual("Mana Cost: 25", OutputHandler.Display.Output[2][2]);
			Assert.AreEqual("Heal Amount: 20", OutputHandler.Display.Output[3][2]);
			Assert.AreEqual("Heal Over Time: 10", OutputHandler.Display.Output[4][2]);
			string healInfoString = $"Heal over time will restore health for {player._Spellbook[spellIndex]._Healing._HealMaxRounds} rounds.";
			Assert.AreEqual(healInfoString, OutputHandler.Display.Output[5][2]);
			OutputHandler.Display.ClearUserOutput();
			string[] input = new[] { "cast", "rejuvenate" };
			string spellName = InputHandler.ParseInput(input);
			Assert.AreEqual("rejuvenate", spellName);
			player.CastSpell(spellName);
			Assert.AreEqual(player._MaxManaPoints - player._Spellbook[spellIndex]._ManaCost, player._ManaPoints);
			Assert.AreEqual(70, player._HitPoints);
			string healString = $"You heal yourself for {player._Spellbook[spellIndex]._Healing._HealAmount} health.";
			Assert.AreEqual(healString, OutputHandler.Display.Output[0][2]);
			Assert.AreEqual(Effect.EffectType.Healing, player._Effects[0]._EffectGroup);
			for (int i = 2; i < 5; i++)
			{
				player._Effects[0].HealingRound(player);
				string healAmtString = $"You have been healed for {player._Effects[0]._EffectAmountOverTime} health.";
				Assert.AreEqual(i, player._Effects[0]._EffectCurRound);
				Assert.AreEqual(healAmtString, OutputHandler.Display.Output[i - 1][2]);
				Assert.AreEqual(70 + ((i - 1) * player._Effects[0]._EffectAmountOverTime), player._HitPoints);
			}
			GameHandler.RemovedExpiredEffectsAsync(player);
			Thread.Sleep(1000);
			Assert.AreEqual(false, player._Effects.Any());
		}
		[Test]
		public void DiamondskinSpellUnitTest()
		{
			Player player = new Player("test", Player.PlayerClassType.Mage) { _MaxManaPoints = 100, _ManaPoints = 100 };
			GearHandler.EquipInitialGear(player);
			OutputHandler.Display.ClearUserOutput();
			string[] inputInfo = new[] { "spell", "diamondskin" };
			int spellIndex = player._Spellbook.FindIndex(
				f => f._SpellCategory == PlayerSpell.SpellType.Diamondskin);
			PlayerHandler.SpellInfo(player, inputInfo);
			Assert.AreEqual("Diamondskin", OutputHandler.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputHandler.Display.Output[1][2]);
			Assert.AreEqual("Mana Cost: 25", OutputHandler.Display.Output[2][2]);
			Assert.AreEqual("Augment Armor Amount: 25", OutputHandler.Display.Output[3][2]);
			string augmentInfoString = $"Armor will be augmented for {player._Spellbook[spellIndex]._ChangeAmount._ChangeMaxRound} rounds.";
			Assert.AreEqual(augmentInfoString, OutputHandler.Display.Output[4][2]);
			string[] input = new[] { "cast", "diamondskin" };
			string spellName = InputHandler.ParseInput(input);
			Assert.AreEqual("diamondskin", spellName);
			int baseArmor = GearHandler.CheckArmorRating(player);
			player._InCombat = true;
			player.CastSpell(spellName);
			Assert.AreEqual(player._MaxManaPoints - player._Spellbook[spellIndex]._ManaCost, player._ManaPoints);
			string augmentString = $"You augmented your armor by {player._Spellbook[spellIndex]._ChangeAmount._Amount} with {player._Spellbook[spellIndex]._Name}.";
			Assert.AreEqual(augmentString, OutputHandler.Display.Output[5][2]);
			OutputHandler.Display.ClearUserOutput();
			Assert.AreEqual(true, player._Effects.Any());
			Assert.AreEqual(Effect.EffectType.ChangeArmor, player._Effects[0]._EffectGroup);
			for (int i = 2; i < 5; i++)
			{
				int augmentedArmor = GearHandler.CheckArmorRating(player);
				Assert.AreEqual(baseArmor + 25, augmentedArmor);
				player._Effects[0].ChangeArmorRound();
				string augmentRoundString = $"Your armor is increased by {player._Effects[0]._EffectAmountOverTime}.";
				Assert.AreEqual(augmentRoundString, OutputHandler.Display.Output[i - 2][2]);
			}
			GameHandler.RemovedExpiredEffectsAsync(player);
			Thread.Sleep(1000);
			Assert.AreEqual(false, player._Effects.Any());
		}
		[Test]
		public void TownPortalSpellUnitTest()
		{
			/* Town Portal should change location of player to where portal is set to, which is 0, 7, 0, town entrance */
			OutputHandler.Display.ClearUserOutput();
			Player player = new Player("test", Player.PlayerClassType.Mage) { _MaxManaPoints = 100, _ManaPoints = 100 };
			RoomHandler.Rooms = new RoomBuilder(100, 5, 0, 4, 0).RetrieveSpawnRooms();
			player._Spellbook.Add(new PlayerSpell(
				"town portal", 100, 1, PlayerSpell.SpellType.TownPortal, 2));
			string[] inputInfo = new[] { "spell", "town", "portal" };
			int spellIndex = player._Spellbook.FindIndex(
				f => f._SpellCategory == PlayerSpell.SpellType.TownPortal);
			PlayerHandler.SpellInfo(player, inputInfo);
			Assert.AreEqual("Town Portal", OutputHandler.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputHandler.Display.Output[1][2]);
			Assert.AreEqual("Mana Cost: 100", OutputHandler.Display.Output[2][2]);
			const string portalString = "This spell will create a portal and return you to town.";
			Assert.AreEqual(portalString, OutputHandler.Display.Output[3][2]);
			Coordinate newCoord = new Coordinate(-2, 6, 0);
			player._PlayerLocation = newCoord;
			Assert.AreEqual(-2, player._PlayerLocation._X);
			Assert.AreEqual(6, player._PlayerLocation._Y);
			Assert.AreEqual(0, player._PlayerLocation._Z);
			string[] input = new[] { "cast", "town", "portal" };
			string spellName = InputHandler.ParseInput(input);
			Assert.AreEqual("town portal", spellName);
			player.CastSpell(spellName);
			Assert.AreEqual(player._MaxManaPoints - player._Spellbook[spellIndex]._ManaCost, player._ManaPoints);
			Assert.AreEqual(0, player._PlayerLocation._X);
			Assert.AreEqual(7, player._PlayerLocation._Y);
			Assert.AreEqual(0, player._PlayerLocation._Z);
			Assert.AreEqual("You open a portal and step through it.", OutputHandler.Display.Output[4][2]);
		}
		[Test]
		public void ReflectDamageSpellUnitTest()
		{
			OutputHandler.Display.ClearUserOutput();
			Player player = new Player("test", Player.PlayerClassType.Mage) { _MaxManaPoints = 100, _ManaPoints = 100 };
			Monster monster = new Monster(3, Monster.MonsterType.Zombie)
			{ _HitPoints = 100, _MaxHitPoints = 100 };
			MonsterBuilder.BuildMonster(monster);
			foreach (IEquipment item in monster._MonsterItems.Where(item => item._Equipped))
			{
				item._Equipped = false;
			}
			player._Spellbook.Add(new PlayerSpell(
				"reflect", 100, 1, PlayerSpell.SpellType.Reflect, 4));
			string[] inputInfo = new[] { "spell", "reflect" };
			int spellIndex = player._Spellbook.FindIndex(
				f => f._SpellCategory == PlayerSpell.SpellType.Reflect);
			PlayerHandler.SpellInfo(player, inputInfo);
			Assert.AreEqual("Reflect", OutputHandler.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputHandler.Display.Output[1][2]);
			Assert.AreEqual("Mana Cost: 100", OutputHandler.Display.Output[2][2]);
			Assert.AreEqual("Reflect Damage Amount: 25", OutputHandler.Display.Output[3][2]);
			string reflectInfoString = $"Damage up to {player._Spellbook[spellIndex]._ChangeAmount._Amount} will be reflected for " +
				$"{player._Spellbook[spellIndex]._ChangeAmount._ChangeMaxRound} rounds.";
			Assert.AreEqual(reflectInfoString, OutputHandler.Display.Output[4][2]);
			string[] input = new[] { "cast", "reflect" };
			string spellName = InputHandler.ParseInput(input);
			Assert.AreEqual("reflect", spellName);
			player.CastSpell(spellName);
			Assert.AreEqual(player._MaxManaPoints - player._Spellbook[spellIndex]._ManaCost, player._ManaPoints);
			Assert.AreEqual("You create a shield around you that will reflect damage.",
				OutputHandler.Display.Output[5][2]);
			Assert.AreEqual(true, player._Effects.Any());
			Assert.AreEqual(Effect.EffectType.ReflectDamage, player._Effects[0]._EffectGroup);
			OutputHandler.Display.ClearUserOutput();
			for (int i = 2; i < 5; i++)
			{
				int attackDamageM = monster._MonsterWeapon.Attack();
				int index = player._Effects.FindIndex(
					f => f._EffectGroup == Effect.EffectType.ReflectDamage);
				int reflectAmount = player._Effects[index]._EffectAmountOverTime < attackDamageM ?
					player._Effects[index]._EffectAmountOverTime : attackDamageM;
				Assert.AreEqual(true, reflectAmount <= player._Effects[index]._EffectAmountOverTime);
				monster._HitPoints -= reflectAmount;
				Assert.AreEqual(monster._HitPoints, monster._MaxHitPoints - reflectAmount * (i - 1));
				player._Effects[index].ReflectDamageRound(reflectAmount);
				Assert.AreEqual(
					$"You reflected {reflectAmount} damage back at your opponent!",
					OutputHandler.Display.Output[i - 2][2]);
			}
			GameHandler.RemovedExpiredEffectsAsync(player);
			Thread.Sleep(1000);
			Assert.AreEqual(false, player._Effects.Any());
		}
		[Test]
		public void ArcaneIntellectSpellUnitTest()
		{
			OutputHandler.Display.ClearUserOutput();
			Player player = new Player("test", Player.PlayerClassType.Mage) { _MaxManaPoints = 150, _ManaPoints = 150 };
			player._Spellbook.Add(new PlayerSpell(
				"arcane intellect", 150, 1, PlayerSpell.SpellType.ArcaneIntellect, 6));
			string[] infoInput = new[] { "spell", "arcane", "intellect" };
			PlayerHandler.SpellInfo(player, infoInput);
			Assert.AreEqual("Arcane Intellect", OutputHandler.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputHandler.Display.Output[1][2]);
			Assert.AreEqual("Mana Cost: 150", OutputHandler.Display.Output[2][2]);
			Assert.AreEqual("Arcane Intellect Amount: 15", OutputHandler.Display.Output[3][2]);
			Assert.AreEqual("_Intelligence is increased by 15 for 10 minutes.",
				OutputHandler.Display.Output[4][2]);
			OutputHandler.Display.ClearUserOutput();
			int baseInt = player._Intelligence;
			int? baseMana = player._ManaPoints;
			int? baseMaxMana = player._MaxManaPoints;
			int spellIndex = player._Spellbook.FindIndex(
				f => f._SpellCategory == PlayerSpell.SpellType.ArcaneIntellect);
			string[] input = new[] { "cast", "arcane", "intellect" };
			string spellName = InputHandler.ParseInput(input);
			Assert.AreEqual("arcane intellect", spellName);
			player.CastSpell(spellName);
			Assert.AreEqual(baseMaxMana - player._Spellbook[spellIndex]._ManaCost, player._ManaPoints);
			Assert.AreEqual(player._Intelligence, baseInt + player._Spellbook[spellIndex]._ChangeAmount._Amount);
			Assert.AreEqual(
				baseMana - player._Spellbook[spellIndex]._ManaCost, player._ManaPoints);
			Assert.AreEqual(
				baseMaxMana + player._Spellbook[spellIndex]._ChangeAmount._Amount * 10, player._MaxManaPoints);
			Assert.AreEqual("You cast Arcane Intellect on yourself.", OutputHandler.Display.Output[0][2]);
			for (int i = 0; i < 10; i++)
			{
				player._Effects[0].ChangeStatRound();
			}
			UserOutput defaultEffectOutput = OutputHandler.ShowEffects(player);
			Assert.AreEqual("Player _Effects:", defaultEffectOutput.Output[0][2]);
			Assert.AreEqual(Settings.FormatGeneralInfoText(), defaultEffectOutput.Output[1][0]);
			Assert.AreEqual("(590 seconds) Arcane Intellect", defaultEffectOutput.Output[1][2]);
			for (int i = 0; i < 590; i++)
			{
				player._Effects[0].ChangeStatRound();
			}
			GameHandler.RemovedExpiredEffectsAsync(player);
			Thread.Sleep(1000);
			Assert.AreEqual(false, player._Effects.Any());
			Assert.AreEqual(baseInt, player._Intelligence);
			Assert.AreEqual(0, player._ManaPoints);
			Assert.AreEqual(baseMaxMana, player._MaxManaPoints);
			defaultEffectOutput = OutputHandler.ShowEffects(player);
			Assert.AreEqual("Player _Effects:", defaultEffectOutput.Output[0][2]);
			Assert.AreEqual(Settings.FormatInfoText(), defaultEffectOutput.Output[1][0]);
			Assert.AreEqual("None.", defaultEffectOutput.Output[1][2]);
		}
		[Test]
		public void FrostNovaSpellUnitTest()
		{
			Player player = new Player("test", Player.PlayerClassType.Mage) { _MaxManaPoints = 100, _ManaPoints = 100 };
			player._Spellbook.Add(new PlayerSpell(
				"frost nova", 40, 1, PlayerSpell.SpellType.FrostNova, 8));
			GearHandler.EquipInitialGear(player);
			OutputHandler.Display.ClearUserOutput();
			Monster monster = new Monster(3, Monster.MonsterType.Demon)
			{ _HitPoints = 100, _MaxHitPoints = 100, _FrostResistance = 0 };
			MonsterBuilder.BuildMonster(monster);
			foreach (IEquipment item in monster._MonsterItems.Where(item => item._Equipped))
			{
				item._Equipped = false;
			}
			int spellIndex = player._Spellbook.FindIndex(
				f => f._SpellCategory == PlayerSpell.SpellType.FrostNova);
			string[] infoInput = new[] { "spell", "frost", "nova" };
			PlayerHandler.SpellInfo(player, infoInput);
			Assert.AreEqual("Frost Nova", OutputHandler.Display.Output[0][2]);
			Assert.AreEqual("Rank: 1", OutputHandler.Display.Output[1][2]);
			Assert.AreEqual("Mana Cost: 40", OutputHandler.Display.Output[2][2]);
			Assert.AreEqual("Instant Damage: 15", OutputHandler.Display.Output[3][2]);
			Assert.AreEqual($"Frost damage will freeze opponent for {player._Spellbook[spellIndex]._Offensive._AmountMaxRounds} rounds.",
				OutputHandler.Display.Output[4][2]);
			Assert.AreEqual("Frozen opponents take 1.5x physical, arcane and frost damage.",
				OutputHandler.Display.Output[5][2]);
			Assert.AreEqual($"Opponent will be stunned for {player._Spellbook[spellIndex]._Offensive._AmountMaxRounds} rounds.",
				OutputHandler.Display.Output[6][2]);
			string[] input = new[] { "cast", "frost", "nova" };
			string spellName = InputHandler.ParseInput(input);
			Assert.AreEqual("frost nova", spellName);
			player._PlayerWeapon._Durability = 100;
			double baseDamage = player.PhysicalAttack(monster);
			player.CastSpell(monster, spellName);
			string attackSuccessString = $"You hit the {monster._Name} for {player._Spellbook[spellIndex]._Offensive._Amount} frost damage.";
			Assert.AreEqual(attackSuccessString, OutputHandler.Display.Output[7][2]);
			string frozenString = $"The {monster._Name} is frozen. Physical, frost and arcane damage to it will be double!";
			Assert.AreEqual(frozenString, OutputHandler.Display.Output[8][2]);
			OutputHandler.Display.ClearUserOutput();
			Assert.AreEqual(player._ManaPoints, player._MaxManaPoints - player._Spellbook[spellIndex]._ManaCost);
			int frostIndex = monster._Effects.FindIndex(
				f => f._EffectGroup == Effect.EffectType.Frozen);
			int stunIndex = monster._Effects.FindIndex(
				f => f._EffectGroup == Effect.EffectType.Stunned);
			Assert.AreEqual(85, monster._HitPoints);
			Assert.AreEqual(1, monster._Effects[frostIndex]._EffectCurRound);
			Assert.AreEqual(1, monster._Effects[stunIndex]._EffectCurRound);
			Assert.AreEqual(2, monster._Effects[frostIndex]._EffectMaxRound);
			Assert.AreEqual(2, monster._Effects[stunIndex]._EffectMaxRound);
			int monsterHitPointsBefore = monster._HitPoints;
			double totalBaseDamage = 0.0;
			double totalFrozenDamage = 0.0;
			double multiplier = monster._Effects[frostIndex]._EffectMultiplier;
			for (int i = 2; i < 4; i++)
			{
				OutputHandler.Display.ClearUserOutput();
				monster._Effects[stunIndex].StunnedRound(monster);
				string stunnedRoundString = $"The {monster._Name} is stunned and cannot attack.";
				Assert.AreEqual(stunnedRoundString, OutputHandler.Display.Output[0][2]);
				Assert.AreEqual(true, monster._IsStunned);
				Assert.AreEqual(i, monster._Effects[stunIndex]._EffectCurRound);
				player._PlayerWeapon._Durability = 100;
				double frozenDamage = player.PhysicalAttack(monster);
				Assert.AreEqual(i, monster._Effects[frostIndex]._EffectCurRound);
				string frozenRoundString = $"The {monster._Name} is frozen. Physical, frost and arcane damage to it will be double!";
				Assert.AreEqual(frozenRoundString, OutputHandler.Display.Output[1][2]);
				monster._HitPoints -= (int)frozenDamage;
				totalBaseDamage += baseDamage;
				totalFrozenDamage += frozenDamage;
			}
			GameHandler.RemovedExpiredEffectsAsync(monster);
			Thread.Sleep(1000);
			Assert.AreEqual(false, monster._Effects.Any());
			Assert.AreEqual(false, monster._IsStunned);
			int finalBaseDamageWithMod = (int)(totalBaseDamage * multiplier);
			int finalTotalFrozenDamage = (int)totalFrozenDamage;
			Assert.AreEqual(finalTotalFrozenDamage, finalBaseDamageWithMod, 7);
			Assert.AreEqual(monster._HitPoints, monsterHitPointsBefore - (int)totalFrozenDamage);
		}
	}
}