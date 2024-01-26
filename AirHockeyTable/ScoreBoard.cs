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
        // Score Board
        //----------------------------------------------------------------------

        public class ScoreBoard : Screen
        {
            bool isMySide = false;
            bool isOtherSide = false;
            List<ScreenSprite> scoreDots = new List<ScreenSprite>();
            List<ScreenSprite> scoreOutlines = new List<ScreenSprite>();
            int score = 0;
            int otherScore = 0;
            string scoreVar = "";
            string otherScoreVar = "";
            string winner = "";
            Color sideColor;
            ScreenSprite EmojiFace;
            int EmojiCooldwon = 0;
            // initialize the score board
            public ScoreBoard(IMyTextSurface drawSurface,string ScoreVar, Color sideColor, string OtherScoreVar) : base(drawSurface)
            {
                BackgroundColor = Color.Black;
                scoreVar = ScoreVar;
                otherScoreVar = OtherScoreVar;
                this.sideColor = sideColor;
                GridInfo.AddChangeListener(ScoreVar,UpdateVar);
                GridInfo.AddChangeListener(OtherScoreVar,UpdateVar);
                GridInfo.AddChangeListener("Winner", UpdateVar);
                score = GridInfo.GetVarAs(scoreVar, 0);
                otherScore = GridInfo.GetVarAs(otherScoreVar, 0);
                winner = GridInfo.GetVarAs("Winner","");
                // if the Size is portrait, then we need to make two columns of dots starting from the bottom
                if (Size.X < Size.Y)
                {
                    AddScoreColumn(Size.X * -0.25f);
                    AddScoreColumn(Size.X * 0.25f);
                    EmojiFace = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopCenter, new Vector2(0, Size.Y * 0.2f), 0f, new Vector2(Size.X * 0.5f), sideColor, "", "LCD_Emote_Sleepy", TextAlignment.CENTER, SpriteType.TEXTURE);
                }
                // otherwise, we need to make two rows of dots starting from the left
                else
                {
                    AddScoreRow(Size.Y * -0.25f);
                    AddScoreRow(Size.Y * 0.25f);
                    EmojiFace = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterRight, new Vector2(Size.X * -0.2f, 0), 0f, new Vector2(Size.Y * 0.5f), sideColor, "", "LCD_Emote_Sleepy", TextAlignment.CENTER, SpriteType.TEXTURE);
                }
                AddSprite(EmojiFace);
                UpdateScore();
            }
            // a colum of five score dots starting from the bottom of the screen
            void AddScoreColumn(float xOffset)
            {
                for (int i = 1; i < 6; i++)
                {
                    ScreenSprite dot = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.BottomCenter, new Vector2(xOffset, (-i * Size.X*0.3f)),0f, new Vector2(Size.X * 0.25f),sideColor,"","Circle",TextAlignment.CENTER,SpriteType.TEXTURE);
                    scoreDots.Add(dot);
                    dot.Visible = false;
                    AddSprite(dot);
                    ScreenSprite outline = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.BottomCenter, new Vector2(xOffset, (-i * Size.X * 0.3f)),0f, new Vector2(Size.X * 0.25f),sideColor,"", "CircleHollow", TextAlignment.CENTER,SpriteType.TEXTURE);
                    scoreOutlines.Add(outline);
                    AddSprite(outline);
                }
            }
            void AddScoreRow(float yOffset)
            {
                for (int i = 1; i < 6; i++)
                {
                    ScreenSprite dot = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterLeft, new Vector2(i * Size.Y * 0.30f, yOffset),0f, new Vector2(Size.Y * 0.25f),sideColor,"","Circle",TextAlignment.CENTER,SpriteType.TEXTURE);
                    scoreDots.Add(dot);
                    dot.Visible = false;
                    AddSprite(dot);
                    ScreenSprite outline = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterLeft, new Vector2(i * Size.Y * 0.30f, yOffset),0f, new Vector2(Size.Y * 0.25f),sideColor,"", "CircleHollow", TextAlignment.CENTER,SpriteType.TEXTURE);
                    scoreOutlines.Add(outline);
                    AddSprite(outline);
                }
            }
            void UpdateVar(string key, string value)
            {
                if(key == scoreVar)
                {
                    score = GridInfo.GetVarAs(scoreVar, 0);
                    UpdateScore();
                }
                else if(key == otherScoreVar)
                {
                    otherScore = GridInfo.GetVarAs(otherScoreVar, 0);
                    UpdateScore();
                }
                else if(key == "Winner")
                {
                    winner = value;
                }
            }
            void UpdateScore()
            {
                for(int i = 0; i < scoreDots.Count; i++)
                {
                    scoreDots[i].Visible = (i < score);
                }
                if(score >= 10)
                {
                    if(otherScore > 8)
                    {
                        EmojiFace.Data = "LCD_Emote_Wink";
                    }
                    else if(otherScore > 5)
                    {
                        EmojiFace.Data = "LCD_Emote_Happy";
                    }
                    else if (otherScore > 2)
                    {
                        EmojiFace.Data = "LCD_Emote_Evil";
                    }
                    else
                    {
                        EmojiFace.Data = "LCD_Emote_Neutral";
                    }
                }
                else if(otherScore >= 10)
                {
                    if(score > 8)
                    {
                        EmojiFace.Data = "LCD_Emote_Angry";
                    }
                    else if(score > 5)
                    {
                        EmojiFace.Data = "LCD_Emote_Skeptical";
                    }
                    else if (score > 2)
                    {
                        EmojiFace.Data = "LCD_Emote_Shocked";
                    }
                    else
                    {
                        EmojiFace.Data = "LCD_Emote_Dead";
                    }

                }
                else if(score > otherScore)
                {
                    EmojiFace.Data = "LCD_Emote_Happy";
                }
                else if(score < otherScore)
                {
                    if(otherScore - score >= 5)
                    {
                        EmojiFace.Data = "LCD_Emote_Crying";
                    }
                    else if(score < 5)
                    {
                        EmojiFace.Data = "LCD_Emote_Sad";
                    }
                    else
                    {
                        EmojiFace.Data = "LCD_Emote_Annoyed";
                    }
                }
                EmojiCooldwon = 100;
            }
            public override void Draw()
            {
                
                if(EmojiCooldwon > 0 && EmojiFace.Data != "LCD_Emote_Sleepy")
                {
                    EmojiCooldwon--;
                    if(EmojiCooldwon == 1)
                    {
                        if (winner != "")
                        {
                            if (winner == scoreVar)
                            {
                                EmojiFace.Data = "LCD_Emote_Happy";
                            }
                            else
                            {
                                EmojiFace.Data = "LCD_Emote_Dead";
                            }
                        }
                        else if (EmojiFace.Data == "LCD_Emote_Neutral")
                        {
                            EmojiFace.Data = "LCD_Emote_Suspicious_Left";
                        }
                        else if (EmojiFace.Data == "LCD_Emote_Suspicious_Left")
                        {
                            EmojiFace.Data = "LCD_Emote_Suspicious_Right";
                        }
                        else if (EmojiFace.Data == "LCD_Emote_Suspicious_Right")
                        {
                            EmojiFace.Data = "LCD_Emote_Suspicious_Left";
                        }
                        else
                        {
                            EmojiFace.Data = "LCD_Emote_Neutral";
                        }
                        EmojiCooldwon = 100;
                    }
                }
                base.Draw();
            }
        }
        //----------------------------------------------------------------------
    }
}
