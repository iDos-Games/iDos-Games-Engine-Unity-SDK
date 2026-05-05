using System;

namespace IDosGames
{
    public class TitleConfig
    {
        public TitlePublicConfigurationModel TitlePublicConfiguration { get; private set; } = new ();

        public event Action OnAnyUpdated;
        public event Action OnTitlePublicConfigurationUpdated;
        public event Action OnTitleCustomDataUpdated;
        public event Action OnCurrencyDefinitionsUpdated;
        public event Action OnItemDefinitionsUpdated;
        
        internal TitleConfig() { }

        internal void ApplyTitlePublicConfiguration(TitlePublicConfigurationModel data)
        {
            TitlePublicConfiguration = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchTitleCustomData(TitleCustomDataResponse data)
        {
            TitlePublicConfiguration.TitleCustomData = data;
            OnTitleCustomDataUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchItemDefinitions(ItemDefinitions data)
        {
            TitlePublicConfiguration.Item = data;
            OnItemDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchCurrencyDefinitions(CurrencyDefinitions data)
        {
            TitlePublicConfiguration.Currency = data;
            OnCurrencyDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }
    }
}
