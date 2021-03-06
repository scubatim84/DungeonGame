﻿using DungeonGame.Helpers;
using DungeonGame.Interfaces;
using DungeonGame.Players;
using System;

namespace DungeonGame.Effects {
	public class ChangePlayerDamageEffect : IEffect {
		public int CurrentRound { get; private set; } = 1;
		public IEffectHolder EffectHolder { get; }
		public bool IsEffectExpired { get; set; }
		public bool IsHarmful { get; }
		public int MaxRound { get; }
		public string Name { get; }
		public int TickDuration { get; } = 1;
		private readonly int _changeAmount;

		public ChangePlayerDamageEffect(string name, int maxRound, int changeAmount) {
			Name = name;
			MaxRound = maxRound;
			_changeAmount = changeAmount;
		}

		public void ProcessChangePlayerDamageRound(Player player) {
			if (IsEffectExpired || player.InCombat == false) {
				return;
			}

			IncrementCurrentRound();

			DisplayChangeDamageMessage();

			if (CurrentRound > MaxRound) {
				SetEffectAsExpired();
			}
		}

		private void IncrementCurrentRound() {
			CurrentRound++;
		}

		private void DisplayChangeDamageMessage() {
			int changeAmount = Math.Abs(_changeAmount);

			string changeDmgString = _changeAmount > 0 ? $"Your damage is increased by {changeAmount}." :
				$"Your damage is decreased by {changeAmount}.";

			OutputHelper.StoreSuccessMessage(changeDmgString);
		}

		public void SetEffectAsExpired() {
			IsEffectExpired = true;
		}

		public int GetUpdatedDamageFromChange(int attackAmount) {
			return attackAmount + _changeAmount;
		}

		public void ProcessRound() {
			throw new NotImplementedException();
		}
	}
}
