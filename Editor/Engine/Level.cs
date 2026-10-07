using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Editor.Engine.Interfaces;
using System.IO;

namespace Editor.Engine
{
    internal class Level : Iserializable            
    {
        private static readonly System.Random s_random = new System.Random();

        public Camera GetCamera() { return m_camera; }

        private List<Models> m_models = new();
        private Camera m_camera = new(new Vector3(0,0,300), 16/9);

        public Level()
        { }

        public void LoadContent(ContentManager content)
        {
            
        }
        private void AddModel(Models model)
        {
            m_models.Add(model);
        }
        public void Update()
        {
            foreach (Models model in m_models)
            {
                model.Update();
            }
        }
        public void Render()
        {
            foreach (Models model in m_models)
            {
                model.Render(m_camera.View, m_camera.Projection);
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

        public void AddSun(ContentManager content)
        {
            int countSun = 0;
            foreach (Models model in m_models)
            {
                if (model.Texture.Tag.ToString() == "SunDiffuse")
                    countSun++;
            }
            if (countSun >= 1)
                return;

            Models Sun = new(content,"Sphere","SunDiffuse","MyShader",Vector3.Zero,2.0f);
            Sun.RotationSpeed = 0.005f;

            AddModel(Sun);
        }
        public void AddPlanet(ContentManager content)
        {
            Models Sun = null;

            // Find the existing Sun
            foreach (Models model in m_models)
            {
                if (model.Texture.Tag.ToString() == "SunDiffuse")
                {
                    Sun = model;
                    break;
                }
            }

            int planetCount = 0;

            foreach (Models model in m_models)
            {
                if (model.Texture.Tag.ToString() == "WorldDiffuse")
                    planetCount++;
            }

            if (planetCount >= 5)
                return;

            // Random starting position
            Vector3 randPos = new(
                s_random.Next(-150, 150),
                s_random.Next(-90, 90),
                0
            );

            Models Planet = new(
                content,
                "Sphere",
                "WorldDiffuse",
                "MyShader",
                randPos,
                0.75f
            );

            Planet.SetParent(Sun);

            Planet.RotationSpeed = 0.02f +
                (float)(s_random.NextDouble() * 0.01f);

            Planet.OrbitSpeed = 0.001f +
                (float)(s_random.NextDouble() * 0.001f);

            AddModel(Planet);
        }

        public void AddMoon(ContentManager content)
        {
            List<Models> worlds = new();
            // Find all existing worlds
            foreach (Models model in m_models)
            {
                if (model.Texture.Tag.ToString() == "WorldDiffuse")
                {
                    worlds.Add(model);
                }
            }

            foreach (Models world in worlds)
            {
                int mooncount = 0;

                foreach (Models model in m_models)
                {
                    if (model.Texture.Tag.ToString() == "MoonDiffuse" &&
                        model.Parent == world)
                    {
                        mooncount++;
                    }
                }

                Vector3 moonPosition = new(world.Position.X + 20, world.Position.Y, world.Position.Z);

                float randomScale = 0.2f +(float)s_random.NextDouble() * 0.2f;

                Models Moon = new(content,"Sphere","MoonDiffuse","MyShader",moonPosition,randomScale);
                Moon.SetParent(world);
                Moon.RotationSpeed = 0.005f +(float)s_random.NextDouble() * 0.005f;
                Moon.OrbitSpeed = 0.01f +(float)s_random.NextDouble() * 0.01f;

                AddModel(Moon);
            }
        }
    }
}
