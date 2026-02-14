namespace ArtPrompter.Models
{
    public class PromptItem
    {
        public PromptItem(int index, string value)
        {
            Index = index;
            Value = value;
        }

        public int Index { get; set; }

        public string Value { get; }
    }
}
