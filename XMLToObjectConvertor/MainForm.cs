﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using XmlToObjectConvertor;
using XmlToObjectConvertor.DataAccessLayer;
using XMLToObjectConvertor;
using XMLToObjectConvertor.BusinessLogicLayer;

namespace XmlConverter
{
    public partial class MainForm : Form
    {
        IList<Person> personList;
        IList<Person> dbPersonList;

        string xmlFile = null;
        string convertedXMLFilePath = null;
        string xmlFromObject = null;

        DBConnection dbc = new DBConnection();
        Functions blFunc = new Functions();
        
        public MainForm()
        {
            InitializeComponent();
            dbPersonList = dbc.Select();
            dbListBox.DataSource = dbc.Select();

            try
            {
                convertedXMLFilePath = ConfigurationSettings.AppSettings["ConvertedXMLPath"];
            }
            catch(Exception ex)
            {
                MessageBox.Show("convertedXMLFilePath Exception");
            }
        }

        public void setDBListBox(IList<Person> pList)
        {
            dbListBox.DataSource = pList;
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (.xml)|*.xml";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string file = openFileDialog.FileName;
                txtFileName.Text = Path.GetFileName(file);
                dbListBox.DataSource = dbc.Select();

                try
                {
                    xmlFile = File.ReadAllText(file);
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Read From File failed! " + ex.Message);
                }
            }
        }

        private void btnConvertToObject_Click(object sender, EventArgs e)
        {

            if (!string.IsNullOrEmpty(xmlFile))
            {
                personList = blFunc.Deserialize<List<Person>>(xmlFile);

                if (personList != null)
                    listBox.DataSource = personList;
            }
            else
            {
                MessageBox.Show("You must upload File before converting!","Warning");
            }
        }

        private void btnUpdateAll_Click(object sender, EventArgs e)
        {

            if (personList != null)
            {
                Logger.Write("Inserting to database");
                dbc.Insert(personList);
                dbListBox.DataSource = dbc.Select();
            }
            else
                MessageBox.Show("Your list is empty!");
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            dbListBox.DataSource = dbc.Select();
        }

        private void btnMakeXML_Click(object sender, EventArgs e)
        {
            btnLoad_Click(this,null);

            try
            {
                string fileName = DateTime.Now.ToString("ddMMyyyy") + "_XML.xml";
                string fullPath = convertedXMLFilePath + fileName;

                string xmlFromObject = blFunc.Serialize(dbc.Select());

                if (xmlFromObject.Contains("<Person>"))
                {
                    File.WriteAllText(fullPath, xmlFromObject);
                    MessageBox.Show("File Created at path : " + fullPath, "SystemInformation");
                }
                else
                {
                    MessageBox.Show("No data for creating file!","Warning");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message,"Error");
                Logger.Write("Exception: " + ex.Message);
            }

        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            CreatePerson newPerson = new CreatePerson(this);
            var dialogResult = newPerson.ShowDialog();
            dbListBox.DataSource = dbc.Select();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                string id = dbListBox.Text.Split(',')[0].Split(':')[1].Trim();
                dbc.Delete(id);
                dbListBox.DataSource = dbc.Select();
            }
            catch(Exception ex)
            {
                if(ex.Message.Contains("Index was outside the bounds of the array"))
                MessageBox.Show("Please select row you want to delete.","Error");
            }
        }
    }
}
