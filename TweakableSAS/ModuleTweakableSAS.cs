// TweakableSAS, a TweakableEverything module
//
// ModuleTweakableSAS.cs
//
// Copyright © 2014, toadicus
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation and/or other
//    materials provided with the distribution.
//
// 3. Neither the name of the copyright holder nor the names of its contributors may be used
//    to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Experience.Effects;
using KSP;
using System;
using System.Collections.Generic;
using ToadicusTools.DebugTools;
using ToadicusTools.Extensions;
using UnityEngine;

namespace TweakableEverything
{
	#if DEBUG
	public class ModuleTweakableSAS : DebugPartModule
	#else
	public class ModuleTweakableSAS : PartModule
	#endif
	{
		private static int maxSASServiceLevel = 0;
		private static List<AvailablePart> researchedSASParts = new List<AvailablePart>();
		private static bool researchedPartsLoaded = false;

		#region Fields
		[KSPField(isPersistant = true, guiName = "#TweakableEverything_SAS_Autopilot", guiFormat = "Level #0", guiActiveEditor = true)]//SAS Autopilot
		[UI_FloatRange(maxValue = 3, minValue = 0, stepIncrement = 1)]
		public float SASServiceLevel;

		private ModuleSAS sasModule;
		#endregion

		#region Properties
		#endregion

		#region PartModule Overrides
		public override void OnAwake()
		{
			base.OnAwake();

			GameEvents.onGameSceneLoadRequested.Add(this.GameSceneLoadHandler);
			GameEvents.OnPartPurchased.Add(this.PartPurchasedHandler);

			if (this.part.tryGetFirstModuleOfType<ModuleSAS>(out this.sasModule))
			{
				this.SASServiceLevel = this.sasModule.SASServiceLevel;
			}

			if (researchedPartsLoaded)
			{
				this.SASServiceLevel = maxSASServiceLevel;
				return;
			}

			if (HighLogic.CurrentGame == null || PartLoader.LoadedPartsList == null)
			{
				return;
			}

			switch (HighLogic.CurrentGame.Mode)
			{
				case Game.Modes.CAREER:
					this.Fields["SASServiceLevel"].guiActiveEditor = true;
					break;
				default:
					this.Fields["SASServiceLevel"].guiActiveEditor = false;

					Array apModes = Enum.GetValues(typeof(AutopilotSkill.Skills));
					int autopilotMode;
					for (int idx = 0; idx < apModes.Length; idx++)
					{
						try
						{
							autopilotMode = (int)apModes.GetValue(idx);

							maxSASServiceLevel = Math.Max(maxSASServiceLevel, autopilotMode);
						}
						catch
						{
							this.LogDebug(
								"Failed converting {0}.{1} to int.",
								typeof(AutopilotSkill.Skills).GetType().Name,
								Enum.GetName(typeof(AutopilotSkill.Skills), apModes.GetValue(idx))
							);
						}
					}

					researchedPartsLoaded = true;

					this.LogDebug("Sandbox mode: maxSASServiceLevel = {0}", maxSASServiceLevel);

					return;
			}

			this.LogDebug("Searching for researched parts with SAS modules...");

			using (var logger = PooledDebugLogger.New(this))
			{
				AvailablePart part;
				for (int idx = 0; idx < PartLoader.LoadedPartsList.Count; idx++)
				{
					part = PartLoader.LoadedPartsList[idx];

					logger.AppendFormat("Checking {0}...", part.title);

					if (ResearchAndDevelopment.PartTechAvailable(part))
					{
						logger.Append(" researched...");

						ModuleSAS sasModule;

						if (part.partPrefab.tryGetFirstModuleOfType(out sasModule))
						{
							logger.Append(" has SAS module, adding to list.");

							researchedSASParts.Add(part);

							maxSASServiceLevel = Math.Max(sasModule.SASServiceLevel, maxSASServiceLevel);

							logger.AppendFormat(" \n\tmaxSASServiceLevel = {0}.", maxSASServiceLevel);
						}
					}
					#if DEBUG
				else
				{
					logger.Append(" not researched!");
				}
					#endif

					logger.Append('\n');
				}

				logger.Append("Researched SAS parts loaded.");

				logger.Print();
			}

			researchedPartsLoaded = true;
		}

		public override void OnStart(StartState state)
		{
			base.OnStart(state);

			if (state == StartState.Editor)
			{
				UI_FloatRange sasTweak = this.Fields["SASServiceLevel"].uiControlEditor as UI_FloatRange;

				sasTweak.maxValue = maxSASServiceLevel;
			}

			this.SASServiceLevel = Math.Min(SASServiceLevel, maxSASServiceLevel);
		}
		#endregion

		#region MonoBehaviour Lifecycle Methods
		public void LateUpdate()
		{
			if (this.sasModule == null)
			{
				return;
			}

			if (this.sasModule.SASServiceLevel != this.SASServiceLevel)
			{
				this.LogDebug("Setting ModuleSAS service level to {1} (was {0}).",
					this.sasModule.SASServiceLevel, this.SASServiceLevel
				);

				this.sasModule.SASServiceLevel = (int)this.SASServiceLevel;
			}
		}

		public void OnDestroy()
		{
			GameEvents.onGameSceneLoadRequested.Remove(this.GameSceneLoadHandler);
			GameEvents.OnPartPurchased.Remove(this.PartPurchasedHandler);
		}
		#endregion

		#region Event Handlers
		private void GameSceneLoadHandler(GameScenes scene)
		{
			if (scene == GameScenes.MAINMENU)
			{
				this.LogDebug("Main menu loaded; resetting cache.");

				researchedPartsLoaded = false;
				maxSASServiceLevel = 0;
				researchedSASParts.Clear();
			}
		}

		private void PartPurchasedHandler(AvailablePart apart)
		{
			ModuleSAS module;

			if (apart.partPrefab.tryGetFirstModuleOfType<ModuleSAS>(out module))
			{
				this.LogDebug("Purchased new SAS part {0}: SASServiceLevel = {1} (old max = {2}).",
					apart.title, module.SASServiceLevel, maxSASServiceLevel
				);

				researchedSASParts.Add(apart);
				maxSASServiceLevel = Math.Max(maxSASServiceLevel, module.SASServiceLevel);
			}
		}
		#endregion
	}
}

