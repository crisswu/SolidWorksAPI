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
    public partial class TimeLenght : Form
    {
        public TimeLenght()
        {
            InitializeComponent();
        }
        public List<ProcessDetail> list;
        private void TimeLenght_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = list;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
