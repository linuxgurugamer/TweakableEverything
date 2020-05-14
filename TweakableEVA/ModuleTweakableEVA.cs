// TweakableEVA, a TweakableEverything module
//
// ModuleTweakableEVA.cs
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
using UnityEngine;
#if DEBUG
using ToadicusTools.DebugTools;
#endif

namespace TweakableEVA
{
	#if DEBUG
	public class ModuleTweakableEVA : DebugPartModule
	#else
	public class ModuleTweakableEVA : PartModule
	#endif
	{
		[KSPField(guiName = "#TweakableEverything_EVA_ThrusterThrottle", guiFormat = "P0", guiActive = true, isPersistant = true)]//Thruster Throttle
		[UI_FloatRange(minValue = 0f, maxValue = 1f, stepIncrement = 0.05f, controlEnabled = true)]
		public float thrusterPowerThrottle;
		private float lastPowerThrottle;

		private KerbalEVA evaModule;

		private float origThrusterPower;
		private float origPropConsumption;

		public ModuleTweakableEVA()
		{
			this.thrusterPowerThrottle = 1f;

			this.LogDebug("Constructed.");
		}

		public override void OnAwake()
		{
			base.OnAwake();

			this.LogDebug("Awake.");
		}

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);

			this.LogDebug("Loaded.");
		}

		public void Update()
		{
			if (HighLogic.LoadedSceneIsFlight)
			{
				if (this.evaModule == null &&
				    this.part != null &&
				    this.part.tryGetFirstModuleOfType(out this.evaModule))
				{
					this.origThrusterPower = this.evaModule.linPower;
					this.origPropConsumption = this.evaModule.PropellantConsumption;
				}

				if (this.evaModule != null && this.lastPowerThrottle != this.thrusterPowerThrottle)
				{
					this.evaModule.linPower = this.origThrusterPower * this.thrusterPowerThrottle;
					this.evaModule.PropellantConsumption = this.origPropConsumption * this.thrusterPowerThrottle;

					this.lastPowerThrottle = this.thrusterPowerThrottle;
				}
			}
		}
	}
}

