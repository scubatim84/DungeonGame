﻿using System;
using System.Runtime;

namespace DungeonGame {
	public class Quest {
		public enum QuestType {
			KillCount,
			KillMonster,
			ClearLevel
		}
		public string Name { get; set; }
		public string Dialogue { get; set; }
		public QuestType QuestCategory { get; set; }
		public int? CurrentKills { get; set; }
		public int? RequiredKills { get; set; }
		public Monster.MonsterType? MonsterKillType { get; set; }
		public bool QuestCompleted { get; set; }

		public Quest(string name, string dialogue, QuestType questCategory) {
			this.Name = name;
			this.Dialogue = dialogue;
			this.QuestCategory = questCategory;
			switch (this.QuestCategory) {
				case QuestType.KillCount:
					var desiredKills = GameHandler.GetRandomNumber(20, 30);
					this.CurrentKills = 0;
					this.RequiredKills = desiredKills;
					break;
				case QuestType.KillMonster:
					var desiredMonsterKills = GameHandler.GetRandomNumber(10, 20);
					this.CurrentKills = 0;
					this.RequiredKills = desiredMonsterKills;
					var randomNum = GameHandler.GetRandomNumber(1, 8);
					this.MonsterKillType = randomNum switch {
						1 => Monster.MonsterType.Demon,
						2 => Monster.MonsterType.Dragon,
						3 => Monster.MonsterType.Elemental,
						4 => Monster.MonsterType.Skeleton,
						5 => Monster.MonsterType.Spider,
						6 => Monster.MonsterType.Troll,
						7 => Monster.MonsterType.Vampire,
						8 => Monster.MonsterType.Zombie,
						_ => this.MonsterKillType
					};
					break;
				case QuestType.ClearLevel:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void CheckQuestCompleted() {
			switch (this.QuestCategory) {
				case QuestType.KillCount:
				case QuestType.KillMonster:
					if (this.CurrentKills >= this.RequiredKills) this.QuestCompleted = true;
					break;
				case QuestType.ClearLevel:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		public void UpdateQuestProgress(Monster.MonsterType monsterType) {
			switch (this.QuestCategory) {
				case QuestType.KillCount:
					this.CurrentKills++;
					break;
				case QuestType.KillMonster:
					if (this.MonsterKillType == monsterType) this.CurrentKills++;
					break;
				case QuestType.ClearLevel:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}