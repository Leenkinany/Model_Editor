using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Editor.Engine
{
    internal enum AssetTypes
    {
        NONE,
        MODEL,
        TEXTURE,
        FONT,
        AUDIO,
        EFFECT
    }
    public delegate void AssetsUpdated();

    internal class AssetMonitor : IDisposable
    {
        private FileSystemWatcher m_watcher;
        private readonly string m_projectFolder;

        public Dictionary<AssetTypes, List<string>> Assets { get; private set; }
            = new Dictionary<AssetTypes, List<string>>();

        public event AssetsUpdated OnAssetsUpdated;
        public AssetMonitor(string folderPath, string projectFolder)
        {
            m_projectFolder = projectFolder;
            m_watcher = new FileSystemWatcher(folderPath);

            m_watcher.IncludeSubdirectories = true;
            m_watcher.NotifyFilter =
                NotifyFilters.FileName |
                NotifyFilters.LastWrite |
                NotifyFilters.CreationTime;

            m_watcher.Created += OnAssetChanged;
            m_watcher.Changed += OnAssetChanged;
            m_watcher.Deleted += OnAssetChanged;
            m_watcher.Renamed += OnAssetRenamed;

            m_watcher.EnableRaisingEvents = true;
            UpdateAssetsDB();
        }
        private bool AddAsset(AssetTypes assetType, string assetName)
        {
            if (!Assets.ContainsKey(assetType))
            {
                Assets.Add(assetType, new List<string>());
            }

            assetName = Path.GetFileNameWithoutExtension(assetName);

            if (Assets[assetType].Contains(assetName))
                return false;

            Assets[assetType].Add(assetName);
            return true;
        }

        public void UpdateAssetsDB()
        {
            bool updated = false;

            Assets.Clear();

            if (!Directory.Exists(m_projectFolder))
                return;

            string[] statsFiles = Directory.GetFiles(
                m_projectFolder,
                "*.mgstats",
                SearchOption.AllDirectories
            );

            foreach (string statsFile in statsFiles)
            {
                using var fileStream = new FileStream(
                    statsFile,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite
                );

                using var streamReader = new StreamReader(fileStream);

                string[] content = streamReader.ReadToEnd()
                    .Split(Environment.NewLine);

                foreach (string line in content)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;

                    string[] fields = line.Split(',');

                    if (fields.Length < 4 ||
                        fields[0] == "Source File")
                        continue;

                    AssetTypes assetType = AssetTypes.NONE;

                    switch (fields[2].Trim().Trim('"'))
                    {
                        case "ModelProcessor":
                            assetType = AssetTypes.MODEL;
                            break;

                        case "TextureProcessor":
                            assetType = AssetTypes.TEXTURE;
                            break;

                        case "SongProcessor":
                        case "SoundEffectProcessor":
                            assetType = AssetTypes.AUDIO;
                            break;

                        case "EffectProcessor":
                            assetType = AssetTypes.EFFECT;
                            break;

                        case "FontDescriptionProcessor":
                            assetType = AssetTypes.FONT;
                            break;

                        default:
                            System.Diagnostics.Debug.WriteLine(
                                "Unhandled processor: " + fields[2]
                            );
                            break;
                    }

                    if (assetType != AssetTypes.NONE)
                    {
                        string assetName = fields[1].Trim().Trim('"');

                        if (AddAsset(assetType, assetName))
                            updated = true;
                    }
                }
            }

            if (updated)
                OnAssetsUpdated?.Invoke();
        }

        private void OnAssetChanged(object sender, FileSystemEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Asset {e.ChangeType}: {e.FullPath}"
            );

            if (Path.GetExtension(e.FullPath)
                .Equals(".mgstats", StringComparison.OrdinalIgnoreCase))
            {
                UpdateAssetsDB();
            }
        }

        private void OnAssetRenamed(object sender, RenamedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Asset renamed: {e.OldFullPath} -> {e.FullPath}"
            );

            if (Path.GetExtension(e.FullPath)
                .Equals(".mgstats", StringComparison.OrdinalIgnoreCase))
            {
                UpdateAssetsDB();
            }
        }

        public void Dispose()
        {
            if (m_watcher != null)
            {
                m_watcher.EnableRaisingEvents = false;

                m_watcher.Created -= OnAssetChanged;
                m_watcher.Changed -= OnAssetChanged;
                m_watcher.Deleted -= OnAssetChanged;
                m_watcher.Renamed -= OnAssetRenamed;

                m_watcher.Dispose();
                m_watcher = null;
            }
        }

        public List<string> GetCompiledAssets()
        {
            List<string> assets = new List<string>();

            string[] statsFiles = Directory.GetFiles(
                m_projectFolder,
                "*.mgstats",
                SearchOption.AllDirectories
            );

            foreach (string statsFile in statsFiles)
            {
                string[] lines = File.ReadAllLines(statsFile);

                // Skip the CSV header
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] columns = lines[i].Split(',');

                    if (columns.Length < 4)
                        continue;

                    string destination = columns[1].Trim('"');

                    string assetName = Path.GetFileNameWithoutExtension(
                        destination
                    );

                    if (!assets.Contains(assetName))
                        assets.Add(assetName);
                }
            }

            return assets;
        }

    }
}
