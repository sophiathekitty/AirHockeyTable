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
        // AirTable
        //----------------------------------------------------------------------
        public class AirTable : Screen
        {
            ScreenSprite centerLine;
            ScreenSprite centerRing;
            ScreenSprite leftRing;
            ScreenSprite rightRing;
            ScreenSprite leftCircle;
            ScreenSprite rightCircle;
            ScreenSprite leftGoal;
            ScreenSprite rightGoal;
            Paddle paddleLeft;
            Paddle paddleRight;
            public Puck puck;
            ScreenSprite leftWall;
            ScreenSprite rightWall;
            ScreenSprite leftTopWall;
            ScreenSprite rightTopWall;
            ScreenSprite leftBottomWall;
            ScreenSprite rightBottomWall;

            public Color leftColor = new Color(91, 207, 250);
            public Color rightColor = new Color(245, 171, 185);
            Color centerColor = new Color(100,100,100);

            IMySoundBlock tableSound;
            bool playerPresent { get { return paddleLeft.PlayerPresent || paddleRight.PlayerPresent; } }
            int musicLoopTime = 1000;
            int musicLoopCount = 0;
            Vector2 leftGoalPos { get { return new Vector2(Size.X * 0.2f, Size.Y * 0.5f); } }
            Vector2 rightGoalPos { get { return new Vector2(Size.X * 0.8f, Size.Y * 0.5f); } }
            string winner = "";
            // initialize the table
            public AirTable() : base(GridBlocks.GetTextSurface("table"))
            {
                tableSound = GridBlocks.GetSoundBlock("music");
                if (tableSound != null)
                {
                    // get the list of available sounds
                    List<string> sounds = new List<string>();
                    tableSound.GetSounds(sounds);
                    if (sounds.Contains("AirHockeyTable")) tableSound.SelectedSound = "AirHockeyTable";
                    else tableSound.SelectedSound = "MusHeavyFight_01";
                }
                BackgroundColor = new Color(203, 227, 202);
                centerLine = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.Center, Vector2.Zero,0f,new Vector2(10,1000), centerColor,"","SquareSimple",TextAlignment.CENTER,SpriteType.TEXTURE);
                centerRing = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.Center, Vector2.Zero, 0f, new Vector2(Size.Y*0.5f), centerColor, "", "CircleHollow", TextAlignment.CENTER, SpriteType.TEXTURE);
                leftRing = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterLeft, Vector2.Zero, 0f, new Vector2(Size.Y*0.7f), new Color(leftColor,0.6f), "", "CircleHollow", TextAlignment.CENTER, SpriteType.TEXTURE);
                rightRing = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterRight, Vector2.Zero, 0f, new Vector2(Size.Y * 0.7f), new Color(rightColor,0.6f), "", "CircleHollow", TextAlignment.CENTER, SpriteType.TEXTURE);
                leftCircle = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterLeft, Vector2.Zero, 0f, new Vector2(Size.Y * 0.5f), new Color(rightColor,0.75f), "", "Circle", TextAlignment.CENTER, SpriteType.TEXTURE);
                rightCircle = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterRight, Vector2.Zero, 0f, new Vector2(Size.Y * 0.5f), new Color(leftColor,0.75f), "", "Circle", TextAlignment.CENTER, SpriteType.TEXTURE);
                AddSprite(centerLine);
                AddSprite(centerRing);
                AddSprite(leftRing);
                AddSprite(rightRing);
                AddSprite(leftCircle);
                AddSprite(rightCircle);
                leftGoal = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterLeft, Vector2.Zero, 0f, new Vector2(Size.X * 0.2f, Size.Y * 0.25f), Color.Black, "", "SquareSimple", TextAlignment.CENTER, SpriteType.TEXTURE);
                rightGoal = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterRight, Vector2.Zero, 0f, new Vector2(Size.X * 0.2f, Size.Y * 0.25f), Color.Black, "", "SquareSimple", TextAlignment.CENTER, SpriteType.TEXTURE);
                AddSprite(leftGoal);
                AddSprite(rightGoal);
                // left color but lighter
                Color leftColorLight = new Color(leftColor.R + 50, leftColor.G + 50, leftColor.B + 50);
                // right color but lighter
                Color rightColorLight = new Color(rightColor.R + 50, rightColor.G + 50, rightColor.B + 50);
                puck = new Puck(new Vector2(Size.X * 0.1f, 0), new Vector2(Size.X * 0.9f, Size.Y));
                AddSprite(puck);
                paddleLeft = new Paddle(new GameInput(GridBlocks.GetPlayerLeft()), new Vector2(Size.X*0.1f, 0), new Vector2(Size.X*0.5f, Size.Y), leftColorLight, leftColor,true);
                paddleRight = new Paddle(new GameInput(GridBlocks.GetPlayerRight()), new Vector2(Size.X * 0.5f, 0), new Vector2(Size.X*0.9f, Size.Y), rightColorLight, rightColor, false);
                AddSprite(paddleLeft);
                AddSprite(paddleRight);
                leftWall = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterLeft, Vector2.Zero, 0f, new Vector2(Size.X * 0.10f, Size.Y), leftColor, "", "SquareSimple", TextAlignment.CENTER, SpriteType.TEXTURE);
                leftTopWall = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopLeft, Vector2.Zero, 0f, new Vector2(Size.X * 0.21f, Size.Y * 0.75f), leftColor, "", "SquareSimple", TextAlignment.CENTER, SpriteType.TEXTURE);
                leftBottomWall = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.BottomLeft, Vector2.Zero, 0f, new Vector2(Size.X * 0.21f, Size.Y * 0.75f), leftColor, "", "SquareSimple", TextAlignment.CENTER, SpriteType.TEXTURE);
                rightWall = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterRight, Vector2.Zero, 0f, new Vector2(Size.X * 0.10f, Size.Y), rightColor, "", "SquareSimple", TextAlignment.CENTER, SpriteType.TEXTURE);
                rightTopWall = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopRight, Vector2.Zero, 0f, new Vector2(Size.X * 0.21f, Size.Y * 0.75f), rightColor, "", "SquareSimple", TextAlignment.CENTER, SpriteType.TEXTURE);
                rightBottomWall = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.BottomRight, Vector2.Zero, 0f, new Vector2(Size.X * 0.21f, Size.Y * 0.75f), rightColor, "", "SquareSimple", TextAlignment.CENTER, SpriteType.TEXTURE);
                AddSprite(leftWall);
                AddSprite(rightWall);
                AddSprite(leftTopWall);
                AddSprite(rightTopWall);
                AddSprite(leftBottomWall);
                AddSprite(rightBottomWall);
                GridInfo.AddChangeListener("Winner", VarUpdated);
            }
            void VarUpdated(string key, string value)
            {
                if (key == "Winner")
                {
                    winner = value;
                    GridInfo.Echo("Winner: " + winner);
                }
            }
            // draw the table
            bool playing = false;
            public override void Draw()
            {
                if (playerPresent && winner == "")
                {
                    if (paddleLeft.PlayerPresent) paddleLeft.Update();
                    else paddleLeft.Update(puck.Position,leftGoalPos);
                    if (paddleRight.PlayerPresent) paddleRight.Update();
                    else paddleRight.Update(puck.Position,rightGoalPos);
                    // if the table sound is available and not playing, start it
                    if (tableSound != null && (!playing || musicLoopCount++ > musicLoopTime))
                    {
                        //GridInfo.Echo("Playing sound? ("+tableSound.SelectedSound+")");
                        tableSound.Play();
                        playing = true;
                        musicLoopCount = 0;
                    }
                }
                else
                {
                    puck.velocity *= 0.95f;
                    tableSound.Stop();
                    playing = false;
                }
                puck.Move();
                puck.Colide(paddleLeft);
                puck.Colide(paddleRight);
                base.Draw();
            }
        }
        //----------------------------------------------------------------------
    }
}
