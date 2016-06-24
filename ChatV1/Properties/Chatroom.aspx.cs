using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ChatV1
{
    public partial class Chatroom : System.Web.UI.Page, ICallbackEventHandler
    {
        private string _callBackStatus;

        protected void Page_Load(object sender, EventArgs e)
        {
            Session["roomID"] = Request["roomID"];

            if (Session["ChatUserID"] == null || Convert.ToInt32(Session["ChatUserID"])==0
                || Session["roomID"] == null || Convert.ToInt32(Session["roomID"])==0)
                    Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
                Session["timeStamp"] = DateTime.Now;
                Session["roomID"] = Request["roomID"];
                Session["lastMessage"] = new Queue<string>();

                PrepareListOfColors();
                GetRoomInformation();
                GetLoggedUsers();
                GetMessages();
                updateTimer.Enabled = true;

                //focusowanie okna
                FocusThisWindow();
            }
            Session["searchForPrivate"] = true;

            //automatyczne wylogowanie, gdy użytkownik zamknie przeglądarkę
            string callBackReference = Page.ClientScript.GetCallbackEventReference(this, "arg", "LogOutUser", "");
            string logoutUserCallBackScript = "function LogOutUserCallBack(arg,context){" + callBackReference + ";}";
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "LogOutUserCallBack",
                logoutUserCallBackScript, true);

            //automatyczne focusowanie, gdy klikniemy w textBox danego TextBoxa w danym oknie
            string focusWindowCallBackReference = Page.ClientScript.GetCallbackEventReference(this, "arg", "FocusThisWindow", "");
            string focusThisWindowCallBackScript = "function FocusThisWindowCallBack(arg, context) { " + focusWindowCallBackReference + "; }";
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "FocusThisWindowCallBack", focusThisWindowCallBackScript, true);
        }

        //ustawienie okna na daną Chatroom
        //dla tego okna nie trzeba nic sprawdzac, kazda otwarta przegladarka to jedna instancja
        //poszczegolne z nich nie dziela ze sobą sesji
        private void FocusThisWindow()
        {
            form1.DefaultButton = "btnSend";
            form1.DefaultFocus = "tBox";
            Session["DefaultWindow"] = "MainWindow";
        }

        //sprawdzenie zaproszen do prywatnych konwersacji
        private void CheckForPrivateMessages()
        {
            //sprawdzam, czy mozna, być może już jest zaproszenie, które jeszcze nie doczekało się odpowiedzi
            if ((bool)Session["searchForPrivate"])
            {
                Session["searchForPrivate"] = false;
                ChatDataContext db = new ChatDataContext();

                var inv = (from msg in db.PrivateMessages
                           where msg.ToUserID == Convert.ToInt32(Session["ChatUserID"])
                           orderby msg.PrivateMessageID
                           select msg).SingleOrDefault();

                if(inv != null)
                {
                    var username = (from user in db.Users
                                    where user.UserID == inv.UserID
                                    select user.Username).SingleOrDefault();

                    lblChatNowUser.Text = username;
                    btnChatNow.OnClientClick =
                       "window.open('ChatWindow.aspx?FromUserId=" + Session["ChatUserID"] +
                       "&ToUserId=" + inv.UserID + "&Username=" + username +"&IsReply=yes','','width=400,"
                       +"height=200'); isLostFocus = 'true';";

                    panelPrivateChat.Visible = true;

                    db.PrivateMessages.DeleteOnSubmit(inv);
                    db.SubmitChanges();
                }
            }
        }

        //pobierz zalogowanych użytkowników
        private void GetLoggedUsers()
        {
            ChatDataContext db = new ChatDataContext();
            //sprawdź, czy bieżący użytkownik jest na liście zalogowanych w tym pokoju
            var usr = (from user in db.LoggedInUsers
                        where user.UserID == Convert.ToInt32(Session["ChatUserID"])
                        && user.RoomID == Convert.ToInt32(Session["roomID"])
                        select user).SingleOrDefault();

            //jeśli nie, to go dodaj
            if (usr == null)
            {
                LoggedInUser loggedUser = new LoggedInUser();
                int id = (from user in db.LoggedInUsers
                          orderby user.LoggedInUserId descending
                          select user.LoggedInUserId
                          ).FirstOrDefault();
                loggedUser.LoggedInUserId = id+1;
                loggedUser.UserID = Convert.ToInt32(Session["ChatUserID"]);
                loggedUser.RoomID = Convert.ToInt32(Session["roomID"]);
                db.LoggedInUsers.InsertOnSubmit(loggedUser);
                db.SubmitChanges();
            }

            StringBuilder sb = new StringBuilder();

            //wyświetl zalogowanych użytkowników w tym pokoju
            var loggedUsers = from users in db.LoggedInUsers
                              where users.RoomID == Convert.ToInt32(Session["roomID"])
                              select users;

            foreach(var loggedOne in loggedUsers)
            {
                //ikonka
                var thisOne = (from users in db.Users
                           where users.UserID == loggedOne.UserID
                           select new { users.Sex,users.Username }).SingleOrDefault();

                if (thisOne.Sex.ToLower() == "m")
                    sb.Append("<img src='man-icon.gif' style='vertical-align:middle;'/>");
                else
                    sb.Append("<img src='female-icon.gif' style='vertical-align:middle;'/>");

                //do wszystkich uzytkownikow oprocz biezacego beda linki (do prywatnej rozmowy)
                if (thisOne.Username != (string)Session["ChatUsername"])
                    sb.Append("<a href=# onclick=\"window.open('ChatWindow.aspx?FromUserID="+ Session["ChatUserID"]
                    + "&ToUserID=" + loggedOne.UserID + "&Username=" + thisOne.Username+ "&isReply=no"+"',"
                    +"'','width=400,height=200,scrollbars=no,toolbars=no,titlebar=no,menubar=no'); isLostFocus='true';\">"
                    +thisOne.Username+"</a><br>");
                else
                    sb.Append("<b>"+thisOne.Username+"</b><br>");
            }

            litUsers.Text = sb.ToString();
        }

        //pobierz wiadomości z bazy (od momentu zalogowania, max 1000)
        private void GetMessages()
        {
            ChatDataContext db = new ChatDataContext();
            var messages = (from message in db.Messages
                            where (message.RoomID == Convert.ToInt32(Session["roomID"]))
                            //&& (message.TimeStamp >= (DateTime)Session["timeStamp"])
                            orderby message.TimeStamp descending
                            select message).Take(500).OrderBy(m=>m.TimeStamp);

            if(messages != null)
            {
                StringBuilder sb = new StringBuilder();
                int backCounter = 0;

                foreach(var message in messages)
                {
                    string color = message.Color;
                    switch (backCounter)
                    {
                        case 0:
                            sb.Append("<div style='padding:10px;color:" + color + "'>");
                            backCounter = 1;
                            break;
                        case 1:
                            sb.Append("<div style='padding:10px;background-color:#EFEFEF;color:"+color+"'>");
                            backCounter = 0;
                            break;
                    }
                    var sexAndUsername = (from users in db.Users
                               where users.UserID == message.UserID
                               select new { users.Sex,users.Username }).SingleOrDefault();
                    
                    if (sexAndUsername.Sex.ToLower() == "m")
                        sb.Append("<img src='man-icon.gif' style='vertical-align:middle;'/>"
                            + "<span style='color:black;font-weight:bold;'>" 
                            + sexAndUsername.Username+": </span>"+ message.Text+"</div>");
                    else
                        sb.Append("<img src='female-icon.gif' style='vertical-align:middle;'/>"
                            + "<span style='color:black;font-weight:bold;'>" 
                            + sexAndUsername.Username + ": </span>" + message.Text + "</div>");
                }
                litMessages.Text= sb.ToString();
            }
        }

        //info o pokoju
        private void GetRoomInformation()
        {
            ChatDataContext db = new ChatDataContext();

            var roomName = (from rooms in db.Rooms
                           where rooms.RoomID == Convert.ToInt32(Session["roomID"])
                           select rooms.Name).SingleOrDefault();

            lblWelcome.Text ="Czat rodzinny:)" + "<br>Witaj w pokoju: " + roomName + 
                "<br>Zalogowano jako: " + Session["ChatUserName"]; 
        }

        //wstawienie wiadomości do bazy danych
        private void InsertMessageToDatabase(string text)
        {
            ChatDataContext db = new ChatDataContext();
            var messageID = (from messages in db.Messages
                            select messages.MessageID).Count();
            Message message = new Message();
            int roomID = Convert.ToInt32(Session["roomID"]);
            message.RoomID = roomID == 0 ? 1 : roomID;
            message.Text = text;
            message.TimeStamp = DateTime.Now;
            message.UserID = Convert.ToInt32(Session["ChatUserID"]);
            message.ToUserID = null; //bedzie sluzyc do prywatych wiadomosci
            message.Color = ddlColors.SelectedValue;
            message.MessageID = messageID;
            db.Messages.InsertOnSubmit(message);
            db.SubmitChanges();
        }

        //lista kolorów możliwych do użycia
        private void PrepareListOfColors()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("ColorsNames", typeof(string)));
            dt.Columns.Add(new DataColumn("ColorsValues", typeof(Color)));

            foreach (var col in Enum.GetNames(typeof(KnownColor)))
            {
                dt.Rows.Add(CreateRow(col, dt));
            }
            DataView dv = new DataView(dt);
            ddlColors.DataSource = dv;
            ddlColors.DataTextField = "ColorsNames";
            ddlColors.DataValueField = "ColorsNames";
            ddlColors.DataBind();
            ddlColors.Attributes.Add("Size", "3");
            ddlColors.SelectedIndex = 11;
        }

       //helper: creating row for DataTable to filling DropDownList of colors
        private DataRow CreateRow(string name, DataTable dt)
        {
            DataRow row = dt.NewRow();
            row[0] = name;
            row[1] = Color.FromName(name);
            return row;
        }

        private void ConfigureScrollBars()
        {
            string script = "var divChat = document.getElementById('one');divChat.scrollTop = divChat.scrollHeight;";
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "scrollScript", script, true);
        }

        //wysłanie wiadomości
        protected void btnSend_Click(object sender, EventArgs e)
        {
            if (tBox.Text.Length > 0)
            {
                ((Queue<string>)Session["lastMessage"]).Enqueue(tBox.Text);
                tBox.Text = string.Empty;
            }
            FocusThisWindow();
            Session["scrollDown"] = true;
        }

        //przesuń scroll na "sam dół"
        private void ScrollToBottom()
        {
            string script = "var divChat=document.getElementById('one');divChat.scrollTop=divChat.scrollHeight;isToUpdate=false;";
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "scrollBottom", script, true);
        }

        //aktualizacja zalogowanych użytkowników oraz wiadomości
        protected void updateTimer_Tick(object sender, EventArgs e)
        {
            if (Session["lastMessage"] != null)
            {
                Queue<string> lastPortionOfData = null;
                foreach (var item in (lastPortionOfData = (Queue<string>)Session["lastMessage"]).ToList())
                {
                    string next = lastPortionOfData.Dequeue();
                    InsertMessageToDatabase(next);
                }
            }
            GetMessages();
            GetRoomInformation();
            GetLoggedUsers();
            if (Session["scrollDown"] != null && (bool)Session["scrollDown"])
            {
                ScrollToBottom();
                Session["scrollDown"] = false;
            }
            CheckForPrivateMessages();

            if (Session["DefaultWindow"] != null)
            {
                if (Session["DefaultWindow"].ToString() == "MainWindow")
                    FocusThisWindow();
            }
        }

        //to zostanie wywołane przez klienta przy zamknięciu przeglądarki przez użytkownika
        public void RaiseCallbackEvent(string eventArgument)
        {
            _callBackStatus = "failed";

            if (!string.IsNullOrEmpty(eventArgument))
            {
                if (eventArgument == "LogOut")
                {
                    ChatDataContext db = new ChatDataContext();
                    var loggedUser = (from user in db.LoggedInUsers
                                      where user.RoomID == Convert.ToInt32(Session["roomID"])
                                      && user.UserID == Convert.ToInt32(Session["ChatUserID"])
                                      select user).SingleOrDefault();

                    db.LoggedInUsers.DeleteOnSubmit(loggedUser);
                    db.SubmitChanges();

                    //usuń "ticket" forms authentication z przeglądarki i wyczyść info o autentykacji bieżącego użytkownika
                    FormsAuthentication.SignOut();
                    HttpContext.Current.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
                    updateTimer.Enabled = false;
                }
                else if(eventArgument == "FocusThisWindow")
                {
                    FocusThisWindow();
                }
            }
                _callBackStatus = "success";
        }

        //to zwracamy jako wynik działania po stronie serwera do metody LogOutUser po stronie klienta
        public string GetCallbackResult()
        {
            return _callBackStatus; 
        }

        protected void tLogout_Click(object sender, EventArgs e)
        {
            ChatDataContext db = new ChatDataContext();
            var loggedUser = (from user in db.LoggedInUsers
                              where user.RoomID == Convert.ToInt32(Session["roomID"])
                              && user.UserID == Convert.ToInt32(Session["ChatUserID"])
                              select user).SingleOrDefault();

            db.LoggedInUsers.DeleteOnSubmit(loggedUser);
            db.SubmitChanges();

            Session.RemoveAll();
            Session.Abandon();
            
            //usuń "ticket" forms authentication z przeglądarki i wyczyść info o autentykacji bieżącego użytkownika
            FormsAuthentication.SignOut();
            HttpContext.Current.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
            updateTimer.Enabled = false;

            Response.Redirect("Default.aspx");
        }

        //obsługa zdarzeń przycisków na panelu akceptacji/odrzucenia propozycji prywatnej wiadomości
        protected void btnChatNow_Click(object sender, EventArgs e)
        {
            panelPrivateChat.Visible = false;
            Session["searchForPrivate"] = true;
        }

        protected void btnChatCancel_Click(object sender, EventArgs e)
        {
            panelPrivateChat.Visible = false;
            Session["searchForPrivate"] = true;
        }
    }
}