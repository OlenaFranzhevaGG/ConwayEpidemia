﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Conway
{
    public partial class Function : Form
    {
        

        public delegate decimal AllCellsFunc(decimal x, decimal y);
        public CaclulationFunctionVM funcParsing = new CaclulationFunctionVM();
        public AllCellsFunc allCellf;
        private readonly Form1 mainForm;
        TextBox[] tb = new TextBox[9];
       
        public decimal[] innerParameters = new decimal[9];
        public bool currentSeparate = false;
        public int HeightImg = 0;
        public int WidthImg = 0;
        public int scale = 0;
        public decimal epsilon = 0.0m;
        public decimal nu = 0.0m;
        public decimal omega = 0.0m;
        bool initFromImage = false;
        public Function(Form1 form)
        {
            InitializeComponent();            
            ClearControlls();
            mainForm = form;
        }
        private void Function_Load(object sender, EventArgs e)
        {
            
            int x, y;
            for (int i = 0; i < 9; i++)
            {
                x = (i == 0 || i == 7 || i == 6) ? 0 : (i == 1 || i == 8 || i == 5) ? 30 : 60;
                y = (i == 7 || i == 8 || i == 3) ? 30 : (i == 0 || i == 1 || i == 2) ? 0 : 60;
                panel2.Controls.Add(tb[i] = new TextBox() { Location = new Point(x, y), Width=28 });
            }            

            ok.Enabled = false;
        }
        //Set function button
        private void button1_Click(object sender, EventArgs e)
        {
            if (fieldsizeHeighttb.Text != "" && fieldsizeHeighttb.Text != "Height" && fieldsizeWidthtb.Text != "Width" && fieldsizeWidthtb.Text != "" && scaletb.Text != "")
            {
                try
                {
                    var uiValidation = 0.0m;
                    for (int p = 0; p < 9; p++)
                    {
                        if (tb[p].Text != "")

                            innerParameters[p] = Convert.ToDecimal(tb[p].Text);

                        else
                            innerParameters[p] = p == 8 ? 1 : 0;
                        uiValidation += innerParameters[p];
                    }
                    if (uiValidation == 1)
                        tb[8].Text = "1";
                    HeightImg = Convert.ToInt32(fieldsizeHeighttb.Text);
                    WidthImg = Convert.ToInt32(fieldsizeWidthtb.Text);
                    scale = Convert.ToInt32(scaletb.Text);
                    epsilon = Convert.ToDecimal(EpsilonTB.Text);
                    nu = Convert.ToDecimal(NuTB.Text);
                    omega = Convert.ToDecimal(OmegaTB.Text);
                    //allCellf = new AllCellsFunc(funcParsing.FunctionForAllParsed("return 4 * (1 - 0.05m * y) * x * (1 - x);"));

                    ok.Enabled = true;
                    weightsLbl.ForeColor = Color.Green;
                    weightsLbl.Text = "Data set is completed. Press OK to continue";
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Input string was not in a correct format.")
                        weightsLbl.Text = "*Values for cell weights, field size and scale should be decimals!";
                    else
                        weightsLbl.Text = "*There is some problem with chosen function";
                }
            }
            else
            {
                Validation();
            }           
        }
        public void Validation()
        {
            fieldSizeEmpty.Text = (fieldsizeHeighttb.Text != "" && fieldsizeHeighttb.Text != "Height")
                && (fieldsizeWidthtb.Text != "Width" && fieldsizeWidthtb.Text != "") ? "" : "*Field size is required";
            ScaleEmpty.Text = scaletb.Text != "" ? "" : "*Scale is required";
        }
        private void Clear_Click(object sender, EventArgs e)
        {
            ClearControlls();
        }
        public void ClearControlls()
        {
            fieldsizeHeighttb.ReadOnly = false;
            fieldsizeHeighttb.Text = "Height";
            fieldsizeHeighttb.ForeColor = Color.DarkGray;
            HeightImg = 0;

            fieldsizeWidthtb.ReadOnly = false;
            fieldsizeWidthtb.Text = "Width";
            fieldsizeWidthtb.ForeColor = Color.DarkGray;
            WidthImg = 0;
            scaletb.Text = "";
            scale = 0;
            fieldSizeEmpty.Text = "";
            ScaleEmpty.Text = "";
            weightsLbl.Text = "";

            for (int i = 0; i < 9; i++)
            {
                if (tb[i] != null)
                    tb[i].Text = "";
            }
            ok.Enabled = false;
            ResetCBData();
        }       
        private void ok_Click(object sender, EventArgs e)
        {
            if (initFromImage)
            {
                MessageBox.Show("Initial Data uploaded from image will be applied");
            }
            else
            {
                MessageBox.Show("Random Initial Data will be applied");
                mainForm.SetInitial();
            }
           
            mainForm.isFirstLaunch = true;            
            this.Hide();
        }
        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
        public void ResetCBData()
        {            
            this.commonAnalyticalCABindingSource.ResetBindings(true);
            CalcFunctionCB.Refresh();
        }
        private void AddNewFunctionBtn_Click(object sender, EventArgs e)
        {
            AddNewFunction addFuncForm = new AddNewFunction(this);
            addFuncForm.Show();
        }
        private void UploadInit_btn_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            var dialogResult = openFileDialog.ShowDialog();
            if (dialogResult != DialogResult.OK) return;
            ImageData(Upload(openFileDialog.FileName));            
        }
        private string Upload(string fileName)
        {           
            try
            {
                var data = File.ReadAllBytes(fileName);
                using (FileStream fcreate = File.Open(fileName, FileMode.Create))
                {
                    fcreate.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return fileName;
        }
        public void ImageData(string path)
        {
            if (path != "")
            {
               
                var image = new Bitmap(path);
                HeightImg = image.Height;
                WidthImg = image.Width;
                var initData = new CellStateVectorVM[HeightImg, WidthImg];
                
                fieldsizeHeighttb.Text = HeightImg.ToString();
                fieldsizeHeighttb.ReadOnly = true;
                fieldsizeWidthtb.Text = WidthImg.ToString();
                fieldsizeWidthtb.ReadOnly = true;
                scale = 1;
                scaletb.Text = scale.ToString();
                for (int i = 0; i < HeightImg; i++)
                    for (int j = 0; j < WidthImg; j++)                    
                        initData[i, j] = new CellStateVectorVM()
                        {
                            Susceptible = 1 - ((decimal)image.GetPixel(j, i).R) / 255,
                            Infected = 1 - ((decimal)image.GetPixel(j, i).G) / 255,
                            Recovered = 1 - ((decimal)image.GetPixel(j, i).B) / 255
                        };
                    

                mainForm.SetInitialFromImage(initData);
                initFromImage = true;
            }
        }
        private void FieldsizeHeighttb_MouseDown(object sender, MouseEventArgs e)
        {
            fieldsizeHeighttb.Text = "";
            fieldsizeHeighttb.ForeColor = Color.Black;
        }
        private void FieldsizeWidthtb_MouseDown(object sender, MouseEventArgs e)
        {
            fieldsizeWidthtb.Text = "";
            fieldsizeWidthtb.ForeColor = Color.Black;
        }
        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (useCurrentAsSeparate.Checked)
            {
                for (int i = 0; i < 9; i++)
                    tb[i].Text = "1";
                currentSeparate = true;
            }
            else {
                for (int i = 0; i < 9; i++)
                    tb[i].Text = "";
                tb[8].ReadOnly = false;
                currentSeparate = false;

            }
        }
    }    
}
