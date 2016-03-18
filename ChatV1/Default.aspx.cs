using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ChatV1
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                HttpCookie cookie = Request.Cookies.Get("isRemembered");
                FormsAuthentication.SetAuthCookie("authenticate", false);

                if (cookie != null)
                {
                    Login1.UserName = Request.Cookies.Get("Username").Value;
                    Login1.RememberMeSet = true;
                }
                else
                {
                    HttpCookie cookie1 = new HttpCookie("isRemembered", "1");
                    HttpCookie usernamecookie = new HttpCookie("Username", Login1.UserName);
                    usernamecookie.Expires = DateTime.Now.AddHours(24);
                    cookie1.Expires = DateTime.Now.AddHours(24);

                    Response.Cookies.Add(cookie1);
                    Response.Cookies.Add(usernamecookie);
                }

                //przekierowanie juz zalogowanego uzytkownika bezposrednio do pokoju rozmow
                if (User.Identity.IsAuthenticated)
                {
                    if(Session["ChatUserID"]!=null)
                        Response.Redirect("Chatroom.aspx?roomID=1");
                    else
                    {
                        FormsAuthentication.SignOut();
                        HttpContext.Current.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
                    }
                }
            }
        }

        protected void Login1_Authenticate(object sender, AuthenticateEventArgs e)
        {
            //data context dla LINQ to SQL
            ChatDataContext db = new ChatDataContext();

            var user = (from users in db.Users
                        where users.Username == Login1.UserName
                        && users.Password == Login1.Password
                        select users).SingleOrDefault();

            if(user != null)
            {
                e.Authenticated = true;
                Session["ChatUserID"] = user.UserID;
                Session["ChatUsername"] = user.Username;
            }
            else
            {
                e.Authenticated = false;
            }
        }

        //zapamietanie wyboru dotyczacego pamietania nazwy uzytkownika
        protected void Login1_LoggingIn(object sender, LoginCancelEventArgs e)
        {
            if (Login1.RememberMeSet)
            {
                Response.Cookies["Username"].Value = Login1.UserName;
                Response.Cookies["isRemembered"].Value = "1";
            }
            else
            {
                Response.Cookies.Remove("isRemembered");
                Response.Cookies["isRemembered"].Expires = DateTime.Now.AddDays(-20);
                Response.Cookies.Remove("Username");
                Response.Cookies["Username"].Expires = DateTime.Now.AddDays(-20);
            }
        }

        //pomyslne zalogowanie
        protected void Login1_LoggedIn(object sender, EventArgs e)
        {
            //przekierowujemy tymczasowo od razu do pokoju nr 1
            Response.Redirect("Chatroom.aspx?roomID=1");
        }
    }
}