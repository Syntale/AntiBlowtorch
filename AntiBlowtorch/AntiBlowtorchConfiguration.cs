using Rocket.API;

namespace RestoreMonarchy.AntiBlowtorch
{
    public class AntiBlowtorchConfiguration : IRocketPluginConfiguration
    {
        public string MessageColor { get; set; }
        public float BlockTime { get; set; }
        public float MessageClearTime { get; set; }
        public float DamageClearTime { get; set; }

        public void LoadDefaults()
        {
            MessageColor = "yellow";
            BlockTime = 60;
            MessageClearTime = 10;
            DamageClearTime = 5;
        }
    }
}
