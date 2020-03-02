﻿using System;

namespace DungeonGame {
	public class Effect {
		public enum EffectType {
			Healing,
			ChangePlayerDamage,
			ChangeOpponentDamage,
			ChangeArmor,
			BlockDamage,
			OnFire,
			Bleeding,
			Stunned,
			ReflectDamage,
			Frozen,
			ChangeStat,
		}
		public string Name { get; set; }
		public EffectType EffectGroup { get; set; }
		public ChangeStat.StatType StatGroup { get; set; }
		public int EffectAmount { get; set; }
		public int EffectAmountOverTime { get; set; }
		public int EffectCurRound { get; set; }
		public int EffectMaxRound { get; set; }
		public double EffectMultiplier { get; set; }
		public bool IsEffectExpired { get; set; }
		public bool IsHarmful { get; set; }
		public int TickDuration { get; set; }

		// Default constructor for JSON serialization
		public Effect() {}
		public Effect(string name, EffectType effectGroup, int effectAmount, int tickDuration) {
			this.Name = name;
			this.EffectGroup = effectGroup;
			this.EffectAmount = effectAmount;
			this.TickDuration = tickDuration;
		}
		public Effect(string name, EffectType effectGroup, int effectCurRound, int effectMaxRound, 
			double effectMultiplier, int tickDuration, bool harmful) {
			this.Name = name;
			this.EffectGroup = effectGroup;
			this.EffectCurRound = effectCurRound;
			this.EffectMaxRound = effectMaxRound;
			this.TickDuration = tickDuration;
			this.EffectMultiplier = effectMultiplier;
			this.IsHarmful = harmful;
		}
		public Effect(string name, EffectType effectGroup, int effectAmountOverTime, int effectCurRound,
			int effectMaxRound, double effectMultiplier, int tickDuration, bool harmful)
			: this(name, effectGroup, effectCurRound, effectMaxRound, effectMultiplier, tickDuration, harmful) {
			this.EffectAmountOverTime = effectAmountOverTime;
		}
		public Effect(string name, EffectType effectGroup, int effectAmountOverTime, int effectCurRound,
			int effectMaxRound, double effectMultiplier, int tickDuration, bool harmful, ChangeStat.StatType statType)
			: this(name, effectGroup, effectAmountOverTime , effectCurRound, effectMaxRound, effectMultiplier, tickDuration, 
				harmful) {
			this.StatGroup = statType;
		}

		public void HealingRound(Player player) {
			if (this.IsEffectExpired) return;
			this.EffectCurRound += 1;
			player.HitPoints += this.EffectAmountOverTime;
			if (player.HitPoints > player.MaxHitPoints) player.HitPoints = player.MaxHitPoints;
			var healAmtString = "You have been healed for " + this.EffectAmountOverTime + " health."; 
			OutputHandler.Display.StoreUserOutput(
				Settings.FormatSuccessOutputText(),
				Settings.FormatDefaultBackground(),
				healAmtString);
			if (this.EffectCurRound <= this.EffectMaxRound) return;
			this.IsEffectExpired = true;
		}
		public void ChangeStatRound() {
			if (this.IsEffectExpired) return;
			this.EffectCurRound += 1;
			if (this.EffectCurRound <= this.EffectMaxRound) return;
			this.IsEffectExpired = true;
		}
		public void BlockDamageRound() {
			if (this.IsEffectExpired) return;
			const string blockString = "Your block effect is slowly fading away.";
			OutputHandler.Display.StoreUserOutput(
				Settings.FormatSuccessOutputText(),
				Settings.FormatDefaultBackground(),
				blockString);
			if (this.TickDuration > 0) return;
			const string blockEndString = "You are no longer blocking damage!";
			OutputHandler.Display.StoreUserOutput(
				Settings.FormatSuccessOutputText(),
				Settings.FormatDefaultBackground(),
				blockEndString);
			this.IsEffectExpired = true;
		}
		public void BlockDamageRound(int blockAmount) {
			if (this.IsEffectExpired) return;
			var blockString = "Your defensive move blocked " + blockAmount + " damage!";
			OutputHandler.Display.StoreUserOutput(
				Settings.FormatSuccessOutputText(),
				Settings.FormatDefaultBackground(),
				blockString);
			this.EffectAmount -= blockAmount;
			if (this.EffectAmount > 0) return;
			const string blockEndString = "You are no longer blocking damage!";
			OutputHandler.Display.StoreUserOutput(
				Settings.FormatSuccessOutputText(),
				Settings.FormatDefaultBackground(),
				blockEndString);
			this.IsEffectExpired = true;
		}
		public void ReflectDamageRound() {
			if (this.IsEffectExpired) return;
			this.EffectCurRound += 1;
			const string reflectString = "Your spell reflect is slowly fading away.";
			OutputHandler.Display.StoreUserOutput(
				Settings.FormatSuccessOutputText(),
				Settings.FormatDefaultBackground(),
				reflectString);
			if (this.EffectCurRound <= this.EffectMaxRound) return;
			this.IsEffectExpired = true;
		}
		public void ReflectDamageRound(int reflectAmount) {
			if (this.IsEffectExpired) return;
			this.EffectCurRound += 1;
			var reflectString = "You reflected " + reflectAmount + " damage back at your opponent!";
			OutputHandler.Display.StoreUserOutput(
				Settings.FormatSuccessOutputText(),
				Settings.FormatDefaultBackground(),
				reflectString);
			if (this.EffectCurRound <= this.EffectMaxRound) return;
			this.IsEffectExpired = true;
		}
		public void ChangeOpponentDamageRound(Player player) {
			if (this.IsEffectExpired || player.InCombat == false) return;
			this.EffectCurRound += 1;
			var changeDmgString = this.EffectAmountOverTime > 0 ?
				"Incoming damage is increased by " + this.EffectAmountOverTime + "."
				: "Incoming damage is decreased by " + this.EffectAmountOverTime + ".";
			OutputHandler.Display.StoreUserOutput(
				Settings.FormatSuccessOutputText(),
				Settings.FormatDefaultBackground(),
				changeDmgString);
			if (this.EffectCurRound <= this.EffectMaxRound) return;
			this.IsEffectExpired = true;
		}
		public void ChangePlayerDamageRound(Player player) {
			if (this.IsEffectExpired || player.InCombat == false) return;
			this.EffectCurRound += 1;
			var changeAmount = Math.Abs(this.EffectAmountOverTime);
			var changeDmgString = this.EffectAmountOverTime > 0 ?
				"Your damage is increased by " + changeAmount + "."
				: "Your damage is decreased by " + changeAmount + ".";
			OutputHandler.Display.StoreUserOutput(
				Settings.FormatSuccessOutputText(),
				Settings.FormatDefaultBackground(),
				changeDmgString);
			if (this.EffectCurRound <= this.EffectMaxRound) return;
			this.IsEffectExpired = true;
		}
		public void ChangeArmorRound() {
			if (this.IsEffectExpired) return;
			this.EffectCurRound += 1;
			var changeAmount = Math.Abs(this.EffectAmountOverTime);
			var changeArmorString = this.EffectAmountOverTime > 0 ?
				"Your armor is increased by " + changeAmount + "."
				: "Your armor is decreased by " + changeAmount + ".";
			OutputHandler.Display.StoreUserOutput(
				Settings.FormatSuccessOutputText(),
				Settings.FormatDefaultBackground(),
				changeArmorString);
			if (this.EffectCurRound <= this.EffectMaxRound) return;
			this.IsEffectExpired = true;
		}
		public void OnFireRound(Monster opponent) {
			if (this.IsEffectExpired) return;
			this.EffectCurRound += 1;
			opponent.HitPoints -= this.EffectAmountOverTime;
			var burnString = "The " + opponent.Name + " burns for " + this.EffectAmountOverTime + " fire damage.";
			OutputHandler.Display.StoreUserOutput(
				Settings.FormatOnFireText(),
				Settings.FormatDefaultBackground(),
				burnString);
			if (this.EffectCurRound <= this.EffectMaxRound) return;
			this.IsEffectExpired = true;
		}
		public void BleedingRound(Monster opponent) {
			if (this.IsEffectExpired) return;
			this.EffectCurRound += 1;
			opponent.HitPoints -= this.EffectAmountOverTime;
			var bleedString = "The " + opponent.Name + " bleeds for " + this.EffectAmountOverTime + " physical damage.";
			OutputHandler.Display.StoreUserOutput(
				Settings.FormatAttackSuccessText(),
				Settings.FormatDefaultBackground(),
				bleedString);
			if (this.EffectCurRound <= this.EffectMaxRound) return;
			this.IsEffectExpired = true;
		}
		public void StunnedRound(Monster opponent) {
			if (this.IsEffectExpired) return;
			opponent.IsStunned = true;
			this.EffectCurRound += 1;
			var stunnedString = "The " + opponent.Name + " is stunned and cannot attack.";
			OutputHandler.Display.StoreUserOutput(
				Settings.FormatAttackSuccessText(),
				Settings.FormatDefaultBackground(),
				stunnedString);
			if (this.EffectCurRound <= this.EffectMaxRound) return;
			this.IsEffectExpired = true;
		}
		public void FrozenRound(Monster opponent) {
			if (this.IsEffectExpired) return;
			this.EffectCurRound += 1;
			var frozenString = "The " + opponent.Name + " is frozen. Physical, frost and arcane damage to it will be double!";
			OutputHandler.Display.StoreUserOutput(
				Settings.FormatAttackSuccessText(),
				Settings.FormatDefaultBackground(),
				frozenString);
			if (this.EffectCurRound <= this.EffectMaxRound) return;
			this.IsEffectExpired = true;
		}
	}
}