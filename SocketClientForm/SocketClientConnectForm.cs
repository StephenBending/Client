using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketClientForm
{
    public partial class SocketClientConnectForm : Form
    {

        public SocketClientConnectForm()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            var chat = new SocketClientChat(txtIPAddr.Text,txtPortNo.Text,txtUser.Text);
            chat.Location = this.Location;
            chat.FormClosing += delegate { this.Close(); };
            chat.Show();
            this.Hide();
        }

        private void SocketClientConnectForm_Load(object sender, EventArgs e)
        {

        }
    }
}
