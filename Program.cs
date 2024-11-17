using System;

namespace HeatSimulation {

    public class Tile {

        public Tile(double temp, double integrity, double threshold, double t_coeff, double int_coeff) {
            Temp = temp;
            _integrity = integrity;
            Temp_threshold = threshold;
            Temp_change_coefficient = t_coeff;
            Integrity_change_coefficient = int_coeff;
            _is_burning = temp >= threshold;
        }
        public double Temp;


        private double _integrity;
        public double Integrity 
        {
            get {return _integrity;}
            set {_integrity = value;} 
        }

        // temperature at which the tile starts burning
        public double Temp_threshold {get;}

        // how much does the temperature rise per time interval when burning?
        // this implies a linear correlation -> research maybe
        public double Temp_change_coefficient {get;}

        // how much integrity does the tile lose per time interval when burning?
        public double Integrity_change_coefficient {get;}

        private bool _is_burning;

        // cannot change is_burning from outside
        public bool Is_burning {get {return _is_burning;}}

        // The following two functions are called every time step
        public void Ignite_or_extinguish() {
            if (Temp < Temp_threshold) {
                _is_burning = false;
            } else {
                _is_burning = true;
            }
        }

        // Take away integrity and increase temperature due to tile's own temperature
        // Call this before spreading
        public void May_burn() {
            if (_is_burning) {
                _integrity -= Integrity_change_coefficient; 
                Temp += Temp_change_coefficient;
            }
        }

        
        public bool Is_intact() {
            return Integrity > 0;
        }

    }

    public class HeatGrid2D {

        // initialize 2D-Heatgrid with homogeneous material
        public HeatGrid2D(int size, double integrity, double threshold, double t_coeff, double int_coeff) {
            Size = size;
            GridSize = size * size;
            Grid = new Tile[GridSize];
            for (int i = 0; i < GridSize; i++)
                Grid[i] = new Tile(0, integrity, threshold, t_coeff, int_coeff);
        }

        // Grid of Size x Size
        private static int Size;
        private static int GridSize;
        // 1D-Array to represent 2D-Matrix. See Below for index conversion.
        public Tile[] Grid;

        // Set temperature of one Cell in Grid
        public void SetHeatGrid(int x, int y, double temp) {
            Grid[GetIndex(x,y,Size)].Temp = temp;
        }

        public void RunAndPrint(int iterations, double thermalCoefficient, int sleepTime_ms) {
            for (int i = 0; i < iterations; i++) {
                Console.WriteLine("Iteration {0}", i);
                PrintGrid(Grid, Size, GridSize);
                Console.WriteLine();
                Grid = HeatStep(Grid, Size, GridSize, thermalCoefficient);

                // await user input
                if (Console.ReadKey().Key == ConsoleKey.M) {
                    Console.WriteLine("ake fire: X,Y,T");
                    Console.Write("> ");
                    string? input = Console.ReadLine();
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

                // Thread.Sleep(sleepTime_ms);
            }
        }

        // Print grid with rounded numbers
        // TODO: Add FIRE 🔥🔥🔥🔥🔥
        static private void PrintGrid(Tile[] grid, int size, int gridSize) {
            double sum = 0;
            for (int i = 0; i < gridSize; i++) {
                if (grid[i].Is_intact()) {
                    Console.Write(String.Format(" [{0:f2} {1} {2:f2}] ", grid[i].Integrity, grid[i].Is_burning ? "🔥" : "-", grid[i].Temp)); // rounding to 2 places after the comma
                    sum += grid[i].Temp;
                } else {
                    Console.Write("                   ");
                }
                if (i % size == size - 1) {
                    Console.Write(Environment.NewLine);
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
        private Tile[] HeatStep(Tile[] grid, int size, int gridSize, double thermalCoefficient) {
            Tile[] newGrid = new Tile[gridSize];

            // First step: Let tiles burn by themselves
            foreach (Tile tile in grid) {
                tile.Ignite_or_extinguish();
                tile.May_burn();
            }

            for (int i = 0; i < gridSize; i++) {
                newGrid[i] = new Tile(grid[i].Temp, grid[i].Integrity, grid[i].Temp_threshold, grid[i].Temp_change_coefficient, grid[i].Integrity_change_coefficient);
            }

            

            // // First step: Let tiles burn by themselves
            // for (int i = 0; i < gridSize; i++) {
            //     grid[i].Ignite_or_extinguish();
            //     grid[i].May_burn();
            // }
            
            // Second step: Spread the heat
            // k 20 is the accuracy. This is probably not in Stein gemeißelt
            for (int k = 0; k < 20; k++) {
                for (int i = 0; i < gridSize; i++) {
                    double newTemp = 0;

                    List<int> neighbourIndices = NeighbourIndices(i, size, gridSize);

                    // tiles beyond the border are considered broken
                    int broken_neighbours = 4 - neighbourIndices.Count;

                    // add temperatures of neighbours of NEW grid
                    // do this only if the neighbour is still intact
                    foreach (int neighbourIndex in neighbourIndices) {
                        if (newGrid[neighbourIndex].Is_intact()) {
                            newTemp += newGrid[neighbourIndex].Temp;
                        } else {
                            broken_neighbours += 1;
                        }
                    }

                    // if tile is broken or at the border, assume the tile has the same temperature
                    newTemp += broken_neighbours * grid[i].Temp;

                    // coefficient a
                    newTemp *= thermalCoefficient;

                    // add old temperature
                    newTemp += grid[i].Temp;

                    // finally, we do this for some reason
                    newTemp /= 1 + 4*thermalCoefficient;

                    newGrid[i].Temp = newTemp;
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
            Console.WriteLine("🔥🔥🔥🔥🔥🔥");
            HeatGrid2D heatGrid = new HeatGrid2D(8, 100, 60, 5, 2);
            heatGrid.SetHeatGrid(1, 1, 500);

            int sleepTime = 500;

            heatGrid.RunAndPrint(100000, 0.03, sleepTime);
        }
    }

}