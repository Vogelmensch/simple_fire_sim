using System;

namespace HeatSimulation {

    public class HeatGrid2D {
        public HeatGrid2D(int size) {
            Size = size;
            GridSize = size * size;
            Grid = new double[GridSize];
        }

        // Grid of Size x Size
        private static int Size;
        private static int GridSize;
        // 1D-Array to represent 2D-Matrix. See Below for index conversion.
        public double[] Grid;

        // Set temperature of one Cell in Grid
        public void SetHeatGrid(int x, int y, double temp) {
            this.Grid[GetIndex(x,y,Size)] = temp;
        }

        public void RunAndPrint(int iterations, double thermalCoefficient, int sleepTime_ms) {
            for (int i = 0; i < iterations; i++) {
                Console.WriteLine("Iteration {0}", i);
                PrintGrid(Grid, Size, GridSize);
                Console.WriteLine();
                Grid = HeatStep(Grid, Size, GridSize, thermalCoefficient);

                // await user input
                //while (Console.ReadKey().Key == ConsoleKey.C) {}
                if (Console.ReadKey().Key == ConsoleKey.M) {
                    Console.WriteLine("ake fire: X,Y,T");
                    Console.Write("> ");
                    String? input = Console.ReadLine();
                    if (input != null) {
                        String[] words = input.Split(',');
                        try {
                            int x = int.Parse(words[0]);
                            int y = int.Parse(words[1]);
                            double temp = double.Parse(words[2]);
                            SetHeatGrid(x, y, temp);
                        } catch (Exception e) {
                            Console.WriteLine(e.Message);
                        }

                    }
                }

                //Thread.Sleep(sleepTime_ms);
            }
        }

        // Print grid with rounded numbers
        static private void PrintGrid(double[] grid, int size, int gridSize) {
            double sum = 0;
            for (int i = 0; i < gridSize; i++) {
                sum += grid[i];
                Console.Write(String.Format(" [{0:f2}] ", grid[i])); // rounding to 2 places after the comma
                if (i % size == size - 1) {
                    Console.Write(Environment.NewLine);
                }
            }
            Console.WriteLine(String.Format("Sum: {0:f2}", sum));
        }

        // Convert coordinates into array index
        static private int GetIndex(int x, int y, int size) {
            if (x < 0 || x >= size || y < 0 || y >= size) {
                throw new Exception("Coordinates out of bounds!");
            }
            return x + size * y;
        }

        // return new heat grid by using HOMOGENE heat equation
        // may be changed to INHOMOGEN later on
        private double[] HeatStep(double[] grid, int size, int gridSize, double thermalCoefficient) {
            double[] newGrid = new double[gridSize];

            // k 20 is the accuracy. This is probably not in Stein gemeißelt
            for (int k = 0; k < 20; k++) {
                for (int i = 0; i < gridSize; i++) {
                    double newTemp = 0;

                    List<int> neighbourIndices = NeighbourIndices(i, size, gridSize);

                    // add temperatures of neighbours of NEW grid (whaaat) (it makes sense, trust me)
                    foreach (int neighbourIndex in neighbourIndices) {
                        newTemp += newGrid[neighbourIndex];
                    }

                    // if cell is at the border, assume the border has the same temperature
                    newTemp += (4 - neighbourIndices.Count) * grid[i];

                    // coefficient a
                    newTemp *= thermalCoefficient;

                    // add old temperature
                    newTemp += grid[i];

                    // finally, we do this for some reason
                    newTemp /= 1 + 4*thermalCoefficient;

                    newGrid[i] = newTemp;
                }
            }

            return newGrid;

        }

        // Check whether the given index n is at the border.
        // If it's not, add the neighbour's index.
        private List<int> NeighbourIndices(int n, int size, int gridSize) {
            List<int> neighbourIndices = new List<int>();

            // top
            if (n >= size) {
                neighbourIndices.Add(n - size);
            }
            // bottom
            if (n < gridSize - size) {
                neighbourIndices.Add(n + size);
            }
            // left
            if (n % size != 0) {
                neighbourIndices.Add(n - 1);
            }
            // right
            if (n % size != size - 1) {
                neighbourIndices.Add(n + 1);
            }

            return neighbourIndices;
        }
    }

    class Heat_sim {
        static void Main(string[] args) {
            HeatGrid2D heatGrid = new HeatGrid2D(8);
            heatGrid.SetHeatGrid(1, 1, 30);

            int sleepTime = 500;

            heatGrid.RunAndPrint(100000, 0.01, sleepTime);
        }
    }

}