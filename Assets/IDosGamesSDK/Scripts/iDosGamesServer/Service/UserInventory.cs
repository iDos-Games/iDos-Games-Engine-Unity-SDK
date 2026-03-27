using IDosGames.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IDosGames
{
    public class UserInventory
    {
        private static readonly Dictionary<string, int> _eachItemAmounts = new();
        private static readonly Dictionary<string, long> _virtualCurrencyAmounts = new();
        private static readonly Dictionary<SpinTicketType, int> _spinTickets = new();
        private static readonly Dictionary<ChestKeyFragmentType, int> _chestKeyFragments = new();

    }
}
