using Editor.Engine;
using Editor.Engine.Interfaces;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;

namespace Editor   
{
    internal class Project : Iserializable
    {
        public Level CurrentLevel { get; set; } = null;
        public List<Level> Levels { get; set; } = new();
        public string Folder { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        private const int CurrentFileVersion = 2;
        private const int FileMagic = 0x4F434532; // "OCE2"
        private AssetMonitor m_assetMonitor;
        public AssetMonitor AssetMonitor => m_assetMonitor;

        public Project() { }

        public Project(GameEditor _game, string _name)
        {
            Folder = Path.GetDirectoryName(_name);
            Name = Path.GetFileNameWithoutExtension(_name);
            if (! Name.ToLower().EndsWith(".oce"))
            {
                Name += ".oce";
            }
            AddLevel(_game);
            StartAssetMonitor();
            string templatePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "ContentTemplate.mgcb");

            string destinationPath = Path.Combine(
                Folder,
                "Content.mgcb"
            );

            if (File.Exists(templatePath) && !File.Exists(destinationPath))
            {
                File.Copy(templatePath, destinationPath);
            }
        }

        public void AddLevel(GameEditor _game)
        {
            CurrentLevel = new();
            CurrentLevel.LoadContent(_game);
            Levels.Add(CurrentLevel);
        }

        public void Update(float _delta)
        {
            CurrentLevel?.Update(_delta);
        }
        public void Render()
        {
            CurrentLevel.Render();
        }

        public void Serialize(BinaryWriter _stream)
        {
            // Header for the new project file format.
            _stream.Write(FileMagic);
            _stream.Write(CurrentFileVersion);

            // Existing project data.
            _stream.Write(Levels.Count);

            int clIndex = Levels.IndexOf(CurrentLevel);

            foreach (var level in Levels)
            {
                level.Serialize(_stream);
            }

            _stream.Write(clIndex);
            _stream.Write(Folder);
            _stream.Write(Name);
        }


        public void Deserialize(BinaryReader _stream, ContentManager _content)
        {
            int firstValue = _stream.ReadInt32();
            int fileVersion;

            if (firstValue == FileMagic)
            {
                // New versioned project format.
                fileVersion = _stream.ReadInt32();

                if (fileVersion != CurrentFileVersion)
                {
                    throw new InvalidDataException(
                        "Unsupported project file version: " + fileVersion);
                }
            }
            else
            {
                // Original project format: first value was level count.
                fileVersion = 1;

                // Return to the beginning so the original level count
                // can be read normally.
                _stream.BaseStream.Position -= sizeof(int);
            }

            int levelCount = _stream.ReadInt32();

            for (int i = 0; i < levelCount; i++)
            {
                Level level = new();

                // Version-aware loading will be connected next.
                level.Deserialize(_stream, _content);

                Levels.Add(level);
            }

            int clIndex = _stream.ReadInt32();
            CurrentLevel = Levels[clIndex];

            Folder = _stream.ReadString();
            Name = _stream.ReadString();

            StartAssetMonitor();
        }


        public void Deserialize(
            BinaryReader stream,
            ContentManager editorContent,
            Func<string, ContentManager> createProjectContent)
        {
            Folder = Path.GetDirectoryName(
                        ((FileStream)stream.BaseStream).Name
                    );
            int firstValue = stream.ReadInt32(); 
            
            int fileVersion;

            if (firstValue == FileMagic)
            {
                fileVersion = stream.ReadInt32();

                if (fileVersion != CurrentFileVersion)
                    throw new InvalidDataException(
                        "Unsupported project file version: " + fileVersion);
            }
            else
            {
                fileVersion = 1;
                stream.BaseStream.Position -= sizeof(int);
            }

            int levelCount = stream.ReadInt32();

            // Project folder is stored after the levels, so we use
            // the selected file's folder before reading model assets.
            ContentManager projectContent =
                createProjectContent(Folder);

            Levels.Clear();

            for (int i = 0; i < levelCount; i++)
            {
                Level level = new();

                level.Deserialize(
                    stream,
                    editorContent,
                    projectContent,
                    fileVersion
                );

                Levels.Add(level);
            }

            int clIndex = stream.ReadInt32();
            CurrentLevel = Levels[clIndex];

            Folder = stream.ReadString();
            Name = stream.ReadString();

            StartAssetMonitor();
        }

        public void StartAssetMonitor()
        {
            if (string.IsNullOrWhiteSpace(Folder))
                return;

            string assetFolder = Path.Combine(Folder, "Assets");

            // Create the asset folder if it doesn't exist.
            Directory.CreateDirectory(assetFolder);

            m_assetMonitor?.Dispose();
            m_assetMonitor = new AssetMonitor(assetFolder, Folder);
        }

        public List<string> GetCompiledAssets()
        {
            return m_assetMonitor?.GetCompiledAssets()
                   ?? new List<string>();
        }
    }
}
