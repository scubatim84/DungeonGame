﻿using DungeonGame.Controllers;
using DungeonGame.Players;
using System;

namespace DungeonGame.Effects {
	public class ChangeMonsterDamageEffect : IEffect {
		public bool _IsEffectExpired { get; set; }
		public int _TickDuration { get; }
		public string _Name { get; set; }
		private readonly int _ChangeAmountOverTime;
		private int _CurrentRound;
		private readonly int _MaxRound;

		public ChangeMonsterDamageEffect(int tickDuration, string name, int changeAmountOverTime, int maxRound) {
			_TickDuration = tickDuration;
			_Name = name;
			_ChangeAmountOverTime = changeAmountOverTime;
			_CurrentRound = 1;
			_MaxRound = maxRound;
		}

		public void ProcessChangeMonsterDamageRound(Player player) {
			if (_IsEffectExpired || player._InCombat == false) {
				return;
			}

			IncrementCurrentRound();

			DisplayChangeDamageMessage();

			if (_CurrentRound > _MaxRound) {
				SetEffectAsExpired();
			}
		}

		private void IncrementCurrentRound() {
			_CurrentRound++;
		}

		private void DisplayChangeDamageMessage() {
			int changeAmount = Math.Abs(_ChangeAmountOverTime);

			string changeDmgString = _ChangeAmountOverTime > 0 ? $"Incoming damage is increased by {changeAmount}." :
				$"Incoming damage is decreased by {changeAmount}.";

			OutputController.Display.StoreUserOutput(
				Settings.FormatSuccessOutputText(),
				Settings.FormatDefaultBackground(),
				changeDmgString);
		}

		public void SetEffectAsExpired() {
			_IsEffectExpired = true;
		}
	}
}