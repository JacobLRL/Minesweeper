using System;
using System.Collections.Generic;
using System.Drawing.Text;
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
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static int[,] board1 = new int[10, 10];
        private static int[,] board2 = new int[10, 10];
        private static int[,] numBoard1 = new int[10,10];
        private static int[,] numBoard2 = new int[10,10];
        private static List<string> clicked = new List<string>();
        private static Button[,] buttonArray = new Button[10, 10];
        private static List<string> flagged = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            AddButtonsToBoard();
            CreateBottomLayer();
            CreateFinalBoard();
        }

        public void AddButtonsToBoard() {
            for (int i = 0; i < 10; i++) {
                for (int j = 0; j < 10; j++)
                {
                    Button b = new Button();
                    b.FontWeight = FontWeights.Bold;
                    var converter = new BrushConverter();
                    var brush = (Brush)converter.ConvertFromString("#83D744");
                    b.BorderThickness = new Thickness(1, 1, 3, 3);
                    b.Name = $"cell{i}{j}";
                    b.FontSize = 30;
                    b.Click += new RoutedEventHandler(Button_Click);
                    b.MouseRightButtonUp += new MouseButtonEventHandler(Right_Click);
                    Grid.SetRow(b, i);
                    Grid.SetColumn(b, j);
                    buttonArray[i, j] = b;
                    
                    grid.Children.Add(b);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = (sender as Button);
            int num = numBoard1[Grid.GetRow(b), Grid.GetColumn(b)];            
            if (num != 0 && num != -1)
            {
                b.Content = num;
            }
            else if (num == -1) {
                // lose
                LoseCond();
                var brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri("https://is3-ssl.mzstatic.com/image/thumb/Purple128/v4/4a/af/28/4aaf2872-b449-63f8-b8a7-a135b168adf2/AppIcon-1x_U007emarketing-85-220-8.png/246x0w.jpg"));
                b.Background = brush;
                MessageBox.Show("You Lose");
            } else if (num == 0) {
                int num12 = Grid.GetRow(b);
                Clear(Grid.GetRow(b), Grid.GetColumn(b), false);
            }
            if (!clicked.Contains(b.Name)) clicked.Add(b.Name);
            b.Click -= Button_Click;
            b.MouseRightButtonUp -= Right_Click;
            b.MouseDoubleClick += Double_Click;
            WinCond();
            
            b.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
            var converter = new BrushConverter();
            try
            {
                b.Foreground = (Brush)converter.ConvertFromString(NumColor(num)); 
            }
            catch { }
        }
        private void Right_Click(object sender, MouseEventArgs e) {
            Button b = (sender as Button);
            var brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri("https://jacoblrl.github.io/sparta-javascript-game/img/flag.png"));
            b.Background = brush;
            b.Click -= Button_Click;
            if (!flagged.Contains(b.Name)) flagged.Add(b.Name);
        }

        private void Double_Click(object sender, MouseEventArgs e) {
            Button b = (sender as Button);
            Clear(Grid.GetRow(b), Grid.GetColumn(b), true);
        }

        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();
        public static int RandomNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return random.Next(min, max);
            }
        }
        // makes sure no repeating bombs and creates backup board for not losing on first click \\\\not implemented
        public void CreateBottomLayer()
        {
            int count1 = 10;
            int count2 = 10;
            for (int i = 0; i < board1.GetLength(0); i++)
            {
                for (int j = 0; j < board1.GetLength(1); j++)
                {
                    board1[i, j] = 0;
                    board2[i, j] = 0;
                }
            }
            while (count1 > 0) {
                int row = RandomNumber(0, 10);
                int col = RandomNumber(0, 10);
                if (board1[row, col] != 1) {
                    board1[row, col] = 1;
                    count1--;
                }
            }
            while (count2 > 0) {
                int row = RandomNumber(0, 10);
                int col = RandomNumber(0, 10);
                if (board1[row, col] != 1 && board2[row, col] != 1)
                {
                    board2[row, col] = 1;
                    count2--;
                }
            }
        }

        public void CreateFinalBoard() {
            
            for (int i = 0; i < board1.GetLength(0); i++)
            {
                for (int j = 0; j < board1.GetLength(1); j++) {
                    if (board1[i, j] == 1)
                    {
                        numBoard1[i, j] = -1;
                    }
                    else
                    {
                        numBoard1[i, j] = CountSurrounding(i, j, board1);
                    }
                    if (board2[i, j] == 1)
                    {
                        numBoard2[i, j] = -1;
                    }
                    else
                    {
                        numBoard2[i, j] = CountSurrounding(i, j, board2);
                    }
                }
            }
        }

        public int CountSurrounding(int row, int col, int[,] board) {
            int count = 0;
            List<int[]> surroundings = GetSurrounding(row, col);
            foreach (var elem in surroundings) {
                count += board[elem[0],elem[1]];
            }
            return count;
        }

        public List<int[]> GetSurrounding(int row, int col)
        {
            int[][] inCells = {
            new int[]{ row - 1, col - 1 },
            new int[]{ row - 1, col },
            new int[]{ row - 1, col + 1 },
            new int[]{ row, col - 1 },
            new int[]{ row, col + 1 },
            new int[]{ row + 1, col - 1 },
            new int[]{ row + 1, col },
            new int[]{ row + 1, col + 1 }
            };
            List<int[]> outCells = new List<int[]>();
            foreach (var cells in inCells)
            {
                if (cells[0] >= 0 && cells[0] < 10 && cells[1] >= 0 && cells[1] < 10) //&& !clicked.Contains($"cell{cells[0]}{cells[1]}"))
                {
                    outCells.Add(cells);
                }
            }
            return outCells;
        }

        public string NumColor(int num)
        {
            switch (num)
            {
                case 1:
                    return "blue";
                case 2:
                    return "green";
                case 3:
                    return "red";
                case 4:
                    return "darkblue";
                case 5:
                    return "maroon";
                case 6:
                    return "#006768";
                case 7:
                    return "purple";
                case 8:
                    return "black";
                default:
                    return "";
            }
        }

        public void LoseCond()
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (board1[i, j] > 0) {
                        var brush = new ImageBrush();
                        brush.ImageSource = new BitmapImage(new Uri("https://esraa-alaarag.github.io/Minesweeper/images/bomb.png"));
                        buttonArray[i,j].Background = brush;
                        foreach (var elem in buttonArray)
                        {
                            elem.Click -= Button_Click;
                            elem.MouseRightButtonUp -= Right_Click;
                            elem.MouseDoubleClick -= Double_Click;
                        }
                    }
                }
            }
            
        }

        public void WinCond() {
            int count = clicked.Count;
            if (clicked.Count == 90) {
                foreach (var elem in buttonArray)
                {
                    elem.Click -= Button_Click;
                    elem.MouseRightButtonUp -= Right_Click;
                    elem.MouseDoubleClick -= Double_Click;
                }
                MessageBox.Show("Winner");
            }
        }

        public void Clear(int row, int col, bool gate) {
            int num = numBoard1[row, col];
            if (row < 0 || row >= 10 || col < 0 || col >= 10) return;
            if (clicked.Contains($"cell{row}{col}") && !gate) return;
            if (flagged.Contains(($"cell{row}{col}"))) return;

            if (numBoard1[row,col] == -1) {
                MessageBox.Show("You Lose");
                return;
            }
            if (numBoard1[row, col] > 0 && !gate) {
                buttonArray[row, col].Content = num;
                buttonArray[row, col].BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
                var converter = new BrushConverter();
                buttonArray[row, col].Foreground = (Brush)converter.ConvertFromString(NumColor(num));
                if(!clicked.Contains($"cell{row}{col}")) clicked.Add($"cell{row}{col}");
                buttonArray[row, col].Click -= Button_Click;
                buttonArray[row, col].MouseRightButtonUp -= Right_Click;
                buttonArray[row, col].MouseDoubleClick += Double_Click;
                WinCond();
                return;
            }
            if (!clicked.Contains($"cell{row}{col}")) clicked.Add($"cell{row}{col}");
            buttonArray[row, col].Click -= Button_Click;
            buttonArray[row, col].MouseRightButtonUp -= Right_Click;
            buttonArray[row, col].MouseDoubleClick += Double_Click;
            WinCond();
            buttonArray[row, col].BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
            var surrounding = GetSurrounding(row, col);
            foreach (var cells in surrounding) {
                Clear(cells[0], cells[1], false);
            }
        }
    }
}
