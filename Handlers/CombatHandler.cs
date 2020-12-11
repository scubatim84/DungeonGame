﻿using System;
using System.Linq;

namespace DungeonGame
{
	public class CombatHandler
	{
		private string[] _Input { get; set; }
		private Monster _Opponent { get; set; }
		private Player _Player { get; set; }
		private bool _FleeSuccess { get; set; }

		public CombatHandler(Monster opponent, Player player)
		{
			_Player = player;
			_Opponent = opponent;
			_Player._InCombat = true;
			_Opponent._InCombat = true;
		}

		public void StartCombat()
		{
			Console.Clear();
			string fightStartString = $"{_Player._Name}, you have encountered a {_Opponent._Name}. Time to fight!";
			OutputHandler.Display.StoreUserOutput(
				Settings.FormatSuccessOutputText(),
				Settings.FormatDefaultBackground(),
				fightStartString);
			while (_Opponent._HitPoints > 0 && _Player._HitPoints > 0 &&
				   _Player._InCombat && _Opponent._InCombat)
			{
				GameHandler.RemovedExpiredEffectsAsync(_Player);
				GameHandler.RemovedExpiredEffectsAsync(_Opponent);
				bool isInputValid = false;
				// Get input and check to see if input is valid, and if not, keep trying to get input from user
				while (!isInputValid)
				{
					// Show initial output that announces start of fight
					OutputHandler.ShowUserOutput(_Player, _Opponent);
					OutputHandler.Display.ClearUserOutput();
					// Player will attack, use ability, cast spells, etc. to cause damage
					_Input = InputHandler.GetFormattedInput(Console.ReadLine());
					Console.Clear();
					isInputValid = ProcessPlayerInput();
				}
				if (_Player._Effects.Any())
				{
					ProcessPlayerEffects();
				}
				if (_FleeSuccess)
				{
					return;
				}
				// Check to see if player attack killed monster
				if (_Opponent._HitPoints <= 0)
				{
					_Opponent.MonsterDeath(_Player);
					return;
				}
				if (_Opponent._Effects.Any())
				{
					ProcessOpponentEffects();
				}
				// Check to see if damage over time effects killed monster
				if (_Opponent._HitPoints <= 0)
				{
					_Opponent.MonsterDeath(_Player);
					return;
				}
				if (_Opponent._IsStunned)
				{
					continue;
				}

				_Opponent.Attack(_Player);
				// Check at end of round to see if monster was killed by combat round
				if (_Opponent._HitPoints > 0)
				{
					continue;
				}

				_Opponent.MonsterDeath(_Player);
				return;
			}
		}
		private void FleeCombat()
		{
			int randomNum = GameHandler.GetRandomNumber(1, 10);
			if (randomNum > 5)
			{
				OutputHandler.Display.StoreUserOutput(
					Settings.FormatSuccessOutputText(),
					Settings.FormatDefaultBackground(),
					"You have fled combat successfully!");
				_Player._InCombat = false;
				_Opponent._InCombat = false;
				_FleeSuccess = true;
				IRoom playerRoom = RoomHandler.Rooms[_Player._PlayerLocation];
				int playerX = _Player._PlayerLocation._X;
				int playerY = _Player._PlayerLocation._Y;
				int playerZ = _Player._PlayerLocation._Z;
				if (playerRoom._Up != null)
				{
					Coordinate newCoord = new Coordinate(playerX, playerY, playerZ + 1);
					RoomHandler.ChangeRoom(_Player, newCoord);
					return;
				}
				if (playerRoom._East != null)
				{
					Coordinate newCoord = new Coordinate(playerX + 1, playerY, playerZ);
					RoomHandler.ChangeRoom(_Player, newCoord);
					return;
				}
				if (playerRoom._West != null)
				{
					Coordinate newCoord = new Coordinate(playerX - 1, playerY, playerZ);
					RoomHandler.ChangeRoom(_Player, newCoord);
					return;
				}
				if (playerRoom._North != null)
				{
					Coordinate newCoord = new Coordinate(playerX, playerY + 1, playerZ);
					RoomHandler.ChangeRoom(_Player, newCoord);
					return;
				}
				if (playerRoom._South != null)
				{
					Coordinate newCoord = new Coordinate(playerX, playerY - 1, playerZ);
					RoomHandler.ChangeRoom(_Player, newCoord);
					return;
				}
				if (playerRoom._NorthEast != null)
				{
					Coordinate newCoord = new Coordinate(playerX + 1, playerY + 1, playerZ);
					RoomHandler.ChangeRoom(_Player, newCoord);
					return;
				}
				if (playerRoom._NorthWest != null)
				{
					Coordinate newCoord = new Coordinate(playerX - 1, playerY + 1, playerZ);
					RoomHandler.ChangeRoom(_Player, newCoord);
					return;
				}
				if (playerRoom._SouthEast != null)
				{
					Coordinate newCoord = new Coordinate(playerX + 1, playerY - 1, playerZ);
					RoomHandler.ChangeRoom(_Player, newCoord);
					return;
				}
				if (playerRoom._SouthWest != null)
				{
					Coordinate newCoord = new Coordinate(playerX - 1, playerY - 1, playerZ);
					RoomHandler.ChangeRoom(_Player, newCoord);
					return;
				}
				if (playerRoom._Down != null)
				{
					Coordinate newCoord = new Coordinate(playerX, playerY, playerZ - 1);
					RoomHandler.ChangeRoom(_Player, newCoord);
					return;
				}
			}
			OutputHandler.Display.StoreUserOutput(
				Settings.FormatFailureOutputText(),
				Settings.FormatDefaultBackground(),
				"You tried to flee combat but failed!");
		}
		private void ProcessPlayerEffects()
		{
			foreach (Effect effect in _Player._Effects)
			{
				switch (effect._EffectGroup)
				{
					case Effect.EffectType.Healing:
						effect.HealingRound(_Player);
						break;
					case Effect.EffectType.ChangePlayerDamage:
						effect.ChangePlayerDamageRound(_Player);
						break;
					case Effect.EffectType.ChangeArmor:
						effect.ChangeArmorRound();
						break;
					case Effect.EffectType.OnFire:
						effect.OnFireRound(_Player);
						break;
					case Effect.EffectType.Bleeding:
						effect.BleedingRound(_Player);
						break;
					case Effect.EffectType.Stunned:
						break;
					case Effect.EffectType.Frozen:
						effect.FrozenRound(_Player);
						break;
					case Effect.EffectType.ChangeStat:
					case Effect.EffectType.ChangeOpponentDamage:
					case Effect.EffectType.BlockDamage:
					case Effect.EffectType.ReflectDamage:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
		private void ProcessOpponentEffects()
		{
			GameHandler.RemovedExpiredEffectsAsync(_Opponent);
			foreach (Effect effect in _Opponent._Effects)
			{
				switch (effect._EffectGroup)
				{
					case Effect.EffectType.Healing:
					case Effect.EffectType.ChangePlayerDamage:
					case Effect.EffectType.ChangeArmor:
					case Effect.EffectType.Frozen:
					case Effect.EffectType.ChangeOpponentDamage:
					case Effect.EffectType.ReflectDamage:
					case Effect.EffectType.ChangeStat:
					case Effect.EffectType.BlockDamage:
						break;
					case Effect.EffectType.OnFire:
						effect.OnFireRound(_Opponent);
						break;
					case Effect.EffectType.Bleeding:
						effect.BleedingRound(_Opponent);
						break;
					case Effect.EffectType.Stunned:
						effect.StunnedRound(_Opponent);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
		private bool ProcessPlayerInput()
		{
			switch (_Input[0])
			{
				case "f":
				case "fight":
					int attackDamage = _Player.PhysicalAttack(_Opponent);
					if (attackDamage - _Opponent.ArmorRating(_Player) <= 0)
					{
						string armorAbsorbString = $"The {_Opponent._Name}'s armor absorbed all of your attack!";
						OutputHandler.Display.StoreUserOutput(
							Settings.FormatAttackFailText(),
							Settings.FormatDefaultBackground(),
							armorAbsorbString);
					}
					else if (attackDamage == 0)
					{
						string attackFailString = $"You missed {_Opponent._Name}!";
						OutputHandler.Display.StoreUserOutput(
							Settings.FormatAttackFailText(),
							Settings.FormatDefaultBackground(),
							attackFailString);
					}
					else
					{
						int attackAmount = attackDamage - _Opponent.ArmorRating(_Player);
						string attackSucceedString = $"You hit the {_Opponent._Name} for {attackAmount} physical damage.";
						OutputHandler.Display.StoreUserOutput(
							Settings.FormatAttackSuccessText(),
							Settings.FormatDefaultBackground(),
							attackSucceedString);
						_Opponent._HitPoints -= attackAmount;
					}
					break;
				case "cast":
					try
					{
						if (_Input[1] != null)
						{
							string spellName = InputHandler.ParseInput(_Input);
							_Player.CastSpell(_Opponent, spellName);
						}
					}
					catch (IndexOutOfRangeException)
					{
						OutputHandler.Display.StoreUserOutput(
							Settings.FormatFailureOutputText(),
							Settings.FormatDefaultBackground(),
							"You don't have that spell.");
						return false;
					}
					catch (NullReferenceException)
					{
						if (_Player._PlayerClass != Player.PlayerClassType.Mage)
						{
							OutputHandler.Display.StoreUserOutput(
								Settings.FormatFailureOutputText(),
								Settings.FormatDefaultBackground(),
								"You can't cast spells. You're not a mage!");
							return false;
						}
					}
					catch (InvalidOperationException)
					{
						OutputHandler.Display.StoreUserOutput(
							Settings.FormatFailureOutputText(),
							Settings.FormatDefaultBackground(),
							_Player._PlayerClass != Player.PlayerClassType.Mage
								? "You can't cast spells. You're not a mage!"
								: "You do not have enough mana to cast that spell!");
						return false;
					}
					break;
				case "use":
					try
					{
						if (_Input[1] != null && _Input[1] != "bandage")
						{
							_Player.UseAbility(_Opponent, _Input);
						}
						if (_Input[1] != null && _Input[1] == "bandage")
						{
							_Player.UseAbility(_Input);
						}
					}
					catch (IndexOutOfRangeException)
					{
						OutputHandler.Display.StoreUserOutput(
							Settings.FormatFailureOutputText(),
							Settings.FormatDefaultBackground(),
							"You don't have that ability.");
						return false;
					}
					catch (ArgumentOutOfRangeException)
					{
						OutputHandler.Display.StoreUserOutput(
							Settings.FormatFailureOutputText(),
							Settings.FormatDefaultBackground(),
							"You don't have that ability.");
						return false;
					}
					catch (NullReferenceException)
					{
						if (_Player._PlayerClass == Player.PlayerClassType.Mage)
						{
							OutputHandler.Display.StoreUserOutput(
								Settings.FormatFailureOutputText(),
								Settings.FormatDefaultBackground(),
								"You can't use abilities. You're not a warrior or archer!");
							return false;
						}
					}
					catch (InvalidOperationException)
					{
						switch (_Player._PlayerClass)
						{
							case Player.PlayerClassType.Mage:
								OutputHandler.Display.StoreUserOutput(
									Settings.FormatFailureOutputText(),
									Settings.FormatDefaultBackground(),
									"You can't use abilities. You're not a warrior or archer!");
								break;
							case Player.PlayerClassType.Warrior:
								OutputHandler.Display.StoreUserOutput(
									Settings.FormatFailureOutputText(),
									Settings.FormatDefaultBackground(),
									"You do not have enough rage to use that ability!");
								break;
							case Player.PlayerClassType.Archer:
								OutputHandler.Display.StoreUserOutput(
									Settings.FormatFailureOutputText(),
									Settings.FormatDefaultBackground(),
									_Player._PlayerWeapon._WeaponGroup != Weapon.WeaponType.Bow
										? "You do not have a bow equipped!"
										: "You do not have enough combo points to use that ability!");
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
						return false;
					}
					break;
				case "equip":
				case "unequip":
					GearHandler.EquipItem(_Player, _Input);
					break;
				case "flee":
					FleeCombat();
					break;
				case "drink":
					if (_Input.Last() == "potion")
					{
						_Player.DrinkPotion(InputHandler.ParseInput(_Input));
					}
					else
					{
						OutputHandler.Display.StoreUserOutput(
							Settings.FormatFailureOutputText(),
							Settings.FormatDefaultBackground(),
							"You can't drink that!");
						return false;
					}
					break;
				case "reload":
					_Player.ReloadQuiver();
					break;
				case "i":
				case "inventory":
					PlayerHandler.ShowInventory(_Player);
					return false;
				case "list":
					switch (_Input[1])
					{
						case "abilities":
							try
							{
								PlayerHandler.ListAbilities(_Player);
							}
							catch (IndexOutOfRangeException)
							{
								OutputHandler.Display.StoreUserOutput(
									Settings.FormatFailureOutputText(),
									Settings.FormatDefaultBackground(),
									"List what?");
								return false;
							}
							break;
						case "spells":
							try
							{
								PlayerHandler.ListSpells(_Player);
							}
							catch (IndexOutOfRangeException)
							{
								OutputHandler.Display.StoreUserOutput(
									Settings.FormatFailureOutputText(),
									Settings.FormatDefaultBackground(),
									"List what?");
								return false;
							}
							break;
					}
					return false;
				case "ability":
					try
					{
						PlayerHandler.AbilityInfo(_Player, _Input);
					}
					catch (IndexOutOfRangeException)
					{
						OutputHandler.Display.StoreUserOutput(
							Settings.FormatFailureOutputText(),
							Settings.FormatDefaultBackground(),
							"What ability did you want to know about?");
					}
					catch (NullReferenceException)
					{
						if (_Player._PlayerClass == Player.PlayerClassType.Mage)
						{
							OutputHandler.Display.StoreUserOutput(
								Settings.FormatFailureOutputText(),
								Settings.FormatDefaultBackground(),
								"You can't use abilities. You're not a warrior or archer!");
						}
					}
					return false;
				case "spell":
					try
					{
						PlayerHandler.SpellInfo(_Player, _Input);
					}
					catch (IndexOutOfRangeException)
					{
						OutputHandler.Display.StoreUserOutput(
							Settings.FormatFailureOutputText(),
							Settings.FormatDefaultBackground(),
							"What spell did you want to know about?");
					}
					catch (NullReferenceException)
					{
						if (_Player._PlayerClass != Player.PlayerClassType.Mage)
						{
							OutputHandler.Display.StoreUserOutput(
								Settings.FormatFailureOutputText(),
								Settings.FormatDefaultBackground(),
								"You can't use spells. You're not a mage!");
						}
					}
					return false;
				default:
					Messages.InvalidCommand();
					return false;
			}
			return true;
		}
	}
}
