using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace DungeonGame {
	class MainClass {
		public static void Main(string[] args) {
			try {
				Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
			}
			catch (Exception ex) {
				RoomHandler.Display.StoreUserOutput(
					Settings.FormatGeneralInfoText(),
					Settings.FormatDefaultBackground(),
					"The game window could not be resized on your computer. Error: " + ex);
			}
			while (true) {
				// Game loading commands
				Messages.GameIntro();
				Player player;
				try {
					player = Newtonsoft.Json.JsonConvert.DeserializeObject<Player>(File.ReadAllText(
						"playersave.json"), new Newtonsoft.Json.JsonSerializerSettings {
						TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
						NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
					});
					RoomHandler.Rooms = Newtonsoft.Json.JsonConvert.DeserializeObject<List<IRoom>>(File.ReadAllText(
						"gamesave.json"), new Newtonsoft.Json.JsonSerializerSettings {
						TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
						NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
					});
					// Insert blank space before game reload info for formatting
					RoomHandler.Display.StoreUserOutput(
						Settings.FormatGeneralInfoText(),
						Settings.FormatDefaultBackground(),
						"");
					RoomHandler.Display.StoreUserOutput(
						Settings.FormatGeneralInfoText(), 
						Settings.FormatDefaultBackground(), 
						"Reloading your saved game.");
					// Insert blank space after game reload info for formatting
					RoomHandler.Display.StoreUserOutput(
						Settings.FormatGeneralInfoText(),
						Settings.FormatDefaultBackground(),
						"");
				}
				catch (FileNotFoundException) {
					// Create dungeon
					RoomHandler.Rooms = new RoomBuilder(
						100, 5, 1, 3, 
						0, 4, 0, RoomBuilder.StartDirection.Down).RetrieveSpawnRooms();
					var playerBuilder = new PlayerBuilder();
					player = playerBuilder.BuildNewPlayer();
					GearHandler.EquipInitialGear(player);
					// Begin game by putting player at coords 0, 7, 0, town entrance
					RoomHandler.SetPlayerLocation(player, 0, 7, 0);
					if (RoomHandler.RoomIndex == -1) throw new InvalidOperationException();
				}
				/* Set initial room condition for player
					On loading game, display room that player starts in */
				RoomHandler.ShowUserOutput(player);
				// Check every second to see if any effects expired or events need to execute
				var globalTimer = new Timer(
					e => GameHandler.CheckStatus(player), 
					null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
				while (!GameHandler.IsGameOver) {
					var input = RoomHandler.GetFormattedInput(Console.ReadLine());
					InputHandler.ProcessUserInput(player, input, globalTimer);
					Console.Clear();
					RoomHandler.ShowUserOutput(player);
				}
			}
		}
	}
}