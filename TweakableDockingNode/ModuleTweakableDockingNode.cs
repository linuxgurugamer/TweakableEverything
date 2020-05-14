// TweakableDockingNode, a TweakableEverything module
//
// ModuleTweakableDockingNode.cs
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
using ToadicusTools.TweakableTools;
using KSP.Localization;

namespace TweakableEverything
{
	public class ModuleTweakableDockingNode : PartModule
	{
		/*
		 * Ctor
		 * Build ALL the objects.
		 * */
		public ModuleTweakableDockingNode() : base()
		{
			this.lastOpenState = false;
			this.AlwaysAllowStack = false;

			this.deployAnimationControllerName = string.Empty;
			this.TDNnodeName = string.Empty;

			this.acquireRange = -1;
			this.acquireForce = -1;
			this.acquireTorque = -1;
			this.undockEjectionForce = -1;
			this.minDistanceToReEngage = -1;

			this.maxCaptureRollAngle = 179.95f;
			this.maxAcquireRollAngle = 179.95f;
		}

		/*
		 * Fields
		 * */
		// Stores the ModuleDockingNode we're wrapping.
		protected ModuleDockingNode dockingNodeModule;

		// Field that references the animationName of the ModuleAnimateGeneric doing the animating.
		[KSPField(isPersistant = false)]
		public string deployAnimationControllerName;
		// Wrap the animation.
		protected ModuleAnimateGeneric deployAnimation = null;

		// String containing the name of the AttachNode that we will toggle.
		[KSPField(isPersistant = false)]
		public string TDNnodeName;
		// We will store our attachment node here.
		protected AttachNode attachNode;

		// Some parts need to leave stacking allowed all the time.
		[KSPField(isPersistant = false)]
		public bool AlwaysAllowStack;

		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false)]
		public float minCaptureRollDotProduct;

		/// <summary>
		/// Maximum roll angle of separation for docking capture, in degrees.
		/// </summary>
		[KSPField(
			isPersistant = false,
			guiName = "#TweakableEverything_DockingNode_MaxCaptureRollAngle",//Max Capture Roll Angle
			guiUnits = "°", guiFormat = "F0",
			guiActive = true, guiActiveEditor = true
		)]
		[UI_FloatRange(minValue = 0, maxValue = 180, stepIncrement = 5f, scene = UI_Scene.Editor)]
		public float maxCaptureRollAngle;

		public float lastMaxCaptureRollAngle;

		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false)]
		public float minAcquireRollDotProduct;

		/// <summary>
		/// Maximum roll angle of separation for docking acquisition, in degrees.
		/// </summary>
		[KSPField(
			isPersistant = false,
			guiName = "#TweakableEverything_DockingNode_MaxAcquireRollAngle",//Max Acquire Roll Angle
			guiUnits = "°", guiFormat = "F0",
			guiActive = true, guiActiveEditor = true
		)]
		[UI_FloatRange(minValue = 0, maxValue = 180, stepIncrement = 5f, scene = UI_Scene.Editor)]
		public float maxAcquireRollAngle;

		public float lastMaxAcquireRollAngle;

		// Stores the open/closed state of the shield.
		protected bool lastOpenState;

        /* @subclass
		private bool yieldedDecouple;*/

        [KSPField(isPersistant = true, guiName = "#TweakableEverything_DockingNode_UnfocusedRange", guiUnits = "m", guiFormat = "F2",//Unfocused Range
        guiActiveEditor = true, guiActive = false)]
        [UI_FloatRange(minValue = 200f, maxValue = 1000, stepIncrement = 1f)]
        public float unfocusedRange;


        [KSPField(isPersistant = true, guiName = "#TweakableEverything_DockingNode_NewAcquireRange", guiUnits = "m", guiFormat = "F2",//new Acquire Range
			guiActiveEditor = true, guiActive = false)]
//        [UI_FloatRange(minValue = -1f, maxValue = float.MaxValue, stepIncrement = 1f)]
        [UI_FloatRange(minValue = 0.2f, maxValue = 1f, stepIncrement = .1f)]

        public float acquireRange;

		[KSPField(isPersistant = true, guiName = "#TweakableEverything_DockingNode_AcquireForce", guiUnits = "kN", guiFormat = "F2",//Acquire Force
			guiActiveEditor = true, guiActive = false)]
		[UI_FloatRange(minValue = -1f, maxValue = float.MaxValue, stepIncrement = 1f)]
		public float acquireForce;

		[KSPField(isPersistant = true, guiName = "#TweakableEverything_DockingNode_AcquireTorque", guiUnits = "kN-m", guiFormat = "F2",//Acquire Torque
			guiActiveEditor = true, guiActive = false)]
		[UI_FloatRange(minValue = -1f, maxValue = float.MaxValue, stepIncrement = 1f)]
		public float acquireTorque;

		[KSPField(isPersistant = true, guiName = "#TweakableEverything_DockingNode_EjectionForce", guiUnits = "kN", guiFormat = "F2",//Ejection Force
			guiActiveEditor = true, guiActive = false)]
		[UI_FloatRange(minValue = -1f, maxValue = float.MaxValue, stepIncrement = 1f)]
		public float undockEjectionForce;

		[KSPField(isPersistant = true, guiName = "#TweakableEverything_DockingNode_ReengageDistance", guiUnits = "m", guiFormat = "F2",//Re-engage Distance
			guiActiveEditor = true, guiActive = false)]
		[UI_FloatRange(minValue = -1f, maxValue = float.MaxValue, stepIncrement = 1f)]
		public float minDistanceToReEngage;

		[KSPField(isPersistant = true)]
		protected bool isDecoupled;

        // protected bool stagingEnabled;
        // Gets the base part's fuelCrossFeed value.
        public bool partCrossFeed
		{
			get
			{
				return base.part.fuelCrossFeed;
			}
			set
			{
				base.part.fuelCrossFeed = value;
			}
		}

        /*
		 * Properties
		 * */
        // Get the part attached on the docking end of things.
        protected Part attachedPart
		{
			get
			{
                if (this.attachNode == null)
				{
					return null;
				}

				return this.attachNode.attachedPart;
			}
		}

        // Get the open/closed state of the shield.
        // commented out following line for 1.4, was causing errors during load
        //[KSPField(isPersistant = false, guiActiveEditor = true)]
        protected bool IsOpen
		{
			get
			{
				if (this.deployAnimation == null)
				{
					this.LogDebug("deployAnimation is null; open status falling back to true.");
					return true;
				}
				else
				{
					return (this.deployAnimation.animTime >= 1f - float.Epsilon * 2f);
				}
			}
		}


        /*
		 * Methods
		 * */
        // Runs when each new part is started.
        public override void OnStart(StartState st)
        {

            // Startup the PartModule stuff first.
            base.OnStart(st);

            if (!this.part.tryGetFirstModuleOfType<ModuleDockingNode>(out this.dockingNodeModule))
            {
                return;
            }
            if (this.dockingNodeModule.deployAnimationController != -1)
            {
                this.deployAnimation = (base.part.Modules.GetModule(this.dockingNodeModule.deployAnimationController) as ModuleAnimateGeneric);
            }
            else
            {
                this.deployAnimation = null;
                //  return;
            }

            //  this.deployAnimation = this.part.getFirstModuleOfType<ModuleAnimateGeneric>();

  
            ModuleDockingNode prefabModule;


            var p = PartLoader.getPartInfoByName(base.part.partInfo.name).partPrefab;
    
            p.tryGetFirstModuleOfType(out prefabModule);
   
            if (prefabModule == null)
            {
                Debug.Log("prefabModule is null");
                return;
            }
            if (this.Fields["acquireRange"].uiControlCurrent() == null)
            {
                Debug.Log("uiControlcurrent is null");
                return;
            }

#if true
            TweakableTools.InitializeTweakable<ModuleTweakableDockingNode>(
				this.Fields["acquireRange"].uiControlCurrent(),
				ref this.acquireRange,
				ref this.dockingNodeModule.acquireRange,
				prefabModule.acquireRange
			);

            UI_FloatRange floatRange = this.Fields["acquireRange"].uiControlCurrent() as UI_FloatRange;
            floatRange.minValue = 0.2f;

            TweakableTools.InitializeTweakable<ModuleTweakableDockingNode>(
				this.Fields["acquireForce"].uiControlCurrent(),
				ref this.acquireForce,
				ref this.dockingNodeModule.acquireForce,
				prefabModule.acquireForce
			);

			TweakableTools.InitializeTweakable<ModuleTweakableDockingNode>(
				this.Fields["acquireTorque"].uiControlCurrent(),
				ref this.acquireTorque,
				ref this.dockingNodeModule.acquireTorque,
				prefabModule.acquireForce
			);

			TweakableTools.InitializeTweakable<ModuleTweakableDockingNode>(
				this.Fields["undockEjectionForce"].uiControlCurrent(),
				ref this.undockEjectionForce,
				ref this.dockingNodeModule.undockEjectionForce,
				prefabModule.undockEjectionForce
			);

			TweakableTools.InitializeTweakable<ModuleTweakableDockingNode>(
				this.Fields["minDistanceToReEngage"].uiControlCurrent(),
				ref this.minDistanceToReEngage,
				ref this.dockingNodeModule.minDistanceToReEngage,
				prefabModule.minDistanceToReEngage
			);
#endif
#if true
            this.Fields["maxCaptureRollAngle"].uiControlFlight.controlEnabled = false;
			this.Fields["maxAcquireRollAngle"].uiControlFlight.controlEnabled = false;

			this.maxCaptureRollAngle = Mathf.Acos(this.minCaptureRollDotProduct) * 180f / Mathf.PI;
//			this.dockingNodeModule.captureMinRollDot = Mathf.Min(this.minCaptureRollDotProduct, 0.99995f);

			this.maxAcquireRollAngle = Mathf.Acos(this.minAcquireRollDotProduct) * 180f / Mathf.PI;
            //			this.dockingNodeModule.acquireMinRollDot = Mathf.Min(this.minAcquireRollDotProduct, 0.99995f);

#if DEBUG
            this.dockingNodeModule.Fields["acquireMinRollDot"].guiActive = true;
            this.dockingNodeModule.Fields["acquireMinRollDot"].guiActiveEditor = true;

            this.dockingNodeModule.Fields["captureMinRollDot"].guiActive = true;
            this.dockingNodeModule.Fields["captureMinRollDot"].guiActiveEditor = true;

            this.Fields["maxCaptureRollAngle"].guiActive = true;
            this.Fields["maxCaptureRollAngle"].guiActiveEditor = true;

            this.Fields["maxAcquireRollAngle"].guiActive = true;
            this.Fields["maxAcquireRollAngle"].guiActiveEditor = true;

            this.Fields["minCaptureRollDotProduct"].guiActive = true;
            this.Fields["minCaptureRollDotProduct"].guiActiveEditor = true;

            this.Fields["minAcquireRollDotProduct"].guiActive = true;
            this.Fields["minAcquireRollDotProduct"].guiActiveEditor = true;
#endif

            this.lastMaxCaptureRollAngle = this.maxCaptureRollAngle;
			this.lastMaxAcquireRollAngle = this.maxAcquireRollAngle;
#if true
            // If we have a tweakable AttachNode, use it.
            if (this.TDNnodeName != string.Empty)
			{
				this.attachNode = base.part.FindAttachNode(this.TDNnodeName);
			}

            base.part.attachRules.allowStack = this.IsOpen | this.AlwaysAllowStack;
#endif
			/* @subclass
			ModuleStagingToggle stagingToggleModule;

			if (this.part.tryGetFirstModuleOfType<ModuleStagingToggle>(out stagingToggleModule))
			{
				stagingToggleModule.OnToggle += new ModuleStagingToggle.ToggleEventHandler(this.OnStagingToggle);
			}
			*/
#endif
			// Yay debugging!
			this.LogDebug(
				"{0}: Started with assembly version {4}." +
                "\n\tdeployAnimationModule={1}, attachNode={2}, TDNnodeName={3}, attachedPart={5}, fuelCrossFeed={6}, AlwaysAllowStack={7} ",
				this.GetType().Name,
				this.deployAnimation,
				this.attachNode,
				this.TDNnodeName,
				this.GetType().Assembly.GetName().Version,
				this.attachedPart,
                base.part.fuelCrossFeed,
                this.AlwaysAllowStack
            );
		}

        // Runs every LateUpdate, because that's how Unity rolls.
        // We're running at LateUpdate to avoid hiding Update, since ModuleDockingNode's Update is private and we
        // can't call it.
#if true
        public void FixedUpdate()
		{
			// If we're in the Editor...
			if (HighLogic.LoadedSceneIsEditor)
			{
				// ...and if we have a deployAnimationModule...
				if (this.deployAnimation != null)
				{
                    // If the Opened state of the port has changed since last update and we have an attachNode...
                    if (this.attachNode != null && this.IsOpen != this.lastOpenState)
					{
						// ...set the last state to the current state
						this.lastOpenState = this.IsOpen;

						// ...and switch allowStack.
						base.part.attachRules.allowStack = this.IsOpen | this.AlwaysAllowStack;

						// Yay debugging!
						this.LogDebug(
							"{0}: IsOpen changed to: {1}, part contains node: {2}, allowStack: {3}",
							this.GetType().Name,
							this.IsOpen,
							base.part.attachNodes.Contains(this.attachNode),
							base.part.attachRules.allowStack
						);
					}

					// ...if the port is closed and the attachNode icon is active...
					if (this.attachNode != null && this.attachNode.icon != null)
					{
						this.attachNode.icon.SetActive(this.IsOpen);
					}
				}

                if (this.maxCaptureRollAngle != this.lastMaxCaptureRollAngle)
				{
					this.minCaptureRollDotProduct = Mathf.Cos(this.maxCaptureRollAngle * Mathf.PI / 180f);
#if false
                    this.dockingNodeModule.captureMinRollDot = this.minCaptureRollDotProduct;
#endif
					this.lastMaxCaptureRollAngle = this.maxCaptureRollAngle;
				}

                if (this.maxCaptureRollAngle > this.maxAcquireRollAngle)
				{
					this.maxAcquireRollAngle = this.maxCaptureRollAngle;
				}
                if (maxAcquireRollAngle == 180)
                    maxAcquireRollAngle = 179.95f;
                if (maxCaptureRollAngle == 180)
                    maxCaptureRollAngle = 179.95f;

  
                if (this.maxAcquireRollAngle != this.lastMaxAcquireRollAngle)
				{
					this.minAcquireRollDotProduct = Mathf.Cos(this.maxAcquireRollAngle * Mathf.PI / 180f);
					this.dockingNodeModule.acquireMinRollDot = this.minAcquireRollDotProduct;
					this.lastMaxAcquireRollAngle = this.maxAcquireRollAngle;
				}

                //dockingNodeModule.Events["SetAsTarget"].unfocusedRange = this.unfocusedRange;
                //dockingNodeModule.Events["UnsetAsTarget"].unfocusedRange = this.unfocusedRange;


            }

            // If we are in flight...
            if (HighLogic.LoadedSceneIsFlight)
			{
                // ...and if we have a deploy animation module and are ready...
                if (
					this.deployAnimation != null &&
					this.dockingNodeModule.state == Localizer.Format("#TweakableEverything_DockingNode_ReadyState")//"Ready"
				)
				{
					// ...and if we have an attached part...
					if (this.attachedPart != null)
					{
						// ...disable the deploy animation.
						this.deployAnimation.Events["Toggle"].active = false;
					}
					// ...otherwise...
					else
					{
						// ...enable the deploy animation.
						this.deployAnimation.Events["Toggle"].active = true;
					}
				}
			}
		}
#endif
                    /*
                     * Chopping out OnActive entirely pending reimplementation as a subclass.
                    public override void OnActive()
                    {
                        yieldedDecouple = false;

                        this.LogDebug("this.attachedPart={0}", this.attachedPart == null ? "null" : this.attachedPart.partInfo.title);

                        ModuleTweakableDockingNode attachedNode;
                        if (
                            this.attachedPart != null &&
                            PartIsStagingDockingPort(this.attachedPart, out attachedNode)
                        )
                        {
                            this.LogDebug("Attached part is staging docking port");

                            if (!attachedNode.yieldedDecouple)
                            {
                                this.LogDebug("Attached part has not yielded, yielding and returning.");
                                this.yieldedDecouple = true;
                                return;
                            }
                        }

                        this.LogDebug("OnActive");

                        base.OnActive();

                        if (this.stagingEnabled && this.dockingNodeModule != null)
                        {
                            switch (this.dockingNodeModule.state.ToLower())
                            {
                                case "preattached":
                                    this.dockingNodeModule.Decouple();
                                    break;
                                case "docked (dockee)":
                                case "docked (docker)":
                                case "docked (same vessel)":
                                    this.dockingNodeModule.Undock();
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    */

#if true
        [KSPAction("Control from Here")]
		public void MakeReferenceTransformAction(KSPActionParam param)
		{
			if (this.dockingNodeModule.Events["MakeReferenceTransform"].active)
			{
				this.dockingNodeModule.MakeReferenceTransform();
			}
		}

#endif
                    /*
                     * @subclass -- Chopping this out pending rewrite; should happen with Stock's logic.
                    protected void OnStagingToggle(object sender, ModuleStagingToggle.BoolArg arg)
                    {
                        this.LogDebug("OnStagingToggle called.");
                        this.stagingEnabled = arg.Value;
                    }*/
                }
}
