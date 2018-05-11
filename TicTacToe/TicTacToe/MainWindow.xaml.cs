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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TicTacToe
{
    enum State { Cross, Circle, Empty };

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const State AI = State.Circle;

        private State[,] States { get; set; }        
        public Button[,] Buttons { get; set; }

        private static Random rand;

        private State m_NextMove;
        private State NextMove
        {
            get
            {
                return m_NextMove;
            }
            set
            {
                if (value != m_NextMove)
                {
                    m_NextMove = value;
                    if (m_NextMove == AI && GetWinner(States) == State.Empty)
                    {
                        DoAIMove();
                    }
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            Initialize();
        }

        public void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.R)
            {
                Reset();                
            }
        }

        private void Initialize()
        {
            rand = new Random();

            Buttons = new Button[MyGrid.Rows, MyGrid.Columns];
            States = new State[MyGrid.Rows, MyGrid.Columns];

            for (int i = 0; i < MyGrid.Rows; i++)
            {
                for (int j = 0; j < MyGrid.Columns; j++)
                {
                    Button button = new Button
                    {
                        Background = Brushes.White,
                        Height = Double.NaN,
                        Width = Double.NaN,
                        FontSize = 80,
                        FontWeight = FontWeights.Bold,
                        BorderThickness = new Thickness(0.5),
                        BorderBrush = Brushes.Black,
                        Content = "",
                        Name = "B" + (i * MyGrid.Columns + j).ToString()
                    };

                    button.Click += Button_Click;

                    Buttons[i, j] = button;
                    MyGrid.Children.Add(button);

                    States[i, j] = State.Empty;
                }
            }

            //NextMove = State.Circle; // Cross begins
            NextMove = (rand.Next(0, 2) == 0) ? State.Circle : State.Cross;
        }

        private void Reset()
        {
            foreach (var button in Buttons)
            {
                button.Content = "";
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    States[i,j] = State.Empty;
                }
            }

            NextMove = (rand.Next(0, 2) == 0) ? State.Circle : State.Cross;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int buttonNo = Convert.ToInt32((sender as Button).Name.Replace("B", ""));

            int row = buttonNo / 3;
            int col = buttonNo % 3;

            if (States[row, col] == State.Empty && GetWinner(States) == State.Empty)
            {
                SetButtonState(row, col, NextMove);
            }
        }

        private State GetWinner(State[,] field)
        {
            for (int i = 0; i < 3; i++)
            {
                // Check horizontal lines
                if ((field[i, 0] == field[i, 1] && field[i, 0] == field[i, 2]) && field[i, 0] != State.Empty)
                {
                    return field[i, 0];
                }

                // Check vertical lines
                if ((field[0, i] == field[1, i] && field[0, i] == field[2, i]) && field[0, i] != State.Empty)
                {
                    return field[i, 0];
                }
            }

            // Check diagonal lines
            if ((field[0, 0] == field[1, 1] && field[0, 0] == field[2, 2]) && field[0, 0] != State.Empty)
            {
                return field[0, 0];
            }
            if ((field[2, 0] == field[1, 1] && field[2, 0] == field[0, 2]) && field[2, 0] != State.Empty)
            {
                return field[2, 0];
            }

            return State.Empty; // No winner
        }

        private void SetButtonState(int row, int col, State state)
        {
            if (state == State.Circle)
            {
                Buttons[row, col].Content = "\u25CB";
                Buttons[row, col].Foreground = Brushes.Red;
            }
            else
            {
                Buttons[row, col].Content = "\uD83D\uDFA8";
                Buttons[row, col].Foreground = Brushes.Blue;
            }

            States[row, col] = state;
            NextMove = GetInvState(state);

        }

        private void DoAIMove()
        {
            int row, col;
            //AI_Random_Strategy(out row, out col);
            AI_Minimax_Search_Strategy(out row, out col);

            if (row != -1 && col != -1)
            {
                SetButtonState(row, col, AI);
            }
        }

        #region Random Strategy
        private void AI_Random_Strategy(out int row, out int col)
        {
            List<int> emptyCells = new List<int>();

            for (int i = 0; i < MyGrid.Rows; i++)
            {
                for (int j = 0; j < MyGrid.Columns; j++)
                {
                    if (States[i, j] == State.Empty)
                    {
                        emptyCells.Add(i * MyGrid.Columns + j);
                    }
                }
            }

            GetRandomCell(emptyCells, out row, out col);
        }
        #endregion Random Strategy

        #region Explicit Strategy
        private void AI_Explicit_Strategy(out int row, out int col)
        {
            List<int> cells = new List<int>(); // possible cells for this move

            // First Move
            if (IsFirstMove())
            {
                cells.Add(0);
                cells.Add(2);
                cells.Add(6);
                cells.Add(8);
            }

            GetRandomCell(cells, out row, out col);
        }
        #endregion Explicit Strategy

        #region Minimax Stategy
        private void AI_Minimax_Search_Strategy(out int row, out int col)
        {
            Minimax(States, NextMove, out row, out col);
        }

        private int Minimax(State[,] curStates, State curPlayer, out int row, out int col)
        {
            if (GetNumberOfMoves(curStates) == 9 || GetWinner(curStates) != State.Empty)
            {
                row = -1;
                col = -1;
                return Minimax_Reward_Function(curStates);
            }
            else
            {
                List<int> possibleCells = new List<int>();
                for (int i = 0; i < MyGrid.Rows; i++)
                {
                    for (int j = 0; j < MyGrid.Columns; j++)
                    {
                        if (curStates[i, j] == State.Empty)
                            possibleCells.Add(i * MyGrid.Rows + j);
                    }
                }

                if (curPlayer == AI) // Maximize
                {
                    int rMax = int.MinValue;
                    List<int> cells = new List<int>();
                    foreach (var cell in possibleCells)
                    {
                        int curRow = cell / 3;
                        int curCol = cell % 3;

                        State[,] newStates = CopyStates(curStates);
                        newStates[curRow, curCol] = curPlayer;

                        int bestRow, bestCol;
                        int r = Minimax(newStates, GetInvState(curPlayer), out bestRow, out bestCol);

                        if (r > rMax)
                        {
                            rMax = r;
                            cells.Clear();
                            cells.Add(curRow * 3 + curCol);
                        }
                        else if (r == rMax)
                        {
                            cells.Add(curRow * 3 + curCol);
                        }
                    }

                    GetRandomCell(cells, out row, out col);
                    return rMax;
                }
                else // Minimize
                {
                    int rMin = int.MaxValue;
                    List<int> cells = new List<int>();
                    foreach (var cell in possibleCells)
                    {
                        int curRow = cell / 3;
                        int curCol = cell % 3;

                        State[,] newStates = CopyStates(curStates);
                        newStates[curRow, curCol] = curPlayer;

                        int bestRow, bestCol;
                        int r = Minimax(newStates, GetInvState(curPlayer), out bestRow, out bestCol);

                        if (r < rMin)
                        {
                            rMin = r;
                            cells.Clear();
                            cells.Add(curRow * 3 + curCol);
                        }
                        else if (r == rMin)
                        {
                            cells.Add(curRow * 3 + curCol);
                        }
                    }

                    GetRandomCell(cells, out row, out col);
                    return rMin;
                }
            }
        }

        private int Minimax_Reward_Function(State[,] curStates)
        {
            int r = 0;

            for (int i = 0; i < 3; i++)
            {
                r += Minimax_Reward_Function_Helper(curStates[0, i], curStates[1, i], curStates[2, i]);

                r += Minimax_Reward_Function_Helper(curStates[i, 0], curStates[i, 1], curStates[i, 2]);
            }

            r += Minimax_Reward_Function_Helper(curStates[0, 0], curStates[1, 1], curStates[2, 2]);

            r += Minimax_Reward_Function_Helper(curStates[0, 2], curStates[1, 1], curStates[2, 0]);

            return r;
        }

        private int Minimax_Reward_Function_Helper(State s1, State s2, State s3)
        {
            int ai_score = ((s1 == AI) ? 1 : 0) +
                           ((s2 == AI) ? 1 : 0) +
                           ((s3 == AI) ? 1 : 0);

            int op_score = ((s1 == GetInvState(AI)) ? 1 : 0) +
                           ((s2 == GetInvState(AI)) ? 1 : 0) +
                           ((s3 == GetInvState(AI)) ? 1 : 0);

            if (ai_score == 3)
            {
                return 100;
            }
            /*else if (ai_score == 2)
            {
                return 10;
            }*/
            else if (op_score == 3)
            {
                return -100;
            }
            /*else if (op_score == 2)
            {
                return -10;
            }*/
            else
            {
                return 0;
            }
        }
        #endregion Minimax Stategy

        #region HelperFunctions
        private State GetInvState(State state)
        {
            if (state == State.Empty)
                return State.Empty;
            else
                return (state == State.Circle) ? State.Cross : State.Circle;
        }

        private bool IsFirstMove()
        {
            foreach (var cell in States)
            {
                if (cell != State.Empty)
                {
                    return false;
                }
            }

            return true;
        }

        private int GetNumberOfMoves(State[,] curStates)
        {
            int ct = 0;

            foreach (var cell in curStates)
            {
                if (cell != State.Empty)
                {
                    ct++;
                }
            }

            return ct;
        }

        private void GetRandomCell(List<int> cells, out int row, out int col)
        {
            if (cells.Count == 0)
            {
                row = -1;
                col = -1;
            }
            else
            {
                int randPos = rand.Next(0, cells.Count);
                row = cells[randPos] / 3;
                col = cells[randPos] % 3;
            }
        }

        private State[,] CopyStates(State[,] states)
        {
            State[,] newStates = new State[MyGrid.Rows, MyGrid.Columns];

            for (int i = 0; i < MyGrid.Rows; i++)
            {
                for (int j = 0; j < MyGrid.Columns; j++)
                {
                    newStates[i, j] = states[i, j];
                }
            }

            return newStates;
        }
        #endregion HelperFunctions
    }
}
