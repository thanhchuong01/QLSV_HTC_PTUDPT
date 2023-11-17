﻿using DevExpress.XtraEditors;
using QLSV_HTC.Class;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLSV_HTC.Forms
{
    public partial class MoLopTinChiForm : DevExpress.XtraEditors.XtraForm
    {
        private int position = -1;
        private int position2 = -1;
        private string state;
        private List<string> mgv = new List<string>();
        private LopTinChiClass LopTinChiData = null;

        public static BindingSource dsgv = new BindingSource();
        public static BindingSource ngaydk = new BindingSource();
        public static BindingSource tghoc = new BindingSource();


        Stack<string> undoStack = new Stack<string>();
        public MoLopTinChiForm()
        {
            InitializeComponent();
            // This line of code is generated by Data Source Configuration Wizard
      

        }

        private void SetButtonState(bool value)
        {
            if (state == "edit")
            {
                barButtonAdd.Enabled
                    = barButtonEdit.Enabled
                    = barButtonDelete.Enabled
                    = barButtonRenew.Enabled
                    = gcLOPTINCHI.Enabled
                    = panelControl2.Enabled
                    = !value;

                barButtonHuy.Enabled
                    = barButtonSave.Enabled
                    = panelControl1.Enabled
                    = value;

                barButtonUndo.Enabled = undoStack.Count > 0;
            }
            else if (state == "add")
            {
                barButtonAdd.Enabled
                    = barButtonEdit.Enabled
                    = barButtonDelete.Enabled
                    = barButtonRenew.Enabled
                    = gcLOPTINCHI.Enabled
                    = panelControl2.Enabled
                    = !value;

                barButtonHuy.Enabled
                    = barButtonSave.Enabled
                    = panelControl1.Enabled
                    = value;
            }
        }

        private bool ValidateForm()
        {
            if (txtNienKhoa.Text.Trim() == "")
            {
                XtraMessageBox.Show("Niên khóa không được để trống!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (txtHocKy.Value > 4 || txtHocKy.Value < 1)
            {
                XtraMessageBox.Show("Học kỳ phải trong khoảng 1-4!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (gridViewGV.RowCount == 0)
            {
                XtraMessageBox.Show("Phải phân công giảng viên dạy!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            } 
                

            if (txtNhom.Value < 1)
            {
                XtraMessageBox.Show("Nhóm phải lớn hơn 0!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (txtSoSVTT.Value < 1)
            {
                XtraMessageBox.Show("Số sinh viên tối thiểu phải lớn hơn 0!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (txtMaMonHoc.EditValue == null)
            {
                XtraMessageBox.Show("Mã môn học không được bỏ trống!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (bdsMONHOC.Find("MAMH", txtMaMonHoc.EditValue.ToString()) == -1)
            {
                XtraMessageBox.Show("Mã môn học không tồn tại!", "Lỗi",
                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (txtMaKhoa.EditValue == null)
            {
                XtraMessageBox.Show("Mã khoa không được bỏ trống!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (gridViewTGH.RowCount == 0)
            {
                XtraMessageBox.Show("Thời gian học không được bỏ trống!", "Lỗi",
                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void barButtonAdd_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            position = bdsLOPTINCHI.Position;
            state = "add";
            SetButtonState(true);
            if (txtMaKhoa.DataBindings.Count > 0)
            {
                this.DS.LOPTINCHI.MAKHOAColumn.DefaultValue = Program.MaKhoa;
                this.DS.LOPTINCHI.HUYLOPColumn.DefaultValue = 0;
                txtMaKhoa.DataBindings[0].DataSourceNullValue = Program.MaKhoa;
            }
            bdsLOPTINCHI.AddNew();
        }

        private void barButtonEdit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            position = bdsLOPTINCHI.Position;
            state = "edit";
            SetButtonState(true);
        }

        private void barButtonRenew_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            position = 0;
            LoadData();
            XtraMessageBox.Show("Làm mới dữ liệu thành công", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void barButtonCancel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (this.panelControl1.Enabled)
            {
                string message = "Lớp tín chỉ đang thêm chưa lưu vào Database. \n Bạn có chắc muốn thoát !";
                if (state == "edit") message = "Lớp tín chỉ đang hiệu chỉnh chưa lưu vào Database. \n Bạn có chắc muốn thoát !";
                DialogResult dr = XtraMessageBox.Show(message, "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.No)
                {
                    return;
                }
            }
            this.Close();
        }

        private bool checkLTC()
        {
            string query = string.Format(" EXEC sp_KiemTraLTC @NIENKHOA = N'{0}', @HOCKY = {1}, @MAMH = N'{2}', @NHOM = {3}", txtNienKhoa.Text.Trim(), Convert.ToInt32(txtHocKy.Text), txtMaMonHoc.Text.Trim(), Convert.ToInt32(txtNhom.Text));

            var check = Program.ExecSqlNonQuery(query);
            if (check == 0) return true;
            return false;
        }

        private void barButtonSave_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!ValidateForm()) return;
            if (state == "add" && !checkLTC()) return;
            try
            {
                if(state =="edit")
                    if (LopTinChiData.SoSVTT < Convert.ToInt32(txtSoSVTT.Text) && bdsDANGKY.Count > 0)
                    {
                        XtraMessageBox.Show("Số SV tối thiểu sau khi sửa phải lớn hơn SV tối thiểu trước đó", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                this.bdsLOPTINCHI.EndEdit();
                this.bdsLOPTINCHI.ResetCurrentItem();
                this.LOPTINCHITableAdapter.Update(this.DS.LOPTINCHI);

                if (state == "edit")
                {
                    undoStack.Push(string.Format("UPDATE LOPTINCHI SET NIENKHOA = N'{0}', HOCKY = {1}, MAMH = N'{2}', NHOM = {3}, MAKHOA = N'{4}', SOSVTOITHIEU = {5}, HUYLOP = {6} WHERE MALTC = {7}", LopTinChiData.NienKhoa, LopTinChiData.HocKy, LopTinChiData.MaMonHoc, LopTinChiData.Nhom, LopTinChiData.MaKhoa, LopTinChiData.SoSVTT, Convert.ToInt32(LopTinChiData.HuyLop), LopTinChiData.MaLTC));
                }
            }
            catch (Exception ex)
            {
                bdsLOPTINCHI.RemoveCurrent();
                XtraMessageBox.Show("Ghi dữ liệu thất lại. Vui lòng kiểm tra lại!\n" + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetButtonState(false);
        }

        private void barButtonUndo_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string command = undoStack.Pop();
            Program.ExecSqlNonQuery(command);

            LoadData();
            barButtonUndo.Enabled = undoStack.Count > 0;
        }

        private void barButtonDelete_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (bdsDANGKY.Count > 0)
            {
                XtraMessageBox.Show("Không thể xóa lớp tín chỉ này vì đã có sinh viên đăng ký.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (XtraMessageBox.Show("Bạn có thực sự muốn xóa lớp tín chỉ này?", "Xác nhận.", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                try
                {
                    position = bdsLOPTINCHI.Position;
                    bdsLOPTINCHI.RemoveCurrent();
                    this.LOPTINCHITableAdapter.Connection.ConnectionString = Program.ConnStr;
                    this.LOPTINCHITableAdapter.Update(this.DS.LOPTINCHI);
                    this.bdsLOPTINCHI.ResetCurrentItem();

                    undoStack.Push(string.Format("INSERT LOPTINCHI (NIENKHOA, HOCKY, MAMH, NHOM, MAGV, MAKHOA, SOSVTOITHIEU, HUYLOP) VALUES (N'{0}', {1}, N'{2}', {3}, N'{4}', N'{5}', {6}, {7})", LopTinChiData.NienKhoa, LopTinChiData.HocKy, LopTinChiData.MaMonHoc, LopTinChiData.Nhom, LopTinChiData.MaKhoa, LopTinChiData.SoSVTT, Convert.ToInt32(LopTinChiData.HuyLop)));
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("Lỗi xóa lớp tín chỉ.\nMã lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK);
                    this.LOPTINCHITableAdapter.Fill(this.DS.LOPTINCHI);
                    bdsLOPTINCHI.Position = position;
                }
            }
            barButtonDelete.Enabled = bdsLOPTINCHI.Count > 0;
            barButtonUndo.Enabled = undoStack.Count > 0;
        }

        private void lOPTINCHIBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.bdsLOPTINCHI.EndEdit();
            this.tableAdapterManager.UpdateAll(this.DS);
        }

        private void LoadData()
        {
            DS.EnforceConstraints = false;

            this.GIANGVIENTableAdapter.Connection.ConnectionString = Program.ConnStr;
            // TODO: This line of code loads data into the 'DS.GIANGVIEN' table. You can move, or remove it, as needed.
            this.GIANGVIENTableAdapter.Fill(this.DS.GIANGVIEN);

            this.MONHOCTableAdapter.Connection.ConnectionString = Program.ConnStr;
            // TODO: This line of code loads data into the 'DS.MONHOC' table. You can move, or remove it, as needed.
            this.MONHOCTableAdapter.Fill(this.DS.MONHOC);


            this.DANGKYTableAdapter.Connection.ConnectionString = Program.ConnStr;
            // TODO: This line of code loads data into the 'DS.DANGKY' table. You can move, or remove it, as needed.
            this.DANGKYTableAdapter.Fill(this.DS.DANGKY);

            this.LOPTINCHITableAdapter.Connection.ConnectionString = Program.ConnStr;
            // TODO: This line of code loads data into the 'dS.MONHOC' table. You can move, or remove it, as needed.
            this.LOPTINCHITableAdapter.Fill(this.DS.LOPTINCHI);

            this.lICHHOCTableAdapter.Connection.ConnectionString = Program.ConnStr;
            this.lICHHOCTableAdapter.Fill(this.DS.LICHHOC);                      

            this.dAYTableAdapter.Connection.ConnectionString = Program.ConnStr;
            // TODO: This line of code loads data into the 'dS.MONHOC' table. You can move, or remove it, as needed.
            this.dAYTableAdapter.Fill(this.DS.DAY);
            //   this.dataGridViewGV.DataSource = null;

            this.sp_GetGVLTCTableAdapter.Fill(this.DS.sp_GetGVLTC);
            this.sp_GetAllTGHTableAdapter.Fill(this.DS.sp_GetAllTGH);

            if (position > -1)
            {
                bdsLOPTINCHI.Position = position;
            }

            barButtonEdit.Enabled = barButtonDelete.Enabled = bdsLOPTINCHI.Count > 0;

            gridViewGV.SetAutoFilterValue(colMALTC, txtMaLTC.Text, DevExpress.XtraGrid.Columns.AutoFilterCondition.Equals);
            gridViewTGH.SetAutoFilterValue(colMALTC1, txtMaLTC.Text, DevExpress.XtraGrid.Columns.AutoFilterCondition.Equals);
        }

        private void MoLopTinChiForm_Load(object sender, EventArgs e)
        {
       
            // TODO: This line of code loads data into the 'DS.THOIGIANHOC' table. You can move, or remove it, as needed.
            this.tHOIGIANHOCTableAdapter.Fill(this.DS.THOIGIANHOC);
            // TODO: This line of code loads data into the 'DS.LICHHOC' table. You can move, or remove it, as needed.
            this.lICHHOCTableAdapter.Fill(this.DS.LICHHOC);
            // TODO: This line of code loads data into the 'DS.THOIGIANDK' table. You can move, or remove it, as needed.
            this.tHOIGIANDKTableAdapter.Fill(this.DS.THOIGIANDK);
            // TODO: This line of code loads data into the 'DS.DAY' table. You can move, or remove it, as needed.
            this.dAYTableAdapter.Fill(this.DS.DAY);
           // TODO: This line of code loads data into the 'DS.sp_GetAllTGH' table. You can move, or remove it, as needed.
            this.sp_GetAllTGHTableAdapter.Fill(this.DS.sp_GetAllTGH);
            // TODO: This line of code loads data into the 'DS.sp_GetGVLTC' table. You can move, or remove it, as needed.
            this.sp_GetGVLTCTableAdapter.Fill(this.DS.sp_GetGVLTC);  
          //  MoLopTinChiForm.tghoc.DataSource = this.tHOIGIANHOCBindingSource;

            //if (Program.AuthGroup == "PGV")
            //{
            //    Program.Bds_Dspm.Filter = "TENKHOA <> 'Phòng Kế Toán'";
            //}
            //else if (Program.AuthGroup == "KHOA")
            //{
            //    Program.Bds_Dspm.Filter = string.Format("TENSERVER = '{0}'", Program.ServerName);
            //}
            DataTable dt = new DataTable();
            //gọi Sql và trả về dưới dạng datatable
            dt = Program.ExecSqlDataTable("SELECT MALTC, LICHHOC.MATGH, BUOIHOC, THU FROM LICHHOC, THOIGIANHOC WHERE LICHHOC.MATGH = THOIGIANHOC.MATGH");

            // cất dt vào biến toàn cục tghoc
            MoLopTinChiForm.tghoc.DataSource = dt;

            Utils.LoadComboBox(cmbKhoa, Program.Bds_Dspm.DataSource);

            if (Utils.ChangeServer(cmbKhoa))
            {
                LoadData();
            }

            gridViewGV.SetAutoFilterValue(colMALTC, txtMaLTC.Text, DevExpress.XtraGrid.Columns.AutoFilterCondition.Equals);
            gridViewTGH.SetAutoFilterValue(colMALTC1, txtMaLTC.Text, DevExpress.XtraGrid.Columns.AutoFilterCondition.Equals);

        }

        private void load_DateTimeDK()
        {
            string queryGetDate = string.Format(" SELECT N'Từ ' + CONVERT(nvarchar,NGAYBDDK,103) + N' đến ' + CONVERT(nvarchar,NGAYKTDK, 103) as DATA FROM THOIGIANDK WHERE MATGDK = {0}", this.LopTinChiData.MaTGDK.ToString());
            var check = Program.ExecSqlDataReader(queryGetDate);
            Console.WriteLine(this.LopTinChiData.MaTGDK);
            if (check != null)
            {
                check.Read();               
                this.textBoxTime.Text = check.GetString(0);
            }
            check.Close();
        }

        private void cmbKhoa_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Utils.ChangeServer(cmbKhoa))
            {
                LoadData();
            }

        }

        private void barButtonHuy_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            bdsLOPTINCHI.CancelEdit();
            SetButtonState(false);
            MoLopTinChiForm_Load(sender, e);
        }

        private void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            try
            {                            

                LopTinChiData = new LopTinChiClass(Convert.ToInt32(txtMaLTC.Text), txtNienKhoa.Text.Trim(), txtMaMonHoc.EditValue.ToString().Trim(), txtMaKhoa.Text.Trim(), Convert.ToInt32(txtHocKy.Text), Convert.ToInt32(txtNhom.Text), Convert.ToInt32(txtSoSVTT.Text), hUYLOPCheckBox.Checked, Convert.ToInt32(txtMaTGDK.Text));
              //  Console.WriteLine(cellData);
            }
            catch { }

            // get data ngay dang ky
            MoLopTinChiForm.ngaydk.ResetBindings(true);
            MoLopTinChiForm.ngaydk.Filter = string.Format("MATGDK = {0}", LopTinChiData.MaTGDK.ToString());

            // filter value of ds GV and time register MH
            gridViewGV.SetAutoFilterValue(colMALTC, txtMaLTC.Text, DevExpress.XtraGrid.Columns.AutoFilterCondition.Equals);
            gridViewTGH.SetAutoFilterValue(colMALTC1, txtMaLTC.Text, DevExpress.XtraGrid.Columns.AutoFilterCondition.Equals);

            
            // get data ds giang vien 
            MoLopTinChiForm.dsgv.ResetBindings(true);
            MoLopTinChiForm.dsgv.RemoveFilter();
            MoLopTinChiForm.dsgv.Filter = string.Format("MALTC = {0}", LopTinChiData.MaLTC.ToString());
         


            // get thoi gian hoc               
            MoLopTinChiForm.tghoc.ResetBindings(true);
            MoLopTinChiForm.tghoc.RemoveFilter();
            MoLopTinChiForm.tghoc.Filter = string.Format("MALTC = {0}", this.LopTinChiData.MaLTC.ToString());
         
            // get thoi gian dang ky
            load_DateTimeDK();
        }


        private void radioBtnAva_CheckedChanged(object sender, EventArgs e)
        {
            this.datiNewEnd.Enabled = false;
            this.datiNewStart.Enabled = false;
            this.lookUpEditTimeDK.Enabled = true;
        }

        private void radioBtnNew_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioBtnNew.Checked)
                {
                this.lookUpEditTimeDK.Enabled = false;
                this.datiNewEnd.Enabled = true;
                this.datiNewStart.Enabled = true;
                }
        }


        private void btnAddGV_Click(object sender, EventArgs e)
        {
            var magv = comboBoxGV.EditValue.ToString().Trim();
            if (magv == null) return;
            string query = string.Format("INSERT DAY (MALTC, MAGV) VALUES( {0}, N'{1}' )  ",Convert.ToInt32(this.txtMaLTC.Text), magv );
            var check = Program.ExecSqlNonQuery(query);
           
       
            this.comboBoxGV.EditValue = null;
            this.btnAddGV.Enabled = false;

    
            LoadData();

        }

        private void btnDelGV_Click(object sender, EventArgs e)
        {
            if (XtraMessageBox.Show("Bạn có thực sự muốn xóa giảng viên này?", "Xác nhận.", MessageBoxButtons.OKCancel) == DialogResult.OK) { 
                position2 = spGetGVLTCBindingSource.Position;
                int maltc = Convert.ToInt32(((DataRowView)spGetGVLTCBindingSource[spGetGVLTCBindingSource.Position])["MALTC"].ToString());
                string magv = (((DataRowView)spGetGVLTCBindingSource[spGetGVLTCBindingSource.Position])["MAGV"].ToString());

                Console.WriteLine(maltc + " " + magv);
                undoStack.Push(string.Format("INSERT DAY (MALTC, MAGV) VALUES ({0}, N'{1}' )", maltc, magv));
                string query = string.Format("DELETE FROM DAY WHERE MALTC = {0} AND MAGV = N'{1}' ", maltc, magv);
                var check = Program.ExecSqlNonQuery(query);
                
                LoadData();
            }
        }

        private void comboBoxGV_EditValueChanged(object sender, EventArgs e)
        {
            this.btnAddGV.Enabled = true;
        }

        private void lookUpEdit1_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void btnDKTime_Click(object sender, EventArgs e)
        {
            if (XtraMessageBox.Show("Lưu thời gian đăng ký ?", "Xác nhận", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {

                if (this.radioBtnAva.Checked)
                {
                    this.LopTinChiData.MaTGDK = Convert.ToInt32(this.lookUpEditTimeDK.SelectedText.ToString());
                }
                else if (this.radioBtnNew.Checked)
                {
                    string startDate = this.datiNewStart.Value.ToString("yyyy-MM-dd");
                    string endDate = this.datiNewEnd.Value.ToString("yyyy-MM-dd");
                    string query = string.Format("INSERT THOIGIANDK (NGAYBDDK, NGAYKTDK) VALUES( N'{0}', N'{1}' )  ", startDate, endDate);
                    int check = Program.ExecSqlNonQuery(query);


                    if (check == 0)
                    {

                        string queryExist = string.Format("SELECT MATGDK FROM THOIGIANDK WHERE NGAYBDDK = N'{0}' AND NGAYKTDK = N'{1}'", startDate, endDate);
                        var matgdk = Program.ExecSqlDataReader(queryExist);
                        matgdk.Read();
                        this.LopTinChiData.MaTGDK = matgdk.GetInt32(0);
                        matgdk.Close();
                        
                        var updateTime = Program.ExecSqlNonQuery(string.Format("UPDATE LOPTINCHI SET MATGDK = {0} WHERE MALTC = {1}", this.LopTinChiData.MaTGDK, this.LopTinChiData.MaLTC));
                        if (updateTime == 0)
                        {
                            Console.WriteLine(" them xong");
                        }
                        Console.WriteLine(this.LopTinChiData.MaTGDK);
                        Utils.eventDataChanged();
                       
                    }
                }
            }
            Console.WriteLine("Sau khi dang ky " + this.LopTinChiData.MaTGDK);
            load_DateTimeDK();
        }

        private void buttonAddtime_Click(object sender, EventArgs e)
        {
            string newMatgh = this.lookUpEditTimeHoc.SelectedText.ToString();
            var addNewMathh = Program.ExecSqlNonQuery(string.Format("INSERT LICHHOC (MALTC, MATGH) VALUES ({0}, {1})", this.LopTinChiData.MaLTC, newMatgh));
            Console.WriteLine(addNewMathh);
            
            if (addNewMathh == 0)
            {
                Console.WriteLine("THem xong");
                this.lookUpEditTimeHoc.Text = "";
                this.buttonAddtime.Enabled = false;
            }

            LoadData();

        }

        private void buttonDelTime_Click(object sender, EventArgs e)
        {
            if (XtraMessageBox.Show("Bạn có thực sự muốn xóa buổi học này?", "Xác nhận.", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                position2 = spGetAllTGHBindingSource.Position;
                int maltc = Convert.ToInt32(((DataRowView)spGetAllTGHBindingSource[spGetAllTGHBindingSource.Position])["MALTC"].ToString());
                int matgh = Convert.ToInt32(((DataRowView)spGetAllTGHBindingSource[spGetAllTGHBindingSource.Position])["MATGH"].ToString());

                Console.WriteLine(maltc + " " + matgh);
                undoStack.Push(string.Format("INSERT LICHHOC (MALTC, MATGH) VALUES ({0}, {1} )", maltc, matgh));
                string query = string.Format("DELETE FROM LICHHOC WHERE MALTC = {0} AND MATGH = {1}", maltc, matgh);
                var check = Program.ExecSqlNonQuery(query);

                LoadData();

            }
        }

        private void lookUpEditTimeHoc_EditValueChanged(object sender, EventArgs e)
        {
            this.buttonAddtime.Enabled = true;
        }
    }
}