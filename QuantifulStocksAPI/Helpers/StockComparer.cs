using System;
using System.Diagnostics.CodeAnalysis;
using QuantifulStocksAPI.Entities;

namespace QuantifulStocksAPI.Helpers
{
	public class StockComparer : IEqualityComparer<StockData>
	{
        public bool Equals(StockData? x, StockData? y)
        {
            return (x.Date == y.Date && x.Name == y.Name);
        }

        public int GetHashCode(StockData obj)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(obj, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashStockName = obj.Name == null ? 0 : obj.Name.GetHashCode();

            //Get hash code for the Code field.
            int hashStockDate = obj.Date.GetHashCode();

            //Calculate the hash code for the stock.
            return hashStockName ^ hashStockDate;
        }
    }
}

