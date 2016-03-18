<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ChatV1.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Login runat="server" ID="Login1" OnAuthenticate="Login1_Authenticate" 
            OnLoggedIn="Login1_LoggedIn" OnLoggingIn="Login1_LoggingIn"
            ></asp:Login>
    </div>
    </form>
</body>
</html>
