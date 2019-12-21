using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace DungeonGame {
  class MainClass {
    public static void Main(string[] args) {
      // Game loading commands
			Console.ForegroundColor = ConsoleColor.Gray;
      Helper.GameIntro();
			var player = new Player("placeholder");
			try {
				player = Newtonsoft.Json.JsonConvert.DeserializeObject<Player>(File.ReadAllText("savegame.json"), new Newtonsoft.Json.JsonSerializerSettings {
					TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
					NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
				});
			}
			catch(FileNotFoundException) {
				string playerName = Helper.FetchPlayerName();
				player.Name = playerName;
				player.EquipInitialGear();
			}
			var spawnedRooms = new SpawnRooms().RetrieveSpawnRooms();
			// Set initial room condition
			// On loading game, display room that player starts in
			// Begin game by putting player in room 100
			int roomIndex = Helper.ChangeRoom(spawnedRooms, player, 0, 0, 0);
      // While loop to continue obtaining input from player
      while (true) {
        player.DisplayPlayerStats();
        spawnedRooms[roomIndex].ShowCommands();
        string input = Helper.GetFormattedInput();
				var inputParse = input.Split(' ');
        // Obtain player command and process command
        switch (inputParse[0]) {
					case "a":
          case "attack":
					case "kill":
						try {
							if (inputParse[1] != null) {
								try {
									spawnedRooms[roomIndex].AttackOpponent(player, inputParse);
								}
								catch (Exception) {
									Console.WriteLine("An error has occurred while attacking.");
								}
							}
						}
						catch (IndexOutOfRangeException) {
							Console.WriteLine("You can't attack that.");
						}
            break;
					case "buy":
						try {
							if (inputParse[1] != null) {
								try {
									TownRoom isTownRoom = spawnedRooms[roomIndex] as TownRoom;
									if (isTownRoom != null) {
										isTownRoom.Vendor.BuyItemCheck(player, inputParse);
										}
									}
								catch (NullReferenceException) {
									Console.WriteLine("There is no vendor in the room to buy an item from.");
									}
								}
							}
						catch (IndexOutOfRangeException) {
							Console.WriteLine("Buy what?");
							}
						break;
					case "equip":
					case "unequip":
						player.EquipItem(inputParse);
						break;
          case "i":
					case "inventory":
						player.ShowInventory(player);
						break;
					case "q":
					case "quit":
						bool quitConfirm = Helper.QuitGame(player);
						if (quitConfirm == true) {
							return;
						}
						break;
					case "l":
					case "look":
						try {
							if (inputParse[1] != null) {
								try {
									spawnedRooms[roomIndex].LookNpc(inputParse);
								}
								catch (Exception) {
									Console.WriteLine("An error has occurred while looking.");
								}
							}
						}
						catch(IndexOutOfRangeException) {
							spawnedRooms[roomIndex].LookRoom();
						}
            break;
          case "loot":
						try {
							if (inputParse[1] != null) {
								try {
									spawnedRooms[roomIndex].LootCorpse(player, inputParse);
								}
								catch (Exception) {
									Console.WriteLine("An error has occurred while looting.");
								}
							}
						}
						catch (IndexOutOfRangeException) {
							Console.WriteLine("Loot what?");
						}
						break;
					case "drink":
						if (inputParse.Last() == "potion") {
							player.DrinkPotion(inputParse);
						}
						else {
							Console.WriteLine("You can't drink that!");
						}
						break;
					case "save":
						Helper.SaveGame(player);
						break;
					case "sell":
						try {
							if (inputParse[1] != null) {
								try {
									TownRoom isTownRoom = spawnedRooms[roomIndex] as TownRoom;
									if (isTownRoom != null) {
										isTownRoom.Vendor.SellItemCheck(player, inputParse);
									}
								}
								catch (NullReferenceException) {
									Console.WriteLine("The vendor doesn't want that.");
								}
							}
						}
						catch (IndexOutOfRangeException) {
							Console.WriteLine("Sell what?");
						}
						break;
					case "repair":
						try {
							if (inputParse[1] != null) {
								try {
									TownRoom isTownRoom = spawnedRooms[roomIndex] as TownRoom;
									if (isTownRoom != null) {
										isTownRoom.Vendor.RepairItem(player, inputParse);
									}
								}
								catch (NullReferenceException) {
									Console.WriteLine("The vendor doesn't repair that type of equipment.");
								}
							}
						}
						catch (IndexOutOfRangeException) {
							Console.WriteLine("Repair what?");
						}
						break;
					case "show":
						try {
							if (inputParse[1] == "forsale") {
								try {
									TownRoom isTownRoom = spawnedRooms[roomIndex] as TownRoom;
									if (isTownRoom != null) {
										isTownRoom.Vendor.DisplayGearForSale(player);
									}
								}
								catch (NullReferenceException) {
									Console.WriteLine("There is no vendor in the room to show inventory available for sale.");
								}
							}
						}
						catch (IndexOutOfRangeException) {
							Console.WriteLine("Show what?");
						}
						break;
					case "n":
					case "north":
            if(spawnedRooms[roomIndex].GoNorth) {
							try {
                roomIndex = Helper.ChangeRoom(spawnedRooms, player, 0, 1, 0);
							}
							catch(ArgumentOutOfRangeException) {
								Helper.InvalidDirection();
							}
						}
						else {
							Helper.InvalidDirection();
						}
						break;
					case "s":
					case "south":
						if(spawnedRooms[roomIndex].GoSouth) {
							try {
                roomIndex = Helper.ChangeRoom(spawnedRooms, player, 0, -1, 0);
              }
							catch(ArgumentOutOfRangeException) {
								Helper.InvalidDirection();
							}
						}
						else {
							Helper.InvalidDirection();
						}
						break;
					case "e":
					case "east":
						if (spawnedRooms[roomIndex].GoEast) {
							try {
								roomIndex = Helper.ChangeRoom(spawnedRooms, player, 1, 0, 0);
							}
							catch (ArgumentOutOfRangeException) {
								Helper.InvalidDirection();
							}
						}
						else {
							Helper.InvalidDirection();
						}
						break;
					case "w":
					case "west":
						if (spawnedRooms[roomIndex].GoWest) {
							try {
								roomIndex = Helper.ChangeRoom(spawnedRooms, player, -1, 0, 0);
							}
							catch (ArgumentOutOfRangeException) {
								Helper.InvalidDirection();
							}
						}
						else {
							Helper.InvalidDirection();
						}
						break;
					case "ne":
					case "northeast":
						if (spawnedRooms[roomIndex].GoNorthEast) {
							try {
								roomIndex = Helper.ChangeRoom(spawnedRooms, player, 1, 1, 0);
							}
							catch (ArgumentOutOfRangeException) {
								Helper.InvalidDirection();
							}
						}
						else {
							Helper.InvalidDirection();
						}
						break;
					case "nw":
					case "northwest":
						if (spawnedRooms[roomIndex].GoNorthWest) {
							try {
								roomIndex = Helper.ChangeRoom(spawnedRooms, player, -1, 1, 0);
							}
							catch (ArgumentOutOfRangeException) {
								Helper.InvalidDirection();
							}
						}
						else {
							Helper.InvalidDirection();
						}
						break;
					case "se":
					case "southeast":
						if (spawnedRooms[roomIndex].GoSouthEast) {
							try {
								roomIndex = Helper.ChangeRoom(spawnedRooms, player, 1, -1, 0);
							}
							catch (ArgumentOutOfRangeException) {
								Helper.InvalidDirection();
							}
						}
						else {
							Helper.InvalidDirection();
						}
						break;
					case "sw":
					case "southwest":
						if (spawnedRooms[roomIndex].GoSouthWest) {
							try {
								roomIndex = Helper.ChangeRoom(spawnedRooms, player, -1, -1, 0);
							}
							catch (ArgumentOutOfRangeException) {
								Helper.InvalidDirection();
							}
						}
						else {
							Helper.InvalidDirection();
						}
						break;
					case "u":
					case "up":
						if (spawnedRooms[roomIndex].GoUp) {
							try {
								roomIndex = Helper.ChangeRoom(spawnedRooms, player, 0, 0, 1);
							}
							catch (ArgumentOutOfRangeException) {
								Helper.InvalidDirection();
							}
						}
						else {
							Helper.InvalidDirection();
						}
						break;
					case "d":
					case "down":
						if (spawnedRooms[roomIndex].GoDown) {
							try {
								roomIndex = Helper.ChangeRoom(spawnedRooms, player, 0, 0, -1);
							}
							catch (ArgumentOutOfRangeException) {
								Helper.InvalidDirection();
							}
						}
						else {
							Helper.InvalidDirection();
						}
						break;
					default:
						Helper.InvalidCommand();
            break;
        }
      }
    }
  }
}