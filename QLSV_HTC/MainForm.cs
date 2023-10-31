﻿using QLSV_HTC.Forms;
//using QLSV_HTC.ReportForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QLSV_HTC
{
    public partial class MainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public bool check = true;
        public MainForm()
        {
            InitializeComponent();
        }

        private void logout()
        {
            Program.AuthUserID = string.Empty;
            Program.AuthLogin = string.Empty;
            Program.AuthPassword = string.Empty;
            Program.AuthGroup = string.Empty;
            Program.AuthHoten = string.Empty;

            Program.MaKhoa = "";
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void logoutBtn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            logout();
            Program.LoginForm.StartPosition = FormStartPosition.CenterScreen;
            Program.LoginForm.Show();
            Program.Bds_Dspm.RemoveFilter();
            this.Hide();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // set thông tin dưới status trip
            statusStrip1.Items[0].Text = string.Format("MÃ SỐ: {0}", Program.AuthUserID);
            statusStrip1.Items[1].Text = string.Format("HỌ VÀ TÊN: {0}", Program.AuthHoten);
            statusStrip1.Items[2].Text = string.Format("NHÓM: {0}", Program.AuthGroup);

            //Hiện tính năng cho mỗi quyền khác nhau
        }

        private void ShowMdiChildren(Type fType)
        {
            foreach (Form f in this.MdiChildren)
            {
                if (f.GetType() == fType)
                {
                    f.Activate();
                    return;
                }
            }
            Form form = (Form)Activator.CreateInstance(fType);
            form.MdiParent = this;
            form.Show();
        }

     // private void registerbtn_itemclick(object sender, devexpress.xtrabars.itemclickeventargs e)
       // {
      //    showmdichildren(typeof(addloginform));
      // }

     //   private void barButtonItem13_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
       // {
       //     ShowMdiChildren(typeof(SinhVienForm));
       // }

    //    private void barButtonItem10_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
    //    {
    //        ShowMdiChildren(typeof(MonHocForm));
    //    }

     //   private void barButtonItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
    //    {
     //       ShowMdiChildren(typeof(InDSLOPTINCHI));
       // }

     //   private void barButtonItem12_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
     //   {
     //       ShowMdiChildren(typeof(MoLopTinChiForm));
        }
    /** 
        private void barButtonItem15_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ShowMdiChildren(typeof(LopForm));
        }

        private void barButtonItem14_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ShowMdiChildren(typeof(DiemForm));
        }

        private void barButtonItem16_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ShowMdiChildren(typeof(DangKyLTCForm));
        }

        private void barButtonItem11_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ShowMdiChildren(typeof(HocPhiForm));
        }

        private void InDSSVBtn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ShowMdiChildren(typeof(InDSSV));
        }

        private void barButtonItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ShowMdiChildren(typeof(InPhieuDiemForm));
        }

        private void barButtonItem5_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ShowMdiChildren(typeof(InDiemForm));
        }

        private void inHocPhi_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ShowMdiChildren(typeof(InDSDongHPForm));
        }

        private void inBDTK_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ShowMdiChildren(typeof(InBangDiemTKForm));
        }

        private void barButtonItem3_ItemClick_1(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ShowMdiChildren(typeof(DangKyLTCForm));

        }
    } **/
}