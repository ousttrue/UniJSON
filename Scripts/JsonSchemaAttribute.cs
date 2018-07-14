using System;


namespace UniJSON
{
    public class JsonSchemaAttribute : Attribute
    {
        #region Annotation
        public string Title;
        public string Description;
        #endregion

        public bool Required;

        #region integer, number
        public double Minimum = double.PositiveInfinity;
        #endregion

        #region array
        public int MinItems;
        #endregion

        public EnumSerializationType EnumSerializationType;

        public PropertyExportFlags ExportFlags = PropertyExportFlags.Default;
    }

    public enum EnumSerializationType
    {
        AsString,
        AsInt,
    }
}
