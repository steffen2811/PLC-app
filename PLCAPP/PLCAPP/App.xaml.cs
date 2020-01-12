using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OASPCL;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MySql.Data.MySqlClient;
using System.Data;

namespace PLCAPP
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class App : ContentPage
	{
        OPCSystemsComponent opc = new OPCSystemsComponent();
        OPCSystemsData opcd = new OPCSystemsData();
        string server = "192.168.100.50";
        MySqlConnection sqlconn;
        string connsqlstring = "Server=192.168.100.50;Port=3306;database=Orders;User Id=test;Password=123;charset=utf8";
        int gulkingsMagasin;
        int MGoldMagasin;
        int PrinceRMagasin;
        string userLoggedIn;
        string info;
        bool ErrorInOrdre = false;

        public App (string username)
		{
			InitializeComponent ();

            welcome.Text = "Velkommen " + username;
            userLoggedIn = username;

            opcd.AddTag(string.Format(@"\\{0}\M1.Value", server));
            opcd.AddTag(string.Format(@"\\{0}\M2.Value", server));
            opcd.AddTag(string.Format(@"\\{0}\M3.Value", server));

            opcd.ValuesChangedAll += Opcd_ValuesChangedAll;
        }

        private void Opcd_ValuesChangedAll(string[] Tags, object[] Values, bool[] Qualities, DateTime[] TimeStamps)
        {
            Device.BeginInvokeOnMainThread(() => {
                int idx = 0;
                foreach (string t in Tags)
                {
                    if (t.Contains("M1.Value") && Values[idx] != null)
                    {
                        gulkingsMagasin = unchecked((int)Values[idx]);
                    }
                    if (t.Contains("M2.Value") && Values[idx] != null)
                    {
                        MGoldMagasin = unchecked((int)Values[idx]);
                    }
                    if (t.Contains("M3.Value") && Values[idx] != null)
                    {
                        PrinceRMagasin = unchecked((int)Values[idx]);
                    }

                    idx++;
                }
            });
        }

        private void b_Ordre(object sender, EventArgs e)
        {
            if (MGoldL.Text == "0" && PrinceRL.Text == "0" && GulKingsL.Text == "0")
            {
                ErrorInOrdre = true;
                info += "Intet valgt";
            }

            if (int.Parse(GulKingsL.Text) > gulkingsMagasin)
            {
                ErrorInOrdre = true;
                info += "Der er kun " + gulkingsMagasin + " i Gulkings magasin" + Environment.NewLine;
            }

            if (int.Parse(MGoldL.Text) > MGoldMagasin)
            {
                ErrorInOrdre = true;
                info += "Der er kun " + MGoldMagasin + " i Marlboro Gold magasin" + Environment.NewLine;
            }

            if (int.Parse(PrinceRL.Text) > PrinceRMagasin)
            {
                ErrorInOrdre = true;
                info += "Der er kun " + PrinceRMagasin + "i PrinseR magasin" + Environment.NewLine;
            }

            if (ErrorInOrdre == true)
            {
                ErrorInOrdre = false;
                DisplayAlert("Info", info, "OK");
                info = "";
            }
            else
            {
                try
                {
                    sqlconn = new MySqlConnection(connsqlstring);

                    sqlconn.Open();

                    string queryString = "insert into orders (id, date, user, GulKings, PrinceL, PrinceR) Values (Null, current_timestamp, \'" + userLoggedIn + "\', \'" + GulKingsL.Text + "\', \'" + MGoldL.Text + "\', \'" + PrinceRL.Text + "\')";
                    MySqlCommand sqlcmd = new MySqlCommand(queryString, sqlconn);
                    try
                    {
                        sqlcmd.ExecuteScalar().ToString();
                    }
                    catch { }


                    Device.BeginInvokeOnMainThread(() =>
                    {
                        opcd.SyncWriteTags(new string[] {
                    string.Format(@"\\{0}\BestillingGuleKingsM1.Value", server) },
                        new object[] { int.Parse(GulKingsL.Text) });
                    });

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        opcd.SyncWriteTags(new string[] {
                    string.Format(@"\\{0}\BestillingMGoldM2.Value", server) },
                        new object[] { int.Parse(MGoldL.Text) });
                    });

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        opcd.SyncWriteTags(new string[] {
                    string.Format(@"\\{0}\BestillingPrinceRM3.Value", server) },
                        new object[] { int.Parse(PrinceRL.Text) });
                    });

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        opcd.SyncWriteTags(new string[] {
                    string.Format(@"\\{0}\M1.Value", server) },
                        new object[] { gulkingsMagasin - int.Parse(GulKingsL.Text) });
                    });

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        opcd.SyncWriteTags(new string[] {
                    string.Format(@"\\{0}\M2.Value", server) },
                        new object[] { MGoldMagasin - int.Parse(MGoldL.Text) });
                    });

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        opcd.SyncWriteTags(new string[] {
                    string.Format(@"\\{0}\M3.Value", server) },
                        new object[] { PrinceRMagasin - int.Parse(PrinceRL.Text) });
                    });

                    DisplayAlert("Info", "Orderen bliver nu leveret", "OK");

                    sqlconn.Close();
                }
                catch
                {
                    Messages.Text = "Ingen forbindelse til databasen";
                }
            }
        }

        private void b_Logout(object sender, EventArgs e)
        {
            opcd.RemoveAllTags();
            Navigation.PushModalAsync(new login());
        }

        private void b_GulKingsplus(object sender, EventArgs e)
        {
            if (Convert.ToInt64(GulKingsL.Text) < 5)
            {
                GulKingsL.Text = Convert.ToString(Convert.ToInt64(GulKingsL.Text) + 1);
            }
        }

        private void b_GulKingsminus(object sender, EventArgs e)
        {
            if (Convert.ToInt64(GulKingsL.Text) > 0)
            {
                GulKingsL.Text = Convert.ToString(Convert.ToInt64(GulKingsL.Text) - 1);
            }
        }

        private void b_MGoldplus(object sender, EventArgs e)
        {
            if (Convert.ToInt64(MGoldL.Text) < 5)
            {
                MGoldL.Text = Convert.ToString(Convert.ToInt64(MGoldL.Text) + 1);
            }
        }

        private void b_MGoldminus(object sender, EventArgs e)
        {
            if (Convert.ToInt64(MGoldL.Text) > 0)
            {
                MGoldL.Text = Convert.ToString(Convert.ToInt64(MGoldL.Text) - 1);
            }
        }

        private void b_PrinceRplus(object sender, EventArgs e)
        {
            if (Convert.ToInt64(PrinceRL.Text) < 5)
            {
                PrinceRL.Text = Convert.ToString(Convert.ToInt64(PrinceRL.Text) + 1);
            }
        }

        private void b_PrinceRminus(object sender, EventArgs e)
        {
            if (Convert.ToInt64(PrinceRL.Text) > 0)
            {
                PrinceRL.Text = Convert.ToString(Convert.ToInt64(PrinceRL.Text) - 1);
            }
        }
    }
}