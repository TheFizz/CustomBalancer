using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CustomBalancer
{
    public partial class Form1 : Form
    {
        ArrayList Players = new ArrayList();
        ArrayList t1 = new ArrayList();
        ArrayList t2 = new ArrayList();
        WebClient wc = new WebClient();
        string[] Loyals = { "hikky", "thefizz", "omelamustdie", "cptjokesparrow", "dadyoshi", "wellmeal","ионийский зенд" };
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
            string htmlCode = wc.DownloadString(link);
            doc.LoadHtml(htmlCode);
            HtmlNode node = doc.DocumentNode.SelectSingleNode("/html/body/div[1]/div[3]/div/div/div[1]/div[3]/div[1]/span");

            try
            { nickname = node.InnerText; }
            catch
            {
                node = doc.DocumentNode.SelectSingleNode("/html/body/div[1]/div[3]/div/div/div[1]/div[2]/div[1]/span");
                try
                { nickname = node.InnerText; }
                catch { MessageBox.Show("Invalid summoner name, server or not existing on OP.GG", "ERROR!"); return null; };
            }

            node = doc.DocumentNode.SelectSingleNode("//*[@id='SummonerLayoutContent']/div[1]/div[1]/div[1]/div/div[2]/div/span");

            try
            { rank = node.InnerText; }
            catch
            {

                node = doc.DocumentNode.SelectSingleNode("//*[@id='SummonerLayoutContent']/div[1]/div[1]/div[1]/div[1]/div[2]/div[1]/span");
                try
                { rank = node.InnerText; }
                catch { MessageBox.Show("Invalid summoner name, server or not existing on OP.GG", "ERROR!"); return null; };
            }
            mmr = RankToELO(rank);

            return new Player(nickname, rank, mmr, link);
        }
        public int RankToELO(string rank)
        {
            int mmr = 0;
            int mult = 0;
            int step = 70;
            int bmult = 6;
            if (rank.Contains("Bronze"))
            {
                mmr = 900;
                mult = (int)Char.GetNumericValue(rank[rank.Length - 1]);

                return (mmr + (step * (bmult - mult))) - 1;
            }
            else if (rank.Contains("Silver"))
            {
                mmr = 1150;
                mult = (int)Char.GetNumericValue(rank[rank.Length - 1]);
                return (mmr + (step * (bmult - mult))) - 1;
            }
            else if (rank.Contains("Gold"))
            {
                mmr = 1500;
                mult = (int)Char.GetNumericValue(rank[rank.Length - 1]);
                return (mmr + (step * (bmult - mult))) - 1;
            }
            else if (rank.Contains("Platinum"))
            {
                mmr = 1850;
                mult = (int)Char.GetNumericValue(rank[rank.Length - 1]);
                return (mmr + (step * (bmult - mult))) - 1;
            }
            else if (rank.Contains("Diamond"))
            {
                mmr = 2200;
                mult = (int)Char.GetNumericValue(rank[rank.Length - 1]);
                return (mmr + (step * (bmult - mult))) - 1;
            }
            else if (rank.Contains("Master"))
            {
                mmr = 2700;
                return mmr;
            }
            else if (rank.Contains("Challenger"))
            {
                mmr = 3000;
                return mmr;
            }
            else if (rank.Contains("Unranked"))
            {
                mmr = 1360;
                return mmr;
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
            Array.Sort(pool); // сортируем по значениям в обратном порядке 
            int sum = 0;
            foreach (int value in pool)
                sum += value;
            float halfSum = sum / 2f;

            int[] team1 = new int[5]; // исходный массив 
            int[] team2 = new int[5]; // исходный массив 
            int sum1 = 0, sum2 = 0;
            int tcnt1 = 0, tcnt2 = 0;
            for (int i = 9; i >= 0; i--)
            {
                if (sum1 <= sum2 || sum2 >= halfSum)
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
                        Players[j] = new Player(p.name, p.rank, p.mmr, 1,p.link);
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
                        Players[j] = new Player(p.name, p.rank, p.mmr, 2,p.link);
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
                    p = (Player)t1[(Int32)Char.GetNumericValue(c.Name,c.Name.Length-1)];
                    c.Text = p.name + "\n" + p.rank;
                    PictureBox pb = (PictureBox)gT1.Controls.Find("elot1p" + c.Name[c.Name.Length - 1], false)[0];
                    pb.ImageLocation = GetImgSource(p.rank);
                    pb.SizeMode = PictureBoxSizeMode.StretchImage;
                    gT1.Controls.Find("elot1p" + c.Name[c.Name.Length - 1], false)[0] = (Control)pb;
                    if (Loyals.Contains(p.name.ToLower()))
                    {
                        c.ForeColor = Color.OrangeRed;
                        string s = "🌟"+c.Text;
                        s = s.Replace("\n", "🌟\n");
                        c.Text = s;
                    }
                    else
                        c.ForeColor = SystemColors.ControlText;
                }
            }

            foreach (Control c in gT2.Controls)
            {
                if (c is Label)
                {
                    p = (Player)t2[(Int32)Char.GetNumericValue(c.Name, c.Name.Length - 1)];
                    c.Text = p.name + "\n" + p.rank;
                    PictureBox pb = (PictureBox)gT2.Controls.Find("elot2p" + c.Name[c.Name.Length - 1], false)[0];
                    pb.ImageLocation = GetImgSource(p.rank);
                    pb.SizeMode = PictureBoxSizeMode.StretchImage;
                    gT2.Controls.Find("elot2p" + c.Name[c.Name.Length - 1], false)[0] = (Control)pb;
                    if (Loyals.Contains(p.name.ToLower()))
                    {
                        c.ForeColor = Color.OrangeRed;
                        string s = "🌟" + c.Text;
                        s = s.Replace("\n", "🌟\n");
                        c.Text = s;
                    }
                    else
                        c.ForeColor = SystemColors.ControlText;
                }
            }
            FlushTeams();
        }
        public void FlushTeams()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                Player p = (Player)Players[i];
                Players[i] = new Player(p.name, p.rank, p.mmr, 0,p.link);
            }

            t1.Clear();
            t2.Clear();

        }
        public string GetImgSource(string rank)
        {
            rank = rank.ToLower();
            if (rank.Contains("ger") || rank.Contains("ter") || rank.Contains("unra"))
            {
                if (rank.Contains("ger"))
                    return "http://opgg-static.akamaized.net/images/medals/" + rank.Replace("ger", "ger_1") + ".png";
                if (rank.Contains("ter"))
                    return "http://opgg-static.akamaized.net/images/medals/" + rank.Replace("ter", "ter_1") + ".png";
                if (rank.Contains("unra"))
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
            }
            if(playersList.SelectedIndex<Players.Count-1)
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

        
    }
    public class Player
    {
        private string _link;
        private string _name;
        private string _rank;
        private int _mmr;
        private int _team = 0;

        public Player(string strname, string strrank, int immr, string strlink)
        {
            this._link = strlink;
            this._name = strname;
            this._rank = strrank;
            this._mmr = immr;
        }
        public Player(string strname, string strrank, int immr, int iteam,string strlink)
        {

            this._link = strlink;
            this._name = strname;
            this._rank = strrank;
            this._mmr = immr;
            this._team = iteam;
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
        }
    }
}
