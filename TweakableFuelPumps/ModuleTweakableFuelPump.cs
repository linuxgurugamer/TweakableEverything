// TweakableFuelPumps, a TweakableEverything module
//
// ModuleTweakableFuelPump.cs
//
// Copyright © 2015, toadicus
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
using System.Text;
using KSP.Localization;
#if DEBUG
using ToadicusTools.DebugTools;
#endif
using ToadicusTools.Extensions;
using UnityEngine;

namespace TweakableFuelPumps
{
	#if DEBUG
	public class ModuleTweakableFuelPump : DebugPartModule
	#else
	public class ModuleTweakableFuelPump : PartModule
	#endif
	{
		[KSPField(isPersistant = false)]
		public string ResourceNames;

		private List<PartResource> resources;

		public override void OnAwake()
		{
			base.OnAwake();

			this.resources = new List<PartResource>();
		}

		public override void OnStart(StartState state)
		{
			base.OnStart(state);

			if (this.ResourceNames != string.Empty)
			{
				string[] names = this.ResourceNames.Split(',');

				string name;
				for (int idx = 0; idx < names.Length; idx++)
				{
					name = names[idx];

					string trimmedName = name.Trim();

					if (this.part.Resources.Contains(trimmedName))
					{
						this.resources.Add(this.part.Resources[trimmedName]);

						this.LogDebug(
							"Found resource {0} in part {1}; added to list.",
							trimmedName,
							this.part.partInfo.name
						);
					}
					else
					{
						this.LogWarning(
							"No resource named {0} found in part {1}; skipping.",
							name,
							this.part.partInfo.name
						);
					}
				}

				if (this.resources.Count > 0)
				{
					StringBuilder joinedResources = new StringBuilder();

					for (int rIdx = 0; rIdx < this.resources.Count; rIdx++)
					{
						if (joinedResources.Length > 0)
						{
							joinedResources.Append(", ");
						}

						joinedResources.Append(this.resources[rIdx].resourceName);
					}

					string pluralChar = this.resources.Count > 1 ? "s" : string.Empty;

					this.Actions["EnableFuelPumpAction"].guiName = Localizer.Format("#TweakableEverything_FuelPump_EnableFuelPump", joinedResources.ToString(), pluralChar);//string.Format("Enable {0} Pump{1}",,,)

					this.Actions["DisableFuelPumpAction"].guiName = Localizer.Format("#TweakableEverything_FuelPump_DisableFuelPump", joinedResources.ToString(), pluralChar);//string.Format("Disable {0} Pump{1}",,)

					this.Actions["ToggleFuelPumpAction"].guiName = Localizer.Format("#TweakableEverything_FuelPump_ToggleFuelPump", joinedResources.ToString(), pluralChar);//string.Format("Toggle {0} Pump{1}", "", , )
				}
			}
			else
			{
				this.LogWarning(
					"ResourceNames is empty for module on part {0}; this module will be useless.",
					this.part.partInfo.name
				);

				this.Actions["EnableFuelPumpAction"].active = false;
				this.Actions["DisableFuelPumpAction"].active = false;
				this.Actions["ToggleFuelPumpAction"].active = false;
			}
		}

		[KSPAction("Enable Fuel Pump")]
		public void EnableFuelPumpAction(KSPActionParam _)
		{
			if (this.resources == null)
			{
				return;
			}

			PartResource resource;
			for (int idx = 0; idx < this.resources.Count; idx++)
			{
				resource = this.resources[idx];

				if (resource == null)
				{
					continue;
				}

				resource.flowState = true;
			}
		}

		[KSPAction("Disable Fuel Pump")]
		public void DisableFuelPumpAction(KSPActionParam _)
		{
			if (this.resources == null)
			{
				return;
			}

			PartResource resource;
			for (int idx = 0; idx < this.resources.Count; idx++)
			{
				resource = this.resources[idx];
				if (resource == null)
				{
					continue;
				}

				resource.flowState = false;
			}
		}

		[KSPAction("Toggle Fuel Pump")]
		public void ToggleFuelPumpAction(KSPActionParam _)
		{
			if (this.resources == null)
			{
				return;
			}

			PartResource resource;
			for (int idx = 0; idx < this.resources.Count; idx++)
			{
				resource = this.resources[idx];
				if (resource == null)
				{
					continue;
				}

				resource.flowState = !resource.flowState;
			}
		}
	}
}

