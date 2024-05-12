# Description
The suggestion site application is a .NET 6 Blazor Server web application that allows users to put through suggestion requests for a given department within a company. 
It allows users to create an account for the website, make suggestions, see their approval status and monitor how many other users voted their suggestion up. Users are either basic users or Admins.

The suggestion site application uses MongoDB for the Database via the MongoDB Driver NutGet package for .NET, and Azure AD B2C for authentication and authorization. 
Admins are given the "Admin" claim which will allow these users to access resources that basic users cannot. 

However, Azure AD can also be used instead of AD B2C, as the setup is somewhat similar requiring only a few tweaks if this was to be used within a proper organisation. 
The login capabilities would also be changed so that users can login with their company email account instead of creating a login on the website.

# Overview
The website has both public pages and some that are hidden behind a login, while some only able to be accessed by admin users. The pages are as follows:

1. Home Page: This will allow all users to see other people's suggestion.

![Index](https://github.com/Franco-Diaz-Licham/SuggestionsApp/assets/138960498/263f4ebd-a894-4c11-b97d-a3c2035e7da4)

2. Admin Approval Page: where only admin users have access to. Admins will be able to make small edits and approve suggestions.

![Approval](https://github.com/Franco-Diaz-Licham/SuggestionsApp/assets/138960498/9dc0e905-d83e-467e-b46e-93a6ee41aba1)

3. Suggestion Details Page: where a any user can see the full details of the suggestion.

![Details](https://github.com/Franco-Diaz-Licham/SuggestionsApp/assets/138960498/c9a9e915-671b-4cc4-9849-f15411e5f236)

4. Create a Suggestion Page: where a user can submit a suggestion. 

![Suggest](https://github.com/Franco-Diaz-Licham/SuggestionsApp/assets/138960498/428b191b-677e-455b-b5df-aa499748abc8)

5. Profile Page: users can find all their submitted (whether approved or rejected) suggestions and also be able to edit their profile.

![Profile](https://github.com/Franco-Diaz-Licham/SuggestionsApp/assets/138960498/e2192433-5785-484c-8561-04b0ce85449a)

6. Login Page: users can create an account and login to the website to access resources whidden behind a login/

![Login](https://github.com/Franco-Diaz-Licham/SuggestionsApp/assets/138960498/9f4b7504-93b7-44f7-9369-40754599bf79)

Future improvements include:
1.	Introducing other departments.
2.	User interface for suggetion categories.

This is a practice project based on Tim Corey's course on C# Application Development.
