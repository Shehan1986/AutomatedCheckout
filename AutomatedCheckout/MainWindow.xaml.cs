using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AutomatedCheckout
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Checkout _checkout;
        private ObservableCollection<ProductItem> _availableItems;
        private ObservableCollection<ProductItem> _checkoutItems;

        private List<(int ItemId, bool IsWeightBased, double Quantity)> checkoutItemsList;

        public ObservableCollection<ProductItem> AvailableItems
        {
            get => _availableItems;
            set
            {
                _availableItems = value;
                OnPropertyChanged(nameof(AvailableItems));
            }
        }

        public ObservableCollection<ProductItem> CheckoutItems
        {
            get => _checkoutItems;
            set
            {
                _checkoutItems = value;
                OnPropertyChanged(nameof(CheckoutItems));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            InitializeProductsList();
        }

        private void InitializeProductsList()
        {
            _checkout = new Checkout();
            checkoutItemsList = new List<(int ItemId, bool IsWeightBased, double Quantity)>();

            AvailableItems = new ObservableCollection<ProductItem>
            {
                new ProductItem { Id = 1, Name = "Toothpaste", Price = 24.95, IsWeightBased = false, PriceType = "each" },
                new ProductItem { Id = 2, Name = "Cheese", Price = 59.00, IsWeightBased = true, PriceType = "per kg" },
                new ProductItem { Id = 3, Name = "Bread", Price = 11.95, IsWeightBased = false, PriceType = "each" },
                new ProductItem { Id = 4, Name = "Coffee", Price = 22.49, IsWeightBased = false, PriceType = "each" },
                new ProductItem { Id = 5, Name = "Apples", Price = 32.95, IsWeightBased = true, PriceType = "per kg" },
                new ProductItem { Id = 6, Name = "Flour", Price = 11.95, IsWeightBased = false, PriceType = "each" },
                new ProductItem { Id = 7, Name = "Ground Beef", Price = 93.00, IsWeightBased = true, PriceType = "per kg" },
                new ProductItem { Id = 8, Name = "Milk", Price = 9.32, IsWeightBased = false, PriceType = "each" }
            };

            CheckoutItems = new ObservableCollection<ProductItem>();

            DiscountListView.Items.Clear();
        }

        private void lvItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = lvItems.SelectedItem as ProductItem;
            if (selectedItem == null)
                return;

            AddItemToCheckout(selectedItem);
        }

        private void lvCheckout_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = lvCheckout.SelectedItem as ProductItem;
            if (selectedItem == null)
                return;

            MessageBoxResult result = MessageBox.Show(
                $"Do you want to delete {selectedItem.Name} from checkout?",
                "Delete Item!",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                RemoveItemFromCheckoutList(selectedItem);
            }
        }

        // this function is use to Add Item To Checkout
        private void AddItemToCheckout(ProductItem selectedItem)
        {
            if (selectedItem.IsWeightBased)
            {
                var weightDialog = new InputDialog("Enter Weight (kg)");
                weightDialog.Owner = this; 

                if (weightDialog.ShowDialog() == true)
                {
                    checkoutItemsList.Add((selectedItem.Id, true, weightDialog.Value));

                    RebuildCheckout();
                }
            }
            else
            {
                var quantityDialog = new InputDialog("Enter Quantity");
                quantityDialog.Owner = this; 

                if (quantityDialog.ShowDialog() == true)
                {
                   int quantity = (int)quantityDialog.Value;
                    for (int i = 0; i < quantity; i++)
                    {
                        checkoutItemsList.Add((selectedItem.Id, false, 1));
                    }

                    RebuildCheckout();
                }
            }
        }

        // this function is use to Remove Item From Checkout List
        private void RemoveItemFromCheckoutList(ProductItem item)
        {
            if (item.IsWeightBased)
            {
                var indx = checkoutItemsList.FindIndex(x => x.ItemId == item.Id);
                if (indx >= 0)
                    checkoutItemsList.RemoveAt(indx);
            }
            else
            {
                checkoutItemsList.RemoveAll(x => x.ItemId == item.Id);
            }

            RebuildCheckout();
        }

        private void RebuildCheckout()
        {
           _checkout = new Checkout();
            CheckoutItems.Clear();

            DiscountListView.Items.Clear();

            Dictionary<int, ProductItem> addedItems = new Dictionary<int, ProductItem>();

            foreach (var item in checkoutItemsList)
            {
                if (item.IsWeightBased)
                {
                    _checkout.AddItem(item.ItemId, item.Quantity);
                }
                else
                {
                    _checkout.AddItem(item.ItemId);
                }

                var originalItem = AvailableItems.FirstOrDefault(x => x.Id == item.ItemId);
                if (originalItem == null)
                    continue;

                if (addedItems.TryGetValue(item.ItemId, out var uiItem))
                {
                    if (item.IsWeightBased)
                    {
                        uiItem.Quantity += item.Quantity;
                    }
                    else
                    {
                        uiItem.Quantity += 1;
                    }
                }
                else
                {
                    var newItem = new ProductItem
                    {
                        Id = originalItem.Id,
                        Name = originalItem.Name,
                        Price = originalItem.Price,
                        Quantity = item.IsWeightBased ? item.Quantity : 1,
                        IsWeightBased = originalItem.IsWeightBased,
                        UnitLabel = originalItem.IsWeightBased ? "kg" : "pcs",
                        PriceType = originalItem.PriceType
                    };
                    CheckoutItems.Add(newItem);
                    addedItems[item.ItemId] = newItem;
                }
            }

            txbTotal.Text = "Total amount: 0 Kr";
        }

        //this function is use to Calculate Total
        private void btnCalculateTotal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DiscountListView.Items.Clear();

                var result = _checkout.Sum();
                double total = result.Total;
                List<string> discounts = result.Discounts;

                txbTotal.Text = $"Total amount: {total:F2} Kr";

                foreach (var discount in discounts)
                {
                    DiscountListView.Items.Add(discount);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating total: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    
}