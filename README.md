# Getting started
1. Create DB using the file 'SQL/OutOfOfficeSQL.sql'
2. Change parameter 'Data Source' in 'Default Connection' section in appsetting.json in this section
```
"ConnectionStrings": {
    "DefaultConnection": "Data Source=YOURSERVER;Initial Catalog=ABPTT;Trusted_Connection=True;TrustServerCertificate=True"
  }
```
3. Same thing in 'Data/AppDBContext' here
```
if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=YOURSERVER;Initial Catalog=outofoffice;Trusted_Connection=True;TrustServerCertificate=True");
            }
```
4. Run app

# How to test
After running the app you will be redirected to home page of the web-site.
Here you will be able to see links in the header but many options are permitted while you're logged out, use 'Log In' button to enter your data and log in to your account (default user after creating a DB is 'admin' 'admin').
By logging in as an admin you can add new users, projects etc. After that you cat press 'Log In' again to switch an account.
More info and screenshots here: [Out Of Office Test](https://github.com/davidrhymes/OutOfOffice/blob/master/OutOfOffice-TestTask.pdf)
