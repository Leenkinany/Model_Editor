
using System;
using System.ComponentModel;

namespace Editor.Engine
{
    public class DiffuseTextureConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(
            ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(
            ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(
            ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(
                new string[]
                {
                    "Grass",
                    "HeightMap",
                    "Metal"
                });
        }
    }
}
