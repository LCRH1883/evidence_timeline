namespace evidence_timeline.Models
{
    public class CaseSettings
    {
        public bool ShowLeftPane { get; set; } = true;
        public bool ShowRightPane { get; set; } = true;
        public bool ShowBottomPane { get; set; } = true;
        public bool SortNewestFirst { get; set; } = true;
        public double ZoomLevel { get; set; } = 1.0;
    }
}
