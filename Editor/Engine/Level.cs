using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Editor.Engine.Interfaces;
using Editor.Engine.Lights;
using System.IO;
using System.Windows.Forms;

namespace Editor.Engine
{
    internal class Level : Iserializable
    {
        private List<Models> m_models = new();

        // Selection
        private Models m_selectedModel = null;
        private ISelectable m_selectedObject = null;
        private bool m_selectionDirty = false;

        // Camera - new version's position
        private Camera m_camera =
            new(new Vector3(0, 400, 500), 16f / 9f);

        // Lighting
        private Light m_light = new()
        {
            Position = new(0, 400, -500),
            Color = new(0.9f, 0.9f, 0.9f)
        };

        // Terrain
        private Terrain m_terrain = null;

        public Level()
        {
        }

        public Camera GetCamera()
        {
            return m_camera;
        }

        public void LoadContent(GameEditor game)
        {
            //m_terrain = new Terrain( game.DefaultEffect,game.DefaultHeightMap,game.DefaultGrass,200, game.GraphicsDevice);
        }

        public void AddModel(Models model)
        {
            m_models.Add(model);
        }
        public void ClearSelectedModels()
        {
            foreach (var model in m_models)
            {
                model.Selected = false;
            }
            if(m_terrain != null)
            {
                m_terrain.Selected = false;
            }
        }

        public List<ISelectable> GetSelectedModels()
        {
            List<ISelectable> models = new();

            foreach (var model in m_models)
            {
                if (model.Selected)
                    models.Add(model);
            }

            if (m_terrain != null)
            {
                if (m_terrain.Selected)
                    models.Add(m_terrain);
            }

            return models;
        }

        public void Render()
        {
            Renderer r = Renderer.Instance;

            r.Camera = m_camera;
            r.Light = m_light;

            foreach (Models m in m_models)
            {
                r.Render(m);
            }

            if (m_terrain != null)
            {
                r.Render(m_terrain);
            }
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

        public void Deserialize(
            BinaryReader _stream,
            GameEditor game)
        {
            m_models.Clear();

            int modelCount = _stream.ReadInt32();

            for (int i = 0; i < modelCount; i++)
            {
                Models model = new();

                model.Deserialize(_stream, game);

                m_models.Add(model);
            }

            m_camera.Deserialize(_stream, game);

            LoadContent(game);

            // Reset selection after loading.
            foreach (Models model in m_models)
            {
                model.Selected = false;
            }

            if (m_terrain != null)
                m_terrain.Selected = false;

            m_selectedModel = null;
            m_selectedObject = null;
            m_selectionDirty = true;
        }

        public override string ToString()
        {
            string s = string.Empty;

            foreach (Models m in m_models)
            {
                if (m.Selected)
                {
                    s += "\nModel : Pos: " +
                         m.Position.ToString() +
                         "  Rot: " +
                         m.Rotation.ToString();
                }
            }

            return m_camera.ToString() + s;
        }

        private void HandleTranslate()
        {
            InputController ic = InputController.Instance;

            Vector3 translate = Vector3.Zero;

            if (ic.IsKeyDown(Keys.Left))
                translate.X += -10;

            if (ic.IsKeyDown(Keys.Right))
                translate.X += 10;

            if (ic.IsKeyDown(Keys.Menu))
            {
                if (ic.IsKeyDown(Keys.Up))
                    translate.Z += 1;

                if (ic.IsKeyDown(Keys.Down))
                    translate.Z += -1;
            }
            else
            {
                if (ic.IsKeyDown(Keys.Up))
                    translate.Y += 10;

                if (ic.IsKeyDown(Keys.Down))
                    translate.Y += -10;
            }

            if (ic.IsMouseButtonDown(MouseButtons.Middle))
            {
                Vector2 dir =
                    ic.MousePosition - ic.LastPosition;

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

                foreach (Models model in m_models)
                {
                    if (model.Selected)
                    {
                        modelTranslated = true;

                        model.Translate(
                            translate / 1000,
                            m_camera
                        );
                    }
                }

                if (!modelTranslated)
                {
                    m_camera.Translate(
                        translate * 0.001f
                    );
                }
            }
        }

        private void HandleRotate(float _delta)
        {
            InputController ic = InputController.Instance;

            if (ic.IsMouseButtonDown(MouseButtons.Right) &&
                (!ic.IsKeyDown(Keys.Menu)))
            {
                Vector2 dir =
                    ic.MousePosition - ic.LastPosition;

                if (dir != Vector2.Zero)
                {
                    Vector3 movement =
                        new Vector3(
                            dir.Y,
                            dir.X,
                            0
                        ) * _delta;

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

        private void HandleScale(float _delta)
        {
            InputController ic = InputController.Instance;

            if (ic.IsMouseButtonDown(MouseButtons.Right) &&
                (!ic.IsKeyDown(Keys.Menu)))
            {
                float l =
                    ic.MousePosition.X -
                    ic.LastPosition.X;

                if (l != 0)
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
            InputController ic =
                InputController.Instance;

            if (ic.IsMouseButtonDown(MouseButtons.Left) ||
                !_select)
            {
                Ray r = HelpMath.GetPickRay(
                    ic.MousePosition,
                    m_camera
                );

                Models clickedModel = null;
                ISelectable clickedObject = null;

                float closestDistance = float.MaxValue;

                // Check model meshes.
                foreach (Models model in m_models)
                {
                    Matrix transform =
                        model.GetTransform();

                    foreach (ModelMesh mesh in model.Mesh.Meshes)
                    {
                        BoundingSphere s =
                            mesh.BoundingSphere;

                        s = s.Transform(transform);

                        float? sphereHit =
                            r.Intersects(s);

                        if (sphereHit.HasValue &&
                            sphereHit.Value < closestDistance)
                        {
                            float? triangleHit =
                                HelpMath.PickTriangle(
                                    in mesh,
                                    ref r,
                                    ref transform
                                );

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
                    Matrix transform = Matrix.Identity;

                    float? terrainHit =
                        HelpMath.PickTriangle(
                            in m_terrain,
                            ref r,
                            ref transform
                        );

                    if (terrainHit.HasValue &&
                        terrainHit.Value < closestDistance)
                    {
                        closestDistance =
                            terrainHit.Value;

                        clickedModel = null;
                        clickedObject = m_terrain;
                    }
                }

                // Drag-and-drop picking must not
                // change the current selection.
                if (!_select)
                    return clickedObject;

                bool selectionChanged =
                    m_selectedObject != clickedObject;

                if (selectionChanged)
                {
                    // Deselect previous object.
                    if (m_selectedObject != null)
                    {
                        m_selectedObject.Selected = false;
                    }

                    // Select new object.
                    m_selectedObject = clickedObject;
                    m_selectedModel = clickedModel;

                    if (m_selectedObject != null)
                    {
                        m_selectedObject.Selected = true;
                    }

                    // Notify GameEditor to update PropertyGrid.
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
