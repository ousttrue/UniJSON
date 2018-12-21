using System;


namespace UniJSON
{
    public class JsonSchemaValidationContext
    {
        string[] m_stack = new string[64];
        int m_pos;

        public JsonSchemaValidationContext(object o)
        {
            Push(o.GetType().Name);
        }

        public ActionDisposer Push(object o)
        {
            m_stack[m_pos++] = o.ToString();
            return new ActionDisposer(Pop);
        }

        public void Pop()
        {
            --m_pos;
        }

        public override string ToString()
        {
            return string.Join(".", m_stack, 0, m_pos);
        }
    }


    public class JsonSchemaValidationException : Exception
    {
        public Exception Error
        {
            get; private set;
        }

        public JsonSchemaValidationException(JsonSchemaValidationContext context, string msg) : base(string.Format("[{0}] {1}", context, msg))
        {
        }

        public JsonSchemaValidationException(JsonSchemaValidationContext context, Exception ex) : base(string.Format("[{0}] {1}", context, ex))
        {
            Error = ex;
        }
    }


    public interface IJsonSchemaValidator
    {
        #region JsonSchema
        void Merge(IJsonSchemaValidator rhs);

        /// <summary>
        /// Parse json schema
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool FromJsonSchema(IFileSystemAccessor fs, string key, JsonNode value);

        void ToJsonScheama(IFormatter f);
        #endregion

        #region Serializer
        /// <summary>
        ///
        /// </summary>
        /// <param name="o"></param>
        /// <returns>return null if validate value</returns>
        JsonSchemaValidationException Validate<T>(JsonSchemaValidationContext context, T value);

        void Serialize<T>(IFormatter f, JsonSchemaValidationContext context, T value);

        void Deserialize<S, T>(S src, ref T dst) where S : IValueNode<S>;
        #endregion
    }
}
