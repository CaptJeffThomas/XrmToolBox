﻿// PROJECT : MsCrmTools.SiteMapEditor
// This project was developed by Tanguy Touzard
// CODEPLEX: http://xrmtoolbox.codeplex.com
// BLOG: http://mscrmtools.blogspot.com

using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace MsCrmTools.SiteMapEditor.Forms
{
    public partial class SiteMapComponentPicker : Form
    {
        public XmlNode SelectedNode { get; set; }

        public SiteMapComponentPicker(string componentName)
        {
            InitializeComponent();

            XmlDocument doc = new XmlDocument();
            
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            using (StreamReader reader = new StreamReader(myAssembly.GetManifestResourceStream("MsCrmTools.SiteMapEditor.Resources.sitemap.xml")))
            {
                doc.LoadXml(reader.ReadToEnd());
            }
            
            FillList(doc, componentName);

            ToolTip tip = new ToolTip();
            tip.ToolTipTitle = "Information";
            tip.SetToolTip(chkAddChildComponents, "Check this control if you want to add components under the one you select (ie. Area with all child Groups and SubArea or just Area)");
        }

        private void FillList(XmlDocument doc, string componentName)
        {
            try
            {
                XmlNodeList list = doc.SelectSingleNode("ImportExportXml/SiteMap/SiteMap").SelectNodes("//" + componentName);

                foreach (XmlNode node in list)
                {
                    if (!lstComponents.Items.ContainsKey(node.Attributes["Id"].Value))
                    {
                        ListViewItem item = new ListViewItem(node.Attributes["Id"].Value);
                        item.SubItems.Add(node.Attributes["Entity"] != null ? node.Attributes["Entity"].Value : "-");
                        item.SubItems.Add(node.Attributes["ResourceId"] != null ? node.Attributes["ResourceId"].Value : "-");
                        item.Tag = node;

                        if (node.ParentNode != null)
                        {
                            string groupName = node.ParentNode.Name;

                            if (node.ParentNode.Attributes["Id"] != null)
                            {
                                groupName += " (" + node.ParentNode.Attributes["Id"].Value + ")";
                            }

                            ListViewGroup group = lstComponents.Groups[groupName.Replace(" ", "")];

                            if (group == null)
                            {
                                group = new ListViewGroup(groupName);
                                group.Name = groupName.Replace(" ", "");
                                lstComponents.Groups.Add(group);
                            }

                            item.Group = group;
                        }

                        lstComponents.Items.Add(item);
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(this, error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);            
            }
        }

        private void lstComponents_DoubleClick(object sender, EventArgs e)
        {
            btnComponentPickerValidate_Click(sender, e);
        }

        private void btnComponentPickerValidate_Click(object sender, EventArgs e)
        {
            if (lstComponents.SelectedItems.Count > 0)
            {
                XmlNode selectedXmlNode = (XmlNode)lstComponents.SelectedItems[0].Tag;

                if (!chkAddChildComponents.Checked)
                {
                    for (int i = selectedXmlNode.ChildNodes.Count - 1; i >= 0; i--)
                    {
                        selectedXmlNode.RemoveChild(selectedXmlNode.ChildNodes[i]);
                    }
                }

                SelectedNode = selectedXmlNode;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(this, "Please select a component!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnComponentPickerCancel_Click(object sender, EventArgs e)
        {
            SelectedNode = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}