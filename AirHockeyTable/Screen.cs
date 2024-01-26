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
        // Screen - encapsulates a drawing surface
        //----------------------------------------------------------------------
        public class Screen
        {
            private IMyTextSurface _drawingSurface;
            private RectangleF _viewport;
            private List<ScreenSprite> _sprites = new List<ScreenSprite>();
            public float bottomPadding = 0f;
            public float topPadding = 0f;
            public float leftPadding = 0f;
            public float rightPadding = 0f;
            public bool hasColorfulIcons
            {
                get
                {
                    List<string> sprites = new List<string>();
                    _drawingSurface.GetSprites(sprites);
                    return sprites.Contains("ColorfulIcons_Ore/Iron");
                }
            }
            public Color BackgroundColor
            {
                get { return _drawingSurface.ScriptBackgroundColor; }
                set { _drawingSurface.ScriptBackgroundColor = value; }
            }
            public Color ForegroundColor
            {
                get { return _drawingSurface.ScriptForegroundColor; }
                set { _drawingSurface.ScriptForegroundColor = value; }
            }
            public Vector2 Size
            {
                get { return _drawingSurface.SurfaceSize; }
            }
            //
            // constructor
            //
            public Screen(IMyTextSurface drawingSurface)
            {
                _drawingSurface = drawingSurface;
                _drawingSurface.ContentType = ContentType.SCRIPT;
                _drawingSurface.Script = "";
                // calculate the viewport offset by centering the surface size onto the texture size
                _viewport = new RectangleF(
                                       (_drawingSurface.TextureSize - _drawingSurface.SurfaceSize) / 2f,
                                                          _drawingSurface.SurfaceSize
                                                                         );
            }
            //
            // DrawSprites - draw sprites to the screen
            //
            public virtual void Draw()
            {
                //GridInfo.Echo("Screen: Draw");
                var frame = _drawingSurface.DrawFrame();
                DrawSprites(ref frame);
                frame.Dispose();
            }
            //
            // DrawSprites - draw sprites to the screen
            //
            private void DrawSprites(ref MySpriteDrawFrame frame)
            {
                //GridInfo.Echo("Screen: DrawSprites: "+_sprites.Count.ToString());
                // draw all the sprites
                foreach (ScreenSprite sprite in _sprites)
                {
                    //GridInfo.Echo("Screen: DrawSprites: sprite: viewport: "+_viewport.ToString());
                    //if(sprite != null) GridInfo.Echo("Screen: DrawSprites: sprite: position: "+sprite.GetPosition(_viewport).ToString());
                    //else GridInfo.Echo("Screen: DrawSprites: sprite: null");
                    if (sprite != null && sprite.Visible) frame.Add(sprite.ToMySprite(_viewport));
                }
            }
            // add a text sprite
            public ScreenSprite AddTextSprite(ScreenSprite.ScreenSpriteAnchor anchor, Vector2 position, float rotationOrScale, Color color, string fontId, string data, TextAlignment alignment)
            {
                ScreenSprite sprite = new ScreenSprite(anchor, position, rotationOrScale, new Vector2(0, 0), color, fontId, data, alignment, SpriteType.TEXT);
                _sprites.Add(sprite);
                return sprite;
            }
            // add a texture sprite
            public ScreenSprite AddTextureSprite(ScreenSprite.ScreenSpriteAnchor anchor, Vector2 position, float rotationOrScale, Vector2 size, Color color, string data)
            {
                ScreenSprite sprite = new ScreenSprite(anchor, position, rotationOrScale, size, color, "", data, TextAlignment.CENTER, SpriteType.TEXTURE);
                _sprites.Add(sprite);
                return sprite;
            }
            public void AddSprite(ScreenSprite sprite)
            {
                _sprites.Add(sprite);
            }
            public void AddSprite(IScreenSpriteProvider spriteProvider)
            {
                spriteProvider.AddToScreen(this);
            }
            public void RemoveSprite(ScreenSprite sprite)
            {
                if (_sprites.Contains(sprite)) _sprites.Remove(sprite);
            }
            public void RemoveSprite(IScreenSpriteProvider spriteProvider)
            {
                spriteProvider.RemoveToScreen(this);
            }
        }
        //----------------------------------------------------------------------
        // screen sprite - encapsulates a sprite
        //----------------------------------------------------------------------
        public class ScreenSprite
        {
            public static float DEFAULT_FONT_SIZE = 1.5f;
            public static float MONOSPACE_FONT_SIZE = 0.2f;
            public enum ScreenSpriteAnchor
            {
                TopLeft,
                TopCenter,
                TopRight,
                CenterLeft,
                Center,
                CenterRight,
                BottomLeft,
                BottomCenter,
                BottomRight
            }
            public ScreenSpriteAnchor Anchor { get; set; }
            public Vector2 Position { get; set; }
            public float RotationOrScale { get; set; }
            public Vector2 Size { get; set; }
            public Color Color { get; set; }
            public string FontId { get; set; }
            public string Data { get; set; }
            public TextAlignment Alignment { get; set; }
            public SpriteType Type { get; set; }
            public bool Visible { get; set; } = true;
            // constructor
            public ScreenSprite()
            {
                Anchor = ScreenSpriteAnchor.Center;
                Position = Vector2.Zero;
                RotationOrScale = 1f;
                Size = Vector2.Zero;
                Color = Color.White;
                FontId = "White";
                Data = "";
                Alignment = TextAlignment.CENTER;
                Type = SpriteType.TEXT;
            }
            // constructor
            public ScreenSprite(ScreenSpriteAnchor anchor, Vector2 position, float rotationOrScale, Vector2 size, Color color, string fontId, string data, TextAlignment alignment, SpriteType type)
            {
                Anchor = anchor;
                Position = position;
                RotationOrScale = rotationOrScale;
                Size = size;
                Color = color;
                FontId = fontId;
                Data = data;
                Alignment = alignment;
                Type = type;
            }
            // convert the sprite to a MySprite
            public virtual MySprite ToMySprite(RectangleF _viewport)
            {
                if (Type == SpriteType.TEXT)
                {
                    return new MySprite()
                    {
                        Type = Type,
                        Data = Data,
                        Position = GetPosition(_viewport),
                        RotationOrScale = RotationOrScale,
                        Color = Color,
                        Alignment = Alignment,
                        FontId = FontId
                    };
                }
                return new MySprite()
                {
                    Type = Type,
                    Data = Data,
                    Position = GetPosition(_viewport),
                    RotationOrScale = RotationOrScale,
                    Color = Color,
                    Alignment = Alignment,
                    Size = Size,
                    FontId = FontId
                };
            }
            public Vector2 GetPosition(RectangleF _viewport)
            {
                Vector2 _position = Position + _viewport.Position;
                switch (Anchor)
                {
                    case ScreenSpriteAnchor.TopCenter:
                        _position = Position + new Vector2(_viewport.Center.X, _viewport.Y);
                        break;
                    case ScreenSpriteAnchor.TopRight:
                        _position = Position + new Vector2(_viewport.Right, _viewport.Y);
                        break;
                    case ScreenSpriteAnchor.CenterLeft:
                        _position = Position + new Vector2(_viewport.X, _viewport.Center.Y);
                        break;
                    case ScreenSpriteAnchor.Center:
                        _position = Position + _viewport.Center;
                        break;
                    case ScreenSpriteAnchor.CenterRight:
                        _position = Position + new Vector2(_viewport.Right, _viewport.Center.Y);
                        break;
                    case ScreenSpriteAnchor.BottomLeft:
                        _position = Position + new Vector2(_viewport.X, _viewport.Bottom);
                        break;
                    case ScreenSpriteAnchor.BottomCenter:
                        _position = Position + new Vector2(_viewport.Center.X, _viewport.Bottom);
                        break;
                    case ScreenSpriteAnchor.BottomRight:
                        _position = Position + new Vector2(_viewport.Right, _viewport.Bottom);
                        break;
                }
                return _position;
            }

        }
        
        //----------------------------------------------------------------------
        // IScreenSpriteProvider - for classes that include sprites
        //----------------------------------------------------------------------
        public interface IScreenSpriteProvider
        {
            void AddToScreen(Screen screen);
            void RemoveToScreen(Screen screen);
        }
        //----------------------------------------------------------------------
        // end sprite screen
        //----------------------------------------------------------------------
    }
}
