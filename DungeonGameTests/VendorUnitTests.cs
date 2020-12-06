using System.Collections.Generic;
using System.Linq;
using DungeonGame;
using NUnit.Framework;

namespace DungeonGameTests
{
	public class VendorUnitTests
	{
		[Test]
		public void BuyItemUnitTest()
		{
			OutputHandler.Display.ClearUserOutput();
			var player = new Player("placeholder", Player.PlayerClassType.Mage);
			player._Gold = 100;
			var room = new TownRoom("test", "test",
				new Vendor("test", "test", Vendor.VendorType.Armorer));
			var input = new string[] { "buy", "helmet" };
			var inputName = InputHandler.ParseInput(input);
			int quantity;
			bool quantityProvided = int.TryParse(input.Last(), out quantity);
			if (!quantityProvided)
			{
				quantity = 1;
			}
			else
			{
				input = input.Take(input.Count() - 1).ToArray();
			}
			var baseGold = player._Gold;
			var index = room._Vendor._VendorItems.FindIndex(
				f => f._Name == inputName || f._Name.Contains(input.Last()));
			var buyItem = room._Vendor._VendorItems[index];
			room._Vendor.BuyItem(player, input, quantity);
			var purchaseString = "You purchased " + buyItem._Name + " from the vendor for " + buyItem.ItemValue + " gold.";
			Assert.AreEqual(purchaseString, OutputHandler.Display.Output[0][2]);
			var buyItemIndex = player._Inventory.FindIndex(
				f => f._Name == inputName || f._Name.Contains(input.Last()));
			Assert.AreNotEqual(-1, buyItemIndex);
			Assert.AreEqual(baseGold - buyItem.ItemValue, player._Gold);
		}
		[Test]
		public void BuySinglePotionUnitTest()
		{
			OutputHandler.Display.ClearUserOutput();
			var player = new Player("placeholder", Player.PlayerClassType.Mage);
			player._Gold = 100;
			var room = new TownRoom("test", "test",
				new Vendor("test", "test", Vendor.VendorType.Healer));
			var input = new string[] { "buy", "health", "potion" };
			var inputName = InputHandler.ParseInput(input);
			int quantity;
			bool quantityProvided = int.TryParse(input.Last(), out quantity);
			if (!quantityProvided)
			{
				quantity = 1;
			}
			else
			{
				input = input.Take(input.Count() - 1).ToArray();
			}
			var baseGold = player._Gold;
			var index = room._Vendor._VendorItems.FindIndex(
				f => f._Name == inputName || f._Name.Contains(inputName));
			var buyItem = room._Vendor._VendorItems[index];
			room._Vendor.BuyItem(player, input, quantity);
			var purchaseString = "You purchased " + buyItem._Name + " from the vendor for " + buyItem.ItemValue + " gold.";
			Assert.AreEqual(purchaseString, OutputHandler.Display.Output[0][2]);
			var buyItemIndex = player._Consumables.FindIndex(
				f => f._Name == inputName || f._Name.Contains(input.Last()));
			Assert.AreNotEqual(-1, buyItemIndex);
			Assert.AreEqual(baseGold - buyItem.ItemValue, player._Gold);
		}
		[Test]
		public void BuyMultiplePotionUnitTest()
		{
			OutputHandler.Display.ClearUserOutput();
			var player = new Player("placeholder", Player.PlayerClassType.Mage)
			{
				_Gold = 1000,
				_Consumables = new List<Consumable>()
			};
			var room = new TownRoom("test", "test",
				new Vendor("test", "test", Vendor.VendorType.Healer));
			var input = new string[] { "buy", "health", "potion", "5" };
			var quantityProvided = int.TryParse(input.Last(), out var quantity);
			if (!quantityProvided)
			{
				quantity = 1;
			}
			else
			{
				input = input.Take(input.Count() - 1).ToArray();
			}
			var inputName = InputHandler.ParseInput(input);
			Assert.AreEqual(5, quantity);
			var baseGold = player._Gold;
			var index = room._Vendor._VendorItems.FindIndex(
				f => f._Name == inputName || f._Name.Contains(inputName));
			var buyItem = room._Vendor._VendorItems[index];
			room._Vendor.BuyItem(player, input, quantity);
			var purchaseString = "You purchased " + buyItem._Name + " from the vendor for " + buyItem.ItemValue + " gold.";
			Assert.AreEqual(purchaseString, OutputHandler.Display.Output[0][2]);
			Assert.AreEqual(purchaseString, OutputHandler.Display.Output[1][2]);
			Assert.AreEqual(purchaseString, OutputHandler.Display.Output[2][2]);
			Assert.AreEqual(purchaseString, OutputHandler.Display.Output[3][2]);
			Assert.AreEqual(purchaseString, OutputHandler.Display.Output[4][2]);
			var buyItemIndex = player._Consumables.FindIndex(
				f => f._Name == inputName || f._Name.Contains(input.Last()));
			Assert.AreNotEqual(-1, buyItemIndex);
			var potionCount = player._Consumables.OfType<Consumable.PotionType>().Count();
			Assert.AreEqual(baseGold - buyItem.ItemValue * quantity, player._Gold);
		}
		[Test]
		public void SellItemUnitTest()
		{
			OutputHandler.Display.ClearUserOutput();
			var player = new Player("placeholder", Player.PlayerClassType.Mage) { _Gold = 100 };
			var room = new TownRoom("test", "test",
				new Vendor("test", "test", Vendor.VendorType.Armorer));
			var input = new[] { "sell", "cap" };
			var inputName = InputHandler.ParseInput(input);
			var index = player._Inventory.FindIndex(
				f => f._Name == inputName || f._Name.Contains(inputName));
			var sellItem = player._Inventory[index];
			var baseGold = player._Gold;
			room._Vendor.SellItem(player, input);
			var soldString = "You sold " + sellItem._Name + " to the vendor for " + sellItem.ItemValue + " gold.";
			Assert.AreEqual(soldString, OutputHandler.Display.Output[0][2]);
			var sellItemIndex = player._Inventory.FindIndex(
				f => f._Name == inputName || f._Name.Contains(inputName));
			Assert.AreEqual(-1, sellItemIndex);
			Assert.AreEqual(baseGold + sellItem.ItemValue, player._Gold);
		}
		[Test]
		public void SellMultipleItemsWithSameName()
		{
			OutputHandler.Display.ClearUserOutput();
			var player = new Player("placeholder", Player.PlayerClassType.Mage)
			{
				_Inventory = new List<IEquipment> {
					new Armor(1, Armor.ArmorSlot.Back),
					new Armor(1, Armor.ArmorSlot.Back)
				}
			};
			player._Inventory[0]._Name = "cape";
			player._Inventory[0].ItemValue = 10;
			player._Inventory[1]._Name = "cape";
			player._Inventory[1].ItemValue = 5;
			var room = new TownRoom("test", "test",
				new Vendor("test", "test", Vendor.VendorType.Armorer));
			var input = new[] { "sell", "cape", "2" };
			room._Vendor.SellItem(player, input);
			Assert.AreEqual(1, player._Inventory.Count);
			Assert.AreEqual(10, player._Inventory[0].ItemValue);
		}
	}
}