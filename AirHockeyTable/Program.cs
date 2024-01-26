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
    //=======================================================================
    partial class Program : MyGridProgram
    {
        AirTable table;
        ScoreBoard scoreLeftSelf;
        ScoreBoard scoreLeftOther;
        ScoreBoard scoreRightSelf;
        ScoreBoard scoreRightOther;
        public Program()
        {
            Echo("Booting Air Hockey Table");
            GridBlocks.InitBlocks(GridTerminalSystem);
            GridInfo.Init("Air Hockey Table", GridTerminalSystem,IGC,Me,Echo);
            GridInfo.Load(Storage);
            table = new AirTable();
            scoreRightSelf = new ScoreBoard(GridBlocks.GetTextSurface("Right Score Self"), "RightScore", table.rightColor,"LeftScore");
            scoreRightOther = new ScoreBoard(GridBlocks.GetTextSurface("Right Score Other"), "RightScore", table.rightColor, "LeftScore");
            scoreLeftSelf = new ScoreBoard(GridBlocks.GetTextSurface("Left Score Self"), "LeftScore", table.leftColor,"RightScore");
            scoreLeftOther = new ScoreBoard(GridBlocks.GetTextSurface("Left Score Other"), "LeftScore", table.leftColor,"RightScore");
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            Echo("Air Hockey Table Ready");
        }

        public void Save()
        {
            Storage = GridInfo.Save();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if(argument.ToLower().Contains("reset"))
            {
                GridInfo.SetVar("LeftScore", "0");
                GridInfo.SetVar("RightScore", "0");
                GridInfo.SetVar("Winner", "");
                if(argument.ToLower().Contains("left"))
                {
                    table.puck.MovePuckToLeftStart();
                }
                else if(argument.ToLower().Contains("right"))
                {
                    table.puck.MovePuckToRightStart();
                }
            }
            table.Draw();
            scoreLeftSelf.Draw();
            scoreLeftOther.Draw();
            scoreRightSelf.Draw();
            scoreRightOther.Draw();
        }
    }
    //=======================================================================
}
