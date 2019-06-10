using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Web.Utils
{
    public static class CurrencyMapper
    {
        private static readonly Dictionary<string, string> _currenciesToSymbols;

        public static string GetSymbol(string currency)
        {
            if (_currenciesToSymbols.ContainsKey(currency))
            {
                return _currenciesToSymbols[currency];
            }
            return currency;
        }

        static CurrencyMapper()
        {
            _currenciesToSymbols = new Dictionary<string, string>();

            IEnumerable<RegionInfo> regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(x => new RegionInfo(x.LCID));
            foreach (RegionInfo region in regions)
            {
                if (!_currenciesToSymbols.ContainsKey(region.ISOCurrencySymbol))
                {
                    _currenciesToSymbols[region.ISOCurrencySymbol] = region.CurrencySymbol;
                }
            }
        }
    }
}
