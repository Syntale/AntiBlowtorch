using Rocket.API;

namespace RestoreMonarchy.AntiBlowtorch
{
    public class AntiBlowtorchConfiguration : IRocketPluginConfiguration
    {
        public string MessageColor { get; set; }

        public void LoadDefaults()
        {
            MessageColor = "yellow";
        }
    }
}
