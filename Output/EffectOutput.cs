﻿using System.Globalization;
using System.Text;

namespace DungeonGame {
	public static class EffectOutput {
		public static UserOutput ShowEffects(Player player) {
			var effectUserOutput = new UserOutput();
			// Draw title to show for player effects
			effectUserOutput.StoreUserOutput(
				Settings.FormatGeneralInfoText(), 
				Settings.FormatDefaultBackground(), 
				"Player Effects:");
			var activeEffects = 0;
			var textInfo = new CultureInfo("en-US", false).TextInfo;
			string effectOutput;
			if (player.InCombat) {
				foreach (var effect in player.Effects) {
					if (effect.EffectMaxRound + 1 - effect.EffectCurRound > 1) {
						effectOutput = "(" + (effect.EffectMaxRound + 1 - effect.EffectCurRound) + " rounds) " + 
						               textInfo.ToTitleCase(effect.Name);
					}
					else {
						effectOutput = "(" + (effect.EffectMaxRound + 1 - effect.EffectCurRound) + " round) " + 
						               textInfo.ToTitleCase(effect.Name);
					}
					activeEffects++;
					effectUserOutput.StoreUserOutput(
						Settings.FormatGeneralInfoText(), 
						Settings.FormatDefaultBackground(), 
						effectOutput);
				}
				if (activeEffects == 0) {
					effectUserOutput.StoreUserOutput(
						Settings.FormatInfoText(),
						Settings.FormatDefaultBackground(),
						"None.");
				}
			}
			else {
				foreach (var effect in player.Effects) {
					if (effect.EffectMaxRound + 1 - effect.EffectCurRound > 1) {
						effectOutput = "(" + ((effect.EffectMaxRound + 1 - effect.EffectCurRound) * effect.TickDuration) + 
						               " seconds) " + textInfo.ToTitleCase(effect.Name);
					}
					else {
						effectOutput = "(" + ((effect.EffectMaxRound + 1 - effect.EffectCurRound) * effect.TickDuration) + 
						               " second) " + textInfo.ToTitleCase(effect.Name);
					}
					activeEffects++;
					effectUserOutput.StoreUserOutput(
						Settings.FormatGeneralInfoText(), 
						Settings.FormatDefaultBackground(), 
						effectOutput);
				}
				if (activeEffects == 0) {
					effectUserOutput.StoreUserOutput(
						Settings.FormatInfoText(),
						Settings.FormatDefaultBackground(),
						"None.");
				}
			}
			// Create bottom border for effects area
			var effectsBorder = new StringBuilder();
			// Effects border needs to extend same width as minimap itself in either direction
			for (var b = 0; b < Settings.GetMiniMapBorderWidth(); b++) {
				effectsBorder.Append("=");
			}
			effectUserOutput.StoreUserOutput(
				Settings.FormatGeneralInfoText(), 
				Settings.FormatDefaultBackground(), 
				effectsBorder.ToString());
			return effectUserOutput;
		}
	}
}