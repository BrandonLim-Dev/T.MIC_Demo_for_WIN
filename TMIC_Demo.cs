using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace T.MIC_Demo_for_WIN
{
    public partial class TMIC_Demo : Form
    {
        /// <summary>
        /// 전역변수 영역
        /// </summary>
        #region Global
        bool IsConnected = false;
        bool IsMute = true;
        #endregion

        public TMIC_Demo()
        {
            InitializeComponent();
        }

        private void TMIC_Demo_Load(object sender, EventArgs e)
        {
            // Combobox Init
            cbProtocol.SelectedIndex = 0;
        }

        #region Button
        private void btnConnection_Click(object sender, EventArgs e)
        {

        }

        private void btnMute_Click(object sender, EventArgs e)
        {

        }
        #endregion

    }
}
