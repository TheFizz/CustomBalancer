using HtmlAgilityPack;
using System;
using System.Collections;
using System.Drawing;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace CustomBalancer
{
    public partial class Form1 : Form
    {
        ArrayList Players = new ArrayList();
        ArrayList t1 = new ArrayList();
        ArrayList t2 = new ArrayList();
        WebClient wc = new WebClient();
        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

        public Form1()
        {
            InitializeComponent();
            nameSearch.Focus();
            this.KeyPreview = true;
            DeclareServers();
            t1.Clear();
            t2.Clear();
        }

        //MAIN

        public Player GetStats(string name)
        {
            wc.Encoding = Encoding.UTF8;
            string nickname = null;
            string rank = null;
            int mmr = 0;
            string srv = srvBox.SelectedValue.ToString();
            string link = "http://" + srv + ".op.gg/summoner/userName=" + name;
            string profilepic="";
            string htmlCode = "";
            try
            {
                htmlCode = wc.DownloadString(link);
            }
            catch
            {
                MessageBox.Show("Can't reach OP.GG", "ERROR!"); return null;
            }
            doc.LoadHtml(htmlCode);
            HtmlNode node = doc.DocumentNode.SelectSingleNode("/html/body/div[1]/div[4]/div/div/div[1]/div[3]/div[1]/span");

            try
            { nickname = node.InnerText; }
            catch
            {
                node = doc.DocumentNode.SelectSingleNode("/html/body/div[1]/div[3]/div/div/div[1]/div[3]/div[1]/span");
                try
                { nickname = node.InnerText; }
                catch { MessageBox.Show("Invalid summoner name, server or not existing on OP.GG", "ERROR!"); return null; };
            }

            node = doc.DocumentNode.SelectSingleNode("//*[@id='SummonerLayoutContent']/div[2]/div[1]/div[1]/div[1]/div[2]/div[2]");

            try
            { rank = node.InnerText; }
            catch
            {
                node = doc.DocumentNode.SelectSingleNode("//*[@id='SummonerLayoutContent']/div[2]/div[1]/div[1]/div/div[2]/div[2]");

                try
                { rank = node.InnerText; }
                catch { MessageBox.Show("Invalid summoner name, server or not existing on OP.GG", "ERROR!"); return null; };
            }

            node = doc.DocumentNode.SelectSingleNode("/html/body/div[1]/div[3]/div/div/div[1]/div[2]/div/img");

            try
            { profilepic = node.OuterHtml; }
            catch
            {
                node = doc.DocumentNode.SelectSingleNode("/html/body/div[1]/div[4]/div/div/div[1]/div[1]/div/img");

                try
                { profilepic = node.OuterHtml; }
                catch { MessageBox.Show("Invalid summoner name, server or not existing on OP.GG", "ERROR!"); return null; };
            }

            profilepic = profilepic.Substring(profilepic.IndexOf('"')+1);
            profilepic = profilepic.Split('"')[0];
            profilepic = "https:" + profilepic;
            if (!rank.Contains("Unranked"))
            {
                mmr = RankToELO(rank);
                node = doc.DocumentNode.SelectSingleNode("//*[@id='SummonerLayoutContent']/div[2]/div[1]/div[1]/div/div[2]/div[3]/span[1]");
                string LP = node.InnerText.Replace(" LP", "");
                mmr += Convert.ToInt32(LP);
            }
            else
            {
                rank = "Unranked";
                mmr = RankToELO(rank);
            }
            return new Player(nickname, rank, mmr, link,profilepic);
        }
        public int RankToELO(string rank)
        {
            rank = rank.ToLower();
            int mult = 0;
            if (rank.Contains("iron"))
            {
                mult = (int)Char.GetNumericValue(rank[rank.Length - 1]);
                switch (mult)
                {
                    case 1:
                        return 300;
                    case 2:
                        return 200;
                    case 3:
                        return 100;
                    case 4:
                        return 0;
                }
            }
            if (rank.Contains("bronze"))
            {
                mult = (int)Char.GetNumericValue(rank[rank.Length - 1]);
                switch (mult)
                {
                    case 1:
                        return 700;
                    case 2:
                        return 600;
                    case 3:
                        return 500;
                    case 4:
                        return 400;
                }
            }
            else if (rank.Contains("silver"))
            {
                mult = (int)Char.GetNumericValue(rank[rank.Length - 1]);
                switch (mult)
                {
                    case 1:
                        return 1100;
                    case 2:
                        return 1000;
                    case 3:
                        return 900;
                    case 4:
                        return 800;
                }
            }
            else if (rank.Contains("gold"))
            {
                mult = (int)Char.GetNumericValue(rank[rank.Length - 1]);
                switch (mult)
                {
                    case 1:
                        return 1500;
                    case 2:
                        return 1400;
                    case 3:
                        return 1300;
                    case 4:
                        return 1200;
                }
            }
            else if (rank.Contains("platinum"))
            {
                mult = (int)Char.GetNumericValue(rank[rank.Length - 1]);
                switch (mult)
                {
                    case 1:
                        return 1900;
                    case 2:
                        return 1800;
                    case 3:
                        return 1700;
                    case 4:
                        return 1600;
                }
            }
            else if (rank.Contains("diamond"))
            {
                mult = (int)Char.GetNumericValue(rank[rank.Length - 1]);
                switch (mult)
                {
                    case 1:
                        return 2300;
                    case 2:
                        return 2200;
                    case 3:
                        return 2100;
                    case 4:
                        return 2000;
                }
            }
            else if (rank.Contains("master"))
            {
                if (rank.Contains("grand"))
                {
                    return 2500;
                }
                else
                    return 2400;
            }
            else if (rank.Contains("challenger"))
            {
                return 2600;
            }
            else if (rank.Contains("unranked"))
            {
                return 1050;
            }

            return 0;
        }
        public void TeamBalancer()
        {
            int[] pool = new int[10];
            for (int i = 0; i < Players.Count; i++)
            {
                Player p = (Player)Players[i];
                pool[i] = p.mmr;
            }
            Array.Sort(pool); // backwards sort
            int sum = 0;
            foreach (int value in pool)
                sum += value;
            float halfSum = sum / 2f;

            int[] team1 = new int[5]; 
            int[] team2 = new int[5]; 
            int sum1 = 0, sum2 = 0;
            int tcnt1 = 0, tcnt2 = 0;
            for (int i = 9; i >= 0; i--)
            {
                if (sum1 <= sum2 || sum2 >= halfSum || tcnt2==5)
                {
                    team1[tcnt1] = pool[i];
                    sum1 += pool[i];
                    tcnt1++;
                }
                else
                {
                    team2[tcnt2] = pool[i];
                    sum2 += pool[i];
                    tcnt2++;
                }
            }
            for (int i = 0; i < team1.Length; i++)
            {
                for (int j = 0; j < Players.Count; j++)
                {
                    Player p = (Player)Players[j];
                    if (team1[i] == p.mmr && p.team == 0)
                    {
                        t1.Add(p);
                        p.team = 1;
                        Players[j] = p;
                        break;
                    }
                }
            }
            for (int i = 0; i < team2.Length; i++)
            {
                for (int j = 0; j < Players.Count; j++)
                {
                    Player p = (Player)Players[j];
                    if (team2[i] == p.mmr && p.team == 0)
                    {
                        t2.Add(p);
                        p.team = 2;
                        Players[j] = p;
                        break;
                    }
                }
            }
            DisplayTeams();
        }
        public void DisplayTeams()
        {
            Player p;
            foreach (Control c in gT1.Controls)
            {
                if (c is Label)
                {
                    p = (Player)t1[(Int32)Char.GetNumericValue(c.Name, c.Name.Length - 1)];
                    c.Text = p.name + "\n" + p.rank;
                    PictureBox pb = (PictureBox)gT1.Controls.Find("elot1p" + c.Name[c.Name.Length - 1], false)[0];
                    PictureBox pbp = (PictureBox)gT1.Controls.Find("proft1p" + c.Name[c.Name.Length - 1], false)[0];
                    pb.ImageLocation = GetImgSource(p.rank);
                    pbp.ImageLocation = p.profilepic;
                    pb.SizeMode = PictureBoxSizeMode.StretchImage;
                    pbp.SizeMode = PictureBoxSizeMode.StretchImage;
                    gT1.Controls.Find("elot1p" + c.Name[c.Name.Length - 1], false)[0] = (Control)pb;
                    gT1.Controls.Find("proft1p" + c.Name[c.Name.Length - 1], false)[0] = (Control)pbp;
                }
            }

            foreach (Control c in gT2.Controls)
            {
                if (c is Label)
                {
                    p = (Player)t2[(Int32)Char.GetNumericValue(c.Name, c.Name.Length - 1)];
                    c.Text = p.name + "\n" + p.rank;
                    PictureBox pb = (PictureBox)gT2.Controls.Find("elot2p" + c.Name[c.Name.Length - 1], false)[0];
                    PictureBox pbp = (PictureBox)gT2.Controls.Find("proft2p" + c.Name[c.Name.Length - 1], false)[0];
                    pb.ImageLocation = GetImgSource(p.rank);
                    pbp.ImageLocation = p.profilepic;
                    pb.SizeMode = PictureBoxSizeMode.StretchImage;
                    pbp.SizeMode = PictureBoxSizeMode.StretchImage;
                    gT2.Controls.Find("elot2p" + c.Name[c.Name.Length - 1], false)[0] = (Control)pb;
                    gT2.Controls.Find("proft2p" + c.Name[c.Name.Length - 1], false)[0] = (Control)pbp;
                }
            }
            FlushTeams();
        }
        public void FlushTeams()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                Player p = (Player)Players[i];
                p.team = 0;
                Players[i] = p;
            }

            t1.Clear();
            t2.Clear();

        }
        public string GetImgSource(string rank)
        {
            rank = rank.ToLower();
            if (rank.Contains("master") || rank.Contains("challenger") || rank.Contains("unranked"))
            {
                if (rank.Contains("grandmaster"))
                    return "http://opgg-static.akamaized.net/images/medals/grandmaster_1.png";
                else if (rank.Contains("master"))
                    return "http://opgg-static.akamaized.net/images/medals/master_1.png";
                else if (rank.Contains("challenger"))
                    return "http://opgg-static.akamaized.net/images/medals/challenger_1.png";
                else if (rank.Contains("unranked"))
                    return "http://opgg-static.akamaized.net/images/medals/default.png";
            }
            else return "http://opgg-static.akamaized.net/images/medals/" + rank.Replace(" ", "_") + ".png";
            return "";
        }
        public void DeclareServers()
        {
            srvBox.DisplayMember = "Text";
            srvBox.ValueMember = "Value";

            var items = new[] {
    new { Text = "RU", Value = "ru" },
    new { Text = "NA", Value = "na" },
    new { Text = "EUW", Value = "euw" },
    new { Text = "EUNE", Value = "eune" },
    new { Text = "BR", Value = "br" },
    new { Text = "TR", Value = "tr" },
    new { Text = "LAS", Value = "las" },
    new { Text = "OCE", Value = "oce" },
    new { Text = "LAN", Value = "lan" },
    new { Text = "JP", Value = "jp" },
    new { Text = "KR", Value = "www" }
};

            srvBox.DataSource = items;
            srvBox.SelectedIndex = 0;
        }

        //COMMON

        public void refreshList()
        {
            playersList.DataSource = null;
            playersList.DataSource = Players;
            playersList.DisplayMember = "name";
            playersList.ValueMember = "mmr";
        }

        //EVENTS

        private void bAdd_Click(object sender, EventArgs e)
        {
            Player p = GetStats(nameSearch.Text);
            if (p != null)
            {
                if (Players.Count > 0)
                {
                    for (int i = 0; i < Players.Count; i++)
                    {
                        Player test = (Player)Players[i];
                        if (p.name == test.name)
                        {
                            MessageBox.Show("Player is already in pool", "ERROR"); return;
                        }
                    }
                }
                Players.Add(p);
                refreshList();
                playersList.SelectedIndex = Players.Count - 1;
            }

            if (playersList.Items.Count == 10)
            {
                bBalance.Enabled = true;
                bBalance.BackColor = Color.YellowGreen;
                nameSearch.Enabled = false;
                bAdd.Enabled = false;
                bBalance.Focus();
            }
            nameSearch.SelectAll();
            nameSearch.Focus();
        }
        private void bRemove_Click(object sender, EventArgs e)
        {
            if (Players.Count > 0)
            {
                Players.RemoveAt(playersList.SelectedIndex);
                refreshList();
            }
            if (playersList.Items.Count < 10)
            {
                bBalance.Enabled = false;
                bBalance.BackColor = SystemColors.Control;
                nameSearch.Enabled = true;
                bAdd.Enabled = true;
                nameSearch.Focus();
            }
            if (playersList.SelectedIndex < Players.Count - 1)
            {
                playersList.SelectedIndex = Players.Count - 1;
            }
        }
        private void bBalance_Click(object sender, EventArgs e)
        {
            idornpic.Visible = false;
            idorntext.Visible = false;
            TeamBalancer();
            gT1.Visible = true;
            gT2.Visible = true;
        }
        private void playersList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (playersList.Items.Count > 0 && playersList.SelectedIndex >= 0)
            {

                bOPGG.Enabled = true;
                bRemove.Enabled = true;
                Player p = (Player)Players[playersList.SelectedIndex];
                label1.Text = p.name + "\n" + p.rank;
            }
            else
            {
                bOPGG.Enabled = false;
                bRemove.Enabled = false;
                label1.Text = "";
            }
        }
        private void srvBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Players.Count > 0)
            {
                Players.Clear();
                nameSearch.Text = "";
                refreshList();
            }
            if (playersList.Items.Count < 10)
            {
                bBalance.Enabled = false;
                bBalance.BackColor = Color.Gainsboro;
                nameSearch.Enabled = true;
                bAdd.Enabled = true;
                nameSearch.Focus();
            }
        }
        private void bOPGG_Click(object sender, EventArgs e)
        {
            Player p = (Player)Players[playersList.SelectedIndex];
            System.Diagnostics.Process.Start(p.link);
        }

        //OVERRIDE
        public class ComboboxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Delete) && playersList.Focused)
            {
                bRemove.PerformClick();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void bScreen_Click(object sender, EventArgs e)
        {
            using (var bmp = new Bitmap(gT1.Width + gT2.Width, gT1.Height))
            {
                String t1 = gT1.Text, t2 = gT2.Text;
                gT1.Text = "";
                gT2.Text = "";
                gT1.DrawToBitmap(bmp, new Rectangle(0,0,gT1.Size.Width, gT1.Size.Height));
                gT2.DrawToBitmap(bmp, new Rectangle(gT1.Size.Width, 0, gT2.Size.Width, gT2.Size.Height));
                bmp.Save(@"SCR.png");
                gT1.Text = t1;
                gT2.Text = t2;
            }
        }

    }
    public class Player
    {
        private string _link;
        private string _profilepic;
        private string _name;
        private string _rank;
        private int _mmr;
        private int _team = 0;

        public Player(string strname, string strrank, int immr, string strlink, string profilepic)
        {
            this._link = strlink;
            this._profilepic = profilepic;
            this._name = strname;
            this._rank = strrank;
            this._mmr = immr;
        }
        public string name
        {
            get
            {
                return _name;
            }
        }

        public string rank
        {

            get
            {
                return _rank;
            }
        }
        public string link
        {

            get
            {
                return _link;
            }
            set
            {
                _link = value;
            }
        }
        public string profilepic
        {

            get
            {
                return _profilepic;
            }
            set
            {
                _profilepic = value;
            }
        }
        public int mmr
        {
            get
            {
                return _mmr;
            }
        }
        public int team
        {
            get
            {
                return _team;
            }
            set
            {
                _team = value;
            }
        }
    }
}
