using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Editor.Editor
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

    internal class AssetMonitor 
    {
        public event AssetsUpdated OnAssetsUpdated;
        private FileSystemWatcher m_watcher = null;
        public Dictionary<AssetTypes, List<string>> Assets { get; private set; }
            = new();
        private readonly string m_metaInfo = string.Empty;

        public AssetMonitor(string folderPath, string projectFolder)
        {
            m_metaInfo = Path.Combine(folderPath, ".mgstats");
            m_watcher = new FileSystemWatcher(folderPath);

            m_watcher.Changed += OnChanged;
            m_watcher.Created += OnCreated;
            m_watcher.Deleted += OnDeleted;
            m_watcher.Filter = "*.mgstats";
            m_watcher.IncludeSubdirectories = false;
            m_watcher.EnableRaisingEvents = true;
        }

        private bool AddAsset(AssetTypes assetType, string _assetName)
        {
            if (!Assets.ContainsKey(assetType))
            {
                Assets.Add(assetType,new());
            }
            string assetName = Path.GetFileNameWithoutExtension(_assetName);
            Assets[assetType].Add(assetName);

            return true;
        }

        public void UpdateAssetsDB()
        {
            if (!File.Exists(m_metaInfo))
            {
                Debug.WriteLine($"Metadata file not found: {m_metaInfo}");
                return;
            }
            bool updated = false;
            AssetTypes assetType = AssetTypes.MODEL;
            using var inStream = new FileStream(m_metaInfo,
                                                FileMode.Open,
                                                FileAccess.Read,
                                                FileShare.ReadWrite);
            using var streamReader = new StreamReader(inStream);
            string[] content = streamReader.ReadToEnd().Split(Environment.NewLine);

            foreach(string line in content)
            {
                if (string.IsNullOrEmpty(line)) continue;
                string[] fields = line.Split(',');
                if (fields[0] == "Source File") continue;
                switch(fields[2])
                {
                    case "\"ModelProcessor\"":
                        assetType = AssetTypes.MODEL;
                        break;
                    case "\"TextureProcessor\"":
                        assetType = AssetTypes.TEXTURE;
                        break;
                    case "\"SongProcessor\"":
                        assetType = AssetTypes.AUDIO;
                        break;
                    case "\"EffectProcessor\"":
                        assetType = AssetTypes.EFFECT;
                        break;
                    default:
                        Debug.Assert(false, "Unhandled processor.");
                        break;
                }

                if (AddAsset(assetType, fields[1])) updated = true;
            }
            if (updated) OnAssetsUpdated?.Invoke();
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Assets.Clear();
            UpdateAssetsDB();
        }
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            Assets.Clear();
            UpdateAssetsDB();
        }
        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Assets.Clear();
            OnAssetsUpdated?.Invoke();
        }

        private void OnAssetRenamed(
            object sender,
            RenamedEventArgs e)
        {
            UpdateAssetsDB();
        }

        public List<string> GetCompiledAssets()
        {
            UpdateAssetsDB();

            List<string> assets = new List<string>();

            foreach (var assetType in Assets)
            {
                foreach (string asset in assetType.Value)
                {
                    if (!assets.Contains(asset))
                    {
                        assets.Add(asset);
                    }
                }
            }

            return assets;
        }

        public void Dispose()
        {
            if (m_watcher != null)
            {
                m_watcher.EnableRaisingEvents = false;

                m_watcher.Created -= OnChanged;
                m_watcher.Changed -= OnChanged;
                m_watcher.Deleted -= OnDeleted;
                m_watcher.Renamed -= OnAssetRenamed;

                m_watcher.Dispose();
                m_watcher = null;
            }
        }
    }
}
