using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Editor.Engine.Interfaces;

namespace Editor.Engine
{
    internal class Terrain : ISelectable, IRenderable
    {
        public bool Selected { get; set; } = false;
        public VertexPositionNormalTexture[] Vertices { get; set; } // Vertex array
        public VertexBuffer VertexBuffer { get; set; } // Vertex Buffer
        public int[] Indices { get; set; } // Index array
        public IndexBuffer IndexBuffer { get; set; } // Index buffer
        public float[,] Heights { get; set; } // Array of vertex heights
        public int Width { get; set; } // Number of vertices on x axis
        public int Length { get; set; } // Number of vertices on z axis
        public int Height { get; set; } // Terrain height factor
        public int VertexCount { get; set; } // Number of vertices
        public int IndexCount { get; set; } // Number of indices
        public GraphicsDevice Device { get; set; } // The graphics device for rendering
        public Vector3 LightDirection { get; set; } // Direction light is emanating from
        public Texture2D HeightMap { get; set; } // Heightmap texture
        public Texture2D BaseTexture { get; set; } // The terrain diffuse texture

        public Material Material { get; private set; }


        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public float Scale { get; set; } = 1.0f;
        public Terrain(Effect effect,Texture2D _heightMap, Texture2D _baseTexture, int _height, GraphicsDevice _device)
        {
            Material = new Material();
            HeightMap = _heightMap;
            Material.Diffuse = _baseTexture;
            Material.Effect = effect;
            BaseTexture = _baseTexture;
            Device = _device;
            Width = _heightMap.Width;
            Length = _heightMap.Height;
            Height = _height;
            LightDirection = new Vector3(0, 1, 1);
            // 1 vertex per pixel
            VertexCount = Width * Length;
            // (Width - 1) * (Length - 1) cells, 2 triangles per cell, 3 indices per triangle
            IndexCount = (Width - 1) * (Length - 1) * 6;

            GetHeights();
            CreateVertices();
            CreateIndices();
            GenNormals();

            // Create the vertex buffer and upload terrain vertices.
            VertexBuffer = new VertexBuffer(
                Device,
                typeof(VertexPositionNormalTexture),
                VertexCount,
                BufferUsage.WriteOnly
            );

            VertexBuffer.SetData(Vertices);

            // Create the index buffer and upload triangle indices.
            IndexBuffer = new IndexBuffer(
                Device,
                IndexElementSize.ThirtyTwoBits,
                IndexCount,
                BufferUsage.WriteOnly
            );

            IndexBuffer.SetData(Indices);
        }

        public void SetTexture(GameEditor _game, string _texture)
        {
            Material.Diffuse = _texture == "DefaultGrass"
                ? _game.DefaultGrass
                : _game.Content.Load<Texture>(_texture);
        }

        public void SetShader(GameEditor _game, string _shader)
        {
            Material.Effect = _shader == "DefaultEffect"
                ? _game.DefaultEffect
                : _game.Content.Load<Effect>(_shader);
        }

        private void GetHeights()
        {
            // Store the height of each terrain vertex.
            Heights = new float[Width, Length];

            // Read pixel colours from the height map.
            Color[] heightMapColors = new Color[Width * Length];
            HeightMap.GetData(heightMapColors);

            // Convert grayscale values into terrain heights.
            for (int x = 0; x < Width; x++)
            {
                for (int z = 0; z < Length; z++)
                {
                    Color pixel = heightMapColors[x + z * Width];

                    // Convert the pixel's red channel from 0-255 to 0-1.
                    float heightValue = pixel.R / 255f;

                    // Scale it by the maximum terrain height.
                    Heights[x, z] = heightValue * Height;
                }
            }
        }

        private void CreateVertices()
        {
            // Create one vertex for every pixel in the height map.
            Vertices = new VertexPositionNormalTexture[VertexCount];

            for (int z = 0; z < Length; z++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = x + z * Width;

                    // Position: X and Z form the grid, Y is the height.
                    Vector3 position = new Vector3(
                        x,
                        Heights[x, z],
                        z
                    );

                    // Normal will be calculated later.
                    Vector3 normal = Vector3.Up;

                    // Texture coordinates allow the grass texture to repeat.
                    Vector2 textureCoordinate = new Vector2(
                        x / 10f,
                        z / 10f
                    );

                    Vertices[index] = new VertexPositionNormalTexture(
                        position,
                        normal,
                        textureCoordinate
                    );
                }
            }
        }

        private void CreateIndices()
        {
            // Each terrain cell consists of two triangles.
            Indices = new int[IndexCount];

            int counter = 0;

            for (int z = 0; z < Length - 1; z++)
            {
                for (int x = 0; x < Width - 1; x++)
                {
                    // Four corners of the current grid cell.
                    int topLeft = x + z * Width;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + Width;
                    int bottomRight = bottomLeft + 1;

                    // First triangle.
                    Indices[counter++] = topLeft;
                    Indices[counter++] = topRight;
                    Indices[counter++] = bottomLeft;

                    // Second triangle.
                    Indices[counter++] = bottomLeft;
                    Indices[counter++] = topRight;
                    Indices[counter++] = bottomRight;
                }
            }
        }

        private void GenNormals()
        {
            // Reset all vertex normals before calculating lighting.
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Normal = Vector3.Zero;
            }

            // Calculate the normal of every triangle.
            for (int i = 0; i < Indices.Length; i += 3)
            {
                int index1 = Indices[i];
                int index2 = Indices[i + 1];
                int index3 = Indices[i + 2];

                Vector3 vertex1 = Vertices[index1].Position;
                Vector3 vertex2 = Vertices[index2].Position;
                Vector3 vertex3 = Vertices[index3].Position;

                // Calculate two edges of the triangle.
                Vector3 edge1 = vertex2 - vertex1;
                Vector3 edge2 = vertex3 - vertex1;

                // Cross product gives the perpendicular direction.
                Vector3 normal = Vector3.Cross(edge2, edge1);

                if (normal != Vector3.Zero)
                {
                    normal.Normalize();
                }

                // Add this triangle's normal to its three vertices.
                Vertices[index1].Normal += normal;
                Vertices[index2].Normal += normal;
                Vertices[index3].Normal += normal;
            }

            // Average the normals shared by neighbouring triangles.
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vector3 normal = Vertices[i].Normal;

                if (normal != Vector3.Zero)
                {
                    normal.Normalize();
                }
                else
                {
                    normal = Vector3.Up;
                }

                Vertices[i].Normal = normal;
            }
        }

        public Matrix GetTransform()
        {
            return Matrix.CreateScale(Scale) *
                   Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) *
                   Matrix.CreateTranslation(Position);
        }
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
                    IndexCount / 3
                );
            }
        }

    }
}