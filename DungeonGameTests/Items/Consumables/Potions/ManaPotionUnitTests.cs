﻿using DungeonGame.Helpers;
using DungeonGame.Items;
using DungeonGame.Items.Consumables.Potions;
using DungeonGame.Players;
using NUnit.Framework;
using System.Collections.Generic;

namespace DungeonGameTests.Items.Consumables.Potions {
	internal class ManaPotionUnitTests {
		private Player player;
		private ManaPotion potion;

		[SetUp]
		public void Setup() {
			potion = new ManaPotion(PotionStrength.Minor);
			player = new Player("test", PlayerClassType.Mage) {
				Inventory = new List<IItem>()
			};
		}

		[Test]
		public void PotionCreationTest() {
			Assert.AreEqual(1, potion.Weight);
		}

		[Test]
		public void MinorPotionCreationTest() {
			potion = new ManaPotion(PotionStrength.Minor);

			Assert.AreEqual("minor mana potion", potion.Name);
			Assert.AreEqual("A minor mana potion that restores 50 mana.", potion.Desc);
			Assert.AreEqual(50, potion.ManaAmount);
			Assert.AreEqual(25, potion.ItemValue);
		}

		[Test]
		public void NormalPotionCreationTest() {
			potion = new ManaPotion(PotionStrength.Normal);

			Assert.AreEqual("mana potion", potion.Name);
			Assert.AreEqual("A mana potion that restores 100 mana.", potion.Desc);
			Assert.AreEqual(100, potion.ManaAmount);
			Assert.AreEqual(50, potion.ItemValue);
		}

		[Test]
		public void GreaterPotionCreationTest() {
			potion = new ManaPotion(PotionStrength.Greater);

			Assert.AreEqual("greater mana potion", potion.Name);
			Assert.AreEqual("A greater mana potion that restores 150 mana.", potion.Desc);
			Assert.AreEqual(150, potion.ManaAmount);
			Assert.AreEqual(75, potion.ItemValue);
		}

		[Test]
		public void PlayerDrinkPotionFullManaTest() {
			potion = new ManaPotion(PotionStrength.Greater);  // Greater mana potion restores 150 mana
			player.Inventory.Add(potion);
			player.MaxManaPoints = 200;
			player.ManaPoints = 25;
			int? oldPlayerMana = player.ManaPoints;

			potion.DrinkPotion(player);

			Assert.AreEqual(oldPlayerMana + potion.ManaAmount, player.ManaPoints);
		}

		[Test]
		public void PlayerDrinkPotionPartialManaTest() {
			potion = new ManaPotion(PotionStrength.Greater);  // Greater mana potion restores 150 mana
			player.Inventory.Add(potion);
			player.MaxManaPoints = 200;
			player.ManaPoints = 100;

			potion.DrinkPotion(player);

			Assert.AreEqual(player.MaxManaPoints, player.ManaPoints);
		}

		[Test]
		public void PlayerDrinkPotionDisplayMessageTest() {
			OutputHelper.Display.ClearUserOutput();
			player.Inventory.Add(potion);
			string displayMessage = $"You drank a potion and replenished {potion.ManaAmount} mana.";

			potion.DrinkPotion(player);

			Assert.AreEqual(displayMessage, OutputHelper.Display.Output[0][2]);
		}
	}
}
