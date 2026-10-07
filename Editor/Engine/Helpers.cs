using System.IO;
using Microsoft.Xna.Framework;

namespace Editor.Engine
{
    internal class HelperSerialize
    {
        public static void Vec3(BinaryWriter _stream, Vector3 _vec)
        {
            _stream.Write(_vec.X);
            _stream.Write(_vec.Y);
            _stream.Write(_vec.Z);
        }
    }

    internal class HelperDeserialize
    {
        public static Vector3 Vec3(BinaryReader _stream)
        {
            Vector3 v = Vector3.Zero;
            v.X = _stream.ReadSingle();
            v.Y = _stream.ReadSingle();
            v.Z = _stream.ReadSingle();
            return v;
        }
    }
}
