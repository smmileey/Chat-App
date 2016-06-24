<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChatWindow.aspx.cs" Inherits="ChatV1.ChatWindow" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Prywatna rozmowa</title>
    <link type="text/css" rel="stylesheet" href="Style.css" />
    <script src="//code.jquery.com/jquery-1.12.0.min.js"></script>
    <script src="//code.jquery.com/jquery-migrate-1.2.1.min.js"></script>

    <script>
        var yPosMsg, yPosUsr, isToUpdate = true, diff = -1;

        window.onload = function () {
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.add_pageLoaded(pageLoadedHandler);
            prm.add_pageLoading(pageLoadingHandler);
        };

        function pageLoadingHandler(sender, args) {
            var divMessages = document.getElementById("divMessagesPrivate");
            yPosMsg = divMessages.scrollTop;
            diff = divMessages.scrollHeight - yPosMsg - divMessages.offsetHeight;
        };

        function pageLoadedHandler(sender, args) {
            var divMessages = document.getElementById("divMessagesPrivate");
            if (diff < 1) {
                divMessages.scrollTop = divMessages.scrollHeight;
                isToUpdate = true;
            }
            else if (isToUpdate) {
                divMessages.scrollTop = yPosMsg;
            }
            else {
                isToUpdate = true;
            }
        };

        function FocusThisWindow(result, context) {
            //wynik otrzymanyt z przebiegu działania metody po stronie serwera związanej z focusem na 
            //konretnym TextBox'ie
        };

        function FocusMe() {
            //funkcja wywoływana po stronie serwera
            FocusThisWindowCallBack("FocusThisWindow");
        };

        function ReportClosureConversation(result, context) {
        };

        function ReportClosure() {
            ReportClosureConversationCallback("Closure");
        };
    </script>
</head>
<body style="margin:0 auto;" onunload="ReportClosure()">
    <form id="form1" runat="server">
    <div>
        <asp:Label Id="lblFromUserId" Visible="false" runat="server" />
        <asp:Label Id="lblToUserId" Visible="false" runat="server" />
        <asp:Label Id="lblFromUsername" Visible="false" runat="server" />
        <asp:Label Id="lblMessageSent" Visible="false" runat="server" />
        <asp:Label Id="lblIdentifier" Visible="false" runat="server" />
        <asp:Label Id="lblScrollIdentifier" Visible="false" runat="server" />
        <asp:ScriptManager runat="server" ID="ScriptManagerPrivate"></asp:ScriptManager>
        <section id="privateChat">
            <asp:UpdatePanel Id="UpdatePanel1" runat="server" RenderMode="Inline" UpdateMode="Conditional">
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlId="Timer1" />
                    </Triggers>
                    <ContentTemplate>
                        <asp:Timer Id="Timer1" Interval="1000" runat="server" OnTick="Timer1_Tick" Enabled="false"/>
                        <div id="divMessagesPrivate" style="width:100%;height:100%;">
                            <asp:Literal Id="litMessagesPrivate" runat="server"/>
                        </div>       
                    </ContentTemplate>
            </asp:UpdatePanel> 
            <asp:UpdatePanel runat="server" RenderMode="Inline" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:TextBox Id="txtMessage" runat="server" Width="70%" OnClick="FocusMe()"/>
                    <asp:Button Id="btnSend" runat="server" Text="Wyślij" Width="25%" OnClick="btnSend_Click"/> 
                </ContentTemplate>
            </asp:UpdatePanel>  
        </section>
    </div>
    </form>
</body>
</html>
