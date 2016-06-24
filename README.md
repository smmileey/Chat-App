# Web Forms Chat Application

This project is my first time with ASP.NET Web Forms technology. 

I've tried to ensure some of the basic features needed for a chat to work. Those are: ability to log into the main room, 
perform talk with the rest of users and private chat with specific ones (after double-clicking their nicknames).

Here, I've used LINQ TO SQL to communicate with SQL database (as it was the proccess of learning that technology too)
that I've prepared for the purposes of this application. If you would like to recreate it on your own machine, then in `Page_Load` method 
of the `Default.aspx` add the following line (and execute once): 

`var dataContext = new ChatDataContext(); dataContext.createDatabase();`

Makse sure that you have proper configuration upon the connection string in your `Web.config`.

Release notes:
- Unfortunately, I haven't added the possibility of registering by now. I'm considering solving this by managing users by Using Membership 
when I get some free time. <b>To be able to test the functionality of the application, you should add some users to the `Users` datbase
by yourself from withing the SQL Server or the application by itself.</b>
