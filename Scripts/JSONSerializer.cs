namespace UniJSON
{
    public class JSONSerializer
    {
        public static JSONSerializer Create()
        {
            return new JSONSerializer();
        }

        public string Serialize(int value)
        {
            return value.ToString();
        }
    }
}
