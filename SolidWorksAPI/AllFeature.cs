using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SolidWorksAPI
{
    public partial class AllFeature : Form
    {
        public AllFeature()
        {
            InitializeComponent();
        }
        public List<SwCAM> list;
        private void AllFeature_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = list;
        }
    }
}
