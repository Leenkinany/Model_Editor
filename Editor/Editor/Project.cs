
using System;
using Editor.Editor;
using Editor.Engine;
using Editor.Engine.Interfaces;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.IO;

namespace Editor
{
    internal class Project : Iserializable
    {
        // Project events
        public event AssetsUpdated OnAssetsUpdated;

        // Levels
        public Level CurrentLevel { get; set; } = null;
        public List<Level> Levels { get; set; } = new();

        // Project directories
        public string Folder { get; set; } = string.Empty;
        public string ContentFolder { get; private set; } = string.Empty;
        public string AssetFolder { get; private set; } = string.Empty;
        public string ObjectFolder { get; private set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        // Project file versioning
        private const int CurrentFileVersion = 2;
        private const int FileMagic = 0x4F434532; // "OCE2"

        // Asset monitoring
        public AssetMonitor AssetMonitor { get; private set; } = null;

        public Project() { }

        public Project(GameEditor _game, string _name)
        {
            Folder = Path.GetDirectoryName(_name);
            Name = Path.GetFileNameWithoutExtension(_name);

            if (!Name.ToLower().EndsWith(".oce"))
            {
                Name += ".oce";
            }

            SetProjectFolders();

            Directory.CreateDirectory(ContentFolder);
            Directory.CreateDirectory(AssetFolder);
            Directory.CreateDirectory(ObjectFolder);

            // Prefer the template beside the application.
            string templatePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "ContentTemplate.mgcb"
            );

            // Existing template location on your computer.
            if (!File.Exists(templatePath))
            {
                templatePath =
                    @"C:\Users\leank\Downloads\ContentTemplate.mgcb";
            }

            string destinationPath = Path.Combine(
                ContentFolder,
                "Content.mgcb"
            );

            if (!File.Exists(destinationPath))
            {
                if (!File.Exists(templatePath))
                {
                    throw new FileNotFoundException(
                        "ContentTemplate.mgcb was not found.",
                        templatePath
                    );
                }

                File.Copy(templatePath, destinationPath);
            }

            StartAssetMonitor();
            AddLevel(_game);
        }

        private void SetProjectFolders()
        {
            ContentFolder = Path.Combine(Folder, "Content");
            AssetFolder = Path.Combine(ContentFolder, "bin");
            ObjectFolder = Path.Combine(ContentFolder, "obj");
        }

        private void AssetMon_onAssetUpdated()
        {
            OnAssetsUpdated?.Invoke();
        }

        public void StartAssetMonitor()
        {
            if (string.IsNullOrWhiteSpace(Folder))
                return;

            SetProjectFolders();

            Directory.CreateDirectory(ContentFolder);
            Directory.CreateDirectory(AssetFolder);
            Directory.CreateDirectory(ObjectFolder);

            AssetMonitor?.Dispose();

            AssetMonitor = new AssetMonitor(ObjectFolder, Folder);

            AssetMonitor.OnAssetsUpdated += AssetMon_onAssetUpdated;
        }

        public List<string> GetCompiledAssets()
        {
            return AssetMonitor?.GetCompiledAssets()
                   ?? new List<string>();
        }

        public void AddLevel(GameEditor game)
        {
            CurrentLevel = new();
            CurrentLevel.LoadContent(game);
            Levels.Add(CurrentLevel);
        }

        public void Update(float _delta)
        {
            CurrentLevel?.Update(_delta);
        }

        public void Render()
        {
            CurrentLevel?.Render();
        }

        public void Serialize(BinaryWriter _stream)
        {
            // Header for versioned project files.
            _stream.Write(FileMagic);
            _stream.Write(CurrentFileVersion);

            // Save levels.
            _stream.Write(Levels.Count);

            int clIndex = Levels.IndexOf(CurrentLevel);

            foreach (var level in Levels)
            {
                level.Serialize(_stream);
            }

            // Save selected level and project details.
            _stream.Write(clIndex);
            _stream.Write(Folder);
            _stream.Write(Name);
        }

        public void Deserialize(
            BinaryReader _stream,
            GameEditor game)
        {
            DeserializeCore(_stream, game, null);
        }

        public void Deserialize(
            BinaryReader stream,
            GameEditor editorContent,
            Func<string, ContentManager> createProjectContent)
        {
            DeserializeCore(
                stream,
                editorContent,
                createProjectContent
            );
        }

        private void DeserializeCore(
            BinaryReader stream,
            GameEditor game,
            Func<string, ContentManager> createProjectContent)
        {
            // Determine the selected project's location.
            string loadedFolder = null;

            if (stream.BaseStream is FileStream fileStream)
            {
                loadedFolder = Path.GetDirectoryName(
                    fileStream.Name
                );

                Folder = loadedFolder;
                SetProjectFolders();
            }

            int firstValue = stream.ReadInt32();
            int fileVersion;

            if (firstValue == FileMagic)
            {
                fileVersion = stream.ReadInt32();

                if (fileVersion != CurrentFileVersion)
                {
                    throw new InvalidDataException(
                        "Unsupported project file version: "
                        + fileVersion
                    );
                }
            }
            else
            {
                // Original format: first integer is level count.
                fileVersion = 1;

                stream.BaseStream.Position -= sizeof(int);
            }

            int levelCount = stream.ReadInt32();

            // Preserve the optional project-content callback.
            // Level/Models must use that ContentManager
            // for it to control how assets are loaded.
            if (createProjectContent != null)
            {
                createProjectContent(Folder);
            }

            Levels.Clear();

            for (int i = 0; i < levelCount; i++)
            {
                Level level = new();
                level.Deserialize(stream, game);
                Levels.Add(level);
            }

            int clIndex = stream.ReadInt32();

            CurrentLevel =
                clIndex >= 0 && clIndex < Levels.Count
                ? Levels[clIndex]
                : null;

            string savedFolder = stream.ReadString();
            Name = stream.ReadString();

            // Prefer the location selected by the user,
            // so projects can be moved to another folder.
            Folder = !string.IsNullOrWhiteSpace(loadedFolder)
                ? loadedFolder
                : savedFolder;

            SetProjectFolders();

            StartAssetMonitor();
        }
    }
}
