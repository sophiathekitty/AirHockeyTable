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
        // Puck
        //----------------------------------------------------------------------
        public class Puck : IScreenSpriteProvider
        {
            ScreenSprite BumperRing;
            ScreenSprite BumperHighlight;
            ScreenSprite CenterCircle;
            Color bumperColor = Color.Black;
            Vector2 minPos;
            Vector2 maxPos;
            float maxVelocity = 50f;
            float radius = 30;
            public Vector2 velocity;
            IMySoundBlock strikeSound;
            float goalSize = 0.25f;
            IMyLightingBlock leftVictoryLight;
            IMyLightingBlock rightVictoryLight;
            int victoryCountdown = 0;
            int leftScore = 0;
            int rightScore = 0;
            public bool Visible
            {
                get { return BumperRing.Visible; }
                set
                {
                    BumperRing.Visible = value;
                    BumperHighlight.Visible = value;
                    CenterCircle.Visible = value;
                }
            }
            public Puck(Vector2 minPos, Vector2 maxPos)
            {
                strikeSound = GridBlocks.GetSoundBlock("effect");
                if (strikeSound != null)
                {
                    // get the list of available sounds
                    List<string> sounds = new List<string>();
                    strikeSound.GetSounds(sounds);
                    if(sounds.Contains("PuckStrike")) strikeSound.SelectedSound = "PuckStrike";
                    else strikeSound.SelectedSound = "";
                }
                radius = GridInfo.GetVarAs("PuckRadius", radius);
                GridInfo.AddChangeListener("PuckRadius", VarUpdated);
                bumperColor = GridInfo.GetVarAs("PuckColor", bumperColor);
                GridInfo.AddChangeListener("PuckColor", VarUpdated);
                this.minPos = minPos;
                this.maxPos = maxPos;
                Vector2 center = (minPos + maxPos) / 2;
                center.X /= 2;
                BumperRing = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopLeft, center,0f,new Vector2(radius*2),bumperColor,"", "Circle",TextAlignment.CENTER,SpriteType.TEXTURE);
                BumperHighlight = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopLeft, center, 0f, new Vector2(radius * 1.5f), new Color(Color.White,0.25f), "", "SemiCircle", TextAlignment.CENTER, SpriteType.TEXTURE);
                CenterCircle = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopLeft, center, 0f, new Vector2(radius * 1f), bumperColor, "", "Circle", TextAlignment.CENTER, SpriteType.TEXTURE);
                leftVictoryLight = GridBlocks.GetLight("Strobe Left");
                rightVictoryLight = GridBlocks.GetLight("Strobe Right");
                if (leftVictoryLight != null) leftVictoryLight.Enabled = false;
                if(rightVictoryLight != null) rightVictoryLight.Enabled = false;
                leftScore = GridInfo.GetVarAs("LeftScore", leftScore);
                GridInfo.AddChangeListener("LeftScore", VarUpdated);
                rightScore = GridInfo.GetVarAs("RightScore", rightScore);
                GridInfo.AddChangeListener("RightScore", VarUpdated);
            }
            void VarUpdated(string key, string value)
            {
                switch (key)
                {
                    case "PuckRadius":
                        radius = GridInfo.GetVarAs("PuckRadius", radius);
                        BumperRing.Size = new Vector2(radius * 2);
                        BumperHighlight.Size = new Vector2(radius * 1.5f);
                        CenterCircle.Size = new Vector2(radius * 1f);
                        break;
                    case "PuckColor":
                        bumperColor = GridInfo.GetVarAs("PuckColor", bumperColor);
                        BumperRing.Color = bumperColor;
                        CenterCircle.Color = bumperColor;
                        break;
                    case "LeftScore":
                        leftScore = GridInfo.GetVarAs("LeftScore", leftScore);
                        break;
                    case "RightScore":
                        rightScore = GridInfo.GetVarAs("RightScore", rightScore);
                        break;
                }
            }
            public Vector2 Position
            {
                get { return BumperRing.Position; }
                set
                {
                    BumperRing.Position = value;
                    BumperHighlight.Position = value;
                    CenterCircle.Position = value;
                }
            }
            public void AddToScreen(Screen screen)
            {
                screen.AddSprite(BumperRing);
                screen.AddSprite(BumperHighlight);
                screen.AddSprite(CenterCircle);
            }
            public void RemoveToScreen(Screen screen) 
            {
                screen.RemoveSprite(BumperRing);
                screen.RemoveSprite(BumperHighlight);
                screen.RemoveSprite(CenterCircle);
            }
            float goalTop
            {
                get { return (maxPos.Y/2) - ((maxPos.Y*goalSize)/2); }
            }
            float goalBottom
            {
                get { return (maxPos.Y / 2) + ((maxPos.Y * goalSize) / 2); }
            }
            public void Move()
            {
                // if we are moving too fast, scale velocity to maxVelocity
                if (velocity.Length() > maxVelocity) velocity = Vector2.Normalize(velocity) * maxVelocity;
                if(Visible)
                {
                    Position += velocity;
                    // if we just hit a wall play a sound and reduce the velocity by 0.1%
                    if (Position.X < minPos.X + radius || Position.X > maxPos.X - radius || Position.Y < minPos.Y + radius || Position.Y > maxPos.Y - radius)
                    {
                        // if this is the goal (25% of y) then don't bounce off x walls
                        if (Position.Y > goalTop && Position.Y < goalBottom)
                        {
                            // in goal zone
                            //if (Position.X < minPos.X + radius || Position.X > maxPos.X - radius) velocity.X = -velocity.X;
                            //Goal();
                            // find how far outside the x range we are
                            float x = Math.Abs(MathHelper.Clamp(Position.X, minPos.X + radius, maxPos.X - radius));
                            GridInfo.Echo("Goal: " + goalTop + ", " + goalBottom + "... " + x);
                            if (x > radius / 2) Goal();
                            /*else
                            {
                                if (Position.Y < goalTop + radius) velocity.Y = Math.Abs(velocity.Y);
                                if (Position.Y > goalBottom - radius) velocity.Y = -Math.Abs(velocity.Y);
                            }*/
                        }
                        else
                        {
                            // bounce off x walls
                            if (Position.X < minPos.X + radius) velocity.X = Math.Abs(velocity.X) + 0.01f;
                            if (Position.X > maxPos.X - radius) velocity.X = -Math.Abs(velocity.X) - 0.01f;
                            if (Position.Y < minPos.Y + radius) velocity.Y = Math.Abs(velocity.Y) + 0.01f;
                            if (Position.Y > maxPos.Y - radius) velocity.Y = -Math.Abs(velocity.Y) - 0.01f;
                            Position = new Vector2(MathHelper.Clamp(Position.X, minPos.X + radius, maxPos.X - radius), MathHelper.Clamp(Position.Y, minPos.Y + radius, maxPos.Y - radius));
                            Strike();
                        }
                    }
                    velocity *= 0.999f;
                }
                if (victoryCountdown-- < 0)
                {
                    if (leftVictoryLight != null) leftVictoryLight.Enabled = false;
                    if (rightVictoryLight != null) rightVictoryLight.Enabled = false;
                    victoryCountdown = 0;
                    Visible = leftScore < 10 && rightScore < 10;
                }
            }
            public void Colide(Paddle paddle)
            {
                if (!Visible) return;
                // if we just hit a paddle play a sound and reduce the velocity by 0.1%
                if (Vector2.Distance(Position, paddle.Position) < radius + paddle.radius)
                {
                    Vector2 normal = Vector2.Normalize(Position - paddle.Position);
                    velocity = Vector2.Reflect(velocity, normal);
                    velocity += paddle.Velocity * 1.2f;
                    // make sure the puck is not inside the paddle
                    Position = paddle.Position + normal * (radius + paddle.radius);
                    Strike();
                }
               
            }
            void Strike()
            {
                if (strikeSound != null)
                {
                    if(strikeSound.SelectedSound != "") strikeSound.SelectedSound = "PuckStrike";
                    strikeSound.Play();
                }
                velocity *= 0.999f;
            }
            void Goal()
            {
                if (strikeSound != null)
                {
                    if (strikeSound.SelectedSound != "") strikeSound.SelectedSound = "RoundEnd";
                    strikeSound.Play();
                }
                Vector2 center = (minPos + maxPos) / 2;
                if (Position.X < maxPos.X / 2)
                {
                    // left goal
                    rightScore++;
                    GridInfo.SetVar("RightScore", rightScore.ToString());
                    // turn on right victory light
                    if (rightVictoryLight != null) rightVictoryLight.Enabled = true;
                    victoryCountdown = 100;
                    center.X /= 2;
                    if(rightScore >= 10) GridInfo.SetVar("Winner", "RightScore");
                }
                else
                {
                    // right goal
                    leftScore++;
                    GridInfo.SetVar("LeftScore", leftScore.ToString());
                    // turn on left victory light
                    if (leftVictoryLight != null) leftVictoryLight.Enabled = true;
                    victoryCountdown = 100;
                    center.X = center.X + (center.X / 2);
                    if (leftScore >= 10) GridInfo.SetVar("Winner", "LeftScore");
                }
                Position = center;
                Visible = false;
                velocity *= 0;
            }
            public void MovePuckToLeftStart()
            {
                Vector2 center = (minPos + maxPos) / 2;
                center.X /= 2;
                Position = center;
                Visible = true;
                velocity *= 0;
            }
            public void MovePuckToRightStart()
            {
                Vector2 center = (minPos + maxPos) / 2;
                center.X = center.X + (center.X / 2);
                Position = center;
                Visible = true;
                velocity *= 0;
            }
        }
        //----------------------------------------------------------------------
    }
}
