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
        public class Paddle : IScreenSpriteProvider
        {
            ScreenSprite BumperRing;
            ScreenSprite BumperHighlight;
            ScreenSprite HandleCircle;
            ScreenSprite HandleHighlight;
            ScreenSprite HandleDot;
            public Vector2 Position
            {
                get { return BumperRing.Position; }
                set
                {
                    BumperRing.Position = value;
                    BumperHighlight.Position = value;
                    HandleCircle.Position = value;
                    HandleHighlight.Position = value;
                    HandleDot.Position = value;
                }
            }
            public Vector2 Velocity
            {
                get { return Position - _lastPos; }
            }
            Vector2 _lastPos;
            public bool PlayerPresent { get { return input.PlayerPresent; } }
            GameInput input;
            Vector2 minPos;
            Vector2 maxPos;
            bool isLeft;
            public float radius = 30;
            public Paddle(GameInput input, Vector2 minPos, Vector2 maxPos, Color bumperColor, Color handleColor, bool isLeft)
            {
                this.input = input;
                this.minPos = minPos;
                this.maxPos = maxPos;
                this.isLeft = isLeft;
                
                // we need to calculate the center for the paddle starting position if left it should be 10% of the width from the min x.
                Vector2 center = new Vector2(minPos.X + (maxPos.X - minPos.X) * 0.2f, (minPos.Y + maxPos.Y) * 0.5f);
                if(!isLeft) center.X = maxPos.X - (maxPos.X - minPos.X) * 0.2f;
                
                _lastPos = center;
                BumperRing = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopLeft,center, 0f, new Vector2(radius*2, radius*2), bumperColor, "", "Circle", TextAlignment.CENTER, SpriteType.TEXTURE);
                BumperHighlight = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopLeft, center, 0.785398f, new Vector2(radius * 1.5f, radius*1.5f), new Color(255, 255, 255,0.1f), "", "SemiCircle", TextAlignment.CENTER, SpriteType.TEXTURE);
                HandleCircle = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopLeft, center, 0f, new Vector2(radius*1f, radius*1f), handleColor, "", "Circle", TextAlignment.CENTER, SpriteType.TEXTURE);
                HandleHighlight = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopLeft, center, 3.92699f, new Vector2(radius*0.9f, radius*0.9f), new Color(255, 255, 255, 0.1f), "", "SemiCircle", TextAlignment.CENTER, SpriteType.TEXTURE);
                HandleDot = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopLeft, center, 0f, new Vector2(radius*0.75f, radius*0.75f), handleColor, "", "Circle", TextAlignment.CENTER, SpriteType.TEXTURE);
            }
            public void Update()
            {
                _lastPos = Position;
                if (input.PlayerPresent)
                {
                    Vector2 newPos = Position;
                    Vector2 move = input.Mouse * 0.5f;
                    if (isLeft)
                    {
                        // we need to rotate the move vector by -90 degrees
                        move = new Vector2(-move.Y, move.X);
                    }
                    else
                    {
                        // we need to rotate the move vector by 90 degrees
                        move = new Vector2(move.Y, -move.X);
                    }
                    newPos += move;
                    if (newPos.X < minPos.X + radius) newPos.X = minPos.X + radius;
                    else if (newPos.X > maxPos.X - radius) newPos.X = maxPos.X - radius;
                    if (newPos.Y < minPos.Y + radius) newPos.Y = minPos.Y + radius;
                    else if (newPos.Y > maxPos.Y - radius) newPos.Y = maxPos.Y - radius;
                    Position = newPos;
                }
            }
            // AI paddle behavior
            Vector2 aiDefenseSpeed = new Vector2(0.05f);
            Vector2 aiAttackSpeed = new Vector2(0.3f);
            float aiAttackDistance = 20;
            public void Update(Vector2 puckPos, Vector2 defendPos)
            {
                //GridInfo.Echo("Paddle AI Update");
                // try to stay between the puck and the defend position
                Vector2 defTarget = (puckPos + defendPos) / 2;
                Vector2 newPos = Position;// + (defTarget - Position) * aiDefenseSpeed;
                // if between the puck and the defend position move towards the puck
                Vector2 move = AImoveMethod1(puckPos, defendPos);
                newPos += move;
                if (newPos.X < minPos.X + radius) newPos.X = minPos.X + radius;
                else if (newPos.X > maxPos.X - radius) newPos.X = maxPos.X - radius;
                if (newPos.Y < minPos.Y + radius) newPos.Y = minPos.Y + radius;
                else if (newPos.Y > maxPos.Y - radius) newPos.Y = maxPos.Y - radius;
                Position = newPos;
            }
            Vector2 aiTargetPos = Vector2.Zero;
            int aiReactTime = 0;
            Random random = new Random();
            Vector2 AImoveMethod1(Vector2 puckPos, Vector2 defensePos)
            {
                aiReactTime--;
                if (aiReactTime < 0)
                {
                    aiReactTime = random.Next(30,200);
                    if(random.Next(0,4) == 0) aiTargetPos = puckPos;
                    else aiTargetPos = new Vector2(random.Next((int)minPos.X,(int)maxPos.X),random.Next((int)minPos.Y,(int)maxPos.Y));
                }
                Vector2 move = Vector2.Zero;
                // if Position.X is closer to the defensePos.X than the puckPos.X then move towards the puck.Y at defenseSpeed for Y.
                if (Math.Abs(Position.X - defensePos.X) < Math.Abs(Position.X - puckPos.X))
                {
                    move.Y = (puckPos.Y - Position.Y) * aiDefenseSpeed.Y;
                    // if puck is within my move area then move towards it (otherwise move towards the defensePos)
                    if (Math.Abs(Position.X - puckPos.X) < aiAttackDistance)
                    {
                        move.X = (puckPos.X - Position.X) * aiAttackSpeed.X;
                    }
                    else
                    {
                        move.X = (defensePos.X - Position.X) * aiDefenseSpeed.X;
                    }
                }
                else
                {
                   // if puck.Y is above Position.Y then move down at defenseSpeed for Y.
                   if (puckPos.Y < Position.Y) move.Y = aiDefenseSpeed.Y;
                   else if (puckPos.Y > Position.Y) move.Y = -aiDefenseSpeed.Y;
                   move.X = (defensePos.X - Position.X) * aiDefenseSpeed.X;
                }

                return move;
            }
            void IScreenSpriteProvider.AddToScreen(Screen screen)
            {
                screen.AddSprite(BumperRing);
                screen.AddSprite(BumperHighlight);
                screen.AddSprite(HandleCircle);
                screen.AddSprite(HandleHighlight);
                screen.AddSprite(HandleDot);
            }

            void IScreenSpriteProvider.RemoveToScreen(Screen screen)
            {
                screen.RemoveSprite(BumperRing);
                screen.RemoveSprite(BumperHighlight);
                screen.RemoveSprite(HandleCircle);
                screen.RemoveSprite(HandleHighlight);
                screen.RemoveSprite(HandleDot);
            }
        }
        //----------------------------------------------------------------------
    }
}
