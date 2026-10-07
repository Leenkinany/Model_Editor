using System.IO;
using Microsoft.Xna.Framework.Content;

namespace Editor.Engine.Interfaces
{
    internal interface Iserializable
    {
        public void Serialize(BinaryWriter _stream);
        public void Deserialize(BinaryReader _stream, ContentManager _content);
    }
}
