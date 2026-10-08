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
        private Camera m_camera = new(new Vector3(0,0,300), 16f/9f);

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

                int parentIndex = model.Parent != null ? m_models.IndexOf(model.Parent) : -1;
                _stream.Write(parentIndex);
            }
            m_camera.Serialize(_stream);
        }

        public void Deserialize(BinaryReader _stream, ContentManager _content)
        {
            int modelCount = _stream.ReadInt32();
            List<int> parentIndices = new();
            for (int i = 0; i < modelCount; i++)
            {
                Models model = new();
                model.Deserialize(_stream, _content);
                m_models.Add(model);
                int parentIndex = _stream.ReadInt32();
                parentIndices.Add(parentIndex);
            }
            for (int i = 0; i < modelCount; i++)
            {
                int parentIndex = parentIndices[i];
                if (parentIndex >= 0 && parentIndex < m_models.Count)
                {
                    m_models[i].Parent=m_models[parentIndex];
                }
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

            Models Sun = new(content,"Sun","SunDiffuse","MyShader",Vector3.Zero,2.0f);
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
                s_random.Next(-150, 151),
                s_random.Next(-90, 91),
                0
            );

            Models Planet = new(
                content,
                "World",
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

                Models Moon = new(content,"Moon","MoonDiffuse","MyShader",moonPosition,randomScale);
                Moon.SetParent(world);
                Moon.RotationSpeed = 0.005f +(float)s_random.NextDouble() * 0.005f;
                Moon.OrbitSpeed = 0.01f +(float)s_random.NextDouble() * 0.01f;

                AddModel(Moon);
            }
        }
    }
}
