namespace AstroDeepak.Views
{
    public class RemedyCheckItem
    {
        public string Name { get; set; } = string.Empty;

        // Left checkbox: is this remedy included in the selection at all.
        public bool IsChecked { get; set; }

        // Right checkboxes - independent, a remedy can be both.
        public bool IsPermanent { get; set; }
        public bool IsYearly { get; set; }
    }
}