using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        //----------------------------------------------------------------------
        // GameInput
        //----------------------------------------------------------------------
        public class GameInput
        {
            IMyShipController controller;
            public GameInput(IMyShipController controller)
            {
                this.controller = controller;
            }
            // buttons pressed
            public bool W { get { return controller.MoveIndicator.Z < 0; } }
            public bool S { get { return controller.MoveIndicator.Z > 0; } }
            public bool A { get { return controller.MoveIndicator.X < 0; } }
            public bool D { get { return controller.MoveIndicator.X > 0; } }
            public bool Space { get { return controller.MoveIndicator.Y > 0; } }
            public bool C { get { return controller.MoveIndicator.Y < 0; } }
            public bool E { get { return controller.RollIndicator > 0; } }
            public bool Q { get { return controller.RollIndicator < 0; } }
            public bool LookLeft { get { return controller.RotationIndicator.Y < 0; } }
            public bool LookRight { get { return controller.RotationIndicator.Y > 0; } }
            public bool LookUp { get { return controller.RotationIndicator.X < 0; } }
            public bool LookDown { get { return controller.RotationIndicator.X > 0; } }
            public Vector2 WASD { get { return new Vector2(controller.MoveIndicator.X, controller.MoveIndicator.Z); } }
            public Vector2 Mouse { get { return new Vector2(controller.RotationIndicator.Y, controller.RotationIndicator.X); } }
            public bool PlayerPresent { get { return controller.IsUnderControl; } }
        }
        //----------------------------------------------------------------------
    }
}
