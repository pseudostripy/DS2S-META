using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media;

namespace DS2S_META
{
    /// <summary>
    /// Interaction logic for CovenantControl.xaml
    /// </summary>
    public partial class RandomizerControl : METAControl
    {
        private Color PURPLE = Color.FromArgb(0xFF, 0xB1, 0x59, 0xCC);
        private Color LIGHTRED = Color.FromArgb(0xFF, 0xDA, 0x4D, 0x4D);
        private Color LIGHTGREEN = Color.FromArgb(0xFF, 0x87, 0xCC, 0x59);

        private List<int> placedKeys;

        private Dictionary<int, ItemLot> VanillaLots;
        internal bool isRandomized = false;


        public RandomizerControl()
        {
            InitializeComponent();
        }



        //Handles the "Seeed..." label on the text box
        private void FixSeedVisibility()
        {
            if (txtSeed.Text == "")
                lblSeed.Visibility = Visibility.Visible;
            else
                lblSeed.Visibility = Visibility.Hidden;

        }

        private void txtSeed_TextChanged(object sender, TextChangedEventArgs e)
        {
            FixSeedVisibility();
        }

        private bool CheckValidPlacement(int paramID, DropInfo loot)
        {
            // paramID represents a specific pickup in the game.
            // DropInfo contains what we want to put into it.


            return false;
        }

        private void randomize()
        {
            // Need to get a list of the vanilla item lots C#.8 pleeeease :(
            if (VanillaLots==null)
                VanillaLots = Hook.GetVanillaLots();
            Dictionary<int, ItemLot> NewLots = new Dictionary<int, ItemLot>();

            // Make into flat list of stuff:
            var flatlist = VanillaLots.SelectMany(kvp => kvp.Value.Lot).ToList();

            
            //flatlist.Shuffle();

            int seed = 1;
            Random rng = new Random(seed);

            foreach (var kvp in VanillaLots)
            {
                // unpack:
                int paramID = kvp.Key;
                ItemLot lot = kvp.Value;
                
                // At some point, should sort the lots to place them in a more sensible order...
                if (lot.NumDrops == 0)
                    continue;

                List<DropInfo> dropList = new List<DropInfo>();
                for (int i = 0; i < lot.NumDrops; i++)
                {
                    while (remainingitemindex)
                    {
                        int index = rng.Next(flatlist.Count());
                        DropInfo randdrop = flatlist[index]; // get the item we want to place here

                        // Ensure we don't softlock the game because of keys blocks
                        if (CheckValidPlacement(paramID, randdrop))
                        {
                            dropList.Add(randdrop);
                            flatlist.RemoveAt(index); // don't place twice
                        }
                        else
                        {
                            // TODO full softlock reset tests.
                        }
                    }
                }
            }



            // Make into new lots:
            foreach (var kvp in VanillaLots)
            {
                ItemLot IL = new ItemLot();
                for (int row = 0; row < kvp.Value.NumDrops; row++)
                {
                    IL.AddDrop(flatlist[0]);
                    flatlist.RemoveAt(0); // pop
                }
                ShuffledLots[kvp.Key] = IL; // add to new dictionary
            }


            Hook.WriteAllLots(ShuffledLots);
            isRandomized = true;
        }
        private void unrandomize()
        {
            var timer = new Stopwatch();
            timer.Start();
            Hook.WriteAllLots(VanillaLots);
            isRandomized = false;

            timer.Stop();
            Console.WriteLine($"Execution time: {timer.Elapsed.TotalSeconds} ms");
        }

        
        private void btnRandomize_Click(object sender, RoutedEventArgs e)
        {
            // Want to try to Hook in DS2 to change the wooden chest above cardinal tower to metal chest items:
            if (!Hook.Hooked)
                return;

            if (isRandomized)
                unrandomize();
            else
                randomize();

            // Force an area reload. TODO add warning:
            Hook.WarpLast();

            // Update UI:
            btnRandomize.Content = isRandomized ? "Unrandomize!" : "Randomize!";
            Color bkg = isRandomized ? PURPLE : LIGHTGREEN;
            lblGameRandomization.Background = new SolidColorBrush(bkg);
            string gamestate = isRandomized ? "Randomized" : "Normal";
            lblGameRandomization.Content = $"Game is {gamestate}!";
        }
    }
}
