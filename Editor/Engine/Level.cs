using Editor.Engine.Interfaces;
using Microsoft.Xna.Framework;
using Editor.Engine.Lights;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Windows.Forms;

namespace Editor.Engine
{
    internal class Level : Iserializable            
    {
        private static readonly System.Random s_random = new System.Random();

        public Camera GetCamera() { return m_camera; }

        private List<Models> m_models = new();
        private Models m_selectedModel = null;
        private ISelectable m_selectedObject = null;
        private bool m_selectionDirty = false;
        private Camera m_camera = new(new Vector3(30, 45, -70), 16f / 9f);
        private Terrain m_terrain;
        private Effect m_terrainEffect;
        private Light m_light = new() { Position = new(0,400,-500), Color = new(0.9f, 0.9f, 0.9f) };

        public Level()
        { }

        public void LoadContent(GameEditor _game)
        {
            m_terrain = new Terrain(
                _game.DefaultEffect,
                _game.DefaultHeightMap,
                _game.DefaultGrass,
                200,
                _game.GraphicsDevice
            );
        }

        public void AddModel(Models model)
        {
            m_models.Add(model);
        }

        public void AddImportedModel(
            ContentManager projectContent,
            ContentManager editorContent,
            string modelName,
            string textureName)
        {
            Models importedModel = new Models(
                projectContent,
                editorContent,
                modelName,
                textureName,
                "MyShader",
                new Vector3(0, 0, 0),
                1.0f
            );

            AddModel(importedModel);
        }


        public List<ISelectable> GetSelectedModels()
        {
            List<ISelectable> models = new();

            foreach (var model in m_models)
            {
                if (model.Selected)
                    models.Add(model);
            }

            if (m_terrain != null && m_terrain.Selected)
            {
                models.Add(m_terrain);
            }

            return models;
        }

        public void Render()
        {
            Renderer r = Renderer.Instance;
            r.Camera = m_camera;
            r.Light = m_light;
            foreach (Models model in m_models)
                r.Render(model);

            if (m_terrain != null)
                r.Render(m_terrain);
        }

        public void Update(float _delta)
        {
            HandleTranslate();
            HandleRotate(_delta);
            HandleScale(_delta);
            HandlePick();
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

            // Restore the saved camera.
            m_camera.Deserialize(_stream, _content);

            // Recreate terrain resources.
            Texture2D heightMap = _content.Load<Texture2D>("DefaultHeightMap");
            Texture2D grass = _content.Load<Texture2D>("DefaultGrass");

            m_terrainEffect = _content.Load<Effect>("DefaultShader");

            IGraphicsDeviceService graphicsService =
                _content.ServiceProvider.GetService(
                    typeof(IGraphicsDeviceService)
                ) as IGraphicsDeviceService;

            if (graphicsService == null)
                throw new InvalidOperationException(
                    "Graphics device unavailable"
                );

            m_terrain = new Terrain(
                m_terrainEffect,
                heightMap,
                grass,
                50,
                graphicsService.GraphicsDevice
            );

        }

        public void Deserialize(
            BinaryReader stream,
            ContentManager editorContent,
            ContentManager projectContent,
            int fileVersion)
        {
            int modelCount = stream.ReadInt32();
            for (int i = 0; i < modelCount; i++)
            {
                Models model = new();

                model.Deserialize(
                    stream,
                    editorContent,
                    projectContent,
                    fileVersion >= 2
                );

                m_models.Add(model);
            }

            // Restore the saved camera.
            m_camera.Deserialize(stream, editorContent);

            // Recreate the terrain without adding another teapot
            // or resetting the saved camera.
            Texture2D heightMap =
                editorContent.Load<Texture2D>("DefaultHeightMap");

            Texture2D grass =
                editorContent.Load<Texture2D>("DefaultGrass");

            m_terrainEffect =
                editorContent.Load<Effect>("DefaultShader");

            IGraphicsDeviceService graphicsService =
                editorContent.ServiceProvider.GetService(
                    typeof(IGraphicsDeviceService)
                ) as IGraphicsDeviceService;

            if (graphicsService == null)
            {
                throw new InvalidOperationException(
                    "Graphics device unavailable");
            }

            m_terrain = new Terrain(
                m_terrainEffect,
                heightMap,
                grass,
                50,
                graphicsService.GraphicsDevice
            );

        }

        public override string ToString()
        {
            string s = string.Empty;
            foreach (Models m in m_models)
            {
                if (m.Selected)
                {
                    s += "\nModel : Pos: " + m.Position.ToString() +
                         "  Rot: " + m.Rotation.ToString();
                }
            }
            return m_camera.ToString() + s;
        }

        private void HandleTranslate()
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
                translate.X = dir.X;
                translate.Y = -dir.Y;
            }
            if (ic.GetWheel() != 0)
            {
                translate.Z = ic.GetWheel() * 2;
            }
            if (translate != Vector3.Zero)
            {
                bool modelTranslated = false;
                foreach(Models model in m_models)
                {
                    if(model.Selected)
                    {
                        modelTranslated = true;
                        model.Translate(translate / 1000, m_camera);
                    }
                }
                if (!modelTranslated)
                {
                    m_camera.Translate(translate * 0.001f);
                }
            }
        }
        private void HandleRotate(float _delta)
        {
            InputController ic = InputController.Instance;
            if(ic.IsMouseButtonDown(MouseButtons.Right) &&
                    (!ic.IsKeyDown(Keys.Menu)))
            {
                Vector2 dir = ic.MousePosition - ic.LastPosition;
                if(dir != Vector2.Zero)
                {
                    Vector3 movement = new Vector3(dir.Y, dir.X, 0) * _delta;
                    bool modelRotated = false;
                    foreach(Models model in m_models)
                    {
                        if(model.Selected)
                        {
                            modelRotated = true;
                            model.Rotation += movement;
                        }
                    }
                    if (!modelRotated)
                    {
                        m_camera.Rotate(movement);
                    }
                }                
            }
        }
        private void HandleScale(float _delta)
        {
            InputController ic = InputController.Instance;
            if (ic.IsMouseButtonDown(MouseButtons.Right) &&
                    (!ic.IsKeyDown(Keys.Menu)))
            {
                float l = ic.MousePosition.X - ic.LastPosition.X;
                if (l !=0)
                {
                    l *= _delta;
                    foreach (Models model in m_models)
                    {
                        if (model.Selected)
                        {
                            model.Scale += l;
                        }
                    }
                }
            }
        }
        internal ISelectable HandlePick(bool _select = true)
        {
            InputController ic = InputController.Instance;

            if (ic.IsMouseButtonDown(MouseButtons.Left) || !_select)
            {
                Ray r = HelpMath.GetPickRay(ic.MousePosition, m_camera);

                Models clickedModel = null;
                ISelectable clickedObject = null;
                float closestDistance = float.MaxValue;

                // Check model triangles.
                foreach (Models model in m_models)
                {
                    Matrix transform = model.GetTransform();

                    foreach (ModelMesh mesh in model.Mesh.Meshes)
                    {
                        BoundingSphere sphere = mesh.BoundingSphere;
                        sphere = sphere.Transform(transform);

                        float? sphereHit = r.Intersects(sphere);

                        if (sphereHit.HasValue &&
                            sphereHit.Value < closestDistance)
                        {
                            float? triangleHit =
                                HelpMath.PickTriangle(
                                    in mesh, ref r, ref transform);

                            if (triangleHit.HasValue &&
                                triangleHit.Value < closestDistance)
                            {
                                closestDistance = triangleHit.Value;
                                clickedModel = model;
                                clickedObject = model;
                            }
                        }
                    }
                }

                // Check terrain triangles.
                if (m_terrain != null)
                {
                    Matrix terrainTransform = Matrix.Identity;

                    float? terrainHit = HelpMath.PickTriangle(
                        in m_terrain,
                        ref r,
                        ref terrainTransform
                    );

                    if (terrainHit.HasValue &&
                        terrainHit.Value < closestDistance)
                    {
                        closestDistance = terrainHit.Value;
                        clickedModel = null;
                        clickedObject = m_terrain;
                    }
                }
                if (!_select)
                    return clickedObject;
                // Update selection.
                bool selectionChanged = m_selectedObject != clickedObject;

                if (selectionChanged)
                {
                    if (m_selectedObject != null)
                        m_selectedObject.Selected = false;

                    m_selectedObject = clickedObject;
                    m_selectedModel = clickedModel;

                    if (m_selectedObject != null)
                        m_selectedObject.Selected = true;

                    m_selectionDirty = true;

                }
            }
            return null;
        }



        public Models SelectedModel
        {
            get { return m_selectedModel; }
        }

        public ISelectable SelectedObject
        {
            get { return m_selectedObject; }
        }

        public bool SelectionDirty
        {
            get { return m_selectionDirty; }
        }

        public void ClearSelectionDirty()
        {
            m_selectionDirty = false;
        }
    }
}
