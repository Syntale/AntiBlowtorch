using Rocket.API;

namespace RestoreMonarchy.AntiBlowtorch
{
    public class AntiBlowtorchConfiguration : IRocketPluginConfiguration
    {
        public string MessageColor { get; set; }
        public string MessageIconUrl { get; set; }
        public float BlockTimeSeconds { get; set; }
        public float MessageThrottleTimeSeconds { get; set; }
        public bool IgnoreOwnerAndGroup { get; set; }

        public void LoadDefaults()
        {
            MessageColor = "yellow";
            MessageIconUrl = "https://i.imgur.com/3bYaNFM.png";
            BlockTimeSeconds = 60;
            MessageThrottleTimeSeconds = 2f;
            IgnoreOwnerAndGroup = false;
        }
    }
}
