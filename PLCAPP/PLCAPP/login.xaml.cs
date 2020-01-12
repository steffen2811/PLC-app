using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Data;

namespace PLCAPP
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class login : ContentPage
	{
        MySqlConnection sqlconn;
        string connsqlstring = "Server=192.168.100.50;Port=3306;database=users;User Id=test;Password=123;charset=utf8";
        bool usernameFound = false;

        public login ()
		{
			InitializeComponent ();
		}

        private void b_Login(object sender, EventArgs e)
        {
            usernameFound = false;
            sqlconn = new MySqlConnection(connsqlstring);

            try
            {
                sqlconn.Open();
                DataSet tickets = new DataSet();
                string queryString = "select `username` from users";
                MySqlDataAdapter adapter = new MySqlDataAdapter(queryString, sqlconn);
                adapter.Fill(tickets, "Item");

                foreach (DataRow row in tickets.Tables["Item"].Rows)
                {
                    if (row[0].ToString() == usernameEntry.Text)
                    {
                        usernameFound = true;
                        queryString = "select password from users where username = \"" + row[0].ToString() + "\"";
                        MySqlCommand sqlcmd = new MySqlCommand(queryString, sqlconn);
                        String password = sqlcmd.ExecuteScalar().ToString();
                        if (password == passwordEntry.Text)
                        {
                            sqlconn.Close();
                            Navigation.PushModalAsync(new App(usernameEntry.Text));
                        }
                        else
                        {
                            messageLabel.Text = "Forkert password";
                        }
                    }
                }

                if (usernameFound == false)
                {
                    messageLabel.Text = "Bruger ikke fundet";
                }
            }
            catch
            {
                messageLabel.Text = "Ingen forbindelse til databasen";
            }

        }

    }
}