namespace UniJSON
{
    public static class StringExtensions
    {
        public static ListTreeNode<JsonValue> ParseAsJson(this string json)
        {
            return JsonParser.Parse(json);
        }
    }
}
