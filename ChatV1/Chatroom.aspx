<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Chatroom.aspx.cs" Inherits="ChatV1.Chatroom" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Chat v0.1</title>
        <script src="//code.jquery.com/jquery-1.12.0.min.js"></script>
        <script src="//code.jquery.com/jquery-migrate-1.2.1.min.js"></script>

    <script type="text/javascript">
        var yPosMsg, yPosUsr,isToUpdate=true,isLostFocus=false,diff=-1;

        window.onload = function () {
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.add_pageLoaded(pageLoadedHandler);
            prm.add_pageLoading(pageLoadingHandler);
        };
        
        function pageLoadingHandler(sender, args) {
            var divMessages = document.getElementById("one");
            var divUsers = document.getElementById("two");
            yPosMsg = divMessages.scrollTop;
            yPosUsr = divUsers.scrollTop;
            diff = divMessages.scrollHeight- yPosMsg - divMessages.offsetHeight;
            console.log(diff);
        };

        function pageLoadedHandler(sender, args) {  
            var divMessages = document.getElementById("one");
            var divUsers = document.getElementById("two");
            if (diff<1) {
                divMessages.scrollTop = divMessages.scrollHeight;
                isToUpdate = true;
            }
            else if (isToUpdate) {
                divMessages.scrollTop = yPosMsg;
                divUsers.scrollTop = yPosUsr;
            }
            else {
                isToUpdate = true;
            }
        };

        function LogOut() {
            //tą funkcję wywołujemy po stronie serwera przy zamknięciu przez użytkonika przeglądarki 
            LogOutUserCallBack();
        };

        function LogOutUser(result, context) {
            //tutaj otrzymamy wynik z przebiegu działania metody po stronie serwera
            //wywołanej przez poniższą metodę LogMeOut()
            //nic z nim nie robimy
        };
    </script>
    <style>
         div#one {
        width: 80%;
        height: 100%;
        background-color:white;
        border-style:solid;
        border-width:5px;
        border-color:brown;
        float: left;
        box-sizing:border-box;
        }
         
        div#two {
        width: 20%;
        height: 100%;
        background-color:white;
        border-style:solid;
        border-width:5px;
        border-color:brown;
        box-sizing:border-box;
        }

        section{
            width: 100%;
            height: 70vh;
            background: aqua;       
        }

        .wrapper:after{
             display:table;
             content:"";
             clear:both;
         }

        #tBox,#btnSend,#tLabel,#ddlColors,#tLogout{
            vertical-align:top;
        }

    </style>
</head>
<body onunload="LogOut()" onclose="LogOut()" style="position:relative;z-index:0;">
    <div id="privateMessage" style="position:absolute;width:70px;height:70px;background-color:red;z-index:10;margin:0;top:50%;left:50%;">
         I'm on the top
    </div> 
    <form id="form1" runat="server" defaultbutton="btnSend" defaultfocus="tBox">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true"></asp:ScriptManager>
        <asp:Timer runat="server" ID="updateTimer" OnTick="updateTimer_Tick" Interval="4000"></asp:Timer>
        <section>
            <asp:UpdatePanel runat="server" UpdateMode="Conditional" RenderMode="Inline">
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="updateTimer"/>
                </Triggers>
                <ContentTemplate>                
                        <div style="background-color:plum;text-align:center;" runat="server">    
                            <asp:Label ID="lblWelcome" runat="server" Text="Czat rodzinny :)"></asp:Label>  
                        </div>
                        <div id="one" style="overflow-y:scroll !important;">
                            <asp:Literal runat="server" ID="litMessages"></asp:Literal>
                        </div>
                        <div id="two" style="overflow-y:scroll";>
                            <asp:Literal runat="server" ID="litUsers"></asp:Literal>
                        </div>                          
                </ContentTemplate>                         
           </asp:UpdatePanel>
            <asp:UpdatePanel runat="server" RenderMode="Inline" UpdateMode="Conditional">
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnSend" />
                </Triggers>
                <ContentTemplate>
                   <asp:TextBox runat="server" ID="tBox" Width="70%"></asp:TextBox>
                   <ajaxToolkit:FilteredTextBoxExtender runat="server" FilterType="Custom" InvalidChars="><" FilterMode="InvalidChars" TargetControlID="tBox"/>
                   <asp:Button runat="server" Text="Wyślij" Width="5%" ID="btnSend" OnClick="btnSend_Click"/>
                   <asp:Label ID="tLabel" runat="server" Width="4%">Kolor:</asp:Label>
                   <asp:DropDownList ID="ddlColors" runat="server"  Width="13%"></asp:DropDownList>
                   <asp:Button ID="tLogout" runat="server" Text="Wyloguj" Width="6%" OnClick="tLogout_Click" />
                </ContentTemplate>
            </asp:UpdatePanel>                 
        </section>
    </form>
</body>
</html>
