
using Editor.Engine.Interfaces;
using Editor.Engine.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Editor.Engine
{
    internal class Level : Iserializable
    {
        private List<Models> m_models = new();
        private Models m_selectedModel = null;
        private ISelectable m_selectedObject = null;
        private bool m_selectionDirty = false;

        private Camera m_camera =
            new(new Vector3(30, 45, -70), 16f / 9f);

        private Terrain m_terrain;

        private Light m_light = new()
        {
            Position = new(0, 400, -500),
            Color = new(0.9f, 0.9f, 0.9f)
        };

        public Level() { }

        public Camera GetCamera()
        {
            return m_camera;
        }

        public void LoadContent(GameEditor game)
        {
            m_terrain = new Terrain(
                game.DefaultEffect,
                game.DefaultHeightMap,
                game.DefaultGrass,
                200,
                game.GraphicsDevice
            );
        }

        public void AddModel(Models model)
        {
            m_models.Add(model);
        }

        public List<ISelectable> GetSelectedModels()
        {
            List<ISelectable> models = new();

            foreach (Models model in m_models)
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
            Renderer renderer = Renderer.Instance;

            renderer.Camera = m_camera;
            renderer.Light = m_light;

            foreach (Models model in m_models)
            {
                renderer.Render(model);
            }

            if (m_terrain != null)
            {
                renderer.Render(m_terrain);
            }
        }

        public void Update(float delta)
        {
            HandleTranslate();
            HandleRotate(delta);
            HandleScale(delta);
            HandlePick();
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.Write(m_models.Count);

            foreach (Models model in m_models)
            {
                model.Serialize(stream);
            }

            m_camera.Serialize(stream);
        }

        public void Deserialize(
            BinaryReader stream,
            GameEditor game)
        {
            m_models.Clear();

            int modelCount = stream.ReadInt32();

            for (int i = 0; i < modelCount; i++)
            {
                Models model = new();
                model.Deserialize(stream, game);
                m_models.Add(model);
            }

            m_camera.Deserialize(stream, game);

            LoadContent(game);

            m_selectedModel = null;
            m_selectedObject = null;
            m_selectionDirty = true;
        }

        public override string ToString()
        {
            string result = string.Empty;

            foreach (Models model in m_models)
            {
                if (model.Selected)
                {
                    result += "\nModel : Pos: " +
                              model.Position.ToString() +
                              "  Rot: " +
                              model.Rotation.ToString();
                }
            }

            return m_camera.ToString() + result;
        }

        private void HandleTranslate()
        {
            InputController ic = InputController.Instance;
            Vector3 translate = Vector3.Zero;

            if (ic.IsKeyDown(Keys.Left))
                translate.X -= 10;

            if (ic.IsKeyDown(Keys.Right))
                translate.X += 10;

            if (ic.IsKeyDown(Keys.Menu))
            {
                if (ic.IsKeyDown(Keys.Up))
                    translate.Z += 1;

                if (ic.IsKeyDown(Keys.Down))
                    translate.Z -= 1;
            }
            else
            {
                if (ic.IsKeyDown(Keys.Up))
                    translate.Y += 10;

                if (ic.IsKeyDown(Keys.Down))
                    translate.Y -= 10;
            }

            if (ic.IsMouseButtonDown(MouseButtons.Middle))
            {
                Vector2 direction =
                    ic.MousePosition - ic.LastPosition;

                translate.X = direction.X;
                translate.Y = -direction.Y;
            }

            if (ic.GetWheel() != 0)
            {
                translate.Z = ic.GetWheel() * 2;
            }

            if (translate != Vector3.Zero)
            {
                bool modelTranslated = false;

                foreach (Models model in m_models)
                {
                    if (model.Selected)
                    {
                        modelTranslated = true;

                        model.Translate(
                            translate / 1000,
                            m_camera);
                    }
                }

                if (!modelTranslated)
                {
                    m_camera.Translate(translate * 0.001f);
                }
            }
        }

        private void HandleRotate(float delta)
        {
            InputController ic = InputController.Instance;

            if (ic.IsMouseButtonDown(MouseButtons.Right) &&
                !ic.IsKeyDown(Keys.Menu))
            {
                Vector2 direction =
                    ic.MousePosition - ic.LastPosition;

                if (direction != Vector2.Zero)
                {
                    Vector3 movement = new Vector3(
                        direction.Y,
                        direction.X,
                        0) * delta;

                    bool modelRotated = false;

                    foreach (Models model in m_models)
                    {
                        if (model.Selected)
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

        private void HandleScale(float delta)
        {
            InputController ic = InputController.Instance;

            if (ic.IsMouseButtonDown(MouseButtons.Right) &&
                !ic.IsKeyDown(Keys.Menu))
            {
                float amount =
                    ic.MousePosition.X - ic.LastPosition.X;

                if (amount != 0)
                {
                    amount *= delta;

                    foreach (Models model in m_models)
                    {
                        if (model.Selected)
                        {
                            model.Scale += amount;
                        }
                    }
                }
            }
        }

        internal ISelectable HandlePick(bool select = true)
        {
            InputController ic = InputController.Instance;

            if (ic.IsMouseButtonDown(MouseButtons.Left) ||
                !select)
            {
                Ray ray = HelpMath.GetPickRay(
                    ic.MousePosition,
                    m_camera);

                Models clickedModel = null;
                ISelectable clickedObject = null;

                float closestDistance = float.MaxValue;

                // Check model triangles.
                foreach (Models model in m_models)
                {
                    Matrix transform = model.GetTransform();

                    foreach (ModelMesh mesh in model.Mesh.Meshes)
                    {
                        BoundingSphere sphere =
                            mesh.BoundingSphere;

                        sphere = sphere.Transform(transform);

                        float? sphereHit =
                            ray.Intersects(sphere);

                        if (sphereHit.HasValue &&
                            sphereHit.Value < closestDistance)
                        {
                            float? triangleHit =
                                HelpMath.PickTriangle(
                                    in mesh,
                                    ref ray,
                                    ref transform);

                            if (triangleHit.HasValue &&
                                triangleHit.Value < closestDistance)
                            {
                                closestDistance =
                                    triangleHit.Value;

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

                    float? terrainHit =
                        HelpMath.PickTriangle(
                            in m_terrain,
                            ref ray,
                            ref terrainTransform);

                    if (terrainHit.HasValue &&
                        terrainHit.Value < closestDistance)
                    {
                        closestDistance = terrainHit.Value;
                        clickedModel = null;
                        clickedObject = m_terrain;
                    }
                }

                // Drag-and-drop checks without changing selection.
                if (!select)
                    return clickedObject;

                bool selectionChanged =
                    m_selectedObject != clickedObject;

                if (selectionChanged)
                {
                    if (m_selectedObject != null)
                    {
                        m_selectedObject.Selected = false;
                    }

                    m_selectedObject = clickedObject;
                    m_selectedModel = clickedModel;

                    if (m_selectedObject != null)
                    {
                        m_selectedObject.Selected = true;
                    }

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
