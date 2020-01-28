// TweakableGimbals, a TweakableEverything module
//
// ModuleTweakableGimbal.cs
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

using KSP;
using System;
using System.Collections.Generic;
using ToadicusTools.Extensions;
using ToadicusTools.TweakableTools;
using UnityEngine;
#if DEBUG
using ToadicusTools.DebugTools;
#endif

namespace TweakableEverything
{
	#if DEBUG
	public class ModuleTweakableGimbal : DebugPartModule
	#else
	public class ModuleTweakableGimbal : PartModule
	#endif
	{
		protected ModuleGimbal gimbalModule;

#if false
        // Stores our tweaked value for gimbal range.
        [KSPField(isPersistant = true, guiName = "Gimbal Range", guiUnits = "°", guiFormat = "F1",
			guiActiveEditor = false)]
		[UI_FloatRange(minValue = float.MinValue, maxValue = float.MaxValue, stepIncrement = .1f)]
		public float gimbalRange;
#endif

		// Stores our tweaked value for control reversal.
		[KSPField(isPersistant = true, guiName = "#TweakableEverything_Gimbals_Control", guiActiveEditor = true, guiActive = true)]//Control
		[UI_Toggle(enabledText = "#TweakableEverything_Gimbals_Reversed", disabledText = "#TweakableEverything_Gimbals_Normal")]//ReversedNormal
		public bool reverseGimbalControl;
		// Stores the previous state of reverseGimbalControl.
		protected bool reverseControlState;

		[KSPField(isPersistant = false)]
		public float lowerMult;

		[KSPField(isPersistant = false)]
		public float upperMult;

		[KSPField(isPersistant = false)]
		public bool disableStockLimiter;

		public ModuleTweakableGimbal()
		{
#if false
			this.gimbalRange = -1f;
#endif
			this.reverseGimbalControl = false;
			this.lowerMult = 0f;
			this.upperMult = 3f;
			this.disableStockLimiter = true;
        }

        // Runs on PartModule startup.
        public override void OnStart(StartState state)
		{
            // Startup the PartModule stuff first.
            base.OnStart(state);

			// Set our state trackers to the opposite of our states, to force first-run updates.
//			this.startLockedState = !this.startLocked;

			// Fetch the gimbal module from the part.
			this.gimbalModule = base.part.getFirstModuleOfType<ModuleGimbal>();
            
			if (this.gimbalModule == null)
			{
				return;
			}


            //PartLoader.getPartInfoByName(base.part.partInfo.name).partPrefab.Modules
            /*.OfType<ModuleGimbal>()
            .FirstOrDefault()
            .gimbalRange*/


#if true
            ModuleGimbal gimbalPrefab;
			if (PartLoader.getPartInfoByName(base.part.partInfo.name).partPrefab.tryGetFirstModuleOfType(out gimbalPrefab))
			{
				// Initialize the gimbal range tweakable and value.
#if false
                	public static void InitializeTweakable<T>(
			UI_Control floatRange,
			ref float localField,
			ref float remoteField,
			float centerValue,
			float lowerMult,
			float upperMult,
			bool clobberEverywhere = false
		)
#endif
#if false
				TweakableTools.InitializeTweakable<ModuleTweakableGimbal>(
					this.Fields["gimbalRange"].uiControlCurrent(),
					ref this.gimbalRange,
					ref this.gimbalModule.gimbalRange,
					gimbalPrefab.gimbalRange,
					this.lowerMult,
					this.upperMult
                    ,true
				);
#endif
			}
#endif

				// If we're in flight mode...
				if (HighLogic.LoadedSceneIsFlight)
			{
				// ...and if our control state and gimbal range don't match...
				if (
					(this.reverseGimbalControl && this.gimbalModule.gimbalRange >= 0) ||
					(!this.reverseGimbalControl && this.gimbalModule.gimbalRange < 0)
				)
				{
					// ...toggle the reverse state.
					this.ToggleGimbalFlip();

					// ...and seed our last state.
					this.reverseControlState = this.reverseGimbalControl;
				}
			}
#if true
            if (this.disableStockLimiter)
			{
				this.gimbalModule.Fields["gimbalLimiter"].guiActive = false;
				this.gimbalModule.Fields["gimbalLimiter"].guiActiveEditor = false;
			}
#endif
		}

		public void LateUpdate()
		{
			if (this.gimbalModule == null)
			{
				return;
			}

			// If we're in flight mode...
			if (HighLogic.LoadedSceneIsFlight)
			{
				// ...and our gimbal control has changed...
				if (this.reverseControlState != this.reverseGimbalControl)
				{
					// ...set the last state
					this.reverseControlState = this.reverseGimbalControl;
					// ...and toggle the reverse state.
					this.ToggleGimbalFlip();
				}
			}
		}

		protected void ToggleGimbalFlip()
		{
			this.LogDebug("Reversing gimbal control.");

			// Literally just negate the gimbal range to change control state.
			this.gimbalModule.gimbalRange = -this.gimbalModule.gimbalRange;
            // Seed this in the persistence file.
            //this.gimbalRange = this.gimbalModule.gimbalRange;
            this.gimbalModule.flipYZ = !this.gimbalModule.flipYZ;

        }

		// Puts this in an action group.
		[KSPAction("Toggle Gimbal Control Flip")]
		public void ActionGimbalFlip(KSPActionParam p)
		{
			// Emulate a tweakble toggle.
			this.reverseGimbalControl = !this.reverseGimbalControl;
		}
	}
}

