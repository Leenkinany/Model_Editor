using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Editor.Engine.Interfaces;
using System.IO;
using System.Windows.Forms;

namespace Editor.Engine
{
    internal class Level : Iserializable            
    {
        private static readonly System.Random s_random = new System.Random();

        public Camera GetCamera() { return m_camera; }

        private List<Models> m_models = new();
        private Camera m_camera = new(new Vector3(0, 2, 2), 16 / 9);

        public Level()
        { }

        public void LoadContent(ContentManager content)
        {
            Models teapot = new(content, "Teapot", "Metal", "MyShader", Vector3.Zero, 1.0f);
            AddModel(teapot);
            teapot = new(content, "Teapot", "Metal", "MyShader", new Vector3(1, 0, 0), 1.0f);
            AddModel(teapot);
        }
        private void AddModel(Models model)
        {
            m_models.Add(model);
        }
        public void Render()
        {
            foreach (Models model in m_models)
            {
                model.Render(m_camera.View, m_camera.Projection);
            }
        }

        public void Update(float _delta)
        {
            InputController ic = InputController.Instance;
            Vector3 translate = Vector3.Zero;
            if (ic.IsKeyDown(Keys.Left)) translate.X += -10;
            if (ic.IsKeyDown(Keys.Right)) translate.X += 10;
            if (ic.IsKeyDown(Keys.Menu))
            {
                if (ic.IsKeyDown(Keys.Up)) translate.Z += 1;
                if (ic.IsKeyDown(Keys.Down)) translate.Z += -1;
            }
            else
            {
                if (ic.IsKeyDown(Keys.Up)) translate.Y += 10;
                if (ic.IsKeyDown(Keys.Down)) translate.Y += -10;
            }
            if (ic.IsMouseButtonDown(MouseButtons.Middle))
            {
                Vector2 dir = ic.MousePosition - ic.LastPosition;
                translate.X = -dir.X;
                translate.Y = dir.Y;
            }
            if(ic.GetWheel() != 0)
            {
                translate.Z = ic.GetWheel() * 2;
            }
            if (translate != Vector3.Zero)
            {
                m_camera.Translate(translate * 0.001f);
            }
            if (ic.IsMouseButtonDown(MouseButtons.Right))
            {
                Vector2 dir = ic.MousePosition - ic.LastPosition;
                if(dir != Vector2.Zero)
                {
                    Vector3 movement = new Vector3(dir.Y, dir.X, 0) * _delta;
                    m_camera.Rotate(movement);
                }
            }
        }

        public void Serialize(BinaryWriter _stream)
        {
            _stream.Write(m_models.Count);
            foreach (var model in m_models)
            {
                model.Serialize(_stream);
            }
            m_camera.Serialize(_stream);
        }

        public void Deserialize(BinaryReader _stream, ContentManager _content)
        {
            int modelCount = _stream.ReadInt32();
            for (int i = 0; i < modelCount; i++)
            {
                Models model = new();
                model.Deserialize(_stream, _content);
                m_models.Add(model);
            }
            m_camera.Deserialize(_stream, _content);
        }
        public override string ToString()
        {
            return m_camera.ToString();
        }

    }
}
