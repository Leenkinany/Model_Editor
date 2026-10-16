using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

namespace Editor.Engine
{
    internal class InputController
    {
        private static readonly Lazy<InputController> lazy = new(() => new InputController());
        public static InputController Instance { get { return lazy.Value; } }

        public Vector2 MousePosition { get; set; } = Vector2.Zero;
        public Vector2 LastPosition { get; private set; } = Vector2.Zero;
        public Vector2 DragStart { get; set; } = Vector2.Zero;
        public Vector2 DragEnd { get; set; } = Vector2.Zero;

        private Dictionary<Keys, bool> m_keyStates = new();
        private Dictionary<MouseButtons, bool> m_buttonState = new();

        private int m_mouseWheel = 0;

        private InputController()
        {
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (m_keyStates.ContainsKey(key)) continue;
                m_keyStates.Add(key, false);
            }
            foreach (MouseButtons button in Enum.GetValues(typeof(MouseButtons)))
            {
                if (m_buttonState.ContainsKey(button)) continue;
                m_buttonState.Add(button, false);
            }
        }

        public void SetKeyDown(Keys key)
        {
            m_keyStates[key] = true;
        }

        public void SetKeyUp(Keys key)
        {
            m_keyStates[key] = false;
        }

        public bool IsKeyDown(Keys key)
        {
            return m_keyStates[key];
        }

        public void SetButtonDown(MouseButtons button)
        {
            m_buttonState[button] = true;
        }

        public void SetButtonUp(MouseButtons button)
        {
            m_buttonState[button] = false;
        }

        public bool IsMouseButtonDown(MouseButtons button)
        {
            return m_buttonState[button];
        }

        public void SetWheel(int _delta)
        {
            m_mouseWheel += _delta;
        }
        public int GetWheel()
        {
            return m_mouseWheel;
        }
        public void Clear()
        {
            m_mouseWheel = 0;
            LastPosition = MousePosition;
        } 

        public Ray GetPickRay(Camera camera)
        {
            Vector3 nearPoint = new Vector3(MousePosition, 0);
            Vector3 farPoint = new Vector3(MousePosition, 1);

            nearPoint = camera.viewport.Unproject(nearPoint, camera.Projection, camera.View, Matrix.Identity);
            farPoint = camera.viewport.Unproject(farPoint, camera.Projection, camera.View, Matrix.Identity);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }

        public override string ToString()
        {
            string s = "Keys Down: ";

            foreach (var key in m_keyStates)
            {
                if (key.Value == true)
                {
                    s += key.Key + " ";
                }
            }

            s += "\nButtons Down: ";
            foreach (var button in m_buttonState)
            {
                if (button.Value == true)
                {
                    s += button.Key + " ";
                }
            }
            return s;
        }
    }
}
