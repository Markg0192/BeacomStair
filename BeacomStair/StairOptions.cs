namespace BeacomStair
{
    internal sealed class StairOptions
    {
        public double Width { get; set; } = 900.0;

        public string StringerProfile { get; set; } =
            "PFC-180*75*20";

        public string TreadProfile { get; set; } =
            "PL10";

        public string TreadSidePlateProfile { get; set; } =
            "PL8";

        public string RailProfile { get; set; } =
            "CHS33.7*3.2";

        public string PlatformSupportAngleProfile { get; set; } =
            "RSA50*50*6";

        public static StairOptions CreateDefault()
        {
            return new StairOptions();
        }
    }
}
