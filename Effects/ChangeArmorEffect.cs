﻿using DungeonGame.Helpers;
using DungeonGame.Interfaces;
using System;

namespace DungeonGame.Effects {
	public class ChangeArmorEffect : IEffect {
		public int ChangeArmorAmount { get; }
		public int CurrentRound { get; private set; } = 1;
		public IEffectHolder EffectHolder { get; }
		public bool IsEffectExpired { get; set; }
		public bool IsHarmful { get; }
		public int MaxRound { get; }
		public string Name { get; }
		public int TickDuration { get; } = 10;

		public ChangeArmorEffect(string name, int maxRound, int changeArmorAmount) {
			Name = name;
			MaxRound = maxRound;
			ChangeArmorAmount = changeArmorAmount;
		}

		public void ProcessChangeArmorRound() {
			if (IsEffectExpired) {
				return;
			}

			IncrementCurrentRound();

			DisplayChangeArmorMessage();

			if (CurrentRound > MaxRound) {
				SetEffectAsExpired();
			}
		}

		private void IncrementCurrentRound() {
			CurrentRound++;
		}

		private void DisplayChangeArmorMessage() {
			int changeAmount = Math.Abs(ChangeArmorAmount);

			string changeArmorString = ChangeArmorAmount > 0 ? $"Your armor is increased by {changeAmount}." :
				$"Your armor is decreased by {changeAmount}.";

			OutputHelper.StoreSuccessMessage(changeArmorString);
		}

		public void SetEffectAsExpired() {
			IsEffectExpired = true;
		}

		public void ProcessRound() {
			throw new NotImplementedException();
		}
	}
}
