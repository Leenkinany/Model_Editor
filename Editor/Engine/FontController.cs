using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Engine
{
    internal class FontController
    {
        private SpriteFont m_fontArial16 = null;
        private SpriteFont m_fontArial18 = null;
        private SpriteFont m_fontArial20 = null;

        public FontController()
        {
        }

        public void LoadContent(ContentManager content)
        {
            m_fontArial16 = content.Load<SpriteFont>("Arial16");
            m_fontArial18 = content.Load<SpriteFont>("Arial18");
            m_fontArial20 = content.Load<SpriteFont>("Arial20");
        }

        public void Draw(SpriteBatch spriteBatch, int _size, string text, Vector2 position, Color color)
        {
            switch (_size)
            {
                case 16:
                    spriteBatch.DrawString(m_fontArial16, text, position, color);
                    break;
                case 18:
                    spriteBatch.DrawString(m_fontArial18, text, position, color);
                    break;
                case 20:
                    spriteBatch.DrawString(m_fontArial20, text, position, color);
                    break;
                default:
                    Debug.Assert(true);
                    break;
            }
        }
    }
}
