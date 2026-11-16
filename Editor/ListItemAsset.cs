using Editor.Engine;

namespace GUI.Editor
{
    internal class ListItemAsset
    {
        public string Name { get; set; }

        public AssetTypes Type { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
