using System.Collections.Generic;

namespace IDosGames
{
    public class TitlePublicConfigurationModel
    {
        public Dictionary<string, string> ImageData { get; set; }
        public Dictionary<string, string> AssetBundle { get; set; }

        public TitleCustomDataResponse TitleCustomData { get; set; }
        public CurrencyDefinitions Currency { get; set; }
        public ItemDefinitions Item { get; set; }

        public CharacterDefinitions Character { get; set; }
        public TimedEventDefinitions TimedEvent { get; set; }
        public QuestDefinitions Quest { get; set; }
    }
}
