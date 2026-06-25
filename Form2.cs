using System;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Windows.Forms;

namespace Battleship_Game
{
    public partial class Form2 : Form
    {
        //initialization and variable declaration
        public Form2()
        {
            InitializeComponent();
            this.KeyPreview = true;
        }
        Form1 FirstForm = new Form1();
        Random rnd = new Random();

        //arrays for board management
        string[,] HumanBoardArr = new string[10, 10];
        string[,] OpponentBoardArr = new string[10, 10];

        //string for ship placement validation
        string ShipCheck = "";

        //ship types, colours and square counts for placement and hit management
        string[] ShipNameArr = new string[5] { "Gunboat", "Destroyer", "Cruiser", "Battleship", "Aircraft Carrier" };
        string[] Shipcolours = new string[5] { "white", "floralwhite", "beige", "AntiqueWhite", "Tan" };
        int[] HumanShipSquaresLeft = new int[5] { 2, 3, 4, 5, 6 };
        int[] OppShipSquaresLeft = new int[5] { 2, 3, 4, 5, 6 };

        //variables for player and opponent ship count management
        int PlayerShipsOnBoard = 0;
        int OppShipsOnBoard = 0;

        //variables for computer opponent attack management
        int[] OppAttackCoords = new int[2]; //coords of attack
        string OppAllSuroundingCells = ""; //all surrounding cells of initial hit
        int[] OppLastSuccessfulHit = new int[2] { -1, -1 }; //last successful hit coords
        string OppAttackOrientation = ""; //vertical or horizontal ship
        int[] OppInitialHit = new int[2] { -1, -1 }; //initial Hit Coords
        bool OppDirectionReversed = false; //direction reversed
        int OppAttackDirection = 1; //direction of attack
        int OppReversedCounter = 1; //how many cells away from initial

        //turn management variables
        bool ShipPlacementModeActive = false;
        bool GameplayModeActive = false;
        bool IsPlayerTurn = false;
        
        //initial board setup
        private void Form2_Load(object sender, EventArgs e)
        {
            HumanBoard.RowCount = 10;
            OpponentBoard.RowCount = 10;
            HumanBoard.DefaultCellStyle.BackColor = Color.LightSkyBlue;
            OpponentBoard.DefaultCellStyle.BackColor = Color.LightSkyBlue;
        }

        //initial instructions and ship placement activation
        private void Form2_Shown(object sender, EventArgs e)
        {
            OpponentBoard.ClearSelection();
            MessageBox.Show("Please place your ships on the left board");
            ShipPlacementModeActive = true;
        }

        //opens menu when game closes
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            FirstForm.Show();
        }

        //manages spacebar input for ship placement and gameplay
        private void Form2_Keydown(object sender, KeyEventArgs e)
        {
            //if Space is pressed
            if (e.KeyCode == Keys.Space)
            {
                //if ship placement mode is active then run ship placement function
                if (ShipPlacementModeActive == true)
                {
                    ShipPlacement();
                    HumanBoard.ClearSelection();
                }
                //if gameplay mode is active and it is the players turn then run hit calculation function
                else if (GameplayModeActive == true && IsPlayerTurn == true)
                {
                    HitCalculation();
                }
            }
        }

        //manages transition between different stages of the game
        private void ProgramManager(string input)
        {
            if (Convert.ToString(input) == "ComputerSelection")
            {
                ShipPlacementModeActive = false;
                ComputerOpponent("placement");
            }
            else if (Convert.ToString(input) == "GamePlay")
            {
                GameplayModeActive = true;
                GameplayRotation();
            }
        }

        //turn management
        private void GameplayRotation()
        {
            //if player turn then disable controls
            if (IsPlayerTurn == true)
            {
                OpponentBoard.Enabled = false;
                OpponentBoard.ClearSelection();
                IsPlayerTurn = false;
                HitCalculation();

            }
            //opponent turn
            else
            {
                OpponentBoard.Enabled = true;
                IsPlayerTurn = true;
            }
        }

        //player ship placement and validation
        private void ShipPlacement()
        {
            //Variables
            int Count = 0;
            bool ValidPlacement = true;
            int[] FirstCellPlace = new int[2];
            int[] SecondCellPlace = new int[2];
            string Type = "";
            //Error prevention
            if (HumanBoard.SelectedCells.Count < 2)
            {
                return;
            }
            //Orientation Checker
            DataGridViewCell firstcell = HumanBoard.SelectedCells[0];
            FirstCellPlace[0] = firstcell.RowIndex; FirstCellPlace[1] = firstcell.ColumnIndex;
            DataGridViewCell secondcell = HumanBoard.SelectedCells[1];
            SecondCellPlace[0] = secondcell.RowIndex; SecondCellPlace[1] = secondcell.ColumnIndex;
            if (FirstCellPlace[0] == SecondCellPlace[0])
            {
                Type = "horizontal";
            }
            else if (FirstCellPlace[1] == SecondCellPlace[1])
            {
                Type = "vertical";
            }

            //validation
            foreach (DataGridViewCell cell in HumanBoard.SelectedCells)
            {
                //Overlap prevention
                Count += 1;
                if (HumanBoardArr[cell.RowIndex, cell.ColumnIndex] != null)
                {
                    ValidPlacement = false;
                    break;
                }
                //Orientation validation
                if (Type == "horizontal")
                {
                    if (cell.RowIndex != FirstCellPlace[0])
                    {
                        ValidPlacement = false;
                        break;
                    }
                }
                else if (Type == "vertical")
                {
                    if (cell.ColumnIndex != FirstCellPlace[1])
                    {
                        ValidPlacement = false;
                        break;
                    }
                }
            }
            if (ValidPlacement == true && 2 <= Count && Count <= 6 && !ShipCheck.Contains(Convert.ToString(Count)))
            {
                //Output
                foreach (DataGridViewCell cell in HumanBoard.SelectedCells)
                {
                    HumanBoardArr[cell.RowIndex, cell.ColumnIndex] = ShipNameArr[Count - 2];
                    cell.Style.BackColor = Color.FromName(Shipcolours[Count -2]);              
                }
                ShipCheck += Convert.ToString(Count);
                PlayerShipsOnBoard += 1;
            }
            //Turnover
            if (PlayerShipsOnBoard == 5)
            {
                HumanBoard.Enabled = false;
                HumanBoard.ClearSelection();
                ProgramManager("ComputerSelection");
            }
        }

        //resets all variables used for computer opponent gameplay
        private void CPUGameplayVariableReset()
        {
            OppAllSuroundingCells = "";
            OppLastSuccessfulHit[0] = -1;
            OppLastSuccessfulHit[1] = -1;
            OppAttackOrientation = "";
            OppInitialHit[0] = -1;
            OppInitialHit[1] = -1;
            OppDirectionReversed = false;
            OppAttackDirection = 1;
            OppReversedCounter = 1;
        }

        //Computer opponent placement and gameplay
        private void ComputerOpponent(string mode)
        {
            if (Convert.ToString(mode) == "placement")
            {
                //variables
                int OppShipsLeft = 5;
                int[] OppPlacement = new int[2];
                bool validplacement;
                while (OppShipsLeft > 0)
                {
                    validplacement = true;
                    OppPlacement[0] = rnd.Next(0, 9); OppPlacement[1] = rnd.Next(0, 9);
                    //orientation randomiser
                    if (rnd.Next(0, 2) == 1)
                    {
                        //valication (vertical)
                        if (OppPlacement[0] + OppShipsLeft > 9)
                        { 
                            //if ship would go out of bounds
                            validplacement = false;
                            continue;
                        }
                        //checks if all selected cells are empty
                        for (int i = 0; i < OppShipsLeft + 1; i += 1)
                        {
                            if (OpponentBoardArr[OppPlacement[0] + i, OppPlacement[1]] != null)
                            {
                                validplacement = false;
                                break;
                            }
                        }
                        if (validplacement == true)
                        {
                            //output
                            for (int i = 0; i < OppShipsLeft + 1; i += 1)
                            {
                                OpponentBoardArr[OppPlacement[0] + i, OppPlacement[1]] = ShipNameArr[OppShipsLeft - 1];
                            }
                            OppShipsOnBoard += 1;
                            OppShipsLeft -= 1;
                        }
                    }
                    else
                    {
                        //validation (horizontal)
                        //if ship would go out of bounds
                        if (OppPlacement[1] + OppShipsLeft > 9)
                        {
                            validplacement = false;
                            continue;
                        }
                        //checks if all selected cells are empty
                        for (int i = 0; i < OppShipsLeft + 1; i += 1)
                        {
                            if (OpponentBoardArr[OppPlacement[0], OppPlacement[1] + i] != null)
                            {
                                validplacement = false;
                                break;
                            }
                        }
                        if (validplacement == true)
                        {
                            //output
                            for (int i = 0; i < OppShipsLeft + 1; i += 1)
                            {
                                OpponentBoardArr[OppPlacement[0], OppPlacement[1] + i] = ShipNameArr[OppShipsLeft - 1];
                            }
                            OppShipsOnBoard += 1;
                            OppShipsLeft -= 1;
                        }
                    }
                }
                //initiates gameplay after all ships have been placed
                ProgramManager("GamePlay");
            }

            else if (Convert.ToString(mode) == "gameplay")
            {
                //checks if there has been a successful hit
                if (OppLastSuccessfulHit[0] != -1)
                {
                    //checks whether ship has been destroyed
                    if (OppInitialHit[0] != -1 && HumanBoardArr[OppInitialHit[0], OppInitialHit[1]] == "destroyed")
                    {
                        //resets variables if ship has been destroyed
                        OppAttackCoords[0] = rnd.Next(0, 10);
                        OppAttackCoords[1] = rnd.Next(0, 10);
                        CPUGameplayVariableReset();
                        return;
                    }
                    //if targeted ship is vertical
                    else if (OppAttackOrientation == "vertical")
                    {
                        //vertically forward logic
                        if (OppDirectionReversed == false)
                        {
                            //out of bounds check
                            if (OppLastSuccessfulHit[0] + OppAttackDirection > 9)
                            {
                                OppDirectionReversed = true;
                                OppReversedCounter = 1;
                                ComputerOpponent("gameplay");
                                return;
                            }
                            OppAttackCoords[0] = OppLastSuccessfulHit[0] + OppAttackDirection;
                            OppAttackCoords[1] = OppLastSuccessfulHit[1];
                        }
                        else
                        //vertically backward logic
                        {
                            //out of bounds check
                            if (OppInitialHit[0] - (OppAttackDirection * OppReversedCounter) < 0)
                            {
                                CPUGameplayVariableReset();
                                return;
                            }
                            OppAttackCoords[0] = OppInitialHit[0] - (OppAttackDirection * OppReversedCounter);
                            OppAttackCoords[1] = OppInitialHit[1];
                            OppReversedCounter += 1;
                        }
                        return;
                    }
                    //if trageted ship is horizontal
                    else if (OppAttackOrientation == "horizontal")
                    {
                        //horizontally forward logic
                        if (OppDirectionReversed == false)
                        {
                            //out of bounds check
                            if (OppLastSuccessfulHit[1] + OppAttackDirection > 9)
                            {
                                OppDirectionReversed = true;
                                OppReversedCounter = 1;
                                ComputerOpponent("gameplay");
                                return;
                            }
                            OppAttackCoords[0] = OppLastSuccessfulHit[0];
                            OppAttackCoords[1] = OppLastSuccessfulHit[1] + OppAttackDirection;
                        }
                        else
                        //horizontally backward logic
                        {
                            //out of bounds check
                            if (OppInitialHit[1] - OppReversedCounter < 0)
                            {
                                CPUGameplayVariableReset();
                                return;
                            }
                            OppAttackCoords[0] = OppInitialHit[0];
                            OppAttackCoords[1] = OppInitialHit[1] - (OppAttackDirection * OppReversedCounter); ;
                            OppReversedCounter += 1;
                        }
                        return;
                    }
                    else if (!OppAllSuroundingCells.Contains("up"))
                    {
                        //out of bounds check
                        OppAttackDirection = -1;
                        OppReversedCounter *= -1;
                        if (OppLastSuccessfulHit[0] - 1 < 0)
                        {
                            OppAllSuroundingCells += "up";
                            ComputerOpponent("gameplay");
                            return;
                        }
                        //attack coordinates set to cell above last successful hit
                        OppAttackCoords[0] = OppLastSuccessfulHit[0] - 1;
                        OppAttackCoords[1] = OppLastSuccessfulHit[1];
                        OppAllSuroundingCells += "up";
                    }
                    else if (!OppAllSuroundingCells.Contains("down"))
                    {
                        //out of bounds check
                        OppAttackDirection = 1;
                        OppReversedCounter *= -1;
                        if (OppLastSuccessfulHit[0] + 1 > 9)
                        {
                            OppAllSuroundingCells += "down";
                            ComputerOpponent("gameplay");
                            return;
                        }
                        //attack coordinates set to cell below last successful hit
                        OppAttackCoords[0] = OppLastSuccessfulHit[0] + 1;
                        OppAttackCoords[1] = OppLastSuccessfulHit[1];
                        OppAllSuroundingCells += "down";
                    }
                    else if (!OppAllSuroundingCells.Contains("left"))
                    {
                        //out of bounds check
                        OppAttackDirection = -1;
                        OppReversedCounter *= -1;
                        if (OppLastSuccessfulHit[1] - 1 < 0)
                        {
                            OppAllSuroundingCells += "left";
                            ComputerOpponent("gameplay");
                            return;
                        }
                        //attack coordinates set to cell left of last successful hit
                        OppAttackCoords[0] = OppLastSuccessfulHit[0];
                        OppAttackCoords[1] = OppLastSuccessfulHit[1] - 1;
                        OppAllSuroundingCells += "left";
                    }
                    else if (!OppAllSuroundingCells.Contains("right"))
                    {
                        //out of bounds check
                        OppAttackDirection = 1;
                        OppReversedCounter *= -1;
                        if (OppLastSuccessfulHit[1] + 1 > 9)
                        {
                            OppAllSuroundingCells += "right";
                            ComputerOpponent("gameplay");
                            return;
                        }
                        //attack coordinates set to cell right of last successful hit
                        OppAttackCoords[0] = OppLastSuccessfulHit[0];
                        OppAttackCoords[1] = OppLastSuccessfulHit[1] + 1;
                        OppAllSuroundingCells += "right";
                    }
                    //if all surrounding cells have been selected, reset last successful hit and surrounding cells string
                    else
                    {
                        CPUGameplayVariableReset();
                        ComputerOpponent("gameplay");
                        return;
                    }
                }
                //random coordinates selected
                else
                {
                    OppAttackCoords[0] = rnd.Next(0, 10);
                    OppAttackCoords[1] = rnd.Next(0, 10);
                }
            }
        }

        //gameplay logic for hit calculation and ship destruction
        private void HitCalculation()
        {
            //variables
            string shiphit;
            bool shipdestroyed = false;
            int[] selectedcell = new int[2];
            //player turn validation
            if (IsPlayerTurn == true)
            {
                //gets selected cell coordinates
                foreach (DataGridViewCell cell in OpponentBoard.SelectedCells)
                {
                    selectedcell[0] = cell.RowIndex;
                    selectedcell[1] = cell.ColumnIndex;
                }
                OpponentBoard.ClearSelection();

                //null check for already selected cell
                if (OpponentBoardArr[selectedcell[0], selectedcell[1]] != null)
                {
                    shiphit = Convert.ToString(OpponentBoardArr[selectedcell[0], selectedcell[1]]);
                    if (shiphit == "Gunboat")
                    {
                        //Gunboat hit logic
                        OppShipSquaresLeft[0] -= 1;
                        OpponentBoardArr[selectedcell[0], selectedcell[1]] = "hit" + "Gunboat";
                        OpponentBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                        //ship destroyed condition
                        if (OppShipSquaresLeft[0] == 0)
                        {
                            shipdestroyed = true;
                            OppShipsOnBoard -= 1;
                        }
                        MessageBox.Show("hit");
                    }
                    else if (shiphit == "Destroyer")
                    {
                        //Destroyer hit logic
                        OppShipSquaresLeft[1] -= 1;
                        OpponentBoardArr[selectedcell[0], selectedcell[1]] = "hit" + "Destroyer";
                        OpponentBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                        //ship destroyed condition
                        if (OppShipSquaresLeft[1] == 0)
                        {
                            shipdestroyed = true;
                            OppShipsOnBoard -= 1;
                        }
                        MessageBox.Show("hit");
                    }
                    else if (shiphit == "Cruiser")
                    {
                        //Cruiser hit logic
                        OppShipSquaresLeft[2] -= 1;
                        OpponentBoardArr[selectedcell[0], selectedcell[1]] = "hit" + shiphit;
                        OpponentBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                        //ship destroyed condition
                        if (OppShipSquaresLeft[2] == 0)
                        {
                            shipdestroyed = true;
                            OppShipsOnBoard -= 1;
                        }
                        MessageBox.Show("hit");
                    }
                    else if (shiphit == "Battleship")
                    {
                        //Battleship hit logic
                        OppShipSquaresLeft[3] -= 1;
                        OpponentBoardArr[selectedcell[0], selectedcell[1]] = "hit" + shiphit;
                        OpponentBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                        //ship destroyed condition
                        if (OppShipSquaresLeft[3] == 0)
                        {
                            shipdestroyed = true;
                            OppShipsOnBoard -= 1;
                        }
                        MessageBox.Show("hit");
                    }
                    else if (shiphit == "Aircraft Carrier")
                    {
                        //Aircraft Carrier hit logic
                        OppShipSquaresLeft[4] -= 1;
                        OpponentBoardArr[selectedcell[0], selectedcell[1]] = "hit" + shiphit;
                        OpponentBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                        //ship destroyed condition
                        if (OppShipSquaresLeft[4] == 0)
                        {
                            shipdestroyed = true;
                            OppShipsOnBoard -= 1;
                        }
                        MessageBox.Show("hit");
                    }

                    //already selected condition
                    else
                    {
                        MessageBox.Show("Gridsquare already selected");
                        return;
                    }

                    //ship destroyed condition
                    if (shipdestroyed == true)
                    {
                        for (int row = 0; row < 10; row += 1)
                        {
                            for (int col = 0; col < 10; col += 1)
                            {
                                if (OpponentBoardArr[row, col] == "hit" + shiphit)
                                {
                                    OpponentBoardArr[row, col] = "destroyed";
                                    OpponentBoard[col, row].Style.BackColor = Color.DarkRed;
                                }
                            }
                        }
                        MessageBox.Show("Ship Destroyed");
                    }
                }
                //miss condition
                else
                {
                    OpponentBoardArr[selectedcell[0], selectedcell[1]] = "Miss";
                    OpponentBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Gray;
                    MessageBox.Show("Miss");
                }
                //win condition check
                if (OppShipsOnBoard == 0)
                {
                    MessageBox.Show("Player wins!!!");
                    this.Close();
                    FirstForm.Show();
                    return;
                }
                GameplayRotation();
            }

            //Opponent turn logic
            else if (IsPlayerTurn == false)
            {
                bool reselectneeded = true;
                bool hit = false;
                while (reselectneeded == true)
                {
                    //gets attack coordinates from computer
                    ComputerOpponent("gameplay");
                    selectedcell[0] = OppAttackCoords[0];
                    selectedcell[1] = OppAttackCoords[1];
                    //null check for already selected cell
                    if (HumanBoardArr[selectedcell[0], selectedcell[1]] != null)
                    {
                        shiphit = Convert.ToString(HumanBoardArr[selectedcell[0], selectedcell[1]]);
                        if (shiphit.StartsWith("hit") || shiphit == "Miss" || shiphit == "destroyed")
                        {
                            //if attack orientation has already been selected
                            if (OppAttackOrientation != "")
                            {
                                //if direction has already been reversed
                                if (OppDirectionReversed == true)
                                {
                                    CPUGameplayVariableReset();
                                }
                                //if direction has not been reversed yet
                                else if (OppInitialHit[0] != -1)
                                {
                                    OppDirectionReversed = true;
                                    OppReversedCounter = 1;
                                }
                            }
                            continue;
                        }
                        else if (shiphit == "Gunboat")
                        {
                            //Gunboat hit logic
                            HumanShipSquaresLeft[0] -= 1;
                            HumanBoardArr[selectedcell[0], selectedcell[1]] = "hit" + shiphit;
                            HumanBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                            //Ship destroyed condition check and update
                            if (HumanShipSquaresLeft[0] == 0)
                            {
                                shipdestroyed = true;
                                PlayerShipsOnBoard -= 1;
                            }
                            hit = true;
                            reselectneeded = false;
                        }
                        else if (shiphit == "Destroyer")
                        {
                            //Destroyer hit logic
                            HumanShipSquaresLeft[1] -= 1;
                            HumanBoardArr[selectedcell[0], selectedcell[1]] = "hit" + shiphit;
                            HumanBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                            //Ship destroyed condition check and update
                            if (HumanShipSquaresLeft[1] == 0)
                            {
                                shipdestroyed = true;
                                PlayerShipsOnBoard -= 1;
                            }
                            hit = true;
                            reselectneeded = false;
                        }
                        else if (shiphit == "Cruiser")
                        {
                            //Cruiser hit logic
                            HumanShipSquaresLeft[2] -= 1;
                            HumanBoardArr[selectedcell[0], selectedcell[1]] = "hit" + shiphit;
                            HumanBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                            //Ship destroyed condition check and update
                            if (HumanShipSquaresLeft[2] == 0)
                            {
                                shipdestroyed = true;
                                PlayerShipsOnBoard -= 1;
                            }
                            hit = true;
                            reselectneeded = false;
                        }
                        else if (shiphit == "Battleship")
                        {
                            //Battleship hit logic
                            HumanShipSquaresLeft[3] -= 1;
                            HumanBoardArr[selectedcell[0], selectedcell[1]] = "hit" + shiphit;
                            HumanBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                            //Ship destroyed condition check and update
                            if (HumanShipSquaresLeft[3] == 0)
                            {
                                shipdestroyed = true;
                                PlayerShipsOnBoard -= 1;
                            }
                            hit = true;
                            reselectneeded = false;
                        }
                        else if (shiphit == "Aircraft Carrier")
                        {
                            //Aircraft carrier hit logic
                            HumanShipSquaresLeft[4] -= 1;
                            HumanBoardArr[selectedcell[0], selectedcell[1]] = "hit" + shiphit;
                            HumanBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                            //Ship destroyed condition check and update
                            if (HumanShipSquaresLeft[4] == 0)
                            {
                                shipdestroyed = true;
                                PlayerShipsOnBoard -= 1;
                            }
                            hit = true;
                            reselectneeded = false;
                        }

                        //Hit condition update and message
                        if (hit == true)
                        {
                            if (OppInitialHit[0] == -1)
                            {
                                OppInitialHit[0] = selectedcell[0];
                                OppInitialHit[1] = selectedcell[1];
                            }
                            else if (selectedcell[0] == OppInitialHit[0])
                            {
                                OppAttackOrientation = "horizontal";
                            }
                            else if (selectedcell[1] == OppInitialHit[1])
                            {
                                OppAttackOrientation = "vertical";
                            }
                            OppLastSuccessfulHit[0] = selectedcell[0];
                            OppLastSuccessfulHit[1] = selectedcell[1];
                            OppAllSuroundingCells = "";
                            MessageBox.Show($"Computer hit your {shiphit}!");
                        }

                        //Ship destroyed condition check and update
                        if (shipdestroyed == true)
                        {
                            for (int row = 0; row < 10; row += 1)
                            {
                                for (int col = 0; col < 10; col += 1)
                                {
                                    if (HumanBoardArr[row, col] == "hit" + shiphit)
                                    {
                                        HumanBoardArr[row, col] = "destroyed";
                                        HumanBoard[col, row].Style.BackColor = Color.DarkRed;
                                    }
                                }
                            }
                        }
                    }
                    //miss condition
                    else
                    {
                        HumanBoardArr[selectedcell[0], selectedcell[1]] = "Miss";
                        HumanBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Gray;
                        //if attack orientation has already been selected
                        if (OppAttackOrientation != "")
                        {
                            //if direction has not been reversed
                            if (OppDirectionReversed == false)
                            {
                                OppDirectionReversed = true;
                                OppReversedCounter = 1;
                            }
                            //if direction has already been reversed
                            else
                            {
                                OppDirectionReversed = true;
                                OppReversedCounter = 1;
                                CPUGameplayVariableReset();
                            }
                        }
                        reselectneeded = false;
                    }
                }
                //win condition check
                if (PlayerShipsOnBoard == 0)
                {
                    MessageBox.Show("Computer wins!!!");
                    this.Close();
                    FirstForm.Show();
                    return;
                }
                //Turn rotation
                GameplayRotation();
            }
        }
    }
}