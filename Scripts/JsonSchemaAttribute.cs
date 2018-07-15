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
        public double Minimum = double.NaN;
        public double Maximum = double.NaN;
        public double MultipleOf;
        #endregion

        #region string
        public string Pattern;
        #endregion

        #region array
        public int MinItems;
        public int MaxItems;
        #endregion

        #region object
        public int MinProperties;
        #endregion

        public EnumSerializationType EnumSerializationType;

        public PropertyExportFlags ExportFlags = PropertyExportFlags.Default;

        /// <summary>
        /// skip validator comparison
        /// </summary>
        public bool Empty;
    }

    public enum EnumSerializationType
    {
        AsString,
        AsInt,
    }
}
