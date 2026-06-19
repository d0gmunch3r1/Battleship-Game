using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Security.Cryptography;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace Battleship_Game
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            this.KeyPreview = true;
        }

        Form1 firstform = new Form1();
        string[,] HumanBoardArr = new string[10, 10];
        string[,] OpponentBoardArr = new string[10, 10];
        Random rnd = new Random();

        string ShipCheck = "";

        string[] ShipNameArr = new string[5] { "Gunboat", "Destroyer", "Cruiser", "Battleship", "Aircraft Carrier" };
        string[] Shipcolours = new string[5] { "white", "floralwhite", "beige", "AntiqueWhite", "Tan" };
        int[] HumanShipSquaresLeft = new int[5] { 2, 3, 4, 5, 6 };
        int[] OppShipSquaresLeft = new int[5] { 2, 3, 4, 5, 6 };
        int PlayerShipsOnBoard = 0;
        int OppShipsOnBoard = 0;
        int[] OppAttackCoords = new int[2];

        bool shipplacementmodeactive = false;
        bool gameplaymodeactive = false;
        bool isplayerturn = false;

        private void Form2_Load(object sender, EventArgs e)
        {
            HumanBoard.RowCount = 10;
            OpponentBoard.RowCount = 10;
            HumanBoard.DefaultCellStyle.BackColor = Color.LightSkyBlue;
            OpponentBoard.DefaultCellStyle.BackColor = Color.LightSkyBlue;
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            OpponentBoard.ClearSelection();
            MessageBox.Show("Please place your ships on the left board");
            shipplacementmodeactive = true;
        }

        private void Form2_Keydown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                if (shipplacementmodeactive == true)
                {
                    ShipPlacement();
                }
                else if (gameplaymodeactive == true && isplayerturn == true)
                {
                    HitCalculation();
                }
            }
            if (e.KeyCode == Keys.P)
            {
                ComputerOpponent("placement");
            }
        }

        private void ProgramManager(string input)
        {
            if (Convert.ToString(input) == "ComputerSelection")
            {
                shipplacementmodeactive = false;
                ComputerOpponent("placement");
            }
            else if (Convert.ToString(input) == "GamePlay")
            {
                gameplaymodeactive = true;
                GameplayRotation();
            }
        }

        private void GameplayRotation()
        {
            if (isplayerturn == true)
            {
                OpponentBoard.Enabled = false;
                OpponentBoard.ClearSelection();
                isplayerturn = false;
                HitCalculation();

            }
            else
            {
                OpponentBoard.Enabled = true;
                isplayerturn = true;
            }
        }

        private void ShipPlacement()
        {
            //Variables
            int count = 0;
            bool validplacement = true;
            int[] firstcellplace = new int[2];
            int[] secondcellplace = new int[2];
            string type = "";
            //error check
            if (HumanBoard.SelectedCells.Count < 2)
            {
                return;
            }
            //type calculation
            DataGridViewCell firstcell = HumanBoard.SelectedCells[0];
            firstcellplace[0] = firstcell.RowIndex; firstcellplace[1] = firstcell.ColumnIndex;
            DataGridViewCell secondcell = HumanBoard.SelectedCells[1];
            secondcellplace[0] = secondcell.RowIndex; secondcellplace[1] = secondcell.ColumnIndex;
            if (firstcellplace[0] == secondcellplace[0])
            {
                type = "horizontal";
            }
            else if (firstcellplace[1] == secondcellplace[1])
            {
                type = "vertical";
            }

            //validation
            foreach (DataGridViewCell cell in HumanBoard.SelectedCells)
            {
                count += 1;
                if (HumanBoardArr[cell.RowIndex, cell.ColumnIndex] != null)
                {
                    validplacement = false;
                    break;
                }
                if (type == "horizontal")
                {
                    if (cell.RowIndex != firstcellplace[0])
                    {
                        validplacement = false;
                        break;
                    }
                }
                else if (type == "vertical")
                {
                    if (cell.ColumnIndex != firstcellplace[1])
                    {
                        validplacement = false;
                        break;
                    }
                }
            }
            if (validplacement == true && 2 <= count && count <= 6 && !ShipCheck.Contains(Convert.ToString(count)))
            {
                //Output
                foreach (DataGridViewCell cell in HumanBoard.SelectedCells)
                {
                    HumanBoardArr[cell.RowIndex, cell.ColumnIndex] = ShipNameArr[count - 2];
                    cell.Style.BackColor = Color.FromName(Shipcolours[count -2]);              
                }
                ShipCheck += Convert.ToString(count);
                PlayerShipsOnBoard += 1;
            }
            if (PlayerShipsOnBoard == 5)
            {
                HumanBoard.Enabled = false;
                HumanBoard.ClearSelection();
                ProgramManager("ComputerSelection");
            }
        }

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
                    if (rnd.Next(0, 2) == 1)
                    {
                        //valication (vertical)
                        if (OppPlacement[0] + OppShipsLeft > 9)
                        {
                            validplacement = false;
                            continue;
                        }
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
                                OpponentBoard[OppPlacement[1], OppPlacement[0] + i].Style.BackColor = Color.FromName(Shipcolours[OppShipsLeft - 1]);
                            }
                            OppShipsOnBoard += 1;
                            OppShipsLeft -= 1;
                        }
                    }
                    else
                    {
                        //validation (horizontal)
                        if (OppPlacement[1] + OppShipsLeft > 9)
                        {
                            validplacement = false;
                            continue;
                        }
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
                                OpponentBoard[OppPlacement[1] + i, OppPlacement[0]].Style.BackColor = Color.FromName(Shipcolours[OppShipsLeft - 1]);
                            }
                            OppShipsOnBoard += 1;
                            OppShipsLeft -= 1;
                        }
                    }
                }
                ProgramManager("GamePlay");
            }

            else if (Convert.ToString(mode) == "gameplay")
            {
                OppAttackCoords[0] = rnd.Next(0, 10);
                OppAttackCoords[1] = rnd.Next(0, 10);
            }
        }

        private void HitCalculation()
        {
            string shiphit;
            bool shipdestroyed = false;
            int[] selectedcell = new int[2];
            if (isplayerturn == true)
            {
                foreach (DataGridViewCell cell in OpponentBoard.SelectedCells)
                {
                    selectedcell[0] = cell.RowIndex;
                    selectedcell[1] = cell.ColumnIndex;
                }

                if (OpponentBoardArr[selectedcell[0], selectedcell[1]] != null)
                {
                    shiphit = Convert.ToString(OpponentBoardArr[selectedcell[0], selectedcell[1]]);
                    if (shiphit == "Gunboat")
                    {
                        OppShipSquaresLeft[0] -= 1;
                        OpponentBoardArr[selectedcell[0], selectedcell[1]] = "hit" + "Gunboat";
                        OpponentBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                        if (OppShipSquaresLeft[0] == 0)
                        {
                            shipdestroyed = true;
                            OppShipsOnBoard -= 1;
                        }
                        MessageBox.Show("hit");
                    }
                    else if (shiphit == "Destroyer")
                    {
                        OppShipSquaresLeft[1] -= 1;
                        OpponentBoardArr[selectedcell[0], selectedcell[1]] = "hit" + "Destroyer";
                        OpponentBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                        if (OppShipSquaresLeft[1] == 0)
                        {
                            shipdestroyed = true;
                            OppShipsOnBoard -= 1;
                        }
                        MessageBox.Show("hit");
                    }
                    else if (shiphit == "Cruiser")
                    {
                        OppShipSquaresLeft[2] -= 1;
                        OpponentBoardArr[selectedcell[0], selectedcell[1]] = "hit" + shiphit;
                        OpponentBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                        if (OppShipSquaresLeft[2] == 0)
                        {
                            shipdestroyed = true;
                            OppShipsOnBoard -= 1;
                        }
                        MessageBox.Show("hit");
                    }
                    else if (shiphit == "Battleship")
                    {
                        OppShipSquaresLeft[3] -= 1;
                        OpponentBoardArr[selectedcell[0], selectedcell[1]] = "hit" + shiphit;
                        OpponentBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                        if (OppShipSquaresLeft[3] == 0)
                        {
                            shipdestroyed = true;
                            OppShipsOnBoard -= 1;
                        }
                        MessageBox.Show("hit");
                    }
                    else if (shiphit == "Aircraft Carrier")
                    {
                        OppShipSquaresLeft[4] -= 1;
                        OpponentBoardArr[selectedcell[0], selectedcell[1]] = "hit" + shiphit;
                        OpponentBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                        if (OppShipSquaresLeft[4] == 0)
                        {
                            shipdestroyed = true;
                            OppShipsOnBoard -= 1;
                        }
                        MessageBox.Show("hit");
                    }

                    else
                    {
                        MessageBox.Show("Gridsquare already selected");
                        return;
                    }

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
                else
                {
                    OpponentBoardArr[selectedcell[0], selectedcell[1]] = "Miss";
                    OpponentBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Gray;
                    MessageBox.Show("Miss");
                }
                if (OppShipsOnBoard == 0)
                {
                    MessageBox.Show("Player wins!!!");
                    this.Close();
                    firstform.Show();
                    return;
                }
                GameplayRotation();
            }
            else if (isplayerturn == false)
            {
                bool reselectneeded = true;
                while (reselectneeded == true)
                {
                    ComputerOpponent("gameplay");
                    selectedcell[0] = OppAttackCoords[0];
                    selectedcell[1] = OppAttackCoords[1];
                    if (HumanBoardArr[selectedcell[0], selectedcell[1]] != null)
                    {
                        shiphit = Convert.ToString(HumanBoardArr[selectedcell[0], selectedcell[1]]);
                        if (shiphit == "Gunboat")
                        {
                            HumanShipSquaresLeft[0] -= 1;
                            HumanBoardArr[selectedcell[0], selectedcell[1]] = "hit" + shiphit;
                            HumanBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                            if (HumanShipSquaresLeft[0] == 0)
                            {
                                shipdestroyed = true;
                                PlayerShipsOnBoard -= 1;
                            }
                            reselectneeded = false;
                        }
                        else if (shiphit == "Destroyer")
                        {
                            HumanShipSquaresLeft[1] -= 1;
                            HumanBoardArr[selectedcell[0], selectedcell[1]] = "hit" + shiphit;
                            HumanBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                            if (HumanShipSquaresLeft[1] == 0)
                            {
                                shipdestroyed = true;
                                PlayerShipsOnBoard -= 1;
                            }
                            reselectneeded = false;
                        }
                        else if (shiphit == "Cruiser")
                        {
                            HumanShipSquaresLeft[2] -= 1;
                            HumanBoardArr[selectedcell[0], selectedcell[1]] = "hit" + shiphit;
                            HumanBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                            if (HumanShipSquaresLeft[2] == 0)
                            {
                                shipdestroyed = true;
                            }
                            reselectneeded = false;
                        }
                        else if (shiphit == "Battleship")
                        {
                            HumanShipSquaresLeft[3] -= 1;
                            HumanBoardArr[selectedcell[0], selectedcell[1]] = "hit" + shiphit;
                            HumanBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                            if (HumanShipSquaresLeft[3] == 0)
                            {
                                shipdestroyed = true;
                                PlayerShipsOnBoard -= 1;
                            }
                            reselectneeded = false;
                        }
                        else if (shiphit == "Aircraft Carrier")
                        {
                            HumanShipSquaresLeft[4] -= 1;
                            HumanBoardArr[selectedcell[0], selectedcell[1]] = "hit" + shiphit;
                            HumanBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Orange;
                            if (HumanShipSquaresLeft[4] == 0)
                            {
                                shipdestroyed = true;
                                PlayerShipsOnBoard -= 1;
                            }
                            reselectneeded = false;
                        }

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
                    else
                    {
                        HumanBoardArr[selectedcell[0], selectedcell[1]] = "Miss";
                        HumanBoard[selectedcell[1], selectedcell[0]].Style.BackColor = Color.Gray;
                        reselectneeded = false;
                    }
                }
                if (PlayerShipsOnBoard == 0)
                {
                    MessageBox.Show("Computer wins!!!");
                    this.Close();
                    firstform.Show();
                    return;
                }
                GameplayRotation();
            }
        }
    }
}