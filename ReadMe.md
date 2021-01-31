## Programmatically add @mention for Active Directory User Account in on-prem Azure DevOps Server work item comment (Jan, 2021)

I administer an on-premises instance of Azure DevOps Server (ADS) 2019 1.1 (Patch 7) running on a closed network. The ADS instance is running in a Windows Active Directory (AD) domain. All ADS users are granted access based on their AD user account. Each AD user account specifies their intranet email address.

I have a requirement to send a notification to the "Assigned To" person\'s AD email address for specific user stories in a specific project on the first Monday of each month.
The hard part is getting the @mention to resolve to the AD user account so that ADS sends the notification.

I decided to implement this requirement such that ADS sends the notification based on an @mention added programmatically like this:
-   On the ADS Application server, create a scheduled task that runs on the first of each month
    
-   The scheduled task runs a program (C# + ADS REST api console app installed on the app server) that locates the relevant user stories and programmatically adds an @mention to a new comment for the user story's "Assigned To" user account. The program runs under a domain admin account that is also a "full control" ADS instance admin account.

The code is a "minimum reproducible example" in the spirit of Stack Overflow
