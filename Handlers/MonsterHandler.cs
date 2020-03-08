﻿using System.Collections.Generic;

namespace DungeonGame {
	public static class MonsterHandler {
		public static void DisplayStats(Monster monster) {
			var opponentHealthString = "Opponent HP: " + monster.HitPoints + " / " + monster.MaxHitPoints;
			OutputHandler.Display.StoreUserOutput(
				Settings.FormatGeneralInfoText(),
				Settings.FormatDefaultBackground(),
				opponentHealthString);
			var healLineOutput = new List<string>();
			var hitPointMaxUnits = monster.MaxHitPoints / 10;
			var hitPointUnits = monster.HitPoints / hitPointMaxUnits;
			for (var i = 0; i < hitPointUnits; i++) {
				healLineOutput.Add(Settings.FormatGeneralInfoText());
				healLineOutput.Add(Settings.FormatHealthBackground());
				healLineOutput.Add("    ");
			}
			OutputHandler.Display.StoreUserOutput(healLineOutput);
			OutputHandler.Display.StoreUserOutput(
				Settings.FormatGeneralInfoText(),
				Settings.FormatDefaultBackground(),
				"==================================================");
		}
		public static int CheckArmorRating(Monster monster) {
			var totalArmorRating = 0;
			if (monster.MonsterChestArmor != null && monster.MonsterChestArmor.Equipped) {
				totalArmorRating += monster.MonsterChestArmor.ArmorRating;
			}
			if (monster.MonsterHeadArmor != null && monster.MonsterHeadArmor.Equipped) {
				totalArmorRating += monster.MonsterHeadArmor.ArmorRating;
			}
			if (monster.MonsterLegArmor != null && monster.MonsterLegArmor.Equipped) {
				totalArmorRating += monster.MonsterLegArmor.ArmorRating;
			}
			return totalArmorRating;
		}
	}
}