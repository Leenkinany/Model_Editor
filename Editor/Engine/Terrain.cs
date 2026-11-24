
using Editor.Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Editor.Engine
{
    internal class Terrain : ISelectable, IRenderable
    {
        // Terrain data
        public VertexPositionNormalTexture[] Vertices { get; set; }
        public VertexBuffer VertexBuffer { get; set; }
        public int[] Indices { get; set; }
        public IndexBuffer IndexBuffer { get; set; }
        public float[,] Heights { get; set; }
        public int Width { get; set; }
        public int Length { get; set; }
        public int Height { get; set; }
        public int VertexCount { get; set; }
        public int IndexCount { get; set; }
        public GraphicsDevice Device { get; set; }
        public Vector3 LightDirection { get; set; }
        public Texture2D HeightMap { get; set; }
        public Texture2D BaseTexure { get; set; }
        public bool Selected { get; set; } = false;
        public Material Material { get; private set; }

        // IRenderable properties
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public float Scale { get; set; } = 1.0f;

        // Constructor
        public Terrain(Effect effect, Texture2D heightMap, Texture2D baseTexture, int height, GraphicsDevice device)
        {
            Material = new Material();

            Material.Diffuse = baseTexture;
            Material.Effect = effect;

            HeightMap = heightMap;
            Device = device;

            Width = heightMap.Width;
            Length = heightMap.Height;
            Height = height;

            LightDirection = new Vector3(0, 1, 1);

            VertexCount = Width * Length;

            IndexCount = (Width - 1) *
                         (Length - 1) * 6;

            // Generate terrain
            GetHeights();
            CreateVertices();
            CreateIndices();
            GenNormals();

            // Send data to the GPU
            VertexBuffer.SetData(Vertices);
            IndexBuffer.SetData(Indices);
        }

        // Read terrain height map
        private void GetHeights()
        {
            Color[] heightMapData =
                new Color[HeightMap.Width * HeightMap.Height];

            HeightMap.GetData(heightMapData);

            Heights = new float[Width, Length];

            for (int y = 0; y < Length; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    float amount =
                        heightMapData[y * Width + x].R;

                    amount /= 255.0f;

                    Heights[x, y] = amount * Height;
                }
            }
        }

        // Create terrain vertices
        private void CreateVertices()
        {
            VertexBuffer = new VertexBuffer(
                Device,
                typeof(VertexPositionNormalTexture),
                VertexCount,
                BufferUsage.WriteOnly);

            Vertices =
                new VertexPositionNormalTexture[VertexCount];

            for (int y = 0; y < Length; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = y * Width + x;

                    Vertices[index] =
                        new VertexPositionNormalTexture();

                    Vertices[index].Position =
                        new Vector3(
                            x,
                            Heights[x, y],
                            y);

                    Vertices[index].Normal =
                        new Vector3(0, 0, 0);

                    Vertices[index].TextureCoordinate =
                        new Vector2(
                            (float)x / Width,
                            (float)y / Length);
                }
            }
        }

        // Create triangle indices
        private void CreateIndices()
        {
            IndexBuffer = new IndexBuffer(
                Device,
                IndexElementSize.ThirtyTwoBits,
                IndexCount,
                BufferUsage.WriteOnly);

            Indices = new int[IndexCount];

            int i = 0;

            for (int y = 0; y < Length - 1; y++)
            {
                for (int x = 0; x < Width - 1; x++)
                {
                    int upperLeft = y * Width + x;
                    int upperRight = upperLeft + 1;
                    int lowerLeft = upperLeft + Width;
                    int lowerRight = lowerLeft + 1;

                    // Upper triangle
                    Indices[i++] = upperLeft;
                    Indices[i++] = upperRight;
                    Indices[i++] = lowerLeft;

                    // Lower triangle
                    Indices[i++] = lowerLeft;
                    Indices[i++] = upperRight;
                    Indices[i++] = lowerRight;
                }
            }
        }

        // Generate vertex normals
        private void GenNormals()
        {
            for (int i = 0; i < IndexCount; i += 3)
            {
                Vector3 v1 =
                    Vertices[Indices[i]].Position;

                Vector3 v2 =
                    Vertices[Indices[i + 1]].Position;

                Vector3 v3 =
                    Vertices[Indices[i + 2]].Position;

                Vector3 normal =
                    Vector3.Cross(v1 - v3, v1 - v2);

                normal.Normalize();

                Vertices[Indices[i]].Normal += normal;
                Vertices[Indices[i + 1]].Normal += normal;
                Vertices[Indices[i + 2]].Normal += normal;
            }

            for (int i = 0; i < VertexCount; i++)
            {
                if (Vertices[i].Normal != Vector3.Zero)
                {
                    Vertices[i].Normal.Normalize();
                }
                else
                {
                    Vertices[i].Normal = Vector3.Up;
                }
            }
        }

        // Change terrain texture
        public void SetTexture(GameEditor game, string texture)
        {
            if (texture == "DefaultTexture")
            {
                Material.Diffuse = game.DefaultTexture;
            }
            else
            {
                Material.Diffuse = (game.Content.Load<Texture>(texture));
            }
            Material.Diffuse.Tag = texture;
        }

        // Change terrain shader
        public void SetShader(GameEditor game, string effect)
        {
            if (effect == "DefaultEffect")
            {
                Material.Effect = game.DefaultEffect;
            }
            else
            {
                Material.Effect = (game.Content.Load<Effect>(effect));
            }
            Material.Effect.Tag = effect;
        }

        // World transformation
        public Matrix GetTransform()
        {
            return Matrix.CreateScale(Scale) *
                   Matrix.CreateFromYawPitchRoll(
                       Rotation.Y,
                       Rotation.X,
                       Rotation.Z) *
                   Matrix.CreateTranslation(Position);
        }

        // Render terrain
        public void Render()
        {
            Device.SetVertexBuffer(VertexBuffer);
            Device.Indices = IndexBuffer;

            foreach (EffectPass pass in Material.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Device.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    IndexCount / 3);
            }
        }
    }
}
