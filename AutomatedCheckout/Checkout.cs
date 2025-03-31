using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AutomatedCheckout
{
    public class Checkout
    {
        // 1 Toothpaste 2 Cheese 3 Bread 4 Coffee 5 Apples 6 Flour 7 Ground 8 Milk
        
        private List<(int ItemId, double QTY, double Price)> items = new List<(int, double, double)>();

        private Dictionary<int, double> itemPrices = new Dictionary<int, double>
        {
            { 1, 24.95 },
            { 2, 59.00 },
            { 3, 11.95 },
            { 4, 22.49 },
            { 5, 32.95 },
            { 6, 11.95 },
            { 7, 93.00 },
            { 8, 9.32 }
        };

        private Dictionary<int, bool> isWeightBasedItem = new Dictionary<int, bool>
        {
            { 1, false },
            { 2, true },
            { 3, false },
            { 4, false },
            { 5, true },
            { 6, false },
            { 7, true },
            { 8, false }
        };

        //AddItem(itemId : int) is called each time an item that does not have a price per weight is entered into the checkout.
        public void AddItem(int itemId)
        {
            if (isWeightBasedItem[itemId])
                throw new InvalidOperationException("Weight required for this item.");

            items.Add((itemId, 1, itemPrices[itemId]));
        }

        //AddItem(itemId : int, weight : double) is called each time an item that does have a price per weight is entered into the checkout.
        public void AddItem(int itemId, double weight)
        {
            if (!isWeightBasedItem[itemId])
                throw new InvalidOperationException("Weight not required for this item.");

            items.Add((itemId, weight, itemPrices[itemId]));
        }

        public (double Total, List<string> Discounts) Sum()
        {
            double regularTotal = 0;
            double discountedTotal = 0;
            List<string> discounts = new List<string>();

            foreach (var item in items)
            {
                if (isWeightBasedItem[item.ItemId])
                {
                    regularTotal += item.QTY * item.Price;
                }
                else
                {
                    regularTotal += item.Price;
                }
            }

            // Buy two packs of coffee for 40kr. 
            int coffeeCount = items.Count(i => i.ItemId == 4);
            if (coffeeCount >= 2)
            {
                double coffeeRegular = coffeeCount * itemPrices[4];
                double coffeeDiscounted = (coffeeCount / 2) * 40 + (coffeeCount % 2 * itemPrices[4]);
                double coffeeSavings = coffeeRegular - coffeeDiscounted;

                if (coffeeSavings > 0)
                    discounts.Add($"Coffee 2 for 40kr: Save {coffeeSavings:F2}Kr");

                discountedTotal += coffeeDiscounted;
            }
            else
            {
                discountedTotal += coffeeCount * itemPrices[4];
            }

            //Buy three packs of toothpaste and pay for two.
            int toothpasteQty = items.Count(i => i.ItemId == 1);
            if (toothpasteQty >= 3)
            {
                double toothpasteRegular = toothpasteQty * itemPrices[1];
                double toothpasteDiscounted = (toothpasteQty / 3) * (itemPrices[1] * 2) + (toothpasteQty % 3 * itemPrices[1]);
                double toothpasteSavings = toothpasteRegular - toothpasteDiscounted;

                if (toothpasteSavings > 0)
                    discounts.Add($"Toothpaste 3 for 2: Save {toothpasteSavings:F2}Kr");

                discountedTotal += toothpasteDiscounted;
            }
            else
            {
                discountedTotal += toothpasteQty * itemPrices[1];
            }

            //Shop other items for over 150kr and you can buy appels for the price of 16.95Kr / kg
            double applesTotalRegular = 0;
            double applesTotalDiscounted = 0;
            
            foreach (var item in items.Where(i => isWeightBasedItem[i.ItemId] && i.ItemId != 5))
            {
                discountedTotal += item.QTY * item.Price;
            }

            double subtotalForAppleCheck = discountedTotal;

            var appleItems = items.Where(i => i.ItemId == 5).ToList();
            foreach (var apple in appleItems)
            {
                applesTotalRegular += apple.QTY * apple.Price;

                if (subtotalForAppleCheck > 150)
                {
                    applesTotalDiscounted += apple.QTY * 16.95;
                }
                else
                {
                    applesTotalDiscounted += apple.QTY * apple.Price;
                }
            }

            if (subtotalForAppleCheck > 150 && appleItems.Any())
            {
                double appleSavings = applesTotalRegular - applesTotalDiscounted;
                if (appleSavings > 0)
                    discounts.Add($"Apples discount (16.95Kr/kg): Save {appleSavings:F2}Kr");
            }

            discountedTotal += applesTotalDiscounted;

            discountedTotal += items.Where(i => !isWeightBasedItem[i.ItemId] && i.ItemId != 4 && i.ItemId != 3 && i.ItemId != 1)
                .Sum(i => i.QTY * i.Price);

            double totalSavings = regularTotal - discountedTotal;
            if (totalSavings > 0)
                discounts.Add($"Total savings: {totalSavings:F2}Kr");

            return (discountedTotal, discounts);
        }
    }
}