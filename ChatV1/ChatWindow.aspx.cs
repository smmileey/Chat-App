using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ChatV1
{
    public partial class ChatWindow : System.Web.UI.Page,ICallbackEventHandler
    {
        private string _callbackResult;
        protected void Page_Load(object sender, EventArgs e)
        {       
            if (Session["ChatUserID"] == null || Convert.ToInt32(Session["ChatUserID"]) == 0
                || Session["roomID"] == null || Convert.ToInt32(Session["roomID"]) == 0)
                Response.Redirect("Default.aspx");

            Title = "Prywatna konwersacja z użytkownikiem: "+ lblFromUsername.Text;
            if (!IsPostBack)
            {
                lblFromUsername.Text = Request["Username"];
                Title = "Prywatna konwersacja z użytkownikiem: " + lblFromUsername.Text;
                lblFromUserId.Text = Request["FromUserID"];
                lblToUserId.Text = Request["ToUserID"];
                string reply = Request["isReply"];

                if(reply == "yes")
                {
                    lblMessageSent.Text = "Sent.";
                }
                
                //identyfikator konkretnej rozmowy
                lblIdentifier.Text = lblFromUserId.Text + lblToUserId.Text;
                lblScrollIdentifier.Text = "scrollDown" + lblIdentifier.Text;
                Session[lblIdentifier.Text] = new Queue<string>();
                Session[lblScrollIdentifier.Text] = false;

                //stan otwarcia okna rozmowy                
                string myClosureID = lblFromUserId.Text + lblToUserId.Text + "_closure";
                Application[myClosureID] = false;
                Timer1.Enabled = true;

                //focusowanie okna
                string chatWindowToFocus = lblFromUserId.Text + "_" + lblToUserId.Text;
                Session["DefaultWindow"] = chatWindowToFocus;
                FocusThisWindow();
            }

            //focusowanie okna po kliknięciu w textbox
            //automatyczne focusowanie, gdy klikniemy w textBox danego TextBoxa w danym oknie
            string focusWindowCallBackReference = Page.ClientScript.GetCallbackEventReference(this, "arg", "FocusThisWindow", "");
            string focusThisWindowCallBackScript = "function FocusThisWindowCallBack(arg, context) { " + focusWindowCallBackReference + "; }";
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "FocusThisWindowCallBack", focusThisWindowCallBackScript, true);

            //powiadomienie o zamknięciu okna konwersacji dla osoby po przeciwnej stronie
            string reportClosureCallBackReference = Page.ClientScript.GetCallbackEventReference(this, "arg", "ReportClosureConversation", "");
            string reportClosureCallBackScript = "function ReportClosureConversationCallback(arg, context) { " + reportClosureCallBackReference + "; }";
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "ReportClosureConversationCallback", reportClosureCallBackScript, true);
        }

        //ustawienie focusu na poszczegolne z mozliie wielu otwartych okien (dzieci głównej instancji)
        //dlatego trzeba sprawdzić, które dokładnie należy ustawić na aktywne
        private void FocusThisWindow()
        {
            string chatWindowToFocus = lblFromUserId.Text + "_" + lblToUserId.Text;
            if (Session["DefaultWindow"].ToString() == chatWindowToFocus)
            {
                form1.DefaultButton = "btnSend";
                form1.DefaultFocus = "txtMessage";
            }
        }

        //sprawdzenie, czy nasz rozmowca nie zamknał okna rozmowy
        private void CheckIfClosed()
        {
            string closureID = lblFromUserId.Text + lblToUserId.Text + "_closure";
            if (Application[closureID] != null && (bool)Application[closureID])
            {
                lblMessageSent.Text = string.Empty;
                Application[closureID] = false;
            }
        }

        //inicjalizacja rozmowy (komunikat o inicjalizacji-zapisanie do bazy tylko raz)
        private void InsertPrivateMessage()
        {
            CheckIfClosed();
            //informacje o checi rozpoczecia konwersacji z danym uzytkownikiem zapisujemy w bazie tylko raz
            //to nie jest tresc wiadomosci, tylko komunikat o checi nawiazania rozmowy
            //spowoduje wyswietlenie okna z proba o zaakceptowania w Chatroom.aspx
            if (string.IsNullOrEmpty(lblMessageSent.Text))
            {
                ChatDataContext db = new ChatDataContext();
                PrivateMessage privateMessage = new PrivateMessage();
                var id = (from message in db.PrivateMessages
                          select message.PrivateMessageID).Count();
                privateMessage.PrivateMessageID = id;
                privateMessage.ToUserID = Convert.ToInt32(lblToUserId.Text);
                privateMessage.UserID = Convert.ToInt32(lblFromUserId.Text);

                db.PrivateMessages.InsertOnSubmit(privateMessage);
                db.SubmitChanges();

                lblMessageSent.Text = "Sent.";
            }
        }
        
        //umieszczenie wysyłanej wiadomości w bazie danych (treść)
        private void InsertMessage(string messageText, bool system)
        {
            ChatDataContext db = new ChatDataContext();
            Message msg = new Message();

            var id = (from message in db.Messages
                      select message.MessageID).Count();
            msg.MessageID = id;
            msg.TimeStamp = DateTime.Now;
            msg.ToUserID = Convert.ToInt32(lblToUserId.Text);
            msg.Text = messageText;
            if (system)
                msg.systemMsg = 1;
            else
                msg.systemMsg = 0;
            msg.UserID = Convert.ToInt32(lblFromUserId.Text);

            db.Messages.InsertOnSubmit(msg);
            db.SubmitChanges();
        }

        //wyświetl prywatne wiadomości od bieżącego użytkownika
        private void DisplayPrivateMessages()
        {
            ChatDataContext db = new ChatDataContext();
            var messages = (from msg in db.Messages
                            where ((msg.UserID == Convert.ToInt32(lblFromUserId.Text) && msg.ToUserID == Convert.ToInt32(lblToUserId.Text))
                            || (msg.UserID == Convert.ToInt32(lblToUserId.Text) && msg.ToUserID == Convert.ToInt32(lblFromUserId.Text)))
                            && (msg.TimeStamp >= (DateTime)Session["timeStamp"])
                            orderby msg.TimeStamp descending
                            select msg).Take(200).OrderBy(msg => msg.TimeStamp);

            int id = Convert.ToInt32(Session["ChatUserID"]);
            if (messages != null)
            {
                StringBuilder sb = new StringBuilder();
                int backCounter = -1;

                foreach (var message in messages)
                {
                    if (message.systemMsg !=1)
                    {
                        backCounter = message.UserID;
                        if (backCounter == Convert.ToInt32(Session["ChatUserID"]))
                        {
                            sb.Append("<div style='padding:10px;color:black;'>");
                        }
                        else
                        {
                            sb.Append("<div style='padding:10px;background-color:#EFEFEF;color:gray'>");
                        }

                        var username = (from users in db.Users
                                        where users.UserID == message.UserID
                                        select users.Username).SingleOrDefault();

                        sb.Append("<span style='color:black;font-weight:bold;'>"
                        + username + ": </span>" + message.Text + "</div>");
                    }
                    else
                    {
                        sb.Append("<div style='padding:10px;color:black;background-color:bisque;'>");
                        sb.Append(message.Text + "</div>");
                    }                
                }
                litMessagesPrivate.Text = sb.ToString();
            }
        }

        //przycisk "Wyślij"
        protected void btnSend_Click(object sender, EventArgs e)
        {
            if (txtMessage.Text.Length > 0)
            {
                InsertPrivateMessage();
                ((Queue<string>)Session[lblIdentifier.Text]).Enqueue(txtMessage.Text);
                txtMessage.Text = string.Empty;
            }
            FocusThisWindow();
            Session[lblScrollIdentifier.Text] = true;
        }

        //timer
        protected void Timer1_Tick(object sender, EventArgs e)
        {
            if (Session[lblIdentifier.Text] != null)
            {
                Queue<string> lastPortionOfData = null;
                foreach (var item in (lastPortionOfData = (Queue<string>)Session[lblIdentifier.Text]).ToList())
                {
                    string next = lastPortionOfData.Dequeue();
                    InsertMessage(next,false);
                }
            }
            DisplayPrivateMessages();
            if (Session[lblScrollIdentifier.Text] != null && (bool)Session[lblScrollIdentifier.Text])
            {
                ScrollToBottom();
                Session[lblScrollIdentifier.Text] = false;
            }

            if (Session["DefaultWindow"] != null)
                FocusThisWindow();
        }

        //scrollowanie na sam dół po wysłaniu wiadomości
        private void ScrollToBottom()
        {
            string script = "var divChat=document.getElementById('divMessagesPrivate');divChat.scrollTop=divChat.scrollHeight;isToUpdate=false;";
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "scrollBottom"+lblIdentifier.Text, script, true);
        }

        //callback po kliknięciu przez użytkownika w textbox
        public void RaiseCallbackEvent(string eventArgument)
        {
            _callbackResult = "failed";
            if (!string.IsNullOrEmpty(eventArgument))
            {
                if(eventArgument == "FocusThisWindow")
                {
                    string chatWindowToFocus = lblFromUserId.Text + "_" + lblToUserId.Text;
                    Session["DefaultWindow"] = chatWindowToFocus;
                    FocusThisWindow();
                }
                if (eventArgument == "Closure")
                {
                    if (!string.IsNullOrEmpty(lblMessageSent.Text))
                    {
                        ChatDataContext db = new ChatDataContext();
                        var username = (from user in db.Users
                                        where user.UserID == Convert.ToInt32(Session["ChatUserID"])
                                        select user.Username).SingleOrDefault();
                        InsertMessage(string.Format("**** Użytkownik <span style=\"font-weight:bold;color:red;\">{0}</span> zamknął okno konwersacji ****",
                            username), true);
                        string Closure = lblToUserId.Text + lblFromUserId.Text + "_closure";
                        Application[Closure] = true;
                    }
                }
            }
            _callbackResult = "success";
        }

        public string GetCallbackResult()
        {
            return _callbackResult;
        }
    }
}